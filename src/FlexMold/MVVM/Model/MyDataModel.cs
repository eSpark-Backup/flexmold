using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using FlexMold.Core;

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
            set { _ProjectName = value;
                OnPropertyChanged();
            }
        }


        private bool m_Complete = false;
        public bool Complete
        {
            get { return this.m_Complete; }
            set
            {
                if (value != this.m_Complete)
                {
                    this.m_Complete = value;
                    OnPropertyChanged();

                }
            }
        }
        private string _Panel;
        public string Panel
        {
            get { return _Panel; }
            set {
                if (value != this._Panel)
                {
                    this._Panel = value;
                    OnPropertyChanged();

                }
            }
        }
        private List<FileInfo> _laserFile;
        public List<FileInfo> LaserFile
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