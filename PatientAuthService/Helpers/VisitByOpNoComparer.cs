using  PatientAuthService.Models.DTOs;

namespace PatientAuthService.Helpers;

// This prevents the same visit appearing twice if it exists in both tables (which happens on the first registration visit — the PatientRegistration row and the first Patient_Consultancy row often share the same OpNo).

public class VisitByOpNoComparer : IEqualityComparer<VisitDetailDto>
{
    public bool Equals(VisitDetailDto? x, VisitDetailDto? y)
        => string.Equals(x?.OpNo, y?.OpNo, StringComparison.OrdinalIgnoreCase);

    public int GetHashCode(VisitDetailDto obj)
        => (obj.OpNo ?? "").ToUpperInvariant().GetHashCode();
}