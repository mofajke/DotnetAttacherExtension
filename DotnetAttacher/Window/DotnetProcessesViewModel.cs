using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DotnetAttacher.Annotations;
using DotnetAttacher.Window.Model;
using System.Management;
using System.Windows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Process = System.Diagnostics.Process;

namespace DotnetAttacher.Window
{
    public partial class DotnetProcessesViewModel : INotifyPropertyChanged
    {
        public string Title => "Attach to dotnet process";
        private DialogWindow loadedWindow;

        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        private int alreadyAttachedId;
        public int AlreadyAttachedId
        {
            get => alreadyAttachedId;
            set
            {
                if (alreadyAttachedId != value)
                {
                    alreadyAttachedId = value;
                    OnPropertyChanged(nameof(AlreadyAttachedId));
                }
            }
        }

        private DotnetProcess selectedDotnetProcess;
        public DotnetProcess SelectedDotnetProcess
        {
            get => selectedDotnetProcess;
            set
            {
                selectedDotnetProcess = value;
                OnPropertyChanged(nameof(SelectedDotnetProcess));
            }
        }

        private ObservableCollection<DotnetProcess> dotnetProcesses;
        public ObservableCollection<DotnetProcess> DotnetProcesses
        {
            get => dotnetProcesses;
            set
            {
                dotnetProcesses = value;
                OnPropertyChanged(nameof(DotnetProcesses));
            }
        }

        public DotnetProcessesViewModel()
        {
            Init();
            InitCommands();
        }
        private void Init()
        {
            SelectedIndex = -1;
            AlreadyAttachedId = -1;
            SelectedDotnetProcess = null;
        }

        public void LoadDotnetProcesses()
        {
            var systemProcesses = Process.GetProcessesByName("dotnet").ToList();
            var selectedId = SelectedDotnetProcess?.Id ?? 0;
            
            var processes = systemProcesses.Select(p =>
            {
                var longPointName = GetCommandLineParam(p.Id);
                return new DotnetProcess
                {
                    Id = p.Id,
                    Name = p.ProcessName,
                    LongPointName = longPointName,
                    ShortPointName = GetShortPointName(longPointName),
                    IsSelected = selectedId > 0 && p.Id == selectedId,
                    Attached = false
                };
            }).ToList();
            DotnetProcesses = new ObservableCollection<DotnetProcess>(processes);

            SetIndex(selectedId);
            SetAlreadyAttached();
        }

        private void SetIndex(int selectedId)
        {
            if (selectedId > 0)
            {
                var item = DotnetProcesses.FirstOrDefault(p => p.IsSelected);
                var index = DotnetProcesses.IndexOf(item);
                SelectedIndex = index;
            }
        }

        private static string GetShortPointName(string longPointName)
        {
            if (String.IsNullOrEmpty(longPointName))
            {
                return "";
            }

            longPointName = longPointName.Replace('\\', '/');
            var lastSlashPosition = longPointName.LastIndexOf('/');
            if (lastSlashPosition < 0)
            {
                return "";
            }

            return longPointName.Substring(lastSlashPosition + 1);
        }

        private static string GetCommandLineParam(int id)
        {
            try
            {
                string query = $"SELECT * FROM Win32_Process WHERE ProcessId = {id}";

                using (var mos = new ManagementObjectSearcher(query))
                {
                    using (var moc = mos.Get())
                    {
                        var pars = moc.Cast<ManagementObject>().ToList();
                        var props = pars.Select(p => p.Properties["CommandLine"]).ToList();
                        return props.FirstOrDefault()?.Value.ToString();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private void AttachToProcess(DotnetProcess process)
        {
            try
            {
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                var allProcesses = dte?.Debugger?.LocalProcesses?.Cast<Process2>();
                var proc = allProcesses?.FirstOrDefault(p => p.ProcessID == process.Id);
                if (proc != null)
                {
                    proc.Attach2();
                    loadedWindow?.Close();
                }
            }
            catch
            {
                MessageBox.Show($"Can't attach to process with Id = {process.Id} and Entry Point = {process.ShortPointName}");
            }
        }

        private void DetachToProcess(DotnetProcess process)
        {
            try
            {
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                var debugProcesses = dte?.Debugger?.DebuggedProcesses?.Cast<Process2>();
                var proc = debugProcesses?.FirstOrDefault(p => p.ProcessID == process.Id);
                if (proc != null)
                {
                    proc.Detach(false);
                    loadedWindow?.Close();
                }
            }
            catch
            {
                MessageBox.Show($"Can't Detach to process with Id = {process.Id} and Entry Point = {process.ShortPointName}");
            }
        }

        private static int FindAttachedProcess(IReadOnlyCollection<int> ids)
        {
            try
            {
                var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
                var debuggedProcesses = dte?.Debugger?.DebuggedProcesses?.Cast<Process2>().ToList();
                if (debuggedProcesses == null || ids == null) return -1;
                foreach (var id in ids)
                {
                    var found = debuggedProcesses.FirstOrDefault(p => p.ProcessID == id);
                    if (found != null)
                    {
                        return found.ProcessID;
                    }
                }

                return -1;
            }
            catch
            {
                MessageBox.Show($"Can't found debugged process");
            }

            return -1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetAlreadyAttached()
        {
            AlreadyAttachedId = FindAttachedProcess(DotnetProcesses.Select(p=>p.Id).ToList());
            if (AlreadyAttachedId > 0)
            {
                var item = DotnetProcesses.FirstOrDefault(p => p.Id == AlreadyAttachedId);
                if (item != null)
                {
                    SelectedDotnetProcess = item;
                    SelectedIndex = DotnetProcesses.IndexOf(item);
                    UpdateAttachedAndSelect();
                }
            }
        }

        private void UpdateAttachedAndSelect()
        {
            var list = DotnetProcesses.Select(p=>new DotnetProcess
            {
                Id = p.Id,
                LongPointName = p.LongPointName,
                Name = p.Name,
                ShortPointName = p.ShortPointName,
                IsSelected = p.Id == AlreadyAttachedId,
                Attached = p.Id == AlreadyAttachedId
            }).ToList();

            DotnetProcesses = new ObservableCollection<DotnetProcess>(list);
            SetIndex(AlreadyAttachedId);
        }
    }
}
