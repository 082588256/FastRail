using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProjectView.Filter
{
    public class AdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;

            // Lấy JWT từ session
            var token = httpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Authen", null);
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwtToken = handler.ReadJwtToken(token);
                Console.WriteLine(jwtToken.ToString());

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
                if (roleClaim?.Value != "admin")
                {
                    context.Result = new RedirectToActionResult("Forbidden", "Authen", null);
                    return;
                }
            }
            catch
            {
                context.Result = new RedirectToActionResult("Login", "Authen", null);
            }
        }
    }
}
