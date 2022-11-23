using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexMold.MVVM.Model
{
    class ProjectModel
    {
        public List<int> projects { get; set; }
        public ProjectModel()
        {
            projects = new List<int>() {1,2,3,4,5,6,7 };
        }
    }
}
