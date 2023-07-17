using Casestudy.DAL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Casestudy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DataController : ControllerBase
    {
        readonly AppDbContext? _ctx;
        public DataController(AppDbContext context) // injected here
        {
            _ctx = context;
        }

        [HttpGet]
        public async Task<ActionResult<String>> Index()
        {
            DataUtility util = new(_ctx!);
            var json = await GetProductJsonAsync();

            string payload = "";
            try
            {
                payload = (await util.LoadProductInfoFromJsonToDb(json)) ? "tables loaded" : "problem loading tables";
            }
            catch (Exception ex)
            {
                payload = ex.Message;
            }

            return JsonSerializer.Serialize(payload);
        }

        private static async Task<String> GetProductJsonAsync()
        {
            return await System.IO.File.ReadAllTextAsync("../../products.json");
        }
    }
}
