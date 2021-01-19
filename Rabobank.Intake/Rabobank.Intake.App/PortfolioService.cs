namespace Rabobank.Intake.App
{
    using System.Collections.Generic;
    using System.IO;
    using Rabobank.Intake.Library;
    using Rabobank.Intake.Library.Interfaces;
    using Rabobank.Intake.Library.Model;

    public class PortfolioService
    {
        private readonly IFundOfMandateCalculator fundOfMandateCalculator;

        public PortfolioService(IFundOfMandateCalculator fundOfMandateCalculator)
        {
            this.fundOfMandateCalculator = fundOfMandateCalculator;
        }

        public Portfolio GetPortfolioWithMandates() =>
            fundOfMandateCalculator
                .CalculateMandates(GetPortfolio, GetFundOfMandatesData);

        private FundsOfMandatesData GetFundOfMandatesData =>
            fundOfMandateCalculator
                .GetFundOfMandates(Path.GetFullPath(@"FundsOfMandatesData.xml"));

        private Portfolio GetPortfolio =>
            new Portfolio()
            {
                Positions = new List<Position>()
                {
                    CreatePosition("NL0000009165", "Heineken", 12345),
                    CreatePosition("NL0000287100", "Optimix Mix Fund", 23456),
                    CreatePosition("LU0035601805", "DP Global Strategy L High", 34567),
                    CreatePosition("NL0000292332", "Rabobank Core Aandelen Fonds T2", 45678),
                    CreatePosition("LU0042381250", "Morgan Stanley Invest US Gr Fnd", 56789)
                }
            };

        private Position CreatePosition(string code, string name, decimal value) =>
            new Position() { Code = code, Name = name, Value = value };
    }
}
