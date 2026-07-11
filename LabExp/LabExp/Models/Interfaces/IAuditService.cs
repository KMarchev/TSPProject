namespace LabExp.Models.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityName, Guid entityId);
    }
}
