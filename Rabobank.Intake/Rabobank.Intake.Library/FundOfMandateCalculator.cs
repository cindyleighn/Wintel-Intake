namespace Rabobank.Intake.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Rabobank.Intake.Library.Interfaces;
    using Rabobank.Intake.Library.Model;

    public class FundOfMandateCalculator : IFundOfMandateCalculator
    {
        public FundsOfMandatesData GetFundOfMandates(string fileName)
        {
            if (fileName == null)
            {
                return null;
            }

            List<FundOfMandates> listFundOfMandates = new List<FundOfMandates>();

            XmlSerializer serializer = new XmlSerializer(typeof(FundsOfMandatesData));
            using (TextReader reader = new StringReader(File.ReadAllText(fileName)))
            {
                FundsOfMandatesData result = (FundsOfMandatesData)serializer.Deserialize(reader);

                foreach (var item in result.FundsOfMandates)
                {
                    listFundOfMandates.Add(item);
                }
            }

            return new FundsOfMandatesData { FundsOfMandates = listFundOfMandates.ToArray() };
        }

        public Portfolio CalculateMandates(Portfolio portfolio, FundsOfMandatesData fundOfMandatesData)
        {
            if (portfolio == null || fundOfMandatesData == null)
            {
                return null;
            }

            foreach (var p in portfolio.Positions)
            {
                foreach (var fund in fundOfMandatesData.FundsOfMandates)
                {
                    if (p.Code == fund.InstrumentCode)
                    {
                        p.Mandates = new List<Model.Mandate>();
                        for (int i = 0; i < fund.Mandates.Count(); i++)
                        {
                            p.Mandates.Add(new Model.Mandate
                            {
                                Name = fund.Mandates[i].MandateName,
                                Allocation = fund.Mandates[i].Allocation / 100,
                                Value = Math.Round(decimal.Multiply(p.Value, fund.Mandates[i].Allocation) / 100, MidpointRounding.AwayFromZero),
                            });
                        }
                        if (fund.LiquidityAllocation > 0)
                        {
                            decimal total = p.Mandates.Sum(x => x.Value);
                            p.Mandates.Add(new Model.Mandate
                            {
                                Name = "Liquidity",
                                Allocation = fund.LiquidityAllocation / 100,
                                Value = Math.Round(decimal.Multiply(decimal.Subtract(p.Value, total), fund.LiquidityAllocation), MidpointRounding.AwayFromZero),
                            });
                        }
                    }
                }
            }

            return portfolio;
        }
    }
}
