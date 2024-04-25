using FlexMold.MVVM.View;
using FlexMold.MVVM.View.Home;
using FlexMold.MVVM.View.MachineTest;
using FlexMold.MVVM.View.Project;
using FlexMold.MVVM.View.Setting;
using FlexMold.MVVM.View.SplashScreen;
using FlexMold.MVVM.ViewModel;
using FlexMold.MVVM.ViewModel.Home;
using FlexMold.MVVM.ViewModel.MachineTest;
using FlexMold.MVVM.ViewModel.Project;
using FlexMold.MVVM.ViewModel.Setting;
using FlexMold.MVVM.ViewModel.SplashScreen;
using FlexMold.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

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
            try { 
            AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ISplashScreenViewModel, SplashScreenViewModel>();
                services.AddSingleton<ITwinCathelper,TwinCathelper>();
                services.AddSingleton<ITwinCatConfigViewModel, TwinCatConfigViewModel>();
                services.AddSingleton<IFoeViewModel, FoeViewModel>();

                services.AddSingleton<SplashMachineSlection>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<FoeView>();

                services.AddSingleton<FlexMoldMachine1>();
                services.AddSingleton<TcDeviceScanning>();

                //services.AddSingleton<Machine>();

                services.AddSingleton<IHomeViewModel, HomeViewModel>();                
                services.AddSingleton<IMachineDetailViewModel, MachineDetailViewModel>();
                services.AddSingleton<IMotorStatusViewmodel, MotorStatusViewmodel>();
                services.AddSingleton<IMachineSettingViewModel, MachineSettingViewModel>();
                services.AddSingleton<MachineDetailView>();
                services.AddSingleton<MotorStatusView>();
                services.AddSingleton<VisualDisplayView>();
                services.AddSingleton<VaccumStatusView>();
                services.AddSingleton<HeaterStatusView>();
                services.AddSingleton<PowerStatusView>();
                services.AddSingleton<EmergencyStatusView>();
                services.AddSingleton<LaserStatusView>();

                services.AddSingleton<MotorTest>();
                services.AddSingleton<ReadMotorView>();
                services.AddSingleton<IMachineTestViewModel, MachineTestViewModel>();
                services.AddSingleton<IMotorTestViewModel, MotorTestViewModel>();
                services.AddSingleton<IReadMotorViewModel, ReadMotorViewModel>();
                services.AddSingleton<PowerOnOffView>();
                services.AddSingleton<RenumberingView>();
                services.AddSingleton<PositionView>();


                services.AddSingleton<ISettingViewModel, SettingViewModel>();
                services.AddSingleton<IMotorSettingViewModel, MotorSettingViewModel>(); 
                services.AddSingleton<MachineSettingView>();
                services.AddSingleton<PowerSettingView>();
                services.AddSingleton<MotorSettingView>();
                services.AddSingleton<VacuumSettingView>();
                services.AddSingleton<LaserProjectorSettingView>();

                services.AddSingleton<IResetViewModel, ResetViewModel>();
                services.AddSingleton<ResetView>();

                services.AddSingleton<IProjectViewModel, ProjectViewModel>();
                services.AddSingleton<ICSVDiaglogViewModel, CSVDiaglogViewModel>();
                services.AddSingleton<ILaserProjectorViewModel, LaserProjectorViewModel>();

                services.AddSingleton<CSVDiaglogView>();
                services.AddSingleton<IPriveledgeUpViewModel, PriveledgeUpViewModel>();
                services.AddSingleton<PriveledgeUpView>();
                services.AddSingleton<SambaConfigWindow>();
                services.AddSingleton<SerialPortConfigView>();
            }
            ).Build();
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Main");
                log.Log(LogLevel.Error, ex.Message);
            }
        }

        public static bool AnotherInstanceExists()
        {
            Process currentRunningProcess = Process.GetCurrentProcess();
            Process[] listOfProcs = Process.GetProcessesByName(currentRunningProcess.ProcessName);
            foreach (Process proc in listOfProcs)
            {
                if ((proc.MainModule.FileName == currentRunningProcess.MainModule.FileName) && (proc.Id != currentRunningProcess.Id))
                    return true;
            }
            return false;
        }

        protected override async void OnStartup(StartupEventArgs e)
        {

            if (AnotherInstanceExists())
            {
                //MessageBox.Show("You Can Not Run More Than One Instance Of This Application.", "Application already running!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                this.OnExit(null);
                return;
            }

            await AppHost!.StartAsync();

            var vCulture = System.Globalization.CultureInfo.CurrentCulture;

            //if (System.Globalization.CultureInfo.CurrentCulture.Name.Contains("en-DK"))
            //{
            //    vCulture.NumberFormat.NumberDecimalSeparator = ",";
            //    vCulture.NumberFormat.CurrencyDecimalSeparator = ",";
            //}
            //else
            //{
            //    vCulture.NumberFormat.NumberDecimalSeparator = ".";
            //    vCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            //}

            Thread.CurrentThread.CurrentCulture = vCulture;
            Thread.CurrentThread.CurrentUICulture = vCulture;
            CultureInfo.DefaultThreadCurrentCulture = vCulture;
            CultureInfo.DefaultThreadCurrentUICulture = vCulture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            var startupScreen = AppHost.Services.GetRequiredService<SplashMachineSlection>();
            startupScreen.Show();


            base.OnStartup(e);
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                await AppHost!.StopAsync();
                base.OnExit(e);
            }
            catch (Exception ex)
            {
                var log = LogManager.GetLogger("Main");
                log.Log(LogLevel.Error, ex.Message);
            }
        }
    }
}
