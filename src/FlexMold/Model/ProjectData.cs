using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.Model
{
    class ProjectData
    {
        public DirectoryInfo projectUrl { get; set; }
        public List<Panel> panels { get; set; }
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
