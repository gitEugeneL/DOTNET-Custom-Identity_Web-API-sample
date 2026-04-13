using Api.Domain.Common;

namespace Api.Domain.Entities;

public sealed class RefreshToken : BaseAuditableEntity
{
    public required string Token { get; init; }
    public required DateTime Expires { get; init; }

    /*** Relations ***/
    public required Guid UserId { get; init; }
}