Imports ACA_Tools.AEC_Helper
Imports AcadDb = Autodesk.AutoCAD.DatabaseServices
Imports AecPropDb = Autodesk.Aec.PropertyData.DatabaseServices
Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput

Namespace ExcelTools

    Public Class ExcelReader

        ''' <summary>
        ''' datafilter and completionhelper
        ''' </summary>
        ''' <param name="modus">Datafilter or CompletionFilter </param>
        ''' <remarks></remarks>
        Shared Sub Start(ByVal modus As String)

            Dim myDB As AcadDb.Database

            Dim myDWG As Document

            Dim myTransMan As AcadDb.TransactionManager

            Dim myTrans As AcadDb.Transaction

            Dim myEd As Editor

            Dim myListValues As New ListValues

            myDWG = Application.DocumentManager.MdiActiveDocument

            myDB = myDWG.Database

            myEd = myDWG.Editor

            Dim _PER As PromptEntityResult

            Dim acSSPrompt As PromptSelectionResult

            acSSPrompt = myEd.SelectImplied()

            Dim acSSet As SelectionSet

            Dim myObjectId As AcadDb.ObjectId

            If acSSPrompt.Status = PromptStatus.OK Then

                acSSet = acSSPrompt.Value

                If acSSet.Count = 1 Then

                    myObjectId = acSSet.Item(0).ObjectId

                End If

            Else

                _PER = myEd.GetEntity(vbLf & "Choose an object:")

                If _PER.Status = PromptStatus.OK Then

                    myObjectId = _PER.ObjectId

                End If

            End If

            myTransMan = myDWG.TransactionManager

            myTrans = myTransMan.StartTransaction

            Try

                Dim myAcadEnt As AcadDb.Entity = myTrans.GetObject(myObjectId, AcadDb.OpenMode.ForRead, False, False)

                If Not myAcadEnt Is Nothing Then

                    'if its a MV-block we need the refernced objec maybe
                    Try

                        If TypeOf myAcadEnt Is Autodesk.Aec.DatabaseServices.MultiViewBlockReference Then
                            'If myAcadEnt.GetType = GetType(Autodesk.AEC.DatabaseServices.MultiViewBlockReference) Then

                            Dim myMVB As Autodesk.Aec.DatabaseServices.MultiViewBlockReference = myAcadEnt

                            Dim myAnchor As AcadDb.ObjectId = myMVB.AnchorId

                            Dim myAnchorTo As Autodesk.Aec.DatabaseServices.AnchorToReference

                            myAnchorTo = myTrans.GetObject(myAnchor, DatabaseServices.OpenMode.ForRead, False, False)

                            Dim myRefID As AcadDb.ObjectId = myAnchorTo.GetReferenceObjectAt(0, Autodesk.Aec.DatabaseServices.RelationType.DisplayReferenceOf)

                            myAcadEnt = myTrans.GetObject(myRefID, DatabaseServices.OpenMode.ForRead, False, False)

                        End If

                    Catch ex As Exception

                    End Try

                    'getting the existing values 
                    Dim PreSetVals As New ListValues

                    Try

                        Dim SetANewValue As Boolean = False

                        Dim setIds As AcadDb.ObjectIdCollection = AecPropDb.PropertyDataServices.GetPropertySets(myAcadEnt)

                        Dim psId As AcadDb.ObjectId

                        For Each psId In setIds

                            Dim pset As AecPropDb.PropertySet = myTransMan.GetObject(psId, AcadDb.OpenMode.ForRead, False, False)

                            Dim pid As Integer

                            Dim myPSD As AecPropDb.PropertySetDefinition

                            myPSD = myTransMan.GetObject(pset.PropertySetDefinition, AcadDb.OpenMode.ForRead, False, False)

                            For Each psdefs As AecPropDb.PropertyDefinition In myPSD.Definitions

                                pid = pset.PropertyNameToId(psdefs.Name)

                                Dim existing = pset.GetAt(pid)

                                If existing IsNot Nothing Then



                                    If Not existing.ToString = "" Then 'value.ToString Then

                                        Dim myListEntry As New ListValues.ListEntry

                                        myListEntry.Name = psdefs.Name

                                        myListEntry.Value = existing

                                        PreSetVals.List.Add(myListEntry)

                                    End If

                                End If

                            Next

                        Next

                    Catch

                    End Try

                    'the name for the propertyset named by the modus variable
                    'the property for the file name called "FileName" and the name for data range "DataName"
                    Dim propName As String = modus

                    Dim FileName As String = "FileName"

                    Dim DataName As String = "DataName"

                    'option:
                    'Only offer OK if all values are filled
                    Dim CheckValue As String = "False"

                    ' get the property sets
                    Dim myPPS As AecPropDb.PropertySet = Nothing

                    myPPS = GetPropertySetsFromStyle(myAcadEnt.Id, propName, True)

                    If myPPS Is Nothing Then

                        myEd.WriteMessage(vbCrLf + vbCrLf + "There is no Propertyset: " + propName)

                        Exit Try

                    End If

                    Dim ValueResult As System.Collections.ArrayList = Nothing

                    ValueResult = GetPropertyValue(myPPS, FileName)

                    If ValueResult.Count <> 1 Then

                        myEd.WriteMessage(vbCrLf + vbCrLf + "Error to get Property with Name: " + FileName)

                        Exit Try

                    End If

                    FileName = ValueResult(0).Value.ToString()

                    'including quotation mark so we can copy/pase the path directly from the clippbaord
                    'via the "copy path" option from the windows explorer context menu

                    If FileName.StartsWith("""") Then

                        FileName = FileName.Substring(1)

                    End If

                    If FileName.EndsWith("""") Then

                        FileName = FileName.Substring(0, FileName.Length - 1)

                    End If

                    ValueResult = GetPropertyValue(myPPS, DataName)

                    If ValueResult.Count <> 1 Then

                        myEd.WriteMessage(vbCrLf + vbCrLf + "Error to get Property with Name: " + DataName)

                        Exit Try

                    End If

                    DataName = ValueResult(0).Value.ToString()

                    'only Datafilter can have CheckValue
                    If propName = "Datafilter" Then

                        ValueResult = GetPropertyValue(myPPS, "CheckValue")

                        If ValueResult.Count = 1 Then

                            CheckValue = ValueResult(0).Value.ToString()

                        End If

                    End If

                    Select Case modus.ToLower
                        Case "datafilter"
                            myListValues = ExcelTools.DatafilterHelp.DataFilterHelp.Main(FileName, DataName, PreSetVals, CheckValue, "False")

                        Case "completionhelper"
                            myListValues = ExcelTools.DatafilterHelp.DataFilterHelp.Main(FileName, DataName, PreSetVals, CheckValue, "True")

                    End Select

                    PreSetVals = Nothing

                    For Each lentry As ListValues.ListEntry In myListValues.List

                        lentry.Name = lentry.Name.Trim

                        lentry.Name = lentry.Name.Replace(" ", "_")

                        SetValuesFromPropertySetByName(myAcadEnt, lentry.Name, lentry.Value)

                    Next

                End If

            Catch ex As Exception

            Finally

                myTrans.Commit()

                myTrans.Dispose()

                myTransMan.Dispose()

            End Try

        End Sub

        ''' <summary>
        ''' read all properties before excel will be opened
        ''' </summary>
        Shared Sub ExcelReader(ByVal quiet As Boolean)

            Dim myDB As AcadDb.Database

            Dim myDWG As Document

            Dim myTransMan As AcadDb.TransactionManager

            Dim myTrans As AcadDb.Transaction

            Dim myEd As Editor

            'the name of the excel file is in the PropertySet called "ExcelReader",
            'the filename is in the property "FileName" and for the data range in "DataName"
            Const PSD_Name As String = "ExcelReader"

            Const PropName_FileName As String = "FileName"

            Const PropName_DataName As String = "DataName"

            Const PropName_KeyName As String = "KeyName"

            Const PropName_Seperator As String = "Seperator"

            Dim FileName As String

            Dim DataName As String

            Dim Keyname As String = ""

            Dim Seperator As String = vbTab

            Dim PropIgnored As Integer = 0

            Dim PropSkipped As Integer = 0

            Dim PropReplaced As Integer = 0

            Dim PropNew As Integer = 0

            Dim KeyValue As String

            myDWG = Application.DocumentManager.MdiActiveDocument

            myDB = myDWG.Database

            myEd = myDWG.Editor

            myTransMan = myDWG.TransactionManager

            myTrans = myTransMan.StartTransaction

            Dim myXLColl As New List(Of DataRead)()

            Dim objcnt As Integer = 0

            Try

                Dim BlockTable As AcadDb.BlockTable = myTransMan.GetObject(myDB.BlockTableId, AcadDb.OpenMode.ForRead, False)

                Dim BlockTableRecord As AcadDb.BlockTableRecord

                Dim myObjWithExcelReaders As New List(Of ObjWithExcelReader)

                Dim myBTE As Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator

                myBTE = BlockTable.GetEnumerator

                While myBTE.MoveNext = True

                    BlockTableRecord = myBTE.Current.GetObject(Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead)

                    If BlockTableRecord.IsLayout Then

                        If Not quiet Then

                            Dim _Layout As AcadDb.Layout

                            _Layout = myTrans.GetObject(BlockTableRecord.LayoutId, DatabaseServices.OpenMode.ForRead)

                            myEd.WriteMessage(vbCrLf & "Entering: " & _Layout.LayoutName)

                        End If

                        For Each ObjectId As AcadDb.ObjectId In BlockTableRecord

                            Dim Entity As AcadDb.Entity = myTransMan.GetObject(ObjectId, AcadDb.OpenMode.ForRead, True)

                            Dim myPPS As AecPropDb.PropertySet = Nothing

                            Do
                                'only in the style
                                myPPS = Nothing

                                If Not TypeOf Entity Is Autodesk.Aec.DatabaseServices.Entity Then

                                    Exit Do

                                End If

                                myPPS = GetPropertySetsFromStyle(ObjectId, PSD_Name, False)

                                If myPPS Is Nothing Then

                                    Exit Do

                                End If

                                Dim ValueResult As System.Collections.ArrayList = Nothing

                                ValueResult = GetPropertyValue(myPPS, PropName_FileName)

                                If ValueResult.Count <> 1 Then

                                    If Not quiet Then

                                        myEd.WriteMessage(vbCrLf + vbCrLf + "Error to get Property: " + PropName_FileName)

                                    End If

                                    Exit Do

                                End If

                                FileName = ValueResult(0).Value.ToString()

                                ValueResult = GetPropertyValue(myPPS, PropName_DataName)

                                If ValueResult.Count <> 1 Then

                                    If Not quiet Then

                                        myEd.WriteMessage(vbCrLf + vbCrLf + "Error to get Property: " + PropName_DataName)

                                    End If

                                    Exit Do

                                End If

                                DataName = ValueResult(0).Value.ToString()

                                ValueResult = GetPropertyValue(myPPS, PropName_KeyName)

                                If ValueResult.Count <> 1 Then

                                    If Not quiet Then

                                        myEd.WriteMessage(vbCrLf + vbCrLf + "Error to get Property: " + PropName_KeyName)

                                    End If

                                    Exit Do

                                End If

                                Keyname = ValueResult(0).Value.ToString()

                                ValueResult = GetValuesFromPropertySetByName2(Keyname, ObjectId)

                                If ValueResult.Count <> 1 Then

                                    If Not quiet Then

                                        myEd.WriteMessage(vbCrLf + vbCrLf + "Error to get Property: " + Keyname)

                                    End If

                                    Exit Do

                                End If

                                KeyValue = ValueResult(0).Value.ToString()

                                ValueResult = GetPropertyValue(myPPS, PropName_Seperator)

                                If ValueResult.Count = 1 Then

                                    Seperator = ValueResult(0).Value.ToString()

                                End If

                                If FileName.StartsWith("""") Then

                                    FileName = FileName.Substring(1)

                                End If

                                If FileName.EndsWith("""") Then

                                    FileName = FileName.Substring(0, FileName.Length - 1)

                                End If

                                Dim aExcelRead As New DataRead(FileName, DataName, Seperator) ', KeyName)

                                'collect all ExcelReader and do not insert doubles
                                Dim newObj As New ObjWithExcelReader

                                newObj.AcadID = ObjectId

                                newObj.KeyName = Keyname

                                newObj.KeyValue = KeyValue

                                If Not myXLColl.Contains(aExcelRead) Then

                                    myXLColl.Add(aExcelRead)

                                    newObj.XLReader = aExcelRead

                                Else

                                    newObj.XLReader = myXLColl(myXLColl.IndexOf(aExcelRead))

                                End If

                                myObjWithExcelReaders.Add(newObj)

                                If Not quiet Then

                                    myEd.WriteMessage(vbCrLf & "Collecting: " & Entity.ToString)

                                End If

                            Loop Until True 'never this is just to escape
                        Next

                    End If

                End While

                'if we have no excelreader at this point, we are finished
                If myXLColl.Count = 0 Then

                    myEd.WriteMessage(vbLf & "Nothing to do.")

                    Exit Try

                End If

                'now read all files
                For Each Reader As DataRead In myXLColl

                    If Not quiet Then

                        myEd.WriteMessage(vbCrLf + "Reading """ & Reader.Shortname & """")

                    End If

                    Reader.readValues()

                    Dim FirstRow As String = ""

                    'if reader.datatable Nothing the file is missing
                    'if datatable.count = 0 there is no datarange
                    If Reader.DataTable Is Nothing Then

                        If Not quiet Then

                            myEd.WriteMessage(vbCrLf + "File not exists: " & Reader.Filename)

                        End If

                    Else

                        If Reader.DataTable.Count = 0 Then

                            If Not quiet Then

                                myEd.WriteMessage(vbCrLf + "No values in cell range name: " & Reader.DataName)

                            End If

                        Else

                            If Not quiet Then

                                For Each colum As xRColumn In Reader.DataTable

                                    FirstRow &= colum.Name & " "

                                Next

                                myEd.WriteMessage(vbCrLf + "First Row: " & FirstRow)

                            End If

                        End If

                    End If

                Next

                objcnt = 0

                For Each aObjWithExcelReader In myObjWithExcelReaders

                    Dim myEnt As AcadDb.Entity

                    Dim OId As AcadDb.ObjectId = aObjWithExcelReader.AcadID ' Prop.Key

                    Dim myReader As DataRead = aObjWithExcelReader.XLReader ' Prop.Value

                    myEnt = myTrans.GetObject(OId, DatabaseServices.OpenMode.ForRead)

                    If Not quiet Then

                        myEd.WriteMessage(vbCrLf + "Work with """ & myEnt.ToString.Substring(myEnt.ToString.LastIndexOf(".") + 1) & """")

                    End If

                    If myReader.DataTable IsNot Nothing AndAlso myReader.DataTable.Count > 0 Then '

                        objcnt += 1

                        Dim KeyCol As Integer

                        Dim ColumnCatch As Boolean = False

                        'looking for the column
                        For i = 0 To aObjWithExcelReader.XLReader.DataTable.Count - 1

                            If aObjWithExcelReader.XLReader.DataTable.Item(i).Name = aObjWithExcelReader.KeyName Then

                                KeyCol = i

                                ColumnCatch = True

                                Exit For

                            End If

                        Next

                        If Not ColumnCatch Then

                            If Not quiet Then

                                myEd.WriteMessage(vbLf & "Can't find Keyname: " & aObjWithExcelReader.KeyName & " on the " & myEnt.ToString.Substring(myEnt.ToString.LastIndexOf(".") + 1) & " object")
                            End If

                        Else
                            'looking for the row
                            Dim RowCatch As Boolean = False

                            Dim KeyRow As Integer

                            For j = 0 To aObjWithExcelReader.XLReader.DataTable.Item(KeyCol).xRRows.Count - 1

                                If aObjWithExcelReader.XLReader.DataTable.Item(KeyCol).xRRows(j) = aObjWithExcelReader.KeyValue Then

                                    KeyRow = j

                                    RowCatch = True

                                    Exit For

                                End If

                            Next

                            If Not RowCatch Then

                                If Not quiet Then

                                    myEd.WriteMessage(vbLf & "Can't find Keyvalue " & aObjWithExcelReader.KeyValue & " from " & myEnt.ToString.Substring(myEnt.ToString.LastIndexOf(".") + 1) & " object, in the excel file.")

                                End If

                            Else

                                'looking the value of the row
                                Dim jetzt As Boolean = False

                                Dim PropIgnoredCurr As Integer = 0

                                Dim PropReplacedCurr As Integer = 0

                                Dim PropSkippedCurr As Integer = 0

                                Dim PropNewCurr As Integer = 0

                                For Each Col In aObjWithExcelReader.XLReader.DataTable

                                    If Col.Name <> "" Then

                                        If Not quiet Then

                                            myEd.WriteMessage(vbCrLf + "Try to set Property """ + Col.Name & """ to:""" & Col.xRRows(KeyRow).ToString & """ - ")

                                        End If

                                        Dim WasChanged As String = SetValuesFromPropertySetByName(myEnt, Col.Name, Col.xRRows(KeyRow).ToString)

                                        If Not quiet Then

                                            myEd.WriteMessage(WasChanged)

                                        End If

                                        Select Case WasChanged

                                            Case "Failed"
                                                PropIgnoredCurr += 1

                                            Case "Replaced"
                                                PropReplacedCurr += 1

                                            Case "Skip"
                                                PropSkippedCurr += 1

                                            Case "New"
                                                PropNewCurr += 1

                                        End Select

                                    End If

                                Next

                                If Not quiet Then

                                    myEd.WriteMessage(vbLf & PropNewCurr & " created, " & PropReplacedCurr & " replaced, " & PropSkippedCurr & " skipped, " & PropIgnoredCurr & " ignored.")

                                End If

                                PropNew += PropNewCurr

                                PropReplaced += PropReplacedCurr

                                PropSkipped += PropSkippedCurr

                                PropIgnored += PropIgnoredCurr

                            End If 'RowCatch

                        End If 'ColumnCatch

                    Else 'myReader.DataTable IsNot Nothing AndAlso myReader.DataTable.Count > 0

                        If Not quiet Then

                            myEd.WriteMessage(vbLf + "Nothing changed, " & myReader.Shortname & " is empty!" & vbCrLf)

                        End If

                    End If

                Next ' aObjWithExcelReader In myObjWithExcelReaders

            Catch ex As Exception

                MsgBox("Error: " & vbCrLf & ex.Message.ToString)

            Finally

                myTrans.Commit()

                myTrans.Dispose()

                myTransMan.Dispose()

            End Try

            myEd.WriteMessage(vbLf & "ExcelReader - Object: " & objcnt & " handled.")

            If Not quiet Then

                myEd.WriteMessage(vbLf & "ExcelReader - Properties: " & PropNew & " created, " & PropReplaced & " replaced, " & PropSkipped & " skipped, " & PropIgnored & " ignored.")

            End If

        End Sub

        Class ObjWithExcelReader

            Public AcadID As AcadDb.ObjectId

            Friend XLReader As DataRead

            Public KeyValue As String

            Public KeyName As String

        End Class

        ''' <summary>
        ''' open the excelfiles
        ''' </summary>
        Shared Sub ExcelReaderOpen()

            Dim myDB As AcadDb.Database

            Dim myDWG As Document

            Dim myTransMan As AcadDb.TransactionManager

            Dim myTrans As AcadDb.Transaction

            Dim myEd As Editor

            'the name of the excel file is in the PropertySet called "ExcelReader",
            'the filename is in the property "FileName" and for the data range in "DataName"
            Const PSD_Name As String = "ExcelReader"

            Const PropName_FileName As String = "FileName"

            Const PropName_DataName As String = "DataName"

            Dim FileName As String

            Dim DataName As String

            myDWG = Application.DocumentManager.MdiActiveDocument

            myDB = myDWG.Database

            myEd = myDWG.Editor

            myTransMan = myDWG.TransactionManager

            myTrans = myTransMan.StartTransaction

            ' Dim myXLColl As New List(Of DataRead)()
            Dim myXLFiles As New List(Of String)

            Try
                Dim BlockTable As AcadDb.BlockTable = myTransMan.GetObject(myDB.BlockTableId, AcadDb.OpenMode.ForRead, False)

                Dim BlockTableRecord As AcadDb.BlockTableRecord

                Dim myBTE As Autodesk.AutoCAD.DatabaseServices.SymbolTableEnumerator

                myBTE = BlockTable.GetEnumerator

                While myBTE.MoveNext = True

                    BlockTableRecord = myBTE.Current.GetObject(Autodesk.AutoCAD.DatabaseServices.OpenMode.ForRead)

                    If BlockTableRecord.IsLayout Then

                        For Each ObjectId As AcadDb.ObjectId In BlockTableRecord

                            Dim Entity As AcadDb.Entity = myTransMan.GetObject(ObjectId, AcadDb.OpenMode.ForRead, True)

                            Dim myPPS As AecPropDb.PropertySet = Nothing

                            Do

                                'only in the style
                                myPPS = Nothing

                                If Not TypeOf Entity Is Autodesk.Aec.DatabaseServices.Entity Then

                                    Exit Do

                                End If

                                myPPS = GetPropertySetsFromStyle(ObjectId, PSD_Name, False)

                                If myPPS Is Nothing Then

                                    Exit Do

                                End If

                                Dim ValueResult As System.Collections.ArrayList = Nothing

                                ValueResult = GetPropertyValue(myPPS, PropName_FileName)

                                If ValueResult.Count <> 1 Then

                                    Exit Do

                                End If

                                FileName = ValueResult(0).Value.ToString()

                                ValueResult = GetPropertyValue(myPPS, PropName_DataName)

                                If ValueResult.Count <> 1 Then

                                    Exit Do

                                End If

                                DataName = ValueResult(0).Value.ToString()

                                If Not myXLFiles.Contains(FileName & "|" & DataName) Then

                                    myXLFiles.Add(FileName & "|" & DataName)

                                End If

                            Loop Until True 'never this is just to escape
                        Next

                    End If

                End While

                'if we have no excelreader at this point, we are finished
                If myXLFiles.Count = 0 Then

                    myEd.WriteMessage(vbLf & "no ExcelTools found.")

                    Exit Try

                End If

                'now read all files
                For Each file As String In myXLFiles

                    myEd.WriteMessage(vbLf & "ExcelReader - Properties: " & file)

                    DataRead.OpenXL(file.Split("|")(0), file.Split("|")(1))

                Next

            Catch ex As Exception

                MsgBox("Error: " & vbCrLf & ex.Message.ToString)

            Finally

                myTrans.Commit()

                myTrans.Dispose()

                myTransMan.Dispose()

            End Try

        End Sub

    End Class

End Namespace
