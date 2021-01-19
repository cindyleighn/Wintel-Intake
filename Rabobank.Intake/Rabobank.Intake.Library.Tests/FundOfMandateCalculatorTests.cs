namespace Rabobank.Intake.Library.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rabobank.Intake.Library.Model;

    [TestClass]
    public class FundOfMandateCalculatorTests
    {
        [TestMethod]
        public void GetFundOfMandates_Success()
        {
            string fileName = Path.GetFullPath(@"TestData/FundsOfMandatesData.xml");
            var mandatesService = new FundOfMandateCalculator();

            var result = mandatesService.GetFundOfMandates(fileName);

            result.FundsOfMandates.Should().NotBeEmpty("Should Not be Empty", result);
        }

        [TestMethod]
        public void CalculateMandates_CodeMatchFound()
        {
            string fileName = Path.GetFullPath(@"TestData/FundsOfMandatesData.xml");
            FundOfMandateCalculator mandatesService = new FundOfMandateCalculator();

            var portfolio = new Portfolio()
            {
                Positions = new List<Position>()
                {
                    new Position()
                    {
                        Code = "NL0000287100",
                        Value = 23456,
                        Name = "Optimix Mix Fund",
                    }
                }
            };
            var fundOfMandatesData = mandatesService.GetFundOfMandates(fileName);

            var result = mandatesService.CalculateMandates(portfolio, fundOfMandatesData);
            result.Positions.Should().NotBeEmpty("Code Mandates found", result);
        }

        [TestMethod]
        public void CalculateMandates_CodeMatchNotFound()
        {
            string fileName = Path.GetFullPath(@"TestData/FundsOfMandatesData.xml");
            FundOfMandateCalculator mandatesService = new FundOfMandateCalculator();

            var portfolio = new Portfolio()
            {
                Positions = new List<Position>()
                {
                    new Position()
                    {
                        Code = "NL0000292330",
                        Value = 45678,
                        Name = "Rabobank Core Aandelen Fonds T2",
                    }
                }
            };
            var fundOfMandatesData = mandatesService.GetFundOfMandates(fileName);

            var result = mandatesService.CalculateMandates(portfolio, fundOfMandatesData);
            result.Positions[0].Mandates.Should().BeNull("Code Mandates not found", result.Positions[0].Mandates);
        }
    }
}
