#Region "Namespaces"
Imports AcadDb = Autodesk.AutoCAD.DatabaseServices
Imports AecDb = Autodesk.Aec.DatabaseServices
Imports AecPropDb = Autodesk.Aec.PropertyData.DatabaseServices
Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.Runtime
#End Region

Public Class AEC_Helper
#Region "GetValuesFromPropertySetByName"

    ''' <summary>
    ''' Returns the values (PropertyValueUnitPair) of a property by name on a given object.
    ''' </summary>
    ''' <param name="pname">The property name to find on the object.</param>
    ''' <param name="dbobj">The object to find the property on. </param>
    ''' <returns> An array of the values </returns>
    Shared Function GetValuesFromPropertySetByName(ByVal pname As String, ByVal dbobj As AcadDb.DBObject) As System.Collections.ArrayList
        Dim setIds As AcadDb.ObjectIdCollection = AecPropDb.PropertyDataServices.GetPropertySets(dbobj)
        Dim values As System.Collections.ArrayList = New System.Collections.ArrayList()
        If setIds.Count = 0 Then
            Return values ' just return emtpy collection...
        End If
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As DatabaseServices.TransactionManager = db.TransactionManager
        Dim psId As AcadDb.ObjectId
        For Each psId In setIds
            Dim pset As AecPropDb.PropertySet = tm.GetObject(psId, AcadDb.OpenMode.ForRead, False, False) 'As AecPropDb.PropertySet
            Dim pid As Integer
            Try
                pid = pset.PropertyNameToId(pname)
                values.Add(pset.GetValueAndUnitAt(pid))
            Catch e As Exception
                ' most likely eKeyNotfound.
            End Try
        Next
        tm.Dispose()
        db.Dispose()

        Return (values)
    End Function


    Shared Function GetValuesFromPropertySetByName2(ByVal pname As String, ByVal dbObjectId As DatabaseServices.ObjectId) As System.Collections.ArrayList
        Dim pObject As AcadDb.DBObject

        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As DatabaseServices.TransactionManager = db.TransactionManager

        pObject = tm.GetObject(dbObjectId, DatabaseServices.OpenMode.ForRead, False, False)


        Dim setIds As AcadDb.ObjectIdCollection = AecPropDb.PropertyDataServices.GetPropertySets(pObject)
        ' Dim setIds As AcadDb.ObjectIdCollection = AecPropDb.PropertyDataServices.GetPropertySets(dbObjectId

        Dim values As System.Collections.ArrayList = New System.Collections.ArrayList()
        If setIds.Count = 0 Then
            Return values ' just return emtpy collection...
        End If



        Dim psId As AcadDb.ObjectId
        For Each psId In setIds
            Dim pset As AecPropDb.PropertySet = tm.GetObject(psId, AcadDb.OpenMode.ForRead, False, False) 'As AecPropDb.PropertySet
            Dim pid As Integer
            Try
                pid = pset.PropertyNameToId(pname)
                values.Add(pset.GetValueAndUnitAt(pid))
            Catch e As Exception
                ' most likely eKeyNotfound.
            End Try
        Next
        tm.Dispose()
        db.Dispose()

        Return values
    End Function



    ''' <summary>
    ''' Returns the values (PropertyValueUnitPair) of a property by name on a given object.
    ''' </summary>
    ''' <param name="pname">The property name to find on the object.</param>
    ''' <param name="dbobjid">The object to find the property on. </param>
    ''' <returns> An array of the values </returns>
    Shared Function GetValuesFromPropertySetByName(ByVal pname As String, ByVal dbObjId As AcadDb.ObjectId) As List(Of System.Collections.ArrayList)
        Dim pObject As AcadDb.DBObject
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As DatabaseServices.TransactionManager = db.TransactionManager
        tm.StartTransaction()

        pObject = tm.GetObject(dbObjId, DatabaseServices.OpenMode.ForRead, False, False)

        Dim PSDID As AcadDb.ObjectId = GetPropertySetDefinitionIdByName(pname)
        Dim myps As Autodesk.Aec.PropertyData.DatabaseServices.PropertySetDefinition = tm.GetObject(PSDID, DatabaseServices.OpenMode.ForRead, False, False)

        Dim propdefs As AecPropDb.PropertyDefinitionCollection = myps.Definitions
        Dim propdef As AecPropDb.PropertyDefinition
        Dim myVals As New List(Of System.Collections.ArrayList)
        For Each propdef In propdefs
            Dim myVal As System.Collections.ArrayList
            myVal = GetValuesFromPropertySetByName(propdef.Name, pObject)
            myVals.Add(myVal)
        Next


        tm.Dispose()
        db.Dispose()
        Return myVals
    End Function


#End Region

#Region "SetValuesFromPropertySetByName"

    ''' <summary>
    ''' Sets the values (PropertyValueUnitPair) of a property by name on a given object.
    ''' </summary>
    ''' <param name="pname">The property name to find on the object.</param>
    ''' <param name="dbobj">The object to set the property on. </param>
    ''' <param name="value">The value to set. </param>
    ''' <returns> "Failed" if the PropertySet exists, "Skip" if the value already set, "New" if the property was empty, "Replaced" if the value was changed</returns>
    Shared Function SetValuesFromPropertySetByName(ByVal dbobj As AcadDb.DBObject, ByVal pname As String, ByVal value As Object) As String

        Dim SetANewValue As String = "Failed"
        Dim PropSetIds As AcadDb.ObjectIdCollection = AecPropDb.PropertyDataServices.GetPropertySets(dbobj)
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As DatabaseServices.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()
        Dim PropSetId As AcadDb.ObjectId
        For Each PropSetId In PropSetIds
            Dim PropSet As AecPropDb.PropertySet = tm.GetObject(PropSetId, AcadDb.OpenMode.ForWrite, False, False) ' As AecPropDb.PropertySet
            Dim PropId As Integer
            Try
                PropId = PropSet.PropertyNameToId(pname)
                If PropSet.IsWriteEnabled Then
                    Dim existing = PropSet.GetAt(PropId)
                    If existing.ToString = value.ToString Then
                        SetANewValue = "Skip"
                    Else

                        'this is graphic property definition
                        If TypeOf (existing) Is Object() Then

                            If existing.length = 6 Then
                                '1 - is the name
                                existing(1) = value.ToString

                                PropSet.SetAt(PropId, existing)
                            End If

                        Else
                            'standard manual property
                            PropSet.SetAt(PropId, value.ToString)

                            If existing.ToString = "" Then
                                SetANewValue = "New"
                            Else
                                SetANewValue = "Replaced"
                            End If

                        End If

                    End If

                    PropSet.UpdateReferencingAttributes()
                End If
            Catch ex As System.Exception
            End Try
        Next
        trans.Commit()
        trans.Dispose()
        Return SetANewValue
    End Function

    Shared Function SetValuesFromPropertySetByName2(ByVal pname As String, ByVal PropSet As AecPropDb.PropertySet, ByVal value As Object) As Boolean
        Dim SetANewValue As Boolean = False
        'Dim setIds As AcadDb.ObjectIdCollection = AecPropDb.PropertyDataServices.GetPropertySets(dbobj)
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As DatabaseServices.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()
        ' Dim psId As AcadDb.ObjectId
        Dim pid As Integer
        Dim myValPair As AecPropDb.PropertyValueUnitPair = Nothing
        Try
            'If TypeOf value(0) Is AecPropDb.PropertyValueUnitPair Then
            If value(0).GetType = GetType(AecPropDb.PropertyValueUnitPair) Then
                myValPair = value(0)
            End If
            ' Dim pset As AecPropDb.PropertySet = dbobj
            pid = PropSet.PropertyNameToId(pname)
            If (PropSet.IsWriteEnabled) Then
                Dim existing = PropSet.GetAt(pid)
                If existing.ToString = myValPair.Value.ToString Then
                    SetANewValue = False
                Else
                    SetANewValue = True
                    PropSet.SetAt(pid, myValPair.Value.ToString)
                    PropSet.UpdateReferencingAttributes()
                End If
            End If
        Catch e As Exception
            'Stop
        End Try

        trans.Commit()
        trans.Dispose()
        Return SetANewValue

    End Function

#End Region

#Region "GetPropertySetDefinitionIdByName"
    ' <summary>
    ' Finds the property set definition objectId with the given name.
    ' </summary>
    ' <param name="psdName">The property set definition name.</param>
    ' <returns> true, if a property with the specified value is found.</returns>
    Shared Function GetPropertySetDefinitionIdByName(ByVal psdName As String) As AcadDb.ObjectId
        Dim psdId As New AcadDb.ObjectId
        psdId = AcadDb.ObjectId.Null
        Dim db As AcadDb.Database = Application.DocumentManager.MdiActiveDocument.Database
        Dim tm As AcadDb.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()

        Dim psdDict As AecPropDb.DictionaryPropertySetDefinitions = New AecPropDb.DictionaryPropertySetDefinitions(db)
        If psdDict.Has(psdName, trans) Then
            psdId = psdDict.GetAt(psdName)
        End If

        trans.Commit()
        trans.Dispose()
        tm.Dispose()
        db.Dispose()

        db = Nothing
        tm = Nothing
        trans = Nothing
        psdDict = Nothing

        Return psdId
    End Function
#End Region

#Region "CreatePropSetOnDBObject"
    ''' <summary>
    ''' Creates a new property set on the given DBObject.
    ''' </summary>
    ''' <param name="dbobjId"></param>
    ''' <param name="propsetdefId"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function CreatePropSetOnDBObject(ByVal dbobjId As AcadDb.ObjectId, ByVal propsetdefId As AcadDb.ObjectId) As Boolean
        Dim _return As Boolean = False
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As DatabaseServices.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()

        Try
            Dim dbobj As AcadDb.DBObject = tm.GetObject(dbobjId, AcadDb.OpenMode.ForWrite, False, False)
            AecPropDb.PropertyDataServices.AddPropertySet(dbobj, propsetdefId)
            _return = True
        Catch
            _return = False ' failure
        End Try

        trans.Commit()
        trans.Dispose()
        tm.Dispose()
        db = Nothing
        trans = Nothing
        tm = Nothing

        Return _return
    End Function
#End Region
    Shared Function SetPropertyValue(ByVal PropSet As AecPropDb.PropertySet, ByVal propName As String, ByVal Value As String) As Boolean
        Dim values As System.Collections.ArrayList = New System.Collections.ArrayList()
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As AcadDb.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()
        Try
            'Dim pid As Integer
            Try

                Dim myPropSet As AecPropDb.PropertySet = tm.GetObject(PropSet.Id, AcadDb.OpenMode.ForWrite, False, False) ' As AecPropDb.PropertySet
                Dim PropId As Integer

                PropId = myPropSet.PropertyNameToId(propName)

                If myPropSet.IsWriteEnabled Then
                    Dim existing = myPropSet.GetAt(PropId)
                    If existing.ToString = Value.ToString Then
                        Return False
                    Else
                        myPropSet.SetAt(PropId, Value.ToString)
                    End If
                    myPropSet.UpdateReferencingAttributes()
                End If

            Catch e As Autodesk.AutoCAD.Runtime.Exception
                ' most likely eKeyNotfound.
            End Try
        Catch
        Finally
            If tm.NumberOfActiveTransactions > 0 Then
                trans.Commit()
            End If
            trans.Dispose()
        End Try
        Return True

    End Function


    Shared Function GetPropertyValue(ByVal PropSet As AecPropDb.PropertySet, ByVal propName As String) As System.Collections.ArrayList
        Dim values As System.Collections.ArrayList = New System.Collections.ArrayList()
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As AcadDb.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()
        Try
            Dim pid As Integer
            Try
                pid = PropSet.PropertyNameToId(propName.ToLower)
                values.Add(PropSet.GetValueAndUnitAt(pid))
            Catch e As Autodesk.AutoCAD.Runtime.Exception
                ' most likely eKeyNotfound.
            End Try
        Catch
        Finally
            trans.Dispose()
        End Try
        Return values
    End Function

    Shared Function GetPropertySet(ByVal id As AcadDb.ObjectId, ByRef PropertySetDefinitionName As String) As AecPropDb.PropertySet

        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As AcadDb.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()

        Try
            Dim obj As Object = tm.GetObject(id, AcadDb.OpenMode.ForRead, False, False)
            If Not TypeOf obj Is AecDb.Entity Then
                Return Nothing
            End If
            Dim ent As AecDb.Entity = obj

            For Each ss In AecPropDb.PropertyDataServices.GetPropertySets(ent)
                Dim myPPS As AecPropDb.PropertySet
                myPPS = tm.GetObject(ss, AcadDb.OpenMode.ForRead, False, False)
                If myPPS.PropertySetDefinitionName = PropertySetDefinitionName Then
                    Return myPPS
                End If
            Next
        Catch
        Finally
            trans.Dispose()
        End Try
        Return Nothing

    End Function

    Shared Function GetPropertySets(ByVal id As AcadDb.ObjectId) As List(Of String)
        Dim myPSDs As New List(Of String)
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As AcadDb.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()
        Try
            Dim obj As Object = tm.GetObject(id, AcadDb.OpenMode.ForRead, False, False)
            If Not TypeOf obj Is AecDb.Entity Then
                Return Nothing
            End If
            Dim ent As AecDb.Entity = obj

            For Each ss In AecPropDb.PropertyDataServices.GetPropertySets(ent)
                Dim myPPS As AecPropDb.PropertySet
                myPPS = tm.GetObject(ss, AcadDb.OpenMode.ForRead, False, False)
                myPSDs.Add(myPPS.PropertySetDefinitionName)
            Next
        Catch
        Finally
            trans.Dispose()
        End Try
        Return myPSDs
    End Function

    Shared Function GetPropertySetsFromStyle(ByVal id As AcadDb.ObjectId, ByVal PropertySetDefinitionName As String, ByVal Nested As Boolean) As AecPropDb.PropertySet
        Dim db As AcadDb.Database = AcadDb.HostApplicationServices.WorkingDatabase
        Dim tm As AcadDb.TransactionManager = db.TransactionManager
        Dim trans As AcadDb.Transaction = tm.StartTransaction()
        Try
            Dim obj As Object = tm.GetObject(id, AcadDb.OpenMode.ForRead, False, False)
            ' If obj.GetType.Name.ToString = "WallStyle" Then
            If obj.GetType = GetType(Autodesk.Aec.Arch.DatabaseServices.WallStyle) Then
                'Stop
                Dim ent1 As Autodesk.Aec.Arch.DatabaseServices.WallStyle = obj
                'obj = ent1.GetType().InvokeMember("StyleId", System.Reflection.BindingFlags.GetProperty, Nothing, ent1, Nothing)
                Dim res As AcadDb.ResultBuffer = ent1.GetXDataForApplication("AEC_PROPERTY_SETS")
            End If
            If Not TypeOf obj Is AecDb.Entity Then
                'If Not obj.GetType = GetType(AecDb.Entity) Then
                Return Nothing
            End If
            Dim myMVRef As AecDb.MultiViewBlockReference = Nothing
            Dim ent As AecDb.Entity = obj

            Select Case ent.DisplayName
                Case "Multi-View Block Reference"
                    myMVRef = obj
                    obj = myMVRef.StyleId

                Case Else
                    ' use late binding to see if the entity has a StyleId property
                    obj = ent.GetType().InvokeMember("StyleId", System.Reflection.BindingFlags.GetProperty, Nothing, ent, Nothing)
            End Select

            If Not TypeOf obj Is AcadDb.ObjectId Then
                'If Not obj.GetType = GetType(AcadDb.ObjectId) Then
                Return Nothing
            End If

            Dim styleId As AcadDb.ObjectId = obj
            If styleId.IsNull Then
                Return Nothing
            End If

            obj = tm.GetObject(styleId, AcadDb.OpenMode.ForRead, False, False)
            If Not TypeOf obj Is AecDb.DBObject Then
                'If Not obj.GetType = GetType(AecDb.DBObject) Then

                Return Nothing
            End If
            Dim style As AecDb.DBObject = obj
            Dim PropSetsIdCol As AcadDb.ObjectIdCollection = AecPropDb.PropertyDataServices.GetPropertySets(style)

            If PropSetsIdCol.Count = 0 Then
                If Not myMVRef Is Nothing Then
                    If Nested Then
                        Dim myAnchor As AecDb.AnchorToReference = tm.GetObject(myMVRef.AnchorId, AcadDb.OpenMode.ForRead, False, False)
                        ' Dim myRefrencedObj As Object = tm.GetObject(myAnchor.GetReferenceObjectAt(0, AecDb.RelationType.LocationReferenceFor), OpenMode.ForRead, False, False)
                        Dim myPPS As AecPropDb.PropertySet
                        myPPS = GetPropertySetsFromStyle(myAnchor.GetReferenceObjectAt(0, AecDb.RelationType.LocationReferenceFor), PropertySetDefinitionName, False)
                        Return myPPS
                    End If
                End If
            Else
                For Each PPS In PropSetsIdCol
                    Dim myObj As Object 'AecPropDb.PropertySet

                    myObj = tm.GetObject(PPS, AcadDb.OpenMode.ForRead, False, False)

                    'If TypeOf myObj Is AecPropDb.PropertySet Then
                    If myObj.GetType = GetType(AecPropDb.PropertySet) Then

                        Dim myPPS As AecPropDb.PropertySet = myObj
                        'no case sensitivity, because a psd can't deside case senstivity
                        If myPPS.PropertySetDefinitionName.ToLower = PropertySetDefinitionName.ToLower Then
                            Return myPPS
                        End If
                    End If
                Next

            End If
        Catch ex As System.Exception
            ' Debug.Print(ex.Message.ToString)
        Finally
            trans.Dispose()
        End Try
        Return Nothing
    End Function

    <Autodesk.AutoCAD.Runtime.LispFunction("IsPSDFromStyle")> _
    Public Function PropertySetFromStyle(ByVal rbArgs As AcadDb.ResultBuffer)
        If rbArgs = Nothing Then Return Nothing

        Dim PropertySetDefinitionName As String = ""
        Dim id As AcadDb.ObjectId

        For Each rb As AcadDb.TypedValue In rbArgs
            Select Case rb.TypeCode
                Case Autodesk.AutoCAD.Runtime.LispDataType.Text
                    PropertySetDefinitionName = rb.Value.ToString
                Case Autodesk.AutoCAD.Runtime.LispDataType.ObjectId
                    id = rb.Value
                Case Else
                    Return Nothing
            End Select
        Next

        If id = Nothing Then Return Nothing
        If PropertySetDefinitionName = "" Then Return Nothing

        If GetPropertySetsFromStyle(id, PropertySetDefinitionName, False) Is Nothing Then
            Return False
        Else
            Return True
        End If
        Return Nothing
    End Function
#Region "isPsdOnObject"

    ''' <summary>
    ''' check if a PSD exist on a object
    ''' </summary>
    ''' <param name="dbobj">a dbObject</param>
    ''' <param name="PsdName">The property set definition name.</param>
    ''' <returns>true if the property set with the given name was found, or false otherwise.</returns>
    Shared Function isPsdOnObject(ByVal dbobj As AcadDb.DBObject, ByVal PsdName As String) As Boolean
        Dim definitionId As AcadDb.ObjectId = GetPropertySetDefinitionIdByName(PsdName)
        Dim propSetId As AcadDb.ObjectId = AcadDb.ObjectId.Null
        If Not definitionId.IsNull Then
            Try
                propSetId = AecPropDb.PropertyDataServices.GetPropertySet(dbobj, definitionId)
            Catch
                ' More than likely eKeyNotFound, so this is a more specific
                ' place to handle a "failed to find" condition.
            End Try
        End If

        If Not propSetId.IsNull Then
            Return True ' got an ID so we found it
        End If

        Return False ' didn't find it by name on this object.
    End Function
#End Region

End Class

''' <summary>
''' Eine Liste von Werten für ein Property Set
''' </summary>
''' <remarks></remarks>
Public Class ListValues
    Public List As New List(Of ListEntry)
    ''' <summary>
    ''' prüft ob ein Property in der List enthalten ist
    ''' </summary>
    ''' <param name="Name">Der Name des Property</param>
    ''' <returns>True wenn das Property gefunden wurde</returns>
    ''' <remarks></remarks>
    Public Function MissingThis(ByVal Name As String) As Boolean
        MissingThis = True
        For Each entry As ListEntry In List
            If entry.Value = Name Then
                Return False
            End If
        Next
    End Function

    ''' <summary>
    ''' liefert den Wert eins Eintrages 
    ''' </summary>
    ''' <param name="Name">Der Name des Property</param>
    ''' <returns>der Wert des gefundenen Propertys, ansonsten ""</returns>
    ''' <remarks></remarks>
    Public Function GetValue(ByVal Name As String) As String
        GetValue = ""
        For Each entry As ListEntry In List
            If entry.Name = Name Then
                Return entry.Value
            End If
        Next
    End Function
    ''' <summary>
    ''' Name und Wert für ein Property
    ''' </summary>
    ''' <remarks></remarks>
    Class ListEntry
        Public Name As String
        Public Value As String
    End Class

End Class

