using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IpdService.Models.Entities.ViphaHms;

[Table("Doctor_Master")]
public class DoctorMaster
{
    [Key]
    public int Id { get; set; }
    public string? Doctype { get; set; }
    public string? Doc_Name { get; set; }
    public string? Gender { get; set; }
    public string? Alias { get; set; }
    public int? StateId { get; set; }
    public int? CityId { get; set; }
    public string? Town { get; set; }
    public string? Add1 { get; set; }
    public string? Add2 { get; set; }
    public string? RegNo { get; set; }
    public string? LicenseNo { get; set; }
    public int? DocUnitId { get; set; }
    public string? Qual1 { get; set; }
    public string? Qual2 { get; set; }
    public int? DepartmentId { get; set; }
    public int? SpecialityId { get; set; }
    public string? Designation { get; set; }
    public string? Jobexp { get; set; }
    public string? Shift { get; set; }
    public int? Def_LocationID { get; set; }
    public string? Opd_Allowed { get; set; }
    public string? OpdRoomFloor { get; set; }
    public string? OpdRoom { get; set; }
    public int? FeeGroupID { get; set; }
    public short? ReVisitValidity { get; set; }
    public short? FreeReVisit { get; set; }
    public string? OTDoctor { get; set; }
    public short? OTBookingLimit { get; set; }
    public string? Heading1 { get; set; }
    public string? Heading2 { get; set; }
    public string? Heading3 { get; set; }
    public string? Heading4 { get; set; }
    public decimal? Inc_Visit { get; set; }
    public decimal? Inc_Consult { get; set; }
    public decimal? Inc_Proc { get; set; }
    public decimal? Inc_Sref { get; set; }
    public decimal? Inc_vac { get; set; }
    public long? OrgId { get; set; }
    public int? UserId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedBy { get; set; }
    public bool? IsActive { get; set; }
    public int? CountryId { get; set; }
    public string? Sur_Typ { get; set; }
    public string? MobileNo { get; set; }
    public string? Typ { get; set; }
    public decimal? Salary { get; set; }
    public string? EmpCode { get; set; }
    public string? QDeviceId { get; set; }
}