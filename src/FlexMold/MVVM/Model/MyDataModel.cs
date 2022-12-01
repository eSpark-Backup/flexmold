using System;
using System.Collections.ObjectModel;
using FlexMold.Core;

namespace FlexMold.MVVM.Model
{
    // Task Class
    // Requires using System.ComponentModel;
    class MyDataModel : ObservableObject
    {
        // The Task class implements INotifyPropertyChanged and IEditableObject
        // so that the datagrid can properly respond to changes to the
        // data collection and edits made in the DataGrid.

        // Private task data.
        private string m_ProjectName = string.Empty;
        private string m_TaskName = string.Empty;
        private DateTime m_DueDate = DateTime.Now;
        private bool m_Complete = false;

        // Data for undoing canceled edits.
        private MyDataModel temp_Task = null;
        private bool m_Editing = false;

        // Public properties.
        public string ProjectName
        {
            get { return this.m_ProjectName; }
            set
            {
                if (value != this.m_ProjectName)
                {
                    this.m_ProjectName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TaskName
        {
            get { return this.m_TaskName; }
            set
            {
                if (value != this.m_TaskName)
                {
                    this.m_TaskName = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime DueDate
        {
            get { return this.m_DueDate; }
            set
            {
                if (value != this.m_DueDate)
                {
                    this.m_DueDate = value;
                    OnPropertyChanged();
                }
            }
        }

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
                }
                OnPropertyChanged();
            }
        }

    }
    // Requires using System.Collections.ObjectModel;
    class MyDataModels : ObservableCollection<MyDataModel>
    {
        // Creating the Tasks collection in this way enables data binding from XAML.
    }
}