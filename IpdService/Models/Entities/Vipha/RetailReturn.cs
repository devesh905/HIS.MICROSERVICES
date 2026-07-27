using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IpdService.Models.Entities.Vipha;

[Table("RetailReturn_Mas", Schema = "dbo")]
public class RetailReturnMas
{
    [Key]
    public long Id { get; set; }
    public string? Return_No { get; set; }
    public DateTime? Return_Date { get; set; }
    public string? Retail_No { get; set; }          // original sale invoice
    public short? Return_Location_ID { get; set; }
    public short? Amendment_No { get; set; }
    public bool? GstInclusive { get; set; }
    public decimal? RetailQty { get; set; }
    public decimal? ReturnQty { get; set; }
    public decimal? ReturnAmount { get; set; }
    public decimal? RoundOff { get; set; }
    public decimal? ReturnNetAmount { get; set; }   // ← this is what we deduct
    public int? OrgId { get; set; }
    public short? FinYr { get; set; }
    public int? UserId { get; set; }
    public int? AccountId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedByID { get; set; }
    public string? LastModifiedSystemName { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedByID { get; set; }
    public string? Remarks { get; set; }
    public short? Mla_Status { get; set; }
    public string? T_Status { get; set; }
    public bool? WithoutRetailNo { get; set; }
    public bool? AgainstIPDNo { get; set; }
    public string? IPDNo { get; set; }              // ← filter by Adm_No
    public string? PayMode { get; set; }
    public bool? AccountPosting { get; set; }
    public bool? CreateSummaryInvoice { get; set; }
    public string? SummaryInvoiceNo { get; set; }
}

[Table("RetailReturn_Det", Schema = "dbo")]
public class RetailReturnDet
{
    [Key]
    public long Id { get; set; }
    public string? Return_No { get; set; }
    public DateTime? Return_Date { get; set; }
    public int? OrgId { get; set; }        // DB: int (was short? → WRONG)
    public short? FinYr { get; set; }
    public int? AccountId { get; set; }
    public string? Retail_No { get; set; }
    public short? Sno { get; set; }        // DB: smallint (was int? → wrong direction)
    public int? ProductID { get; set; }
    public decimal? BillQty { get; set; }
    public decimal? ReturnQty { get; set; }
    public int? UOM_ID { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Taxable_Amount { get; set; }
    public decimal? Gst_Per { get; set; }
    public decimal? SGST_Amt { get; set; }
    public decimal? CGST_Amt { get; set; }
    public decimal? IGST_Amt { get; set; }
    public decimal? Item_Net_Amount { get; set; }
    public string? Batch_No { get; set; }
    public string? Mfg_Date { get; set; }
    public string? Exp_Date { get; set; }
    public int? CreatedByID { get; set; }
    public decimal? Disc_Per { get; set; }
    public decimal? Disc_Amount { get; set; }
    public string? IPDNo { get; set; }
    public int? UnderServiceId { get; set; }
    public string? IncludeInBill { get; set; }  // DB: nvarchar (was bool? → WRONG)
}