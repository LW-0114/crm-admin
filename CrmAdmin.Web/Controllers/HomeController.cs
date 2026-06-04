using Dapper;
using Microsoft.AspNetCore.Mvc;
using CrmAdmin.Web.Data;

namespace CrmAdmin.Web.Controllers;

public sealed class HomeController(IDbFactory db) : Controller
{
    public async Task<IActionResult> Index()
    {
        using var conn = db.AppDb();
        var counts = await conn.QuerySingleAsync(@"
SELECT 
  (SELECT COUNT(*) FROM dbo.MockMaximizerUsers) AS UsersCount,
  (SELECT COUNT(*) FROM dbo.Video) AS VideosCount,
  (SELECT COUNT(*) FROM dbo.Onboarding) AS OnboardingCount,
  (SELECT COUNT(*) FROM dbo.TrainingSession) AS SessionsCount,
  (SELECT COUNT(*) FROM dbo.Activity) AS ActivityCount;
");
        return View(counts);
    }
}
