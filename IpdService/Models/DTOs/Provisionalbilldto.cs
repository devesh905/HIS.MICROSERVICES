namespace IpdService.Models.DTOs;

public class ProvisionalBillDto
{
    // Patient / Admission header
    public string? AdmNo { get; set; }
    public string? UhidNo { get; set; }
    public string? PatientName { get; set; }
    public string? Gender { get; set; }
    public int? AgeInYears { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string? WardBed { get; set; }
    public string? DoctorName { get; set; }
    public string? Department { get; set; }
    public string? Sponsor { get; set; }

    public string? Diagnosis { get; set; }
    public string? AdmType { get; set; }
    public string? PatientType { get; set; }

    // Bill summary
    public decimal TotalCharges { get; set; }       // sum of all NetAmount lines
    public decimal ServiceDiscount { get; set; }
    public decimal BillDiscount { get; set; }
    public decimal NetBilledAmount { get; set; }
    public decimal AmountReceived { get; set; }     // advance deposited - refunds
    public decimal Balance { get; set; }            // NetBilledAmount - AmountReceived

    public bool IsDischarge { get; set; }
    public string? DischargeDocNo { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string? DischargeTime { get; set; }
    public string? DischargeMode { get; set; }
    public string? PatientCondition { get; set; }
    public string? DischargeStatus { get; set; }

    // Charge sections
    public List<ProvisionalBillLineDto> AccommodationCharges { get; set; } = [];
    public List<ProvisionalBillLineDto> ServiceCharges { get; set; } = [];
    public List<ProvisionalBillLineDto> LaboratoryCharges { get; set; } = [];
    public List<ProvisionalBillLineDto> PharmacyCharges { get; set; } = [];

    public List<ProvisionalBillLineDto> RadiologyCharges { get; set; } = [];

    // Advance / receipt details
    public List<ReceiptDetailDto> Receipts { get; set; } = [];

    // Meta
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
    public bool IsFinalBill { get; set; } = false;
}

public class ProvisionalBillLineDto
{
    public string? ChargeHead { get; set; }         // e.g. "Nat General Ward", "Blood CBC"
    public string? Description { get; set; }
    public string? ChargeType { get; set; }         // S / R / L / Accommodation / Pharmacy
    public decimal Qty { get; set; }
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal DisPer { get; set; }
    public decimal DisAmt { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime? Date { get; set; }
}

public class ReceiptDetailDto
{
    public string? ReceiptNo { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public decimal Amount { get; set; }
    public string? Type { get; set; }   // IPD / Refund
}
