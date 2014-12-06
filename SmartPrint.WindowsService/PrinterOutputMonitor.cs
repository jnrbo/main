﻿using System;
using System.Diagnostics;
using System.IO;
using System.Security.Permissions;
using SmartPrint.WindowsService.Helpers;

namespace SmartPrint.WindowsService
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    public class PrinterOutputMonitor
    {
        private FileSystemWatcher _watchDog;

        public event FilePrinted FilePrinted;

        public void Start(string path)
        {
            if (Directory.Exists(path))
            {
                _watchDog = new FileSystemWatcher(path, "*.ps");
                _watchDog.NotifyFilter = NotifyFilters.DirectoryName;

                _watchDog.NotifyFilter = _watchDog.NotifyFilter | NotifyFilters.FileName;
                _watchDog.NotifyFilter = _watchDog.NotifyFilter | NotifyFilters.Attributes;

                _watchDog.Created += FileCreated;

                try
                {
                    _watchDog.EnableRaisingEvents = true;
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
            else
            {
                Debug.WriteLine(string.Format("Unable to monitor folder: {0}. Folder does not exist.", path));
            }
        }

        public void Stop()
        {
            if (_watchDog != null)
            {
                _watchDog.EnableRaisingEvents = false;
                _watchDog.Dispose();
            }
        }

        private void FileCreated(object sender, FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:

                    while (FileHelper.IsFileLocked(e.FullPath))
                        System.Threading.Thread.Sleep(250);

                    if (FilePrinted != null)
                        FilePrinted(e.FullPath);

                    break;
            }
        }
    }
}
