using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BillingService.Models.Entities.ViphaHms;

[Table("Sponsor_Master")]
public class SponsorMaster
{
    [Key]
    public int Id { get; set; }
    public string? Sponsor_type { get; set; }
    public string? Sponsor_Name { get; set; }
    public bool? Default_Status { get; set; }
    public int? OrgId { get; set; }
    public short? Reg_Charge { get; set; }
    public string? Ipd_report { get; set; }
    public string? Opd_report { get; set; }
    public string? Opd_Cover_Report { get; set; }
    public string? Ipd_Cover_Report { get; set; }
    public string? Medical_Report { get; set; }
    public short? Opd_Dis_perc { get; set; }
    public short? OpdProc_Dis_perc { get; set; }
    public short? Ipd_Dis_perc { get; set; }
    public short? Boarding_Dis_perc { get; set; }
    public short? OpdLab_Dis_perc { get; set; }
    public short? IpdLab_Dis_perc { get; set; }
    public short? OpdRad_Dis_perc { get; set; }
    public short? IpdRad_Dis_perc { get; set; }
    public short? OpdMed_Dis_perc { get; set; }
    public short? IpdMed_Dis_perc { get; set; }
    public int? Auth_Id { get; set; }
    public int? Dis_Id { get; set; }
    public string? PayMode { get; set; }
    public short? Payee_Id { get; set; }
    public int? Tag_SponsorId { get; set; }
    public string? Opd_Doc_series { get; set; }
    public string? Ipd_Doc_series { get; set; }
    public int? Indent_company_Id { get; set; }
    public string? Refferal_Required { get; set; }
    public bool? IsActive { get; set; }
    public string? Show_Advance { get; set; }
    public string? SystemName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? LastModifiedby { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public int? PriceListId { get; set; }
    public bool? IsReqClaimId { get; set; }
    public string? MedicineSale { get; set; }
    public int? PayeeAccountId { get; set; }
    public bool? ReqCardCatInReg { get; set; }
    public bool? NABHPrice { get; set; }
    public decimal? PurchasePlusPer { get; set; }
    public decimal? UnderPackagePer { get; set; }
    public decimal? PharmacyCreditLimit { get; set; }
}