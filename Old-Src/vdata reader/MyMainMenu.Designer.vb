<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MyMainMenu
    Inherits Leayal.Forms.LeayalExtendedForm

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.TreeView1 = New System.Windows.Forms.TreeView()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.MenuToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.OpenToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CloseToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExtractFilesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExtractAllFilesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.DeselectAllToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ProgressTextToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CancelExtractToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ExtractFilesToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExtractAllFilesToolStripMenuItem1 = New System.Windows.Forms.ToolStripMenuItem()
        Me.MenuStrip1.SuspendLayout()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'TreeView1
        '
        Me.TreeView1.AllowDrop = True
        Me.TreeView1.CheckBoxes = True
        Me.TreeView1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TreeView1.Font = New System.Drawing.Font("Tahoma", 8.25!)
        Me.TreeView1.FullRowSelect = True
        Me.TreeView1.Location = New System.Drawing.Point(2, 26)
        Me.TreeView1.Name = "TreeView1"
        Me.TreeView1.ShowNodeToolTips = True
        Me.TreeView1.Size = New System.Drawing.Size(439, 290)
        Me.TreeView1.TabIndex = 0
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MenuToolStripMenuItem, Me.FileToolStripMenuItem, Me.ProgressTextToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(2, 2)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(439, 24)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'MenuToolStripMenuItem
        '
        Me.MenuToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenToolStripMenuItem, Me.CloseToolStripMenuItem, Me.ExitToolStripMenuItem})
        Me.MenuToolStripMenuItem.Name = "MenuToolStripMenuItem"
        Me.MenuToolStripMenuItem.Size = New System.Drawing.Size(50, 20)
        Me.MenuToolStripMenuItem.Text = "Menu"
        '
        'OpenToolStripMenuItem
        '
        Me.OpenToolStripMenuItem.Name = "OpenToolStripMenuItem"
        Me.OpenToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.O), System.Windows.Forms.Keys)
        Me.OpenToolStripMenuItem.Size = New System.Drawing.Size(146, 22)
        Me.OpenToolStripMenuItem.Text = "Open"
        '
        'CloseToolStripMenuItem
        '
        Me.CloseToolStripMenuItem.Enabled = False
        Me.CloseToolStripMenuItem.Name = "CloseToolStripMenuItem"
        Me.CloseToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.X), System.Windows.Forms.Keys)
        Me.CloseToolStripMenuItem.Size = New System.Drawing.Size(146, 22)
        Me.CloseToolStripMenuItem.Text = "Close"
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(146, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExtractFilesToolStripMenuItem, Me.ExtractAllFilesToolStripMenuItem, Me.DeselectAllToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.E), System.Windows.Forms.Keys)
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(37, 20)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'ExtractFilesToolStripMenuItem
        '
        Me.ExtractFilesToolStripMenuItem.Enabled = False
        Me.ExtractFilesToolStripMenuItem.Name = "ExtractFilesToolStripMenuItem"
        Me.ExtractFilesToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.E), System.Windows.Forms.Keys)
        Me.ExtractFilesToolStripMenuItem.Size = New System.Drawing.Size(224, 22)
        Me.ExtractFilesToolStripMenuItem.Text = "Extract File(s)"
        '
        'ExtractAllFilesToolStripMenuItem
        '
        Me.ExtractAllFilesToolStripMenuItem.Enabled = False
        Me.ExtractAllFilesToolStripMenuItem.Name = "ExtractAllFilesToolStripMenuItem"
        Me.ExtractAllFilesToolStripMenuItem.ShortcutKeys = CType(((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.Shift) _
            Or System.Windows.Forms.Keys.E), System.Windows.Forms.Keys)
        Me.ExtractAllFilesToolStripMenuItem.Size = New System.Drawing.Size(224, 22)
        Me.ExtractAllFilesToolStripMenuItem.Text = "Extract All Files"
        '
        'DeselectAllToolStripMenuItem
        '
        Me.DeselectAllToolStripMenuItem.Enabled = False
        Me.DeselectAllToolStripMenuItem.Name = "DeselectAllToolStripMenuItem"
        Me.DeselectAllToolStripMenuItem.Size = New System.Drawing.Size(224, 22)
        Me.DeselectAllToolStripMenuItem.Text = "Deselect All"
        '
        'ProgressTextToolStripMenuItem
        '
        Me.ProgressTextToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CancelExtractToolStripMenuItem})
        Me.ProgressTextToolStripMenuItem.Name = "ProgressTextToolStripMenuItem"
        Me.ProgressTextToolStripMenuItem.Size = New System.Drawing.Size(85, 20)
        Me.ProgressTextToolStripMenuItem.Text = "ProgressText"
        Me.ProgressTextToolStripMenuItem.Visible = False
        '
        'CancelExtractToolStripMenuItem
        '
        Me.CancelExtractToolStripMenuItem.Name = "CancelExtractToolStripMenuItem"
        Me.CancelExtractToolStripMenuItem.Size = New System.Drawing.Size(148, 22)
        Me.CancelExtractToolStripMenuItem.Text = "Cancel Extract"
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExtractFilesToolStripMenuItem1, Me.ExtractAllFilesToolStripMenuItem1})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(153, 48)
        '
        'ExtractFilesToolStripMenuItem1
        '
        Me.ExtractFilesToolStripMenuItem1.Enabled = False
        Me.ExtractFilesToolStripMenuItem1.Name = "ExtractFilesToolStripMenuItem1"
        Me.ExtractFilesToolStripMenuItem1.Size = New System.Drawing.Size(152, 22)
        Me.ExtractFilesToolStripMenuItem1.Text = "Extract File(s)"
        '
        'ExtractAllFilesToolStripMenuItem1
        '
        Me.ExtractAllFilesToolStripMenuItem1.Name = "ExtractAllFilesToolStripMenuItem1"
        Me.ExtractAllFilesToolStripMenuItem1.Size = New System.Drawing.Size(152, 22)
        Me.ExtractAllFilesToolStripMenuItem1.Text = "Extract All Files"
        '
        'MyMainMenu
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(443, 318)
        Me.Controls.Add(Me.TreeView1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.MainMenuStrip = Me.MenuStrip1
        Me.MinimumSize = New System.Drawing.Size(459, 357)
        Me.Name = "MyMainMenu"
        Me.Text = "VData Explorer"
        Me.TransparencyKey = System.Drawing.Color.Lavender
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents TreeView1 As TreeView
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents MenuToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents OpenToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CloseToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FileToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExtractFilesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ExtractAllFilesToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ProgressTextToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents DeselectAllToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents CancelExtractToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents ExtractFilesToolStripMenuItem1 As ToolStripMenuItem
    Friend WithEvents ExtractAllFilesToolStripMenuItem1 As ToolStripMenuItem
End Class
