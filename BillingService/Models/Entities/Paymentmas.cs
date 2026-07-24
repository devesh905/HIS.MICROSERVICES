using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models.Entities.Hms;

[Table("Payment_Mas", Schema = "dbo")]
public class PaymentMas
{
    [Key]
    public long Id { get; set; }
    public string? DocNo { get; set; }              // e.g. "Ref/1/22"
    public DateTime? DocDate { get; set; }
    public string? DocTime { get; set; }
    public string? UhidNo { get; set; }
    public string? Adm_No { get; set; }             // ← join key
    public string? PatientName { get; set; }
    public decimal? Amount { get; set; }            // ← refund amount
    public string? DocType { get; set; }            // "IPD"
    public string? DocGroup { get; set; }           // "Indoor Advance"
    public string? PayMode { get; set; }
    public int? AccountId { get; set; }
    public string? CardNo { get; set; }
    public string? ChequeNo { get; set; }
    public string? Remarks { get; set; }
    public string? ReceiptNo { get; set; }
    public string? CancelNo { get; set; }
    public string? DiscountNo { get; set; }
    public int? OrgId { get; set; }
    public short? FinYr { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedByID { get; set; }
    public string? LastModifiedSystemName { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedByID { get; set; }
    public short? Mla_Status { get; set; }

    /// <summary>"O" = Open/Active, "C" = Cancelled.</summary>
    public string? T_Status { get; set; }

    public short? VoucherTypeId { get; set; }
    public string? CancelSystemName { get; set; }
    public int? CancelById { get; set; }
    public DateTime? CancelByAt { get; set; }
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? Balance { get; set; }
    public bool? AccountPosting { get; set; }
    public string? SendToAccId { get; set; }
}