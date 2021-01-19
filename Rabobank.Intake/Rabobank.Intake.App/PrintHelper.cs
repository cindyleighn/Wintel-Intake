namespace Rabobank.Intake.App
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Rabobank.Intake.Library.Model;

    public static class PrintHelper
    {
        public static void Print(this Portfolio portfolio)
        {
            Console.WriteLine();

            portfolio.Positions.ForEach(position =>
            {
                PrintBreak();

                position.Print();
                position.Mandates.Print();
            });

            PrintBreak();

            Console.WriteLine();
        }

        private static void Print(this Position position)
        {
            Console.WriteLine($" {position.Code} - {position.Name,-40} - {position.Value.ToString("N0", CultureInfo.CreateSpecificCulture("nl-NL")),8}");
        }

        private static void Print(this List<Mandate> mandates)
        {
            mandates?.ForEach(mandate =>
            {
                Console.WriteLine($"  * {mandate.Name,-40} - {mandate.Allocation.ToString("P1", CultureInfo.CreateSpecificCulture("nl-NL")),9} - {mandate.Value.ToString("N0", CultureInfo.CreateSpecificCulture("nl-NL")),8}");
            });
        }

        private static void PrintBreak()
        {
            Console.WriteLine("--------------------------------------------------------------------");
        }
    }
}
