using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCampService.Models.Entities.Hms;

[Table("Patient_Consultancy", Schema = "dbo")]
public class PatientConsultancy
{
    [Key]
    public long Id { get; set; }              

    public string? AppointmentNo { get; set; }
    public string? AppointmentTime { get; set; }

    [MaxLength(50)]
    public string? UhidNo { get; set; }

    [MaxLength(50)]
    public string? OpNo { get; set; }

    public DateTime? VisitDate { get; set; }
    public string? VisitTime { get; set; }

    public short? AgeInDays { get; set; }    
    public short? AgeInMonths { get; set; }  
    public short? AgeInYears { get; set; }    

    public short? SponsorId { get; set; }    
    public int? RefByID { get; set; }        

    public int? DoctorId { get; set; }      
    public int? DepId { get; set; }          
    public int? DocUnitId { get; set; }       

    public string? IDProofType { get; set; }  
    public string? IDProofNo { get; set; }   

    public string? ReceiptMode { get; set; }  
    public int? BankCashId { get; set; }     
    public string? Cheque_CardNo { get; set; }

    public decimal? RegAmount { get; set; }  
    public decimal? ConsultCharge { get; set; }
    public decimal? DisAmt { get; set; }     
    public decimal? TotalAmt { get; set; }   
    public decimal? RecdAmount { get; set; } 
    public decimal? BalAmount { get; set; }  

    public string? Cancel_Reason { get; set; }
    public string? Remarks { get; set; }     
    public string? SystemName { get; set; }   

    public DateTime? CreatedAt { get; set; }  
    public int? CreatedBy { get; set; }       
    public DateTime? LastModifiedDate { get; set; } 
    public int? LastModifiedBy { get; set; }  
    public string? ModifiedSystemName { get; set; } 

    public DateTime? CancelledDate { get; set; }   
    public int? CancelledBy { get; set; }    
    public string? CancelledSystemName { get; set; }

    public string? Religion { get; set; }    
    public int? OrgId { get; set; }        
    public short? FinYr { get; set; }        
    public short? VisitNo { get; set; }      
    public string? OpdType { get; set; }      
    public short? VerticalId { get; set; }    

    public string? PolicyNo { get; set; }   
    public string? ClaimId { get; set; }      
    public string? ServiceNo { get; set; }    
    public short? RegLocaionID { get; set; }  

    [Column("mode")]                          
    public string? Mode { get; set; }        

    public int? CancelledAuthId { get; set; } 
    public long? ReceiptNo { get; set; }     

    public bool? AccountPosting { get; set; } 
    public string? AccountPostingVoucher { get; set; } 

    public int? QueueNo { get; set; }      
    public string? ApprovalType { get; set; }
    public string? AbhaNo { get; set; }       
}