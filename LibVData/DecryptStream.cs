using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lamie.LibVData
{
    /// <summary>
    /// Wrapper stream on top of a readable stream to decrypt on-the-fly
    /// </summary>
    public class DecryptStream : Stream
    {
        private static readonly Task CompletedTask = Task.FromResult(true);
        private bool _leaveOpen;
        private Stream _baseStream;
        /// <summary>
        /// Gets the underlying stream that is used to create this stream
        /// </summary>
        public Stream BaseStream => this._baseStream;

        /// <summary>
        /// Initialize a new decrypt stream
        /// </summary>
        /// <param name="stream">The stream which will be decrypted</param>
        /// <exception cref="ArgumentException" />
        public DecryptStream(Stream stream) : this(stream, false) { }

        /// <summary>
        /// Initialize a new decrypt stream
        /// </summary>
        /// <param name="stream">The stream which will be decrypted</param>
        /// <param name="leaveOpen">Sets the value indicating whether the underlying stream should be disposed when the current stream is disposed</param>
        /// <exception cref="ArgumentException" />
        public DecryptStream(Stream stream, bool leaveOpen)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Cannot decrypt the stream if it is not readable.", "stream");
            this._leaveOpen = leaveOpen;
            this._baseStream = stream;
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking
        /// </summary>
        public override bool CanSeek => this._baseStream.CanSeek;

        /// <summary>
        /// Gets a value that determines whether the current stream can time out
        /// </summary>
        public override bool CanTimeout => this._baseStream.CanTimeout;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Gets the length in bytes of the stream
        /// </summary>
        public override long Length => this._baseStream.Length;

        /// <summary>
        /// Gets or sets the position within the current stream
        /// </summary>
        public override long Position { get => this._baseStream.Position; set => this._baseStream.Position = value; }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to write before timing out
        /// </summary>
        public override int WriteTimeout { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out
        /// </summary>
        public override int ReadTimeout { get => this._baseStream.ReadTimeout; set => this._baseStream.ReadTimeout = value; }

        /// <summary>
        /// Gets or sets a value determine whether the underlying stream should be closed when this stream is closed.
        /// </summary>
        public bool LeaveBaseStreamOpen { get => this._leaveOpen; set => this._leaveOpen = value; }

        /// <summary>
        /// Releases all resources used by the System.IO.Stream
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && !this._leaveOpen)
            {
                this._baseStream.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device
        /// </summary>
        public override void Flush()
        {
            // Do nothing, since the stream is read-only.
        }

        /// <summary>
        /// Asynchronously clears all buffers for this stream and causes any buffered data to be written to the underlying device
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None</param>
        /// <returns></returns>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            // Do nothing as well, since the stream is read-only.
            return CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">The buffer to write the data into</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream</param>
        /// <param name="count">The maximum number of bytes to read</param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int theRead = this._baseStream.Read(buffer, offset, count);
            if (theRead != 0)
                Decryptor.DecryptBuffer(buffer, offset, theRead);
            return theRead;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream
        /// </summary>
        /// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream</returns>
        public override int ReadByte()
        {
            int b = this._baseStream.ReadByte();
            if (b == -1)
                return b;
            return Decryptor.Decrypt(b);
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read
        /// </summary>
        /// <param name="buffer">The buffer to write the data into</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data from the stream</param>
        /// <param name="count">The maximum number of bytes to read</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The stream is currently in use by a previous read operation</exception>
        /// <exception cref="ObjectDisposedException">The stream has been disposed</exception>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this._baseStream.ReadAsync(buffer, offset, count, cancellationToken).ContinueWith((t) =>
            {
                int theRead = t.Result;
                if (theRead != 0)
                    Decryptor.DecryptBuffer(buffer, offset, theRead);
                return theRead;
            });
        }

        /// <summary>
        /// Sets the position within the current stream
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter</param>
        /// <param name="origin">A value of type System.IO.SeekOrigin indicating the reference point used to obtain the new position</param>
        /// <returns>The new position within the current stream</returns>
        /// <exception cref="IOException">An I/O error occurs</exception>
        /// <exception cref="NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output</exception>
        /// <exception cref="ObjectDisposedException">Methods were called after the stream was closed</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this._baseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes</param>
        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Invalid operation. The stream is decrypting only.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Invalid operation. The stream is decrypting only
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Invalid operation. The stream is decrypting only
        /// </summary>
        /// <param name="value"></param>
        public override void WriteByte(byte value)
        {
            throw new InvalidOperationException();
        }
    }
}
