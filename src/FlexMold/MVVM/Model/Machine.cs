using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace FlexMold.MVVM.Model
{
    public class Machine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        private EMachineType _MachineType;

        public EMachineType MachineType
        {
            get { return _MachineType; }
            set
            {
                switch (value)
                {
                    case EMachineType.Motor:
                        IconT = new PackIconMaterial() { Height = 80, Width = 80, Kind = PackIconMaterialKind.Draw };
                        break;
                    case EMachineType.ScanDevices:
                        IconT = new PackIconMaterial() { Height = 80, Width = 80, Kind = PackIconMaterialKind.SettingsHelper };
                        break;
                    case EMachineType.Projector:
                        IconT = new PackIconMaterial() { Height = 80, Width = 80, Kind = PackIconMaterialKind.Projector };
                        break;
                }
                _MachineType = value;
            }
        }

        public Control IconT { get; private set; }
    }

    public enum EMachineType
    {
        Motor = 0,
        Projector, 
        ScanDevices
    }
}