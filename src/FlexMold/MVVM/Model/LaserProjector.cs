namespace FlexMold.MVVM.Model
{
    public class LaserProjector
    {
        public string Name { get; set; }
        public string IpAddres { get; set; }
        public int Status { get; set; }
        public int TelnetPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string WorkGroup { get; set; }
        public string DataFolderName { get; set; }
        public string PlotFolderName { get; set; }

        public LaserProjector()
        {
            PlotFolderName = "Plot";
        }
    }

    public enum EProjectorComMode
    {
        Serial,
        Samba,
        Telnet
    }
}