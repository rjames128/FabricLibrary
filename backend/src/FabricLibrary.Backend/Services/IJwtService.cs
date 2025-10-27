using System.Security.Claims;

namespace FabricLibrary.Backend.Services;

public interface IJwtService
{
    /// <summary>
    /// Create an application JWT for the given user.
    /// </summary>
    /// <param name="userId">The user's unique identifier (GUID).</param>
    /// <param name="email">The user's email address.</param>
    /// <returns>A signed JWT string.</returns>
    string CreateToken(Guid userId, string email);
}
