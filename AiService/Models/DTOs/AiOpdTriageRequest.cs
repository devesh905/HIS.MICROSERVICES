namespace AiService.Models.DTOs;

public class AiOpdTriageRequest
{
    public string SymptomText { get; set; } = "";
    public string HospitalKey { get; set; } = "";
    public List<DoctorOption> Doctors { get; set; } = [];
    public List<DeptOption> Departments { get; set; } = [];
    public DateTime? VisitDate { get; set; }
}

public class DoctorOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Department { get; set; } = "";
}

public class DeptOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}