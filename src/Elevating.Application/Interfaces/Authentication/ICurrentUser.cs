namespace Elevating.Application.Interfaces.Authentication;

public interface ICurrentUser
{
    Guid? UserId { get; }

    bool IsAuthenticated { get; }
}