Imports System.ComponentModel
Imports Ionic.VArchive
Imports Microsoft.Win32

Public Class MyMainMenu

    Private currentFile As ZipFile
    Private currentCache As List(Of ZipEntry)
    Private WithEvents BWorker_Extraction As BackgroundWorker
    Private selectedNodes As List(Of TreeNode)
    Private CancelAndExit As Boolean
    Private ForceClose As Boolean
    Private ArchivePassword As String

    Private ExtractionMode As Boolean
    Private ExtractionPath As String

    Private Sub MyMainMenu_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Icon = My.Resources.haru_sd_WM1UAm_256px
        Me.BWorker_Extraction = New BackgroundWorker()
        Me.BWorker_Extraction.WorkerSupportsCancellation = True
        Me.currentCache = New List(Of ZipEntry)()
        Me.selectedNodes = New List(Of TreeNode)()
        Me.ExtractionMode = False
        Me.ExtractionPath = String.Empty
        BWorker_Extraction.WorkerReportsProgress = True
        Me.ArchivePassword = String.Empty
        Me.CancelAndExit = False
        Me.ForceClose = False
        Dim theArgs() As String = System.Environment.GetCommandLineArgs()
        If (theArgs.Length > 1) Then
            If (My.Computer.FileSystem.FileExists(theArgs(1))) Then
                If (theArgs(1).ToLower().EndsWith(".v")) Then
                    LoadFile(theArgs(1))
                End If
            End If
        End If
    End Sub

    Private Property Password() As String
        Get
            Return Me.ArchivePassword
        End Get
        Set(value As String)
            If (currentFile IsNot Nothing) Then
                Me.ArchivePassword = value
                currentFile.Password = value
            End If
        End Set
    End Property

    Private Sub LoadFile(ByVal theFilePath As String)
        If (My.Computer.FileSystem.FileExists(theFilePath)) Then
            Dim theLastOpenArchive As ZipFile = Nothing
            If (currentFile IsNot Nothing) Then
                theLastOpenArchive = currentFile
            End If
            Try
                currentFile = ZipFile.Read(theFilePath)
                Me.Text = "VData Explorer - " & System.IO.Path.GetFileName(theFilePath)
                If (theLastOpenArchive IsNot Nothing) Then
                    TreeView1.Nodes.Clear()
                    selectedNodes.Clear()
                    ClearSelectedNodesCache()
                    theLastOpenArchive.Dispose()
                End If
                CloseToolStripMenuItem.Enabled = True
                ReadFile()
            Catch ex As Exception
                If (theLastOpenArchive IsNot Nothing) Then currentFile = theLastOpenArchive
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Else
            MessageBox.Show("File not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub ReadFile()
        If (currentFile IsNot Nothing) Then
            currentCache.Clear()
            For Each theNode In currentFile
                currentCache.Add(theNode)
                TreeAddNode(theNode)
            Next
            ExtractAllFilesToolStripMenuItem.Enabled = True
        End If
    End Sub

    Private Sub TreeAddNode(ByVal theNode As ZipEntry)
        Dim path() As String = Nothing
        If (theNode.FileName.IndexOf("/") > -1) Then
            path = theNode.FileName.Split("/"c)
        End If
        Dim currentNode As TreeNode = Nothing
        Dim currentReading As TreeNodeCollection = Nothing
        Dim pathNode As List(Of TreeNodeCollection) = New List(Of TreeNodeCollection)()

        If (path IsNot Nothing) AndAlso (path.Length > 0) Then
            If (path.Length > 1) Then
                For cou As Integer = 0 To path.Length - 2
                    currentNode = Nothing
                    If (pathNode.Count > 0) Then
                        currentReading = pathNode.Last
                    Else
                        currentReading = TreeView1.Nodes
                    End If
                    For Each ExistingNode As TreeNode In currentReading
                        If (ExistingNode.Text.ToLower().Trim() = path(cou).ToLower.Trim()) Then
                            currentNode = ExistingNode
                            Exit For
                        End If
                    Next
                    If (currentNode Is Nothing) Then
                        With (currentReading.Insert(0, path(cou)))
                            .ToolTipText = "Folder"
                            pathNode.Add(.Nodes)
                            .Name = ""

                        End With
                    Else
                        pathNode.Add(currentNode.Nodes)
                    End If
                Next
            End If
            With (pathNode.Last().Add(path.Last()))
                .ToolTipText = "File"
                .Tag = theNode
            End With
        End If
    End Sub

    Private Sub OpenToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OpenToolStripMenuItem.Click
        Using theBrowse As New OpenFileDialog()
            theBrowse.Title = "Select file"
            theBrowse.SupportMultiDottedExtensions = False
            theBrowse.RestoreDirectory = True
            theBrowse.Multiselect = False
            theBrowse.Filter = "SoulWorker VData (*.v)|*.v"
            theBrowse.DefaultExt = "v"
            theBrowse.CheckPathExists = True
            theBrowse.CheckFileExists = True
            If (theBrowse.ShowDialog(Me) = DialogResult.OK) Then
                LoadFile(theBrowse.FileName())
            End If
        End Using
    End Sub

    Private Sub CloseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CloseToolStripMenuItem.Click
        If (currentFile IsNot Nothing) Then
            TreeView1.Nodes.Clear()
            selectedNodes.Clear()
            ClearSelectedNodesCache()
            currentFile.Dispose()
            currentFile = Nothing
            ExtractAllFilesToolStripMenuItem.Enabled = False
            CloseToolStripMenuItem.Enabled = False
        End If
        Me.Text = "VData Explorer"
    End Sub

    Private Sub ClearSelectedNodesCache()
        currentCache.Clear()
        ExtractFilesToolStripMenuItem.Enabled = False
        ExtractFilesToolStripMenuItem1.Enabled = False
        DeselectAllToolStripMenuItem.Enabled = False
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub ExtractAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExtractAllFilesToolStripMenuItem.Click, ExtractAllFilesToolStripMenuItem1.Click
        If (currentCache IsNot Nothing) AndAlso (currentCache.Count > 0) Then
            If (BWorker_Extraction.IsBusy()) Then
                MessageBox.Show("Explorer currently extracting file(s)...", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                Dim something As New Leayal.Forms.FolderSelectDialog()
                something.Title = "Select location where to be extracted"
                'something.FileName = ""
                Dim result As String = DirectCast(Registry.GetValue(IO.Path.Combine(Registry.CurrentUser.Name, "SOFTWARE", "Leayal", "VDataExplorer"), "LastExtractDirectory", String.Empty), String)
                If (Not String.IsNullOrWhiteSpace(result) AndAlso IO.Directory.Exists(result)) Then
                    something.InitialDirectory = result
                End If
                If (something.ShowDialog(Me.Handle)) Then
                    Registry.SetValue(IO.Path.Combine(Registry.CurrentUser.Name, "SOFTWARE", "Leayal", "VDataExplorer"), "LastExtractDirectory", something.FileName)
                    StartExtraction(True, something.FileName)
                End If

                'Using theFolderBrowse As New FolderBrowserDialog()
                '    theFolderBrowse.Description = "Select location where to be extracted"
                '    theFolderBrowse.ShowNewFolderButton = True
                '    theFolderBrowse.RootFolder = Environment.SpecialFolder.Desktop
                '    If (theFolderBrowse.ShowDialog(Me) = DialogResult.OK) Then
                '        StartExtraction(True, theFolderBrowse.SelectedPath())
                '    End If
                'End Using
            End If
        End If
    End Sub

    Private Sub StartExtraction(ByVal SelectedMode As Boolean, ByVal vPath As String)
        Me.ExtractionMode = SelectedMode
        Me.ExtractionPath = vPath
        BWorker_Extraction.RunWorkerAsync(New Object() {SelectedMode, vPath})
    End Sub

    Private Sub RestartExtraction()
        BWorker_Extraction.RunWorkerAsync(New Object() {Me.ExtractionMode, Me.ExtractionPath})
    End Sub

    Private Function NormalizePath(ByVal theString As String) As String
        Dim result As String = theString
        If (result.StartsWith(".")) Then
            result = result.TrimStart("."c)
        End If
        If (result.StartsWith("/")) Or (result.StartsWith("\")) Then
            result = result.TrimStart(New Char() {"/"c, "\"c})
        End If
        If (result.EndsWith("/")) Or (result.EndsWith("\")) Then
            result = result.TrimEnd(New Char() {"/"c, "\"c})
        End If
        Return result
    End Function

    Private Sub ExtractFilesToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExtractFilesToolStripMenuItem.Click, ExtractFilesToolStripMenuItem1.Click
        If (selectedNodes IsNot Nothing) AndAlso (selectedNodes.Count > 0) Then
            If (BWorker_Extraction.IsBusy()) Then
                MessageBox.Show("Explorer currently extracting file(s)...", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                If (selectedNodes.Count > 1) Then
                    Using theFolderBrowse As New FolderBrowserDialog()
                        theFolderBrowse.Description = "Select location where to be extracted"
                        theFolderBrowse.ShowNewFolderButton = True
                        theFolderBrowse.RootFolder = Environment.SpecialFolder.Desktop
                        If (theFolderBrowse.ShowDialog(Me) = DialogResult.OK) Then
                            StartExtraction(False, theFolderBrowse.SelectedPath())
                        End If
                    End Using
                Else
                    Dim theTarget As ZipEntry = DirectCast(selectedNodes(0).Tag, ZipEntry)
                    Dim theFileName As String = System.IO.Path.GetFileName(theTarget.FileName)
                    Dim theExt As String = System.IO.Path.GetExtension(theFileName).TrimStart("."c)
                    Using theFolderBrowse As New SaveFileDialog()
                        theFolderBrowse.Title = "Save File: " & System.IO.Path.GetFileName(theTarget.FileName)
                        theFolderBrowse.SupportMultiDottedExtensions = True
                        theFolderBrowse.RestoreDirectory = True
                        theFolderBrowse.Filter = "*." & theExt & "|*." & theExt
                        theFolderBrowse.AddExtension = True
                        theFolderBrowse.DefaultExt = theExt
                        theFolderBrowse.CheckFileExists = False
                        theFolderBrowse.CheckPathExists = True
                        theFolderBrowse.OverwritePrompt = True
                        theFolderBrowse.FileName = theFileName
                        If (theFolderBrowse.ShowDialog(Me) = DialogResult.OK) Then
                            StartExtraction(False, theFolderBrowse.FileName())
                        End If
                    End Using
                End If
            End If
        End If
    End Sub

    Private Sub Extractor_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles BWorker_Extraction.DoWork
        Dim CurrentBWorker As BackgroundWorker = DirectCast(sender, BackgroundWorker)
        Dim theObj() As Object = DirectCast(e.Argument, Object())
        Dim thePath As String = DirectCast(theObj(1), String)
        BWorker_Extraction.ReportProgress(0)
        If (DirectCast(theObj(0), Boolean) = False) Then
            Dim theString As String = Nothing
            Dim Node As ZipEntry
            If (selectedNodes.Count > 1) Then
                For cou As Integer = 0 To selectedNodes.Count - 1
                    If (CurrentBWorker.CancellationPending) Then
                        e.Cancel = True
                        Exit For
                    End If
                    Node = DirectCast(selectedNodes(cou).Tag, ZipEntry)
                    theString = System.IO.Path.Combine(thePath, NormalizePath(Node.FileName))
                    My.Computer.FileSystem.CreateDirectory(My.Computer.FileSystem.GetParentPath(theString))
                    BWorker_Extraction.ReportProgress(2, (cou + 1).ToString() & "/" & selectedNodes.Count.ToString())
                    Using theStream As New System.IO.FileStream(theString, IO.FileMode.Create, IO.FileAccess.Write)
                        Node.Extract(theStream)
                    End Using
                Next
            Else
                If (CurrentBWorker.CancellationPending) Then
                    e.Cancel = True
                End If
                Node = DirectCast(selectedNodes(0).Tag, ZipEntry)
                BWorker_Extraction.ReportProgress(1, Node)
                Using theStream As New System.IO.FileStream(thePath, IO.FileMode.Create, IO.FileAccess.Write)
                    Node.Extract(theStream)
                End Using
            End If
        Else
            Dim theString As String = Nothing
            Dim Node As ZipEntry
            For cou As Integer = 0 To currentCache.Count - 1
                If (CurrentBWorker.CancellationPending) Then
                    e.Cancel = True
                    Exit For
                End If
                Node = currentCache(cou)
                theString = System.IO.Path.Combine(thePath, NormalizePath(Me.RemoveInvalidCharacters(Node.FileName)))
                My.Computer.FileSystem.CreateDirectory(My.Computer.FileSystem.GetParentPath(theString))
                BWorker_Extraction.ReportProgress(2, (cou + 1).ToString() & "/" & currentCache.Count.ToString())
                Using theStream As New System.IO.FileStream(theString, IO.FileMode.Create, IO.FileAccess.Write)
                    Node.Extract(theStream)
                End Using
            Next
        End If
        e.Result = New Object() {theObj(0), thePath}
    End Sub

    Private Function RemoveInvalidCharacters(ByVal input As String) As String
        input = input.Replace("?", "_")
        Return input
    End Function

    Private Sub Extractor_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles BWorker_Extraction.ProgressChanged
        If (e.ProgressPercentage = 0) Then
            OpenToolStripMenuItem.Enabled = False
            CloseToolStripMenuItem.Enabled = False
            FileToolStripMenuItem.Enabled = False
            TreeView1.Enabled = False
            ProgressTextToolStripMenuItem.Text = ""
            ProgressTextToolStripMenuItem.Visible = True
        ElseIf (e.ProgressPercentage = 1) Then
            With (DirectCast(e.UserState(), ZipEntry))
                ProgressTextToolStripMenuItem.Text = "Extracting: " & System.IO.Path.GetFileName(.FileName)
            End With
        ElseIf (e.ProgressPercentage = 2) Then
            ProgressTextToolStripMenuItem.Text = "Extracting: " & DirectCast(e.UserState(), String)
        End If
    End Sub

    Private Sub Extractor_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles BWorker_Extraction.RunWorkerCompleted
        OpenToolStripMenuItem.Enabled = True
        CloseToolStripMenuItem.Enabled = True
        ProgressTextToolStripMenuItem.Visible = False
        FileToolStripMenuItem.Enabled = True
        TreeView1.Enabled = True
        If (e.Error IsNot Nothing) Then
            If (e.Error.GetType() = GetType(BadPasswordException)) Then
                If (String.IsNullOrEmpty(Password)) Then
                    Using BadPassForm As New BadPasswordForm(Password, "The archive has been encrypted.")
                        If (BadPassForm.ShowDialog(Me) = DialogResult.OK) Then
                            Password = BadPassForm.Password
                            RestartExtraction()
                        End If
                    End Using
                Else
                    Using BadPassForm As New BadPasswordForm(Password, "Wrong password. Please enter a new one.")
                        If (BadPassForm.ShowDialog(Me) = DialogResult.OK) Then
                            Password = BadPassForm.Password
                            RestartExtraction()
                        Else
                            MessageBox.Show(Me, e.Error.Message, "Error: " & e.Error.GetType.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End If
                    End Using
                End If
            Else
                MessageBox.Show(Me, e.Error.Message, "Error: " & e.Error.GetType.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Else
            If (e.Cancelled) Then

            Else
                If (MessageBox.Show(Me, "Extraction Completed." & vbNewLine & "Open the output directory ?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes) Then
                    Try
                        Dim theObjs = DirectCast(e.Result, Object())
                        Dim theString As String = DirectCast(theObjs(1), String)
                        Dim theFileAttr As System.IO.FileAttributes = System.IO.File.GetAttributes(theString)
                        If (theFileAttr And System.IO.FileAttributes.Directory) = System.IO.FileAttributes.Directory Then
                            System.Diagnostics.Process.Start("explorer.exe", "/select,""" & theString & """")
                        Else
                            System.Diagnostics.Process.Start(My.Computer.FileSystem.GetParentPath(theString))
                        End If
                    Catch ex As Exception
                        MessageBox.Show(Me, ex.Message, "Error: " & ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End If
            End If
            If (CancelAndExit) Then
                Me.ForceClose = True
                Me.Close()
            End If
        End If
    End Sub

    Private Sub DeselectNodes(nodes As TreeNodeCollection)
        For Each aNode As System.Windows.Forms.TreeNode In nodes
            If (aNode.Checked) Then
                aNode.Checked = False
            End If

            If aNode.Nodes.Count > 0 Then
                DeselectNodes(aNode.Nodes)
            End If
        Next
    End Sub

    Private Sub SelectNodes(nodes As TreeNodeCollection)
        For Each aNode As System.Windows.Forms.TreeNode In nodes
            If (Not aNode.Checked) Then
                aNode.Checked = True
            End If

            If aNode.Nodes.Count > 0 Then
                SelectNodes(aNode.Nodes)
            End If
        Next
    End Sub

    Private Sub TreeView1_DragDrop(sender As Object, e As DragEventArgs) Handles TreeView1.DragDrop
        Dim files As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())
        Dim theString As String = files.First()
        If (theString.ToLower().EndsWith(".v")) Then
            LoadFile(files.First())
        End If
    End Sub

    Private Sub TreeView1_DragEnter(sender As Object, e As DragEventArgs) Handles TreeView1.DragEnter
        If e.Data.GetDataPresent(DataFormats.FileDrop) Then e.Effect = DragDropEffects.Copy
    End Sub

    Private Sub TreeView1_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterCheck
        If (e.Node.ToolTipText = "File") Then
            If (e.Node.Checked) Then
                If (selectedNodes.IndexOf(e.Node) = -1) Then selectedNodes.Add(e.Node)
            Else
                If (selectedNodes.IndexOf(e.Node) > -1) Then selectedNodes.Remove(e.Node)
            End If
            If (selectedNodes.Count > 0) Then
                ExtractFilesToolStripMenuItem.Enabled = True
                ExtractFilesToolStripMenuItem1.Enabled = True
                DeselectAllToolStripMenuItem.Enabled = True
            Else
                ExtractFilesToolStripMenuItem.Enabled = False
                ExtractFilesToolStripMenuItem1.Enabled = False
                DeselectAllToolStripMenuItem.Enabled = False
            End If
        Else
            If (e.Node.Checked) Then
                SelectNodes(e.Node.Nodes)
            Else
                DeselectNodes(e.Node.Nodes)
            End If
        End If
    End Sub

    Private Sub DeselectAllToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeselectAllToolStripMenuItem.Click
        DeselectNodes(TreeView1.Nodes)
    End Sub

    Private Sub MyMainMenu_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        selectedNodes.Clear()
        If (currentCache IsNot Nothing) Then currentCache.Clear()
        If (currentFile IsNot Nothing) Then currentFile.Dispose()
    End Sub

    Private Sub TreeView1_MouseUp(sender As Object, e As MouseEventArgs) Handles TreeView1.MouseUp
        If (e.Button = MouseButtons.Right) Then
            If (currentCache IsNot Nothing) AndAlso (currentCache.Count > 0) Then
                ContextMenuStrip1.Show(TreeView1, e.Location)
            Else
                MenuToolStripMenuItem.ShowDropDown()
            End If
        End If
    End Sub

    Private Sub TreeView1_NodeMouseDoubleClick(sender As Object, e As TreeNodeMouseClickEventArgs) Handles TreeView1.NodeMouseDoubleClick
        If (e.Node.ToolTipText = "File") Then e.Node.Checked = Not e.Node.Checked
    End Sub

    Private Sub MyMainMenu_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If (Not ForceClose) AndAlso (e.CloseReason = CloseReason.UserClosing) Then
            If (BWorker_Extraction.IsBusy) Then
                e.Cancel = True
                If (MessageBox.Show("VData Explorer is currently extracting file." & vbNewLine & "You want to cancel and exit ?", "Notice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes) Then
                    Me.CancelAndExit = True
                    Me.ForceClose = True
                    BWorker_Extraction.CancelAsync()
                End If
            End If
        End If
    End Sub

    Private Sub MyMainMenu_MouseUp(sender As Object, e As MouseEventArgs) Handles MyBase.MouseUp
        If (BWorker_Extraction.IsBusy) Then
            If (e.Button = MouseButtons.Right) Then
                ProgressTextToolStripMenuItem.ShowDropDown()
            End If
        End If
    End Sub

    Private Sub CancelExtractToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CancelExtractToolStripMenuItem.Click
        If (BWorker_Extraction.IsBusy) Then
            If (MessageBox.Show("You want to cancel extraction ?" & vbNewLine & "(Please note that file(s) already extracted will remain)", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes) Then
                BWorker_Extraction.CancelAsync()
            End If
        End If
    End Sub
End Class