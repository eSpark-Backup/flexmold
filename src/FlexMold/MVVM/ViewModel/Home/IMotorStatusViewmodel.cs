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
        RelayCommand Calibrate02Command { get; set; }
        IList<float> CalibrationData { get; set; }
        RelayCommand DebugDetailClickCommand { get; set; }
        uint[] DeviceStatus { get; set; }
        bool IsMotorsIdle { get; set; }
        uint[] MotorDiagnistics { get; set; }
        float[] mPosition { get; set; }
        ObservableCollection<MotorStatusControl> MyMotors { get; set; }
        RelayCommand OnLoadedCommand { get; set; }
        RelayCommand OnUnLoadedCommand { get; set; }

        void Calibrate();
    }
}