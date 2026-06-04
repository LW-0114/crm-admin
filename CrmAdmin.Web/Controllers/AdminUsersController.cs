using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CrmAdmin.Web.Data;

namespace CrmAdmin.Web.Controllers
{
    [Authorize] // keep admin behind login
    public sealed class AdminUsersController : Controller
    {
        private readonly IDbFactory _db;

        public AdminUsersController(IDbFactory db)
        {
            _db = db;
        }

        public sealed record UserRow(
            string LoginName,
            string FullName,
            string? Email,
            bool IsActive,
            DateTime? UpdatedUtc
        );

        // GET /AdminUsers
        public async Task<IActionResult> Index()
        {
            using var conn = _db.AppDb();

            var sql = @"
SELECT
    u.LoginName,
    u.FullName,
    u.Email,
    CAST(ISNULL(s.IsActive, 1) AS bit) AS IsActive,
    s.UpdatedUtc
FROM dbo.MockMaximizerUsers u
LEFT JOIN dbo.UserStatus s
    ON s.LoginName = u.LoginName
ORDER BY u.LoginName;
";
            var rows = await conn.QueryAsync<UserRow>(sql);

            return View(rows);
        }

        // POST /AdminUsers/Toggle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(string loginName)
        {
            if (string.IsNullOrWhiteSpace(loginName))
            {
                TempData["msg"] = "No user selected.";
                return RedirectToAction(nameof(Index));
            }

            using var conn = _db.AppDb();

            // If status row exists -> flip it
            // If not exists -> create as inactive (flip from default active)
            var sql = @"
IF EXISTS (SELECT 1 FROM dbo.UserStatus WHERE LoginName = @loginName)
BEGIN
    UPDATE dbo.UserStatus
    SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END,
        UpdatedUtc = SYSUTCDATETIME()
    WHERE LoginName = @loginName;
END
ELSE
BEGIN
    INSERT INTO dbo.UserStatus (LoginName, IsActive, UpdatedUtc)
    VALUES (@loginName, 0, SYSUTCDATETIME());
END
";
            await conn.ExecuteAsync(sql, new { loginName });

            TempData["msg"] = $"Updated status for {loginName}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
