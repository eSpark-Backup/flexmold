using ScriptingTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TwincatAutomationTestAppWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool _initializing = false;


        public bool IsInitializing
        {
            get { return _initializing; }
        }


        public MainWindow()
        {
            _initializing = true;
            InitializeComponent();

            int defaultIndex = -1;
            int currentIndex = -1;
            IDictionary<string, DTEInfo> progIdDict = ConfigurationFactory.GetVisualStudioProgIds(out currentIndex, out defaultIndex);

            //List<string> progIds = new List<string>(progIdDict.Keys);


            foreach (DTEInfo key in progIdDict.Values)
            {
                this.cBProgID.Items.Add(key);
            }

            int actualIndex = defaultIndex;

            if (actualIndex == -1)
                actualIndex = currentIndex;


            //obsolete this.cBProgID.Items.Add(ConfigurationGenerator.ProgID_XAE);

            this.cBProgID.SelectedIndex = actualIndex;

            this.cbProgVisible.IsChecked = true;
            this.cbUserControl.IsChecked = true;
            this.cbSuppressUI.IsChecked = true;

            _initializing = false;

            // Setting Script filter (Filter Out all Exes)

            ScriptLoader.ScriptFilter = new Func<Type, bool>(

                (type) =>
                {
                    return (type.Assembly.EntryPoint == null);
                }
            );

            setScripts(ScriptLoader.Scripts); // Setting the Scripts to the ListView
            enableDisableControls(); // EnableDisable Visual Elements
        }


        private void setScripts(IList<Script> scripts)
        {
            //this.lVScripts.DataContext = scripts;
            scripts.Add(new FlexMoldMachine1());

            this.lVScripts.ItemsSource = scripts;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lVScripts.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("Category", ListSortDirection.Ascending));

        }


        public DTEInfo DTEInfo
        {
            get { return (DTEInfo)cBProgID.SelectedValue; }
        }

        /// <summary>
        /// Gets a value indicating whether the IDE is visible during script
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is IDE visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsIDEVisible
        {
            get { return cbProgVisible.IsChecked.Value; }
        }

        /// <summary>
        /// Gets a value indicating whether the IDE is controlled by User
        /// </summary>
        /// <value>
        /// </value>
        public bool IsIDEUserControl
        {
            get { return cbUserControl.IsChecked.Value; }
        }

        /// Gets a value indicating whether VisualStudio UI is suppressed.
        public bool SupressUI
        {
            get { return cbSuppressUI.IsChecked.Value; }
        }

        IWorker _worker = null;


        ConfigurationFactory _factory = null;


        Script _runningScript = null;


        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            Debug.Assert(this._factory == null);

            if (this.SelectedScript != null)
            {
                _runningScript = this.SelectedScript;
                _runningScript.StatusChanged += new EventHandler<ScriptStatusChangedEventArgs>(script_StatusChanged);

                VsFactory vsFactory = new VsFactory();

                if (_runningScript is ScriptEarlyBound)
                    this._factory = new EarlyBoundFactory(vsFactory);
                else if (_runningScript is ScriptLateBound)
                    this._factory = new LateBoundFactory(vsFactory);

                if (this._factory == null)
                {
                    throw new ApplicationException("Generator not found!");
                }

                Dictionary<string, dynamic> parameterSet = new Dictionary<string, dynamic>();
                ScriptContext context = new ScriptContext(_factory, null, parameterSet);

                _worker = new ScriptBackgroundWorker(/*this._factory,*/ _runningScript, context);
                this.Update(_runningScript);
                _worker.ConfigurationInitialized += new EventHandler(_worker_ConfigurationInitialized);
                _worker.ProgressChanged += new ProgressChangedEventHandler(_worker_ProgressChanged);
                _worker.ProgressStatusChanged += new EventHandler<ProgressStatusChangedArgs>(_worker_ProgressStatusChanged);
                _worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_worker_RunWorkerCompleted);
                _factory.AppID = this.DTEInfo.ProgId;
                _factory.IsIdeVisible = this.IsIDEVisible;
                _factory.IsIdeUserControl = this.IsIDEUserControl;
                _factory.SuppressUI = this.SupressUI;
                SetExecution(true);
                _worker.BeginScriptExecution();
            }
        }

        void _worker_ConfigurationInitialized(object sender, EventArgs e)
        {
            Action action = () =>
            {
                this.cbProgVisible.IsChecked = _factory.IsIdeVisible;
                this.cbSuppressUI.IsChecked = _factory.SuppressUI;
                this.cbUserControl.IsChecked = _factory.IsIdeUserControl;
            };

            this.Dispatcher.Invoke(action, new object[] { });
        }

        void _worker_ProgressStatusChanged(object sender, ProgressStatusChangedArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                int index = this.lvMessages.Items.Add(e.Status);
                object item = this.lvMessages.Items[index];
                this.lvMessages.ScrollIntoView(item);
            }
            else
            {
                Action<string> action = (str) =>
                {
                    int index = this.lvMessages.Items.Add(str);
                    object item = this.lvMessages.Items[index];
                    this.lvMessages.ScrollIntoView(item);
                };

                this.Dispatcher.Invoke(action, new object[] { e.Status });
            }
        }

        void script_StatusChanged(object sender, ScriptStatusChangedEventArgs e)
        {
            Action<Script> action = (script) =>
            {
                if (e.NewState == ScriptStatus.Initializing)
                {
                    this.lvMessages.Items.Clear();
                }

                Update(script);
            };

            this.Dispatcher.Invoke(action, new object[] { this._runningScript });
        }


        private void Update(Script script)
        {
            switch (script.Status)
            {
                case ScriptStatus.None:
                    if (script.Result == ScriptResult.None)
                        statusBox.Background = Brushes.Silver;
                    else if (script.Result == ScriptResult.Fail)
                        statusBox.Background = Brushes.Red;
                    else if (script.Result == ScriptResult.Ok)
                        statusBox.Background = Brushes.Green;
                    break;
                case ScriptStatus.Initializing:
                case ScriptStatus.Cleanup:
                    statusBox.Background = Brushes.Gray;
                    break;
                case ScriptStatus.Executing:
                    statusBox.Background = Brushes.Yellow;
                    break;
            }
        }

        void _worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Action<Script> action = (script) =>
            {
                SetExecution(false);
                _factory = null;
                Update(script);
                _runningScript.StatusChanged -= new EventHandler<ScriptStatusChangedEventArgs>(script_StatusChanged);
                _runningScript = null;
            };

            this.Dispatcher.BeginInvoke(action, new object[] { this._runningScript });
        }


        bool _executing = false;

        public bool IsExecuting
        {
            get { return _executing; }
        }


        private void SetExecution(bool set)
        {
            _executing = set;
            enableDisableControls();
        }


        private void enableDisableControls()
        {
            bool scriptSelected = (this.SelectedScript != null);
            bool isExecuting = this.IsExecuting;
            bool isCancelPending = this.IsExecuting && this._worker.CancellationPending;

            this.btnCancel.IsEnabled = isExecuting && !isCancelPending;
            this.btnExecute.IsEnabled = !isExecuting && scriptSelected;
            //this.gbBinding.IsEnabled = !isExecuting;
            this.gbProgID.IsEnabled = !isExecuting;
            this.lVScripts.IsEnabled = !isExecuting;

            gBSettings.IsEnabled = !isExecuting;
        }

        void _worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.progressBar1.Value = e.ProgressPercentage;
            }
            else
            {
                Action<int> action = (i) =>
                {
                    this.progressBar1.Value = i;
                };

                this.Dispatcher.Invoke(action, new object[] { e.ProgressPercentage });
            }
        }

        public Script SelectedScript
        {
            get
            {
                return (Script)this.lVScripts.SelectedItem;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this._worker != null)
            {
                this._worker.CancelRequest();
                this.enableDisableControls();
                //this._worker.CancelAndWait(TimeSpan.FromSeconds(0.1));
            }
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            if (this._worker != null)
                this._worker.CancelAndWait(TimeSpan.FromSeconds(1.0));

            base.OnClosing(e);
        }

        private void lVScripts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToolTip toolTip = new ToolTip();

            Script selected = this.SelectedScript;
            if (selected != null)
                toolTip.Content = selected.DetailedDescription;
            else
                toolTip.Content = string.Empty;

            lVScripts.ToolTip = toolTip;
            enableDisableControls();
            Update(selected);
        }
    }
}
