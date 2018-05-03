using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using VData_Explorer.Interop;

namespace VData_Explorer.Classes
{
    public class WpfFolderBrowserDialogEx : IDisposable //, IDialogControlHost
    {
        protected readonly Collection<string> fileNames;
        internal NativeDialogShowState showState = NativeDialogShowState.PreShow;
        protected ResourceManager stringManager;

        private IFileDialog nativeDialog;
        //        private IFileDialogCustomize nativeDialogCustomize;
        private bool? canceled;
        private Window parentWindow;

        protected const string IllegalPropertyChangeString = " cannot be changed while dialog is showing";

        #region Constructors

        public WpfFolderBrowserDialogEx()
        {
            fileNames = new Collection<string>();
            stringManager = new ResourceManager("en-US", Assembly.GetExecutingAssembly());
        }

        public WpfFolderBrowserDialogEx(string title) : this()
        {
            this.title = title;
        }

        #endregion

        // Template method to allow derived dialog to create actual
        // specific COM coclass (e.g. FileOpenDialog or FileSaveDialog)
        private NativeFileOpenDialog openDialogCoClass;


        internal IFileDialog GetNativeFileDialog()
        {
            Debug.Assert(openDialogCoClass != null,
                "Must call Initialize() before fetching dialog interface");
            return (IFileDialog)openDialogCoClass;
        }

        internal void InitializeNativeFileDialog()
        {
            openDialogCoClass = new NativeFileOpenDialog();
        }

        internal void CleanUpNativeFileDialog()
        {
            if (openDialogCoClass != null)
                Marshal.ReleaseComObject(openDialogCoClass);
        }

        internal void PopulateWithFileNames(Collection<string> names)
        {
            IShellItemArray resultsArray;
            uint count;
            if (names != null)
            {
                openDialogCoClass.GetResults(out resultsArray);
                resultsArray.GetCount(out count);

                names.Clear();
                for (int i = 0; i < count; i++)
                    names.Add(GetFileNameFromShellItem(GetShellItemAt(resultsArray, i)));

                if (count > 0)
                {
                    FileName = names[0];
                }
            }
        }

        static internal NativeMethods.FOS GetDerivedOptionFlags(NativeMethods.FOS flags)
        {

            flags |= NativeMethods.FOS.FOS_PICKFOLDERS;
            // TODO: other flags

            return flags;
        }


        #region Public API

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                if (NativeDialogShowing)
                    nativeDialog.SetTitle(value);
            }
        }

        // TODO: implement AddExtension
        private bool addExtension;
        internal bool AddExtension
        {
            get { return addExtension; }
            set { addExtension = value; }
        }

        // This is the first of many properties that are backed by the FOS_*
        // bitflag options set with IFileDialog.SetOptions(). SetOptions() fails
        // if called while dialog is showing (e.g. from a callback)
        private bool checkFileExists;
        internal bool CheckFileExists
        {
            get { return checkFileExists; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("CheckFileExists", CultureInfo.CurrentUICulture));
                checkFileExists = value;
            }
        }

        private bool checkPathExists;
        internal bool CheckPathExists
        {
            get { return checkPathExists; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("CheckPathExists", CultureInfo.CurrentUICulture));
                checkPathExists = value;
            }
        }

        private bool checkValidNames;
        internal bool CheckValidNames
        {
            get { return checkValidNames; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("CheckPathExists", CultureInfo.CurrentUICulture));
                checkValidNames = value;
            }
        }

        private bool checkReadOnly;
        internal bool CheckReadOnly
        {
            get { return checkReadOnly; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("CheckReadOnly", CultureInfo.CurrentUICulture));
                checkReadOnly = value;
            }
        }

        // TODO: Bizzare semantics bug here, needs resolution
        // semantics of FOS_NOCHANGEDIR, as the specs indicate that it has changed;
        // if so, we'll need to cache this ourselves
        private bool restoreDirectory;
        internal bool RestoreDirectory
        {
            get { return restoreDirectory; }
            set
            {
                // ThrowIfDialogShowing(stringManager.GetString("RestoreDirectory", CultureInfo.CurrentUICulture));
                restoreDirectory = value;
            }
        }

        private bool showPlacesList = true;
        public bool ShowPlacesList
        {

            get { return showPlacesList; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("ShowPlacesList", CultureInfo.CurrentUICulture));
                showPlacesList = value;
            }
        }

        private bool addToMruList = true;
        public bool AddToMruList
        {
            get { return addToMruList; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("AddToMruList", CultureInfo.CurrentUICulture));
                addToMruList = value;
            }
        }

        private bool showHiddenItems;
        public bool ShowHiddenItems
        {
            get { return showHiddenItems; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("ShowHiddenItems", CultureInfo.CurrentUICulture));
                showHiddenItems = value;
            }
        }

        // TODO: Implement property editing
        private bool allowPropertyEditing;
        internal bool AllowPropertyEditing
        {
            get { return allowPropertyEditing; }
            set { allowPropertyEditing = value; }
        }

        private bool dereferenceLinks;
        public bool DereferenceLinks
        {
            get { return dereferenceLinks; }
            set
            {
                ThrowIfDialogShowing(stringManager.GetString("DereferenceLinks", CultureInfo.CurrentUICulture));
                dereferenceLinks = value;
            }
        }

        private string fileName;
        public string FileName
        {
            get
            {
                CheckFileNamesAvailable();
                if (fileNames.Count > 1)
                    throw new InvalidOperationException("Multiple files selected - the FileNames property should be used instead");
                fileName = fileNames[0];
                return fileNames[0];
            }
            set
            {
                fileName = value;
            }
        }

        private string initialDirectory;
        public string InitialDirectory
        {
            get { return initialDirectory; }
            set { initialDirectory = value; }
        }

        public bool? ShowDialog(Window owner)
        {
            parentWindow = owner;
            return ShowDialog();
        }

        public bool? ShowDialog()
        {
            bool? result = null;

            try
            {
                // Fetch derived native dialog (i.e. Save or Open)

                InitializeNativeFileDialog();
                nativeDialog = GetNativeFileDialog();

                // Process custom controls, and validate overall state
                ProcessControls();
                ValidateCurrentDialogState();

                // Apply outer properties to native dialog instance
                ApplyNativeSettings(nativeDialog);

                // Show dialog
                showState = NativeDialogShowState.Showing;
                int hresult = nativeDialog.Show(GetHandleFromWindow(parentWindow));
                showState = NativeDialogShowState.Closed;

                // Create return information
                if (ErrorHelper.Matches(hresult, Win32ErrorCode.ERROR_CANCELLED))
                {
                    canceled = true;
                    fileNames.Clear();
                }
                else
                {
                    canceled = false;

                    // Populate filenames - though only if user didn't cancel
                    PopulateWithFileNames(fileNames);
                }
                result = !canceled.Value;
            }
            finally
            {
                CleanUpNativeFileDialog();
                showState = NativeDialogShowState.Closed;
            }
            return result;
        }


        #endregion

        #region Configuration

        private void ApplyNativeSettings(IFileDialog dialog)
        {
            Debug.Assert(dialog != null, "No dialog instance to configure");
            
            if (parentWindow == null)
                parentWindow = Interop.Helpers.GetDefaultOwnerWindow();

            // Apply option bitflags
            dialog.SetOptions(CalculateNativeDialogOptionFlags());

            // Other property sets
            dialog.SetTitle(title);

            // TODO: Implement other property sets

            string directory = (String.IsNullOrEmpty(fileName)) ? initialDirectory : System.IO.Path.GetDirectoryName(fileName);


            if (directory != null)
            {
                IShellItem folder;
                SHCreateItemFromParsingName(directory, IntPtr.Zero, new System.Guid(IIDGuid.IShellItem), out folder);

                if (folder != null)
                    dialog.SetFolder(folder);
            }


            if (!String.IsNullOrEmpty(fileName))
            {
                string name = System.IO.Path.GetFileName(fileName);
                dialog.SetFileName(name);
            }
        }

        private NativeMethods.FOS CalculateNativeDialogOptionFlags()
        {
            // We start with only a few flags set by default, then go from there based
            // on the current state of the managed dialog's property values
            NativeMethods.FOS flags =
                NativeMethods.FOS.FOS_NOTESTFILECREATE
                | NativeMethods.FOS.FOS_FORCEFILESYSTEM;

            // Call to derived (concrete) dialog to set dialog-specific flags
            flags = GetDerivedOptionFlags(flags);

            // Apply other optional flags
            if (checkFileExists)
                flags |= NativeMethods.FOS.FOS_FILEMUSTEXIST;
            if (checkPathExists)
                flags |= NativeMethods.FOS.FOS_PATHMUSTEXIST;
            if (!checkValidNames)
                flags |= NativeMethods.FOS.FOS_NOVALIDATE;
            if (!checkReadOnly)
                flags |= NativeMethods.FOS.FOS_NOREADONLYRETURN;
            if (restoreDirectory)
                flags |= NativeMethods.FOS.FOS_NOCHANGEDIR;
            if (!showPlacesList)
                flags |= NativeMethods.FOS.FOS_HIDEPINNEDPLACES;
            if (!addToMruList)
                flags |= NativeMethods.FOS.FOS_DONTADDTORECENT;
            if (showHiddenItems)
                flags |= NativeMethods.FOS.FOS_FORCESHOWHIDDEN;
            if (!dereferenceLinks)
                flags |= NativeMethods.FOS.FOS_NODEREFERENCELINKS;
            return flags;
        }

        static private void ValidateCurrentDialogState()
        {
            // TODO: Perform validation - both cross-property and pseudo-controls
        }

        static private void ProcessControls()
        {
            // TODO: Sort controls if necesarry - COM API might not require it, however
        }

        #endregion

        //#region IDialogControlHost Members

        //bool IDialogControlHost.IsCollectionChangeAllowed()
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        //void IDialogControlHost.ApplyCollectionChanged()
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        //bool IDialogControlHost.IsControlPropertyChangeAllowed(string propertyName, DialogControl control)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        //void IDialogControlHost.ApplyControlPropertyChange(string propertyName, DialogControl control)
        //{
        //    throw new Exception("The method or operation is not implemented.");
        //}

        //#endregion

        #region Helpers

        protected void CheckFileNamesAvailable()
        {
            if (showState != NativeDialogShowState.Closed)
                throw new InvalidOperationException("Filename not available - dialog has not closed yet");
            if (canceled.GetValueOrDefault())
                throw new InvalidOperationException("Filename not available - dialog was canceled");
            Debug.Assert(fileNames.Count != 0,
                    "FileNames empty - shouldn't happen dialog unless dialog canceled or not yet shown");
        }

        static private IntPtr GetHandleFromWindow(Window window)
        {
            if (window == null)
                return NativeMethods.NO_PARENT;
            return (new WindowInteropHelper(window)).Handle;
        }

        static private bool IsOptionSet(IFileDialog dialog, NativeMethods.FOS flag)
        {
            NativeMethods.FOS currentFlags = GetCurrentOptionFlags(dialog);

            return (currentFlags & flag) == flag;
        }

        static internal NativeMethods.FOS GetCurrentOptionFlags(IFileDialog dialog)
        {
            NativeMethods.FOS currentFlags;
            dialog.GetOptions(out currentFlags);
            return currentFlags;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszPath,
        [In] IntPtr pbc,
        [In, MarshalAs(UnmanagedType.LPStruct)] Guid iIdIShellItem,
        [Out, MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItem iShellItem);

        #endregion

        #region Helpers

        private bool NativeDialogShowing
        {
            get
            {
                return (nativeDialog != null)
                    && (showState == NativeDialogShowState.Showing ||
                    showState == NativeDialogShowState.Closing);
            }
        }

        static internal string GetFileNameFromShellItem(IShellItem item)
        {
            string filename;
            item.GetDisplayName(NativeMethods.SIGDN.SIGDN_DESKTOPABSOLUTEPARSING, out filename);
            return filename;
        }

        static internal IShellItem GetShellItemAt(IShellItemArray array, int i)
        {
            IShellItem result;
            var index = (uint)i;
            array.GetItemAt(index, out result);
            return result;
        }

        protected void ThrowIfDialogShowing(string message)
        {
            //if (NativeDialogShowing)
            //    throw new NotSupportedException(message);
        }

        #endregion

        #region IDisposable Members

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~WpfFolderBrowserDialogEx()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources
                //if (managedResource != null) {
                //	managedResource.Dispose();
                //	managedResource = null;
                //}
            }
            // free native resources if there are any.
            //if (nativeResource != IntPtr.Zero) {
            //	Marshal.FreeHGlobal(nativeResource);
            //	nativeResource = IntPtr.Zero;
            //}
        }

        #endregion

        #region Event handling members

        protected virtual void OnFileOk(CancelEventArgs e)
        {
            //CancelEventHandler handler = FileOk;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
        }

        //protected virtual void OnFolderChanging(CommonFileDialogFolderChangeEventArgs e)
        //{
        //    //EventHandler<CommonFileDialogFolderChangeEventArgs> handler = FolderChanging;
        //    //if (handler != null)
        //    //{
        //    //    handler(this, e);
        //    //}
        //}

        protected virtual void OnFolderChanged(EventArgs e)
        {
            //EventHandler handler = FolderChanged;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
        }

        protected virtual void OnSelectionChanged(EventArgs e)
        {
            //EventHandler handler = SelectionChanged;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
        }

        protected virtual void OnFileTypeChanged(EventArgs e)
        {
            //EventHandler handler = FileTypeChanged;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}   
        }

        protected virtual void OnOpening(EventArgs e)
        {
            //EventHandler handler = Opening;
            //if (handler != null)
            //{
            //    handler(this, e);
            //}
        }

        #endregion
    }
}
