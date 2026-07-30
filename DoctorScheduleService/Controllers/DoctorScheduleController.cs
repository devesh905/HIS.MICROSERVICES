using DoctorScheduleService.Models.DTOs;
using DoctorScheduleService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorScheduleService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorScheduleController : ControllerBase
{
    private readonly IDoctorScheduleService _service;
    private readonly ILogger<DoctorScheduleController> _logger;

    public DoctorScheduleController(
        IDoctorScheduleService service,
        ILogger<DoctorScheduleController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // GET api/doctorschedule/available?hospitalKey=MRT&departmentId=5&date=2026-07-09
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(
        [FromQuery] string hospitalKey,
        [FromQuery] int departmentId,
        [FromQuery] DateTime? date)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey) || departmentId <= 0)
            return BadRequest(new { message = "hospitalKey and departmentId are required." });

        var targetDate = date ?? DateTime.Today;
        var result = await _service.GetAvailableDoctorsAsync(hospitalKey, departmentId, targetDate);
        return Ok(new { doctors = result, date = targetDate.ToString("yyyy-MM-dd") });
    }

    // GET api/doctorschedule/month?hospitalKey=MRT&scheduleMonth=2026-07
    [HttpGet("month")]
    public async Task<IActionResult> GetForMonth(
        [FromQuery] string hospitalKey,
        [FromQuery] string scheduleMonth)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey) || string.IsNullOrWhiteSpace(scheduleMonth))
            return BadRequest(new { message = "hospitalKey and scheduleMonth are required." });

        var result = await _service.GetScheduleForMonthAsync(hospitalKey, scheduleMonth);
        return Ok(new { schedule = result });
    }

    // NEW — GET api/doctorschedule/list?hospitalKey=MRT&scheduleMonth=2026-07&departmentId=5&doctorId=10&dayOfWeek=1
    // Admin management grid — same underlying data as /month but with optional extra filters
    // and a flat, edit-friendly row shape (id, doctorName, deptName resolved server-side).
    [HttpGet("list")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string hospitalKey,
        [FromQuery] string scheduleMonth,
        [FromQuery] int? departmentId,
        [FromQuery] int? doctorId,
        [FromQuery] int? dayOfWeek)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey) || string.IsNullOrWhiteSpace(scheduleMonth))
            return BadRequest(new { message = "hospitalKey and scheduleMonth are required." });

        try
        {
            var result = await _service.GetSchedulesAsync(hospitalKey, scheduleMonth, departmentId, doctorId, dayOfWeek);
            return Ok(new { schedules = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch schedule list for {Hospital}/{Month}", hospitalKey, scheduleMonth);
            return StatusCode(500, new { message = "Failed to load schedules." });
        }
    }

    // NEW — GET api/doctorschedule/meta?hospitalKey=MRT
    // Lightweight doctor + department lookup dedicated to this admin page,
    // separate from the larger OpdMetaController.GetLookups payload.
    [HttpGet("meta")]
    public async Task<IActionResult> GetMeta([FromQuery] string hospitalKey)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest(new { message = "hospitalKey is required." });

        try
        {
            var (doctors, departments) = await _service.GetDoctorAndDepartmentOptionsAsync(hospitalKey);
            return Ok(new { doctors, departments });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch schedule meta for {Hospital}", hospitalKey);
            return StatusCode(500, new { message = "Failed to load doctor/department list." });
        }
    }

    // POST api/doctorschedule
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] DoctorScheduleCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.HospitalKey) || dto.DoctorId <= 0 || dto.DepartmentId <= 0
            || string.IsNullOrWhiteSpace(dto.ScheduleMonth))
        {
            return BadRequest(new { message = "hospitalKey, doctorId, departmentId, scheduleMonth are required." });
        }

        try
        {
            // TODO: replace 0 with logged-in admin's user id once auth claim is wired for this endpoint
            var count = await _service.AddScheduleAsync(dto, createdBy: 0);
            return Ok(new { success = true, rowsCreated = count });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // NEW — PUT api/doctorschedule/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] DoctorScheduleUpdateDto dto)
    {
        if (dto.DoctorId <= 0 || dto.DepartmentId <= 0)
            return BadRequest(new { message = "doctorId and departmentId are required." });

        try
        {
            var updated = await _service.UpdateScheduleAsync(id, dto);
            if (!updated) return NotFound(new { message = "Schedule row not found." });
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update schedule {Id}", id);
            return StatusCode(500, new { message = "Failed to update schedule." });
        }
    }

    // DELETE api/doctorschedule/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteScheduleAsync(id);
        if (!deleted) return NotFound(new { message = "Schedule row not found." });
        return Ok(new { success = true });
    }
}