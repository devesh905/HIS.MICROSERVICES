using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models.Entities.Hms;

[Table("IPDRegistration", Schema = "dbo")]
public class IpdRegistration
{
    [Key]
    public long Id { get; set; }
    public string? UhidNo { get; set; }
    public string? Adm_No { get; set; }
    public string? Ref_No { get; set; }
    public DateTime? Adm_Date { get; set; }
    public string? Timein { get; set; }
    public string? Adm_Type { get; set; }
    public string? Patient_Type { get; set; }
    public short? OrgId { get; set; }
    public short? FinYr { get; set; }
    public int? DocId { get; set; }
    public string? Ref_Type { get; set; }
    public int? Ref_Id { get; set; }
    public int? Cur_WardId { get; set; }
    public int? Cur_RoomId { get; set; }
    public string? Cur_Bed { get; set; }
    public string? Adm_Mode { get; set; }
    public int? Sponsor_Id { get; set; }
    public bool? Package_Status { get; set; }
    public bool? Medical_Lego_Status { get; set; }
    public bool? Accidental_Status { get; set; }
    public bool? Pharmacy_Hosp_Credit { get; set; }
    public string? Old_Ward { get; set; }
    public string? Old_Room { get; set; }
    public string? Old_Bed { get; set; }
    public string? Surgery_Name { get; set; }
    public string? MLCNO { get; set; }
    public int? Cur_Dept_Id { get; set; }
    public int? Old_Dept_Id { get; set; }
    public int? DocUnitId { get; set; }
    public int? Old_DocUnitId { get; set; }
    public int? Cur_Nur_Id { get; set; }
    public int? Old_Nur_Id { get; set; }
    public int? CUnit { get; set; }
    public int? Ounit { get; set; }
    public string? Cradle { get; set; }
    public string? Pro_Diagnos { get; set; }
    public string? Mother_AdmNo { get; set; }
    public string? Mother_Name { get; set; }
    public string? Remarks { get; set; }
    public int? Pat_Sponsor_Id { get; set; }
    public int? TpaId { get; set; }
    public string? Policy_No { get; set; }
    public string? CHSS_CL_No { get; set; }
    public string? Employe_Name { get; set; }
    public string? Card_No { get; set; }
    public decimal? Basic_Salary { get; set; }
    public decimal? TPA_Apr_Limit { get; set; }
    public string? CCNA_No { get; set; }
    public string? ECHS_RegNo { get; set; }
    public string? ServiceNo { get; set; }
    public int? Echs_Rank { get; set; }
    public string? ESM_Name { get; set; }
    public string? ESM_Rel_Name { get; set; }
    public string? ESM_Ref_No { get; set; }
    public DateTime? Ref_Date { get; set; }
    public string? MStatus { get; set; }
    public DateTime? DOM { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? Mrd_File_Rec_Status { get; set; }
    public string? MLCFIlePath { get; set; }
    public string? MLCFileSize { get; set; }
    public bool? IsMlc { get; set; }
    public int? MlcId { get; set; }
    public int? ODocId { get; set; }
    public int? JResDocId { get; set; }
    public bool? BedAllotStatus { get; set; }
    public string? Relieve_Status { get; set; }
    public string? ClaimId { get; set; }
    public string? CancelSystemName { get; set; }
    public int? CancelById { get; set; }
    public DateTime? CancelAt { get; set; }
    public string? OpNo { get; set; }
    public bool? IsEmergencyAdm { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public short? AgeInDays { get; set; }
    public short? AgeInMonths { get; set; }
    public short? AgeInYears { get; set; }
    public string? IpdType { get; set; }
    public string? modepatient { get; set; }
    public string? DeliveryBy { get; set; }
    public string? KnowTo { get; set; }
    public string? ApprovalType { get; set; }
    public string? AbhaNo { get; set; }
}