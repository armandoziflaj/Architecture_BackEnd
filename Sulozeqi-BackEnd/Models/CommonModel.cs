using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sulozeqi_BackEnd.Models;

public abstract class CommonData
{
    [Key]
    public long Id { get; set; }
    public DateTime DateTimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateTimeUpdated { get; set; } = DateTime.UtcNow;
    public uint RowVersion { get; set; }
}