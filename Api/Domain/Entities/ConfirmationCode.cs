using Api.Domain.Common;

namespace Api.Domain.Entities;

public sealed class ConfirmationCode : BaseEntity
{
    public required string Code { get; init; }
    public required DateTime Expires { get; init; }

    /*** Relations ***/
    public required Guid UserId { get; init; }
}