using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace VData_Explorer
{
    public static class Settings
    {
        const string MyRegisteredHomeOrNot = @"SOFTWARE\Leayal\VDataExplorer";
        public static string LastExtractLocation
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(MyRegisteredHomeOrNot))
                {
                    if (regKey == null)
                        return string.Empty;
                    return (string)regKey.GetValue("LastExtractLocation", string.Empty);
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(MyRegisteredHomeOrNot))
                {
                    regKey.SetValue("LastExtractLocation", value, RegistryValueKind.String);
                }
            }
        }

        public static string LastFileLocation
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(MyRegisteredHomeOrNot))
                {
                    if (regKey == null)
                        return string.Empty;
                    return (string)regKey.GetValue("LastFileLocation", string.Empty);
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(MyRegisteredHomeOrNot))
                {
                    regKey.SetValue("LastFileLocation", value, RegistryValueKind.String);
                }
            }
        }

        public static string[] UsedPassword
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(MyRegisteredHomeOrNot))
                {
                    if (regKey == null)
                        return null;
                    var sumthin = regKey.GetValue("UsedPassword");
                    if (sumthin is string[] result)
                    {
                        return result;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(MyRegisteredHomeOrNot))
                {
                    regKey.SetValue("UsedPassword", value, RegistryValueKind.MultiString);
                }
            }
        }
    }
}
