using System;
using VData_Explorer.Classes;
using System.Collections.Generic;
using System.ComponentModel;

namespace VData_Explorer.Helpers
{
    internal static class Delegates
    {
        internal delegate void ErrorDisplay(Exception ex);
        internal delegate void UpdateView(DirectoryList sender, IList<ItemViewModel> obj);
        internal delegate void ProgressBarValue(double value);
        internal delegate void ProgressBarText(string value);
        internal delegate void CancelEventInvoke(CancelEventArgs e);
        internal delegate void JustAction();
    }
}
