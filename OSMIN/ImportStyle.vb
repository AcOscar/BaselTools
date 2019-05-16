Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.DatabaseServices

Class ImportStyle

    Property Name As String

    Property LayerKeyNames As List(Of String)

    Property LayerStyles As List(Of LayerStyle)

    Property LayerPrefix As String

    Property NodeLayername As String

    Property License As String

    Property LicenseLayer As String

    Property NodeTagKeys As List(Of String)

    Sub Load(ByVal xImportStyle As XElement)

        LicenseLayer = xImportStyle.<layer4license>.Value

        License = xImportStyle.<licenstext>.Value

        NodeLayername = xImportStyle.<layer4nodes>.Value

        LayerPrefix = xImportStyle.<layerprefix>.Value

        Name = xImportStyle.@name

        LayerKeyNames = New List(Of String)

        LayerStyles = New List(Of LayerStyle)

        For Each xLyKy In xImportStyle.<layerkeynames>.Elements

            LayerKeyNames.Add(xLyKy.Value)

        Next

        NodeTagKeys = New List(Of String)

        For Each xLyKy In xImportStyle.<nodetagkeynames>.Elements

            NodeTagKeys.Add(xLyKy.Value)

        Next

        For Each xLayerStyle As XElement In xImportStyle.<layersettings>.Elements

            Dim myLayerstyle As New LayerStyle

            myLayerstyle.Load(xLayerStyle, LayerPrefix)

            LayerStyles.Add(myLayerstyle)
        Next


    End Sub

    Function GetLayer(ByVal LayerName As String) As Autodesk.AutoCAD.DatabaseServices.LayerTableRecord

        For Each LyStyle As LayerStyle In LayerStyles

            If LyStyle.RegExNameIs(LayerName) Then

                Return LyStyle.acLayer(LayerName)

            End If

        Next

        Dim myLayer As DatabaseServices.LayerTableRecord

        myLayer = New DatabaseServices.LayerTableRecord

        myLayer.Name = Me.LayerPrefix

        Return myLayer

    End Function

End Class
