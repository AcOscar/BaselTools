#Region "Namespace"
Imports System.IO
Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.Runtime

Imports Autodesk.AutoCAD.ApplicationServices.DocumentCollectionExtension
Imports System.Windows.Forms

#End Region
'by Chuck Gabriel found at: https : //www.theswamp.org/index.php?topic=31863.0$

Public Class XrefTools

    Public Delegate Sub ProcessSingleXref(ByVal btr As BlockTableRecord)

    Public Delegate Sub ProcessMultipleXrefs(ByVal xrefIds As ObjectIdCollection)

    Public Shared Sub detachXref(ByVal btr As BlockTableRecord)

        Core.Application.DocumentManager.MdiActiveDocument.Database.DetachXref(btr.ObjectId)

    End Sub

    Public Shared Sub openXref(ByVal btr As BlockTableRecord)

        Dim xrefPath As String = btr.PathName

        If xrefPath.Contains(".\") Then

            Dim hostPath As String = Core.Application.DocumentManager.MdiActiveDocument.Database.Filename

            Directory.SetCurrentDirectory(Path.GetDirectoryName(hostPath))

            xrefPath = Path.GetFullPath(xrefPath)

        End If

        If Not File.Exists(xrefPath) Then

            Return

        End If

        Dim doc As Document = Core.Application.DocumentManager.Open(xrefPath, False)

        If doc.IsReadOnly Then

            MessageBox.Show(doc.Name + " opened in read-only mode.", "OpenXrefs", MessageBoxButtons.OK, MessageBoxIcon.Warning)

        End If

    End Sub

    Public Shared Sub bindInsertXrefs(ByVal xrefIds As ObjectIdCollection)

        Core.Application.DocumentManager.MdiActiveDocument.Database.BindXrefs(xrefIds, True)

    End Sub

    Public Shared Sub bindBindtXrefs(ByVal xrefIds As ObjectIdCollection)

        Core.Application.DocumentManager.MdiActiveDocument.Database.BindXrefs(xrefIds, False)

    End Sub

    Public Shared Sub BindInsertExplodeXrefs(ByVal xrefIds As ObjectIdCollection)

        'Die XrefNamen merken zum Explodieren
        Dim myNames As New List(Of String)

        Dim db As Database = Core.Application.DocumentManager.MdiActiveDocument.Database

        Using tr As Transaction = db.TransactionManager.StartTransaction()

            For Each ObId As ObjectId In xrefIds

                Dim myXREF As BlockTableRecord = tr.GetObject(ObId, OpenMode.ForRead)

                myNames.Add(myXREF.Name)

            Next

        End Using

        'die Xrefs inserten
        Core.Application.DocumentManager.MdiActiveDocument.Database.BindXrefs(xrefIds, True)

        Try

            Using tr As Transaction = db.TransactionManager.StartTransaction()

                Dim myBT As DatabaseServices.BlockTable
                Dim myBTR As DatabaseServices.BlockTableRecord
                Dim myBTRE As DatabaseServices.SymbolTableEnumerator

                myBT = db.BlockTableId.GetObject(OpenMode.ForRead)
                myBTRE = myBT.GetEnumerator

                While myBTRE.MoveNext

                    myBTR = myBTRE.Current.GetObject(OpenMode.ForRead)

                    If myBTR.IsAnonymous = False And myBTR.IsLayout = False Then

                        For Each BlockName In myNames

                            If myBTR.Name = BlockName Then

                                Dim myObjIDs As DatabaseServices.ObjectIdCollection

                                myObjIDs = myBTR.GetBlockReferenceIds(False, False)

                                If myObjIDs.Count > 0 Then

                                    For I = 1 To myObjIDs.Count

                                        Dim myBRef As DatabaseServices.BlockReference

                                        myBRef = myObjIDs.Item(I - 1).GetObject(OpenMode.ForWrite)

                                        Dim acDBObjColl As DBObjectCollection = New DBObjectCollection()

                                        myBRef.ExplodeToOwnerSpace()
                                        myBRef.Erase()

                                        tr.Commit()

                                    Next

                                End If

                            End If

                        Next

                    End If

                End While

            End Using

        Catch ex As Exception

        End Try

    End Sub

    Public Shared Sub reloadXrefs(ByVal xrefIds As ObjectIdCollection)

        Core.Application.DocumentManager.MdiActiveDocument.Database.ReloadXrefs(xrefIds)

    End Sub

    Public Shared Sub unloadXrefs(ByVal xrefIds As ObjectIdCollection)

        Core.Application.DocumentManager.MdiActiveDocument.Database.UnloadXrefs(xrefIds)

    End Sub

    Public Shared Sub processXrefs(ByVal promptMessage As String, ByVal process As ProcessSingleXref)

        Dim myEd As Editor = Core.Application.DocumentManager.MdiActiveDocument.Editor

        Dim filterList As TypedValue() = {New TypedValue(0, "INSERT")}

        myEd.WriteMessage(promptMessage)

        Dim result As PromptSelectionResult = myEd.GetSelection(New SelectionFilter(filterList))

        If result.Status <> PromptStatus.OK Then
            Return
        End If

        Dim ids As ObjectId() = result.Value.GetObjectIds()

        Dim db As Database = Core.Application.DocumentManager.MdiActiveDocument.Database

        Using tr As Transaction = db.TransactionManager.StartTransaction()

            Dim xrefIds As New ObjectIdCollection()

            For Each id As ObjectId In ids

                Dim blockRef As BlockReference = DirectCast(tr.GetObject(id, OpenMode.ForRead, False, True), BlockReference)
                Dim bId As ObjectId = blockRef.BlockTableRecord

                If Not xrefIds.Contains(bId) Then

                    xrefIds.Add(bId)

                    Dim btr As BlockTableRecord = DirectCast(tr.GetObject(bId, OpenMode.ForRead), BlockTableRecord)

                    If btr.IsFromExternalReference Then

                        process(btr)

                    End If

                End If

            Next

            tr.Commit()

        End Using

    End Sub

    Public Shared Sub processXrefs(ByVal promptMessage As String, ByVal process As ProcessMultipleXrefs)

        Dim myEd As Editor = Core.Application.DocumentManager.MdiActiveDocument.Editor

        Dim filterList As TypedValue() = {New TypedValue(0, "INSERT")}

        myEd.WriteMessage(promptMessage)

        Dim result As PromptSelectionResult = myEd.GetSelection(New SelectionFilter(filterList))

        If result.Status <> PromptStatus.OK Then

            Return

        End If

        Dim ids As ObjectId() = result.Value.GetObjectIds()

        Dim db As Database = Core.Application.DocumentManager.MdiActiveDocument.Database

        Using tr As Transaction = db.TransactionManager.StartTransaction()

            Dim blockIds As New ObjectIdCollection()

            For Each id As ObjectId In ids

                Dim blockRef As BlockReference = DirectCast(tr.GetObject(id, OpenMode.ForRead, False, True), BlockReference)

                blockIds.Add(blockRef.BlockTableRecord)

            Next

            Dim xrefIds As ObjectIdCollection = filterXrefIds(blockIds)

            If xrefIds.Count <> 0 Then

                process(xrefIds)

            End If

            tr.Commit()

        End Using

    End Sub

    Public Shared Sub attachXrefs(ByVal fileNames As String())

        Dim myEd As Editor = Core.Application.DocumentManager.MdiActiveDocument.Editor

        Array.Sort(fileNames)

        Dim db As Database = Core.Application.DocumentManager.MdiActiveDocument.Database
        Dim dimScale As Double = db.Dimscale

        For Each fileName As String In fileNames

            Dim options As New PromptPointOptions("Pick insertion point for " & fileName & ": ")

            options.AllowNone = False

            Dim pt As PromptPointResult = myEd.GetPoint(options)

            If pt.Status <> PromptStatus.OK Then

                Continue For

            End If

            Dim xrefScale As Double = getDwgScale(fileName)
            Dim scaleFactor As Double = dimScale / xrefScale

            Using tr As Transaction = Core.Application.DocumentManager.MdiActiveDocument.TransactionManager.StartTransaction()

                Dim xrefId As ObjectId = db.AttachXref(fileName, Path.GetFileNameWithoutExtension(fileName))

                Dim blockRef As New BlockReference(pt.Value, xrefId)
                Dim layoutBlock As BlockTableRecord = DirectCast(tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite), BlockTableRecord)

                blockRef.ScaleFactors = New Scale3d(scaleFactor, scaleFactor, scaleFactor)
                blockRef.Layer = "0"

                layoutBlock.AppendEntity(blockRef)

                tr.AddNewlyCreatedDBObject(blockRef, True)
                tr.Commit()

            End Using

        Next

    End Sub

    Public Shared Function getDwgScale(ByVal fileName As String) As Double
        Using db As New Database(False, True)

            db.ReadDwgFile(fileName, FileOpenMode.OpenForReadAndAllShare, False, String.Empty)

            Return db.Dimscale

        End Using

    End Function

    Public Shared Function filterXrefIds(ByVal blockIds As ObjectIdCollection) As ObjectIdCollection

        Dim xrefIds As New ObjectIdCollection()

        For Each bId As ObjectId In blockIds

            If Not xrefIds.Contains(bId) Then

                Dim btr As BlockTableRecord = DirectCast(bId.GetObject(OpenMode.ForRead), BlockTableRecord)

                If btr.IsFromExternalReference Then
                    xrefIds.Add(bId)
                End If

            End If

        Next

        Return xrefIds

    End Function

End Class
