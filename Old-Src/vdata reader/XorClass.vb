Imports System.IO
Public NotInheritable Class XorClass
    Public Const SecretByte As Integer = &H55
    Public Shared Sub XorByteArray(ByRef bytes() As Byte)
        For i As Integer = 0 To bytes.Length - 1
            bytes(i) = CByte(bytes(i) Xor SecretByte)
        Next
    End Sub

    Public Overloads Shared Sub XorFile(ByVal path As String)
        Using memStream As New MemoryStream()
            Dim intsss As Long = 0
            Using instream As FileStream = File.OpenRead(path)
                intsss = instream.Length
                If (intsss > 0) Then
                    XorFile(instream, memStream)
                End If
            End Using
            If (intsss > 0) Then
                memStream.Position = 0
                Using outstream As FileStream = File.OpenWrite(path)
                    StreamCopy(memStream, outstream)
                End Using
            End If
        End Using
    End Sub

    Public Overloads Shared Sub XorFile(ByVal inFile As String, ByVal outFile As String)
        Using instream As FileStream = File.OpenRead(inFile)
            Using outstream As FileStream = File.OpenWrite(outFile)
                XorFile(instream, outstream)
            End Using
        End Using
    End Sub

    Private Shared Sub StreamCopy(ByVal inStream As Stream, ByVal outStream As Stream)
        inStream.Position = 0
        Dim bytebuffer(1024) As Byte
        Dim readbytes As Integer = inStream.Read(bytebuffer, 0, 1024)
        While (readbytes > 0)
            outStream.Write(bytebuffer, 0, readbytes)
            readbytes = inStream.Read(bytebuffer, 0, 1024)
        End While
    End Sub

    Public Overloads Shared Sub XorFile(ByVal inStream As Stream, ByVal outStream As Stream)
        Dim bytebuffer(1024) As Byte
        Dim readbytes As Integer = inStream.Read(bytebuffer, 0, 1024)
        While (readbytes > 0)
            XorByteArray(bytebuffer)
            outStream.Write(bytebuffer, 0, readbytes)
            readbytes = inStream.Read(bytebuffer, 0, 1024)
        End While
    End Sub

    Class XorFileStream
        Inherits FileStream

        Public Shared Function OpenRead(ByVal path As String) As XorFileStream
            Return New XorFileStream(path, FileMode.Open, FileAccess.Read)
        End Function

        Public Shared Function OpenWrite(ByVal path As String) As XorFileStream
            Return New XorFileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite)
        End Function

        Public Sub New(ByVal path As String, ByVal mode As FileMode)
            MyBase.New(path, mode)
        End Sub

        Public Sub New(ByVal path As String, ByVal mode As FileMode, ByVal access As FileAccess)
            MyBase.New(path, mode, access)
        End Sub

        Public Sub New(ByVal path As String, ByVal mode As FileMode, ByVal access As FileAccess, ByVal share As FileShare)
            MyBase.New(path, mode, access, share)
        End Sub

        Public Overrides Function Read(array() As Byte, offset As Int32, count As Int32) As Int32
            Dim result As Integer = MyBase.Read(array, offset, count)
            XorByteArray(array)
            Return result
        End Function

        Public Overrides Sub Write(array() As Byte, offset As Int32, count As Int32)
            XorByteArray(array)
            MyBase.Write(array, offset, count)
        End Sub

        Public Overrides Sub WriteByte(value As Byte)
            MyBase.WriteByte(CByte(value Xor SecretByte))
        End Sub

        Public Overrides Function ReadByte() As Int32
            Return (MyBase.ReadByte() Xor SecretByte)
        End Function
    End Class

    Class XorStream
        Inherits Stream
        Private innerStream As Stream
        Public Shadows Sub Dispose()
            Me.innerStream.Dispose()
        End Sub
        Public Sub New(ByVal source As Stream)
            Me.innerStream = source
        End Sub
        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return Me.innerStream.CanRead
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return Me.innerStream.CanSeek
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return Me.innerStream.CanWrite
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Int64
            Get
                Return Me.innerStream.Length
            End Get
        End Property

        Public Overrides Property Position As Int64
            Get
                Return Me.innerStream.Position
            End Get
            Set(value As Int64)
                Me.innerStream.Position = value
            End Set
        End Property

        Public Overrides Sub Flush()
            Me.innerStream.Flush()
        End Sub

        Public Overrides Sub SetLength(value As Int64)
            Me.innerStream.SetLength(value)
        End Sub

        Public Overrides Sub Write(buffer() As Byte, offset As Int32, count As Int32)
            XorByteArray(buffer)
            Me.innerStream.Write(buffer, offset, count)
        End Sub

        Public Overrides Function Read(buffer() As Byte, offset As Int32, count As Int32) As Int32
            Dim result As Integer = Me.innerStream.Read(buffer, offset, count)
            XorByteArray(buffer)
            Return result
        End Function

        Public Overrides Function Seek(offset As Int64, origin As SeekOrigin) As Int64
            Return Me.innerStream.Seek(offset, origin)
        End Function
    End Class
End Class