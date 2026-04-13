using Api.Domain.Common;

namespace Api.Domain.Entities;

public sealed class ConfirmationCode : BaseEntity
{
    public required string Code { get; set; }
    public required DateTime Expires { get; set; }

    /*** Relations ***/
    public Guid UserId { get; init; }
}