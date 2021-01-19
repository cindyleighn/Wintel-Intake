namespace Rabobank.Intake.App
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Rabobank.Intake.Library;
    using Rabobank.Intake.Library.Interfaces;

    class Program
    {
        static void Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            using IServiceScope serviceScope = host.Services.CreateScope();

            var provider = serviceScope.ServiceProvider;
            var service = provider.GetRequiredService<PortfolioService>();

            service.GetPortfolioWithMandates()
                .Print();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services.AddScoped<IFundOfMandateCalculator, FundOfMandateCalculator>()
                            .AddTransient<PortfolioService>());
    }
}
