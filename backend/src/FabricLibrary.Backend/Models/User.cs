using System.ComponentModel.DataAnnotations;

namespace FabricLibrary.Backend.Models;

/// <summary>
/// Represents a user who has signed in to the FabricLibrary application.
/// </summary>
public class User
{
    /// <summary>
    /// Primary key (GUID).
    /// </summary>
    [Key]
    public Guid Id { get; set; }

    /// <summary>
    /// The stable Google subject identifier (sub claim) used to uniquely identify the user from Google's identity system.
    /// </summary>
    public string GoogleSub { get; set; } = null!;

    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Optional display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Creation timestamp (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the user last authenticated or was seen.
    /// </summary>
    public DateTime? LastSeenAt { get; set; }
}
