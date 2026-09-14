using System.Security.Claims;

namespace Aura.Api.Common;

public static class ClaimsPrincipalExtensions
{
    public static string UserId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
        principal.FindFirstValue("sub") ??
        throw new ServiceException(StatusCodes.Status401Unauthorized, "Authentication is required.");
}
