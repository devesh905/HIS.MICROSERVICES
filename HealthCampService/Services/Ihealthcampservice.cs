using HealthCampService.Models.DTOs;

namespace HealthCampService.Services;

public interface IHealthCampService
{
    Task<List<HealthCampTypeDto>> GetActiveCampTypesAsync(string hospitalKey);

    Task<List<SlotAvailabilityDto>> GetSlotAvailabilityAsync(
        string hospitalKey, int campTypeId, DateTime fromDate, DateTime toDate);

    Task<BookingResultDto> CreateBookingAsync(CreateBookingRequest request);

    Task<bool> CancelBookingAsync(string hospitalKey, string bookingRefNo, string? reason, int? cancelledBy);

    Task<BookingListItemDto?> GetBookingByRefNoAsync(string hospitalKey, string bookingRefNo);

    Task<List<BookingListItemDto>> GetBookingsBySlotAsync(int slotId);

    Task<List<BookingListItemDto>> GetBookingsByMobileAsync(string hospitalKey, string mobileNo);

    Task<int> CreateSlotAsync(CreateSlotRequest request, int? createdBy);

    Task<bool> MarkCardGeneratedAsync(string bookingRefNo, string sentVia);

    Task<BillDto?> GetReceiptByBillNoAsync(string hospitalKey, string billNo);

    Task<List<SlotAdminListDto>> GetSlotsForAdminAsync(string hospitalKey, int? campTypeId, DateTime fromDate, DateTime toDate);
    Task<bool> UpdateSlotAsync(int slotId, UpdateSlotRequest request);
    Task<SlotDeactivateResultDto> DeactivateSlotAsync(int slotId, int? modifiedBy);
    Task<BulkCreateSlotsResultDto> BulkCreateSlotsAsync(BulkCreateSlotsRequest request);
}   