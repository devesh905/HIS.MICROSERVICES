using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAuthService.Models.Entities.Portal;

[Table("LoginActivityLog")]
public class LoginActivityLog
{
    [Key]
    public long Id { get; set; }

    [Required, MaxLength(20)]
    public string UserType { get; set; } = default!;   // "Patient" | "Employee"

    [MaxLength(50)]
    public string? UhidNo { get; set; }

    [MaxLength(100)]
    public string? UserName { get; set; }

    [MaxLength(15)]
    public string? MobileNo { get; set; }

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [MaxLength(10)]
    public string? HospitalKey { get; set; }

    [MaxLength(100)]
    public string? HospitalName { get; set; }

    [Required, MaxLength(30)]
    public string LoginMethod { get; set; } = "OTP";

    public bool IsSuccess { get; set; } = true;

    [MaxLength(300)]
    public string? FailureReason { get; set; }

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [MaxLength(500)]
    public string? UserAgent { get; set; }

    public DateTime LoginAt { get; set; } = DateTime.UtcNow;

    public DateTime? FirstLoginAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int LoginCount { get; set; }
}