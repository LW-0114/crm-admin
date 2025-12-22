using Dapper;
using Microsoft.AspNetCore.Mvc;
using CrmAdmin.Web.Data;

namespace CrmAdmin.Web.Controllers;

[ApiController]
[Route("api/seed-preview")]
public sealed class SeedPreviewController(IDbFactory db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        using var conn = db.AppDb();

        var countsSql = @"
SELECT 
  (SELECT COUNT(*) FROM dbo.MockMaximizerUsers) AS UsersCount,
  (SELECT COUNT(*) FROM dbo.Video) AS VideosCount,
  (SELECT COUNT(*) FROM dbo.Onboarding) AS OnboardingCount,
  (SELECT COUNT(*) FROM dbo.VideoProgress) AS VideoProgressCount,
  (SELECT COUNT(*) FROM dbo.TrainingSession) AS SessionsCount,
  (SELECT COUNT(*) FROM dbo.SessionAttendee) AS SessionAttendeesCount,
  (SELECT COUNT(*) FROM dbo.Activity) AS ActivityCount;
";
        var counts = await conn.QuerySingleAsync(countsSql);

        var users = await conn.QueryAsync("SELECT TOP 3 LoginName, FullName, Email FROM dbo.MockMaximizerUsers ORDER BY LoginName");
        var videos = await conn.QueryAsync("SELECT TOP 3 Id, Title, Url FROM dbo.Video ORDER BY Title");
        var onboarding = await conn.QueryAsync(@"
SELECT TOP 5 o.Id, o.PersonId, o.Stage, o.Owner, o.DueDate, o.ProgressPct
FROM dbo.Onboarding o ORDER BY o.CreatedUtc DESC");

        return Ok(new { counts, users, videos, onboarding });
    }
}
