Imports System.Collections.ObjectModel
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.Runtime

Public Class OSMIN
    Implements IExtensionApplication

    Public Sub Initialize() Implements IExtensionApplication.Initialize

        Dim ed As Editor = Application.DocumentManager.MdiActiveDocument.Editor

        ed.WriteMessage(vbCr + "Loading Basel Tools - OSMIN V" & My.Application.Info.Version.ToString & " ... " &
                  IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly.Location).ToString("d.MM.yy HH:mm:ss") & vbCrLf)

    End Sub

    Public Sub Terminate() Implements Autodesk.AutoCAD.Runtime.IExtensionApplication.Terminate

    End Sub

    <CommandMethod("OSMIN")>
    Sub Command_OSMIN()

        If Not Initialize_OSM() Then

            Exit Sub

        End If

        Dim myImportOptionsDialog As New ImportOSMFormSelectImportStyle

        myImportOptionsDialog.Styles = myOSM.Settings.Styles

        myImportOptionsDialog.FileFilter = myOSM.Settings.FileFilter

        myImportOptionsDialog.CurrentStyleName = OSM_Painter.CurrentStyleName

        Dim myDialogResult = myImportOptionsDialog.ShowDialog()

        If myDialogResult <> System.Windows.Forms.DialogResult.OK Then

            Exit Sub

        End If

        Dim ed As Editor = Core.Application.DocumentManager.MdiActiveDocument.Editor

        ed.WriteMessage(vbLf & "Reading file: " & myImportOptionsDialog.OSM_FileName)

        BuildOSM(myImportOptionsDialog.OSM_FileName, DeleteAfterRead:=False, StylName:=myImportOptionsDialog.CurrentStyleName)

    End Sub

#If withPallette Then
    <CommandMethod("OSMPALLETTE")>
    Public Sub Command_OSMPALLETTE()

        If Not Initialize_OSM() Then

            Exit Sub

        End If

        Dim ps As New Autodesk.AutoCAD.Windows.PaletteSet("OSM Import")

        Dim myPalette As OSMIN_Control = New OSMIN_Control

        'myPalette.UrlBase = myOSM.Settings.downloadurlbase

        ps.Add("OSM Import", myPalette)

        ps.Visible = True

        Dim myuri As New Uri(myOSM.Settings.exporturl, UriKind.Absolute)

        myPalette.CurrentStyleName = OSM_Painter.CurrentStyleName

        myPalette.Styles = myOSM.Settings.Styles

        myPalette.WebBrowser.Navigate(myuri)

        AddHandler myPalette.OSMCatched, Sub(filename, stylename) BuildOSM(filename, DeleteAfterRead:=True, StylName:=stylename)

    End Sub
#End If

    Private Function Initialize_OSM() As Boolean

        myOSM = New OSM

        OSM_Painter.LoadLayerList()

        If Not ReadConfig() Then

            Return False

        End If

        Return True

    End Function

    Private Function ReadConfig() As Boolean

        Try

            Dim directory As String = IO.Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly.Location)

            Dim files As ReadOnlyCollection(Of String)

            files = My.Computer.FileSystem.GetFiles(directory,
                                                    Microsoft.VisualBasic.FileIO.SearchOption.SearchTopLevelOnly,
                                                    "OSMIN.config")

            Select Case files.Count

                Case Is > 1
                    Messenger.Desaster("More than one config file.")

                    Return False

                Case Is = 0
                    Messenger.Desaster("There is no config file.")

                    Return False

            End Select

            For Each ConfigFile In files

                If Not myOSM.ReadConfigXML(ConfigFile) Then

                    Messenger.Desaster("error reading config file: " & files(0))

                    Return False

                End If

            Next

        Catch ex As Exception

            Return False

        End Try

        Return True

    End Function

    Private Sub BuildOSM(ByVal OSM_Filename As String, ByVal DeleteAfterRead As Boolean, ByVal StylName As String)

        If Not Initialize_OSM() Then

            Exit Sub

        End If

        OSM_Painter.lom = New LongOperationManager("OSM Import")

        'go out if is there no document
        If Core.Application.DocumentManager.MdiActiveDocument Is Nothing Then

            Exit Sub

        End If

        'set the current importstyle
        For Each st In myOSM.Settings.Styles

            If st.Name = StylName Then

                OSM_Painter.CurrentStyle = st

                Exit For

            End If

        Next

        Dim ed As Editor = Core.Application.DocumentManager.MdiActiveDocument.Editor

        Using OSM_Painter.lom

            If myOSM.ReadFile(OSM_Filename) Then

                ed.WriteMessage(vbLf)

                OSM_Painter.ZommToBound()

                ed.UpdateScreen()

                OSM_Painter.lom.TotalOperations = myOSM.minimumTicks

                myOSM.buildRelations()

                OSM_Painter.buildNodes2Insert()

                OSM_Painter.DrawWays()

                OSM_Painter.DrawRelations()

                OSM_Painter.DrawNodes()

                OSM_Painter.DrawImportedBoundAndCredit()

                ed.WriteMessage(vbLf & "Done")

            End If

        End Using

        myOSM = Nothing

        If DeleteAfterRead Then

            My.Computer.FileSystem.DeleteFile(OSM_Filename)

        End If

    End Sub

End Class
