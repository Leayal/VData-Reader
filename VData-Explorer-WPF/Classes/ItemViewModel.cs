using SharpCompress.Common;
using SharpCompress.Common.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace VData_Explorer.Classes
{
    class ItemViewDirectory : ItemViewModel
    {
        private DirectoryList myrealself;
        public new DirectoryList Entry => this.myrealself;
        public bool IsExcluded { get; }
        public ItemViewDirectory(DirectoryList self, string dirname) : this(self, "Folder", false, dirname) { }

        public ItemViewDirectory(DirectoryList self, string iconID, bool excluded, string dirname) : base(null, dirname, iconID, true)
        {
            this.IsExcluded = excluded;
            this.myrealself = self;
        }

        public void DoubleClicked()
        {
            if (!this.IsDirectory) return;
            this.RequestOpen?.Invoke(this.myrealself);
        }

        public event Action<DirectoryList> RequestOpen;
    }

    class ItemViewFile : ItemViewModel
    {
        public ItemViewFile(ZipEntry entry) : base(entry, Path.GetFileName(entry.Key), "Page", false) { }
    }

    abstract class ItemViewModel
    {
        private ZipEntry info;
        public ZipEntry Entry => this.info;
        private bool _isdirectory;

        public ItemViewModel(ZipEntry entry, string appearenceName, string iconID, bool directory)
        {
            this.IconID = iconID;
            this._isdirectory = directory;
            this.info = entry;
            this.Filename = appearenceName;
        }

        public string Filename { get; }
        public string IconID { get; }

        public DateTime? ArchivedTime
        {
            get
            {
                if (this.info == null)
                    return null;
                return this.info.ArchivedTime;
            }
        }

        public long? CompressedSize
        {
            get
            {
                long? val = null;
                if (this.info != null)
                    val = info.CompressedSize;
                return val;
            }
        }

        public DateTime? CreatedTime
        {
            get
            {
                if (this.info == null)
                    return null;
                return this.info.CreatedTime;
            }
        }

        public string Key
        {
            get
            {
                if (this.info == null)
                    return null;
                return this.info.Key;
            }
        }

        public bool IsDirectory
        {
            get
            {
                if (info == null)
                    return this._isdirectory;
                else
                {
                    return (this.info.IsDirectory || this._isdirectory);
                }
            }
        }

        public bool? IsEncrypted
        {
            get
            {
                if (this.info == null)
                    return null;
                return this.info.IsEncrypted;
            }
        }

        public DateTime? LastAccessedTime
        {
            get
            {
                if (this.info == null)
                    return null;
                return this.info.LastAccessedTime;
            }
        }

        public DateTime? LastModifiedTime
        {
            get
            {
                if (this.info == null)
                    return null;
                return this.info.LastModifiedTime;
            }
        }

        public long? Size
        {
            get
            {
                long? val = null;
                if (this.info != null)
                    val = info.Size;
                return val;
            }
        }

        public int? Attrib => info.Attrib;


    }
}
