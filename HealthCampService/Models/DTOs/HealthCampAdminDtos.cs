namespace HealthCampService.Models.DTOs;

public class SlotAdminListDto
{
    public int SlotId { get; set; }
    public string HospitalKey { get; set; } = "";
    public int CampTypeId { get; set; }
    public string CampTypeName { get; set; } = "";
    public DateTime SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSlotRequest
{
    public DateTime SlotDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
    public int? ModifiedBy { get; set; }
}

public class SlotDeactivateResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class BulkCreateSlotsRequest
{
    public string HospitalKey { get; set; } = "";
    public int CampTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public bool SkipSundays { get; set; } = true;
    public List<SlotTimeRange> TimeSlots { get; set; } = new();
    public int? CreatedBy { get; set; }
}

public class SlotTimeRange
{
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int Capacity { get; set; }
}

public class BulkCreateSlotsResultDto
{
    public int CreatedCount { get; set; }
    public int SkippedSundayCount { get; set; }
    public int SkippedOverlapCount { get; set; }
    public List<string> OverlapDetails { get; set; } = new(); // e.g. "21 Jul 08:00-09:00 already exists"
}