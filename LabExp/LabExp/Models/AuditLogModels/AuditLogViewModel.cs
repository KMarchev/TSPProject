namespace LabExp.Models.AuditLogModels
{
    public class AuditLogViewModel
    {
        public Guid? Id { get; set; }

        public DateTime TimeStamp { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public Guid? EntityId { get; set; }
    }
}
