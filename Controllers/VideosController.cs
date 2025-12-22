using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrmAdmin.Web.Data;

namespace CrmAdmin.Web.Controllers
{
    [Authorize]
    public sealed class VideosController : Controller
    {
        private readonly IDbFactory _db;

        public VideosController(IDbFactory db)
        {
            _db = db;
        }

        // View model row
        public sealed record VideoRow(
            Guid Id,
            string Title,
            string Url,
            int? DurationSec,
            bool IsRequired,
            int WatchedPct,
            bool SignedOff
        );

        // GET /Videos
        public async Task<IActionResult> Index(string personId = "LWRAITH")
        {
            using var conn = _db.AppDb();

            var sql = @"
SELECT 
    v.Id,
    v.Title,
    v.Url,
    v.DurationSec,
    v.IsRequired,
    ISNULL(p.WatchedPct, 0)      AS WatchedPct,
    ISNULL(p.SignedOff, 0)       AS SignedOff
FROM dbo.Video v
LEFT JOIN dbo.VideoProgress p 
    ON p.VideoId = v.Id 
   AND p.PersonId = @personId
ORDER BY v.Title;
";
            var rows = await conn.QueryAsync<VideoRow>(sql, new { personId });

            ViewBag.PersonId = personId;
            return View(rows);
        }

        // POST /Videos/SignOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOff(Guid id, string personId)
        {
            if (personId is null or { Length: 0 })
                personId = "LWRAITH"; // fallback for now

            using var conn = _db.AppDb();

            // Upsert progress & sign-off
            var upsert = @"
MERGE dbo.VideoProgress AS tgt
USING (SELECT @personId AS PersonId, @id AS VideoId) AS src
ON (tgt.PersonId = src.PersonId AND tgt.VideoId = src.VideoId)
WHEN MATCHED THEN 
    UPDATE SET 
        WatchedPct   = 100,
        SignedOff    = 1,
        SignedOffUtc = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (PersonId, VideoId, WatchedPct, SignedOff, SignedOffUtc)
    VALUES (src.PersonId, src.VideoId, 100, 1, SYSUTCDATETIME());
";

            await conn.ExecuteAsync(upsert, new { id, personId });

            // Add activity entry
            var activity = @"
INSERT INTO dbo.Activity (EntityType, EntityId, Kind, Title, Body, Actor)
VALUES ('Video', @id, 'signoff', 'Video signed off', NULL, @personId);
";
            await conn.ExecuteAsync(activity, new { id, personId });

            TempData["msg"] = "Video signed off successfully.";
            return RedirectToAction(nameof(Index), new { personId });
        }
    }
}
