using System.Diagnostics;
using System.Windows;
using DotnetAttacher.Window.Model;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;

namespace DotnetAttacher.Window
{
    public partial class DotnetProcessesViewModel
    {
        public DelegateCommand RefreshCommand { get; set; }
        public DelegateCommand AttachCommand { get; set; }
        public DelegateCommand SelectCommand { get; set; }
        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand LoadedWindowCommand { get; set; }
        public DelegateCommand DetachCommand { get; set; }

        private void InitCommands()
        {
            RefreshCommand = new DelegateCommand((object o) =>
            {
                LoadDotnetProcesses();
            });
            AttachCommand = new DelegateCommand((object o) =>
            {
                var proc = SelectedDotnetProcess;
                if (proc == null) return;
                Process aliveProc = null;
                try
                {
                    aliveProc = Process.GetProcessById(proc.Id);
                }
                catch
                {
                    MessageBox.Show($"Can't attach. Process with Id = {proc.Id} and Entry Point = {proc.ShortPointName} not started");
                    Init();
                    LoadDotnetProcesses();
                    return;
                }
                
                if (aliveProc?.Id <= 0) return;
                AttachToProcess(proc);
            });
            DetachCommand = new DelegateCommand((object o) =>
            {
                var proc = SelectedDotnetProcess;
                if (proc == null) return;
                Process aliveProc;
                try
                {
                    aliveProc = Process.GetProcessById(proc.Id);
                }
                catch
                {
                    MessageBox.Show($"Can't detach. Process with Id = {proc.Id} and Entry Point = {proc.ShortPointName} not started");
                    Init();
                    LoadDotnetProcesses();
                    return;
                }

                if (aliveProc?.Id <= 0) return;
                DetachToProcess(proc);
            });
            SelectCommand = new DelegateCommand((object o) =>
            {
                var proc = o as DotnetProcess;
                if (proc != null && proc.Id > 0)
                {
                    SelectedDotnetProcess = proc;
                }
            });
            CancelCommand = new DelegateCommand((object o) =>
            {
                var window = o as DialogWindow;
                window?.Close();
            });
            LoadedWindowCommand = new DelegateCommand((object o) =>
            {
                loadedWindow = o as DialogWindow;
                LoadDotnetProcesses();
            });
        }
    }
}
