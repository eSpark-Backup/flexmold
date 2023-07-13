using FlexMold.Core;
using FlexMold.MVVM.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FlexMold.MVVM.ViewModel.Home
{
    public interface IMotorStatusViewmodel
    {
        bool CalibrateButtonEnable { get; set; }
        RelayCommand CalibrateCommand { get; set; }
        IList<double> CalibrationData { get; set; }
        RelayCommand DebugDetailClickCommand { get; set; }
        uint[] DeviceStatus { get; set; }
        uint[] MotorDiagnistics { get; set; }
        float[] mPosition { get; set; }
        ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        RelayCommand OnLoadedCommand { get; set; }
        RelayCommand OnUnLoadedCommand { get; set; }

        void Calibrate();
    }
}