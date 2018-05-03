using Lamie.LibVData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpCompress.Common;
using SharpCompress.Common.Zip;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;

namespace VData_Explorer.Classes
{
    sealed class TreeItemsList
    {
        private static readonly char[] splitter = { '/', '\\' };
        private DirectoryList root;
        private DirectoryList _currentDirectory;
        public DirectoryList CurrentDirectory => this._currentDirectory;
        private VFile myfile;
        private Dictionary<DirectoryList, List<ItemViewModel>> cacheViews;
        private Typeface typeface;
        private double _fontsize;
        private Dictionary<string, DirectoryList> _directories;
        public Dictionary<string, DirectoryList> Directories => this._directories;

        private static string[] GetFolders(string filepath)
        {
            return filepath.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
        }

        internal static IEnumerable<ZipEntry> GetEntriesFromDirectory(DirectoryList directory) => GetEntriesFromDirectory(directory, SearchOption.AllDirectories);
        internal static IEnumerable<ZipEntry> GetEntriesFromDirectory(DirectoryList directory, SearchOption searchOption)
        {
            if (searchOption == SearchOption.AllDirectories && (directory.Directories.Count != 0))
            {
                List<ZipEntry> entries = new List<ZipEntry>();
                if (directory.Files.Count != 0)
                    entries.AddRange(directory.Files.Values);
                foreach (DirectoryList dir in directory.Directories)
                    entries.AddRange(GetEntriesFromDirectory(dir, searchOption));
                return entries;
            }
            else
            {
                return directory.Files.Values;
            }
        }

        public TreeItemsList(VFile file, Typeface fontface, double fontsize)
        {
            this._fontsize = fontsize;
            this.typeface = fontface;
            this.myfile = file;
            this.root = new DirectoryList(string.Empty, "<Root>", null);
            this.cacheViews = new Dictionary<DirectoryList, List<ItemViewModel>>();
            this._directories = new Dictionary<string, DirectoryList>(StringComparer.OrdinalIgnoreCase);
            this._directories.Add(string.Empty, this.root);
        }

        public void Init()
        {
            DirectoryList currentDir;
            string[] dir;
            int i = 0;
            string key;
            foreach (var item in this.myfile)
            {
                if (!item.IsDirectory)
                {
                    /*
                    if (item.Key.StartsWith(".."))
                        key = item.Key.TrimStart('.', '/', '\\');
                    else
                        key = item.Key;
                    //*/
                    key = item.Key;
                    if ((key.IndexOf('/') == -1) && (key.IndexOf('\\') == -1))
                    {
                        this.root.Files.Add(Path.GetFileName(key), item);
                    }
                    else
                    {
                        dir = GetFolders(key);
                        currentDir = this.root;
                        for (i = 0; i < (dir.Length - 1); i++)
                        {
                            currentDir = currentDir[dir[i]];
                            if (!this._directories.ContainsKey(currentDir.Fullname))
                                this._directories.Add(currentDir.Fullname, currentDir);
                        }
                        currentDir.Files.Add(dir[dir.Length - 1], item);
                    }
                }
            }
            this._currentDirectory = this.root;
        }

        public void SelectDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                this._currentDirectory = this.root;
                this.GenerateListCurrent();
            }
            else
            {
                if (this._directories.ContainsKey(path))
                {
                    this._currentDirectory = this._directories[path];
                    this.GenerateListCurrent();
                }
                else if ((path.IndexOf('/') == -1) && (path.IndexOf('\\') == -1))
                {
                    this._currentDirectory = this.root[path];
                    this.GenerateListCurrent();
                }
                else
                {
                    if (path == "/" || path == "\\")
                    {
                        this._currentDirectory = this.root;
                        this.GenerateListCurrent();
                    }
                    else
                    {
                        string[] dirs = GetFolders(path);
                        if (dirs.Length == 0)
                        {
                            this._currentDirectory = this.root;
                            this.GenerateListCurrent();
                        }
                        else
                        {
                            this.SelectDirectory(dirs);
                        }
                    }
                }
            }
        }

        public void SelectDirectory(string[] dir)
        {
            DirectoryList currentDir = this.root;
            for (int i = 0; i < dir.Length; i++)
                currentDir = currentDir[dir[i]];
            this._currentDirectory = currentDir;
            this.GenerateListCurrent();
        }

        public void GenerateListCurrent()
        {
            int count = this._currentDirectory.Directories.Count + this._currentDirectory.Files.Count + 1;

            List<ItemViewModel> meh;
            if (!this.cacheViews.TryGetValue(this._currentDirectory, out meh))
            {
                meh = new List<ItemViewModel>(count);
                ItemViewDirectory huh;

                if (this._currentDirectory.Directories.Count > 0)
                {
                    foreach (var dir in this._currentDirectory.Directories)
                    {
                        huh = new ItemViewDirectory(dir, dir.Name);
                        huh.RequestOpen += this.Huh_RequestOpen;
                        meh.Add(huh);
                    }
                }

                if (this._currentDirectory != this.root)
                {
                    huh = new ItemViewDirectory(this._currentDirectory.Parent, "InboxOut", true, "..");
                    huh.RequestOpen += this.Huh_RequestOpen;
                    meh.Add(huh);
                }

                if (this._currentDirectory.Files.Count > 0)
                {
                    FormattedText ft;
                    double longestSize = this.GetFormattedText("Size").Width, longestCompressedSize = this.GetFormattedText("Packed Size").Width;
                    ItemViewFile viewFile;
                    foreach (var val in this._currentDirectory.Files.Values)
                    {
                        viewFile = new ItemViewFile(val);
                        if (viewFile.Size.HasValue)
                        {
                            ft = this.GetFormattedText(viewFile.Size.Value.ToString());
                            if (ft.Width > longestSize)
                                longestSize = ft.Width;
                        }
                        if (viewFile.CompressedSize.HasValue)
                        {
                            ft = this.GetFormattedText(viewFile.CompressedSize.Value.ToString());
                            if (ft.Width > longestCompressedSize)
                                longestCompressedSize = ft.Width;
                        }
                        meh.Add(viewFile);
                    }
                    this._currentDirectory.LongestSize = longestSize;
                    this._currentDirectory.LongestCompressedSize = longestCompressedSize;
                }
                meh.Sort(ViewComparer.Default);
                this.cacheViews.Add(this._currentDirectory, meh);
            }
            
            this.ViewReset?.Invoke(this._currentDirectory, meh);
        }

        private FormattedText GetFormattedText(string str)
        {
            return new FormattedText(str, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, this.typeface, this._fontsize, null);
        }

        private void Huh_RequestOpen(DirectoryList obj)
        {
            this._currentDirectory = obj;
            this.GenerateListCurrent();
        }

        public event Action<DirectoryList, IList<ItemViewModel>> ViewReset;

        public void Clear() => this.root.Clear();

        private class ViewComparer : IComparer<ItemViewModel>
        {
            public static readonly ViewComparer Default = new ViewComparer();
            public int Compare(ItemViewModel x, ItemViewModel y)
            {
                bool xDir = x.IsDirectory,
                    yDir = y.IsDirectory;
                if (xDir == yDir)
                    return NameComparer.Default.Compare(x.Filename, y.Filename);
                else
                {
                    if (xDir)
                        return -1;
                    else
                        return 1;
                }
            }
        }

        private class NameComparer : IComparer<string>
        {
            public static readonly NameComparer Default = new NameComparer();
            public int Compare(string x, string y)
            {
                bool xBack = StringComparer.Ordinal.Equals(x, ".."),
                    yBack = StringComparer.Ordinal.Equals(y, "..");
                if (xBack && yBack)
                    return 0;
                else
                {
                    if (xBack)
                        return -1;
                    else if (yBack)
                        return 1;
                    else
                        return StringComparer.Ordinal.Compare(x, y);
                }
            }
        }
    }

    class DirectoryList
    {
        public DirectoryList Parent { get; }
        public string Name { get; }
        private Dictionary<string, DirectoryList> dirs;
        public ICollection<DirectoryList> Directories => this.dirs.Values;
        public FileCollection Files { get; }
        public double LongestSize { get; set; } = double.NaN;
        public double LongestCompressedSize { get; set; } = double.NaN;
        public string Fullname { get; }

        public DirectoryList(string name, string fullname, DirectoryList parent)
        {
            this.Parent = parent;
            this.Name = name;
            this.Fullname = fullname;
            this.Files = new FileCollection();
            this.dirs = new Dictionary<string, DirectoryList>(StringComparer.OrdinalIgnoreCase);
        }

        public DirectoryList this[string directoryname]
        {
            get
            {
                if (this.dirs.ContainsKey(directoryname))
                    return this.dirs[directoryname];
                else
                {
                    var dir = new DirectoryList(directoryname, this.Fullname + Path.DirectorySeparatorChar + directoryname, this);
                    this.dirs.Add(directoryname, dir);
                    return dir;
                }
            }
        }

        public void Clear()
        {
            this.dirs.Clear();
            this.Files.Clear();
        }
    }

    class FileCollection : Dictionary<string, ZipEntry>
    {
        public FileCollection() : base(StringComparer.OrdinalIgnoreCase) { }
    }
}
