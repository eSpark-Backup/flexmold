using FlexMold.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel.Home
{
    public class StatusBaseViewModel : ObservableObject
    {

        private bool _HomeVacuumStatus;
        public bool HomeVacuumStatus
        {
            get { return _HomeVacuumStatus; }
            set
            {
                _HomeVacuumStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _HomeMachineStatus;
        public bool HomeMachineStatus
        {
            get { return _HomeMachineStatus; }
            set
            {
                _HomeMachineStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _HomeMotorStatus;
        public bool HomeMotorStatus
        {
            get { return _HomeMotorStatus; }
            set
            {
                _HomeMotorStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _HomeHeaterStatus;
        public bool HomeHeaterStatus
        {
            get { return _HomeHeaterStatus; }
            set
            {
                _HomeHeaterStatus = value;
                OnPropertyChanged();
            }
        }


        private bool _HomePowerStatus;
        public bool HomePowerStatus
        {
            get { return _HomePowerStatus; }
            set
            {
                _HomePowerStatus = value;
                OnPropertyChanged();
            }
        }

        private bool _HomeEmergencyStatus;
        public bool HomeEmergencyStatus
        {
            get { return _HomeEmergencyStatus; }
            set
            {
                _HomeEmergencyStatus = value;
                OnPropertyChanged();
            }
        }
        
        private bool _HomeLaserStatus;
        public bool HomeLaserStatus
        {
            get { return _HomeLaserStatus; }
            set
            {
                _HomeLaserStatus = value;
                OnPropertyChanged();
            }
        }

        public int[] GetMismatchedIndexes<T>(T[] array1, T[] array2)
        {
            var mismatchedIndexes = new List<int>();

            if (array1 == null)
                return Enumerable.Range(0, array2.Length).ToArray();
            
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException("Arrays must have the same length.");
            }
            Parallel.For(0, array1.Length, i =>
            {
                if (!array1[i].Equals(array2[i]))
                {
                    mismatchedIndexes.Add(i);
                }
            });

            return mismatchedIndexes.ToArray();
        }
    }
}
