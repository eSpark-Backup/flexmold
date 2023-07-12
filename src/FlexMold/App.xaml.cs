using FlexMold.Extenstions;
using FlexMold.MVVM.Model;
using FlexMold.MVVM.View.SplashScreen;
using FlexMold.MVVM.ViewModel;
using FlexMold.MVVM.ViewModel.SplashScreen;
using FlexMold.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }
        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<SplashScreenViewModel>();
                services.AddSingleton<SplashMachineSlection>();
                services.AddScoped<Machine>();
                services.AddScoped<MainWindow>();
                services.AddScoped<FoeView>();
                services.AddScoped<FoeViewModel>();
                services.AddScoped<TwinCatConfigViewModel>();
            }
            ).Build();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupScreen = AppHost.Services.GetRequiredService<SplashMachineSlection>();
            startupScreen.DataContext = AppHost.Services.GetRequiredService<SplashScreenViewModel>();
            startupScreen.Show();

            base.OnStartup(e);
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            base.OnExit(e);
        }
    }
}