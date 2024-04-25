using FlexMold.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.Model
{
    public class ProjectFilePanel : ObservableObject
    {
        public int ID { get; set; }
        public string FileName { get; set; }
        public string FileFullName { get; set; }
    }
}
