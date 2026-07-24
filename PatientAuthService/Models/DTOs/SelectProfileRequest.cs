namespace PatientAuthService.Models.DTOs;

public class SelectProfileRequest
{
    public string Mobile { get; set; } = "";
    public string UhidNo { get; set; } = "";
    public string? AllUhids { get; set; }

    /// <summary>
    /// Short hospital identifier — "MRT" or "DDN".
    /// Set by the frontend from the profile card the user tapped.
    /// </summary>
    public string HospitalKey { get; set; } = "";
}