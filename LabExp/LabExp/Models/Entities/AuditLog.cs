using System.ComponentModel.DataAnnotations;
namespace LabExp.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public Guid? EntityId { get; set; }

        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}
