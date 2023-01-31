using FlexMold.MVVM.ViewModel;
using MahApps.Metro.Controls;
using ScriptingTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace FlexMold
{
    /// <summary>
    /// Interaction logic for SplashMachineSlection.xaml
    /// </summary>
    public partial class SplashMachineSlection : MetroWindow
    {
        public SplashMachineSlection()
        {
            InitializeComponent();
            var vm = DataContext as SplashScreenViewModel;
            vm.OnRequestClose += (s, e) => this.Close();
        }
    }
}
