Public Class BadPasswordForm

    Private PasswordField As String
    Public Sub New(ByVal vPassword As String, ByVal MessageString As String)
        InitializeComponent()
        Me.PasswordField = vPassword
        Me.TextBox1.UseSystemPasswordChar = True
        Me.Label2.Text = MessageString
        Me.Label2.Visible = True
    End Sub

    Public ReadOnly Property Password() As String
        Get
            Return Me.PasswordField
        End Get
    End Property

    Public Sub New()
        Me.New(String.Empty, "Bad Password. Please enter a new one.")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.DialogResult = DialogResult.Cancel
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        Me.PasswordField = TextBox1.Text
    End Sub

    Private Sub TextBox1_KeyUp(sender As Object, e As KeyEventArgs) Handles TextBox1.KeyUp
        If (e.KeyCode = Keys.Enter) Then
            Button1.PerformClick()
        ElseIf (e.KeyCode = Keys.Escape) Then
            Button2.PerformClick()
        End If
    End Sub

    Private Sub CheckBox1_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox1.CheckedChanged
        If (CheckBox1.Checked) Then
            TextBox1.UseSystemPasswordChar = False
        Else
            TextBox1.UseSystemPasswordChar = True
        End If
    End Sub
End Class