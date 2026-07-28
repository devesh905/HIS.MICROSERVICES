using HealthCampService.Models.DTOs;
using HealthCampService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCampService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthCampController : ControllerBase
{
    private readonly IHealthCampService _service;
    private readonly ILogger<HealthCampController> _logger;

    public HealthCampController(IHealthCampService service, ILogger<HealthCampController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// Active camp/test types for a hospital (e.g. TMT, ECG) - for dropdown.
    [HttpGet("types/{hospitalKey}")]
    public async Task<IActionResult> GetCampTypes(string hospitalKey)
    {
        var result = await _service.GetActiveCampTypesAsync(hospitalKey);
        return Ok(result);
    }

    /// Slot availability for a camp type across a date range - for the slot picker UI.
    [HttpGet("slots")]
    public async Task<IActionResult> GetSlotAvailability(
        [FromQuery] string hospitalKey,
        [FromQuery] int campTypeId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime? toDate = null)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey) || campTypeId <= 0)
            return BadRequest("hospitalKey and campTypeId are required.");

        var end = toDate ?? fromDate.AddDays(30);
        var result = await _service.GetSlotAvailabilityAsync(hospitalKey, campTypeId, fromDate, end);
        return Ok(result);
    }

    /// Book a seat in a slot. Supports existing-UHID and new-registration paths
    /// (see CreateBookingRequest.IsNewRegistration). Writes only to PortalDb -
    /// no Vipha registration/billing happens here.
    [HttpPost("book")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _service.CreateBookingAsync(request);

        if (!result.Success)
            return Conflict(result); // 409 - slot full / duplicate / missing identity

        return Ok(result);
    }

    /// Cancel an existing booking by its reference number.
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelBooking(
        [FromQuery] string hospitalKey,
        [FromQuery] string bookingRefNo,
        [FromQuery] string? reason = null)
    {
        var cancelledBy = GetCurrentUserId();
        var success = await _service.CancelBookingAsync(hospitalKey, bookingRefNo, reason, cancelledBy);

        if (!success)
            return NotFound("Booking not found, already cancelled, or already processed in Vipha (contact reception to cancel a processed booking).");

        return Ok(new { success = true });
    }

    /// lookup a booking by its reference number (for status check / confirmation reprint).
    [HttpGet("booking/{hospitalKey}/{bookingRefNo}")]
    public async Task<IActionResult> GetBooking(string hospitalKey, string bookingRefNo)
    {
        var booking = await _service.GetBookingByRefNoAsync(hospitalKey, bookingRefNo);
        if (booking == null) return NotFound();
        return Ok(booking);
    }

    /// All bookings for a given slot - admin/reception view for a day's list.
    [HttpGet("slot/{slotId}/bookings")]
    public async Task<IActionResult> GetBookingsBySlot(int slotId)
    {
        var result = await _service.GetBookingsBySlotAsync(slotId);
        return Ok(result);
    }

    /// Patient's own bookings - for a "My Bookings" list on the portal.
    [HttpGet("my-bookings")]
    public async Task<IActionResult> GetMyBookings([FromQuery] string hospitalKey, [FromQuery] string mobileNo)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey) || string.IsNullOrWhiteSpace(mobileNo))
            return BadRequest("hospitalKey and mobileNo are required.");

        var result = await _service.GetBookingsByMobileAsync(hospitalKey, mobileNo);
        return Ok(result);
    }


    /// Admin: create a new slot (e.g. 7-8 AM, capacity 10, for TMT on a given date).
    [HttpPost("slots")]
    // [Authorize(Roles = "employee")] 
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotRequest request)
    {
        try
        {
            var createdBy = GetCurrentUserId();
            var slotId = await _service.CreateSlotAsync(request, createdBy);
            return Ok(new { slotId });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// Mark a booking's confirmation card as generated/sent.
    [HttpPost("booking/{bookingRefNo}/card-sent")]
    public async Task<IActionResult> MarkCardSent(string bookingRefNo, [FromQuery] string sentVia)
    {
        var success = await _service.MarkCardGeneratedAsync(bookingRefNo, sentVia);
        if (!success) return NotFound();
        return Ok(new { success = true });
    }

    /// Single-bill receipt lookup for a processed Health Camp booking.
    /// billNo passed as query param (not route segment) because Vipha BillNo
    /// path-segment routing even when URL-encoded as %2F.
    [HttpGet("receipt/{hospitalKey}")]
    public async Task<IActionResult> GetReceipt(string hospitalKey, [FromQuery] string billNo)
    {
        if (string.IsNullOrWhiteSpace(billNo))
            return BadRequest(new { message = "billNo is required." });

        var bill = await _service.GetReceiptByBillNoAsync(hospitalKey, billNo);
        if (bill == null) return NotFound(new { message = "Bill not found." });
        return Ok(bill);
    }

    /// Admin: list slots (including inactive) for management UI.
    [HttpGet("admin/slots")]
    public async Task<IActionResult> GetSlotsForAdmin(
        [FromQuery] string hospitalKey,
        [FromQuery] int? campTypeId,
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime? toDate = null)
    {
        if (string.IsNullOrWhiteSpace(hospitalKey))
            return BadRequest("hospitalKey is required.");

        var end = toDate ?? fromDate.AddDays(30);
        var result = await _service.GetSlotsForAdminAsync(hospitalKey, campTypeId, fromDate, end);
        return Ok(result);
    }

    /// Admin: edit an existing slot's timing/capacity.
    [HttpPut("slots/{slotId}")]
    public async Task<IActionResult> UpdateSlot(int slotId, [FromBody] UpdateSlotRequest request)
    {
        try
        {
            request.ModifiedBy = GetCurrentUserId();
            var success = await _service.UpdateSlotAsync(slotId, request);
            if (!success) return NotFound();
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// Admin: deactivate (soft-delete) a slot. Blocked if active bookings exist.
    [HttpDelete("slots/{slotId}")]
    public async Task<IActionResult> DeactivateSlot(int slotId)
    {
        var result = await _service.DeactivateSlotAsync(slotId, GetCurrentUserId());
        if (!result.Success) return Conflict(result);
        return Ok(new { success = true });
    }

    [HttpPost("slots/bulk")]
    public async Task<IActionResult> BulkCreateSlots([FromBody] BulkCreateSlotsRequest request)
    {
        try
        {
            request.CreatedBy = GetCurrentUserId();
            var result = await _service.BulkCreateSlotsAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private int? GetCurrentUserId()
    {
        var claim = User?.FindFirst("UserId") ?? User?.FindFirst("EmployeeId");
        return claim != null && int.TryParse(claim.Value, out var id) ? id : null;
    }
}