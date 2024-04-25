using FlexMold.Utility;
using NLog;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Setting
{
    /// <summary>
    /// Interaction logic for LaserSettingView.xaml
    /// </summary>
    public partial class LaserProjectorSettingView : UserControl
    {
        private Logger log = LogManager.GetLogger("Setting");

        public LaserProjectorSettingView()
        {
            InitializeComponent();
            GetProjectorsList();

        }

        private void GetProjectorsList()
        {
            DataSet dsMachSet = new DataSet();

            //if (whClause.Length>0)
            //    dsMachSet = FlexMoldDB.ReadData("FM_LaserProjectors", whClause);
            //else
                dsMachSet = FlexMoldDB.ReadData("FM_LaserProjectors", null);

            string[] listProjIP = new string[dsMachSet.Tables[0].Rows.Count];
            int cntRows = 0;
            if (dsMachSet != null)
            {
                foreach (DataTable dt in dsMachSet.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        listProjIP[cntRows] = dr["LP_IPAddress"].ToString();
                        cntRows++;
                    }
                }
            }

            if (listProjIP.Length > 0)
                LP_SelectProj.ItemsSource = listProjIP;            
        }


        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            TelnetHelper.Instance.SendMessage = this.txtSendmsg.Text;
        }

        private void LP_SelectProj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetProjectorsSelectedItem(LP_SelectProj.SelectedValue.ToString());
        }

        private void GetProjectorsSelectedItem(string whClause)
        {
            DataSet dsMachSet = new DataSet();

            //if (whClause.Length > 0)
                dsMachSet = FlexMoldDB.ReadData("FM_LaserProjectors", "LP_IPAddress='" + whClause+"' ");
            //else
            //    dsMachSet = FlexMoldDB.ReadData("FM_LaserProjectors", null);

            string[] listProjIP = new string[dsMachSet.Tables[0].Rows.Count];
            int cntRows = 0;
            if (dsMachSet != null)
            {
                foreach (DataTable dt in dsMachSet.Tables)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        txtLP_IP.Text = dr["LP_IPAddress"].ToString();
                        txtLP_Port.Text = dr["LP_Port"].ToString();
                        //ZlaserIP = dr["LP_IPAddress"].ToString();
                        //try { ZlaserPort = int.Parse(dr["LP_Port"].ToString()); } catch { }
                        try
                        {
                            CSVHelper.IpAddres = txtLP_IP.Text;
                            CSVHelper.TelnetPort = int.Parse(txtLP_Port.Text);
                        }
                        catch { }

                        break;
                    }
                    break;
                }
            }

        }

        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateSettingsDB();
        }

        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {

        }
        private void UpdateSettingsDB()
        {
            //CREATE TABLE "FM_LaserProjectors"(
            //    "LP_IPAddress"  TEXT,
            //    "LP_Port"   TEXT,
            //    "LP_ProjectorName"  INTEGER,
            //    "LP_IsDefault"  INTEGER,
            //    PRIMARY KEY("LP_IPAddress")
            //)
            try
            {

                //set all projectors to 0
                Dictionary<string, string> dct = new Dictionary<string, string>();
                dct.Add("LP_IsDefault", "0");
                FlexMoldDB.InsertOrUpdateData("FM_LaserProjectors",null, dct);
                //log.Log(LogLevel.Info, $"Saved projector settings to database successfully!");

                //set selected projector to 1
                dct = new Dictionary<string, string>();
                dct.Add("LP_IsDefault", "1");
                FlexMoldDB.InsertOrUpdateData("FM_LaserProjectors", "LP_IPAddress='" + txtLP_IP.Text.ToString() + "' ", dct);
                CSVHelper.IpAddres = txtLP_IP.Text;
                CSVHelper.TelnetPort = int.Parse(txtLP_Port.Text);
                log.Log(LogLevel.Info, $"Updated default projector '" + txtLP_IP.Text.ToString() + "' to database successfully!");
            }
            catch { }

        }

    }
}


//private SerialPortConfigView _SerialConfigWindow { get { return App.AppHost.Services.GetRequiredService<SerialPortConfigView>(); } }

//private void SerialConfigWindow()
//{
//    Logger log = LogManager.GetLogger("Serial Port Config");
//    try
//    {
//        log.Log(LogLevel.Info, $"Current port {SelectedSerialPort.PortName} Baud {SelectedSerialPort.BaudRate}");
//        log.Log(LogLevel.Info, $"Please Select Port params from available list ");
//        Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
//        {
//            await DialogHost.Show(_SerialConfigWindow, "RootDialog", ClosingEventHandler);
//        }));
//        if (SaveData)
//        {
//            SelectedSerialPort = _SerialConfigWindow.CurrentPort;
//            log.Log(LogLevel.Info, $"Saved Port {SelectedSerialPort.PortName} Baud {SelectedSerialPort.BaudRate}");
//        }
//        else
//        {
//            log.Log(LogLevel.Warn, $"Closing without saving");
//        }
//    }
//    catch (Exception ex)
//    {
//        log.Log(LogLevel.Error, ex.Message);
//        TNF.ShowError(ex.Message);
//    }
//}
