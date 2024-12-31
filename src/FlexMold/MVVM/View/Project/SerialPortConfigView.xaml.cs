using FlexMold.MVVM.ViewModel.Project;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace FlexMold.MVVM.View.Project
{
    /// <summary>
    /// Interaction logic for SerialPortConfigView.xaml
    /// </summary>
    public partial class SerialPortConfigView : UserControl
    {
        public SerialPort CurrentPort { get; set; }
        public bool SaveData { get; set; }
        ILaserProjectorViewModel _LaserProjectorViewModel = App.AppHost.Services.GetRequiredService<ILaserProjectorViewModel>();

        public SerialPortConfigView()
        {
            DataContext = _LaserProjectorViewModel;
            InitializeComponent();
            _LaserProjectorViewModel.SaveData = false;
            CurrentPort = new SerialPort();
            CurrentPort =_LaserProjectorViewModel.SelectedSerialPort;
            CB_SerialBaudRate.ItemsSource = new int[] { 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, 256000 };
            CB_SerialPortName.ItemsSource = SerialPort.GetPortNames();
            CB_SerialBaudRate.SelectedItem = _LaserProjectorViewModel.SelectedSerialPort.BaudRate;
            CB_SerialPortName.SelectedItem = _LaserProjectorViewModel.SelectedSerialPort.PortName;
            if (CB_SerialPortName.SelectedItem == null)
                CB_SerialPortName.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CB_SerialBaudRate.SelectedItem != null & CB_SerialPortName.SelectedItem.ToString() != "")
            {
                _LaserProjectorViewModel.SaveData = true;
            }
            else
            {
                MessageBox.Show("Please Enter Values in all fields first \n otherwise close or discard window");
            }
        }
    }
}