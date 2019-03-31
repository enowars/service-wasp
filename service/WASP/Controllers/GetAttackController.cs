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
        [HttpGet]
        public IActionResult Get(int id, string password)
        {
            var attack = WaspDbContext.GetAttack(id, password);
            return Json(new { attack });
        }
    }
}
