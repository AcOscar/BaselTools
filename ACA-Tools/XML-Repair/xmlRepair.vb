Imports AcadDb = Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.Aec.Project
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput

Public Class xmlRepair

    Shared Sub Main()

        Dim myPrjConfig As ProjectConfiguration = Nothing

        Dim currDoc As Document = Application.DocumentManager.MdiActiveDocument

        Dim ed As Editor = currDoc.Editor

        Try

            Dim currDb As AcadDb.Database = currDoc.Database

            myPrjConfig = ProjectQueryUtilities.GetProjectConfiguration(currDb)

            Dim myRepairForm As New XMLREPAIRFORM

            myRepairForm.RootPathConstruct = myPrjConfig.ConstructLocation

            myRepairForm.RootPathElements = myPrjConfig.ElementLocation

            myRepairForm.RootPathSheets = myPrjConfig.SheetLocation

            myRepairForm.ShowDialog()

        Catch ex As Exception

            If myPrjConfig Is Nothing Then

                ed.WriteMessage("This is not a project file." & vbCrLf)

            End If

        End Try

    End Sub

End Class

Class XMLcomplement

    Public XmlFileName As String

    Private NameString As String

    Private Description As String

    Private VersionString As String = "75"

    Private Function WriteXML() As Boolean

        Dim myXmlSettings As New Xml.XmlWriterSettings

        myXmlSettings.CheckCharacters = True

        myXmlSettings.ConformanceLevel = Xml.ConformanceLevel.Document

        myXmlSettings.Indent = True

        myXmlSettings.NewLineOnAttributes = False

        Try
            Using XMLWriter As Xml.XmlWriter = Xml.XmlWriter.Create(XmlFileName, myXmlSettings)

                XMLWriter.WriteStartDocument()

                XMLWriter.WriteStartElement("File")

                XMLWriter.WriteAttributeString("Name", NameString)

                XMLWriter.WriteStartElement("Version")

                XMLWriter.WriteString(VersionString)

                XMLWriter.WriteEndElement() 'Version

                XMLWriter.WriteStartElement("Description")

                XMLWriter.WriteString(Description)

                XMLWriter.WriteEndElement() 'Description

                XMLWriter.WriteEndElement() 'File

                XMLWriter.WriteEndDocument()

            End Using

            Return True

        Catch

        End Try

        Return False

    End Function

    Private Sub fromXML()

        'liest die XML ein 
        Dim XMLDoc As New Xml.XmlDocument

        Dim XMLNode As Xml.XmlNode

        If IO.File.Exists(XmlFileName) Then

            Try

                XMLDoc.Load(XmlFileName)

                If XMLDoc.DocumentElement.HasAttribute("Name") Then

                    NameString = XMLDoc.DocumentElement.GetAttribute("Name")

                End If

                For Each XMLNode In XMLDoc.DocumentElement.ChildNodes

                    Select Case XMLNode.Name.ToLower

                        Case "version"

                            VersionString = XMLNode.InnerXml

                        Case "description"

                            Description = XMLNode.InnerXml

                    End Select

                Next

            Catch

                Stop

            End Try

        End If

    End Sub

    Private Function CheckName() As Boolean

        Dim myNameString As String

        myNameString = XmlFileName.Substring(XmlFileName.LastIndexOf("\") + 1)

        myNameString = myNameString.Remove(myNameString.Length - 4)

        If myNameString <> NameString Then

            NameString = myNameString

            CheckName = True

        Else

            Return False

        End If

    End Function

    Function Exists() As Boolean

        Exists = False

        If IO.File.Exists(XmlFileName) Then

            Exists = True

        End If

    End Function

    Function Create() As Boolean

        If XmlFileName <> "" Then

            Try

                NameString = XmlFileName.Substring(XmlFileName.LastIndexOf("\") + 1)

                NameString = NameString.Remove(NameString.Length - 4)

                Description = NameString

                If WriteXML() Then

                    Create = True

                End If

            Catch ex As Exception

            End Try

        End If

        Return True

    End Function

    Function CheckRepairXML() As Boolean

        fromXML()

        If CheckName() Then

            If WriteXML() Then

                Return True

            Else

                Return False

            End If

        Else

            Return False

        End If

    End Function

End Class
