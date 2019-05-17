#Region "Namespaces"
Imports AcadDb = Autodesk.AutoCAD.DatabaseServices
Imports AecArchDb = Autodesk.AEC.Arch.DatabaseServices
Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
#End Region

Public Class AddSpaces

    Public Sub CL_MultiSpaceAdd()

        Try

            Dim myEd As Editor = Application.DocumentManager.MdiActiveDocument.Editor

            'liste der property sets
            Dim PSDNames As New List(Of String)
            'liste der properties
            Dim myListValues As New ListValues

            Dim CmdVals As PromptResult = myEd.GetString(vbCrLf + "x,y,z,numberOfAreas,totalArea,PSD-Name,PropertyName,PropertyValue: ")

            If CmdVals.Status <> PromptStatus.OK Then
                Return
            End If

            Dim myLEntry As New ListValues.ListEntry

            Dim myVals As String() = Split(CmdVals.StringResult, ",")

            If myVals.Length <> 8 Then

                myEd.WriteMessage(vbCrLf & "wrong parameter numbers: " & myVals.Length)

                Exit Sub

            End If

            Dim NoArea As Integer = CDbl(myVals(3))
            Dim TotalArea As Double = CDbl(myVals(4))

            Dim mySingleArea As Double = TotalArea / NoArea

            Dim myLength As Double = Math.Sqrt(mySingleArea)


            Dim noOfElementsInARow As Integer = Math.Ceiling(Math.Sqrt(NoArea))

            PSDNames.Add(myVals(5))

            myLEntry.Name = myVals(6)
            myLEntry.Value = myVals(7)

            myListValues.List.Add(myLEntry)

            Dim tx As Double = CDbl(myVals(0))
            Dim y As Double = CDbl(myVals(1))
            Dim z As Double = CDbl(myVals(2))

            Dim x As Double

            For row = 0 To noOfElementsInARow - 1

                x = tx

                For col = 0 To noOfElementsInARow - 1

                    If ((row * noOfElementsInARow) + col) < NoArea Then

                        Dim Location As New Point3d(x, y, z)

                        DrawSpace(Location, myLength, myLength, PSDNames, myListValues)

                        x += myLength

                    End If

                Next

                y += myLength

            Next

            myEd = Nothing
            CmdVals = Nothing
            myListValues = Nothing
            PSDNames = Nothing

        Catch ex As System.Exception

            Debug.Print(ex.Message.ToString)

        End Try

    End Sub

    Sub CL_SpaceAdd2()

        Try

            Dim myEd As Editor = Application.DocumentManager.MdiActiveDocument.Editor

            'liste der property sets
            Dim PSDNames As New List(Of String)
            'liste der properties
            Dim myListValues As New ListValues

            Dim CmdVals As PromptResult = myEd.GetString(vbCrLf + "x,y,z,length,width,numbers,PSD-Name,PropertyName,PropertyValue: ")

            If CmdVals.Status <> PromptStatus.OK Then
                Return
            End If

            Dim myLEntry As New ListValues.ListEntry

            Dim myVals As String() = Split(CmdVals.StringResult, ",")

            If myVals.Length <> 9 Then

                myEd.WriteMessage(vbCrLf & "wrong parameter numbers: " & myVals.Length)

                Exit Sub

            End If

            Dim Location As New Point3d(CDbl(myVals(0)), CDbl(myVals(1)), CDbl(myVals(2)))

            Dim length As Double = CDbl(myVals(3))
            Dim width As Double = CDbl(myVals(4))
            Dim numbers As Integer = CInt(myVals(5))

            If numbers < 1 Then

                Exit Sub

            End If

            PSDNames.Add(myVals(6))

            myLEntry.Name = myVals(7)
            myLEntry.Value = myVals(8)

            myListValues.List.Add(myLEntry)

            For i As Integer = 1 To numbers
                DrawSpace(Location, length, width, PSDNames, myListValues)
            Next


            myEd = Nothing
            CmdVals = Nothing
            myListValues = Nothing
            PSDNames = Nothing

        Catch ex As System.Exception

            Debug.Print(ex.Message.ToString)

        End Try

    End Sub

    Sub CL_SpaceAdd()

        Try

            Dim myEd As Editor = Application.DocumentManager.MdiActiveDocument.Editor

            'liste der property sets
            Dim PSDNames As New List(Of String)
            'liste der properties
            Dim myListValues As New ListValues

            Dim CmdVals As PromptResult = myEd.GetString(vbCrLf + "x,y,z,length,width,PSD-Name,PropertyName,PropertyValue: ")

            If CmdVals.Status <> PromptStatus.OK Then
                Return
            End If

            Dim myLEntry As New ListValues.ListEntry

            Dim myVals As String() = Split(CmdVals.StringResult, ",")

            If myVals.Length <> 8 Then

                myEd.WriteMessage(vbCrLf & "wrong parameter numbers: " & myVals.Length)

                Exit Sub

            End If

            Dim Location As New Point3d(CDbl(myVals(0)), CDbl(myVals(1)), CDbl(myVals(2)))

            Dim length As Double = CDbl(myVals(3))
            Dim width As Double = CDbl(myVals(4))

            PSDNames.Add(myVals(5))

            myLEntry.Name = myVals(6)
            myLEntry.Value = myVals(7)

            myListValues.List.Add(myLEntry)

            DrawSpace(Location, length, width, PSDNames, myListValues)

            myEd = Nothing
            CmdVals = Nothing
            myListValues = Nothing
            PSDNames = Nothing

        Catch ex As System.Exception

            Debug.Print(ex.Message.ToString)

        End Try

    End Sub

    Sub SpaceAdd()

        Try

            Dim myEd As Editor
            myEd = Application.DocumentManager.MdiActiveDocument.Editor

            Dim TargetSpaceArea As Double

            'liste der property sets
            Dim PSDNames As New List(Of String)
            'liste der properties
            Dim myListValues As New ListValues

            Dim AreaRslt As PromptDoubleResult = myEd.GetDouble(vbCrLf + "Enter the area value for the space: ")
            If AreaRslt.Status <> PromptStatus.OK Then
                Return
            End If
            TargetSpaceArea = AreaRslt.Value

            While True
                ' prompt for the property name
                Dim res1 As PromptResult = myEd.GetString(vbCrLf + "Enter property set definition name to add on space: ")

                If (res1.Status <> PromptStatus.OK) Then
                    Exit While
                Else
                    If res1.StringResult <> "" Then
                        Dim pname As String = res1.StringResult
                        PSDNames.Add(pname)
                    Else
                        Exit While
                    End If
                End If

            End While

            If PSDNames.Count > 0 Then

                While True
                    Dim myLEntry As New ListValues.ListEntry

                    Dim res2 As PromptResult = myEd.GetString(vbCrLf + "Enter property name to set: ")

                    If res2.Status <> PromptStatus.OK Then
                        Exit While
                    Else
                        If res2.StringResult <> "" Then
                            myLEntry.Name = res2.StringResult

                        Else
                            Exit While
                        End If
                    End If

                    Dim res3 As PromptResult = myEd.GetString(vbCrLf + "Enter value for " & myLEntry.Name & ": ")

                    If res3.Status <> PromptStatus.OK Then
                        Exit While
                    Else
                        If res3.StringResult <> "" Then
                            myLEntry.Value = res3.StringResult
                        Else
                            Exit While
                        End If
                    End If

                    myListValues.List.Add(myLEntry)

                End While

            End If

            DrawSpace(New Point3d(0, 0, 0), Math.Sqrt(AreaRslt.Value), Math.Sqrt(AreaRslt.Value), PSDNames, myListValues)

            myEd = Nothing
            AreaRslt = Nothing
            myListValues = Nothing
            PSDNames = Nothing

        Catch ex As System.Exception

            Debug.Print(ex.Message.ToString)

        End Try

    End Sub

    Private Shared Sub DrawSpace(ByVal Location As Point3d,
                                 ByVal length As Double,
                                 ByVal width As Double,
                                 ByVal PSDNames As List(Of String),
                                 ByVal myListValues As ListValues)

        Dim myDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim myDB As AcadDb.Database = myDoc.Database
        Dim myEd As Editor = myDoc.Editor

        Dim SpaceProfile As AcadDb.Polyline = CreateRectangle(length, width)

        If SpaceProfile IsNot Nothing Then
            Dim myBTR As AcadDb.BlockTableRecord
            Dim myBT As AcadDb.BlockTable

            Using myTransManSp As DatabaseServices.TransactionManager = myDB.TransactionManager

                Dim myTransSp As AcadDb.Transaction = myTransManSp.StartTransaction

                Dim myProfile As New Autodesk.Aec.Geometry.Profile
                myProfile = Autodesk.Aec.Geometry.Profile.CreateFromEntity(SpaceProfile, myEd.CurrentUserCoordinateSystem)

                Dim mySpace As New AecArchDb.Space

                mySpace.SetDatabaseDefaults(myDB)
                mySpace.SetToStandard(myDB)
                mySpace.SetProfile(AecArchDb.SpaceProfileType.Base, myProfile)

                mySpace.Location = Location

                myBT = myTransSp.GetObject(myDB.BlockTableId, AcadDb.OpenMode.ForRead)
                myBTR = myTransSp.GetObject(myBT(AcadDb.BlockTableRecord.ModelSpace), AcadDb.OpenMode.ForWrite)

                myBTR.AppendEntity(mySpace)
                myTransSp.AddNewlyCreatedDBObject(mySpace, True)
                myTransSp.Commit()

                Dim entid As AcadDb.ObjectId = mySpace.ObjectId
                Dim WasCreated As Boolean = False
                Dim PSDError As Boolean = False

                Dim propsetId As AcadDb.ObjectId

                'die psd anhängen
                For Each pname As String In PSDNames

                    propsetId = AEC_Helper.GetPropertySetDefinitionIdByName(pname)
                    If (propsetId.IsNull) Then
                        myEd.WriteMessage(vbCrLf + " There are no property set definitions by that name :" & pname)
                        PSDError = True
                    End If

                    WasCreated = AEC_Helper.CreatePropSetOnDBObject(entid, propsetId)

                    If Not WasCreated Then
                        myEd.WriteMessage(vbCrLf + " Failed to create property set " & pname & "on entity.: ") '& Blockname)
                        PSDError = True
                    End If
                Next

                'die properties setzen wenn es psd gibt
                If Not PSDError Then

                    If PSDNames.Count > 0 Then

                        For Each lentry As ListValues.ListEntry In myListValues.List

                            AEC_Helper.SetValuesFromPropertySetByName(mySpace, lentry.Name, lentry.Value)

                        Next

                    End If

                End If

                If myBT IsNot Nothing Then
                    myBT.Dispose()
                    myBT = Nothing
                End If

                If myBTR IsNot Nothing Then
                    myBTR.Dispose()
                    myBTR = Nothing
                End If

                myProfile = Nothing

                If myTransSp IsNot Nothing Then

                    myTransSp.Dispose()
                    myTransSp = Nothing

                End If

            End Using

        End If

        myEd = Nothing
        myDB.Dispose()
        myDB = Nothing
        myDoc = Nothing

    End Sub

    Private Shared Function CreateRectangle(ByVal lengt As Double, ByVal width As Double) As AcadDb.Polyline

        '' Create a polyline with two segments (3 points)
        Dim acPoly As AcadDb.Polyline = New AcadDb.Polyline
        acPoly.SetDatabaseDefaults()

        acPoly.AddVertexAt(0, New Point2d(0, 0), 0, 0, 0)
        acPoly.AddVertexAt(1, New Point2d(0, lengt), 0, 0, 0)
        acPoly.AddVertexAt(2, New Point2d(width, lengt), 0, 0, 0)
        acPoly.AddVertexAt(3, New Point2d(width, 0), 0, 0, 0)
        acPoly.AddVertexAt(4, New Point2d(0, 0), 0, 0, 0)

        Return acPoly

    End Function

End Class
