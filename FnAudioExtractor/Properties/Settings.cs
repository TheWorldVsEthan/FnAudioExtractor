using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FnAudioExtractor.Properties
{
    public static class Settings
    {
        public static Assembly AppAssembly = Assembly.GetExecutingAssembly();
        public static string Version = "2.0";
        public static string Name = AppAssembly.GetName().Name!;

        public static string Environment = "Development";


        public static string binkadecPath = null;
        public static string exportsFolder;
    }
}