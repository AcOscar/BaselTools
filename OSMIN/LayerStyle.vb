Imports System.Text.RegularExpressions
Imports Autodesk.AutoCAD.Colors
Imports Autodesk.AutoCAD.DatabaseServices

Class LayerStyle

    Property Name As String
    Property LyOn As Boolean
    Property Freeze As Boolean
    Property Color As Color
    Property Description As String

    ReadOnly Property acLayer(ByVal Layername As String) As LayerTableRecord

        Get
            Dim myLayer As New LayerTableRecord

            With myLayer
                .Name = Layername
                .IsOff = Not LyOn
                .IsFrozen = Freeze
                .Color = Color
                .Description = Description
            End With

            Return myLayer

        End Get

    End Property

    Function Load(ByVal xLayerStyle As XElement, ByVal LayerPrefix As String) As Boolean

        Dim myVals As String() = xLayerStyle.Value.Split(";")

        If myVals.Count <> 10 Then

            Return False

        End If

        Try

            Name = IIf(LayerPrefix = "", "", LayerPrefix & "_") & myVals(0)

            LyOn = CBool(myVals(1))

            Freeze = CBool(myVals(2))

            Color = getAcColor(myVals(3))

            'Linetype = myVals(4)

            'Lineweight = myVals(5)

            'Transparency = getTransparency(myVals(6))

            'Plot = CBool(myVals(7))

            'NewVPFreeze = CBool(myVals(8))

            Description = myVals(9)

        Catch ex As Exception

        End Try

        Return True

    End Function

    Private Function getAcColor(ByVal Colorname As String) As Color

        Dim myColor As Color
        myColor = Color.FromColorIndex(ColorMethod.ByAci, 0)
        'RGB
        If Colorname.Contains(",") Then

            Dim rgb() As String = Colorname.Split(",")

            If rgb.Count = 3 Then

                myColor = Color.FromRgb(rgb(0), rgb(1), rgb(2))

                Return myColor

            End If

        End If

        'ACI
        myColor = Color.FromColorIndex(ColorMethod.ByAci, Colorname)

        Return myColor

    End Function

    Private Function getTransparency(ByVal TranparencyName As String) As Autodesk.AutoCAD.Colors.Transparency

        Dim alpha As Byte = CType((255 * ((100 - TranparencyName) / 100)), Byte)

        Return New Transparency(alpha)

    End Function

    Function RegExNameIs(ByVal LayerName As String) As Boolean

        Dim _reg As Regex = New Regex(Me.Name)

        Dim m As Match = _reg.Match(LayerName)

        Return m.Success

    End Function

End Class
