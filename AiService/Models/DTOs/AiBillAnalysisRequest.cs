namespace AiService.Models.DTOs;

public class AiBillAnalysisRequest
{
    public string AdmNo { get; set; } = string.Empty;
    public string HospitalKey { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string Diagnosis { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int AdmissionDays { get; set; }
    public List<BillServiceItem> ExistingServices { get; set; } = new();

    public string DoctorName { get; set; } = string.Empty;
    public string WardBed { get; set; } = string.Empty;
    public string AgeSex { get; set; } = string.Empty;

    public string SponsorName { get; set; } = "";      // TPA/Insurance company name
    public string PatientType { get; set; } = "";      // General / TPA / CGHS / ESI
    public string AdmType { get; set; } = "";          // Emergency / Elective
    public string SurgeryName { get; set; } = "";      // if surgical case
    public decimal TotalChargesSoFar { get; set; }     // running total
    public decimal AmountReceived { get; set; }        // advances paid
    public decimal Balance { get; set; }               // balance due
    public bool IsDischarge { get; set; }              // is patient being discharged
    public string DischargeMode { get; set; } = "";    // Relieved / LAMA / Referred etc
}

public class BillServiceItem
{
    public string ServiceName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Quantity { get; set; }
}