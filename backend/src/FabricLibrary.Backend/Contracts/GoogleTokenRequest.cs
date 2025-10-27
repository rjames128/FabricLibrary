namespace FabricLibrary.Backend.Contracts;

/// <summary>
/// Request contract for exchanging a Google ID token with the backend.
/// </summary>
/// <param name="IdToken">The raw Google ID token (JWT) returned by the Google Sign-In flow.</param>
public record GoogleTokenRequest(string IdToken);
