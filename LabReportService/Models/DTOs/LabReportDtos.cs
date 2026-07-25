namespace LabReportService.Models.DTOs;

public class LabReportSummaryDto
{
    public long BookingId { get; set; }
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public long ResultMasId { get; set; }
    public string? ReportNo { get; set; }   // e.g. RES/1/22
    public DateTime? ReportDate { get; set; }
    public string? PatientName { get; set; }
    public string? UhidNo { get; set; }
    public string? Gender { get; set; }
    public string? PrintRemarks { get; set; }
    public string? Diagnosis { get; set; }
}

// One parameter row in a tabular test
public class LabResultRowDto
{
    public int Sno { get; set; }
    public string? TestName { get; set; }
    public string? ParameterName { get; set; }
    public string? Result { get; set; }
    public string? Unit { get; set; }
    public string? Method { get; set; }
    public string? RangeFrom { get; set; }
    public string? RangeTo { get; set; }
    public string? RefRangeText { get; set; }  // e.g. "< 0.15", "Negative"
    public bool IsOutOfRange { get; set; }   // true when RowColor == "OutOfRange"
}

// One HTML-template block (HIV, culture, etc.)
public class LabTemplateBlockDto
{
    public int TestId { get; set; }
    public string? TestName { get; set; }
    public string? HtmlContent { get; set; }  // render as innerHTML on frontend
}

// Full detail for one report (BId)
public class LabReportDetailDto
{
    // Header
    public long BookingId { get; set; }
    public string? BillNo { get; set; }
    public DateTime? BillDate { get; set; }
    public string? ReportNo { get; set; }
    public DateTime? ReportDate { get; set; }
    public string? UhidNo { get; set; }
    public string? PatientName { get; set; }
    public string? Gender { get; set; }
    public string? RefByName { get; set; }
    public string? PrintRemarks { get; set; }

    // Tabular rows grouped by test
    public List<LabTestGroupDto> TabularTests { get; set; } = new();

    // HTML template blocks
    public List<LabTemplateBlockDto> TemplateTests { get; set; } = new();
}

// Tabular rows grouped by test name
public class LabTestGroupDto
{
    public int TestId { get; set; }
    public string? TestName { get; set; }
    public List<LabResultRowDto> Rows { get; set; } = new();
}