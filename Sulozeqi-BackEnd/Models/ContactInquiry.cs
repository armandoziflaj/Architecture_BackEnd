using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Sulozeqi_BackEnd.Models;

[Index(nameof(IsRead))]
public class ContactInquiry : CommonData
{
    [DataMember]
    [Required]
    [MaxLength(100)]
    public required string FullName { get; set; } = string.Empty;

    [DataMember]
    [Required]
    [MaxLength(100)]
    public required string Email { get; set; } = string.Empty;

    [DataMember]
    [MaxLength(30)] 
    public string PhoneNumber { get; set; } = string.Empty;

    [DataMember] 
    [Required] 
    [MaxLength(5000)]
    public required string Message { get; set; }
    
    [DataMember]
    public bool IsRead { get; set; }
}