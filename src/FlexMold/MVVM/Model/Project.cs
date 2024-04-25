using FlexMold.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace FlexMold.MVVM.Model
{
    // Task Class
    // Requires using System.ComponentModel;
    public class Project : ObservableObject
    {
        // The Task class implements INotifyPropertyChanged and IEditableObject
        // so that the datagrid can properly respond to changes to the
        // data collection and edits made in the DataGrid.

        // Private task data.

        private string _ProjectName;

        public string ProjectName
        {
            get { return _ProjectName; }
            set
            {
                _ProjectName = value;
                OnPropertyChanged();
            }
        }

        private string _Panel;

        public string Panel
        {
            get { return _Panel; }
            set
            {
                if (value != this._Panel)
                {
                    this._Panel = value;
                    OnPropertyChanged();
                }
            }
        }

        private List<ProjectFilePanel> _laserFile;

        public List<ProjectFilePanel> LaserFile
        {
            get { return _laserFile; }
            set
            {
                _laserFile = value;
                OnPropertyChanged();
            }
        }
    }

    // Requires using System.Collections.ObjectModel;
    public class Porjects : ObservableCollection<Project>
    {
        // Creating the Tasks collection in this way enables data binding from XAML.
    }
}