using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WASP.Models
{
    public class AttackDescriptionContent
    {
        [Key] public long rowid { get; set; }
        public string Content { get; set; }
    }
}
