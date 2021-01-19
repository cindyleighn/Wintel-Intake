namespace Rabobank.Intake.Library.Interfaces
{
    using Rabobank.Intake.Library.Model;

    public interface IFundOfMandateCalculator
    {
        FundsOfMandatesData GetFundOfMandates(string fileName);

        Portfolio CalculateMandates(Portfolio portfolio, FundsOfMandatesData fundOfMandatesData);
    }
}
