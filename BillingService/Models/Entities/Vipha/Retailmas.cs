using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models.Entities.Vipha;

[Table("Retail_Mas", Schema = "dbo")]
public class RetailMas
{
    [Key]
    public long Id { get; set; }
    public string? Retail_No { get; set; }
    public DateTime? Retail_Date { get; set; }
    public short? Retail_Location_ID { get; set; }
    public bool? GstInclusive { get; set; }
    public decimal? RetailAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? Discount { get; set; }
    public decimal? RoundOff { get; set; }
    public decimal? RetailNetAmount { get; set; }
    public string? PmtMode { get; set; }
    public decimal? PayAmount { get; set; }
    public decimal? Balance { get; set; }
    public int? OrgId { get; set; }
    public short? FinYr { get; set; }
    public int? UserId { get; set; }
    public int? AccountId { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedByID { get; set; }
    public string? UHIDNo { get; set; }
    public string? IPDNo { get; set; }
    public string? PatientName { get; set; }
    public string? PatientAddress { get; set; }
    public string? Consultant { get; set; }
    public string? Sponsar { get; set; }
    public string? SaleMode { get; set; }
    public decimal? DisPer { get; set; }
    public string? PayMode { get; set; }
    public string? RefNo { get; set; }
    public string? Remarks { get; set; }
    public string? PayMode2 { get; set; }
    public int? AccountId2 { get; set; }
    public decimal? PayAmount1 { get; set; }
    public decimal? PayAmount2 { get; set; }
    public string? MobileNo { get; set; }
    public bool? CreateSummaryInvoice { get; set; }
    public string? SummaryInvoiceNo { get; set; }
    public bool? AccountPosting { get; set; }
    public int? ProductTypeId { get; set; }
    public bool? UnderPackagePatient { get; set; }
    public string? MedicineOnSale { get; set; }
    public bool? PaymentStatus { get; set; }
    public bool? PaymentStatus2 { get; set; }
    public string? PaymentSystemMsg { get; set; }
    public string? PaymentSystemMsg2 { get; set; }
    public bool? IsCashItem { get; set; }
}