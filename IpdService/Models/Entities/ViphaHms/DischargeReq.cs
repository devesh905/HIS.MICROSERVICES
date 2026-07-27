using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IpdService.Models.Entities.ViphaHms;

[Table("Discharge_Req")]
public class DischargeReq
{
    [Key]
    public long Id { get; set; }
    public string? DocNo { get; set; }
    public DateTime? DocDate { get; set; }
    public string? DocTime { get; set; }
    public string? UhidNo { get; set; }
    public string? Adm_No { get; set; }
    public string? PatientName { get; set; }
    public string? WardNo { get; set; }
    public string? RoomNo { get; set; }
    public string? BedNo { get; set; }
    public string? Remarks { get; set; }
    public string? Discharge_Status { get; set; }
    public int? OrgId { get; set; }
    public short? FinYr { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int? LastModifiedBy { get; set; }
    public DateTime? Adm_Date { get; set; }
    public int? Discharge_Mode { get; set; }
    public string? P_Condition { get; set; }
    public bool? IsCancel { get; set; }
    public string? CancelRemarks { get; set; }
    public int? CancelById { get; set; }
    public DateTime? CancelAt { get; set; }
    public string? CancelSystemName { get; set; }
}