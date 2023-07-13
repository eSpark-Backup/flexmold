using FlexMold.Core;
using FlexMold.MVVM.Model;
using System.Collections.Generic;
using TwinCAT.Ads.TypeSystem;

namespace FlexMold.MVVM.ViewModel.MachineTest
{
    public interface IMotorTestViewModel
    {
        byte[] buffer { get; set; }
        uint[] DeviceIds { get; set; }
        uint DeviceVariableHandle { get; set; }
        IEnumerable<ETMode> EnumModes { get; }
        bool justread { get; set; }
        EthercatDevice[] MyPLCMotor { get; set; }
        RelayCommand OnLoadedCommand { get; set; }
        RelayCommand OnUnLoadedCommand { get; set; }
        double Position { get; set; }
        Symbol PositionSymbol { get; set; }
        int SelectedMotorDirValue { get; set; }
        double SelectedMotorDisplacementValue { get; set; }
        uint SelectedMotorIndex { get; set; }
        ETMode SelectedMotorModeValue { get; set; }
        double SelectedMotorRampDelayValue { get; set; }
        double SelectedMotorRPM { get; set; }
        double SelectedMotorTimeValue { get; set; }
        RelayCommand SendAllCommand { get; set; }
        RelayCommand SendDisplacementCommand { get; set; }
        RelayCommand SendRampCommand { get; set; }
        RelayCommand SendRPMCommand { get; set; }
        RelayCommand SendTimeCommand { get; set; }
        int structSize { get; set; }
        Symbol SymbolEthercatDevice { get; }

        void OnLoaded();
    }
}