using Dapper;
using Microsoft.AspNetCore.Mvc;
using CrmAdmin.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace CrmAdmin.Web.Controllers;

[Authorize]
public sealed class SessionsController(IDbFactory db) : Controller
{
    public sealed record SessionRow(Guid Id, string Title, DateTime StartUtc, DateTime EndUtc, string? Trainer, string? Location, int Attendees);
    public async Task<IActionResult> Index()
    {
        using var conn = db.AppDb();
        var rows = await conn.QueryAsync<SessionRow>(@"
SELECT s.Id, s.Title, s.StartUtc, s.EndUtc, s.Trainer, s.Location,
       (SELECT COUNT(*) FROM dbo.SessionAttendee a WHERE a.SessionId = s.Id) AS Attendees
FROM dbo.TrainingSession s
ORDER BY s.StartUtc;");
        return View(rows);
    }
}
