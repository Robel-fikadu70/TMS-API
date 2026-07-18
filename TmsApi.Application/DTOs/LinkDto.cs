namespace TmsApi.Application.DTOs;

// Href: The actual URL
// Rel: The "Relationship" (e.g., "self", "delete", "enroll")
// Method: The HTTP verb (GET, POST, etc.)
public record LinkDto(string? Href, string Rel, string Method);
