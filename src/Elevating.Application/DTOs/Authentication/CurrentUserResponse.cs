namespace Elevating.Application.DTOs.Authentication;

public sealed record CurrentUserResponse(Guid UserId, string Email);