using FlexMold.Core;
using FlexMold.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlexMold.MVVM.ViewModel
{
     class ProjectViewModel : ObservableObject
    {
        public  ProjectData projectData { get; set; }

        public ProjectViewModel()
        {
            projectData = new ProjectData();
            Task.Run(() =>
            {
                while (true)
                {
                    Random rn = new Random();
                    projectData.labeltext = rn.Next(1, 1000).ToString();
                    Thread.Sleep(1000);
                }
            });
        }
    }

}
