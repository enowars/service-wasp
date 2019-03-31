using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WASP.Storage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WASP.Controllers
{
    [Route("api/[controller]")]
    public class AddAttackController : Controller
    {
        [HttpPost]
        public IActionResult Post(string date, string location, string description, string password)
        {
            WaspDbContext.AddAttack(date, location, description, password);
            return Accepted();
        }
    }
}
