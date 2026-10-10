using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CentralBankRate.Core;
using CentralBankRate.Core.Configurations;
using CentralBankRate.Core.Services.Api;
using CentralBankRate.Core.Services.Currency;
using CentralBankRate.Core.Services.Parser;
using CentralBankRate.Core.Services.Report;
using CentralBankRate.Core.Services.Sender;
using CentralBankRate.UI.ViewModels;
using CentralBankRate.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CentralBankRate.UI;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var builder = Host.CreateApplicationBuilder();
        ConfigureServices(builder.Services);
        _host = builder.Build();
        _host.Start();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _host.Services.GetRequiredService<MainWindow>();
            desktop.Exit += DesktopOnExit;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var config = ConfigLoad.Load();
        // Core
        services.AddSingleton(_ => new HttpClient());
        services.AddSingleton<IApiService, ApiService>();
        services.AddSingleton<IParser, XmlParser>();
        services.AddSingleton(config.Value ?? new SmtpSettings());
        services.AddSingleton<ISenderService, EmailSenderService>();
        services.AddSingleton<IReportService, PdfReportService>();
        services.AddSingleton<ICurrencyReportService, CurrencyReportService>();

        // UI
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
    }

    private void DesktopOnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_host is null)
        {
            return;
        }

        using (_host)
        {
            _host.StopAsync();
        }
    }
}
