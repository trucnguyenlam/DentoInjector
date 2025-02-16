﻿using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Bleak;
using DentoInjector.Core.Bindings;
using Microsoft.Win32;
using AdonisMessageBox = AdonisUI.Controls.MessageBox;

namespace DentoInjector.Graphics
{

    public partial class WnMain
    {

        private Injector _currentInjector;

        private int _targetProcessId;

        public WnMain()
        {
            InitializeComponent();
            if (App.Settings.DllFiles == null)
                return;
            foreach (var dll in App.Settings.DllFiles)
                if (File.Exists(dll.Path))
                    DllFileList.Items.Add(dll);
            UpdateDllSelection(null!, null!);
        }

        private void SaveSettings(object sender, CancelEventArgs args)
        {
            App.Settings.Save();
        }

        private void Exit(object sender, RoutedEventArgs args)
        {
            Application.Current.Shutdown();
        }

        private void InjectToProcess(object sender, RoutedEventArgs args)
        {
            if (!InjectButton.IsEnabled)
                return;
            if (DllFileList.SelectedItem == null || _targetProcessId <= 0)
            {
                AdonisMessageBox.Show("Select a DLL and a target process before injecting.", "DentoInjector");
                return;
            }
            try
            {
                var binding = (DllFileBinding)DllFileList.SelectedItem;
                var method = MethodBox.SelectedIndex switch
                {
                    1 => InjectionMethod.HijackThread,
                    2 => InjectionMethod.ManualMap,
                    _ => InjectionMethod.CreateThread
                };
                var flag = FlagBox.SelectedIndex switch
                {
                    1 => InjectionFlags.HideDllFromPeb,
                    2 => InjectionFlags.RandomiseDllHeaders,
                    3 => InjectionFlags.RandomiseDllName,
                    _ => InjectionFlags.None
                };
                _currentInjector = new Injector(_targetProcessId, binding.Path, method, flag);
                _currentInjector.InjectDll();
                var message = "DLL has been injected into process!";
                if (flag != InjectionFlags.HideDllFromPeb)
                {
                    Dispatcher.Invoke(() =>
                    {
                        InjectButton.IsEnabled = false;
                        EjectButton.IsEnabled = true;
                    });
                    message += " You can also eject the DLL from the process at will.";
                }
                AdonisMessageBox.Show($"Injection successful!\n\n{message}", "DentoInjector");
            }
            catch
            {
                AdonisMessageBox.Show("Injection unsuccessful!\n\nDLL has been injected into process! The DLL's architecture might not be the same as the target process's architecture. Restart and reselect the target process and try again.", "DentoInjector");
            }
        }

        private void EjectFromProcess(object sender, RoutedEventArgs args)
        {
            if (!EjectButton.IsEnabled)
                return;
            try
            {
                _currentInjector.EjectDll();
                _currentInjector.Dispose();
                AdonisMessageBox.Show("DLL has been ejected from process!", "DentoInjector");
            }
            catch
            {
                AdonisMessageBox.Show("Unable to eject from process! Restart the target process as an alternative.", "DentoInjector");
            }
            Dispatcher.Invoke(() =>
            {
                InjectButton.IsEnabled = true;
                EjectButton.IsEnabled = false;
            });
        }

        private void ImportDll(object sender, RoutedEventArgs args)
        {
            var dialog = new OpenFileDialog { Filter = "Dynamic Link Library|*.dll", Multiselect = true };
            if (dialog.ShowDialog() != true)
                return;
            var items = DllFileList.Items.OfType<DllFileBinding>().ToArray();
            try
            {
                foreach (var path in dialog.FileNames)
                {
                    var alreadyExisted = false;
                    foreach (var item in items)
                        if (item.Path == path)
                            alreadyExisted = true;
                    if (alreadyExisted)
                        continue;
                    var binding = DllFileBinding.Create(path);
                    DllFileList.Items.Add(binding);
                    App.Settings.DllFiles = DllFileList.Items.OfType<DllFileBinding>().ToArray();
                }
                UpdateDllSelection(null!, null!);
            }
            catch
            {
                AdonisMessageBox.Show("Import unsuccessful! The file might be invalid or unreadable.", "DentoInjector");
            }
        }

        private void RemoveDlls(object sender, RoutedEventArgs args)
        {
            var items = DllFileList.SelectedItems.OfType<DllFileBinding>().ToArray();
            foreach (var item in items)
                DllFileList.Items.Remove(item);
            App.Settings.DllFiles = DllFileList.Items.OfType<DllFileBinding>().ToArray();
            UpdateDllSelection(null!, null!);
        }

        private void SelectProcess(object sender, RoutedEventArgs args)
        {
            var dialog = new WnSelectProcess { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == false)
                return;
            _targetProcessId = dialog.SelectedProcessId;
            TargetProcessBox.Text = $"{dialog.SelectedProcessName} ({dialog.SelectedProcessId})";
        }

        private void UpdateDllSelection(object sender, SelectionChangedEventArgs args)
        {
            RemoveButton.IsEnabled = DllFileList.SelectedItem != null;
        }

        private void CopyDllPath(object sender, RoutedEventArgs args)
        {
            var item = (DllFileBinding)DllFileList.SelectedItem;
            if (item == null)
                return;
            Clipboard.SetText(item.Path);
        }

    }

}