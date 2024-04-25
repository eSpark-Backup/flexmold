using FlexMold.Core;
using FlexMold.MVVM.View.Setting;
using FlexMold.Utility;
using NLog;
using System.Collections.Generic;

namespace FlexMold.MVVM.ViewModel
{
    public class PriveledgeUpViewModel : ObservableObject, IPriveledgeUpViewModel
    {
        /// <summary>
        /// Relay Commands
        /// </summary>
        //public RelayCommand btnPrivDownCommand { get; set; }
        private Logger log = LogManager.GetLogger("MachineSettings");

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public PriveledgeUpViewModel()
        {
            //CurrentView = btnPrivDownCommand;

            //btnPrivDownCommand = new RelayCommand(o => { GetPasswordUpdate(); });


        }

        private void GetPasswordUpdate()
        {
            //if (FlexMoldDB.checkAdminPriv(_PriveledgeUpView.LP_ZLaserIPSelect.Text))
            //{
            //    MessageBox.Show("Now you are in Admin mode \r\n Be Carefull when Updating values as it may effect on performance!", "Priveledge Mode Enabled!", MessageBoxButtons.OK);
            //    FlexMoldDB.IsEditingEnabled = true;
            //}
            //MessageBox.Show("Please try again!", "Incorrect Password", MessageBoxButtons.OK);


            //if (FlexMoldDB.IsEditingEnabled)
            //{
            //    UpdateSettingsDB();

            //    //MessageBox.Show("Now you are in Admin mode \r\n Be Carefull when Updating values as it may effect on performance!", "Priveledge Mode Enabled!", MessageBoxButtons.OK);
            //    //FlexMoldDB.IsEditingEnabled = true;
            //}
        }

    }
}