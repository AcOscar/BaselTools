Imports System.Windows.Forms

Public Class MatchFrm

    Shared BlockEvent As Boolean = False

    Private Sub TV_PSD_AfterCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles TV_PSD.AfterCheck
        Dim myNode As TreeNode = e.Node

        If Not BlockEvent Then

            BlockEvent = True

            For Each node As TreeNode In myNode.Nodes

                node.Checked = myNode.Checked

            Next

            BlockEvent = False

        End If

    End Sub

End Class