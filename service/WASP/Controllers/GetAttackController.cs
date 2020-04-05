using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASP.Storage;

namespace WASP.Controllers
{
    [Route("api/[controller]")]
    public class GetAttackController : Controller
    {
        private readonly IWaspDb Db;

        public GetAttackController(IWaspDb db)
        {
            Db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id, string password)
        {
            var attack = await Db.GetAttack(id, password);
            return Json(new { attack });
        }
    }
}
