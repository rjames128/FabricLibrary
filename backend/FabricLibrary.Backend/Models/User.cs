namespace FabricLibrary.Backend.Models;

/// <summary>
/// Represents a user in the Fabric Library application.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Google Subject identifier (sub claim from Google ID token).
    /// This is the primary way we identify users who sign in with Google.
    /// </summary>
    public string GoogleSub { get; set; } = null!;

    /// <summary>
    /// User's email address from their Google account.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// User's display name from their Google account.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// When the user account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time the user signed in or accessed the application.
    /// </summary>
    public DateTime? LastSeenAt { get; set; }
}
