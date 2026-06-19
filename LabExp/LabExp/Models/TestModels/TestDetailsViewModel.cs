namespace LabExp.Models.TestModels
{
    public class TestDetailsViewModel
    {
        public Guid Id { get; set; }

        public int Number { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string Substance { get; set; } = null!;

        public string Status { get; set; } = null!;

        public List<string> Scientists { get; set; } = new();
    }
}