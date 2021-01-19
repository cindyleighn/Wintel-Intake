namespace Rabobank.Intake.Library.Model
{
    using System.Collections.Generic;

    public class Position
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public decimal Value { get; set; }

        public List<Mandate> Mandates { get; set; }
    }
}
