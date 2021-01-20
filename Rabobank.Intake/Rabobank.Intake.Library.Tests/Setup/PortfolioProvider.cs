namespace Rabobank.Intake.Library.Tests.Setup
{
    using System.Collections.Generic;
    using Rabobank.Intake.Library.Model;

    public class PortfolioProvider
    {
        public static Portfolio NotMatchingPositionCode => new Portfolio()
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

        public static Portfolio MatchingPositionCode => new Portfolio()
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

        public static Portfolio MatchingPositionForLiquidityMandate => new Portfolio()
        {
            Positions = new List<Position>()
                {
                    new Position()
                    {
                        Code = "NL0000440584",
                        Value = 1212,
                        Name = "Test fund of mandates",
                    }
                }
        };

        public static Portfolio MatchingPositionNoLiquidityMandate => new Portfolio()
        {
            Positions = new List<Position>()
                {
                    new Position()
                    {
                        Code = "NL0000440555",
                        Value = 1212,
                        Name = "Test fund of mandates",
                    }
                }
        };
    }
}
