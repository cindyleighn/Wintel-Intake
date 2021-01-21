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

    /// <summary>
    /// Mandate calculator class which helps in calculating mandates
    /// </summary>
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
        /// Reads provided file to build fundsOfMandatesData
        /// </summary>
        /// <param name="filename">xml file from where we can read the fundsOfMandatesData</param>
        /// <returns>FundOfMandatesData read from a file.</returns>
        public FundsOfMandatesData GetFundOfMandates(string filename)
        {
            _fileValidators.ForEach(validator =>
            {
                if (!validator.validate(filename))
                    throw new ArgumentException(validator.message);
            });

            try
            {
                using var stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                var serializer = new XmlSerializer(typeof(FundsOfMandatesData));
                return (FundsOfMandatesData)serializer.Deserialize(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"failed to get mandates data, failed with {ex.Message}");
                //logging or custom exception
                throw;
            }
        }

        #region Mandate calculation for portfolio

        /// <summary>
        /// Calculates mandates for positions in Portfolio based on the mandates and positions.
        /// </summary>
        /// <param name="portfolio">Portfolio object which contains positions collection.</param>
        /// <param name="fundOfMandatesData">fundOfMandatesData which contains mandates which will be used for calculating mandates for positions in portfolio.</param>
        /// <returns>Portfolio object where the positions collection contains calculated mandates in them.</returns>
        public Portfolio CalculateMandates(Portfolio portfolio, FundsOfMandatesData fundOfMandatesData)
        {
            ValidateMandateInput(portfolio, fundOfMandatesData);

            //create dictionary where the key is Instrument code, based on this we can check position code 
            var fundsOfMandatesDictionary = fundOfMandatesData
                                                    .FundsOfMandates
                                                    .ToDictionary(fundOfMandate => fundOfMandate.InstrumentCode);

            foreach (var position in portfolio.Positions)
            {
                //if fund of mandates instrument code matches position code then proceed to add mandates on position
                if (fundsOfMandatesDictionary.TryGetValue(position.Code, out FundOfMandates fund))
                {
                    //keep adding mandate values, if we have to create liquidity mandate we will use it
                    decimal sumOfMandateValues = 0;
                    position.Mandates = new List<Model.Mandate>();

                    foreach (var mandate in fund.Mandates)
                    {
                        var calculatedMandate = CalculateMandate(position.Value, mandate.Allocation, mandate.MandateName);
                        position.Mandates.Add(calculatedMandate);

                        sumOfMandateValues += calculatedMandate.Value;
                    }
                    //check if we should create liquidity mandate. If yes then add it to the mandates' collection
                    if (ShouldCreateLiquidityMandate(fund.LiquidityAllocation))
                    {
                        position.Mandates.Add(CreateLiquidityMandate(position.Value, fund.LiquidityAllocation, sumOfMandateValues));
                    }
                }
            }
            return portfolio;
        }

        private static void ValidateMandateInput(Portfolio portfolio, FundsOfMandatesData fundOfMandatesData)
        {
            if (portfolio == null)
                throw new ArgumentException(string.Format(ErrorMessages.RequiredForPortfolio, nameof(portfolio)));

            if (fundOfMandatesData == null)
                throw new ArgumentException(string.Format(ErrorMessages.RequiredForPortfolio, nameof(fundOfMandatesData)));
        }

        /// <summary>
        /// Method to calculate and build mandate object
        /// </summary>
        /// <param name="positionValue">current position value</param>
        /// <param name="mandateAllocation">allocation on mandate</param>
        /// <param name="name">name of the mandate</param>
        /// <returns>Calculated mandate VM</returns>
        private static Model.Mandate CalculateMandate(decimal positionValue, decimal mandateAllocation, string name)
        {
            decimal allocation = mandateAllocation / 100;
            decimal value = Math.Round(decimal.Multiply(positionValue, mandateAllocation) / 100, MidpointRounding.AwayFromZero);
            return CreateMandate(allocation, value, name);
        }

        /// <summary>
        /// Method to create liquidity mandate object
        /// </summary>
        /// <param name="positionValue">current position value</param>
        /// <param name="liquidityAllocation">liquidity allocation on current fund of mandate</param>
        /// <param name="sumOfMandateValues">Sum of all mandate values under current fund of mandate</param>
        /// <returns></returns>
        private static Model.Mandate CreateLiquidityMandate(decimal positionValue, decimal liquidityAllocation, decimal sumOfMandateValues)
        {
            decimal allocation = liquidityAllocation / 100;
            decimal value = Math.Round(decimal.Multiply(decimal.Subtract(positionValue, sumOfMandateValues), liquidityAllocation), MidpointRounding.AwayFromZero);
            return CreateMandate(allocation, value, "Liquidity");
        }

        private static Model.Mandate CreateMandate(decimal allocation, decimal value, string name) =>
            new Model.Mandate
            {
                Name = name,
                Value = value,
                Allocation = allocation
            };

        /// <summary>
        /// Checks whether Liquidity mandate should be created or not
        /// </summary>
        /// <param name="liquidityAllocation">Liquidity alloocation of fund of mandates objet</param>
        /// <returns>True if the liquidityAllocation is greater than 0</returns>
        private static bool ShouldCreateLiquidityMandate(decimal liquidityAllocation) => liquidityAllocation > 0;

        #endregion Mandate calculation for portfolio

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
