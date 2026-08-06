
using System;
using System.ComponentModel.DataAnnotations;

namespace Sulozeqi_BackEnd.Models
{
    public class VisitorCounter
    {
        [Key]
        public int Id { get; set; }

        public long Count { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
