using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPDRegistrationService.Models.Entities.Hms;

[Table("PatientRegistration")]
public class PatientRegistration
{
    [Key]
    public long Id { get; set; }      // bigint
    public string? AppointmentNo { get; set; }      // nvarchar
    public string? AppointmentTime { get; set; }      // nvarchar
    public string? UhidNo { get; set; }      // nvarchar
    public string? OpNo { get; set; }      // nvarchar
    public DateTime? RegistrationDate { get; set; }      // datetime
    public string? RegistrationTime { get; set; }      // nvarchar
    public string? Title { get; set; }      // nvarchar
    public string? PatientName { get; set; }      // nvarchar
    public string? Sl_PatientName { get; set; }      // nvarchar
    public string? PatientFirstName { get; set; }      // nvarchar
    public string? PatientLastName { get; set; }      // nvarchar
    public string? RelTitle { get; set; }      // nvarchar
    public string? RelativeName { get; set; }      // nvarchar
    public string? Sl_RelativeName { get; set; }      // nvarchar
    public string? MotherName { get; set; }      // nvarchar
    public string? Sl_MotherName { get; set; }      // nvarchar
    public string? Gender { get; set; }      // nvarchar
    public DateTime? DateOfBirth { get; set; }      // datetime
    public short? AgeInDays { get; set; }      // smallint
    public short? AgeInMonths { get; set; }      // smallint
    public short? AgeInYears { get; set; }      // smallint
    public string? BloodGroup { get; set; }      // nvarchar
    public short? SponsorId { get; set; }      // smallint
    public short? VerticalId { get; set; }      // smallint
    public int? RefByID { get; set; }      // int
    public int? DoctorId { get; set; }      // int
    public int? DepId { get; set; }      // int
    public int? DocUnitId { get; set; }      // int
    public short? RegLocaionID { get; set; }      // smallint
    public short? CountryId { get; set; }      // smallint
    public int? StateId { get; set; }      // int
    public int? CityId { get; set; }      // int
    public int? RegionId { get; set; }      // int
    public string? Town { get; set; }      // nvarchar
    public string? Address { get; set; }      // nvarchar
    public string? District { get; set; }      // nvarchar
    public string? PinCode { get; set; }      // nvarchar
    public string? EmailId { get; set; }      // nvarchar
    public string? MobileNo { get; set; }      // nvarchar  ← LOGIN KEY
    public string? AltMobileNo { get; set; }      // nvarchar
    public string? IDProofType { get; set; }      // nvarchar
    public string? IDProofNo { get; set; }      // nvarchar
    public string? AdharNo { get; set; }      // nvarchar
    public short? CorCountryId { get; set; }      // smallint
    public int? CorStateId { get; set; }      // int
    public int? CorCityId { get; set; }      // int
    public int? CorRegionId { get; set; }      // int
    public string? CorTown { get; set; }      // nvarchar
    public string? CorAddress { get; set; }      // nvarchar
    public string? CorPinCode { get; set; }      // nvarchar
    public string? EmgContactName { get; set; }      // nvarchar
    public int? EmgConactRel { get; set; }      // int
    public string? EmgContactEmailId { get; set; }      // varchar
    public string? EmgContactAdd { get; set; }      // nvarchar
    public string? EmgContactMobileNo { get; set; }      // nvarchar
    public string? CardNo { get; set; }      // nvarchar
    public string? CardType { get; set; }      // nvarchar
    public string? CardRegNo { get; set; }      // nvarchar
    public DateTime? CardRegDate { get; set; }      // datetime
    public string? EmpID { get; set; }      // nvarchar
    public string? RelWithEsm { get; set; }      // nvarchar
    public string? ESMUhid { get; set; }      // nvarchar
    public int? EsmRank { get; set; }      // int
    public string? ReceiptMode { get; set; }      // nvarchar
    public int? BankCashId { get; set; }      // int
    public string? Cheque_CardNo { get; set; }      // nvarchar
    public decimal? RegAmount { get; set; }      // numeric
    public decimal? RecdAmount { get; set; }      // numeric
    public decimal? BalAmount { get; set; }      // numeric
    public string? Cancel_Reason { get; set; }      // nvarchar
    public string? Remarks { get; set; }      // nvarchar
    public string? SystemName { get; set; }      // nvarchar
    public DateTime? CreatedAt { get; set; }      // datetime
    public int? CreatedBy { get; set; }      // int
    public DateTime? LastModifiedDate { get; set; }      // datetime
    public int? LastModifiedBy { get; set; }      // int
    public string? ModifiedSystemName { get; set; }      // nvarchar
    public DateTime? CancelledDate { get; set; }      // datetime
    public int? CancelledBy { get; set; }      // int
    public string? CancelledSystemName { get; set; }     // nvarchar
    public string? Religion { get; set; }      // nvarchar
    public int? OrgId { get; set; }      // int
    public short? FinYr { get; set; }      // smallint
    public bool? IsBillConsultancy { get; set; }      // bit
    public decimal? ConsultCharge { get; set; }      // decimal
    public decimal? TotalAmt { get; set; }      // decimal
    public decimal? DisAmt { get; set; }      // decimal
    public short? VisitNo { get; set; }      // smallint
    public string? OpdType { get; set; }      // nvarchar
    public string? PEmpId { get; set; }      // nvarchar
    public string? PolicyNo { get; set; }      // nvarchar
    public int? SchemeId { get; set; }      // int
    public string? SchemeCardNo { get; set; }      // nvarchar
    public string? ClaimId { get; set; }      // nvarchar
    public string? ServiceNo { get; set; }      // nvarchar
    public string? Mode { get; set; }      // nvarchar
    public string? Nationality { get; set; }      // nvarchar
}