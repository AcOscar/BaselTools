Imports Autodesk.AEC.Arch.DatabaseServices
Imports Autodesk.Aec.PropertyData.DatabaseServices
Imports Autodesk.AutoCAD.DatabaseServices

Public Module AEC_Extension

    <System.Runtime.CompilerServices.Extension()>
    Function PSDS(ByVal myEntity As Autodesk.Aec.DatabaseServices.Entity) As List(Of PSD)
        'renamed from PSDsFromObject

        Dim myPSDs As New List(Of PSD)

        Dim db As Database = HostApplicationServices.WorkingDatabase

        Dim tr As Transaction = db.TransactionManager.StartTransaction

        Dim idCol As Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection = PropertyDataServices.GetPropertySets(myEntity)

        For Each propId As Autodesk.AutoCAD.DatabaseServices.ObjectId In idCol

            Dim propSet As PropertySet = propId.GetObject(OpenMode.ForRead)

            Dim myPSD As New PSD

            Dim propSetDef As PropertySetDefinition = tr.GetObject(propSet.PropertySetDefinition, OpenMode.ForRead)

            myPSD.Name = propSetDef.Name

            Dim propDefs As PropertyDefinitionCollection = propSetDef.Definitions

            ' Loop all properties in the propSetDef 
            For Each propDef As PropertyDefinition In propDefs

                Dim myProp As New PSD.Prop

                Dim myValue As Object = Nothing

                If propDef.Automatic Then 'AUTOMATIC
                    Try

                        Dim myBlockrefpath As New ObjectIdCollection

                        myValue = propSetDef.GetValue(propDef.Id, myEntity.ObjectId, myBlockrefpath)

                    Catch

                    Finally

                        If myValue Is Nothing Then
                            myValue = New String("Error getting value.")
                        End If

                    End Try

                    myProp.Automatic = True

                Else ' STATIC
                    myValue = propSet.GetAt(propDef.Id)

                    myProp.Automatic = False

                End If

                myProp.Name = propDef.Name

                myProp.Description = propDef.Description

                myProp.ValueString = myValue.ToString

                myProp.Value = myValue

                myPSD.Properties.Add(myProp)

            Next

            myPSDs.Add(myPSD)

        Next

        tr.Commit()

        Return myPSDs

    End Function

    <System.Runtime.CompilerServices.Extension()>
    Function Stylename(ByVal myspace As Space) As String

        Dim myStylname As String = String.Empty

        Dim db As Database = HostApplicationServices.WorkingDatabase

        Using trx As Transaction = db.TransactionManager.StartTransaction

            Dim mySpaceStyle As SpaceStyle = trx.GetObject(myspace.StyleId, OpenMode.ForRead)

            myStylname = mySpaceStyle.Name

        End Using

        Return myStylname

    End Function

    <System.Runtime.CompilerServices.Extension()>
    Function SetProperty(ByVal myEntity As Autodesk.AEC.DatabaseServices.Entity,
                         ByVal PSDName As String,
                         ByVal PropName As String,
                         ByVal Value As Object) As Boolean

        Dim db As Database = HostApplicationServices.WorkingDatabase

        Dim tr As Transaction = db.TransactionManager.StartTransaction

        Dim PropSetidCol As Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection = PropertyDataServices.GetPropertySets(myEntity)

        Dim propSet As PropertySet

        For Each propId As Autodesk.AutoCAD.DatabaseServices.ObjectId In PropSetidCol

            propSet = propId.GetObject(OpenMode.ForWrite)

            Dim propSetDef As PropertySetDefinition = tr.GetObject(propSet.PropertySetDefinition, OpenMode.ForRead)

            If propSetDef.Name <> PSDName Then

                Continue For

            End If

            Try

                Dim PropIdInt As Integer

                PropIdInt = propSet.PropertyNameToId(PropName)

                If propSet.IsWriteEnabled Then

                    Dim existing = propSet.GetAt(PropIdInt)

                    propSet.SetAt(PropIdInt, Value)

                End If

            Catch e As Autodesk.AutoCAD.Runtime.Exception
                ' most likely eKeyNotfound.
            End Try

        Next

        If propSet IsNot Nothing Then

            propSet.UpdateReferencingAttributes()

        End If

        tr.Commit()

        Return True

    End Function

    'todo: this is untested!!!
    <System.Runtime.CompilerServices.Extension()>
    Function GetProperty(ByVal myEntity As Autodesk.AEC.DatabaseServices.Entity, ByVal PSDName As String, ByVal PropName As String) As Object

        Dim myReturn As Object

        Dim db As Database = HostApplicationServices.WorkingDatabase

        Dim tr As Transaction = db.TransactionManager.StartTransaction

        Dim PropSetidCol As Autodesk.AutoCAD.DatabaseServices.ObjectIdCollection = PropertyDataServices.GetPropertySets(myEntity)

        Dim propSet As PropertySet

        For Each propId As Autodesk.AutoCAD.DatabaseServices.ObjectId In PropSetidCol

            propSet = propId.GetObject(OpenMode.ForWrite)

            Dim propSetDef As PropertySetDefinition = tr.GetObject(propSet.PropertySetDefinition, OpenMode.ForRead)

            If propSetDef.Name <> PSDName Then

                Continue For

            End If

            Try
                ' Loop all properties in the propSetDef 
                For Each propDef As PropertyDefinition In propSetDef.Definitions

                    If propDef.Name <> PropName Then

                        Continue For

                    End If

                    If propDef.Automatic Then 'AUTOMATIC

                        Try

                            Dim myBlockrefpath As New ObjectIdCollection

                            myReturn = propSetDef.GetValue(propDef.Id, myEntity.ObjectId, myBlockrefpath)

                        Catch

                        Finally

                            If myReturn Is Nothing Then

                                myReturn = New String("Error getting value.")

                            End If

                        End Try

                    Else ' STATIC

                        myReturn = propSet.GetAt(propDef.Id)

                    End If

                Next

            Catch e As Autodesk.AutoCAD.Runtime.Exception
                ' most likely eKeyNotfound.
            End Try

        Next

           tr.Commit()

        Return myReturn

    End Function

    Class PSD

        Public Name As String

        Public Properties As List(Of Prop)

        Class Prop

            Public Name As String

            Public Description As String

            Public ValueString As String

            Public Automatic As Boolean

            Public Value As Object

        End Class

        Public Sub New()

            Properties = New List(Of Prop)

        End Sub

    End Class

End Module
