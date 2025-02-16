﻿using System.Diagnostics;
using System.Windows;
using DentoInjector.Core.Bindings;
using AdonisMessageBox = AdonisUI.Controls.MessageBox;

namespace DentoInjector.Graphics
{

    public partial class WnSelectProcess
    {

        public WnSelectProcess()
        {
            InitializeComponent();
            Refresh(null, null);
        }

        public int SelectedProcessId { get; private set; }
        public string SelectedProcessName { get; private set; }

        private void Continue(object sender, RoutedEventArgs args)
        {
            var item = (ProcessItemBinding)ProcessList.SelectedItem;
            if (item == null)
            {
                AdonisMessageBox.Show("Select a process before continuing!", "DentoInjector");
                return;
            }
            SelectedProcessId = item.Id;
            SelectedProcessName = item.Name;
            DialogResult = true;
            Close();
        }

        private void Refresh(object sender, RoutedEventArgs args)
        {
            ProcessList.Items.Clear();
            foreach (var process in Process.GetProcesses())
            {
                if (string.IsNullOrEmpty(process.MainWindowTitle))
                    continue;
                ProcessList.Items.Add(ProcessItemBinding.Create(process));
            }
        }

        private void CopyProcessId(object sender, RoutedEventArgs args)
        {
            var item = (ProcessItemBinding)ProcessList.SelectedItem;
            if (item == null)
                return;
            Clipboard.SetText(item.Id.ToString());
        }

        private void CopyExecutablePath(object sender, RoutedEventArgs args)
        {
            var item = (ProcessItemBinding)ProcessList.SelectedItem;
            if (item == null)
                return;
            Clipboard.SetText(item.Path);
        }

    }

}