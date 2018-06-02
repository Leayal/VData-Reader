using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Lamie.LibVData;
using Microsoft.Win32;
using static VData_Explorer.Helpers.Delegates;
using VData_Explorer.Classes;
using SharpCompress.Common.Zip;
using System.Windows.Threading;
using System.Windows.Data;
using System.Threading;
using System.Reflection;

namespace VData_Explorer.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        const char replacedinvalidcharacterpath = '-';
        private static readonly char[] pathsplitters = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        private static readonly char[] invalidpathchars = Path.GetInvalidFileNameChars();
        const string DirectoryRoot = "<Root>";
        const string ThisWindowTitle = "VData Explorer";
        private VFile archive;
        private CancellationTokenSource isWorking;
        private TreeItemsList viewer;
        private string currentfullpath;

        public MainWindow()
        {
            this.archive = null;
            this.isWorking = null;
            using (Stream resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VData_Explorer.Resources.WindowIcon.png"))
            {
                if (resStream != null)
                {
                    BitmapImage img = new BitmapImage();
                    img.BeginInit();
                    img.StreamSource = resStream;
                    img.DecodePixelHeight = 44;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                    img.Freeze();
                    this.Icon = img;
                }
            }

            InitializeComponent();

            this.windowIcon.Source = this.Icon;
            this.Title = ThisWindowTitle;

            this.Loaded += this.MainWindow_Loaded;
            this.Closing += this.MainWindow_Closing;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
            {
                if (!string.IsNullOrWhiteSpace(args[1]) && File.Exists(args[1]))
                    await this.OpenArchive(args[1]);
            }
        }

        private async void TabNofile_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                // Assuming you have one file that you care about, pass it off to whatever
                // handling code you have defined.
                if (files != null && files.Length > 0)
                {
                    if (!string.IsNullOrWhiteSpace(files[0]) && File.Exists(files[0]))
                        await this.OpenArchive(files[0]);
                }
            }
        }

        private async void OpenCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await this.OpenArchive(null);
        }

        private async void CmdClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.isWorking == null || (await this.ShowMessageAsync("Question", "Are you sure you want to cancel the current operation and exit the application?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { DialogResultOnCancel = MessageDialogResult.Negative, AffirmativeButtonText = "Yes", NegativeButtonText = "No" }) == MessageDialogResult.Affirmative))
            {
                await this.CloseArchive();
            }
        }

        private void CmdExit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        public async Task OpenArchive(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                OpenFileDialog ofd = new OpenFileDialog();
                string lastLocation = Settings.LastFileLocation;
                if (!string.IsNullOrWhiteSpace(lastLocation) && Directory.Exists(lastLocation))
                    ofd.InitialDirectory = lastLocation;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.DefaultExt = "v";
                ofd.Multiselect = false;
                ofd.Filter = "SoulWorker Data File|*.v";
                ofd.Title = "Select file to open";
                if (ofd.ShowDialog(this) == true)
                {
                    filepath = ofd.FileName;
                    Settings.LastFileLocation = Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(filepath);
                }
                else
                    return;
            }
            else
            {
                filepath = Path.GetFullPath(filepath);
            }

            if (string.Equals(filepath, this.currentfullpath, StringComparison.OrdinalIgnoreCase)) return;
            this.currentfullpath = filepath;

            await this.CloseArchive();

            this.isWorking = null;
            this.tabLoading.IsSelected = true;
            FileStream fs = null;
            try
            {
                var typeface = new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch);
                double fontsize = this.FontSize;

                fs = File.OpenRead(filepath);

                bool justabool = await Task.Run(() => { return VFile.IsValid(fs); });
                if (justabool)
                {
                    ZipEntry testitem = await Task.Run(() => 
                    {
                        this.archive = VFile.Read(fs);
                        var item = this.archive.FirstOrDefault(entry => (!entry.IsDirectory && entry.IsEncrypted && (entry.CompressedSize > 0)));
                        if (item != null)
                            this.archive.LeaveBaseStreamOpen = true;
                        return item;
                    });
                    if (testitem != null)
                    {
                        justabool = true;
                        var alwihgliawhg = new MetroDialogSettings() { DefaultText = "Password?" };
                        PasswordPromptDialog promptDialog = new PasswordPromptDialog(this, alwihgliawhg) { Description = "The file has password-protected encryption." };
                        await this.ShowMetroDialogAsync(promptDialog, promptDialog.DialogSettings);
                        await promptDialog.WaitUntilUnloadedAsync();
                        string pw = promptDialog.Password; // await this.ShowInputAsync("Password", "The file has password-protected encryption.", alwihgliawhg);
                        while (justabool)
                        {
                            if (string.IsNullOrEmpty(pw))
                            {
                                throw new SharpCompress.Common.CryptographicException("Cannot open the file because of the password protection.");
                            }
                            else
                            {
                                await Task.Run(() =>
                                {
                                    this.archive.Dispose();
                                    this.archive = VFile.Read(fs, pw, true);
                                    testitem = this.archive.FirstOrDefault(entry => (!entry.IsDirectory && entry.IsEncrypted && (entry.CompressedSize > 0)));
                                    try
                                    {
                                        using (Stream entryStream = this.archive.GetEntryStream(testitem))
                                        {
                                            string[] oldpw = Settings.UsedPassword;
                                            if (oldpw == null || oldpw.Length == 0)
                                            {
                                                Settings.UsedPassword = new string[] { pw };
                                            }
                                            else
                                            {
                                                int foundindex = Array.IndexOf(oldpw, pw);
                                                if (foundindex == -1)
                                                {
                                                    string[] newpw = new string[oldpw.Length + 1];
                                                    newpw[0] = pw;
                                                    Array.Copy(oldpw, 0, newpw, 1, oldpw.Length);
                                                    Settings.UsedPassword = newpw;
                                                }
                                                else if (foundindex != 0)
                                                {
                                                    for (int i = foundindex; i > 0; i--)
                                                        oldpw[i] = oldpw[i - 1];
                                                    oldpw[0] = pw;
                                                    Settings.UsedPassword = oldpw;
                                                }
                                            }
                                            justabool = false;
                                        }
                                    }
                                    catch (SharpCompress.Common.CryptographicException)
                                    {
                                        // Wrong password which make CryptographicException occurered
                                    }
                                });
                            }
                            if (justabool)
                            {
                                promptDialog.Description = "The password did not match.";
                                await this.ShowMetroDialogAsync(promptDialog, promptDialog.DialogSettings);
                                await promptDialog.WaitUntilUnloadedAsync();
                                pw = promptDialog.Password; // await this.ShowInputAsync("Password", "The password did not match.", alwihgliawhg);
                            }
                        }
                        this.archive.LeaveBaseStreamOpen = false;
                    }

                    var huh = await Task.Run(() =>
                    {
                        this.CreateView(this.archive, typeface, fontsize);
                        List<string> meh = new List<string>(this.viewer.Directories.Count);
                        meh.AddRange(this.viewer.Directories.Keys);
                        meh.Sort(StringComparer.Ordinal);
                        meh[0] = DirectoryRoot;
                        return meh;
                    });

                    bool huhNotEmpty = (huh.Count != 0);

                    if (huhNotEmpty)
                        this.addressbar.ItemsSource = new ListCollectionView(huh);
                    this.Title = ThisWindowTitle + ": " + Path.GetFileName(filepath);
                    this.tabList.IsSelected = true;
                    this.addressbar.SelectedIndex = 0;
                    this.Addressbar_SelectionChanged(this.addressbar, new SelectionChangedEventArgs(ComboBox.SelectionChangedEvent, new string[0], new string[] { huh[0] }));

                    if (huhNotEmpty)
                    {
                        await this.filelist.Dispatcher.BeginInvoke(new JustAction(() =>
                        {
                            this.filelist.UpdateLayout();
                            var item = this.filelist.ItemContainerGenerator.ContainerFromIndex(0);
                            if (item is ListBoxItem listboxitem)
                            {
                                listboxitem.IsSelected = true;
                                listboxitem.Focus();
                            }
                        }), DispatcherPriority.Loaded, null);
                    }
                }
                else
                {
                    fs.Dispose();
                    fs = null;
                    throw new InvalidDataException();
                }
            }
            catch (Exception ex)
            {
                if (fs != null)
                    fs.Dispose();
                await this.CloseArchive();
                await this.ShowMessageAsync("Error", "Failed to open the file as VFile:\n" + ex.Message);
            }
        }

        public void CreateView(VFile file, Typeface typeface, double fontsize)
        {
            if (this.viewer != null)
            {
                this.viewer.Clear();
                this.viewer.ViewReset -= this.Viewer_ViewReset;
            }
            this.viewer = new TreeItemsList(file, typeface, fontsize);
            this.viewer.ViewReset += this.Viewer_ViewReset;
            this.viewer.Init();
        }

        private void Viewer_ViewReset(DirectoryList sender, IList<ItemViewModel> obj)
        {
            this.filelist.Dispatcher.BeginInvoke(new UpdateView((x, y) =>
            {
                this.addressbar.Text = x.Fullname;
                this.ignoreSelection = true;
                this.addressbar.SelectedItem = x.Fullname;
                this.ignoreSelection = false;
                if (x.Files.Count == 0)
                {
                    this.colSize.Visibility = Visibility.Collapsed;
                    this.colCompressedSize.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.colSize.Width = x.LongestSize;
                    this.colCompressedSize.Width = x.LongestCompressedSize;
                    this.colSize.Visibility = Visibility.Visible;
                    this.colCompressedSize.Visibility = Visibility.Visible;
                }
                this.filelist.ItemsSource = y;
                if (y.Count > 0)
                {
                    this.filelist.ScrollIntoView(y[0]);
                }
            }), DispatcherPriority.Normal, sender, obj);
        }

        public Task<bool> CloseArchive()
        {
            if (this.archive == null)
            {
                this.tabNofile.IsSelected = true;
                return Task.FromResult(true);
            }
            if (this.isWorking == null)
            {
                this.tabNofile.IsSelected = true;
                var tmp = this.archive;
                this.archive = null;
                if (this.viewer != null)
                {
                    this.viewer.Clear();
                    this.viewer.ViewReset -= this.Viewer_ViewReset;
                    this.viewer = null;
                }
                this.addressbar.ItemsSource = null;
                this.Title = ThisWindowTitle;
                tmp.Dispose();
                return Task.FromResult(true);
            }
            else
            {
                TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
                this.isWorking.Token.Register((x) => 
                {
                    TaskCompletionSource<bool> t = (TaskCompletionSource<bool>)x;
                    this.tabNofile.IsSelected = true;
                    var tmp = this.archive;
                    this.archive = null;
                    if (this.viewer != null)
                    {
                        this.viewer.Clear();
                        this.viewer.ViewReset -= this.Viewer_ViewReset;
                        this.viewer = null;
                    }
                    this.addressbar.ItemsSource = null;
                    this.Title = ThisWindowTitle;
                    tmp.Dispose();
                    t.SetResult(true);
                }, task, true);
                this.isWorking.Cancel();
                return task.Task;
            }
        }

        private async void ButtonCancelExtracting_Click(object sender, RoutedEventArgs e)
        {
            if (this.isWorking != null)
            {
                if (await this.ShowMessageAsync("Question", "Are you sure you want to cancel the current operation?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { DialogResultOnCancel = MessageDialogResult.Negative, AffirmativeButtonText = "Yes", NegativeButtonText = "No" }) == MessageDialogResult.Affirmative)
                {
                    this.isWorking.Cancel();
                }
            }
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.isWorking == null)
            {
                await this.CloseArchive();
                return;
            }
            e.Cancel = true;
            if (await this.ShowMessageAsync("Question", "Are you sure you want to cancel the current operation and exit the application?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { DialogResultOnCancel = MessageDialogResult.Negative, AffirmativeButtonText = "Yes", NegativeButtonText = "No" }) == MessageDialogResult.Affirmative)
            {
                await this.CloseArchive();
                await this.Dispatcher.BeginInvoke(new JustAction(() => { this.Close(); }), DispatcherPriority.Normal, null);
            }
        }

        private void ListItem_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                ItemViewDirectory model = item.DataContext as ItemViewDirectory;
                if (model != null)
                    model.DoubleClicked();
            }
        }

        private void ListBoxItem_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                var yes = ((MenuItem)item.ContextMenu.Items[0]);
                ItemViewDirectory model = item.DataContext as ItemViewDirectory;
                if (model != null)
                {
                    if (model.IsExcluded && model.Filename == "..")
                    {
                        yes.Header = "Go to parent folder";
                    }
                    else
                    {
                        yes.Header = "Open Folder";
                    }
                    yes.Visibility = Visibility.Visible;
                }
                else
                {
                    yes.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void MenuItemOpen_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            ItemViewDirectory model = item.DataContext as ItemViewDirectory;
            if (model != null)
            {
                model.DoubleClicked();
            }
        }

        private void Filelist_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                this.headercolumns.MaxWidth = (this.filelist.ActualWidth - 28);
        }

        private void Addressbar_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!this.tabList.IsSelected) return;
            if (e.Key == Key.Enter)
            {
                if (!e.IsRepeat)
                {
                    e.Handled = true;
                    ComboBox comboBox = (ComboBox)sender;
                    ListCollectionView view = (ListCollectionView)comboBox.ItemsSource;
                    List<string> src = (List<string>)view.SourceCollection;
                    for (int i = 0; i < src.Count; i++)
                        if (StringComparer.OrdinalIgnoreCase.Equals(comboBox.Text, src[i]))
                        {
                            comboBox.SelectedItem = src[i];
                            return;
                        }
                    comboBox.Text = this.viewer.CurrentDirectory.Fullname;
                    this.Dispatcher.BeginInvoke(new JustAction(async () =>
                    {
                        await this.ShowMessageAsync("Warning", "The path you provided is not existed.");
                    }), DispatcherPriority.Normal, null);
                }
            }
        }

        private bool ignoreSelection;
        private void Addressbar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!this.tabList.IsSelected || this.viewer == null) return;
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                string path = (string)e.AddedItems[0];
                ComboBox combobox = (ComboBox)sender;
                if (this.ignoreSelection) return;
                if (path == DirectoryRoot)
                    this.viewer.SelectDirectory(string.Empty);
                else
                    this.viewer.SelectDirectory(path);
            }
        }

        private async void CmdExtractSelected_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.isWorking != null || !this.tabList.IsSelected) return;
            if (this.filelist.SelectedItems.Count == 0) return;

            if ((this.filelist.SelectedItems.Count == 1) && (this.filelist.SelectedItem is ItemViewFile item))
            {
                SaveFileDialog sfd = new SaveFileDialog();
                string ext = Path.GetExtension(item.Filename);
                if (ext.StartsWith(".", StringComparison.OrdinalIgnoreCase))
                    ext = ext.Remove(0, 1);
                sfd.Title = "Select the destination to save the file";
                sfd.Filter = ext.ToUpper() + " File|*." + ext;
                if (TryNormallizePath(item.Filename, out var normalizedpath))
                    sfd.FileName = normalizedpath;
                else
                    sfd.FileName = item.Filename;
                sfd.DefaultExt = ext;
                sfd.OverwritePrompt = true;
                string lastpath = Settings.LastFileExtractLocation;
                if (!string.IsNullOrWhiteSpace(lastpath))
                    sfd.InitialDirectory = lastpath;
                sfd.CheckPathExists = true;
                sfd.CheckFileExists = false;
                if (sfd.ShowDialog(this) == true)
                {
                    string parentPath = Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(sfd.FileName);
                    Settings.LastFileExtractLocation = parentPath;
                    this.isWorking = this.CreateIsWorking();
                    this.mainProgressbar.IsIndeterminate = true;
                    this.mainProgressbar.Value = 0;
                    this.tabExtracting.IsSelected = true;
                    this.mainProgressText.Text = "Preparing";
                    ExtractionComplete actionWhenCompleted = Settings.ActionWhenComplete;
                    bool autocorrectpath = Settings.EnableAutoCorrectPath;
                    try
                    {
                        await Task.Run(() =>
                        {
                            bool hasProgress = item.Size.HasValue;
                            this.mainProgressbar.Dispatcher.Invoke(() =>
                            {
                                this.mainProgressbar.Maximum = hasProgress ? 1 : item.Size.Value;
                                this.mainProgressbar.IsIndeterminate = false;
                            });
                            Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(parentPath);
                            using (Stream entryStream = this.archive.GetEntryStream(item.Entry))
                            using (FileStream fs = File.Create(sfd.FileName))
                            {
                                this.mainProgressText.Dispatcher.BeginInvoke(new ProgressBarText((val) =>
                                {
                                    this.mainProgressText.Text = val;
                                }), DispatcherPriority.Normal, $"Extracting: {item.Key} (1/1)");
                                byte[] buffer = new byte[4096];
                                double progressVal = 0;
                                int readbyte = entryStream.Read(buffer, 0, buffer.Length);
                                while (readbyte > 0)
                                {
                                    if (this.isWorking.IsCancellationRequested)
                                    {
                                        fs.Dispose();
                                        try { File.Delete(sfd.FileName); } catch { }
                                        break;
                                    }
                                    fs.Write(buffer, 0, readbyte);
                                    if (hasProgress)
                                    {
                                        progressVal += readbyte;
                                        this.mainProgressbar.Dispatcher.BeginInvoke(new ProgressBarValue((val) =>
                                        {
                                            this.mainProgressbar.Value = val;
                                        }), DispatcherPriority.Normal, progressVal);
                                    }
                                    readbyte = entryStream.Read(buffer, 0, buffer.Length);
                                }
                            }
                        });

                        switch (actionWhenCompleted)
                        {
                            case ExtractionComplete.Prompt:
                                if (await this.ShowMessageAsync("Question", "Show file in the destination directory?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Open folder", NegativeButtonText = "Cancel" }) == MessageDialogResult.Affirmative)
                                {
                                    Interop.Helpers.ShowFolder(sfd.FileName);
                                }
                                break;
                            case ExtractionComplete.Always:
                                Interop.Helpers.ShowFolder(sfd.FileName);
                                break;
                        }

                        this.isWorking = null;
                    }
                    catch (Exception ex)
                    {
                        this.isWorking = null;
                        MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if (this.archive != null)
                        this.tabList.IsSelected = true;
                }
            }
            else
            {
                if ((this.filelist.SelectedItems.Count == 1) && (this.filelist.SelectedItem is ItemViewDirectory folder))
                {
                    if (folder.IsExcluded)
                        return;
                }
                using (WpfFolderBrowserDialogEx dialog = new WpfFolderBrowserDialogEx("Select destination folder to extract"))
                {
                    string lastLocation = Settings.LastExtractLocation;
                    if (!string.IsNullOrWhiteSpace(lastLocation) && Directory.Exists(lastLocation))
                        dialog.InitialDirectory = lastLocation;
                    if (dialog.ShowDialog(this) == true)
                    {
                        Settings.LastExtractLocation = dialog.FileName;
                        Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(dialog.FileName);

                        this.isWorking = this.CreateIsWorking();
                        this.mainProgressbar.IsIndeterminate = true;
                        this.mainProgressbar.Value = 0;
                        this.tabExtracting.IsSelected = true;
                        this.mainProgressText.Text = "Preparing";
                        ExtractionComplete actionWhenCompleted = Settings.ActionWhenComplete;
                        bool autocorrectpath = Settings.EnableAutoCorrectPath;
                        try
                        {
                            MetroDialogSettings inputdialogsettings = new MetroDialogSettings() { AffirmativeButtonText = "OK", NegativeButtonText = "Skip file" };
                            string impossibru = this.viewer.CurrentDirectory.Fullname;
                            var targeted = this.filelist.SelectedItems;
                            // Impossible to equals to DirectoryRoot
                            if (impossibru == DirectoryRoot)
                                impossibru = string.Empty;
                            else
                                impossibru = impossibru.Remove(0, DirectoryRoot.Length + 1);
                            await Task.Run(() =>
                            {
                                List<ZipEntry> entries = new List<ZipEntry>(Math.Max(targeted.Count, 16));
                                foreach (ItemViewModel model in targeted)
                                {
                                    if (model is ItemViewDirectory dir)
                                    {
                                        if (!dir.IsExcluded)
                                            entries.AddRange(TreeItemsList.GetEntriesFromDirectory(dir.Entry));
                                    }
                                    else if (model is ItemViewFile file)
                                    {
                                        entries.Add(file.Entry);
                                    }
                                }

                                int count = entries.Count, current = 0;
                                this.mainProgressbar.Dispatcher.Invoke(new ProgressBarValue((val) =>
                                {
                                    this.mainProgressbar.Maximum = val;
                                    this.mainProgressbar.IsIndeterminate = false;
                                }), DispatcherPriority.Normal, count);
                                string filepath, filepath2, entrypath;
                                int byteread;
                                byte[] buffer = new byte[4096];
                                bool isfullpath;
                                foreach (ZipEntry entry in entries)
                                {
                                    if (this.isWorking.IsCancellationRequested)
                                        break;
                                    current++;

                                    if (impossibru.Length == 0)
                                        entrypath = entry.Key;
                                    else
                                        entrypath = entry.Key.Remove(0, impossibru.Length + 1);
                                    filepath = Path.Combine(dialog.FileName, entrypath);
                                    if (entrypath.IndexOf(Path.DirectorySeparatorChar) == -1 && entrypath.IndexOf(Path.AltDirectorySeparatorChar) == -1)
                                    {
                                        if (TryNormallizePath(entrypath, out filepath2))
                                        {
                                            if (autocorrectpath)
                                            {
                                                filepath = Path.Combine(dialog.FileName, filepath2);
                                            }
                                            else
                                            {
                                                inputdialogsettings.DefaultText = filepath2;
                                                filepath2 = this.mainProgressbar.Dispatcher.Invoke(async () =>
                                                {
                                                    string asdasdasd = await this.ShowInputAsync("Invalid file path", $"\"{entrypath}\" is not a valid path.\nPlease input the new filename.", inputdialogsettings);
                                                    return asdasdasd;
                                                }).Result;
                                                if (string.IsNullOrWhiteSpace(filepath2))
                                                    continue;
                                                else
                                                    filepath = Path.Combine(dialog.FileName, filepath2);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        string[] asdasd = entrypath.Split(pathsplitters, StringSplitOptions.RemoveEmptyEntries);
                                        if (TryNormallizePath(asdasd, out filepath2, out isfullpath))
                                        {
                                            if (isfullpath)
                                            {
                                                if (autocorrectpath)
                                                {
                                                    filepath = Path.Combine(dialog.FileName, filepath2);
                                                }
                                                else
                                                {
                                                    inputdialogsettings.DefaultText = filepath2;
                                                    filepath2 = this.mainProgressbar.Dispatcher.Invoke(async () =>
                                                    {
                                                        string asdasdasd = await this.ShowInputAsync("Invalid file path", $"\"{entrypath}\" is not a valid path.\nPlease input the new file path.", inputdialogsettings);
                                                        return asdasdasd;
                                                    }).Result;
                                                    if (string.IsNullOrWhiteSpace(filepath2))
                                                        continue;
                                                    else
                                                        filepath = Path.Combine(dialog.FileName, filepath2);
                                                }
                                            }
                                            else
                                            {
                                                string[] evennewitem = new string[asdasd.Length + 1];
                                                int countexceptlastindex = asdasd.Length - 1;
                                                for (int i = 0; i < countexceptlastindex; i++)
                                                {
                                                    evennewitem[1 + i] = asdasd[i];
                                                }
                                                if (autocorrectpath)
                                                {
                                                    evennewitem[0] = dialog.FileName;
                                                    evennewitem[evennewitem.Length - 1] = filepath2;
                                                    filepath = Path.Combine(evennewitem);
                                                }
                                                else
                                                {
                                                    inputdialogsettings.DefaultText = filepath2;
                                                    filepath2 = this.mainProgressbar.Dispatcher.Invoke(async () =>
                                                    {
                                                        string asdasdasd = await this.ShowInputAsync("Invalid file path", $"\"{entrypath}\" is not a valid path.\nPlease input the new filename.\nFolder: {string.Join(Path.DirectorySeparatorChar.ToString(), evennewitem).Trim(pathsplitters)}", inputdialogsettings);
                                                        return asdasdasd;
                                                    }).Result;
                                                    if (string.IsNullOrWhiteSpace(filepath2))
                                                        continue;
                                                    else
                                                    {
                                                        evennewitem[0] = dialog.FileName;
                                                        evennewitem[evennewitem.Length - 1] = filepath2;
                                                        filepath = Path.Combine(evennewitem);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    
                                    Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(filepath));
                                    using (Stream entryStream = this.archive.GetEntryStream(entry))
                                    using (FileStream fs = File.Create(filepath))
                                    {
                                        if (this.isWorking.IsCancellationRequested)
                                            break;
                                        byteread = entryStream.Read(buffer, 0, buffer.Length);
                                        while (byteread > 0)
                                        {
                                            if (this.isWorking.IsCancellationRequested)
                                            {
                                                fs.Dispose();
                                                try { File.Delete(filepath); } catch { }
                                                break;
                                            }
                                            fs.Write(buffer, 0, byteread);
                                            byteread = entryStream.Read(buffer, 0, buffer.Length);
                                        }
                                        fs.Flush();
                                    }
                                    this.mainProgressbar.Dispatcher.BeginInvoke(new ProgressBarValue((val) =>
                                    {
                                        this.mainProgressbar.Value = val;
                                    }), DispatcherPriority.Normal, current);
                                    this.mainProgressText.Dispatcher.BeginInvoke(new ProgressBarText((val) =>
                                    {
                                        this.mainProgressText.Text = val;
                                    }), DispatcherPriority.Normal, $"Extracting: {entry.Key} ({current}/{count})");
                                }
                            });

                            switch (actionWhenCompleted)
                            {
                                case ExtractionComplete.Prompt:
                                    if (await this.ShowMessageAsync("Question", "Open destination directory?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Open folder", NegativeButtonText = "Cancel" }) == MessageDialogResult.Affirmative)
                                    {
                                        System.Diagnostics.Process.Start(dialog.FileName);
                                        // destinationFolder
                                    }
                                    break;
                                case ExtractionComplete.Always:
                                    System.Diagnostics.Process.Start(dialog.FileName);
                                    break;
                            }

                            this.isWorking = null;
                        }
                        catch (Exception ex)
                        {
                            this.isWorking = null;
                            MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        if (this.archive != null)
                            this.tabList.IsSelected = true;
                    }
                }
            }
        }

        private async void CmdExtractAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.isWorking != null || !this.tabList.IsSelected) return;
            using (WpfFolderBrowserDialogEx dialog = new WpfFolderBrowserDialogEx("Select destination folder to extract"))
            {
                string lastLocation = Settings.LastExtractLocation;
                if (!string.IsNullOrWhiteSpace(lastLocation) && Directory.Exists(lastLocation))
                    dialog.InitialDirectory = lastLocation;
                if (dialog.ShowDialog(this) == true)
                {
                    Settings.LastExtractLocation = dialog.FileName;
                    Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(dialog.FileName);
                    await this.ExtractAll(dialog.FileName);
                }
            }
        }

        private CancellationTokenSource CreateIsWorking()
        {
            if (this.isWorking != null) return this.isWorking;
            var eh = new CancellationTokenSource();
            return eh;
        }

        private async Task ExtractAll(string destinationFolder)
        {
            if (this.isWorking != null) return;
            this.isWorking = this.CreateIsWorking();
            this.mainProgressbar.IsIndeterminate = true;
            this.mainProgressbar.Value = 0;
            this.tabExtracting.IsSelected = true;
            this.mainProgressText.Text = "Preparing";
            ExtractionComplete actionWhenCompleted = Settings.ActionWhenComplete;
            try
            {
                await Task.Run(() =>
                {
                    bool autocorrectpath = Settings.EnableAutoCorrectPath;
                    int count = this.archive.EntryCount, current = 0;
                    this.mainProgressbar.Dispatcher.Invoke(new ProgressBarValue((val) =>
                    {
                        this.mainProgressbar.Maximum = val;
                        this.mainProgressbar.IsIndeterminate = false;
                    }), DispatcherPriority.Normal, count);
                    string filepath, filepath2;
                    byte[] buffer = new byte[4096];
                    MetroDialogSettings inputdialogsettings = new MetroDialogSettings() { AffirmativeButtonText = "OK", NegativeButtonText = "Skip file" };
                    int byteread;
                    bool isfullpath;
                    IEnumerable<ZipEntry> walker = this.archive;
                    foreach (ZipEntry entry in walker)
                    {
                        if (this.isWorking.IsCancellationRequested)
                            break;
                        current++;
                        if (!entry.IsDirectory)
                        {
                            filepath = Path.Combine(destinationFolder, entry.Key);
                            if (entry.Key.IndexOf(Path.DirectorySeparatorChar) == -1 && entry.Key.IndexOf(Path.AltDirectorySeparatorChar) == -1)
                            {
                                if (TryNormallizePath(entry.Key, out filepath2))
                                {
                                    if (autocorrectpath)
                                    {
                                        filepath = Path.Combine(destinationFolder, filepath2);
                                    }
                                    else
                                    {
                                        inputdialogsettings.DefaultText = filepath2;
                                        filepath2 = this.mainProgressbar.Dispatcher.Invoke(async () =>
                                        {
                                            string asdasdasd = await this.ShowInputAsync("Invalid file path", $"\"{entry.Key}\" is not a valid path.\nPlease input the new filename.", inputdialogsettings);
                                            return asdasdasd;
                                        }).Result;
                                        if (string.IsNullOrWhiteSpace(filepath2))
                                            continue;
                                        else
                                            filepath = Path.Combine(destinationFolder, filepath2);
                                    }
                                }
                            }
                            else
                            {
                                string[] asdasd = entry.Key.Split(pathsplitters, StringSplitOptions.RemoveEmptyEntries);
                                if (TryNormallizePath(asdasd, out filepath2, out isfullpath))
                                {
                                    if (isfullpath)
                                    {
                                        if (autocorrectpath)
                                        {
                                            filepath = Path.Combine(destinationFolder, filepath2);
                                        }
                                        else
                                        {
                                            inputdialogsettings.DefaultText = filepath2;
                                            filepath2 = this.mainProgressbar.Dispatcher.Invoke(async () =>
                                            {
                                                string asdasdasd = await this.ShowInputAsync("Invalid file path", $"\"{entry.Key}\" is not a valid path.\nPlease input the new file path.", inputdialogsettings);
                                                return asdasdasd;
                                            }).Result;
                                            if (string.IsNullOrWhiteSpace(filepath2))
                                                continue;
                                            else
                                                filepath = Path.Combine(destinationFolder, filepath2);
                                        }
                                    }
                                    else
                                    {
                                        string[] evennewitem = new string[asdasd.Length + 1];
                                        int countexceptlastindex = asdasd.Length - 1;
                                        for (int i = 0; i < countexceptlastindex; i++)
                                        {
                                            evennewitem[1 + i] = asdasd[i];
                                        }
                                        if (autocorrectpath)
                                        {
                                            evennewitem[0] = destinationFolder;
                                            evennewitem[evennewitem.Length - 1] = filepath2;
                                            filepath = Path.Combine(evennewitem);
                                        }
                                        else
                                        {
                                            inputdialogsettings.DefaultText = filepath2;
                                            filepath2 = this.mainProgressbar.Dispatcher.Invoke(async () =>
                                            {
                                                string asdasdasd = await this.ShowInputAsync("Invalid file path", $"\"{entry.Key}\" is not a valid path.\nPlease input the new filename.\nFolder: {string.Join(Path.DirectorySeparatorChar.ToString(), evennewitem).Trim(pathsplitters)}", inputdialogsettings);
                                                return asdasdasd;
                                            }).Result;
                                            if (string.IsNullOrWhiteSpace(filepath2))
                                                continue;
                                            else
                                            {
                                                evennewitem[0] = destinationFolder;
                                                evennewitem[evennewitem.Length - 1] = filepath2;
                                                filepath = Path.Combine(evennewitem);
                                            }
                                        }
                                    }
                                }
                            }
                            Microsoft.VisualBasic.FileIO.FileSystem.CreateDirectory(Microsoft.VisualBasic.FileIO.FileSystem.GetParentPath(filepath));
                            using (Stream entryStream = this.archive.GetEntryStream(entry))
                            using (FileStream fs = File.Create(filepath))
                            {
                                if (this.isWorking.IsCancellationRequested)
                                    break;
                                byteread = entryStream.Read(buffer, 0, buffer.Length);
                                while (byteread > 0)
                                {
                                    if (this.isWorking.IsCancellationRequested)
                                    {
                                        fs.Dispose();
                                        try { File.Delete(filepath); } catch { }
                                        break;
                                    }
                                    fs.Write(buffer, 0, byteread);
                                    byteread = entryStream.Read(buffer, 0, buffer.Length);
                                }
                                fs.Flush();
                            }
                            this.mainProgressbar.Dispatcher.BeginInvoke(new ProgressBarValue((val) =>
                            {
                                this.mainProgressbar.Value = val;
                            }), DispatcherPriority.Normal, current);
                            this.mainProgressText.Dispatcher.BeginInvoke(new ProgressBarText((val) =>
                            {
                                this.mainProgressText.Text = val;
                            }), DispatcherPriority.Normal, $"Extracting: {entry.Key} ({current}/{count})");
                        }
                    }
                });
                switch (actionWhenCompleted)
                {
                    case ExtractionComplete.Prompt:
                        if (await this.ShowMessageAsync("Question", "Open destination directory?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Open folder", NegativeButtonText = "Cancel" }) == MessageDialogResult.Affirmative)
                        {
                            System.Diagnostics.Process.Start(destinationFolder);
                            // destinationFolder
                        }
                        break;
                    case ExtractionComplete.Always:
                        System.Diagnostics.Process.Start(destinationFolder);
                        break;
                }
                this.isWorking = null;
            }
            catch (Exception ex)
            {
                this.isWorking = null;
                MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (this.archive != null)
                this.tabList.IsSelected = true;
        }

        private static bool TryNormallizePath(string stringIn, out string output)
        {
            bool result = false;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(stringIn.Length);
            foreach (char c in stringIn)
            {
                if (Array.IndexOf(invalidpathchars, c) != -1)
                {
                    sb.Append(replacedinvalidcharacterpath);
                    result = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
            if (result)
                output = sb.ToString();
            else
                output = null;
            return result;
        }

        private static bool TryNormallizePath(string[] stringIn, out string output, out bool isfullpath)
        {
            bool result = false;
            isfullpath = false;
            string mystring;
            string[] result2 = new string[stringIn.Length];
            int lastindex = stringIn.Length - 1;
            for (int i = 0; i < stringIn.Length;i++)
            {
                if (TryNormallizePath(stringIn[i], out mystring))
                {
                    result2[i] = mystring;
                    result = true;
                    if (i < lastindex)
                    {
                        isfullpath = true;
                    }
                }
                else
                {
                    result2[i] = stringIn[i];
                }
            }
            if (result)
            {
                if (isfullpath)
                    output = string.Join(Path.DirectorySeparatorChar.ToString(), result2);
                else
                    output = result2[result2.Length - 1];
            }
            else
                output = null;
            return result;
        }

        private void ListBoxItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat && e.Key == Key.Enter && this.filelist.SelectedItems.Count == 1)
            {
                ItemViewDirectory dir = this.filelist.SelectedItem as ItemViewDirectory;
                if (dir != null)
                {
                    dir.DoubleClicked();
                }
            }
        }

        private async void MenuItemClearPasswordHistory_Click(object sender, RoutedEventArgs e)
        {
            if (await this.ShowMessageAsync("Confirmation", "Are you sure you want to clear the password history?", MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "Yes", NegativeButtonText = "No" }) == MessageDialogResult.Affirmative)
            {
                Settings.UsedPassword = null;
            }
        }

        private void MenuItemOpenDirNone_Click(object sender, RoutedEventArgs e)
        {
            Settings.ActionWhenComplete = ExtractionComplete.DoNothing;
        }

        private void MenuItemOpenDirPrompt_Click(object sender, RoutedEventArgs e)
        {
            Settings.ActionWhenComplete = ExtractionComplete.Prompt;
        }

        private void MenuItemOpenDirAlways_Click(object sender, RoutedEventArgs e)
        {
            Settings.ActionWhenComplete = ExtractionComplete.Always;
        }

        private void MenuItemAutoCorrectPathWhenExtract_Checked(object sender, RoutedEventArgs e)
        {
            Settings.EnableAutoCorrectPath = this.autoCorrectPathWhenExtract.IsChecked;
        }

        private void MenuItemOptions_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            switch (Settings.ActionWhenComplete)
            {
                case ExtractionComplete.Always:
                    this.openDirAlways.IsChecked = true;
                    this.openDirNone.IsChecked = false;
                    this.openDirPrompt.IsChecked = false;
                    break;
                case ExtractionComplete.DoNothing:
                    this.openDirAlways.IsChecked = false;
                    this.openDirNone.IsChecked = true;
                    this.openDirPrompt.IsChecked = false;
                    break;
                default:
                    this.openDirAlways.IsChecked = false;
                    this.openDirNone.IsChecked = false;
                    this.openDirPrompt.IsChecked = true;
                    break;
            }
            this.autoCorrectPathWhenExtract.IsChecked = Settings.EnableAutoCorrectPath;
        }
    }
}
