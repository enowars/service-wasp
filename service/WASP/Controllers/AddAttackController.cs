using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WASP.Storage;

namespace WASP.Controllers
{
    [Route("api/[controller]")]
    public class AddAttackController : Controller
    {
        private readonly IWaspDb Db;

        public AddAttackController(IWaspDb db)
        {
            Db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Post(string date, string location, string description, string password)
        {
            await Db.AddAttack(date, location, description, password);
            return Accepted();
        }
    }
}
