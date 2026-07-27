using IpdService.Models.DTOs;
using IpdService.Models.Entities.ViphaHms;
using IpdService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IpdPackageController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;
    private readonly ILogger<IpdPackageController> _logger;
    private readonly IConfiguration _config;

    public IpdPackageController(
        IHospitalQueryService hospitals,
        ILogger<IpdPackageController> logger,
        IConfiguration config)
    {
        _hospitals = hospitals;
        _logger = logger;
        _config = config;
    }

    [HttpGet("details")]
    public async Task<IActionResult> GetPackageDetails(
        [FromQuery] string hospitalKey,
        [FromQuery] string admNo,
        [FromQuery] int packageId)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });
        if (string.IsNullOrWhiteSpace(admNo))
            return BadRequest(new { message = "admNo is required." });
        if (packageId <= 0)
            return BadRequest(new { message = "packageId must be a positive integer." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital: {hospitalKey}" });

        // OrgId, DefaultLab, DefaultPharmacy are hospital-specific.
        // Adjust these if your registry carries them; hardcoded to match SP test values for now.
        const int orgId = 2;
        const int defaultLab = 3;
        const int defaultPharmacy = 7;

        try
        {
            await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

            var results = await ctx.IpdPackageDetailResults
                .FromSqlRaw(
                    "EXEC HMS_IPD_Bill_Mas_GetPackageDetails @OrgId, @DefaultLab, @DefaultPharamcy, @Adm_No, @PackageId",
                    new SqlParameter("@OrgId", SqlDbType.Int) { Value = orgId },
                    new SqlParameter("@DefaultLab", SqlDbType.Int) { Value = defaultLab },
                    new SqlParameter("@DefaultPharamcy", SqlDbType.Int) { Value = defaultPharmacy },
                    new SqlParameter("@Adm_No", SqlDbType.NVarChar) { Value = admNo },
                    new SqlParameter("@PackageId", SqlDbType.Int) { Value = packageId })
                .ToListAsync();

            var dto = results.Select(r => new IpdPackageDetailsDto
            {
                OrgId = r.OrgId,
                Id = r.Id,
                BillNo = r.BillNo,
                SerId = r.SerId,
                SerName = r.Ser_Name,
                Typ = r.Typ
            }).ToList();

            return Ok(new
            {
                admNo,
                packageId,
                count = dto.Count,
                items = dto
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching package details for AdmNo={AdmNo} PackageId={PackageId}", admNo, packageId);
            return StatusCode(500, new
            {
                message = ex.Message,
                type = ex.GetType().Name,
                inner = ex.InnerException?.Message
            });
        }
    }

    // Adm No se packages list
    // GET api/ipdpackage/by-admission?hospitalKey=MRT&admNo=IP-220124014
    [HttpGet("by-admission")]
    public async Task<IActionResult> GetPackagesByAdmission(
        [FromQuery] string hospitalKey,
        [FromQuery] string admNo)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });
        if (string.IsNullOrWhiteSpace(admNo))
            return BadRequest(new { message = "admNo is required." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital: {hospitalKey}" });

        const int orgId = 2;

        try
        {
            await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

            var results = await ctx.PackageListResults
                .FromSqlRaw(
                    "EXEC HMS_IPD_Service_Bill_Det_GetPackageDetailByAdmNo @OrgId, @Adm_No",
                    new SqlParameter("@OrgId", SqlDbType.Int) { Value = orgId },
                    new SqlParameter("@Adm_No", SqlDbType.NVarChar) { Value = admNo })
                .ToListAsync();

            return Ok(new
            {
                admNo,
                count = results.Count,
                packages = results.Select(r => new
                {
                    packageId = r.SerId,
                    packageName = r.Ser_Name
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching packages for AdmNo={AdmNo}", admNo);
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    // Package master details (service master se — what the package contains)
    // GET api/ipdpackage/master-details?hospitalKey=MRT&packageId=2366
    [HttpGet("master-details")]
    public async Task<IActionResult> GetPackageMasterDetails(
        [FromQuery] string hospitalKey,
        [FromQuery] int packageId)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });
        if (packageId <= 0)
            return BadRequest(new { message = "packageId must be positive." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital: {hospitalKey}" });

        const int orgId = 2;

        try
        {
            await using var ctx = _hospitals.CreateHisContext(entry.HisCs);

            var results = await ctx.PackageMasterDetailResults
                .FromSqlRaw(
                    "EXEC HMS_Service_Master_GetPackageDetailsById @OrgId, @Id",
                    new SqlParameter("@OrgId", SqlDbType.Int) { Value = orgId },
                    new SqlParameter("@Id", SqlDbType.BigInt) { Value = (long)packageId })
                .ToListAsync();

            return Ok(new
            {
                packageId,
                count = results.Count,
                items = results.Select(r => new
                {
                    sno = r.Sno,
                    typ = r.Typ,
                    serId = r.SerId,
                    serName = r.Ser_Name,
                    actualRate = r.ActualRate,
                    packageRate = r.PackageRate,
                    shareAmt = r.ShareAmt
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching master details for PackageId={PackageId}", packageId);
            return StatusCode(500, new { message = ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("service-register")]
    public async Task<IActionResult> GetPackageServiceRegister(
        [FromQuery] string hospitalKey,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? userId = "",
        [FromQuery] int outSourced = 0,
        [FromQuery] int osAccountId = 0)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });
        if (fromDate == default || toDate == default)
            return BadRequest(new { message = "fromDate and toDate are required." });
        if (toDate < fromDate)
            return BadRequest(new { message = "toDate cannot be before fromDate." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital: {hospitalKey}" });

        const int orgId = 2;

        try
        {
            var results = new List<PackageServiceRegisterResult>();

            var cs = _config.GetConnectionString(entry.HisCs)
          ?? throw new InvalidOperationException($"Connection string '{entry.HisCs}' not found.");
            await using var conn = new SqlConnection(cs);
            await conn.OpenAsync();

            // ANSI_WARNINGS OFF ....  truncation error suppress ho jaayega ... 
            // here Raw ADO.NET use kiya EF Core ke bajaye — SET ANSI_WARNINGS OFF pehle execute karo, phir SP call karo. Yeh truncation error suppress kar deta hai SQL Server mein.
            await using (var warnCmd = new SqlCommand("SET ANSI_WARNINGS OFF", conn))
                await warnCmd.ExecuteNonQueryAsync();

            await using var cmd = new SqlCommand(
                "EXEC Rep_Daily_Under_Package_Service_Register_Hms @Fdate, @Tdate, @OrgId, @UserId, @OutSourced, @OSAccountId",
                conn);

            cmd.Parameters.Add(new SqlParameter("@Fdate", SqlDbType.DateTime) { Value = fromDate.Date });
            cmd.Parameters.Add(new SqlParameter("@Tdate", SqlDbType.DateTime) { Value = toDate.Date });
            cmd.Parameters.Add(new SqlParameter("@OrgId", SqlDbType.Int) { Value = orgId });
            cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.NVarChar) { Value = userId ?? "" });
            cmd.Parameters.Add(new SqlParameter("@OutSourced", SqlDbType.Int) { Value = outSourced });
            cmd.Parameters.Add(new SqlParameter("@OSAccountId", SqlDbType.Int) { Value = osAccountId });

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new PackageServiceRegisterResult
                {
                    DocNo = reader["DocNo"] as string,
                    UhidNo = reader["UhidNo"] as string,
                    Adm_No = reader["Adm_No"] as string,
                    DocDate = reader["DocDate"] as DateTime?,
                    PatientName = reader["PatientName"] as string,
                    Paymode = reader["Paymode"] as string,
                    DocName = reader["DocName"] as string,
                    SponsorName = reader["SponsorName"] as string,
                    SerName = reader["SerName"] as string,
                    Qty = reader["Qty"] as int?,
                    NetAmt = reader["NetAmt"] == DBNull.Value ? null : Convert.ToDecimal(reader["NetAmt"]),
                    RefBy = reader["RefBy"] as string,
                    UnderServiceId = reader["UnderServiceId"] as int?,
                    PackageName = reader["PackageName"] as string,
                    CreatedBy = reader["CreatedBy"] as string,
                });
            }

            var grouped = results
                .GroupBy(r => new { r.UnderServiceId, r.PackageName })
                .Select(g => new
                {
                    packageId = g.Key.UnderServiceId,
                    packageName = g.Key.PackageName,
                    totalNet = g.Sum(r => r.NetAmt ?? 0),
                    itemCount = g.Count(),
                    items = g.Select(r => new
                    {
                        docNo = r.DocNo,
                        uhidNo = r.UhidNo,
                        admNo = r.Adm_No,
                        docDate = r.DocDate,
                        patientName = r.PatientName,
                        paymode = r.Paymode,
                        doctorName = r.DocName,
                        sponsorName = r.SponsorName,
                        serName = r.SerName,
                        qty = r.Qty,
                        netAmt = r.NetAmt,
                        packageName = r.PackageName,
                        createdBy = r.CreatedBy,
                        refBy = r.RefBy
                    }).ToList()
                })
                .OrderBy(g => g.packageName)
                .ToList();

            return Ok(new
            {
                fromDate,
                toDate,
                totalRecords = results.Count,
                totalNet = results.Sum(r => r.NetAmt ?? 0),
                packages = grouped
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in package service register for {From} to {To}", fromDate, toDate);
            return StatusCode(500, new
            {
                message = ex.Message,
                type = ex.GetType().Name,
                inner = ex.InnerException?.Message
            });
        }
    }
}