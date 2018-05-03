using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace VData_Explorer
{
    public static class Settings
    {
        public static string LastExtractLocation
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Leayal\VDataExplorer"))
                {
                    if (regKey == null)
                        return string.Empty;
                    return (string)regKey.GetValue("LastExtractLocation", string.Empty);
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Leayal\VDataExplorer"))
                {
                    regKey.SetValue("LastExtractLocation", value, RegistryValueKind.String);
                }
            }
        }

        public static string LastFileLocation
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Leayal\VDataExplorer"))
                {
                    if (regKey == null)
                        return string.Empty;
                    return (string)regKey.GetValue("LastFileLocation", string.Empty);
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Leayal\VDataExplorer"))
                {
                    regKey.SetValue("LastFileLocation", value, RegistryValueKind.String);
                }
            }
        }
    }
}
