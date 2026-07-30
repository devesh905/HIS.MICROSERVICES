using OPDRegistrationService.Data;
using OPDRegistrationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OPDRegistrationService.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "employee")]
[Authorize]
public class OpdMetaController : ControllerBase
{
    private readonly IHospitalQueryService _hospitals;

    public OpdMetaController(IHospitalQueryService hospitals) => _hospitals = hospitals;

    [HttpGet("lookups")]
    public async Task<IActionResult> GetLookups([FromQuery] string hospitalKey)
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null)
            return BadRequest(new { message = $"Unknown hospital key: {hospitalKey}" });

        await using var ctx = _hospitals.CreateHmsContext(entry.HisCs);
        await using var ctxvipha = _hospitals.CreateViphaContext(entry.ViphaCs);
        var orgId = 2;

        var doctors = await ctx.DoctorMasters
            .Where(d => d.OrgId == orgId && d.IsActive == true)
            .OrderBy(d => d.Doc_Name)
            .Select(d => new { d.Id, d.Doc_Name, d.DepartmentId, d.DocUnitId })
            .ToListAsync();

        var departments = await ctx.DepartmentMas
            .Where(d => d.OrgId == orgId && d.IsActive == true)
            .OrderBy(d => d.Dept_Name)
            .Select(d => new { d.Id, d.Dept_Name })
            .ToListAsync();

        var sponsors = await ctx.SponsorMasters
            .Where(s => s.OrgId == orgId && s.IsActive == true)
            .OrderBy(s => s.Sponsor_Name)
            .Select(s => new { s.Id, s.Sponsor_Name })
            .ToListAsync();

        var verticals = await ctx.VerticalMasters
            .Where(v => v.OrgId == orgId && v.IsActive == true)
            .OrderBy(v => v.Vertical_Name)
            .Select(v => new { v.Id, v.Vertical_Name })
            .ToListAsync();

        var refDoctors = await ctx.DoctorMasters
            .Where(d => d.OrgId == orgId && d.IsActive == true)
            .OrderBy(d => d.Doc_Name)
            .Select(d => new { d.Id, d.Doc_Name })
            .ToListAsync();

        var patientTypes = await ctx.OpdTypes
            .Where(p => p.OrgId == orgId && p.IsActive == true)
            .OrderBy(p => p.Descript)
            .Select(p => new { p.Id, p.Descript })
            .ToListAsync();

        //var countries = await ctxvipha.CountryMasters
        //     .Where(c => c.IsActive == true)
        //     .OrderBy(c => c.Name)
        //    .Select(c => new { id = c.Id, name = c.Name })
        //     .ToListAsync();

        var countries = await ctxvipha.CountryMasters
             .OrderBy(c => c.Name)
            .Select(c => new { id = c.Id, name = c.Name })
            .ToListAsync();

        var states = await ctxvipha.StateMasters
            .Where(s => s.IsActive == true)
            .OrderBy(s => s.StateName)
            .Select(s => new { id = s.Id, stateName = s.StateName, countryId = s.CountryId })
            .ToListAsync();

        var cities = await ctxvipha.CityMasters
            .Where(c => c.IsActive == true)
            .OrderBy(c => c.CityName)
           .Select(c => new { id = c.Id, cityName = c.CityName, stateId = c.StateId })
            .ToListAsync();

        var titles = await ctx.TitleMasters
          .Where(t => t.OrgId == orgId && t.Title_Status == "Active") // adjust filter to match actual status values used in DB
         .OrderBy(t => t.Title_Name)
         .Select(t => new { t.Id, t.Title_Name, t.Gender })
        .ToListAsync();

        var relations = await ctx.RelationMasters
            .Where(r => r.OrgId == orgId && r.IsActive == true)
            .OrderBy(r => r.Descript)
            .Select(r => new { r.Id, r.Descript, r.Gender, r.TitleId })
            .ToListAsync();

        return Ok(new { doctors, departments, sponsors, verticals, refDoctors, patientTypes, countries, states, cities, titles, relations });
    }

    // GET api/opdmeta/fee?hospitalKey=MRT&deptId=5&sponsorId=10&verticalId=1&opdType=GENERAL
    [HttpGet("fee")]
    public async Task<IActionResult> GetFee(
        [FromQuery] string hospitalKey,
        [FromQuery] int deptId,
        [FromQuery] int sponsorId = 10,
        [FromQuery] int verticalId = 1,
        [FromQuery] string opdType = "GENERAL")
    {
        var entry = HospitalRegistry.Find(hospitalKey);
        if (entry == null) return BadRequest();

        await using var ctx = _hospitals.CreateHmsContext(entry.HisCs);

        var fee = await ctx.VerticalDepartmentFeeDets
            .Where(v => v.DeptId == deptId && v.SponsorId == sponsorId
                     && v.VerticalId == verticalId && v.ServiceName == opdType)
            .Select(v => v.Fee)
            .FirstOrDefaultAsync();

        return Ok(new { fee });
    }

}