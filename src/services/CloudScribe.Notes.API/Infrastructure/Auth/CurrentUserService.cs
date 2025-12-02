using System.Security.Claims;

namespace CloudScribe.Notes.API.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private Guid? _cachedId;
    
    public Guid Id
    {
        get
        {
            if (_cachedId.HasValue)
            {
                return _cachedId.Value;
            }

            var user = httpContextAccessor.HttpContext?.User;
            
            var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? user?.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(idClaim) || !Guid.TryParse(idClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User context is missing.");
            }

            _cachedId = userId;
            return userId;
        }
    }
}