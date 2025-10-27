using System;

namespace FabricLibrary.Backend.Contracts;

/// <summary>
/// Public-facing user information returned for /api/me.
/// </summary>
public record MeResponse(Guid Id, string Email, string? DisplayName);
