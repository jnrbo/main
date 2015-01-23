﻿using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32;

namespace SmartPrint.DriverSetupAction
{
    public class SmartPrintPort : PrintPort
    {
        #region Constants

        public const string DEFAULT_NAME             = "SMARTPRINTER:\0";
        public const string DEFAULT_OUTPUT_PATH      = @"C:\Program Files\SMARTdoc\Share\SMARTPRINTER\";
        public const string DEFAULT_FILE_PATTERN     = "%r_%c_%u_%Y%m%d_%H%n%s_%j.ps";
        public const int    DEFAULT_OVERWRITE        = 0;
        public const string DEFAULT_USER_COMMAND     = "";
        public const string DEFAULT_EXEC_PATH        = "";
        public const int    DEFAULT_WAIT_TERMINATION = 0;
        public const int    DEFAULT_PIPE_DATA        = 0;
        public const string DEFAULT_REGISTRY_KEY     = @"SYSTEM\CurrentControlSet\Control\Print\Monitors\SMARTPRINTER\SMARTPRINTER:";

        #endregion

        #region Private Fields

        new private string _name            = DEFAULT_NAME;
        new private string _outputPath      = DEFAULT_OUTPUT_PATH;
        new private string _filePattern     = DEFAULT_FILE_PATTERN;
        new private int    _overwrite       = DEFAULT_OVERWRITE;
        new private string _userCommand     = DEFAULT_USER_COMMAND;
        new private string _execPath        = DEFAULT_EXEC_PATH;
        new private int    _waitTermination = DEFAULT_WAIT_TERMINATION;
        new private int    _pipeData        = DEFAULT_PIPE_DATA;
        new private string _portKey         = DEFAULT_REGISTRY_KEY;

        #endregion

        #region Constructors

        private SmartPrintPort(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException();
            _name = name;
            saveAll();
        }

        private SmartPrintPort(string name, string appPath)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException();
            _name = name;
            if (!string.IsNullOrEmpty(appPath))
                OutputPath = appPath + (appPath.EndsWith("\\") ? "" : "\\") + Name;
            saveAll();
        }

        #endregion

        #region Properties

        new public string Name
        {
            get { return _name; }
        }

        public string OutputPath
        {
            get { return _outputPath; }
            set
            {
                _outputPath = value;
                try { saveProperty("OutputPath", value); }
                catch { throw; }
            }
        }

        public string FilePattern
        {
            get { return _filePattern; }
            set
            {
                _filePattern = value;
                try { saveProperty("FilePattern", value); }
                catch { throw; }
            }
        }

        public int Overwrite
        {
            get { return _overwrite; }
            set
            {
                _overwrite = value;
                try { saveProperty("Overwrite", value, RegistryValueKind.DWord); }
                catch { throw; }
            }
        }

        public string UserCommand
        {
            get { return _userCommand; }
            set
            {
                _userCommand = value;
                try { saveProperty("UserCommand", value); }
                catch { throw; }
            }
        }

        public string ExecPath
        {
            get { return _execPath; }
            set
            {
                _execPath = value;
                try { saveProperty("ExecPath", value); }
                catch { throw; }
            }
        }

        public int WaitTermination
        {
            get { return _waitTermination; }
            set
            {
                _waitTermination = value;
                try { saveProperty("WaitTermination", value, RegistryValueKind.DWord); }
                catch { throw; }
            }
        }

        public int PipeData
        {
            get { return _pipeData; }
            set
            {
                _pipeData = value;
                try { saveProperty("PipeData", value, RegistryValueKind.DWord); }
                catch { throw; }
            }
        }

        #endregion

        #region Static Methods

        public static SmartPrintPort AddPort(string portName, string appPath = "")
        {
            IntPtr printerHandle;
            PrinterDefaults defaults = new PrinterDefaults { DesiredAccess = PRINTER_ACCESS.ServerAdmin };
            try
            {
                if (!PrintPort.OpenPrinter(",XcvMonitor " + PrintMonitor.MONITOR_NAME, out printerHandle, ref defaults))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                try
                {
                    if (!portName.EndsWith("\0")) portName += "\0";
                    uint size = (uint)(portName.Length * 2);
                    IntPtr pointer = Marshal.AllocHGlobal((int)size);
                    Marshal.Copy(portName.ToCharArray(), 0, pointer, portName.Length);
                    IntPtr pOutputData = IntPtr.Zero;
                    UInt32 outputNeeded, status;
                    try
                    {
                        if (!PrintPort.XcvDataW(printerHandle, "AddPort", pointer, size, pOutputData, 0, out outputNeeded, out status))
                            throw new Win32Exception((int)status);
                    }
                    catch { throw; }
                    finally
                    {
                        Marshal.FreeHGlobal(pointer);
                    }
                }
                catch { throw; }
                finally
                {
                    PrintPort.ClosePrinter(printerHandle);
                }
            }
            catch { throw; }
            return new SmartPrintPort(portName, appPath);
        }

        public static void DeletePort(string portName)
        {
            IntPtr printerHandle;
            PrinterDefaults defaults = new PrinterDefaults { DesiredAccess = PRINTER_ACCESS.ServerAdmin };
            try
            {
                if (!PrintPort.OpenPrinter(",XcvMonitor " + PrintMonitor.MONITOR_NAME, out printerHandle, ref defaults))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                try
                {
                    if (!portName.EndsWith("\0")) portName += "\0";
                    uint size = (uint)(portName.Length * 2);
                    IntPtr pointer = Marshal.AllocHGlobal((int)size);
                    Marshal.Copy(portName.ToCharArray(), 0, pointer, portName.Length);
                    UInt32 outputNeeded, status;
                    try
                    {
                        if (!PrintPort.XcvDataW(printerHandle, "DeletePort", pointer, size, IntPtr.Zero, 0, out outputNeeded, out status))
                            throw new Win32Exception((int)status);
                    }
                    catch { throw; }
                    finally
                    {
                        Marshal.FreeHGlobal(pointer);
                    }
                }
                catch { throw; }
                finally
                {
                    ClosePrinter(printerHandle);
                }
                deleteSavedData(portName);
            }
            catch { throw; }
        }

        #endregion

        #region Helper Methods

        private void saveProperty(string keyName, object keyValue, RegistryValueKind kind = RegistryValueKind.String)
        {
            try
            {
                RegistryKey portKey = Registry.LocalMachine.OpenSubKey(_portKey);
                if (portKey == null) { portKey = Registry.LocalMachine.CreateSubKey(_portKey); }
                portKey.SetValue(keyName, keyValue, kind);
                portKey.Close();
            }
            catch { throw; }
        }

        private void saveAll()
        {
            try
            {
                RegistryKey portKey = Registry.LocalMachine.OpenSubKey(_portKey);
                if (portKey == null) { portKey = Registry.LocalMachine.CreateSubKey(_portKey); }
                portKey.SetValue("OutputPath", _outputPath, RegistryValueKind.String);
                portKey.SetValue("FilePattern", DEFAULT_FILE_PATTERN, RegistryValueKind.String);
                portKey.SetValue("Overwrite", _overwrite, RegistryValueKind.DWord);
                portKey.SetValue("UserCommand", _userCommand, RegistryValueKind.String);
                portKey.SetValue("ExecPath", _execPath, RegistryValueKind.String);
                portKey.SetValue("WaitTermination", _waitTermination, RegistryValueKind.DWord);
                portKey.SetValue("PipeData", _pipeData, RegistryValueKind.DWord);
                portKey.Close();
            }
            catch { throw; }

        }
        private static void deleteSavedData(string portName)
        {
            try
            {
                RegistryKey monitorKey = Registry.LocalMachine.OpenSubKey(PrintMonitor.REGISTRY_KEY);
                RegistryKey portKey = monitorKey.OpenSubKey(portName);
                if (portKey != null) { Registry.LocalMachine.DeleteSubKeyTree(portName); }
            }
            catch { throw; }
        }

        private void deleteSavedData()
        {
            try
            {
                RegistryKey portKey = Registry.LocalMachine.OpenSubKey(_portKey);
                if (portKey != null) { Registry.LocalMachine.DeleteSubKeyTree(_portKey); }
            }
            catch { throw; }
        }

        #endregion
    }
}
