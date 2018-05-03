Imports Microsoft.VisualBasic.ApplicationServices

Module Program
    Public Sub Main()
        AddHandler AppDomain.CurrentDomain.AssemblyResolve, New ResolveEventHandler(AddressOf AssemblyResolver.CurrentDomain_AssemblyResolve)

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim asdasdasd As New AppController()
        asdasdasd.Run(System.Environment.GetCommandLineArgs())
    End Sub

    Private Class AppController
        Inherits Global.Microsoft.VisualBasic.ApplicationServices.WindowsFormsApplicationBase

        Public Sub New()
            MyBase.New(Global.Microsoft.VisualBasic.ApplicationServices.AuthenticationMode.Windows)
            Me.IsSingleInstance = False
            Me.EnableVisualStyles = True
            Me.SaveMySettingsOnExit = False
            Me.ShutdownStyle = Global.Microsoft.VisualBasic.ApplicationServices.ShutdownMode.AfterMainFormCloses
            AddHandler Microsoft.Win32.SystemEvents.UserPreferenceChanged, AddressOf App_UserPreferenceChangedEventArgs
        End Sub

        Private Sub App_UserPreferenceChangedEventArgs(sender As Object, e As Microsoft.Win32.UserPreferenceChangedEventArgs)
            If (e.Category = Microsoft.Win32.UserPreferenceCategory.Color) Or (e.Category = Microsoft.Win32.UserPreferenceCategory.VisualStyle) Or (e.Category = Microsoft.Win32.UserPreferenceCategory.General) Then
                Leayal.Forms.AeroControl.ThemeInfo.GetAccentColor()
            End If
        End Sub

        Protected Overrides Function OnStartup(eventArgs As StartupEventArgs) As Boolean
            Leayal.Forms.AeroControl.ThemeInfo.GetAccentColorEx()
            Return MyBase.OnStartup(eventArgs)
        End Function

        Protected Overrides Sub OnShutdown()
            RemoveHandler Microsoft.Win32.SystemEvents.UserPreferenceChanged, AddressOf App_UserPreferenceChangedEventArgs
            MyBase.OnShutdown()
        End Sub

        <Global.System.Diagnostics.DebuggerStepThroughAttribute()>
        Protected Overrides Sub OnCreateMainForm()
            Me.MainForm = Global.vdata_reader.MyMainMenu
        End Sub
    End Class
End Module
