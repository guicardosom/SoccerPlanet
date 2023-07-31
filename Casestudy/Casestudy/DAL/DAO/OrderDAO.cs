using Casestudy.DAL.DomainClasses;
using Casestudy.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Casestudy.DAL.DAO
{
    public class OrderDAO
    {
        private readonly AppDbContext _db;
        public OrderDAO(AppDbContext ctx)
        {
            _db = ctx;
        }

        public async Task<List<Order>> GetAll(int id)
        {
            return await _db.Orders!.Where(order => order.CustomerId == id).ToListAsync<Order>();
        }

        public async Task<int> AddOrder(int customerId, OrderSelectionHelper[] selections)
        {
            int orderId = -1;
            // we need a transaction as multiple entities involved
            using (var _trans = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    Order order = new();
                    order.CustomerId = customerId;
                    order.OrderDate = System.DateTime.Now;
                    order.OrderAmount = 0;

                    // calculate the total and then add the order row to the table
                    foreach (OrderSelectionHelper selection in selections)
                    {
                        order.OrderAmount += selection.Product!.MSRP * selection.Qty;
                    }

                    await _db.Orders!.AddAsync(order);
                    await _db.SaveChangesAsync();

                    // then add each item to the OrderLineItems table
                    foreach (OrderSelectionHelper selection in selections)
                    {
                        OrderLineItem oItem = new();
                        oItem.ProductId = selection.Product!.Id;
                        oItem.OrderId = order.Id;
                        oItem.SellingPrice = selection.Product!.MSRP;

                        if (selection.Qty <= selection.Product!.QtyOnHand)
                        {
                            oItem.QtySold = selection.Qty;
                            oItem.QtyOrdered = selection.Qty;
                            oItem.QtyBackOrdered = 0;

                            selection.Product!.QtyOnHand -= selection.Qty;
                        }
                        else
                        {
                            oItem.QtySold = selection.Product!.QtyOnHand;
                            oItem.QtyOrdered = selection.Qty;
                            oItem.QtyBackOrdered = selection.Qty - selection.Product!.QtyOnHand;

                            Product? prod = await _db.Products!.FindAsync(selection.Product.Id);
                            prod!.QtyOnBackOrder = selection.Qty - selection.Product!.QtyOnHand;
                            prod!.QtyOnHand = 0;

                            await _db.SaveChangesAsync();
                        }


                        await _db.OrderLineItems!.AddAsync(oItem);
                        await _db.SaveChangesAsync();
                    }

                    await _trans.CommitAsync();
                    orderId = order.Id;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await _trans.RollbackAsync();
                }
            }
            return orderId;
        }

        public async Task<List<OrderDetailsHelper>> GetOrderDetails(int oid, string email)
        {
            Customer? customer = _db.Customers!.FirstOrDefault(customer => customer.Email == email);
            List<OrderDetailsHelper> allDetails = new();

            // LINQ way of doing INNER JOINS
            var results = from o in _db.Orders
                          join oi in _db.OrderLineItems! on o.Id equals oi.OrderId
                          join p in _db.Products! on oi.ProductId equals p.Id
                          where (o.CustomerId == customer!.Id && o.Id == oid)
                          select new OrderDetailsHelper
                          {
                              OrderId = o.Id,
                              CustomerId = customer!.Id,
                              ProductId = oi.ProductId!,
                              ProductName = p.ProductName,
                              Cost = oi.SellingPrice,
                              QtySold = oi.QtySold,
                              QtyOrdered = oi.QtyOrdered,
                              QtyBackOrdered = oi.QtyBackOrdered,
                              OrderDate = o.OrderDate.ToString() //date will be formatted in client-side
                          };

            allDetails = await results.ToListAsync();
            return allDetails;
        }
    }
}
