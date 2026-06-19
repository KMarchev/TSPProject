namespace LabExp.Models.SubstanceModels
{
    public class SubstanceModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Severity { get; set; }
    }
}