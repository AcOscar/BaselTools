Imports Autodesk.AutoCAD.Runtime

Public Class Commands
    Implements IExtensionApplication

    Public Sub Initialize() Implements IExtensionApplication.Initialize
        Dim myEd As Autodesk.AutoCAD.EditorInput.Editor = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor

        myEd.WriteMessage(vbCr + "Loading Basel Tools - AutoCAD Tools V" & My.Application.Info.Version.ToString & " ... " &
                          IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly.Location).ToString("d.MM.yy HH:mm:ss") & vbCrLf)

    End Sub

    Public Sub Terminate() Implements Autodesk.AutoCAD.Runtime.IExtensionApplication.Terminate

    End Sub

    <CommandMethod("BaselTools", "RealXrefInsert", CommandFlags.Modal Or CommandFlags.DocExclusiveLock)>
    Sub Command_RealXrefInsert()

        XrefTools.processXrefs(vbLf & "Select xrefs to insert all entities: ", AddressOf XrefTools.BindInsertExplodeXrefs)

    End Sub

    <CommandMethod("BaselTools", "Binsert", CommandFlags.Modal)>
    Sub Command_bindXref()

        XrefTools.processXrefs(vbLf & "Select xrefs to bind-insert: ", AddressOf XrefTools.bindInsertXrefs)

    End Sub

    <CommandMethod("BaselTools", "DetachXref", CommandFlags.Modal)>
    Sub Command_DetachXref()

        XrefTools.processXrefs(vbLf & "Select xrefs to detach: ", AddressOf XrefTools.detachXref)

    End Sub

    <CommandMethod("BaselTools", "BlockShaker", CommandFlags.Modal)>
    Sub Command_BlockShaker()

        Dim myBlockShkr As New BlockShaker

        myBlockShkr.Shaker()

    End Sub

    <CommandMethod("BaselTools", "LayerList", CommandFlags.Modal)>
    Sub Command_LayerList()

        Dim myLyrWrk As New LayerList

        myLyrWrk.Start()

    End Sub

End Class
