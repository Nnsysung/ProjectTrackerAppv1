using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
   
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public DateTime EventDateUtc { get; set; }
        public string EventType { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public string ColumnName { get; set; }
        public string? OriginalValue { get; set; }
        public string? NewValue { get; set; }
        public string Ip { get; set; }
    }
}
