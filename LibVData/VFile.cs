using SharpCompress.Archives;
using SharpCompress.Readers;
using SharpCompress.Archives.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Common.Zip;
using System.Collections;

namespace Lamie.LibVData
{
    /// <summary>
    /// Provides interaction with the V data file from a stream
    /// </summary>
    public sealed class VFile : IDisposable, IEnumerable<ZipEntry>
    {
        private Dictionary<string, ZipArchiveEntry> entrylist;
        private ZipArchive archive;
        private DecryptStream stream;
        private bool _disposed;

        /// <summary>
        /// Validate if the stream contains a <see cref="VFile"/>
        /// </summary>
        /// <param name="stream">The source stream to read</param>
        /// <returns></returns>
        public static bool IsValid(Stream stream)
        {
            using (DecryptStream dec = new DecryptStream(stream, true))
                return ZipArchive.IsZipFile(dec);
        }

        /// <summary>
        /// Validate if the stream contains a <see cref="VFile"/> with the given password
        /// </summary>
        /// <param name="stream">The source stream to read</param>
        /// <param name="password">The password to validate the <see cref="VFile"/></param>
        /// <returns></returns>
        public static bool IsValid(Stream stream, string password)
        {
            using (DecryptStream dec = new DecryptStream(stream, true))
                return ZipArchive.IsZipFile(dec, password);
        }

        /// <summary>
        /// Read the stream and "open" it as <see cref="VFile"/>
        /// </summary>
        /// <param name="stream">The source stream to read</param>
        /// <returns></returns>
        public static VFile Read(Stream stream)
        {
            return new VFile(stream, null, false);
        }

        /// <summary>
        /// Read the stream and "open" it as <see cref="VFile"/>
        /// </summary>
        /// <param name="stream">The source stream to read</param>
        /// <param name="password">Sets the password which will be used to read data file</param>
        /// <returns></returns>
        public static VFile Read(Stream stream, string password)
        {
            return new VFile(stream, password, false);
        }

        /// <summary>
        /// Read the stream and "open" it as <see cref="VFile"/>
        /// </summary>
        /// <param name="stream">The source stream to read</param>
        /// <param name="password">Sets the password which will be used to read data file</param>
        /// <param name="leaveOpen">Sets the value determine whether the stream will be closed when this instance is disposed</param>
        /// <returns></returns>
        public static VFile Read(Stream stream, string password, bool leaveOpen)
        {
            return new VFile(stream, password, leaveOpen);
        }

        private VFile(Stream stream, string password, bool leaveOpen)
        {
            this._disposed = false;
            this.stream = new DecryptStream(stream, leaveOpen);
            if (string.IsNullOrEmpty(password))
                this.archive = ZipArchive.Open(this.stream, new ReaderOptions() { LeaveStreamOpen = leaveOpen });
            else
                this.archive = ZipArchive.Open(this.stream, new ReaderOptions() { LeaveStreamOpen = leaveOpen, Password = password });
            this.entrylist = new Dictionary<string, ZipArchiveEntry>(this.archive.Entries.Count, StringComparer.OrdinalIgnoreCase);
            foreach (ZipArchiveEntry entry in this.archive.Entries)
                this.entrylist.Add(entry.Key, entry);
        }

        /// <summary>
        /// Retrieve an entry from the <see cref="VFile"/>
        /// </summary>
        /// <param name="entryPath">The path which point to the given file</param>
        /// <returns></returns>
        public ZipEntry this[string entryPath]
        {
            get
            {
                if (entryPath.IndexOf('\\') != -1)
                    entryPath = entryPath.Replace('\\', '/');
                if (entryPath.IndexOf("//") != -1)
                    entryPath = entryPath.Replace("//", "/");
                return this.entrylist[entryPath];
            }
        }

        /// <summary>
        /// Gets a number of entries contained in this <see cref="VFile"/>
        /// </summary>
        public int EntryCount => this.entrylist.Count;

        /// <summary>
        /// Gets or sets a value determine whether the underlying stream should be closed when this instance is disposed.
        /// </summary>
        public bool LeaveBaseStreamOpen { get => this.stream.LeaveBaseStreamOpen; set => this.stream.LeaveBaseStreamOpen = value; }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="VFile"/>'s entries
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ZipEntry> GetEnumerator() => this.entrylist.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.entrylist.Values.GetEnumerator();

        /// <summary>
        /// Extract an entry to a stream
        /// </summary>
        /// <param name="entry">The target entry</param>
        /// <param name="outstream">The destination stream to write the entry to</param>
        public void ExtractEntry(ZipEntry entry, Stream outstream)
        {
            if (!outstream.CanWrite)
                throw new ArgumentException("The given stream should be writable.", "outstream");

            ZipArchiveEntry realentry = entry as ZipArchiveEntry;
            if (realentry == null)
                realentry = this.entrylist[entry.Key];
            else if (realentry.Archive != this.archive)
                throw new ArgumentException("The given entry is not a part of this data file", "entry");
            realentry.WriteTo(outstream);
        }

        /// <summary>
        /// Extract an entry to a stream
        /// </summary>
        /// <param name="entryPath">The path inside <see cref="VFile"/> of the entry</param>
        /// <param name="outstream">The destination stream to write the entry to</param>
        public void ExtractEntry(string entryPath, Stream outstream)
        {
            if (!outstream.CanWrite)
                throw new ArgumentException("The given stream should be writable.", "outstream");
            if (!this.entrylist.TryGetValue(entryPath, out var val))
                throw new ArgumentException("The given entry is not a part of this data file", "entry");
            val.WriteTo(outstream);
        }

        /// <summary>
        /// Open a read-only stream to the given entry
        /// </summary>
        /// <param name="entry">The target entry</param>
        /// <returns></returns>
        public Stream GetEntryStream(ZipEntry entry)
        {
            ZipArchiveEntry realentry = entry as ZipArchiveEntry;
            if (realentry == null)
                realentry = this.entrylist[entry.Key];
            else if (realentry.Archive != this.archive)
                throw new ArgumentException("The given entry is not a part of this data file", "entry");
            return realentry.OpenEntryStream();
        }

        /// <summary>
        /// Open a read-only stream to the given entry
        /// </summary>
        /// <param name="entryPath">The path inside <see cref="VFile"/> of the entry</param>
        /// <returns></returns>
        public Stream GetEntryStream(string entryPath)
        {
            if (!this.entrylist.TryGetValue(entryPath, out var val))
                throw new ArgumentException("The given entry is not a part of this data file", "entry");
            return val.OpenEntryStream();
        }

        /// <summary>
        /// Release all the resources used by the <see cref="VFile"/>
        /// </summary>
        public void Dispose()
        {
            if (this._disposed) return;
            this._disposed = true;
            entrylist.Clear();
            entrylist = null;
            this.archive.Dispose();
            this.stream.Dispose();
        }
    }
}
