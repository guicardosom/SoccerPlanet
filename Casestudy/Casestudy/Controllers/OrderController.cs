using Casestudy.DAL;
using Casestudy.DAL.DAO;
using Casestudy.DAL.DomainClasses;
using Casestudy.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
    }
}
