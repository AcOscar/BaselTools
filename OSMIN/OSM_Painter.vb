Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Geometry

Module OSM_Painter

    Public LayerList As List(Of String)

    Public CurrentStyle As ImportStyle

    Public CurrentStyleName As String

    Public lom As LongOperationManager

    Public myOSM As OSM

    Sub DrawWays()

        Dim myTransMan As DatabaseServices.TransactionManager
        Dim myTrans As DatabaseServices.Transaction
        Dim myDoc As ApplicationServices.Document
        Dim myBT As DatabaseServices.BlockTable
        Dim myBTR As DatabaseServices.BlockTableRecord
        Dim plID As ObjectId
        'Get the active document and begin a Transaction
        myDoc = Core.Application.DocumentManager.MdiActiveDocument

        Using docloc As DocumentLock = myDoc.LockDocument

            myTransMan = myDoc.TransactionManager
            myTrans = myTransMan.StartTransaction

            'Open the BlockTable for Read
            myBT = myDoc.Database.BlockTableId.GetObject(
                DatabaseServices.OpenMode.ForRead)
            myBTR = myBT(DatabaseServices.BlockTableRecord.ModelSpace).GetObject(
                DatabaseServices.OpenMode.ForWrite)

            For Each way As KeyValuePair(Of Long, OSM_Way) In myOSM.Ways
                ' Dim myPoints As New Geometry.Point2dCollection

                If way.Value.isCorrupt Then

                    Continue For

                End If

                'it means this way has a higher role in a relation
                If way.Value.tags.Count = 0 Then

                    Continue For

                End If

                Dim LayerName As String = GetLayerName(way.Value)

                CreateLayerIfNotExist(LayerName)

                Dim mypline As New Polyline(way.Value.NodeIds.Count)

                For Each nodeId As Long In way.Value.NodeIds

                    Dim oNode As New OSM_Node

                    oNode = myOSM.Nodes(nodeId)

                    mypline.AddVertexAt(mypline.NumberOfVertices, New Geometry.Point2d(oNode.X, oNode.Y), 0, 0, 0)

                    lom.Tick()

                Next

                plID = myBTR.AppendEntity(mypline)

                myTrans.AddNewlyCreatedDBObject(mypline, True)

                mypline.Layer = LayerName

                If way.Value.isArea Then

                    Try

                        Dim acHatchLoopObjIdColl As ObjectIdCollection = New ObjectIdCollection()
                        acHatchLoopObjIdColl.Add(plID)

                        Dim acHatch As Hatch = New Hatch()
                        myBTR.AppendEntity(acHatch)
                        myTrans.AddNewlyCreatedDBObject(acHatch, True)

                        '' Set the properties of the hatch object
                        '' Associative must be set after the hatch object is appended to the 
                        '' block table record and before AppendLoop
                        acHatch.SetDatabaseDefaults()
                        acHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID")
                        acHatch.Associative = False
                        acHatch.AppendLoop(HatchLoopTypes.Outermost, acHatchLoopObjIdColl)
                        acHatch.EvaluateHatch(True)

                        acHatch.Layer = LayerName
                        'mypline.Erase()

                    Catch ex As Exception

                    End Try

                End If

                If lom.cancelled Then

                    'Commit the Transaction
                    myTrans.Commit()

                    myTrans.Dispose()
                    myTransMan.Dispose()

                    Exit Sub

                End If

            Next

            'Commit the Transaction
            myTrans.Commit()

            myTrans.Dispose()
            myTransMan.Dispose()

        End Using

    End Sub

    Sub DrawRelations()

        Dim myTransMan As DatabaseServices.TransactionManager
        Dim myTrans As DatabaseServices.Transaction
        Dim myDoc As ApplicationServices.Document
        Dim myBT As DatabaseServices.BlockTable
        Dim myBTR As DatabaseServices.BlockTableRecord
        Dim plID As ObjectId
        'Get the active document and begin a Transaction
        myDoc = Core.Application.DocumentManager.MdiActiveDocument

        Using docloc As DocumentLock = myDoc.LockDocument

            myTransMan = myDoc.TransactionManager
            myTrans = myTransMan.StartTransaction

            'Open the BlockTable for Read
            myBT = myDoc.Database.BlockTableId.GetObject(DatabaseServices.OpenMode.ForRead)

            myBTR = myBT(DatabaseServices.BlockTableRecord.ModelSpace).GetObject(DatabaseServices.OpenMode.ForWrite)

            'For Each Relation As OSM_Relation In myOSM.Relations
            For Each Relation As KeyValuePair(Of Long, OSM_Relation) In myOSM.Relations

                Dim LayerName As String = GetLayerName(Relation.Value)

                CreateLayerIfNotExist(LayerName)

                If Relation.Value.isMultipolygon Then

                    Dim acHatchLoopObjIdColl As ObjectIdCollection = New ObjectIdCollection()

                    For Each p In Relation.Value.Loops4AutoCAD

                        If p.NodeIds.Count = 0 Then

                            Continue For

                        End If

                        Dim mypline As New Polyline(p.NodeIds.Count)

                        For Each nd In p.NodeIds

                            mypline.AddVertexAt(mypline.NumberOfVertices, New Geometry.Point2d(myOSM.Nodes(nd).X, myOSM.Nodes(nd).Y), 0, 0, 0)

                            lom.Tick()

                        Next

                        mypline.Layer = LayerName

                        myBTR.AppendEntity(mypline)

                        If mypline.StartPoint = mypline.EndPoint Then

                            mypline.Closed = True

                            acHatchLoopObjIdColl.Add(mypline.ObjectId)

                        End If

                        myTrans.AddNewlyCreatedDBObject(mypline, True)

                    Next p

                    If Relation.Value.isArea Then
                        If acHatchLoopObjIdColl.Count > 0 Then

                            Try

                                Dim acHatch As Hatch = New Hatch()

                                myBTR.AppendEntity(acHatch)

                                myTrans.AddNewlyCreatedDBObject(acHatch, True)

                                '' Set the properties of the hatch object
                                '' Associative must be set after the hatch object is appended to the 
                                '' block table record and before AppendLoop
                                acHatch.SetDatabaseDefaults()
                                acHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID")
                                acHatch.Associative = False

                                acHatch.HatchStyle = HatchStyle.Normal

                                For Each objectId In acHatchLoopObjIdColl
                                    Dim ids = New ObjectIdCollection()
                                    ids.Add(objectId)
                                    acHatch.AppendLoop(HatchLoopTypes.Outermost, ids)
                                Next

                                acHatch.EvaluateHatch(True)

                                acHatch.Layer = LayerName

                            Catch ex As Exception

                                Diagnostics.Debug.Print(Relation.Key)

                            End Try

                        End If

                    End If

                Else 'If Relation.Value.isMultipolygon Then
                    Dim myPoints As New Geometry.Point2dCollection

                    For Each member In Relation.Value.Members

                        myPoints = New Geometry.Point2dCollection

                        Select Case member.type.ToLower

                            Case "way"
                                For Each ndId In member.Way.NodeIds

                                    ' myPoints.Add(New Geometry.Point2d(nd.X, nd.Y))
                                    myPoints.Add(New Geometry.Point2d(myOSM.Nodes(ndId).X, myOSM.Nodes(ndId).Y))

                                    lom.Tick()

                                Next

                                'Case "node"
                                '    Continue For
                                'Case "relation"
                                '    Continue For

                            Case Else
                                Continue For

                        End Select


                        Dim mypline As New Polyline(myPoints.Count)

                        For Each p As Geometry.Point2d In myPoints

                            mypline.AddVertexAt(mypline.NumberOfVertices, p, 0, 0, 0)

                        Next

                        If Relation.Value.isArea Then

                            mypline.Closed = True

                        End If

                        plID = myBTR.AppendEntity(mypline)

                        mypline.Layer = LayerName

                        myTrans.AddNewlyCreatedDBObject(mypline, True)

                        If Relation.Value.isArea Then

                            Try

                                Dim acHatchLoopObjIdColl As ObjectIdCollection = New ObjectIdCollection()
                                acHatchLoopObjIdColl.Add(plID)

                                Dim acHatch As Hatch = New Hatch()
                                myBTR.AppendEntity(acHatch)
                                myTrans.AddNewlyCreatedDBObject(acHatch, True)

                                '' Set the properties of the hatch object
                                '' Associative must be set after the hatch object is appended to the 
                                '' block table record and before AppendLoop
                                acHatch.SetDatabaseDefaults()
                                acHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID")
                                acHatch.Associative = False
                                acHatch.AppendLoop(HatchLoopTypes.Outermost, acHatchLoopObjIdColl)
                                acHatch.EvaluateHatch(True)

                                acHatch.Layer = LayerName
                                'mypline.Erase()

                            Catch ex As Exception

                            End Try

                        End If

                    Next

                End If 'If Relation.Value.isMultipolygon Then
                'lom.Tick()

                If lom.cancelled Then

                    myTrans.Commit()

                    myTrans.Dispose()

                    myTransMan.Dispose()

                    Exit Sub

                End If
            Next

            'Commit the Transaction
            myTrans.Commit()

            myTrans.Dispose()

            myTransMan.Dispose()

        End Using
    End Sub

    Sub DrawNodes()

        For Each n As Long In myOSM.Nodes2Insert

            insertNode(n)

            lom.Tick()

            If lom.cancelled Then Exit Sub

        Next

    End Sub

    Sub CreateLayerIfNotExist(ByRef LayerName As String)

        If LayerName = "" Then

            LayerName = CurrentStyle.LayerPrefix.ToUpper

        End If

        LayerName = SymbolUtilityServices.RepairSymbolName(LayerName.ToUpper, False)

        If Not LayerList.Contains(LayerName) Then

            Dim myDWG As ApplicationServices.Document
            Dim myDB As DatabaseServices.Database
            Dim myTransMan As DatabaseServices.TransactionManager
            Dim myTrans As DatabaseServices.Transaction

            myDWG = Core.Application.DocumentManager.MdiActiveDocument
            myDB = myDWG.Database
            myTransMan = myDWG.TransactionManager
            myTrans = myTransMan.StartTransaction

            Dim myLT As DatabaseServices.LayerTable

            myLT = myDB.LayerTableId.GetObject(DatabaseServices.OpenMode.ForWrite)

            Dim myLayer As DatabaseServices.LayerTableRecord

            myLayer = CurrentStyle.GetLayer(LayerName.ToUpper)

            'myLayer.Name can be defer from Layername and Layername ist byRef for later using
            LayerName = myLayer.Name

            myLT.Add(myLayer)

            myTrans.AddNewlyCreatedDBObject(myLayer, True)

            myTrans.Commit()

            LayerList.Add(myLayer.Name.ToUpper)

            myTrans.Dispose()
            myTransMan.Dispose()

        End If

    End Sub

    Sub DrawImportedBoundAndCredit()
        ' Get the current document and database
        Dim acDoc As Document = Application.DocumentManager.MdiActiveDocument
        Dim acCurDb As Database = acDoc.Database

        Dim Height As Double = 4

        Using docloc As DocumentLock = acDoc.LockDocument

            ' Start a transaction
            Using acTrans As Transaction = acCurDb.TransactionManager.StartTransaction()

                'Dim Layername As String = "OSM_IMPORT_BOUNDARY"
                Dim Layername As String = CurrentStyle.LicenseLayer

                CreateLayerIfNotExist(Layername)

                ' Open the Block table for read
                Dim acBlkTbl As BlockTable
                acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead)

                ' Open the Block table record Model space for write
                Dim acBlkTblRec As BlockTableRecord
                acBlkTblRec = acTrans.GetObject(acBlkTbl(BlockTableRecord.ModelSpace), OpenMode.ForWrite)

                ' Create a polyline with two segments (3 points)
                Dim acPoly As Polyline = New Polyline()
                acPoly.SetDatabaseDefaults()

                Dim minX, minY, maxX, maxY As Double

                minX = myOSM.Settings.BoundMin.X
                minY = myOSM.Settings.BoundMin.Y

                maxX = myOSM.Settings.BoundMax.X
                maxY = myOSM.Settings.BoundMax.Y


                'add credit
                '© OpenStreetMap contributors

                'one hundreth of the height of the box
                Height = (maxY - minY) / 100

                Dim acMText As MText = New MText()
                acMText.SetDatabaseDefaults()
                acMText.Location = New Point3d(maxX, minY, 0)
                acMText.TextHeight = Height
                acMText.Contents = CurrentStyle.License

                acMText.Attachment = AttachmentPoint.BottomRight
                acMText.BackgroundFill = True

                acMText.Layer = Layername

                acBlkTblRec.AppendEntity(acMText)
                acTrans.AddNewlyCreatedDBObject(acMText, True)

                'a boundary box
                acPoly.AddVertexAt(0, New Point2d(minX, minY), 0, 0, 0)
                acPoly.AddVertexAt(1, New Point2d(minX, maxY), 0, 0, 0)
                acPoly.AddVertexAt(2, New Point2d(maxX, maxY), 0, 0, 0)
                acPoly.AddVertexAt(3, New Point2d(maxX, minY), 0, 0, 0)

                acPoly.Closed = True


                acPoly.Layer = Layername

                ' Add the new object to the block table record and the transaction
                acBlkTblRec.AppendEntity(acPoly)
                acTrans.AddNewlyCreatedDBObject(acPoly, True)

                '' Save the new object to the database
                acTrans.Commit()

            End Using

        End Using

    End Sub

    Sub ZommToBound()

        Dim myMinPoint As New Point2d(myOSM.Settings.BoundMin.X, myOSM.Settings.BoundMin.Y)
        Dim myMaxPoint As New Point2d(myOSM.Settings.BoundMax.X, myOSM.Settings.BoundMax.Y)

        ZoomWin(myMinPoint, myMaxPoint)

    End Sub

    Private Sub ZoomWin(ByVal min As Point2d, ByVal max As Point2d)

        Dim ed As Editor = Core.Application.DocumentManager.MdiActiveDocument.Editor

        Dim min2d As Point2d = New Point2d(min.X, min.Y)
        Dim max2d As Point2d = New Point2d(max.X, max.Y)

        Dim view As ViewTableRecord = New ViewTableRecord()

        view.CenterPoint = min2d + ((max2d - min2d) / 2.0)
        view.Height = max2d.Y - min2d.Y
        view.Width = max2d.X - min2d.X

        ed.SetCurrentView(view)

    End Sub

    ''' <summary>
    ''' fill LayerList with all existend Layernames
    ''' </summary>
    ''' <remarks>we need this for performance reaseons instead of permantly asking the dwg database</remarks>
    Sub LoadLayerList()

        LayerList = New List(Of String)

        Dim myDWG As ApplicationServices.Document
        Dim myDB As DatabaseServices.Database
        Dim myTransMan As DatabaseServices.TransactionManager
        Dim myTrans As DatabaseServices.Transaction

        myDWG = Core.Application.DocumentManager.MdiActiveDocument
        myDB = myDWG.Database
        myTransMan = myDWG.TransactionManager
        myTrans = myTransMan.StartTransaction

        Dim myLT As DatabaseServices.LayerTable
        Dim myLTR As DatabaseServices.LayerTableRecord
        Dim mySTE As DatabaseServices.SymbolTableEnumerator

        myLT = myDB.LayerTableId.GetObject(DatabaseServices.OpenMode.ForRead)
        mySTE = myLT.GetEnumerator

        While mySTE.MoveNext
            myLTR = mySTE.Current.GetObject(DatabaseServices.OpenMode.ForRead)
            LayerList.Add(myLTR.Name.ToUpper)

        End While

        myTrans.Dispose()
        myTransMan.Dispose()

    End Sub

    Function GetLayerName(ByVal way As OSM_Way) As String

        Dim myLayerName As String = String.Empty

        If way.tags.Count > 0 Then

            'For Each s In way.tags

            '    If CurrentStyle.LayerKeyNames.Contains(s.Key.ToLower) Then

            '        myLayerName = IIf(CurrentStyle.LayerPrefix = "", "", CurrentStyle.LayerPrefix & "_") & s.Key & "_" & s.Value

            '        Exit For

            '    End If

            'Next


            Dim keylist As New List(Of String)(way.tags.Keys)

            For Each k In CurrentStyle.LayerKeyNames

                If keylist.Contains(k) Then

                    Dim t As String

                    t = way.tags(k)

                    myLayerName = IIf(CurrentStyle.LayerPrefix = "", "", CurrentStyle.LayerPrefix & "_") & k & "_" & t

                    Exit For

                End If

            Next

        End If

        Return myLayerName

    End Function

    Function GetLayerName(ByVal Relation As OSM_Relation) As String

        Dim myLayerName As String = String.Empty

        Dim keylist As New List(Of String)(Relation.tags.Keys)

        If Relation.tags.Count > 0 Then

            For Each k In CurrentStyle.LayerKeyNames

                If keylist.Contains(k) Then

                    Dim t As String

                    t = Relation.tags(k)

                    myLayerName = IIf(CurrentStyle.LayerPrefix = "", "", CurrentStyle.LayerPrefix & "_") & k & "_" & t

                    Return myLayerName

                End If
            Next

        End If

        'if its empty we looking in the ways at in hope to find usefull infos there

        For Each m In Relation.Members

            If m.Way IsNot Nothing Then

                keylist = New List(Of String)(m.Way.tags.Keys)

                For Each k In CurrentStyle.LayerKeyNames

                    If keylist.Contains(k) Then

                        Dim t As String

                        t = m.Way.tags(k)

                        myLayerName = IIf(CurrentStyle.LayerPrefix = "", "", CurrentStyle.LayerPrefix & "_") & k & "_" & t

                        Return myLayerName

                    End If
                Next

            End If

        Next

        Return myLayerName

    End Function

    ''' <summary>
    ''' creates a blockdefiniton for a single node insertion
    ''' </summary>
    ''' <remarks>if a blockdefiniton already exists we do nothing he4r, this gives us the oportunitie
    ''' to work in a template with predifined synmbols for parking, telephone etc.</remarks>
    Sub CreateNodePointIfNotExist(ByVal BlockName As String)

        Dim myDwg As Document
        Dim myBT As BlockTable

        Dim myTransMan As DatabaseServices.TransactionManager
        Dim myTrans As DatabaseServices.Transaction

        myDwg = Application.DocumentManager.MdiActiveDocument
        myTransMan = myDwg.TransactionManager
        myTrans = myTransMan.StartTransaction

        Using docloc As DocumentLock = myDwg.LockDocument

            myBT = myDwg.Database.BlockTableId.GetObject(OpenMode.ForWrite)

            If Not myBT.Has(BlockName) Then

                Dim myBlockDef As BlockTableRecord

                'create a new one
                myBlockDef = New BlockTableRecord

                myBlockDef.Name = BlockName

                Dim myPoint As Point3d = New Point3d(0, 0, 0)


                Dim dbpoint As New DBPoint(myPoint)

                dbpoint.Layer = "0"

                'myBlockDef.AppendEntity(New DBPoint(New Point3d(0, 0, 0)))

                myBlockDef.AppendEntity(dbpoint)

                Dim myAttr As New DatabaseServices.AttributeDefinition(
                                   New Geometry.Point3d(0, 0, 0),
                                   "", "Tag", "", Nothing)

                myAttr.Layer = "0"

                myAttr.IsMTextAttributeDefinition = True

                myBlockDef.AppendEntity(myAttr)

                myBT.Add(myBlockDef)

                'Commit the Transaction
                myTrans.AddNewlyCreatedDBObject(myBlockDef, True)

            End If

            myTrans.Commit()
            myTrans.Dispose()
            myTransMan.Dispose()

        End Using

    End Sub

    Private Sub insertNode(ByVal key As Long)

        Dim oNode As New OSM_Node

        myOSM.Nodes.TryGetValue(key, oNode)

        Dim insertPoint As New Point3d(oNode.X, oNode.Y, 0)

        Dim BlockName As String = getBlockName(oNode)

        BlockName = SymbolUtilityServices.RepairSymbolName(BlockName, False)

        Dim TagText As String = getTagText(oNode)

        CreateNodePointIfNotExist(BlockName)

        CreateLayerIfNotExist(CurrentStyle.NodeLayername)

        SetAttributes(InsertNodeBlock(insertPoint, BlockName), TagText)

    End Sub

    Private Function getTagText(ByVal n As OSM_Node) As String

        Dim myText As New Text.StringBuilder

        For Each t In n.tags

            myText.Append(t.Key)

            myText.Append(": ")

            myText.Append(t.Value)

            myText.Append(vbCrLf)

        Next

        myText.Remove(myText.Length - 2, 2)

        Return myText.ToString

    End Function

    Private Function getBlockName(ByVal n As OSM_Node) As String

        Dim myNodeName As String = String.Empty

        If n.tags.Count > 0 Then

            For Each s In n.tags

                If CurrentStyle.NodeTagKeys.Contains(s.Key.ToLower) Then

                    myNodeName = IIf(CurrentStyle.LayerPrefix = "", "", CurrentStyle.LayerPrefix & "_") & s.Key & "_" & s.Value

                    Exit For

                End If

            Next

        End If

        Return myNodeName

    End Function

    Public Function InsertNodeBlock(ByVal InsPt As Geometry.Point3d,
    ByVal BlockName As String) As DatabaseServices.ObjectId

        Dim myTransMan As DatabaseServices.TransactionManager
        Dim myTrans As DatabaseServices.Transaction
        Dim myDwg As Document
        Dim myBT As BlockTable
        Dim myBTR As BlockTableRecord

        myDwg = Application.DocumentManager.MdiActiveDocument
        myTransMan = myDwg.TransactionManager
        myTrans = myTransMan.StartTransaction
        Using docloc As DocumentLock = myDwg.LockDocument

            'Open the database for Write
            myBT = myDwg.Database.BlockTableId.GetObject(OpenMode.ForRead)
            myBTR = myBT(BlockTableRecord.ModelSpace).GetObject(OpenMode.ForWrite)

            'Insert the Block
            Dim myBlockDef As BlockTableRecord = myBT(BlockName).GetObject(OpenMode.ForRead)
            Dim myBlockRef As New DatabaseServices.BlockReference(InsPt, myBT(BlockName))

            myBlockRef.Layer = CurrentStyle.NodeLayername

            myBTR.AppendEntity(myBlockRef)
            myTrans.AddNewlyCreatedDBObject(myBlockRef, True)

            'Set the Attribute Value
            Dim myAttColl As DatabaseServices.AttributeCollection
            Dim myEnt As DatabaseServices.Entity
            Dim myBTREnum As BlockTableRecordEnumerator
            myAttColl = myBlockRef.AttributeCollection
            myBTREnum = myBlockDef.GetEnumerator
            While myBTREnum.MoveNext
                myEnt = myBTREnum.Current.GetObject(OpenMode.ForWrite)
                If TypeOf myEnt Is DatabaseServices.AttributeDefinition Then
                    Dim myAttDef As DatabaseServices.AttributeDefinition = myEnt
                    Dim myAttRef As New DatabaseServices.AttributeReference
                    myAttRef.SetAttributeFromBlock(myAttDef, myBlockRef.BlockTransform)
                    myAttRef.Layer = "0"
                    myAttColl.AppendAttribute(myAttRef)
                    myTrans.AddNewlyCreatedDBObject(myAttRef, True)
                End If
            End While

            'Commit the Transaction
            myTrans.Commit()

            'Dispose of the Transaction Objects
            myTrans.Dispose()
            myTransMan.Dispose()
            Return myBlockRef.ObjectId

        End Using

    End Function

    Public Sub SetAttributes(ByVal BlockID As DatabaseServices.ObjectId, ByVal Text As String)
        Dim myDwg As Document
        myDwg = Application.DocumentManager.MdiActiveDocument

        Dim myTransMan As DatabaseServices.TransactionManager
        Dim myTrans As DatabaseServices.Transaction

        myTransMan = myDwg.TransactionManager
        myTrans = myTransMan.StartTransaction

        Using docloc As DocumentLock = myDwg.LockDocument

            Dim myBlkRef As DatabaseServices.BlockReference
            Dim myAttColl As DatabaseServices.AttributeCollection

            myBlkRef = BlockID.GetObject(OpenMode.ForWrite)
            myAttColl = myBlkRef.AttributeCollection

            Dim myEnt As DatabaseServices.ObjectId
            Dim myAttRef As DatabaseServices.AttributeReference

            For Each myEnt In myAttColl
                myAttRef = myEnt.GetObject(OpenMode.ForWrite)

                If myAttRef.Tag = "Tag" Then
                    myAttRef.TextString = Text
                End If

                myAttRef.Layer = "0"

            Next

            myTrans.Commit()
            myTrans.Dispose()
            myTransMan.Dispose()

        End Using

    End Sub

    Sub buildNodes2Insert()

        For Each n As KeyValuePair(Of Long, OSM_Node) In myOSM.Nodes

            If getBlockName(n.Value) = String.Empty Then
                'we have much more nodes which we not have to insert, so that we can ignoring the tick now and tick them while we insert them
                lom.Tick()

            Else

                myOSM.Nodes2Insert.Add(n.Key)

            End If

            If lom.cancelled Then Exit Sub

        Next

    End Sub

End Module
