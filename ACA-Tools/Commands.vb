Imports Autodesk.AutoCAD.Runtime

Public Class Commands
    Implements IExtensionApplication

    Public Sub Initialize() Implements IExtensionApplication.Initialize
        Dim myEd As Autodesk.AutoCAD.EditorInput.Editor = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor

        myEd.WriteMessage(vbCr + "Loading Basel Tools - AutoCAD Architecture Tools V" & My.Application.Info.Version.ToString & " ... " &
                  IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly.Location).ToString("d.MM.yy HH:mm:ss") & vbCrLf)

    End Sub

    Public Sub Terminate() Implements IExtensionApplication.Terminate

    End Sub


    <CommandMethod("BaselTools", "CompletionHelper", CommandFlags.UsePickSet)>
    Sub Command_CompletionHelper()

        ExcelTools.ExcelReader.Start("CompletionHelper")

    End Sub

    <CommandMethod("BaselTools", "DataFilter", CommandFlags.UsePickSet)>
    Sub Command_DataFilter()

        ExcelTools.ExcelReader.Start("DataFilter")

    End Sub

    <CommandMethod("BaselTools", "ExcelReader", CommandFlags.Modal)>
    Sub Command_ExcelReader()

        ExcelTools.ExcelReader.ExcelReader(quiet:=True)

    End Sub

    <CommandMethod("BaselTools", "ExcelReaderLog", CommandFlags.Modal)>
    Sub Command_ExcelReader_Log()

        ExcelTools.ExcelReader.ExcelReader(quiet:=False)

    End Sub

    <CommandMethod("BaselTools", "ExcelReaderOpen", CommandFlags.Modal)>
    Sub Command_ExcelReaderOpen()

        ExcelTools.ExcelReader.ExcelReaderOpen()

    End Sub

    <CommandMethod("BaselTools", "-BSTMultiSpaceAdd", CommandFlags.DocExclusiveLock)>
    Sub Command_CmdBSTMultiSpaceAdd()

        Dim mySpaces As New AddSpaces

        mySpaces.CL_MultiSpaceAdd()

    End Sub

    <CommandMethod("BaselTools", "BSTSpaceAdd", CommandFlags.DocExclusiveLock)>
    Sub Command_BSTSpaceAdd()

        Dim mySpaces As New AddSpaces

        mySpaces.SpaceAdd()

    End Sub

    <CommandMethod("BaselTools", "-SpaceAdd", CommandFlags.DocExclusiveLock)>
    Sub Command_CmdBSTSpaceAdd()

        Dim mySpaces As New AddSpaces

        mySpaces.CL_SpaceAdd()

    End Sub

    <CommandMethod("BaselTools", "-BSTSpaceAdd2", CommandFlags.DocExclusiveLock)>
    Sub Command_CmdBSTSpaceAdd2()

        Dim mySpaces As New AddSpaces


        mySpaces.CL_SpaceAdd2()

    End Sub

    <CommandMethod("BaselTools", "MatchPSDProperty", CommandFlags.Modal)>
    Sub Command_MatchPSDProperty()

        MatchPropertiesPSD.Match()

    End Sub

    <CommandMethod("BaselTools", "PropertyRenumber", CommandFlags.Modal)>
    Sub Command_PropertyRenumber()

        AECRenumber.Main()

    End Sub

    <CommandMethod("BaselTools", "SelectByProperty", CommandFlags.Modal + CommandFlags.UsePickSet)>
    Sub Command_SelectByProperty()

        SelectByProperty.Main()

    End Sub

    <CommandMethod("BaselTools", "XMLREPAIR", CommandFlags.Modal)>
    Sub Command_XMLREPAIR()

        xmlRepair.Main()

    End Sub

End Class
