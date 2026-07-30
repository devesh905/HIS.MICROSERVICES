namespace DoctorScheduleService.Models.DTOs;

public class DoctorScheduleCreateDto
{
    public string HospitalKey { get; set; } = "";
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public List<byte> DaysOfWeek { get; set; } = new();   // e.g. [1,3,5] Mon/Wed/Fri
    public string? SlotLabel { get; set; }
    public string ScheduleMonth { get; set; } = "";        // "2026-07"
}

public class DoctorScheduleDto
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";
    public byte DayOfWeek { get; set; }
    public string? SlotLabel { get; set; }
    public string ScheduleMonth { get; set; } = "";
}

public class AvailableDoctorDto
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public string? SlotLabel { get; set; }
}

// used by PUT /api/doctorschedule/{id}
public class DoctorScheduleUpdateDto
{
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public int DayOfWeek { get; set; }        // 0=Sunday..6=Saturday
    public string? SlotLabel { get; set; }
    public bool IsActive { get; set; } = true;
}

// one row in the admin management grid (GET /api/doctorschedule/list)
public class DoctorScheduleRowDto
{
    public int Id { get; set; }
    public string HospitalKey { get; set; } = "";
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = "";
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";
    public int DayOfWeek { get; set; }
    public string? SlotLabel { get; set; }
    public string ScheduleMonth { get; set; } = "";
    public bool IsActive { get; set; }
}

// simple dropdown option shape for GET /api/doctorschedule/meta
public class DoctorOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}


public class DepartmentOptionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}