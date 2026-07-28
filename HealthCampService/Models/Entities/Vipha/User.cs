using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.API.Models.Entities.Vipha;

[Table("Users", Schema = "dbo")]
public class User
{
    [Key]
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmpCode { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? PasswordSalt { get; set; }
    public string? PasswordHash { get; set; }
    public bool IsPasswordChanged { get; set; }
    public string? Qualification { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfJoining { get; set; }
    public DateTime? DateOfLeaving { get; set; }
    public string? CommunicationAddress { get; set; }
    public int? CountryId { get; set; }
    public int? StateId { get; set; }
    public string? PermanentAddress { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsActive { get; set; }
    public bool IsSysAdmin { get; set; }
    public string? Designation { get; set; }
    public bool IsSysSubAdmin { get; set; }
    public int? DeptId { get; set; }
    public bool? IsCCUser { get; set; }
    public bool? ResetPassword { get; set; }
    public DateTime? ResetDate { get; set; }
}