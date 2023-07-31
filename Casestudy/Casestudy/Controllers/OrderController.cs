using Casestudy.DAL;
using Casestudy.DAL.DAO;
using Casestudy.DAL.DomainClasses;
using Casestudy.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Castle.Components.DictionaryAdapter.Xml;

namespace Casestudy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        readonly AppDbContext? _ctx;

        public OrderController(AppDbContext context)
        {
            _ctx = context;
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<ActionResult<string>> Index(OrderHelper helper)
        {
            string retVal;
            try
            {
                CustomerDAO cDao = new(_ctx!);
                Customer? orderOwner = await cDao.GetByEmail(helper.Email);
                OrderDAO oDao = new(_ctx!);
                int orderId = await oDao.AddOrder(orderOwner!.Id, helper.Selections!);
                retVal = orderId > 0
                ? "Order " + orderId + " created!"
                : "Order not created";
            }
            catch (Exception ex)
            {
                retVal = "Order not created " + ex.Message;
            }
            return retVal;
        }

        [Route("{email}")]
        [HttpGet]
        public async Task<ActionResult<List<Order>>> List(string email)
        {
            List<Order> orders;
            CustomerDAO cDao = new(_ctx!);
            Customer? orderOwner = await cDao.GetByEmail(email);
            OrderDAO oDao = new(_ctx!);
            orders = await oDao.GetAll(orderOwner!.Id);
            return orders;
        }

        [Route("{orderid}/{email}")]
        [HttpGet]
        public async Task<ActionResult<List<OrderDetailsHelper>>> GetOrderDetails(int orderid, string email)
        {
            OrderDAO dao = new(_ctx!);
            return await dao.GetOrderDetails(orderid, email);
        }
    }
}
