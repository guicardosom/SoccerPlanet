using Casestudy.DAL.DomainClasses;
using Casestudy.Helpers;

namespace Casestudy.DAL.DAO
{
    public class OrderDAO
    {
        private readonly AppDbContext _db;
        public OrderDAO(AppDbContext ctx)
        {
            _db = ctx;
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

                            selection.Product!.QtyOnBackOrder = selection.Qty - selection.Product!.QtyOnHand;
                            selection.Product!.QtyOnHand = 0;
                        }

                        oItem.SellingPrice = selection.Product!.MSRP;

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
    }
}
