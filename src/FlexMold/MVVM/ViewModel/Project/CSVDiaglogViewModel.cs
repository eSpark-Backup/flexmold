using FlexMold.Core;
using System;

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
        private float maxDisplacement;
        private float recommendedRPM;
        private float maxRPM;

        private Boolean _btnEnbl;

        public Boolean BtnEnbl
        {
            get { return _btnEnbl; }
            set
            {
                _btnEnbl = value;
                OnPropertyChanged();
            }
        }
        private string _message;
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }
        public float MaxDisplacement
        {
            get => maxDisplacement;
            set
            {
                maxDisplacement = value;
                OnPropertyChanged();
            }
        }

        public float MotorTime
        {
            get { return _MotorTime; }
            set
            {
                if (value != _MotorTime)
                {
                    _MotorTime = value;
                    //{{d / 2}Max/ total time of operation}*60
                    float temp = (float)(((maxDisplacement / 2.0) / value) * 60.0);
                    if (temp != RecommendedRPM)
                    {
                        RecommendedRPM = temp;
                    }
                    if (RecommendedRPM <= maxRPM)
                    {
                        BtnEnbl = true;
                        Message = "";
                    }
                    else
                    {
                        Message = "Value cannot exceed more than Maximum RPM";
                        BtnEnbl = false;
                    }
                }
                OnPropertyChanged();
            }
        }
        public float RecommendedRPM
        {
            get { return recommendedRPM; }
            set
            {
                float time = (maxDisplacement * 60) / (value * 2);
                if (value != recommendedRPM)
                {
                    recommendedRPM = value;
                    //total time = (d * 60) / (RPM * 2)
                    BtnEnbl = true;
                    Message = "";
                    if (time != _MotorTime)
                    {
                        //total time = (d * 60) / (RPM * 2)
                        MotorTime = time;
                    }
                    if (RecommendedRPM <= maxRPM)
                    {
                        BtnEnbl = true;
                        Message = "";
                    }
                    else
                    {
                        Message = "Value cannot exceed more than Maximum RPM";
                        BtnEnbl = false;
                    }
                }
                OnPropertyChanged();
            }
        }
        public float MaxRPM
        {
            get => maxRPM;
            set
            {
                maxRPM = value;
                OnPropertyChanged();
            }
        }
    }
}
