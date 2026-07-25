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
public class LabReportController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public LabReportController(IHospitalQueryService hospitals)
    {
        _hospitals = hospitals;
    }

    // GET api/labreport/by-uhid/{uhid}?hospitalKey=MRT
    [HttpGet("by-uhid/{uhid}")]
    public async Task<IActionResult> GetReportsByUhid(string uhid, [FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(uhid))
            return BadRequest(new { message = "UHID is required." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var lis = _hospitals.CreateLisContext(entry.LisCs);

        var reports = await (
            from ibm in lis.InvestBookingMas
            join rm in lis.ResultMas on ibm.Id equals rm.BId
            where ibm.UhidNo == uhid
               && ibm.IsCancel != true
            orderby ibm.Bill_Date descending
            select new LabReportSummaryDto
            {
                BookingId = ibm.Id,
                BillNo = ibm.BillNo,
                BillDate = ibm.Bill_Date,
                ResultMasId = rm.Id,
                ReportNo = rm.Doc_No,
                ReportDate = rm.Doc_Date,
                PatientName = ibm.PatientName,
                UhidNo = ibm.UhidNo,
                Gender = ibm.Gender,
                PrintRemarks = rm.PrintRemarks,
                Diagnosis = rm.Remarks
            }
        ).ToListAsync();

        if (!reports.Any())
            return NotFound(new { message = "No lab reports found for this UHID." });

        return Ok(reports);
    }

    // GET api/labreport/by-uhids?uhids=CSSH-001,CSSH-002&hospitalKey=MRT
    [HttpGet("by-uhids")]
    public async Task<IActionResult> GetReportsByUhids(
        [FromQuery] string uhids,
        [FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(uhids))
            return BadRequest(new { message = "No UHIDs provided." });

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        var uhidList = uhids.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        await using var lis = _hospitals.CreateLisContext(entry.LisCs);

        var reports = await (
            from ibm in lis.InvestBookingMas
            join rm in lis.ResultMas on ibm.Id equals rm.BId
            where uhidList.Contains(ibm.UhidNo!)
               && ibm.IsCancel != true
            orderby ibm.Bill_Date descending
            select new LabReportSummaryDto
            {
                BookingId = ibm.Id,
                BillNo = ibm.BillNo,
                BillDate = ibm.Bill_Date,
                ResultMasId = rm.Id,
                ReportNo = rm.Doc_No,
                ReportDate = rm.Doc_Date,
                PatientName = ibm.PatientName,
                UhidNo = ibm.UhidNo,
                Gender = ibm.Gender,
                PrintRemarks = rm.PrintRemarks,
                Diagnosis = rm.Remarks
            }
        ).ToListAsync();

        if (!reports.Any())
            return NotFound(new { message = "No lab reports found." });

        return Ok(reports);
    }

    // GET api/labreport/detail/{bookingId}?hospitalKey=MRT
    [HttpGet("detail/{bookingId:long}")]
    public async Task<IActionResult> GetReportDetail(long bookingId, [FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var lis = _hospitals.CreateLisContext(entry.LisCs);

        var booking = await lis.InvestBookingMas
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
            return NotFound(new { message = "Booking not found." });

        var resultMas = await lis.ResultMas
            .FirstOrDefaultAsync(r => r.BId == bookingId);

        if (resultMas == null)
            return NotFound(new { message = "Report not yet finalized for this booking." });

        var tabularRows = await (
            from rd in lis.ResultDet
            join tm in lis.TestMas on rd.TestId equals tm.Id
            join tim in lis.TestItemMaster on rd.TestItemId equals tim.Id
            where rd.BId == bookingId
            orderby rd.TestId, rd.Sno
            select new
            {
                rd.Sno,
                rd.TestId,
                TestName = tm.Test_Name,
                ParameterName = tim.TestItem_Name,
                rd.Result,
                rd.Unit,
                rd.Method,
                rd.RangeFrom,
                rd.RangeTo,
                RefRangeText = tim.Ref_Range_Intext,
                rd.RowColor
            }
        ).ToListAsync();

        var tabularGroups = tabularRows
            .GroupBy(r => new { r.TestId, r.TestName })
            .Select(g => new LabTestGroupDto
            {
                TestId = g.Key.TestId ?? 0,
                TestName = g.Key.TestName,
                Rows = g.Select(r => new LabResultRowDto
                {
                    Sno = r.Sno ?? 0,
                    TestName = r.TestName,
                    ParameterName = r.ParameterName,
                    Result = r.Result,
                    Unit = r.Unit,
                    Method = r.Method,
                    RangeFrom = r.RangeFrom,
                    RangeTo = r.RangeTo,
                    RefRangeText = r.RefRangeText,
                    IsOutOfRange = r.RowColor == "OutOfRange"
                }).ToList()
            }).ToList();

        var templateBlocks = await (
            from rtd in lis.ResultTemplateDet
            join tm in lis.TestMas on rtd.TestId equals tm.Id
            where rtd.BId == bookingId
            select new LabTemplateBlockDto
            {
                TestId = rtd.TestId ?? 0,
                TestName = tm.Test_Name,
                HtmlContent = rtd.TemplateDesc
            }
        ).ToListAsync();

        var detail = new LabReportDetailDto
        {
            BookingId = booking.Id,
            BillNo = booking.BillNo,
            BillDate = booking.Bill_Date,
            ReportNo = resultMas.Doc_No,
            ReportDate = resultMas.Doc_Date,
            UhidNo = booking.UhidNo,
            PatientName = booking.PatientName,
            Gender = booking.Gender,
            RefByName = booking.RefByName,
            PrintRemarks = resultMas.PrintRemarks,
            TabularTests = tabularGroups,
            TemplateTests = templateBlocks
        };

        return Ok(detail);
    }
}