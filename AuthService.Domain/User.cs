using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthService.Domain;

[Table("USERS")]
public class User
{
    [Column("ID")]
    public Guid Id { get; set; }

    [Column("USER_ACCT")]
    public required string Email { get; set; }

    [Column("PWD_HASH")]
    public required string PasswordHash { get; set; }    // Base64 32 bytes

    [Column("PWD_SALT")]
    public required string PasswordSalt { get; set; }    // Base64 16 bytes

    [Column("PWD_MAC")]
    public required string PasswordMac { get; set; }     // HMAC  // Base64 32 bytes

    [Column("KEY_VER")]
    public string? KmsKeyVersion { get; set; }  // e.g. "1"

    [Column("CREATE_DT")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
