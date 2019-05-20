#Region "Namespaces"
Imports System.Globalization
Imports AcadDb = Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

#End Region

Public Class LayerList
    Sub Start()
        Dim myDWG As ApplicationServices.Document
        Dim myDB As DatabaseServices.Database
        Dim myTransMan As DatabaseServices.TransactionManager
        Dim myTrans As DatabaseServices.Transaction
        Dim myBT As DatabaseServices.BlockTable
        Dim myBTR As DatabaseServices.BlockTableRecord

        myDWG = ApplicationServices.Application.DocumentManager.MdiActiveDocument
        myDB = myDWG.Database
        myTransMan = myDWG.TransactionManager
        myTrans = myTransMan.StartTransaction
        myBT = myDWG.Database.BlockTableId.GetObject(AcadDb.OpenMode.ForRead)
        myBTR = myBT(DatabaseServices.BlockTableRecord.ModelSpace).GetObject(AcadDb.OpenMode.ForWrite)

        Dim Linetypes As New List(Of dwglinetype)
        Dim myLinetypeTable As AcadDb.LinetypeTable
        myLinetypeTable = myDB.LinetypeTableId.GetObject(AcadDb.OpenMode.ForRead)
        Dim myTEnu As AcadDb.SymbolTableEnumerator
        myTEnu = myLinetypeTable.GetEnumerator
        Dim myLiTR As DatabaseServices.LinetypeTableRecord
        While myTEnu.MoveNext
            myLiTR = myTEnu.Current.GetObject(AcadDb.OpenMode.ForRead)
            'xref style interesieren nicht
            If Not myLiTR.IsDependent Then
                Dim aLinetype As New dwglinetype
                aLinetype.ReadLinetype(myLiTR)
                Linetypes.Add(aLinetype)
            End If
        End While

        Dim layers As New List(Of dwglayer)
        Dim myLayerTable As AcadDb.LayerTable
        myLayerTable = myDB.LayerTableId.GetObject(AcadDb.OpenMode.ForRead)
        Dim myLEnu As AcadDb.SymbolTableEnumerator
        myLEnu = myLayerTable.GetEnumerator
        Dim myLyTR As AcadDb.LayerTableRecord
        While myLEnu.MoveNext
            myLyTR = myLEnu.Current.GetObject(AcadDb.OpenMode.ForRead)
            'xref layer interesieren nicht
            If Not myLyTR.IsDependent Then
                Dim aLayer As New dwglayer
                aLayer.ReadRecord(myLyTR, Linetypes)
                layers.Add(aLayer)
            End If
        End While

        Dim myComp As New MyStringComparer(CompareInfo.GetCompareInfo("de-CH"), CompareOptions.StringSort)
        Dim layerarray As Array = layers.ToArray
        Array.Sort(layerarray, myComp)

        layers = New List(Of dwglayer)
        layers.AddRange(layerarray)

        Dim PrmptKywrdOpts As PromptKeywordOptions = New PromptKeywordOptions("")
        PrmptKywrdOpts.Message = vbLf & "group layer names? "
        PrmptKywrdOpts.Keywords.Add("Yes")
        PrmptKywrdOpts.Keywords.Add("No")
        PrmptKywrdOpts.AllowNone = False
        PrmptKywrdOpts.Keywords.Default = "Yes"
        Dim PrmptRslt As PromptResult = myDWG.Editor.GetKeywords(PrmptKywrdOpts)

        Dim Grouping As Boolean

        If PrmptRslt.StringResult = "Yes" Then
            Grouping = True
        End If

        Dim UpperLeft As Geometry.Point3d
        UpperLeft = myDWG.Editor.GetPoint(vbCrLf & vbTab & "Select Position Point").Value

        Dim Counter As Integer
        'Layout Values
        Dim ZPos As Double = 0
        Dim RowOffset As Double = -0.5
        Dim ExamplLineOffset As Double = 5
        Dim LtNameTextOffset As Double = 10.5
        Dim ExamplLineLength As Double = 5
        Dim LineweightOffset As Double = 13
        Dim DescriptionTextOffset As Double = 14.75
        Dim XPos As Double = UpperLeft.X
        Dim TextHeigt As Double = 0.3
        Dim LineYOffset As Double = 0.15

        ' For Each layer As dwglayer In layers
        For l As Integer = 0 To layers.Count - 1
            Dim layer As dwglayer = layers(l)


            If Grouping Then

                If "0 Ashade Defpoints".Contains(layer.Name) Then
                    Continue For
                End If

                If l > 0 Then

                    Dim vorLayer = layers(l - 1)

                    If layer.Name.Length < 2 Or vorLayer.Name.Length < 2 Then
                        Continue For

                    End If

                    If layer.Name.Substring(0, 2) <> vorLayer.Name.Substring(0, 2) Then
                        Counter += 1
                    End If

                    Dim layerAr As Array = layer.Name.Split("-")
                    Dim vorLayerAr As Array = vorLayer.Name.Split("-")

                    If UBound(layerAr) = UBound(vorLayerAr) Then
                        If UBound(layerAr) > 1 Then
                            If layerAr(0) <> vorLayerAr(0) Then
                                Counter += 1
                            ElseIf layerAr(1) <> vorLayerAr(1) Then
                                Counter += 1
                            End If


                        End If
                    End If
                End If
            End If

            Dim YPos As Double = UpperLeft.Y + (Counter * RowOffset)

            Dim LyNameText As New AcadDb.MText
            LyNameText.SetContentsRtf(layer.Name)
            LyNameText.Layer = layer.Name
            LyNameText.Location = New Point3d(XPos, YPos, ZPos)
            LyNameText.TextHeight = TextHeigt

            Dim ExamplLine As New AcadDb.Line
            ExamplLine.Layer = layer.Name
            ExamplLine.StartPoint = New Point3d(XPos + ExamplLineOffset, YPos - LineYOffset, ZPos)
            ExamplLine.EndPoint = New Point3d(XPos + ExamplLineOffset + ExamplLineLength, YPos - LineYOffset, ZPos)

            Dim LtNameText As New AcadDb.MText
            LtNameText.SetContentsRtf(layer.LineTyp)
            LtNameText.Layer = layer.Name
            LtNameText.Location = New Point3d(XPos + LtNameTextOffset, YPos, ZPos)
            LtNameText.TextHeight = TextHeigt

            Dim LineWeightText As New AcadDb.MText
            LineWeightText.SetContentsRtf(FormatLineWeigth(layer.Lineweight))
            LineWeightText.Layer = layer.Name
            LineWeightText.Location = New Point3d(XPos + LineweightOffset, YPos, ZPos)
            LyNameText.TextHeight = TextHeigt

            Dim DescriptionText As New AcadDb.MText
            DescriptionText.SetContentsRtf(layer.Description)
            DescriptionText.Layer = layer.Name
            DescriptionText.Location = New Point3d(XPos + DescriptionTextOffset, YPos, ZPos)
            LyNameText.TextHeight = TextHeigt

            myBTR.AppendEntity(LyNameText)
            myBTR.AppendEntity(LtNameText)
            myBTR.AppendEntity(DescriptionText)
            myBTR.AppendEntity(LineWeightText)
            myBTR.AppendEntity(ExamplLine)

            myTrans.AddNewlyCreatedDBObject(LyNameText, True)
            myTrans.AddNewlyCreatedDBObject(LtNameText, True)
            myTrans.AddNewlyCreatedDBObject(DescriptionText, True)
            myTrans.AddNewlyCreatedDBObject(LineWeightText, True)
            myTrans.AddNewlyCreatedDBObject(ExamplLine, True)
            Counter += 1
        Next

        myTrans.Commit()
        myTrans.Dispose()
        myTransMan.Dispose()
    End Sub

    Private Class dwglinetype
        Public Name As String
        Public Id As IntPtr
        Public Definition As String
        Public Description As String
        Sub ReadLinetype(ByVal myRecord As DatabaseServices.LinetypeTableRecord)
            Try
                Name = myRecord.Name

                Id = myRecord.ObjectId.OldIdPtr

                Description = myRecord.AsciiDescription
                If myRecord.NumDashes > 0 Then
                    Definition = "A"
                    For i As Integer = 0 To myRecord.NumDashes - 1
                        Dim myDash As String
                        myDash = myRecord.DashLengthAt(i)
                        Definition += ", " + myDash
                    Next
                End If

            Catch ex As System.Exception
                Debug.Print(ex.ToString)

            End Try
        End Sub


    End Class

    Private Class dwglayer

        Public Name As String
        Public Color As String
        Public LineTyp As String
        Public Lineweight As String
        Public Plot As Boolean
        Public Description As String

        Sub ReadRecord(ByVal myRecord As DatabaseServices.LayerTableRecord, ByVal myLines As List(Of dwglinetype))
            Try
                Name = myRecord.Name
                Color = myRecord.Color.ToString
                For Each _lintype As dwglinetype In myLines
                    If _lintype.Id = myRecord.LinetypeObjectId.OldIdPtr Then
                        LineTyp = _lintype.Name
                        Exit For
                    End If
                Next

                Lineweight = myRecord.LineWeight.ToString
                Plot = myRecord.IsPlottable
                Description = myRecord.Description
            Catch ex As System.Exception
                Debug.Print(ex.ToString)
            End Try
        End Sub

    End Class

    Private Class MyStringComparer
        Implements IComparer

        Private myComp As CompareInfo
        Private myOptions As CompareOptions = CompareOptions.None

        ' Constructs a comparer using the specified CompareOptions.
        Public Sub New(ByVal cmpi As CompareInfo, ByVal options As CompareOptions)
            myComp = cmpi
            Me.myOptions = options
        End Sub 'New

        ' Compares strings with the CompareOptions specified in the constructor.
        Public Function Compare(ByVal a As Object, ByVal b As Object) As Integer Implements IComparer.Compare

            Dim LayerA, LayerB As dwglayer
            Dim NameA, NameB As String

            LayerA = TryCast(a, dwglayer)
            LayerB = TryCast(b, dwglayer)

            NameA = LayerA.Name
            NameB = LayerB.Name

            If Not (NameA Is Nothing) And Not (NameB Is Nothing) Then
                Return myComp.Compare(NameA, NameB, myOptions)
            End If

            Throw New ArgumentException("a and b should be strings.")

        End Function 'Compare 

    End Class 'MyStringComparer

    Private Shared Function FormatLineWeigth(ByVal Lineweight As String) As String

        If Lineweight.ToLower.StartsWith("b") Then

            Return "Default"

        Else

            Return Lineweight.Substring(10, 1).ToString & "." & Lineweight.Substring(11, 2).ToString & " mm"

        End If

    End Function

End Class
