using FlexMold.MVVM.ViewModel.Project;
using FlexMold.Utility;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FlexMold.MVVM.View.Home
{
    /// <summary>
    /// Interaction logic for VisualDisplayView.xaml
    /// </summary>
    public partial class VisualDisplayView : UserControl
    {
        DispatcherTimer statusCheckTimer = new DispatcherTimer();

        public VisualDisplayView()
        {
            InitializeComponent();

            statusCheckTimer.Tick += new EventHandler(Timer_Elapsed);
            statusCheckTimer.Interval = new TimeSpan(0, 0, 5);
            statusCheckTimer.Start();

        }

        private void Timer_Elapsed(object sender, EventArgs e)
        {
            TSWHomeVacuumStatus.IsOn = false;
            try { if (CSVHelper.MachineEnabled) TSWHomeMachineStatus.IsOn = true; else TSWHomeMachineStatus.IsOn = false; } catch { }
            try { if (CSVHelper.MotorsStatusGlobal[0] == "Healthy") TSWHomeMotorStatus.IsOn = true; else TSWHomeMotorStatus.IsOn = false; } catch { }            
            TSWHomeHeaterStatus.IsOn = false;
            try { if (CSVHelper.MachineEnabled) TSWHomePowerStatus.IsOn = true; else TSWHomePowerStatus.IsOn = false; } catch { }            
            try { if (CSVHelper.LaserEnabled) TSWHomeLaserStatus.IsOn = true; else TSWHomeLaserStatus.IsOn = false; } catch { }
            try { if (ProjectViewModel.UserStop) TSWHomeEmergencyStatus.IsOn = true; else TSWHomeEmergencyStatus.IsOn = false; } catch { }
            
        }
    }
}