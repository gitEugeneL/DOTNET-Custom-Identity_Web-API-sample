using Api.Domain.Common;
using Api.Domain.Enums;

namespace Api.Domain.Entities;

public sealed class User : BaseAuditableEntity
{
    public required string Email { get; init; }
    public byte[] PwdHash { get; set; } = [];
    public byte[] PwdSalt { get; set; } = [];

    public bool EmailConfirmed { get; set; }
    public bool LoginLocked { get; set; }
    public bool ConfirmLocked { get; set; }

    public int LoginFailedCount { get; set; }
    public int ConfirmFailedCount { get; set; }
    public int GenerateCodeCount { get; set; }

    public DateTime? LoginLockExpires { get; set; }
    public DateTime? ConfirmLockExpires { get; set; }

    public required Role Role { get; init; }
}