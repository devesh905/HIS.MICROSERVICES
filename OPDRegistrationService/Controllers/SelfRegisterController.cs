using OPDRegistrationService.Models.DTOs;
using OPDRegistrationService.Models.Entities.Hms;
using OPDRegistrationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;      
using Microsoft.EntityFrameworkCore; 
using System.Data;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SelfRegisterController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;
    private readonly ILogger<SelfRegisterController> _logger;
    private readonly IJwtService _jwt;

    private const short FinYr = 0;  
    private const int SponsorId = 33;
    private const int VerticalId = 9;   
    private const string OpdType = "GENERAL";
    private const string ReceiptMode = "Credit";  

    public SelfRegisterController(
        IHospitalQueryService hospitals,
        ILogger<SelfRegisterController> logger,
         IJwtService jwt)
    {
        _hospitals = hospitals;
        _logger = logger;
        _jwt = jwt;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] SelfRegisterPatientRequest req)
    {
        var entry = HospitalRegistry.Find(req.HospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Hospital not found: {req.HospitalKey}" });

        //var mobileClaim = User.FindFirst("mobile")?.Value;
        //if (string.IsNullOrWhiteSpace(mobileClaim))
        //    return Unauthorized(new { message = "Mobile not found in token." });

        // mobile comes from the request body directly
        var mobileClaim = req.MobileNo?.Trim();
        if (string.IsNullOrWhiteSpace(mobileClaim))
            return BadRequest(new { message = "Mobile number is required." });

        const int createdBy = 0;
        var systemName = $"{Environment.MachineName}#HIS.PATIENT";
        var patientName = $"{req.PatientFirstName} {req.PatientLastName}".Trim();
        var orgId = 2;

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var fee = await ctx.VerticalDepartmentFeeDets
            .Where(v => v.DeptId == req.DepId
                     && v.SponsorId == SponsorId
                     && v.VerticalId == VerticalId
                     && v.ServiceName == OpdType
                     && v.OrgId == orgId)
            .Select(v => v.Fee)
            .FirstOrDefaultAsync() ?? 0m;

        short visitNo = (short)req.VisitNo;
        string uhidNo = req.UhidNo ?? "";

        // If no UHID provided, check if patient already exists by mobile
        if (string.IsNullOrWhiteSpace(uhidNo))
        {
            var existing = await ctx.PatientRegistrations
                .Where(p => p.MobileNo == mobileClaim && p.OrgId == orgId)
                .OrderByDescending(p => p.RegistrationDate)
                .Select(p => new { p.UhidNo, p.VisitNo })
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                uhidNo = existing.UhidNo;
                visitNo = (short)(existing.VisitNo + 1); 
                _logger.LogInformation("Existing patient found UHID={Uhid}, using VisitNo={VisitNo}", uhidNo, visitNo);
            }
        }


        var discountTable = new DataTable();
        discountTable.Columns.Add("Sno", typeof(int));
        discountTable.Columns.Add("DisAuthId", typeof(int));
        discountTable.Columns.Add("DisId", typeof(int));
        discountTable.Columns.Add("DisPer", typeof(decimal));
        discountTable.Columns.Add("DisAmt", typeof(decimal));

        var pId = new SqlParameter("@Id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

        //var pUhidNo = new SqlParameter("@UhidNo", SqlDbType.NVarChar, 16) { Direction = ParameterDirection.Output };

        var pUhidNo = new SqlParameter("@UhidNo", SqlDbType.NVarChar, 16)
        {
            Direction = ParameterDirection.InputOutput,  
            Value = uhidNo                            
        };

        var pOpNo = new SqlParameter("@OpNo", SqlDbType.NVarChar, 18) { Direction = ParameterDirection.Output };
        var pReceiptNo = new SqlParameter("@ReceiptNo", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };

        var pDiscount = new SqlParameter("@PatientRegDiscountDet", SqlDbType.Structured)
        {
            TypeName = "dbo.PatientRegDiscountDetails",
            Value = discountTable
        };

        // pUhidNo.Value = req.UhidNo ?? "";

        var parameters = new[]
        {
    new SqlParameter("@OrgId",                 SqlDbType.Int)      { Value = orgId },
    new SqlParameter("@FinYr",                 SqlDbType.SmallInt) { Value = (short)0 },
    pId,
    new SqlParameter("@AppointmentNo",         SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@AppointmentTime",       SqlDbType.NVarChar) { Value = "" },
    pUhidNo,
    pOpNo,
    new SqlParameter("@RegistrationDate",      SqlDbType.DateTime) { Value = DateTime.Now },
    new SqlParameter("@RegistrationTime",      SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@Title",                 SqlDbType.NVarChar) { Value = req.Title ?? "" },
    new SqlParameter("@PatientName",           SqlDbType.NVarChar) { Value = patientName },
    new SqlParameter("@Sl_PatientName",        SqlDbType.NVarChar) { Value = patientName },
    new SqlParameter("@PatientFirstName",      SqlDbType.NVarChar) { Value = req.PatientFirstName ?? "" },
    new SqlParameter("@PatientLastName",       SqlDbType.NVarChar) { Value = req.PatientLastName  ?? "" },
    new SqlParameter("@RelTitle",              SqlDbType.NVarChar) { Value = req.RelTitle    ?? "" },
    new SqlParameter("@RelativeName",          SqlDbType.NVarChar) { Value = req.RelativeName ?? "" },
    new SqlParameter("@Sl_RelativeName",       SqlDbType.NVarChar) { Value = req.RelativeName ?? "" },
    new SqlParameter("@MotherName",            SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@Sl_MotherName",         SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@Gender",                SqlDbType.NVarChar) { Value = req.Gender ?? "" },
    new SqlParameter("@DateOfBirth",           SqlDbType.DateTime) { Value = (object?)req.DateOfBirth ?? DBNull.Value },
    new SqlParameter("@AgeInDays",             SqlDbType.SmallInt) { Value = (short)req.AgeInDays   },
    new SqlParameter("@AgeInMonths",           SqlDbType.SmallInt) { Value = (short)req.AgeInMonths },
    new SqlParameter("@AgeInYears",            SqlDbType.SmallInt) { Value = (short)req.AgeInYears  },
    new SqlParameter("@BloodGroup",            SqlDbType.NVarChar) { Value = req.BloodGroup ?? "" },
    new SqlParameter("@SponsorId",             SqlDbType.SmallInt) { Value = (short)SponsorId  },
    new SqlParameter("@VerticalId",            SqlDbType.SmallInt) { Value = (short)VerticalId },
    new SqlParameter("@RefByID",               SqlDbType.Int)      { Value = 1 },
    new SqlParameter("@DoctorId",              SqlDbType.Int)      { Value = req.DoctorId },
    new SqlParameter("@DepId",                 SqlDbType.Int)      { Value = req.DepId    },
    new SqlParameter("@DocUnitId",             SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@RegLocaionID",          SqlDbType.SmallInt) { Value = (short)9 },
    new SqlParameter("@CountryId",             SqlDbType.SmallInt) { Value = (short)(req.CountryId > 0 ? req.CountryId : 1) },
    new SqlParameter("@StateId",               SqlDbType.Int)      { Value = req.StateId },
    new SqlParameter("@CityId",                SqlDbType.Int)      { Value = req.CityId  },
    new SqlParameter("@RegionId",              SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@District",              SqlDbType.NVarChar) { Value = req.District    ?? "" },
    new SqlParameter("@Town",                  SqlDbType.NVarChar) { Value = req.Town        ?? "" },
    new SqlParameter("@Address",               SqlDbType.NVarChar) { Value = req.Address     ?? "" },
    new SqlParameter("@PinCode",               SqlDbType.NVarChar) { Value = req.PinCode     ?? "" },
    new SqlParameter("@EmailId",               SqlDbType.NVarChar) { Value = req.EmailId     ?? "" },
    new SqlParameter("@MobileNo",              SqlDbType.NVarChar) { Value = mobileClaim     ?? "" },
    new SqlParameter("@AltMobileNo",           SqlDbType.NVarChar) { Value = req.AltMobileNo ?? "" },
    new SqlParameter("@IDProofType",           SqlDbType.NVarChar) { Value = "Aadhar" },
    new SqlParameter("@IDProofNo",             SqlDbType.NVarChar) { Value = req.AdharNo ?? "" },
    new SqlParameter("@AdharNo",               SqlDbType.NVarChar) { Value = req.AdharNo ?? "" },
    new SqlParameter("@CorCountryId",          SqlDbType.SmallInt) { Value = (short)0 },
    new SqlParameter("@CorStateId",            SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@CorCityId",             SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@CorRegionId",           SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@CorTown",               SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@CorAddress",            SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@CorPinCode",            SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@EmgContactName",        SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@EmgConactRel",          SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@EmgContactEmailId",     SqlDbType.VarChar)  { Value = "" },
    new SqlParameter("@EmgContactAdd",         SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@EmgContactMobileNo",    SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@CardNo",                SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@CardType",              SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@CardRegNo",             SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@CardRegDate",           SqlDbType.DateTime) { Value = DBNull.Value },
    new SqlParameter("@EmpID",                 SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@RelWithEsm",            SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@ESMUhid",               SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@EsmRank",               SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@ReceiptMode",           SqlDbType.NVarChar) { Value = ReceiptMode },
    new SqlParameter("@BankCashId",            SqlDbType.Int)      { Value = 0 }, // bill are on credit
    new SqlParameter("@Cheque_CardNo",         SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@IsBillConsultancy",     SqlDbType.Bit)      { Value = true },
    new SqlParameter("@RegAmount",             SqlDbType.Decimal)  { Value = 0m,  Precision = 18, Scale = 2 },
    new SqlParameter("@RecdAmount",            SqlDbType.Decimal)  { Value = fee, Precision = 18, Scale = 2 },
    new SqlParameter("@ConsultCharge",         SqlDbType.Decimal)  { Value = fee, Precision = 18, Scale = 2 },
    new SqlParameter("@TotalAmt",              SqlDbType.Decimal)  { Value = fee, Precision = 18, Scale = 2 },
    new SqlParameter("@DisAmt",                SqlDbType.Decimal)  { Value = 0m,  Precision = 18, Scale = 2 },
    new SqlParameter("@BalAmount",             SqlDbType.Decimal)  { Value = 0m,  Precision = 18, Scale = 2 },
    new SqlParameter("@Remarks",               SqlDbType.NVarChar) { Value = req.Remarks ?? "" },
    new SqlParameter("@symptoms",              SqlDbType.NVarChar) { Value = "No" },
    new SqlParameter("@Int_Travel",            SqlDbType.NVarChar) { Value = "No" },
    new SqlParameter("@Covid_Rep",             SqlDbType.NVarChar) { Value = "No" },
    new SqlParameter("@Health_Worker",         SqlDbType.NVarChar) { Value = "No" },
    new SqlParameter("@Covid_Contact",         SqlDbType.NVarChar) { Value = "No" },
    new SqlParameter("@Brought_From",          SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@Brought_By",            SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@First_Date",            SqlDbType.DateTime) { Value = DBNull.Value },
    new SqlParameter("@Sar_Screen",            SqlDbType.NVarChar) { Value = "No" },
    new SqlParameter("@AB_Test",               SqlDbType.NVarChar) { Value = "No" },
    new SqlParameter("@SystemName",            SqlDbType.NVarChar) { Value = systemName },
    new SqlParameter("@CreatedBy",             SqlDbType.Int)      { Value = 1993 },
    new SqlParameter("@Religion",              SqlDbType.NVarChar) { Value = "" },
    pDiscount,
    new SqlParameter("@VisitNo", SqlDbType.SmallInt) { Value = visitNo },
    new SqlParameter("@ManageConsultancyInReg",SqlDbType.Bit)      { Value = true },
    new SqlParameter("@OpdType",               SqlDbType.NVarChar) { Value = OpdType },
    new SqlParameter("@PEmpId",                SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@FluTypeId",             SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@PolicyNo",              SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@ClaimId",               SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@ServiceNo",             SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@SchemeId",              SqlDbType.Int)      { Value = 0 },
    new SqlParameter("@SchemeCardNo",          SqlDbType.NVarChar) { Value = "" },
    new SqlParameter("@Nationality",           SqlDbType.NVarChar) { Value = req.Nationality ?? "Indian" },
    new SqlParameter("@ApprovalType",          SqlDbType.NVarChar) { Value = "" },
    pReceiptNo,
    new SqlParameter("@AbhaNo",                SqlDbType.NVarChar) { Value = "" },
    pSuccess,
    pMessage,
};

        try
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "EXEC HMS_PatientRegistration_Create " + BuildParamString(),
                parameters);

            var success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
            var message = pMessage.Value?.ToString() ?? "";

            if (!success)
            {
                _logger.LogWarning("Self-reg SP failed: {Message}", message);
                return BadRequest(new { message });
            }

            return Ok(new
            {
                success = true,
                message,
                id = pId.Value != DBNull.Value ? (long)pId.Value : 0,
                uhidNo = pUhidNo.Value?.ToString() ?? "",
                opNo = pOpNo.Value?.ToString() ?? "",
                receiptNo = pReceiptNo.Value != DBNull.Value ? (long)pReceiptNo.Value : 0,
                fee       
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error during self-registration");
            return StatusCode(500, new { message = ex.Message, type = ex.GetType().Name, inner = ex.InnerException?.Message });
        }
    }

    [HttpPost("register-new")]
    //[AllowAnonymous]   // Guest token or no token — new patient has no account yet
    public async Task<IActionResult> RegisterNew([FromBody] SelfRegisterNewPatientRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.HospitalKey))
            return BadRequest(new { message = "Hospital selection is required." });

        var entry = HospitalRegistry.Find(req.HospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital: {req.HospitalKey}" });

        var mobileClaim = User.FindFirst("mobile")?.Value?.Trim();
        var mobile = !string.IsNullOrWhiteSpace(mobileClaim)
            ? mobileClaim
            : req.MobileNo?.Trim();

        if (string.IsNullOrWhiteSpace(mobile))
            return BadRequest(new { message = "Verified mobile number is required." });

        if (string.IsNullOrWhiteSpace(req.PatientFirstName))
            return BadRequest(new { message = "First name is required." });
        if (string.IsNullOrWhiteSpace(req.PatientLastName))
            return BadRequest(new { message = "Last name is required." });
        if (string.IsNullOrWhiteSpace(req.Gender))
            return BadRequest(new { message = "Gender is required." });
        if (req.DoctorId <= 0)
            return BadRequest(new { message = "Consultant selection is required." });
        if (req.DepId <= 0)
            return BadRequest(new { message = "Department selection is required." });

        // Prevent registering twice if they refresh and resubmit
        await using var checkCtx = _hospitals.CreateHisContext(entry.HisCs);
        var alreadyExists = await checkCtx.PatientRegistrations
            .AnyAsync(p => p.MobileNo == mobile);
        if (alreadyExists)
        {
            _logger.LogWarning("register-new called but mobile {Mobile} already exists in {Hospital}",
                mobile, req.HospitalKey);
            return Conflict(new
            {
                message = "A patient with this mobile number already exists. Please login instead.",
                alreadyRegistered = true
            });
        }

        const int orgId = 2;
        const int sponsorId = 10;
        var verticalId = req.VerticalId > 0 ? req.VerticalId : 1;
        const string opdType = "GENERAL";

        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var fee = await ctx.VerticalDepartmentFeeDets
            .Where(v => v.DeptId == req.DepId
                     && v.SponsorId == sponsorId
                     && v.VerticalId == verticalId
                     && v.ServiceName == opdType
                     && v.OrgId == orgId)
            .Select(v => v.Fee)
            .FirstOrDefaultAsync() ?? 0m;

        var patientName = $"{req.PatientFirstName} {req.PatientLastName}".Trim().ToUpper();
        var systemName = $"{Environment.MachineName}#HIS.SELF-REG";


        const bool paymentImplemented = false;  // flip to true when payment is ready

        if (!paymentImplemented)
        {
            return StatusCode(503, new
            {
                message = "Online self-registration is currently unavailable. Please visit the hospital reception desk to register.",
                code = "PAYMENT_NOT_IMPLEMENTED"
            });
        }

        var discountTable = new DataTable();
        discountTable.Columns.Add("Sno", typeof(int));
        discountTable.Columns.Add("DisAuthId", typeof(int));
        discountTable.Columns.Add("DisId", typeof(int));
        discountTable.Columns.Add("DisPer", typeof(decimal));
        discountTable.Columns.Add("DisAmt", typeof(decimal));

        var pId = new SqlParameter("@Id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        var pUhidNo = new SqlParameter("@UhidNo", SqlDbType.NVarChar, 16) { Direction = ParameterDirection.Output, Value = "" };
        var pOpNo = new SqlParameter("@OpNo", SqlDbType.NVarChar, 18) { Direction = ParameterDirection.Output };
        var pReceiptNo = new SqlParameter("@ReceiptNo", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        var pSuccess = new SqlParameter("@Success", SqlDbType.Bit) { Direction = ParameterDirection.Output };
        var pMessage = new SqlParameter("@Message", SqlDbType.VarChar, -1) { Direction = ParameterDirection.Output };

        var pDiscount = new SqlParameter("@PatientRegDiscountDet", SqlDbType.Structured)
        {
            TypeName = "dbo.PatientRegDiscountDetails",
            Value = discountTable
        };

        var parameters = new[]
        {
        new SqlParameter("@OrgId",                  SqlDbType.Int)      { Value = orgId },
        new SqlParameter("@FinYr",                  SqlDbType.SmallInt) { Value = (short)0 },
        pId,
        new SqlParameter("@AppointmentNo",          SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@AppointmentTime",        SqlDbType.NVarChar) { Value = "" },
        pUhidNo,         
        pOpNo,
        new SqlParameter("@RegistrationDate",       SqlDbType.DateTime) { Value = DateTime.Now },
        new SqlParameter("@RegistrationTime",       SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@Title",                  SqlDbType.NVarChar) { Value = req.Title ?? "" },
        new SqlParameter("@PatientName",            SqlDbType.NVarChar) { Value = patientName },
        new SqlParameter("@Sl_PatientName",         SqlDbType.NVarChar) { Value = patientName },
        new SqlParameter("@PatientFirstName",       SqlDbType.NVarChar) { Value = req.PatientFirstName.ToUpper() },
        new SqlParameter("@PatientLastName",        SqlDbType.NVarChar) { Value = req.PatientLastName.ToUpper()  },
        new SqlParameter("@RelTitle",               SqlDbType.NVarChar) { Value = req.RelTitle     ?? "" },
        new SqlParameter("@RelativeName",           SqlDbType.NVarChar) { Value = (req.RelativeName ?? "").ToUpper() },
        new SqlParameter("@Sl_RelativeName",        SqlDbType.NVarChar) { Value = (req.RelativeName ?? "").ToUpper() },
        new SqlParameter("@MotherName",             SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@Sl_MotherName",          SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@Gender",                 SqlDbType.NVarChar) { Value = req.Gender ?? "" },
        new SqlParameter("@DateOfBirth",            SqlDbType.DateTime) { Value = (object?)req.DateOfBirth ?? DBNull.Value },
        new SqlParameter("@AgeInDays",              SqlDbType.SmallInt) { Value = (short)req.AgeInDays   },
        new SqlParameter("@AgeInMonths",            SqlDbType.SmallInt) { Value = (short)req.AgeInMonths },
        new SqlParameter("@AgeInYears",             SqlDbType.SmallInt) { Value = (short)req.AgeInYears  },
        new SqlParameter("@BloodGroup",             SqlDbType.NVarChar) { Value = req.BloodGroup ?? "" },
        new SqlParameter("@SponsorId",              SqlDbType.SmallInt) { Value = (short)sponsorId  },
        new SqlParameter("@VerticalId",             SqlDbType.SmallInt) { Value = (short)verticalId },
        new SqlParameter("@RefByID",                SqlDbType.Int)      { Value = 1 },
        new SqlParameter("@DoctorId",               SqlDbType.Int)      { Value = req.DoctorId },
        new SqlParameter("@DepId",                  SqlDbType.Int)      { Value = req.DepId    },
        new SqlParameter("@DocUnitId",              SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@RegLocaionID",           SqlDbType.SmallInt) { Value = (short)9 },
        new SqlParameter("@CountryId",              SqlDbType.SmallInt) { Value = (short)(req.CountryId > 0 ? req.CountryId : 1) },
        new SqlParameter("@StateId",                SqlDbType.Int)      { Value = req.StateId },
        new SqlParameter("@CityId",                 SqlDbType.Int)      { Value = req.CityId  },
        new SqlParameter("@RegionId",               SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@District",               SqlDbType.NVarChar) { Value = req.District    ?? "" },
        new SqlParameter("@Town",                   SqlDbType.NVarChar) { Value = req.Town        ?? "" },
        new SqlParameter("@Address",                SqlDbType.NVarChar) { Value = (req.Address ?? "").ToUpper() },
        new SqlParameter("@PinCode",                SqlDbType.NVarChar) { Value = req.PinCode     ?? "" },
        new SqlParameter("@EmailId",                SqlDbType.NVarChar) { Value = req.EmailId     ?? "" },
        new SqlParameter("@MobileNo",               SqlDbType.NVarChar) { Value = mobile },         
        new SqlParameter("@AltMobileNo",            SqlDbType.NVarChar) { Value = req.AltMobileNo ?? "" },
        new SqlParameter("@IDProofType",            SqlDbType.NVarChar) { Value = "Aadhar" },
        new SqlParameter("@IDProofNo",              SqlDbType.NVarChar) { Value = req.AdharNo ?? "" },
        new SqlParameter("@AdharNo",                SqlDbType.NVarChar) { Value = req.AdharNo ?? "" },
        new SqlParameter("@CorCountryId",           SqlDbType.SmallInt) { Value = (short)0 },
        new SqlParameter("@CorStateId",             SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@CorCityId",              SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@CorRegionId",            SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@CorTown",                SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@CorAddress",             SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@CorPinCode",             SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@EmgContactName",         SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@EmgConactRel",           SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@EmgContactEmailId",      SqlDbType.VarChar)  { Value = "" },
        new SqlParameter("@EmgContactAdd",          SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@EmgContactMobileNo",     SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@CardNo",                 SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@CardType",               SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@CardRegNo",              SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@CardRegDate",            SqlDbType.DateTime) { Value = DBNull.Value },
        new SqlParameter("@EmpID",                  SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@RelWithEsm",             SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@ESMUhid",                SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@EsmRank",                SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@ReceiptMode",            SqlDbType.NVarChar) { Value = "ONLINE" },
        new SqlParameter("@BankCashId",             SqlDbType.Int)      { Value = 1 },
        new SqlParameter("@Cheque_CardNo",          SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@IsBillConsultancy",      SqlDbType.Bit)      { Value = true },
        new SqlParameter("@RegAmount",              SqlDbType.Decimal)  { Value = 0m,  Precision = 18, Scale = 2 },
        new SqlParameter("@RecdAmount",             SqlDbType.Decimal)  { Value = fee, Precision = 18, Scale = 2 },
        new SqlParameter("@ConsultCharge",          SqlDbType.Decimal)  { Value = fee, Precision = 18, Scale = 2 },
        new SqlParameter("@TotalAmt",               SqlDbType.Decimal)  { Value = fee, Precision = 18, Scale = 2 },
        new SqlParameter("@DisAmt",                 SqlDbType.Decimal)  { Value = 0m,  Precision = 18, Scale = 2 },
        new SqlParameter("@BalAmount",              SqlDbType.Decimal)  { Value = 0m,  Precision = 18, Scale = 2 },
        new SqlParameter("@Remarks",                SqlDbType.NVarChar) { Value = req.Remarks ?? "" },
        new SqlParameter("@symptoms",               SqlDbType.NVarChar) { Value = "No" },
        new SqlParameter("@Int_Travel",             SqlDbType.NVarChar) { Value = "No" },
        new SqlParameter("@Covid_Rep",              SqlDbType.NVarChar) { Value = "No" },
        new SqlParameter("@Health_Worker",          SqlDbType.NVarChar) { Value = "No" },
        new SqlParameter("@Covid_Contact",          SqlDbType.NVarChar) { Value = "No" },
        new SqlParameter("@Brought_From",           SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@Brought_By",             SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@First_Date",             SqlDbType.DateTime) { Value = DBNull.Value },
        new SqlParameter("@Sar_Screen",             SqlDbType.NVarChar) { Value = "No" },
        new SqlParameter("@AB_Test",                SqlDbType.NVarChar) { Value = "No" },
        new SqlParameter("@SystemName",             SqlDbType.NVarChar) { Value = systemName },
        new SqlParameter("@CreatedBy",              SqlDbType.Int)      { Value = 1993 },
        new SqlParameter("@Religion",               SqlDbType.NVarChar) { Value = "" },
        pDiscount,
        new SqlParameter("@VisitNo",                SqlDbType.SmallInt) { Value = (short)1 },  // always 1 for new patient
        new SqlParameter("@ManageConsultancyInReg", SqlDbType.Bit)      { Value = true },
        new SqlParameter("@OpdType",                SqlDbType.NVarChar) { Value = opdType },
        new SqlParameter("@PEmpId",                 SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@FluTypeId",              SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@PolicyNo",               SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@ClaimId",                SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@ServiceNo",              SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@SchemeId",               SqlDbType.Int)      { Value = 0 },
        new SqlParameter("@SchemeCardNo",           SqlDbType.NVarChar) { Value = "" },
        new SqlParameter("@Nationality",            SqlDbType.NVarChar) { Value = req.Nationality ?? "Indian" },
        new SqlParameter("@ApprovalType",           SqlDbType.NVarChar) { Value = "" },
        pReceiptNo,
        new SqlParameter("@AbhaNo",                 SqlDbType.NVarChar) { Value = "" },
        pSuccess,
        pMessage,
    };

        try
        {
            await ctx.Database.ExecuteSqlRawAsync(
                "EXEC HMS_PatientRegistration_Create " + BuildParamString(),
                parameters);

            var success = pSuccess.Value != DBNull.Value && (bool)pSuccess.Value;
            var message = pMessage.Value?.ToString() ?? "";

            if (!success)
            {
                _logger.LogWarning("register-new SP failed for mobile {Mobile}: {Message}", mobile, message);
                return BadRequest(new { message });
            }

            var newUhid = pUhidNo.Value?.ToString() ?? "";
            var opNo = pOpNo.Value?.ToString() ?? "";

            // Fetch newly created record and issue real JWT 
            await using var authCtx = _hospitals.CreateHisContext(entry.HisCs);
            var newPatient = await authCtx.PatientRegistrations
                .Where(p => p.UhidNo == newUhid && p.MobileNo == mobile)
                .OrderBy(p => p.RegistrationDate)
                .FirstOrDefaultAsync();

            string? patientToken = null;
            PatientDto? patientDto = null;

            if (newPatient != null)
            {
                patientToken = _jwt.GenerateToken(newPatient, entry.Key);
                patientDto = BuildPatientDto(newPatient, entry.Key, entry.Name);
            }
            else
            {
                // SP succeeded but record not immediately readable — edge case
                _logger.LogWarning("register-new SP succeeded but patient record not found for UHID {Uhid}", newUhid);
            }

            _logger.LogInformation(
                "New patient registered: UHID={Uhid} OpNo={OpNo} Hospital={Hospital} Mobile={Mobile}",
                newUhid, opNo, entry.Key, mobile);

            return Ok(new
            {
                success = true,
                isNewPatient = true,
                message,
                id = pId.Value != DBNull.Value ? (long)pId.Value : 0,
                uhidNo = newUhid,
                opNo,
                receiptNo = pReceiptNo.Value != DBNull.Value ? (long)pReceiptNo.Value : 0,
                fee,
                token = patientToken,   // real patient JWT — replaces guest token
                patient = patientDto      // full object for localStorage
            });
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error in register-new for mobile {Mobile}", mobile);
            return StatusCode(500, new
            {
                message = "Database error during registration. Please try again.",
                detail = sqlEx.Message,
                type = "SqlException"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in register-new for mobile {Mobile}", mobile);
            return StatusCode(500, new
            {
                message = ex.Message,
                type = ex.GetType().Name,
                inner = ex.InnerException?.Message
            });
        }
    }

    //[HttpGet("ping")]
    //[Authorize]
    //public IActionResult Ping() => Ok(new
    //{
    //    user = User.Identity?.Name,
    //    claims = User.Claims.Select(c => new { c.Type, c.Value }),
    //    isAuth = User.Identity?.IsAuthenticated
    //});

    //[HttpGet("token-debug")]
    //[AllowAnonymous]
    //public IActionResult TokenDebug()
    //{
    //    var authHeader = Request.Headers["Authorization"].ToString();
    //    if (string.IsNullOrEmpty(authHeader))
    //        return Ok(new { error = "No Authorization header" });

    //    var token = authHeader.Replace("Bearer ", "");
    //    try
    //    {
    //        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    //        var jwt = handler.ReadJwtToken(token);
    //        return Ok(new
    //        {
    //            issuer = jwt.Issuer,
    //            audience = jwt.Audiences,
    //            expires = jwt.ValidTo,
    //            isExpired = jwt.ValidTo < DateTime.UtcNow,
    //            claims = jwt.Claims.Select(c => new { c.Type, c.Value })
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        return Ok(new { error = ex.Message });
    //    }
    //}

    //[HttpGet("fee")]
    //[AllowAnonymous]
    //public async Task<IActionResult> GetFee(
    //    [FromQuery] string hospitalKey,
    //    [FromQuery] int deptId,
    //    [FromQuery] int sponsorId = 10)   
    //{
    //    var entry = HospitalRegistry.Find(hospitalKey);
    //    if (entry == null) return BadRequest();

    //    var orgId = 2;
    //    await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

    //    var fee = await ctx.VerticalDepartmentFeeDets
    //        .Where(v => v.DeptId == deptId
    //                 && v.SponsorId == sponsorId   
    //                 && v.VerticalId == VerticalId
    //                 && v.ServiceName == OpdType
    //                 && v.OrgId == orgId)
    //        .Select(v => v.Fee)
    //        .FirstOrDefaultAsync() ?? 0m;

    //    return Ok(new { fee, currency = "INR" });
    //}

    [HttpGet("fee")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFee(
    [FromQuery] string hospitalKey,
    [FromQuery] int deptId,
    [FromQuery] int sponsorId = 33,
    [FromQuery] int verticalId = 9,
    [FromQuery] string opdType = "GENERAL")
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null) return BadRequest();

        var orgId = 2;
        await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

        var fee = await ctx.VerticalDepartmentFeeDets
            .Where(v => v.DeptId == deptId
                     && v.SponsorId == sponsorId
                     && v.VerticalId == verticalId
                     && v.ServiceName == opdType
                     && v.OrgId == orgId)
            .Select(v => v.Fee)
            .FirstOrDefaultAsync() ?? 0m;

        return Ok(new { fee, currency = "INR" });
    }

    private static string BuildParamString() =>
        "@OrgId, @FinYr, @Id OUTPUT, @AppointmentNo, @AppointmentTime, " +
        "@UhidNo OUTPUT, @OpNo OUTPUT, @RegistrationDate, @RegistrationTime, " +
        "@Title, @PatientName, @Sl_PatientName, @PatientFirstName, @PatientLastName, " +
        "@RelTitle, @RelativeName, @Sl_RelativeName, @MotherName, @Sl_MotherName, " +
        "@Gender, @DateOfBirth, @AgeInDays, @AgeInMonths, @AgeInYears, @BloodGroup, " +
        "@SponsorId, @VerticalId, @RefByID, @DoctorId, @DepId, @DocUnitId, " +
        "@RegLocaionID, @CountryId, @StateId, @CityId, @RegionId, @District, " +
        "@Town, @Address, @PinCode, @EmailId, @MobileNo, @AltMobileNo, " +
        "@IDProofType, @IDProofNo, @AdharNo, @CorCountryId, @CorStateId, " +
        "@CorCityId, @CorRegionId, @CorTown, @CorAddress, @CorPinCode, " +
        "@EmgContactName, @EmgConactRel, @EmgContactEmailId, @EmgContactAdd, @EmgContactMobileNo, " +
        "@CardNo, @CardType, @CardRegNo, @CardRegDate, @EmpID, @RelWithEsm, " +
        "@ESMUhid, @EsmRank, @ReceiptMode, @BankCashId, @Cheque_CardNo, " +
        "@IsBillConsultancy, @RegAmount, @RecdAmount, @ConsultCharge, @TotalAmt, " +
        "@DisAmt, @BalAmount, @Remarks, @symptoms, @Int_Travel, @Covid_Rep, " +
        "@Health_Worker, @Covid_Contact, @Brought_From, @Brought_By, @First_Date, " +
        "@Sar_Screen, @AB_Test, @SystemName, @CreatedBy, @Religion, " +
        "@PatientRegDiscountDet, @VisitNo, @ManageConsultancyInReg, @OpdType, " +
        "@PEmpId, @FluTypeId, @PolicyNo, @ClaimId, @ServiceNo, @SchemeId, " +
        "@SchemeCardNo, @Nationality, @ApprovalType, @ReceiptNo OUTPUT, " +
        "@AbhaNo, @Success OUTPUT, @Message OUTPUT";


    private static PatientDto BuildPatientDto(
    PatientRegistration p,
    string hospitalKey,
    string hospitalName) => new()
    {
        UhidNo = p.UhidNo,
        OpNo = p.OpNo,
        Title = p.Title,
        PatientName = p.PatientName,
        Gender = p.Gender,
        DateOfBirth = p.DateOfBirth,
        AgeInYears = p.AgeInYears,
        BloodGroup = p.BloodGroup,
        MobileNo = p.MobileNo,
        EmailId = p.EmailId,
        Address = p.Address,
        Town = p.Town,
        District = p.District,
        OpdType = p.OpdType,
        RegistrationDate = p.RegistrationDate,
        RelativeName = p.RelativeName,
        RelTitle = p.RelTitle,
        Nationality = p.Nationality,
        AdharNo = p.AdharNo,
        HospitalKey = hospitalKey,
        HospitalName = hospitalName
    };

}