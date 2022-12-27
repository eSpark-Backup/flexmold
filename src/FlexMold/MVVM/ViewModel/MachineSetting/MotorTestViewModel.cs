using FlexMold.Core;
using FlexMold.Utility;
using NLog.Fluent;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using TwinCAT.Ads.TypeSystem;
using TwinCAT.TypeSystem;
using System.Windows.Media;
using TwinCAT;
using System.Data.Common;
using TwinCAT.Ads;

namespace FlexMold.MVVM.ViewModel.MachineSetting
{
    internal class MotorTestViewModel : ObservableObject
    {
        public Logger log;
        public List<Symbol> MototDirectionSymbols { get; set; }
        private Symbol _SelectedMototDirectionSymbol;
        public Symbol SelectedMototDirectionSymbol
        {
            get { return _SelectedMototDirectionSymbol; }
            set
            {
                _SelectedMototDirectionSymbol = value;
                justread = true;
                SelectedMototDirectionValue = Convert.ToBoolean(SelectedMototDirectionSymbol.ReadValue());
                log.Log(LogLevel.Info, $"Get Value : {SelectedMototDirectionValue} From Motor : {SelectedMototDirectionSymbol.InstancePath}");
                justread = false;
            }
        }
        public bool justread { get; set; }
        private bool _SelectedMototDirectionValue;
        public bool SelectedMototDirectionValue
        {
            get { return _SelectedMototDirectionValue; }
            set
            {
                _SelectedMototDirectionValue = value;
                if (!justread)
                {
                    WriteValueToTwicat(SelectedMototDirectionSymbol, value);
                }
                OnPropertyChanged();
            }
        }
        public List<Symbol> MototSpeedSymbols { get; set; }
        private Symbol _SelectedMototSpeedSymbol;
        public Symbol SelectedMototSpeedSymbol
        {
            get { return _SelectedMototSpeedSymbol; }
            set
            {
                _SelectedMototSpeedSymbol = value;
                justread = true;
                SelectedMototSpeedValue = Convert.ToUInt16(SelectedMototSpeedSymbol.ReadValue());
                log.Log(LogLevel.Info, $"Get Value : {SelectedMototSpeedValue} From Motor : {SelectedMototSpeedSymbol.InstancePath}");
                justread = false;
            }
        }
        private UInt32 _SelectedMototSpeedValue;
        public UInt32 SelectedMototSpeedValue
        {
            get { return _SelectedMototSpeedValue; }
            set
            {
                _SelectedMototSpeedValue = value;
                if (!justread)
                {
                    WriteValueToTwicat(SelectedMototSpeedSymbol, value);
                }
                OnPropertyChanged();
            }
        }
        void WriteValueToTwicat(Symbol _symbol, dynamic _myValue)
        {
            _symbol.WriteValue(_myValue);
            log.Log(LogLevel.Info, $"Set Value : {_myValue} To Motor : {_symbol.InstancePath}");
        }
        public MotorTestViewModel()
        {
            log = LogManager.GetLogger("Motor Test");

            MototDirectionSymbols = new List<Symbol>();
            MototSpeedSymbols = new List<Symbol>();
            MototDirectionSymbols = TwinCathelper.MySymbols.Where(a => a.InstancePath.StartsWith("MAIN.dirM")).ToList();
            MototSpeedSymbols = TwinCathelper.MySymbols.Where(a => a.InstancePath.StartsWith("MAIN.speedM")).ToList();
            Console.WriteLine();
        }
    }
}
