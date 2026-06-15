using LabExp.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace LabExp.Models.TestModels
{
    public class TestModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int Number { get; set; }

        public string Subject { get; set; }

        public string Substance { get; set; }

        public int ScientistCount { get; set; }
    }
}
