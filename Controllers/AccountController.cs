using System.Security.Claims;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CrmAdmin.Web.Data;

namespace CrmAdmin.Web.Controllers
{
    [AllowAnonymous]
    public sealed class AccountController : Controller
    {
        private readonly IDbFactory _db;

        public AccountController(IDbFactory db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var conn = _db.AppDb();

            // Simple lookup in MockMaximizerUsers
            var user = await conn.QuerySingleOrDefaultAsync<MockUser>(@"
SELECT LoginName, FullName, Email 
FROM dbo.MockMaximizerUsers
WHERE UPPER(LoginName) = UPPER(@loginName);",
                new { loginName = model.Username });

            if (user is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // For dev: accept any non-empty password or a fixed password
            // e.g. "Passw0rd!" – you can change this later.
            if (string.IsNullOrWhiteSpace(model.Password) || model.Password != "Passw0rd!")
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // Build claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.LoginName),
                new Claim(ClaimTypes.Name, user.LoginName),
                new Claim("FullName", user.FullName ?? user.LoginName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // simple internal record types
        public sealed record MockUser(string LoginName, string FullName, string? Email);

        public sealed class LoginViewModel
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public bool RememberMe { get; set; }
            public string? ReturnUrl { get; set; }
        }
    }
}
