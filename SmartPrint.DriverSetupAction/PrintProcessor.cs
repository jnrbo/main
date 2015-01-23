﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartPrint.DriverSetupAction
{
    public class PrintProcessor
    {
        public const string DEFAULT_NAME = "WinPrint";
        public const string DEFAULT_DATATYPE = "RAW";
        public const string DEFAULT_PARAMETERS = "";

        public string Name { get; set; }
        public string DataType { get; set; }
        public string Parameters { get; set; }

        public PrintProcessor() : this(DEFAULT_NAME, DEFAULT_DATATYPE, DEFAULT_PARAMETERS) { }
        public PrintProcessor(string name, string dataType, string parameters)
        {
            Name = name;
            DataType = dataType;
            Parameters = parameters;
        }

        public static PrintProcessor Default
        {
            get { return new PrintProcessor(); }
        }
    }
}
