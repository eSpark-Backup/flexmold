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
                services.AddSingleton<ISplashScreenViewModel, SplashScreenViewModel>();
                services.AddSingleton<SplashMachineSlection>();
                services.AddSingleton<Machine>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<FoeView>();
                services.AddSingleton<FlexMoldMachine1>();
                services.AddSingleton<TcDeviceScanning>();
                services.AddSingleton<IFoeViewModel, FoeViewModel>();
                services.AddSingleton<ITwinCatConfigViewModel, TwinCatConfigViewModel>();
                services.AddSingleton<ITwinCathelper,TwinCathelper>();
            }
            ).Build();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var startupScreen = AppHost.Services.GetRequiredService<SplashMachineSlection>();
            startupScreen.DataContext = AppHost.Services.GetRequiredService<ISplashScreenViewModel>();
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