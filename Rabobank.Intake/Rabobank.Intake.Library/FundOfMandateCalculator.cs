namespace Rabobank.Intake.Library
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using Rabobank.Intake.Library.Interfaces;
    using Rabobank.Intake.Library.Model;
    using Rabobank.Intake.Library.Resources;

    public class FundOfMandateCalculator : IFundOfMandateCalculator
    {
        /// <summary>
        /// List of validators to validate file
        /// </summary>
        private static readonly List<(Func<string, bool> validate, string message)> _fileValidators;

        static FundOfMandateCalculator()
        {
            _fileValidators = new List<(Func<string, bool> validate, string message)>
            {
                (FilePathProvided, ErrorMessages.FilePathNotProvided),
                (FileExists, ErrorMessages.FileDoesNotExist),
                (IsXmlFile, ErrorMessages.NotXmlFile)
            };
        }

        /// <summary>
        /// Reads provided file to build fundOfMandatesData
        /// </summary>
        /// <param name="mandatesFileName">xml file from where we can read the fundOfMandatesData</param>
        /// <returns></returns>
        public FundsOfMandatesData GetFundOfMandates(string mandatesFileName)
        {
            _fileValidators.ForEach(validator =>
            {
                if (!validator.validate(mandatesFileName))
                    throw new ArgumentException(validator.message);
            });

            using var stream = new FileStream(mandatesFileName, FileMode.Open, FileAccess.Read);
            var serializer = new XmlSerializer(typeof(FundsOfMandatesData));
            return (FundsOfMandatesData)serializer.Deserialize(stream);
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

        #region mandate file validation

        /// <summary>
        /// validate that the file path is provided 
        /// </summary>
        /// <param name="filePath">file path from where the mandates can be read</param>
        /// <returns>True if file path is provided</returns>
        private static bool FilePathProvided(string filePath) => !string.IsNullOrEmpty(filePath);

        /// <summary>
        /// validates that the file exists at the provided location
        /// </summary>
        /// <param name="filePath">file path from where the mandates can be read</param>
        /// <returns>True if the file exists at provided location</returns>
        private static bool FileExists(string filePath) => FilePathProvided(filePath) && File.Exists(filePath);

        /// <summary>
        /// Validates that the provided file is an xml file
        /// </summary>
        /// <param name="filePath">file path from where the mandates can be read</param>
        /// <returns>True if file is xml file</returns>
        private static bool IsXmlFile(string filePath) => FilePathProvided(filePath) && Path.GetExtension(filePath).Equals(".xml", StringComparison.OrdinalIgnoreCase);

        #endregion mandate file validation
    }
}
