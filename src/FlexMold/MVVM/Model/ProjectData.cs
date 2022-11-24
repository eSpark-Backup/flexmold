using FlexMold.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.Model
{
    class ProjectData : ObservableObject
    {
        private DirectoryInfo _projectUrl;

        public DirectoryInfo projectUrl
        {
            get { return _projectUrl; }
            set
            {
                _projectUrl = value;
                OnPropertyChanged();
            }
        }

        public List<Panel> panels { get; set; }
        private string _labetext;

        public string labeltext
        {
            get { return _labetext; }
            set
            {
                _labetext = value;
                OnPropertyChanged();
            }
        }
    }
    class Panel
    {
        public FileInfo panelUrl { get; set; }
        public FileInfo csvUrl { get; set; }
        public List<LaserFile> laserFile { get; set; }
    }
    class LaserFile
    {
        public FileInfo lsvUrl { get; set; }
    }
}