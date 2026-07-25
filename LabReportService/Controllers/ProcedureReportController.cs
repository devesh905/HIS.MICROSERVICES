using LabReportService.Data;
using LabReportService.Models.DTOs;
using LabReportService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LabReportService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProcedureReportController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public ProcedureReportController(IHospitalQueryService hospitals)
    {
        _hospitals = hospitals;
    }

    // GET api/procedurereport/by-uhid/{uhid}?hospitalKey=MRT
    [HttpGet("by-uhid/{uhid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUhid(string uhid, [FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(uhid))
            return BadRequest(new { message = "UHID is required." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHmsContext(entry.HmsCs);

        var reports = await ctx.ProcedureReports
            .Where(r => r.UHIDNo == uhid && r.T_Status != "Cancelled")
            .OrderByDescending(r => r.Report_Date)
            .Select(r => new ProcedureReportSummaryDto
            {
                Id = r.Id,
                ReportNo = r.Report_No,
                ReportDate = r.Report_Date,
                ReportTime = r.Report_Time,
                ServiceTitle = r.ServiceTitle,
                Impression = r.Impression,
                Remarks = r.Remarks,
                UhidNo = r.UHIDNo,
                RoomNo = r.RoomNo,
                HandOverTo = r.HandOverTo,
                IsCritical = r.IsCritical
            })
            .ToListAsync();

        if (!reports.Any())
            return NotFound(new { message = "No procedure reports found for this UHID." });

        return Ok(reports);
    }

    // GET api/procedurereport/by-uhids?uhids=X,Y&hospitalKey=MRT
    [HttpGet("by-uhids")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByUhids(
        [FromQuery] string uhids,
        [FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(uhids))
            return BadRequest(new { message = "No UHIDs provided." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        var uhidList = uhids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        await using var ctx = _hospitals.CreateHmsContext(entry.HmsCs);

        var reports = await ctx.ProcedureReports
            .Where(r => uhidList.Contains(r.UHIDNo!) && r.T_Status != "Cancelled")
            .OrderByDescending(r => r.Report_Date)
            .Select(r => new ProcedureReportSummaryDto
            {
                Id = r.Id,
                ReportNo = r.Report_No,
                ReportDate = r.Report_Date,
                ReportTime = r.Report_Time,
                ServiceTitle = r.ServiceTitle,
                Impression = r.Impression,
                Remarks = r.Remarks,
                UhidNo = r.UHIDNo,
                RoomNo = r.RoomNo,
                HandOverTo = r.HandOverTo,
                IsCritical = r.IsCritical
            })
            .ToListAsync();

        if (!reports.Any())
            return NotFound(new { message = "No procedure reports found." });

        return Ok(reports);
    }

    // GET api/procedurereport/detail/{id}?hospitalKey=MRT
    [HttpGet("detail/{id:long}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDetail(long id, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHmsContext(entry.HmsCs);

        var r = await ctx.ProcedureReports.FirstOrDefaultAsync(x => x.Id == id);
        if (r == null)
            return NotFound(new { message = "Report not found." });

        var patient = await ctx.PatientRegistrations
            .Where(p => p.UhidNo == r.UHIDNo)
            .OrderBy(p => p.RegistrationDate)
            .FirstOrDefaultAsync();

        var refDoctor = r.DocId.HasValue
            ? await ctx.DoctorMasters.FirstOrDefaultAsync(d => d.Id == r.DocId.Value)
            : null;

        var authDoctor = r.AuthDoctorId.HasValue
            ? await ctx.DoctorMasters.FirstOrDefaultAsync(d => d.Id == r.AuthDoctorId.Value)
            : null;

        var repDoctor = r.RepDocId.HasValue
            ? await ctx.DoctorMasters.FirstOrDefaultAsync(d => d.Id == r.RepDocId.Value)
            : null;

        return Ok(new ProcedureReportDetailDto
        {
            Id = r.Id,
            ReportNo = r.Report_No,
            ReportDate = r.Report_Date,
            ReportTime = r.Report_Time,
            ServiceTitle = r.ServiceTitle,
            Impression = r.Impression,
            LastImpression = r.LastImpression,
            RepResult = r.Rep_Result,
            Remarks = r.Remarks,
            ClinicalNotes = r.ClinicalNotes,
            Instruction = r.Instruction,
            Justification = r.Justification,
            Notes = r.Notes,
            UhidNo = r.UHIDNo,
            RoomNo = r.RoomNo,
            HandOverTo = r.HandOverTo,
            HandOverToMobileNo = r.HandOverToMobileNo,
            HandOverAt = r.HandOverAt,
            IsCritical = r.IsCritical,
            ReportMode = r.Report_Mode,
            RefDocNo = r.Ref_Doc_No,        // OPDB/124853/25 — the actual ref doc number
            AckDocNo = r.AckDocNo,          // 260307/311890  — the acknowledge doc number
            RefDocDateTime = r.Ref_Doc_DateTime,
            // Patient info
            PatientName = patient?.PatientName,
            PatientTitle = patient?.Title,
            Gender = patient?.Gender,
            AgeInYears = patient?.AgeInYears,
            Address = patient != null
                ? string.Join(", ", new[] { patient.Address, patient.Town, patient.District }
                    .Where(s => !string.IsNullOrWhiteSpace(s)))
                : null,
            // Doctors
            RefDoctorName = refDoctor?.Doc_Name,
            RefDoctorQual = refDoctor != null
                ? string.Join(", ", new[] { refDoctor.Qual1, refDoctor.Qual2 }
                    .Where(q => !string.IsNullOrWhiteSpace(q)))
                : null,
            AuthDoctorName = authDoctor?.Doc_Name,
            AuthDoctorQual = authDoctor != null
                ? string.Join(", ", new[] { authDoctor.Qual1, authDoctor.Qual2 }
                    .Where(q => !string.IsNullOrWhiteSpace(q)))
                : null,
            RepDoctorName = repDoctor?.Doc_Name,
        });
    }
}