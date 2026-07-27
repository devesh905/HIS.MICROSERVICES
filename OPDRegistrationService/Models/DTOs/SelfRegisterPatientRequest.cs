using System.Diagnostics.Contracts;

public class SelfRegisterPatientRequest
{
    public string HospitalKey { get; set; } = "";
    public string Title { get; set; } = "Mr.";
    public string PatientFirstName { get; set; } = "";
    public string PatientLastName { get; set; } = "";
    public string RelTitle { get; set; } = "S/o";
    public string RelativeName { get; set; } = "";
    public string Gender { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public int AgeInYears { get; set; }
    public int AgeInMonths { get; set; }
    public int AgeInDays { get; set; }
    public string MobileNo { get; set; } = ""; 
    public string Address { get; set; } = "";
    public string Town { get; set; } = "";
    public string District { get; set; } = "";
    public string PinCode { get; set; } = "";
    public int DoctorId { get; set; }
    public int DepId { get; set; }

    public int DocUnitId { get; set; }

    public string BloodGroup { get; set; } = "";
    public string AdharNo { get; set; } = "";
    public string EmailId { get; set; } = "";
    public string AltMobileNo { get; set; } = "";
    public string Remarks { get; set; } = "";
    public string Nationality { get; set; } = "Indian";

    // VisitNo: 1 = new patient, 2 = revisit
    // Patient sends their UhidNo on revisit; empty for new
    public string UhidNo { get; set; } = "";
    public short VisitNo { get; set; } = 1;

    // Locked on backend — not sent by patient:
    // SponsorId=10, VerticalId=1, OpdType=GENERAL, ReceiptMode=CASH

    public short CountryId { get; set; } = 1;
    public int StateId { get; set; } = 0;
    public int CityId { get; set; } = 0;

}


public class SelfRegisterNewPatientRequest
{
    public string? Title { get; set; }
    public string PatientFirstName { get; set; } = "";
    public string PatientLastName { get; set; } = "";
    public string? RelTitle { get; set; }
    public string? RelativeName { get; set; }
    public string Gender { get; set; } = "";
    public DateTime? DateOfBirth { get; set; }
    public int AgeInYears { get; set; }
    public int AgeInMonths { get; set; }
    public int AgeInDays { get; set; }
    public string? BloodGroup { get; set; }
    public string? Nationality { get; set; } = "Indian";

    // Mobile is read from the verified JWT claim.
    // Sent in body as fallback (dev/testing only — do not trust in production).
    public string? MobileNo { get; set; }
    public string? AltMobileNo { get; set; }
    public string? EmailId { get; set; }
    public string? AdharNo { get; set; }

    public string? Address { get; set; }
    public string? Town { get; set; }
    public string? District { get; set; }
    public string? PinCode { get; set; }
    public int CountryId { get; set; } = 1;
    public int StateId { get; set; }
    public int CityId { get; set; }

    public string HospitalKey { get; set; } = "";
    public int DoctorId { get; set; }
    public int DepId { get; set; }
    public string? Remarks { get; set; }
    public int VerticalId { get; set; } = 1;

    // VisitNo is always 1 for new patients — hardcoded in the endpoint, not sent by client
}