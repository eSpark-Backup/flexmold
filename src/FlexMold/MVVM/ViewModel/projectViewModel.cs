using FlexMold.Core;
using FlexMold.MVVM.Model;
using Sylvan.Data.Csv;
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

    class ProjectViewModel
    {
        RelayCommand ReadFileCommand { get; set; }

        private MyDataModel _selectedMyDataModel;
        public MyDataModel selectedMyDataModel
        {
            get { return _selectedMyDataModel; }
            set
            {
                _selectedMyDataModel = value;
                slectedFile = null;
            }
        }
        public MyDataModels myDataModels { get; set; }
        private FileInfo _slectedFile;

        public FileInfo slectedFile
        {
            get { return _slectedFile; }
            set
            {
                _slectedFile = value;
            }
        }

        void readCsvLasFile()
        {
            if (slectedFile != null)
            {

                Console.WriteLine();
            }
        }
        public ProjectViewModel()
        {
            ReadFileCommand = new RelayCommand(o => { readCsvLasFile(); });
            //FileInfo f = new FileInfo("");
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
