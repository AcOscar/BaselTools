Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Runtime
Imports Autodesk.Aec.PropertyData.DatabaseServices

'SHORTCUTS:
Imports AcadDb = Autodesk.AutoCAD.DatabaseServices
Imports AecDb = Autodesk.Aec.DatabaseServices
Imports AecPropDb = Autodesk.Aec.PropertyData.DatabaseServices
Imports DBTransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager

Imports ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId
Imports ObjectIdCollection = Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection

Public Class SelectByProperty

    ' Shared strlist = New List(Of String)

    Shared Function getallpropertysets() As List(Of String)
        Dim db As Database = Application.DocumentManager.MdiActiveDocument.Database
        Dim tm As DBTransactionManager = db.TransactionManager
        Dim strlist = New List(Of String)
        Using mytrans As Transaction = db.TransactionManager.StartTransaction
            Try
                Dim psdDict As AecPropDb.DictionaryPropertySetDefinitions = New AecPropDb.DictionaryPropertySetDefinitions(db)
                For r = 0 To psdDict.Records.Count - 1
                    Dim psdid As ObjectId = psdDict.Records.Item(r)
                    Dim prset As AecPropDb.PropertySetDefinition = mytrans.GetObject(psdid, OpenMode.ForRead)
                    strlist.Add(prset.Name)
                Next
            Catch e As Autodesk.AutoCAD.Runtime.Exception
                Debug.WriteLine(e.Message)
            End Try
            mytrans.Commit()
        End Using

        Return strlist

    End Function

    Shared Function getallproperties(ByVal psetname As String) As List(Of String)
        Dim strlist As New List(Of String)
        Dim db As Database = Application.DocumentManager.MdiActiveDocument.Database
        Dim tm As DBTransactionManager = db.TransactionManager
        Using mytrans As Transaction = db.TransactionManager.StartTransaction
            Dim psdId As ObjectId = GetPropertySetIdByName(db, tm, mytrans, psetname)

            Dim prset As AecPropDb.PropertySetDefinition = mytrans.GetObject(psdId, OpenMode.ForRead)
            For p = 0 To prset.Definitions.Count - 1
                Dim prtagname As String = prset.Definitions.Item(p).Name
                strlist.Add(prtagname)
            Next
            mytrans.Commit()
        End Using
        Return strlist
    End Function

    Shared Function GetPropertySetIdByName(ByRef mydb As Database, ByRef mytm As DBTransactionManager, ByRef mytr As Transaction, ByVal psdName As String) As ObjectId
        Dim psdId As ObjectId = ObjectId.Null

        Dim psdDict As AecPropDb.DictionaryPropertySetDefinitions = New AecPropDb.DictionaryPropertySetDefinitions(mydb)
        If psdDict.Has(psdName, mytr) Then
            psdId = psdDict.GetAt(psdName)
        End If

        Return psdId

    End Function

    Shared Function GetPropertyValue(ByRef mytm As DBTransactionManager, ByRef mytr As Transaction, ByVal id As ObjectId, ByVal psdname As String, ByVal prtag As String) As String

        Dim exvalue As String = "[NULL!]"

        Dim setIds As ObjectIdCollection = New ObjectIdCollection()

        Dim obj As Object = mytm.GetObject(id, OpenMode.ForRead, False, False)

        If Not TypeOf obj Is AecDb.Entity Then

            GetPropertyValue = exvalue

            Exit Function

        End If

        Dim ent As AecDb.Entity = obj

        ' get the property sets on the object
        setIds = PropertyDataServices.GetPropertySets(ent)

        For s = 0 To setIds.Count - 1

            Try

                Dim prset As AecPropDb.PropertySet = mytr.GetObject(setIds.Item(s), OpenMode.ForRead)

                If prset.PropertySetDefinitionName = psdname Then

                    Dim ppos As Integer = prset.PropertyNameToId(prtag)

                    exvalue = prset.GetValueAndUnitAt(ppos).Value.ToString

                End If

            Catch e As Autodesk.AutoCAD.Runtime.Exception

                ' most likely eKeyNotfound
                'Debug.WriteLine(e.Message)

            End Try

        Next

        If exvalue = "[NULL!]" Then
            setIds = GetPropertySetsFromStyle(mytm, mytr, id)
            For s = 0 To setIds.Count - 1
                Try
                    Dim prset As AecPropDb.PropertySet = mytr.GetObject(setIds.Item(s), OpenMode.ForRead)
                    If prset.PropertySetDefinitionName = psdname Then
                        Dim ppos As Integer = prset.PropertyNameToId(prtag)
                        exvalue = prset.GetValueAndUnitAt(ppos).Value.ToString
                    End If

                Catch e As Autodesk.AutoCAD.Runtime.Exception
                    ' most likely eKeyNotfound
                    Debug.WriteLine(e.Message)
                End Try
            Next

        End If

        GetPropertyValue = exvalue

    End Function

    Shared Function GetPropertySetsFromStyle(ByRef mytm As DBTransactionManager, ByRef mytr As Transaction, ByVal id As ObjectId) As ObjectIdCollection

        Dim setIdsex = New ObjectIdCollection()

        Try

            Dim obj As Object = mytm.GetObject(id, OpenMode.ForRead, False, False)
            If Not TypeOf obj Is AecDb.Entity Then
                GetPropertySetsFromStyle = setIdsex
                Exit Function
            End If
            Dim ent As AecDb.Entity = obj

            ' use late binding to see if the entity has a StyleId property
            obj = ent.GetType().InvokeMember("StyleId", System.Reflection.BindingFlags.GetProperty, Nothing, ent, Nothing)
            If Not TypeOf obj Is ObjectId Then
                GetPropertySetsFromStyle = setIdsex
                Exit Function
            End If

            Dim styleId As ObjectId = obj
            If styleId.IsNull Then
                GetPropertySetsFromStyle = setIdsex
                Exit Function
            End If

            obj = mytm.GetObject(styleId, OpenMode.ForRead, False, False)
            If Not TypeOf obj Is AecDb.DBObject Then
                GetPropertySetsFromStyle = setIdsex
                Exit Function
            End If
            Dim style As AecDb.DBObject = obj

            ' get the property sets from style
            setIdsex = PropertyDataServices.GetPropertySets(style)

        Catch

        Finally

        End Try

        GetPropertySetsFromStyle = setIdsex

    End Function

    Private Function HasPropertySet(ByRef mytm As DBTransactionManager, ByRef mytr As Transaction, ByVal id As ObjectId, ByVal psdname As String) As Boolean
        Dim hasset As Boolean = False
        Dim setIds As ObjectIdCollection = New ObjectIdCollection()

        Dim obj As Object = mytm.GetObject(id, OpenMode.ForRead, False, False)

        If Not TypeOf obj Is AecDb.Entity Then
            HasPropertySet = hasset
            Exit Function
        End If

        Dim ent As AecDb.Entity = obj

        ' get the property sets on the object
        setIds = PropertyDataServices.GetPropertySets(ent)
        For s = 0 To setIds.Count - 1
            Try
                Dim prset As AecPropDb.PropertySet = mytr.GetObject(setIds.Item(s), OpenMode.ForRead)
                If prset.PropertySetDefinitionName = psdname Then
                    hasset = True
                End If

            Catch e As Autodesk.AutoCAD.Runtime.Exception
                ' most likely eKeyNotfound
                Debug.WriteLine(e.Message)
            End Try
        Next
        If hasset = False Then
            setIds = GetPropertySetsFromStyle(mytm, mytr, id)
            For s = 0 To setIds.Count - 1
                Try
                    Dim prset As AecPropDb.PropertySet = mytr.GetObject(setIds.Item(s), OpenMode.ForRead)
                    If prset.PropertySetDefinitionName = psdname Then
                        hasset = True
                    End If
                Catch e As Autodesk.AutoCAD.Runtime.Exception
                    ' most likely eKeyNotfound
                    Debug.WriteLine(e.Message)
                End Try
            Next
        End If

        HasPropertySet = hasset

    End Function

    Shared Sub StartSelect(ByVal PSDName As String,
                                ByVal PropName As String,
                                ByVal txtSearch As String,
                                ByVal WholeWord As Boolean,
                                ByVal Mode As String) 'As SelectionSet

        Dim ed As Editor = Application.DocumentManager.MdiActiveDocument.Editor

        Dim db As Database = HostApplicationServices.WorkingDatabase
        Dim tm As AcadDb.TransactionManager = db.TransactionManager

        'Dim dbobj As AcadDb.DBObject
        Using lock As DocumentLock = ed.Document.LockDocument 'WICHTIG BEI FORMS/COMMANDBUTTONS!!!!
            Using trans As Transaction = tm.StartTransaction()
                Dim psrSelected As PromptSelectionResult
                psrSelected = ed.SelectImplied

                Dim FoundIdList = New ObjectIdCollection

                Dim PreSelected As SelectionSet = Nothing

                If psrSelected.Status = PromptStatus.OK Then
                    PreSelected = psrSelected.Value
                End If

                Dim bt As BlockTable = tm.GetObject(db.BlockTableId, OpenMode.ForRead, False)
                Dim btr As BlockTableRecord = tm.GetObject(bt(BlockTableRecord.ModelSpace), OpenMode.ForRead, False)

                For Each id As ObjectId In btr
                    Dim foundvalue = SelectByProperty.GetPropertyValue(tm, trans, id, PSDName, PropName)
                    'Debug.WriteLine(foundvalue)
                    If foundvalue <> "[NULL!]" Then
                        If WholeWord Then
                            If foundvalue.ToString Like txtSearch Then
                                FoundIdList.Add(id)
                            End If
                        Else
                            If foundvalue.ToString.Contains(txtSearch) Then
                                FoundIdList.Add(id)
                            End If
                        End If

                    End If
                Next

                ' Dim ssfound As SelectionSet = Autodesk.AutoCAD.EditorInput.SelectionSet.FromObjectIds(FoundIdList)

                Select Case Mode
                    Case "add"

                        If PreSelected IsNot Nothing Then

                            '' Step through the second selection set
                            For Each acObjId As ObjectId In PreSelected.GetObjectIds
                                '' Add each object id to the ObjectIdCollection
                                FoundIdList.Add(acObjId)

                            Next

                        End If

                        Dim ids As ObjectId() = New ObjectId(FoundIdList.Count - 1) {}

                        FoundIdList.CopyTo(ids, 0)

                        ed.SetImpliedSelection(ids)

                        ed.WriteMessage(vbCr & FoundIdList.Count & " item(s) added." & vbCrLf)

                    Case "remove"

                        Dim resultSS As SelectionSet

                        If PreSelected IsNot Nothing Then

                            Dim myPrevSelectionIds As New List(Of ObjectId)(PreSelected.GetObjectIds)

                            For Each id In FoundIdList

                                If myPrevSelectionIds.Contains(id) Then

                                    myPrevSelectionIds.Remove(id)

                                End If

                            Next

                            resultSS = Autodesk.AutoCAD.EditorInput.SelectionSet.FromObjectIds(myPrevSelectionIds.ToArray)

                            ed.SetImpliedSelection(resultSS)

                            ed.WriteMessage(vbCr & FoundIdList.Count & " item(s) removed." & vbCrLf)

                        Else
                            ed.WriteMessage(vbCr & "0 items removed." & vbCrLf)

                        End If

                    Case Else 'new
                        'clear
                        ed.SetImpliedSelection(New ObjectId() {})

                        Dim ids As ObjectId() = New ObjectId(FoundIdList.Count - 1) {}

                        FoundIdList.CopyTo(ids, 0)

                        ed.SetImpliedSelection(ids)

                        ed.WriteMessage(vbCr & FoundIdList.Count & " item(s) selected." & vbCrLf)

                End Select

                trans.Commit()

                Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView()

                Dim myDoc As Document = Application.DocumentManager.MdiActiveDocument

            End Using

        End Using

    End Sub

    Shared Sub Main()

        Dim theForm As New SelectByPropertyFrm

        Application.ShowModelessDialog(theForm)

    End Sub

End Class
