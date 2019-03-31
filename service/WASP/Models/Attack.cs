using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WASP.Models
{
    public class Attack
    {
        public long Id { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }
        public string AttackDate { get; set; }
        public long Contentrowid { get; set; }
        public AttackDescriptionContent Content { get; set; }
    }
}
