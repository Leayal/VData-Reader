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

        public static string LastFileExtractLocation
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(MyRegisteredHomeOrNot))
                {
                    if (regKey == null)
                        return string.Empty;
                    return (string)regKey.GetValue("LastFileExtractLocation", string.Empty);
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(MyRegisteredHomeOrNot))
                {
                    regKey.SetValue("LastFileExtractLocation", value, RegistryValueKind.String);
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

        public static bool EnableAutoCorrectPath
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(MyRegisteredHomeOrNot))
                {
                    if (regKey == null)
                        return false;
                    return ((int)regKey.GetValue("EnableAutoCorrectPath", 0) != 0);
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(MyRegisteredHomeOrNot))
                {
                    if (value)
                        regKey.SetValue("EnableAutoCorrectPath", 1, RegistryValueKind.DWord);
                    else
                        regKey.SetValue("EnableAutoCorrectPath", 0, RegistryValueKind.DWord);
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

        public static ExtractionComplete ActionWhenComplete
        {
            get
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(MyRegisteredHomeOrNot))
                {
                    if (regKey == null)
                        return ExtractionComplete.Prompt;
                    return (ExtractionComplete)((int)regKey.GetValue("ActionWhenComplete", 1));
                }
            }
            set
            {
                using (var regKey = Registry.CurrentUser.CreateSubKey(MyRegisteredHomeOrNot))
                {
                    regKey.SetValue("ActionWhenComplete", (int)value, RegistryValueKind.DWord);
                }
            }
        }
    }

    public enum ExtractionComplete : int
    {
        DoNothing = 1,
        Prompt,
        Always
    }
}
