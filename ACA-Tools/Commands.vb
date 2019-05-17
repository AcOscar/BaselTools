Imports Autodesk.AutoCAD.Runtime

Public Class Commands
    Implements IExtensionApplication

    Public Sub Initialize() Implements IExtensionApplication.Initialize
        Dim myEd As Autodesk.AutoCAD.EditorInput.Editor = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor

        myEd.WriteMessage(vbCr + "Herzog & de Meuron - AutoCAD Architecture Tools loaded: " &
                          My.Application.Info.Version.ToString & " - " &
                          IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly.Location).ToString("d.MM.yy HH:mm:ss") & vbCrLf)

    End Sub

    Public Sub Terminate() Implements IExtensionApplication.Terminate

    End Sub

    'off ACA2019 HOR leaving
    '<CommandMethod("HdM-NET-Tools", "AddFromTemplate", CommandFlags.Modal)>
    'Sub Command_AddFromTemplate()

    '    FromACATemplate.Main.Add("AFT")

    'End Sub

    'off ACA2019 HOR leaving
    '<CommandMethod("HdM-NET-Tools", "CompletionHelper", CommandFlags.UsePickSet)>
    'Sub Command_CompletionHelper()

    '    ExcelTools.ExcelReader.Start("CompletionHelper")

    'End Sub

    'off ACA2019 HOR leaving
    '<CommandMethod("HdM-NET-Tools", "DataFilter", CommandFlags.UsePickSet)>
    'Sub Command_DataFilter()

    '    ExcelTools.ExcelReader.Start("DataFilter")

    'End Sub

    <CommandMethod("HdM-NET-Tools", "ExcelReader", CommandFlags.Modal)>
    Sub Command_ExcelReader()

        ExcelTools.ExcelReader.ExcelReader(quiet:=True)

    End Sub

    <CommandMethod("HdM-NET-Tools", "ExcelReaderLog", CommandFlags.Modal)>
    Sub Command_ExcelReader_Log()

        ExcelTools.ExcelReader.ExcelReader(quiet:=False)

    End Sub

    <CommandMethod("HdM-NET-Tools", "ExcelReaderOpen", CommandFlags.Modal)>
    Sub Command_ExcelReaderOpen()

        ExcelTools.ExcelReader.ExcelReaderOpen()

    End Sub

    <CommandMethod("HdM-NET-Tools", "-HdMMultiSpaceAdd", CommandFlags.DocExclusiveLock)>
    Sub Command_CmdHdMMultiSpaceAdd()

        Dim mySpaces As New AddSpaces

        mySpaces.CL_MultiSpaceAdd()

    End Sub

    <CommandMethod("HdM-NET-Tools", "HdMSpaceAdd", CommandFlags.DocExclusiveLock)>
    Sub Command_HdMSpaceAdd()

        Dim mySpaces As New AddSpaces

        mySpaces.SpaceAdd()

    End Sub

    <CommandMethod("HdM-NET-Tools", "-HdMSpaceAdd", CommandFlags.DocExclusiveLock)>
    Sub Command_CmdHdMSpaceAdd()

        Dim mySpaces As New AddSpaces

        mySpaces.CL_SpaceAdd()

    End Sub

    <CommandMethod("HdM-NET-Tools", "-HdMSpaceAdd2", CommandFlags.DocExclusiveLock)>
    Sub Command_CmdHdMSpaceAdd2()

        Dim mySpaces As New AddSpaces


        mySpaces.CL_SpaceAdd2()

    End Sub

    <CommandMethod("HdM-NET-Tools", "MatchPSDProperty", CommandFlags.Modal)> _
    Sub Command_MatchPSDProperty()

        MatchPropertiesPSD.Match()

    End Sub

    <CommandMethod("HdM-NET-Tools", "PropertyRenumber", CommandFlags.Modal)> _
    Sub Command_PropertyRenumber()

        AECRenumber.Main()

    End Sub

    <CommandMethod("HdM-NET-Tools", "SelectByProperty", CommandFlags.Modal + CommandFlags.UsePickSet)> _
    Sub Command_SelectByProperty()

        SelectByProperty.Main()

    End Sub

    <CommandMethod("HdM-NET-Tools", "XMLREPAIR", CommandFlags.Modal)>
    Sub Command_XMLREPAIR()

        xmlRepair.Main()

    End Sub

End Class
