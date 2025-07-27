using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace ProjectView.Filter
{
    public class RequireJwtAttribute : ActionFilterAttribute
    {
        private readonly string _requiredRole;

        public RequireJwtAttribute(string requiredRole = "")
        {
            _requiredRole = requiredRole;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var tokenString = session.GetString("JWT");

            if (string.IsNullOrEmpty(tokenString))
            {
                context.Result = new RedirectToActionResult("Login", "Authen", null);
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(tokenString);

                // Kiểm tra hạn token
                var exp = token.Payload.Expiration;
                if (exp.HasValue)
                {
                    var expiryDate = DateTimeOffset.FromUnixTimeSeconds(exp.Value).UtcDateTime;
                    if (expiryDate < DateTime.UtcNow)
                    {
                        context.Result = new RedirectToActionResult("Login", "Authen", null);
                        return;
                    }
                }

                // Kiểm tra vai trò (claim "role" hoặc "roles")
                if (!string.IsNullOrEmpty(_requiredRole))
                {
                    var roleClaims = token.Claims.Where(c => c.Type == "role" || c.Type == "roles").Select(c => c.Value);
                    if (!roleClaims.Contains(_requiredRole))
                    {
                        context.Result = new RedirectToActionResult("Forbidden", "Authen", null);
                        return;
                    }
                }
            }
            catch
            {
                context.Result = new RedirectToActionResult("Login", "Authen", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
