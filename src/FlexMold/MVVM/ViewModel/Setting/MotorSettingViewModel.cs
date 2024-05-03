using FlexMold.Core;
using FlexMold.Utility;
using MaterialDesignThemes.Wpf;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace FlexMold.MVVM.ViewModel.Setting
{
    public class MotorSettingViewModel : ObservableObject, IMotorSettingViewModel
    {
        private Logger log;
        public RelayCommand btnSearchMotors { get; set; }

        private uint _SelectedMotorIndex;
        private bool _IsDialogOpen;

        public bool IsDialogOpen
        {
            get { return _IsDialogOpen; }
            set
            {
                _IsDialogOpen = value;
                OnPropertyChanged();
            }
        }

        private bool _txtListMotors;

        public bool txtListMotors
        {
            get { return _txtListMotors; }
            set
            {
                _txtListMotors = value;
                OnPropertyChanged();
            }
        }

        private string _txtMaxRPM;
        public string txtMaxRPM
        {
            get { return _txtMaxRPM; }
            set
            {
                _txtMaxRPM = value;
                OnPropertyChanged();
            }
        }

        private string _txtRecRPM;
        public string txtRecRPM
        {
            get { return _txtRecRPM; }
            set
            {
                _txtRecRPM = value;
                OnPropertyChanged();
            }
        }

        private bool _RampUpDown;
        public bool RampUpDown
        {
            get { return _RampUpDown; }
            set
            {
                _RampUpDown = value;
                OnPropertyChanged();
            }
        }

        //private string _txtrpmAssume;
        //public string txtrpmAssume
        //{
        //    get { return _txtrpmAssume; }
        //    set
        //    {
        //        _txtrpmAssume = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _txtmotorDirVal;
        //public string txtmotorDirVal
        //{
        //    get { return _txtmotorDirVal; }
        //    set
        //    {
        //        _txtmotorDirVal = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private string _txtcalibrSeq02;
        //public string txtcalibrSeq02
        //{
        //    get { return _txtcalibrSeq02; }
        //    set
        //    {
        //        _txtcalibrSeq02 = value;
        //        OnPropertyChanged();
        //    }
        //}

        private int _txtdefaultTimeSec;
        public int txtdefaultTimeSec
        {
            get { return _txtdefaultTimeSec; }
            set
            {
                _txtdefaultTimeSec = value;
                OnPropertyChanged();
            }
        }

        //private int _txtdefaultTimeSec;
        //public int txtdefaultTimeSec
        //{
        //    get { return _txtdefaultTimeSec; }
        //    set
        //    {
        //        _txtdefaultTimeSec = value;
        //        OnPropertyChanged();
        //    }
        //}


        private string _txtmaxMotorMove;
        public string txtmaxMotorMove
        {
            get { return _txtmaxMotorMove; }
            set
            {
                _txtmaxMotorMove = value;
                OnPropertyChanged();
            }
        }

        private bool _editingEnable;
        public bool editingEnable
        {
            get { return _editingEnable; }
            set { _editingEnable = value; OnPropertyChanged(); }
        }
        //private bool _SearchMotorFrom;
        //public bool SearchMotorFrom
        //{
        //    get { return _SearchMotorFrom; }
        //    set
        //    {
        //        _SearchMotorFrom = value;
        //        OnPropertyChanged();
        //    }
        //}

        //private bool _SearchMotorTo;
        //public bool SearchMotorTo
        //{
        //    get { return _SearchMotorTo; }
        //    set
        //    {
        //        _SearchMotorTo = value;
        //        OnPropertyChanged();
        //    }
        //}

        public RelayCommand UpdateMotorSettingCommand { get; set; }
        public uint SelectedMotorIndex
        {
            get { return _SelectedMotorIndex; }
            set
            {
                _SelectedMotorIndex = value;
                OnPropertyChanged();
            }
        }

        private uint[] _DeviceIds;
        private readonly ITwinCathelper _TwinCathelper;

        public uint[] DeviceIds
        {
            get { return _DeviceIds; }
            set
            {
                _DeviceIds = value;
                OnPropertyChanged();
            }
        }
        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            Console.WriteLine();
        }


        public MotorSettingViewModel()
        {
            CSVHelper.offsetRes_MotorSettingViewModel = this;
            RampUpDown = true;
        }

    public MotorSettingViewModel(ITwinCathelper twinCathelper)
        {
            this._TwinCathelper = twinCathelper;

            if (_TwinCathelper.Client == null)
            {
                return;
            }

            log = LogManager.GetLogger("Motor Setting");
            DeviceIds = new uint[TwinCathelper.DeviceCount];
            for (uint i = 0; i < TwinCathelper.DeviceCount; i++)
            {
                DeviceIds[i] = i;
            }
            
            LoadMotorSettingsDB();

            UpdateMotorSettingCommand = new RelayCommand(async => { UpdateMotorSettingsDB(); });

            //btnSearchMotors = new RelayCommand(o =>
            //{
            //    try { filterMotors(); }
            //    catch (Exception ex)
            //    {
            //        log.Log(LogLevel.Error, ex.Message);
            //        TNF.ShowError(ex.Message);
            //    }

            //});        
            CSVHelper.offsetRes_MotorSettingViewModel = this;            
        }

        public void LoadMotorSettingsDB()
        {

            try
            {
                log.Log(LogLevel.Info, "Starting to search motors from ");
                //CREATE TABLE "FM_MotorSetting"(
                //    "id"    INTEGER,
                //    "MaxRPM"    INTEGER,
                //    "RecRPM"    INTEGER,
                //    "RPMAssume" INTEGER,
                //    "MotorDir"  INTEGER,
                //    "CalibrSeq02"   INTEGER,
                //    "DefaultTimeSec"    INTEGER,
                //    "MaxMotorMove"  INTEGER,
                //    "NegativeOffset"    TEXT,
                //    PRIMARY KEY("id")
                //)
                DataSet dsMachSet = FlexMoldDB.ReadData("FM_MotorSetting", null);

                if (dsMachSet != null)
                {
                    foreach (DataTable dt in dsMachSet.Tables)
                    {
                        //if (dt.TableName == "FM_MachineSetting")
                        foreach (DataRow dr in dt.Rows)
                        {
                            txtMaxRPM = dr["MaxRPM"].ToString();
                            txtRecRPM = dr["RecRPM"].ToString();
                            //txtrpmAssume = dr["RPMAssume"].ToString();
                            //txtmotorDirVal = dr["MotorDir"].ToString();
                            RampUpDown = dr["MotorDir"].ToString() == "1" ? true : false;
                            //txtcalibrSeq02 = dr["CalibrSeq02"].ToString();
                            txtdefaultTimeSec = int.Parse(dr["DefaultTimeSec"].ToString());
                            txtmaxMotorMove = dr["MaxMotorMove"].ToString();                            
                            try { CSVHelper.NegativeoffsetResume = dr["NegativeOffset"].ToString(); CSVHelper.SetNegativeoffsetResumeFlt(); } catch { CSVHelper.NegativeoffsetResume = ""; }
                            try { CSVHelper.LastPositionValues = dr["LastPositionValues"].ToString(); } catch { CSVHelper.NegativeoffsetResume = ""; }

                            break;
                        }
                        break;
                    }
                    try { CSVHelper.MaxRPM = int.Parse(txtMaxRPM); } catch { }
                    try { CSVHelper.RecommendedRPM = int.Parse(txtRecRPM); } catch { }
                    //try { CSVHelper.RPMAssume = int.Parse(txtrpmAssume); } catch { }
                    try { CSVHelper.MotorDirValue = int.Parse(RampUpDown?"1":"0"); } catch { }
                    //try { CSVHelper.CalibrSeq02 = int.Parse(txtcalibrSeq02); } catch { }
                    try { CSVHelper.MotorRampDelayValue = txtdefaultTimeSec; } catch { }
                    try { CSVHelper.Max_mm = int.Parse(txtmaxMotorMove); } catch { }

                }
                editingEnable = FlexMoldDB.IsEditingEnabled;

                //GetSettings ends here!

            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
                TNF.ShowError(ex.Message);
            }

        }


        public void UpdateMotorSettingsDB()
        {

            try
            {
                //validations
                if (int.Parse(txtRecRPM) > 200 || int.Parse(txtMaxRPM) > 200 || int.Parse(txtRecRPM) > int.Parse(txtMaxRPM))
                {
                    MessageBox.Show("Max RPM or Recommeded RPM is invalid!", "Incorrect Values", MessageBoxButton.OK);
                    log.Log(LogLevel.Error, "User entered Invalid Max RPM or Recommeded RPM !");
                    return;
                }
                if (txtdefaultTimeSec > 5 || txtdefaultTimeSec < 1)
                {
                    MessageBox.Show("Max time is invalid!", "Incorrect Values", MessageBoxButton.OK);
                    log.Log(LogLevel.Error, "User entered Invalid Motor time !");
                    return;
                }


                Dictionary<string, string> dct = new Dictionary<string, string>();
                dct.Add("MaxRPM", txtMaxRPM.ToString());
                dct.Add("RecRPM", txtRecRPM.ToString());
                //dct.Add("RPMAssume", txtrpmAssume.ToString());
                dct.Add("MotorDir", RampUpDown?"1":"0");// txtmotorDirVal.ToString()
                //dct.Add("CalibrSeq02", txtcalibrSeq02.ToString());
                dct.Add("DefaultTimeSec", txtdefaultTimeSec.ToString());
                dct.Add("MaxMotorMove", txtmaxMotorMove.ToString());
                try { dct.Add("NegativeOffset", CSVHelper.NegativeoffsetResume); } catch { }

                FlexMoldDB.UpdateData("FM_MotorSetting", null, dct);

                log.Log(LogLevel.Info, "Successfully Updated Motor settings");

                LoadMotorSettingsDB();
            }
            catch (Exception ex)
            {
                log.Log(LogLevel.Error, ex.Message);
                TNF.ShowError(ex.Message);
            }

        }

    }
}