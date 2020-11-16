using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaspChecker.Models;

namespace WaspChecker.Models
{
    public record AttackMatches(List<AttachMatch> Matches);
    public record AttachMatch(
        long Id,
        string? Password,
        string Location,
        string AttackDate,
        long Contentrowid,
        AttachMatchContent Content);
    public record AttachMatchContent(long Rowid, string Content);
}
