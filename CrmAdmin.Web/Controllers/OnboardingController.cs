using Dapper;
using Microsoft.AspNetCore.Mvc;
using CrmAdmin.Web.Data;
using Microsoft.AspNetCore.Authorization;

namespace CrmAdmin.Web.Controllers;

[Authorize]
public sealed class OnboardingController(IDbFactory db) : Controller
{
    public sealed record Row(Guid Id, string PersonId, string FullName, string Stage, string? Owner, DateTime? DueDate, int ProgressPct);

    // ── existing action ──────────────────────────────────────────────────────
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

    // ── NEW: card detail drawer (AJAX or direct GET) ─────────────────────────
    [HttpGet("/api/onboarding/{id:guid}")]
    public async Task<IActionResult> Detail(Guid id)
    {
        using var conn = db.AppDb();

        var card = await conn.QuerySingleOrDefaultAsync<Row>(@"
SELECT o.Id, o.PersonId, u.FullName, o.Stage, o.Owner, o.DueDate, o.ProgressPct
FROM dbo.Onboarding o
LEFT JOIN dbo.MockMaximizerUsers u ON u.LoginName = o.PersonId
WHERE o.Id = @id;", new { id });

        if (card is null) return NotFound();

        var activity = await conn.QueryAsync(@"
SELECT Kind, Title, Body, Actor, WhenUtc
FROM dbo.Activity
WHERE EntityType = 'Onboarding' AND EntityId = @id
ORDER BY WhenUtc DESC;", new { id });

        return Ok(new { card, activity });
    }

    // ── NEW: stage change (drag & drop PATCH) ────────────────────────────────
    [HttpPatch("/api/onboarding/{id:guid}/stage")]
    public async Task<IActionResult> UpdateStage(Guid id, [FromBody] StageUpdate body)
    {
        using var conn = db.AppDb();

        await conn.ExecuteAsync(@"
UPDATE dbo.Onboarding
SET Stage = @Stage, UpdatedUtc = SYSUTCDATETIME()
WHERE Id = @id;", new { body.Stage, id });

        // log to activity timeline
        await conn.ExecuteAsync(@"
INSERT dbo.Activity (EntityType, EntityId, Kind, Title, Actor)
VALUES ('Onboarding', @id, 'stage_changed', @Title, @Actor);",
            new { id, Title = $"Moved to {body.Stage}", Actor = User.Identity?.Name ?? "system" });

        return Ok(new { ok = true, stage = body.Stage });
    }

    public sealed record StageUpdate(string Stage);
}