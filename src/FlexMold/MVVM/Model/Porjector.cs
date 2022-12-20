using System.IO.Ports;

namespace FlexMold.MVVM.Model
{
    public class Projector
    {
        public string Name { get; set; }
        public string IpAddres { get; set; }
        public int Status { get; set; }
        public SerialPort Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string WorkGroup { get; set; }
        public string DataFolderName { get; set; }
        public string PlotFolderName { get; set; }
        public Projector()
        {
            PlotFolderName = "Share";
        }
    }
}