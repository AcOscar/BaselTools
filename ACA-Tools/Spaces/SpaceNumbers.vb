Imports Autodesk.AEC.PropertyData.DatabaseServices
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports ACA_Tools.AEC_Helper

Public Class AECRenumber

    Private Shared LastCountValue As Integer = 1

    Shared Sub Main()

        Dim mydoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim myDb As Database = mydoc.Database
        Dim myEd As Editor = mydoc.Editor

        'select polyline
        Dim tvs As TypedValue() = {New TypedValue(0, "LWPOLYLINE")}
        Dim sf As New SelectionFilter(tvs)
        Dim pso As New PromptSelectionOptions()
        pso.MessageForAdding = vbLf & "Select polyline:"
        pso.SingleOnly = True
        pso.AllowDuplicates = False

        Dim psr As PromptSelectionResult = myEd.GetSelection(pso, sf)

        If psr.Status <> PromptStatus.OK Then
            Return
        End If

        Dim SelSet As SelectionSet = psr.Value
        Dim SelIdArray As ObjectId() = SelSet.GetObjectIds()

        'select PSD Name
        Dim PSDName As String = String.Empty

        If Not GetString(PSDName, vbCrLf + "Enter PropertySetDefinition name for numbering: ") Then
            Return
        End If

        'select Property Name
        Dim myPropName As String = String.Empty

        If Not GetString(myPropName, vbCrLf + "Enter PropertyName to number: ") Then
            Return
        End If

        'select Start Value
        Dim CountVal As Integer

        Dim PStrO As New PromptStringOptions(vbCrLf + "Enter start number: ")
        PStrO.DefaultValue = LastCountValue

        Dim res3 As PromptResult = myEd.GetString(PStrO)
        If res3.Status <> PromptStatus.OK Then
            Return
        Else
            If res3.StringResult <> "" Then
                CountVal = res3.StringResult
            Else
                Return
            End If
        End If

        'here we start with the real work
        Dim myTransMan As Autodesk.AutoCAD.DatabaseServices.TransactionManager
        Dim myTrans As Transaction

        myTransMan = mydoc.TransactionManager
        myTrans = myTransMan.StartTransaction

        'dim list with the distance to polyline startpoint
        Dim myAECEntDistance As New List(Of Double)
        'the related PSD
        Dim myAECEntPPS As New List(Of PropertySet)
        'we need just the first
        Dim DistToFirstorLast As Double = Double.PositiveInfinity

        Dim FirstPosition As Integer = -1
        Dim firstPPS As PropertySet

        '#If DEBUG Then
        '        AcStopWatch.Start()
        '#End If

        Try

            Dim filter As TypedValue() = New TypedValue() {New TypedValue(0, "AEC*")}
            Dim psrAEC As PromptSelectionResult = SelectAllVisible(filter)

            Dim myPoly As Polyline = myTrans.GetObject(SelIdArray(0), OpenMode.ForRead)

            Dim allClosest As Double = Double.PositiveInfinity

            For Each x As SelectedObject In psrAEC.Value

                Dim Entity As Entity = myTransMan.GetObject(x.ObjectId, OpenMode.ForRead, False)

                Dim myPPS As PropertySet = Nothing

                myPPS = GetPropertySet(x.ObjectId, PSDName)

                If myPPS Is Nothing Then Continue For

                Dim closest As Double = Double.PositiveInfinity

                Dim myPoints As New Point3dCollection

                myPoly.IntersectWith(Entity, Intersect.OnBothOperands, myPoints, IntPtr.Zero, IntPtr.Zero)

                Dim myPointsCount As Integer = myPoints.Count

                If myPointsCount > 0 Then

                    For Each p In myPoints

                        Dim dist As Double = myPoly.GetDistAtPoint(p)

                        closest = Math.Min(closest, dist)

                    Next

                    allClosest = Math.Min(allClosest, closest)

                    'erster oder letzter
                    If myPointsCount Mod 2 <> 0 Then

                        If DistToFirstorLast > closest Then

                            FirstPosition = myAECEntPPS.Count

                            DistToFirstorLast = closest

                        End If

                    End If

                    myAECEntDistance.Add(closest)
                    myAECEntPPS.Add(myPPS)

                End If

            Next

            If DistToFirstorLast <> allClosest Then
                'firstposition ist der derletzt nicht der erste
                FirstPosition = -1
            End If


            If FirstPosition > -1 Then

                firstPPS = myAECEntPPS(FirstPosition)

                myAECEntDistance.RemoveAt(FirstPosition)
                myAECEntPPS.RemoveAt(FirstPosition)

            End If

            'bubble sort

            Dim TempDist As Double
            Dim TempPPS As PropertySet

            For outer = myAECEntDistance.Count - 1 To 0 Step -1

                For inner = 0 To outer - 1

                    If myAECEntDistance(inner) > myAECEntDistance(inner + 1) Then

                        TempDist = myAECEntDistance(inner)
                        TempPPS = myAECEntPPS(inner)

                        myAECEntDistance(inner) = myAECEntDistance(inner + 1)
                        myAECEntPPS(inner) = myAECEntPPS(inner + 1)

                        myAECEntDistance(inner + 1) = TempDist
                        myAECEntPPS(inner + 1) = TempPPS

                    End If

                Next

            Next

            If FirstPosition > -1 Then
                myAECEntPPS.Insert(0, firstPPS)
            End If

            'numbering now
            For i = 0 To myAECEntPPS.Count - 1

                SetPropertyValue(myAECEntPPS(i), myPropName, CountVal)

                CountVal += 1

            Next

            'remember for next time
            LastCountValue = CountVal

            myEd.WriteMessage(vbCrLf & "{0} objects from {1} to {2} numbered.", myAECEntPPS.Count, CountVal - myAECEntPPS.Count, CountVal - 1)

        Catch ex As Exception
            MsgBox("Error: " & vbCrLf & ex.Message.ToString, , "Property Renumber")
        Finally
            myTrans.Commit()
            myTrans.Dispose()
            myTransMan.Dispose()
        End Try

        '#If DEBUG Then
        '        AcStopWatch.Stop()
        '#End If

    End Sub

    Private Shared Function GetString(ByRef StringToGet As String, ByVal Message As String) As Boolean
        'select Property Name

        Dim ProRe As PromptResult = Application.DocumentManager.MdiActiveDocument.Editor.GetString(Message)

        If ProRe.Status = PromptStatus.OK Then
            If ProRe.StringResult <> "" Then
                StringToGet = ProRe.StringResult
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If

    End Function

    Public Shared Function SelectAllVisible(ByVal filter As TypedValue()) As PromptSelectionResult
        Dim ed As Editor = Application.DocumentManager.MdiActiveDocument.Editor
        Dim psr As PromptSelectionResult = Nothing

        Dim db As Database = HostApplicationServices.WorkingDatabase

        layers = New List(Of String)

        Using tr As Transaction = db.TransactionManager.StartTransaction()

            Dim lt As LayerTable = db.LayerTableId.GetObject(OpenMode.ForRead)

            For Each ltrId As ObjectId In lt
                Dim ltr As LayerTableRecord = tr.GetObject(ltrId, OpenMode.ForRead)
                If ltr.IsFrozen OrElse ltr.IsHidden OrElse ltr.IsOff Then
                    layers.Add(ltr.Name)
                End If
            Next
            tr.Commit()
        End Using

        Try
            AddHandler ed.SelectionAdded, AddressOf onSelectionAdded
            psr = ed.SelectAll(New SelectionFilter(filter))
        Finally
            RemoveHandler ed.SelectionAdded, AddressOf onSelectionAdded
        End Try
        Return psr
    End Function


    Private Shared layers As List(Of String)

    Private Shared Sub onSelectionAdded(ByVal sender As Object, ByVal e As SelectionAddedEventArgs)
        Dim db As Database = HostApplicationServices.WorkingDatabase
        Using tr As Transaction = db.TransactionManager.StartTransaction()

            Dim toRemove As New List(Of Integer)
            Dim ss As SelectionSet = e.AddedObjects

            For i As Integer = 0 To ss.Count - 1
                Dim br As Entity = ss(i).ObjectId.GetObject(OpenMode.ForRead)

                If layers.Contains(br.Layer) Then
                    toRemove.Add(i)
                End If
            Next

            For Each i As Integer In toRemove
                e.Remove(i)
            Next
            tr.Commit()
        End Using
    End Sub
End Class
