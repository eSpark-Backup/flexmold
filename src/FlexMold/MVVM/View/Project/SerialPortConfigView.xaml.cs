using MahApps.Metro.Controls;
using System.IO.Ports;
using System.Windows;
using System.Windows.Media.Effects;

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for SerialPortConfigView.xaml
    /// </summary>
    public partial class SerialPortConfigView : MetroWindow
    {
        public MainWindow main { get; set; }
        public SerialPort CurrentPort { get; set; }
        public bool SaveData { get; set; }

        public SerialPortConfigView(MainWindow _main, SerialPort _SelectedSerialPort)
        {
            main = _main;
            main.IsEnabled = false;
            main.Effect = new BlurEffect();
            InitializeComponent();
            SaveData = false;
            CurrentPort = new SerialPort();
            CurrentPort = _SelectedSerialPort;
            CB_SerialBaudRate.ItemsSource = new int[] { 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, 256000 };
            CB_SerialPortName.ItemsSource = SerialPort.GetPortNames();
            CB_SerialBaudRate.SelectedItem = _SelectedSerialPort.BaudRate;
            CB_SerialPortName.SelectedItem = _SelectedSerialPort.PortName;
            if (CB_SerialPortName.SelectedItem == null)
                CB_SerialPortName.SelectedIndex = 0;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SaveData)
            {
                main.Effect = null;
                main.IsEnabled = true;
                CurrentPort.PortName = CB_SerialPortName.SelectedItem.ToString();
                CurrentPort.BaudRate = (int)CB_SerialBaudRate.SelectedItem;
            }
            else
            {
                main.Effect = null;
                main.IsEnabled = true;
            }
        }

        private void MetroWindow_Closing(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CB_SerialBaudRate.SelectedItem != null & CB_SerialPortName.SelectedItem.ToString() != "")
            {
                SaveData = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please Enter Values in all fields first \n otherwise close or discard window");
            }
        }
    }
}