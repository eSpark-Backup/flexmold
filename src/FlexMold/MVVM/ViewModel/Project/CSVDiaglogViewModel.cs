using FlexMold.Core;

namespace FlexMold.MVVM.ViewModel.Project
{
    public class CSVDiaglogViewModel : ObservableObject, ICSVDiaglogViewModel
    {
        private int _MotorDirPValue;

        public int MotorDirPValue
        {
            get { return _MotorDirPValue; }
            set
            {
                _MotorDirPValue = value;
                OnPropertyChanged();
            }
        }
        private float _MotorRampDelayValue;

        public float MotorRampDelayValue
        {
            get { return _MotorRampDelayValue; }
            set
            {
                _MotorRampDelayValue = value;
                OnPropertyChanged();
            }
        }
        private float _MotorTime;
        public float MotorTime
        {
            get { return _MotorTime; }
            set
            {
                _MotorTime = value;
                OnPropertyChanged();
            }
        }
    }
}
