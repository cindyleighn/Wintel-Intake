namespace Rabobank.Intake.Library.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rabobank.Intake.Library.Tests.Setup;

    [TestClass]
    public class FundOfMandateCalculatorTests
    {
        private string _fileName;
        FundOfMandateCalculator _mandatesCalculator;

        [TestInitialize]
        public void Initialize()
        {
            _fileName = Path.GetFullPath(@"TestData/FundsOfMandatesData.xml");

            _mandatesCalculator = new FundOfMandateCalculator();
        }

        #region GetFundOfMandates tests

        [TestMethod]
        public void GetFundOfMandates_FilePathNotProvidedFailure()
        {
            Action act = () => new FundOfMandateCalculator().GetFundOfMandates("");

            act
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("File path not provided, provide file path to read mandates.")
            ;
        }

        [TestMethod]
        public void GetFundOfMandates_FileDoesNotExistFailure()
        {
            _fileName = $@"C:\temp\{Guid.NewGuid()}";
            Action act = () => new FundOfMandateCalculator().GetFundOfMandates(_fileName);

            act
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("File does not exist at the provided location.")
            ;
        }

        [TestMethod]
        public void GetFundOfMandates_IncorrectFileFormat()
        {
            _fileName = Path.GetFullPath(@"TestData/Dummy.json");
            Action act = () => new FundOfMandateCalculator().GetFundOfMandates(_fileName);

            act
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("Provided file to read mandates is not in xml format.")
            ;
        }

        [TestMethod]
        public void GetFundOfMandates_Success()
        {
            var result = _mandatesCalculator.GetFundOfMandates(_fileName);

            result.FundsOfMandates.Should().NotBeEmpty("Should Not be Empty", result);
        }

        [TestMethod]
        public void GetFundOfMandates_FundOfMandatesContainsMandates()
        {
            var result = _mandatesCalculator.GetFundOfMandates(_fileName);

            result.FundsOfMandates.First()
                .Mandates.Should().NotBeEmpty("Should Not be Empty", result)
                .And
                .HaveCount(3);
        }

        [TestMethod]
        public void GetFundOfMandates_ValidateFundOfMandatesData()
        {
            XDocument xmlData = XDocument.Load(_fileName);
            var fundOfMandatesElement = XName.Get("FundOfMandates", "http://amt.rnss.rabobank.nl/");
            var instrumentCodeElement = XName.Get("InstrumentCode", "http://amt.rnss.rabobank.nl/");
            var instrumentNameElement = XName.Get("InstrumentName", "http://amt.rnss.rabobank.nl/");
            var liquidityAllocationElement = XName.Get("LiquidityAllocation", "http://amt.rnss.rabobank.nl/");

            var dataFromXML = from e in xmlData.Descendants(fundOfMandatesElement)
                              select new
                              {
                                  InstrumentCode = e.Element(instrumentCodeElement).Value,
                                  InstrumentName = e.Element(instrumentNameElement).Value,
                                  LiquidityAllocation = decimal.Parse(e.Element(liquidityAllocationElement).Value)
                              };

            var fundOfMandates = _mandatesCalculator.GetFundOfMandates(_fileName).FundsOfMandates;

            fundOfMandates.Should().NotBeEmpty()
                .And.SatisfyRespectively(
                firstItem =>
                {
                    firstItem.InstrumentCode.Should().Be(dataFromXML.First().InstrumentCode);
                    firstItem.InstrumentName.Should().Be(dataFromXML.First().InstrumentName);
                    firstItem.LiquidityAllocation.Should().Be(dataFromXML.First().LiquidityAllocation);
                },
                secondItem =>
                {
                    secondItem.InstrumentCode.Should().Be(dataFromXML.ElementAt(1).InstrumentCode);
                    secondItem.InstrumentName.Should().Be(dataFromXML.ElementAt(1).InstrumentName);
                    secondItem.LiquidityAllocation.Should().Be(dataFromXML.ElementAt(1).LiquidityAllocation);
                },
                thirdItem =>
                {
                    thirdItem.InstrumentCode.Should().Be(dataFromXML.ElementAt(2).InstrumentCode);
                    thirdItem.InstrumentName.Should().Be(dataFromXML.ElementAt(2).InstrumentName);
                    thirdItem.LiquidityAllocation.Should().Be(dataFromXML.ElementAt(2).LiquidityAllocation);
                },
                _ => { });
        }

        [TestMethod]
        public void GetFundOfMandates_ValidateMandatesData()
        {
            XDocument xmlData = XDocument.Load(_fileName);
            var mandatesElement = XName.Get("Mandate", "http://amt.rnss.rabobank.nl/");
            var mandateIdElement = XName.Get("MandateId", "http://amt.rnss.rabobank.nl/");
            var mandateNameElement = XName.Get("MandateName", "http://amt.rnss.rabobank.nl/");
            var allocationElement = XName.Get("Allocation", "http://amt.rnss.rabobank.nl/");

            var dataFromXML = from e in xmlData.Descendants(mandatesElement)
                              select new
                              {
                                  MandateId = e.Element(mandateIdElement).Value,
                                  MandateName = e.Element(mandateNameElement).Value,
                                  Allocation = decimal.Parse(e.Element(allocationElement).Value)
                              };

            var fundOfMandates = _mandatesCalculator.GetFundOfMandates(_fileName).FundsOfMandates;
            var mandates = fundOfMandates.SelectMany(x => x.Mandates);

            var index = 0;
            foreach (var mandate in mandates)
            {
                mandate.MandateId.Should().Be(dataFromXML.ElementAt(index).MandateId);
                mandate.Allocation.Should().Be(dataFromXML.ElementAt(index).Allocation);
                mandate.MandateName.Should().Be(dataFromXML.ElementAt(index).MandateName);
                index++;
            }
        }

        #endregion GetFundOfMandates tests

        #region CalculateMandates test
        [TestMethod]
        public void CalculateMandates_PortfolioNullException()
        {
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);
            Action act = () => _mandatesCalculator.CalculateMandates(null, fundOfMandatesData);

            act
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("portfolio can not be null, it is required to build portfolio.")
            ;
        }

        [TestMethod]
        public void CalculateMandates_FundOfMandatesDataNullException()
        {
            var portfolio = PortfolioProvider.MatchingPositionCode;
            Action act = () => _mandatesCalculator.CalculateMandates(portfolio, null);

            act
            .Should()
            .Throw<ArgumentException>()
            .WithMessage("fundOfMandatesData can not be null, it is required to build portfolio.")
            ;
        }

        [TestMethod]
        public void CalculateMandates_CodeMatchFound()
        {
            var portfolio = PortfolioProvider.MatchingPositionCode;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            var result = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData);
            result.Positions.Should().NotBeEmpty("Code Mandates found", result);
        }

        [TestMethod]
        public void CalculateMandates_CodeMatchNotFound()
        {
            var portfolio = PortfolioProvider.NotMatchingPositionCode;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            var result = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData);
            result.Positions[0].Mandates.Should().BeNull("Code Mandates not found", result.Positions[0].Mandates);
        }

        [TestMethod]
        public void CalculateMandates_LiquidityAllocationGreaterThanZero()
        {
            var portfolio = PortfolioProvider.MatchingPositionForLiquidityMandate;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            var result = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData);
            result.Positions.Last().Mandates.Last().Name.Should().Be("Liquidity");
        }

        [TestMethod]
        public void CalculateMandates_LiquidityAllocationLessThanZero()
        {
            var portfolio = PortfolioProvider.MatchingPositionNoLiquidityMandate;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            var result = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData);
            result.Positions.Last().Mandates.Any(mandate => mandate.Name == "Liquidity").Should().Be(false);
        }

        [TestMethod]
        public void TestDataPortfolioLoaded_ValidateMandateAllocationData()
        {
            //arrange
            XDocument xmlData = XDocument.Load(_fileName);
            var mandatesElement = XName.Get("Mandate", "http://amt.rnss.rabobank.nl/");
            var allocationElement = XName.Get("Allocation", "http://amt.rnss.rabobank.nl/");

            var dataFromXML = from e in xmlData.Descendants(mandatesElement)
                              select new
                              {
                                  Allocation = decimal.Parse(e.Element(allocationElement).Value)
                              };

            var portfolio = PortfolioProvider.MatchingPositionCode;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            //act
            var positions = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData).Positions;

            //assert
            var mandates = positions.SelectMany(position => position.Mandates);
            var index = 0;
            foreach (var mandate in mandates)
            {
                if (mandate.Name == "Liquidity")
                {
                    continue;
                }
                mandate.Allocation.Should().Be(dataFromXML.ElementAt(index).Allocation / 100);
                index++;
            }
        }

        [TestMethod]
        public void TestDataPortfolioLoaded_ValidateMandateValue()
        {
            //arrange
            XDocument xmlData = XDocument.Load(_fileName);
            var mandatesElement = XName.Get("Mandate", "http://amt.rnss.rabobank.nl/");
            var mandateIdElement = XName.Get("MandateId", "http://amt.rnss.rabobank.nl/");
            var allocationElement = XName.Get("Allocation", "http://amt.rnss.rabobank.nl/");

            var dataFromXML = from e in xmlData.Descendants(mandatesElement)
                              select new
                              {
                                  MandateId = e.Element(mandateIdElement).Value,
                                  Allocation = decimal.Parse(e.Element(allocationElement).Value)
                              };
            var testMandateIds = new string[] { "NL0000287100-01", "NL0000287100-02", "NL0000287100-03" };
            var testMandates = dataFromXML.Where(data => testMandateIds.Contains(data.MandateId));

            var portfolio = PortfolioProvider.MatchingPositionCode;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            //act
            var positions = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData).Positions;

            var positionValue = positions.First().Value;
            var mandates = positions.SelectMany(position => position.Mandates);

            //assert
            var index = 0;
            foreach (var mandate in mandates)
            {
                if (mandate.Name == "Liquidity")
                {
                    continue;
                }

                var mandateAllocation = testMandates.ElementAt(index).Allocation;
                decimal expectedValue = Math.Round(decimal.Multiply(positionValue, mandateAllocation) / 100, MidpointRounding.AwayFromZero);
                mandate.Value.Should().Be(expectedValue);
                index++;
            }
        }

        [TestMethod]
        public void CalculateMandates_ValidateLiquidityMandateAllocation()
        {
            //arrange
            decimal expectedLiquidityMandateAllocation = (decimal)0.001;

            var portfolio = PortfolioProvider.MatchingPositionCode;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            //act
            var positions = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData).Positions;


            var mandates = positions.SelectMany(position => position.Mandates);

            //assert
            foreach (var mandate in mandates)
                if (mandate.Name == "Liquidity")
                    mandate.Allocation
                        .Should()
                        .Be(expectedLiquidityMandateAllocation);
        }

        [TestMethod]
        public void CalculateMandates_ValidateLiquidityMandateValue()
        {
            //arrange
            var portfolio = PortfolioProvider.MatchingPositionCode;
            var fundOfMandatesData = _mandatesCalculator.GetFundOfMandates(_fileName);

            //act
            var positions = _mandatesCalculator.CalculateMandates(portfolio, fundOfMandatesData).Positions;

            var positionValue = positions.First().Value;

            var mandates = positions.SelectMany(position => position.Mandates);
            var liquidityMandate = mandates.FirstOrDefault(mandate => mandate.Name == "Liquidity");
            var otherMandates = mandates.Where(mandate => mandate.Name != "Liquidity");

            //assert

            var liquidityAllocationData = fundOfMandatesData.FundsOfMandates.First(fom => fom.InstrumentCode == positions.First().Code).LiquidityAllocation;

            var expectedValue =
                Math.Round(
                    decimal.Multiply(
                        decimal.Subtract(positionValue, otherMandates.Select(mandate => mandate.Value).Sum()),
                        liquidityAllocationData),
                    MidpointRounding.AwayFromZero);

            liquidityMandate.Value
                .Should()
                .Be(expectedValue);
        }

        #endregion CalculateMandates test
    }
}
