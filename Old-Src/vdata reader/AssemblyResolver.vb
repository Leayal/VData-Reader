Module AssemblyResolver
    Dim theAssemblyDict As System.Collections.Generic.Dictionary(Of String, System.Reflection.Assembly) = Nothing

    Public Function CurrentDomain_AssemblyResolve(ByVal sender As Object, ByVal args As System.ResolveEventArgs) As System.Reflection.Assembly
        If (theAssemblyDict Is Nothing) Then
            theAssemblyDict = New Dictionary(Of String, Reflection.Assembly)()
        End If
        If (theAssemblyDict.ContainsKey(args.Name)) Then
            Return theAssemblyDict(args.Name)
        Else
            Dim bytes As Byte() = Nothing
            Dim RealName As String = args.Name.Split(","c)(0).Trim()
            Dim resourceName As String = "vdata_reader." & RealName & ".dll"
            Dim currentAssembly As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
            Using stream = currentAssembly.GetManifestResourceStream(resourceName)
                bytes = New Byte(CInt(stream.Length)) {}
                stream.Read(bytes, 0, CInt(stream.Length))
            End Using
            theAssemblyDict.Add(args.Name, System.Reflection.Assembly.Load(bytes))
            Return theAssemblyDict(args.Name)
        End If
    End Function
End Module
