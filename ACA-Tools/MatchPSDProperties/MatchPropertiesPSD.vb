Imports System.Windows.Forms
Imports Autodesk.AutoCAD
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices

Public Class MatchPropertiesPSD
    Shared myDWG As ApplicationServices.Document
    Shared myDB As DatabaseServices.Database
    Shared myEd As EditorInput.Editor
    Shared myMatchfrm As MatchFrm

    Shared Sub Match()

        myDWG = ApplicationServices.Application.DocumentManager.MdiActiveDocument

        myDB = myDWG.Database

        myEd = myDWG.Editor

        Dim tvs As DatabaseServices.TypedValue() = {New DatabaseServices.TypedValue(0, "AEC*")}

        Dim sf As New EditorInput.SelectionFilter(tvs)

        Dim pso As New EditorInput.PromptSelectionOptions()

        pso.MessageForAdding = vbCr & "Select source object:"

        pso.AllowDuplicates = False

        pso.SingleOnly = True

        Dim psrPos As EditorInput.PromptSelectionResult = myEd.GetSelection(pso, sf)

        If psrPos.Status <> EditorInput.PromptStatus.OK Then

            Exit Sub

        End If

        Dim myAEC As Autodesk.Aec.DatabaseServices.Entity

        myMatchfrm = New MatchFrm

        myMatchfrm.TV_PSD.Nodes.Clear()

        Using TransRead = myDWG.TransactionManager.StartTransaction

            Dim SpaceId As ObjectId = psrPos.Value.GetObjectIds(0)

            myAEC = TransRead.GetObject(SpaceId, OpenMode.ForRead, False)

            For Each psd As PSD In myAEC.PSDS

                Dim myPSDNode As TreeNode = myMatchfrm.TV_PSD.Nodes.Add(psd.Name)

                For Each prop In psd.Properties

                    If Not prop.Automatic Then

                        Dim myPSDSubNode As TreeNode = myPSDNode.Nodes.Add(prop.Name)

                        myPSDSubNode.ToolTipText = prop.ValueString.ToString

                        myPSDSubNode.Tag = prop.Value

                    End If

                Next

                myPSDNode.Checked = True

            Next

            If myMatchfrm.TV_PSD.Nodes.Count = 0 Then

                myEd.WriteMessage(vbCr & "No PropertySetDefinitions to match on this object.")

                Exit Sub

            Else

                myMatchfrm.TV_PSD.ExpandAll()

                myMatchfrm.Show()

            End If

            TransRead.Commit()

        End Using

        Dim stay As Boolean = True

        pso.MessageForAdding = vbLf & "Select destination object(s)"

        pso.SinglePickInSpace = True

        Dim TransWrite As Transaction

        Do
            'we have constantly start a transcation to commit every change
            TransWrite = myDWG.TransactionManager.StartTransaction

            psrPos = myEd.GetSelection(pso, sf)

            If psrPos.Status <> EditorInput.PromptStatus.OK Then

                stay = False

                Continue Do

            End If

            Dim myTargetIds() As ObjectId = psrPos.Value.GetObjectIds

            For Each ObjectId In myTargetIds

                myAEC = TransWrite.GetObject(ObjectId, OpenMode.ForWrite, False)

                For Each node As TreeNode In myMatchfrm.TV_PSD.Nodes

                    For Each subnode As TreeNode In node.Nodes

                        If subnode.Checked Then

                            Dim PSDName As String = subnode.Parent.Text

                            Dim PropName As String = subnode.Text

                            Dim PropValue As Object = subnode.Tag

                            myAEC.SetProperty(PSDName, PropName, PropValue)

                        End If

                    Next

                Next

            Next

            'only after a commit an active display-theme will be aplied to the aec object
            TransWrite.Commit()

        Loop While stay

        TransWrite.Dispose()

        myMatchfrm.Hide()

    End Sub

End Class
