using Dapper;
using Microsoft.AspNetCore.Mvc;
using CrmAdmin.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace CrmAdmin.Web.Controllers;

[Authorize]
public sealed class OnboardingController(IDbFactory db) : Controller
{
    public sealed record Row(Guid Id, string PersonId, string FullName, string Stage, string? Owner, DateTime? DueDate, int ProgressPct);

    public async Task<IActionResult> Index()
    {
        using var conn = db.AppDb();
        var rows = await conn.QueryAsync<Row>(@"
SELECT o.Id, o.PersonId, u.FullName, o.Stage, o.Owner, o.DueDate, o.ProgressPct
FROM dbo.Onboarding o
LEFT JOIN dbo.MockMaximizerUsers u ON u.LoginName = o.PersonId
ORDER BY o.Stage, o.DueDate ASC, o.CreatedUtc DESC;");
        return View(rows);
    }
}
