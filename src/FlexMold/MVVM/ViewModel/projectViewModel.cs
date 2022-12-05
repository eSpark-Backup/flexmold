using FlexMold.Core;
using FlexMold.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel
{

    public class ProjectViewModel : ObservableObject
    {
        private MyDataModel _selectedMyDataModel;
        public MyDataModel selectedMyDataModel
        {
            get { return _selectedMyDataModel; }
            set
            {
                _selectedMyDataModel = value;
                sda = new ObservableCollection<FileInfo>(value.LaserFile) ;
                OnPropertyChanged();
            }
        }
        public MyDataModels myDataModels { get; set; }
        private ObservableCollection<FileInfo> _sda;

        public ObservableCollection<FileInfo> sda
        {
            get { return _sda; }
            set { _sda = value;
                OnPropertyChanged();
            }
        }

        public ProjectViewModel()
        {
            sda = new ObservableCollection<FileInfo>();
            sda.Add(new FileInfo(@"C:\Users\123\Desktop\Untitled.png"));
            myDataModels = new MyDataModels();
            string selectedPath = @"C:\Users\123\Desktop\Projects";
            DirectoryInfo di = new DirectoryInfo(selectedPath);
            var laserFilesPath = di.GetFiles("*.las", SearchOption.AllDirectories);
            var panels = laserFilesPath.GroupBy(p => p.Directory.Name);
            foreach (var item in panels)
            {
                myDataModels.Add(new MyDataModel()
                {
                    Panel = item.Key,
                    ProjectName = item.FirstOrDefault().Directory.Parent.Name,
                    LaserFile = item.ToList(),
                    Complete = false
                });
            }
        }
    }
}
