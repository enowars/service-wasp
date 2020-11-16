namespace WaspChecker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    public record Attack(
        long Id,
        string? Password,
        string? Location,
        string? AttackDate,
        AttackDescriptionContent? Content);

    public record AttackDescriptionContent(
        long Rowid,
        string Content);
}
