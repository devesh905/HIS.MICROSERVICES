using DoctorScheduleService.Data;
using DoctorScheduleService.Models.DTOs;
using DoctorScheduleService.Models.Entities.Portal;
using Microsoft.EntityFrameworkCore;

namespace DoctorScheduleService.Services;

public class DoctorScheduleService : IDoctorScheduleService
{
    private readonly PortalDbContext _portalDb;
    private readonly IHospitalQueryService _hospitals;
    private readonly ILogger<DoctorScheduleService> _logger;

    public DoctorScheduleService(
        PortalDbContext portalDb,
        IHospitalQueryService hospitals,
        ILogger<DoctorScheduleService> logger)
    {
        _portalDb = portalDb;
        _hospitals = hospitals;
        _logger = logger;
    }

    public async Task<List<AvailableDoctorDto>> GetAvailableDoctorsAsync(
        string hospitalKey, int departmentId, DateTime date)
    {
        var scheduleMonth = date.ToString("yyyy-MM");
        var dayOfWeek = (byte)date.DayOfWeek; // 0=Sunday...6=Saturday, matches System.DayOfWeek

        var rows = await _portalDb.DoctorSchedules
            .Where(s => s.HospitalKey == hospitalKey
                     && s.DepartmentId == departmentId
                     && s.ScheduleMonth == scheduleMonth
                     && s.DayOfWeek == dayOfWeek
                     && s.IsActive)
            .ToListAsync();

        if (rows.Count == 0)
            return new List<AvailableDoctorDto>();

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
        {
            _logger.LogWarning("GetAvailableDoctorsAsync: unknown hospital {Key}", hospitalKey);
            return new List<AvailableDoctorDto>();
        }

        await using var hisCtx = _hospitals.CreateHisContext(entry.HisCs);

        var doctorIds = rows.Select(r => r.DoctorId).Distinct().ToList();
        var doctorNames = await hisCtx.DoctorMasters
            .Where(d => doctorIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Doc_Name);

        return rows.Select(r => new AvailableDoctorDto
        {
            DoctorId = r.DoctorId,
            DoctorName = doctorNames.TryGetValue(r.DoctorId, out var name) ? name : "Unknown",
            SlotLabel = r.SlotLabel
        }).ToList();
    }

    public async Task<List<DoctorScheduleDto>> GetScheduleForMonthAsync(
        string hospitalKey, string scheduleMonth)
    {
        var rows = await _portalDb.DoctorSchedules
            .Where(s => s.HospitalKey == hospitalKey && s.ScheduleMonth == scheduleMonth && s.IsActive)
            .OrderBy(s => s.DepartmentId).ThenBy(s => s.DayOfWeek)
            .ToListAsync();

        if (rows.Count == 0)
            return new List<DoctorScheduleDto>();

        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null) return new List<DoctorScheduleDto>();

        await using var hisCtx = _hospitals.CreateHisContext(entry.HisCs);

        var doctorIds = rows.Select(r => r.DoctorId).Distinct().ToList();
        var deptIds = rows.Select(r => r.DepartmentId).Distinct().ToList();

        var doctorNames = await hisCtx.DoctorMasters
            .Where(d => doctorIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Doc_Name);

        var deptNames = await hisCtx.DepartmentMas
            .Where(d => deptIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Dept_Name);

        return rows.Select(r => new DoctorScheduleDto
        {
            Id = r.Id,
            DoctorId = r.DoctorId,
            DoctorName = doctorNames.TryGetValue(r.DoctorId, out var dn) ? dn : "Unknown",
            DepartmentId = r.DepartmentId,
            DepartmentName = deptNames.TryGetValue(r.DepartmentId, out var pn) ? pn : "Unknown",
            DayOfWeek = r.DayOfWeek,
            SlotLabel = r.SlotLabel,
            ScheduleMonth = r.ScheduleMonth
        }).ToList();
    }

    public async Task<int> AddScheduleAsync(DoctorScheduleCreateDto dto, int createdBy)
    {
        if (dto.DaysOfWeek == null || dto.DaysOfWeek.Count == 0)
            throw new ArgumentException("At least one day must be selected.");

        var entities = dto.DaysOfWeek.Select(day => new DoctorSchedule
        {
            HospitalKey = dto.HospitalKey,
            DoctorId = dto.DoctorId,
            DepartmentId = dto.DepartmentId,
            DayOfWeek = day,
            SlotLabel = dto.SlotLabel,
            ScheduleMonth = dto.ScheduleMonth,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.Now
        }).ToList();

        _portalDb.DoctorSchedules.AddRange(entities);
        await _portalDb.SaveChangesAsync();

        return entities.Count;
    }

    public async Task<bool> DeleteScheduleAsync(int id)
    {
        var row = await _portalDb.DoctorSchedules.FindAsync(id);
        if (row == null) return false;

        _portalDb.DoctorSchedules.Remove(row);
        await _portalDb.SaveChangesAsync();
        return true;
    }


    public async Task<List<DoctorScheduleRowDto>> GetSchedulesAsync(
        string hospitalKey, string scheduleMonth,
        int? departmentId, int? doctorId, int? dayOfWeek)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            throw new ArgumentException($"Unknown hospital key: {hospitalKey}");

        await using var hisDb = _hospitals.CreateHisContext(entry.HisCs);

        var query = _portalDb.DoctorSchedules
            .Where(s => s.HospitalKey == hospitalKey && s.ScheduleMonth == scheduleMonth);

        if (departmentId.HasValue) query = query.Where(s => s.DepartmentId == departmentId.Value);
        if (doctorId.HasValue) query = query.Where(s => s.DoctorId == doctorId.Value);
        if (dayOfWeek.HasValue) query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);

        var rows = await query
            .OrderBy(s => s.DayOfWeek).ThenBy(s => s.DepartmentId)
            .ToListAsync();

        if (!rows.Any())
            return new List<DoctorScheduleRowDto>();

        // Resolve doctor/department names in bulk (avoids N+1 queries)
        var doctorIds = rows.Select(r => r.DoctorId).Distinct().ToList();
        var deptIds = rows.Select(r => r.DepartmentId).Distinct().ToList();

        var doctorNames = await hisDb.DoctorMasters
            .Where(d => doctorIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Doc_Name);

        var deptNames = await hisDb.DepartmentMas
            .Where(d => deptIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Dept_Name);

        return rows.Select(s => new DoctorScheduleRowDto
        {
            Id = s.Id,
            HospitalKey = s.HospitalKey,
            DoctorId = s.DoctorId,
            DoctorName = doctorNames.TryGetValue(s.DoctorId, out var dn) ? dn : $"#{s.DoctorId}",
            DepartmentId = s.DepartmentId,
            DepartmentName = deptNames.TryGetValue(s.DepartmentId, out var pn) ? pn : $"#{s.DepartmentId}",
            DayOfWeek = s.DayOfWeek,
            SlotLabel = s.SlotLabel,
            ScheduleMonth = s.ScheduleMonth,
            IsActive = s.IsActive
        }).ToList();
    }

    public async Task<bool> UpdateScheduleAsync(int id, DoctorScheduleUpdateDto dto)
    {

        var row = await _portalDb.DoctorSchedules.FirstOrDefaultAsync(s => s.Id == id);
        if (row == null) return false;

        row.DoctorId = dto.DoctorId;
        row.DepartmentId = dto.DepartmentId;
        row.DayOfWeek = (byte)dto.DayOfWeek;
        row.SlotLabel = dto.SlotLabel;
        row.IsActive = dto.IsActive;

        await _portalDb.SaveChangesAsync();
        return true;
    }

    public async Task<(List<DoctorOptionDto> doctors, List<DepartmentOptionDto> departments)>
        GetDoctorAndDepartmentOptionsAsync(string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            throw new ArgumentException($"Unknown hospital key: {hospitalKey}");

        await using var hisDb = _hospitals.CreateHisContext(entry.HisCs);
        const int orgId = 2;

        var doctors = await hisDb.DoctorMasters
            .Where(d => d.OrgId == orgId && d.IsActive == true)
            .OrderBy(d => d.Doc_Name)
            .Select(d => new DoctorOptionDto { Id = d.Id, Name = d.Doc_Name })
            .ToListAsync();

        var departments = await hisDb.DepartmentMas
            .Where(d => d.OrgId == orgId && d.IsActive == true)
            .OrderBy(d => d.Dept_Name)
            .Select(d => new DepartmentOptionDto { Id = d.Id, Name = d.Dept_Name })
            .ToListAsync();

        return (doctors, departments);
    }
}