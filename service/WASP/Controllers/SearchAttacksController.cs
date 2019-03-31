using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WASP.Storage;

namespace WASP.Controllers
{
    [Route("api/[controller]")]
    public class SearchAttacksController : Controller
    {
        [HttpGet]
        public IActionResult Get(string needle)
        {
            var matches = WaspDbContext.GetMatchingAttacks(needle);
            // clear all confidential data
            matches.ForEach(m =>
            {
                m.AttackDate = null;
                m.Password = null;
            });
            return Json(new { matches });
        }
    }
}
