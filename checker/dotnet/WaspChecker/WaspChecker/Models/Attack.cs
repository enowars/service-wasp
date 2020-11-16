using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WaspChecker.Models
{
    public record Attack(
        long Id,
        string Password,
        string Location,
        string AttackDate,
        string Description)
    {

    }
}
