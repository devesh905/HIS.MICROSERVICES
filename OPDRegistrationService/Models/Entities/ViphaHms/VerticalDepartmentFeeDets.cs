using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OPDRegistrationService.Models.Entities.Hms;

[Table("VerticalDepartment_Fee_Det")]
public class VerticalDepartmentFeeDets
{
    public int? SponsorId { get; set; }

    public int? DeptId { get; set; }

    public int? VerticalId { get; set; }

    public short? Sno { get; set; }

    public string? ServiceName { get; set; }

    public decimal? DisAmt { get; set; }

    public decimal? Fee { get; set; }

    public int? OrgId { get; set; }

    public string? SystemName { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }
}