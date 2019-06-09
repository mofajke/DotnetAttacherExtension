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
        public DelegateCommand<DotnetProcess> AttachCommand { get; set; }
        public DelegateCommand<DotnetProcess> SelectCommand { get; set; }
        public DelegateCommand<DialogWindow> CancelCommand { get; set; }
        public DelegateCommand<DialogWindow> LoadedWindowCommand { get; set; }
        public DelegateCommand<DotnetProcess> DetachCommand { get; set; }

        private void InitCommands()
        {
            RefreshCommand = new DelegateCommand(() =>
            {
                LoadDotnetProcesses();
            });
            AttachCommand = new DelegateCommand<DotnetProcess>((e) =>
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
            DetachCommand = new DelegateCommand<DotnetProcess>((e) =>
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
            SelectCommand = new DelegateCommand<DotnetProcess>((proc) =>
            {
                if (proc != null && proc.Id > 0)
                {
                    SelectedDotnetProcess = proc;
                }
            });
            CancelCommand = new DelegateCommand<DialogWindow>((window) =>
            {
                window?.Close();
            });
            LoadedWindowCommand = new DelegateCommand<DialogWindow>((window) =>
            {
                loadedWindow = window;
                LoadDotnetProcesses();
            });
        }
    }
}
