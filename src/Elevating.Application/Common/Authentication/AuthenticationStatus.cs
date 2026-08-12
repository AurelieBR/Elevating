namespace Elevating.Application.Common.Authentication;

public enum AuthenticationStatus
{
    Succeeded,
    DuplicateEmail,
    InvalidCredentials,
    LockedOut,
    Failed
}