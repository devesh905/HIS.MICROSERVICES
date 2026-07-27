using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IpdService.Models.Entities.ViphaHms;

[Table("Receipt_Mas", Schema = "dbo")]
public class ReceiptMas
{
    [Key]
    public long Id { get; set; }
    public string? DocNo { get; set; }              // e.g. "REC-10000-22"
    public DateTime? DocDate { get; set; }
    public string? DocTime { get; set; }
    public string? UhidNo { get; set; }
    public string? Adm_No { get; set; }             // ← join key
    public string? PatientName { get; set; }
    public decimal? Amount { get; set; }            // ← the deposit amount
    public string? DocType { get; set; }            // "IPD"
    public string? DocGroup { get; set; }           // "Indoor Advance"
    public string? RecMode { get; set; }            // "CASH" etc.
    public int? AccountId { get; set; }
    public string? ReceiptNo { get; set; }
    public string? CardNo { get; set; }
    public string? ChequeNo { get; set; }
    public string? Remarks { get; set; }
    public int? OrgId { get; set; }
    public short? FinYr { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedByID { get; set; }
    public string? LastModifiedSystemName { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedByID { get; set; }
    public short? Mla_Status { get; set; }

    /// <summary>"O" = Open/Active, "C" = Cancelled. Use this instead of IsCancel.</summary>
    public string? T_Status { get; set; }

    public short? VoucherTypeId { get; set; }
    public string? BillNo { get; set; }
    public DateTime? Bill_Date { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? Balance { get; set; }
    public string? CancelSystemName { get; set; }
    public int? CancelById { get; set; }
    public DateTime? CancelByAt { get; set; }
    public bool? AccountPosting { get; set; }
}