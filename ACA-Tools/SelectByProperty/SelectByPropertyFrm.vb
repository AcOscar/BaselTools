Public Class SelectByPropertyFrm

    Private Sub Form_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        BT_Refresh_Click(Me, e)

    End Sub

    Private Sub cmbSearchSet_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CB_PSDNames.SelectedIndexChanged

        Dim strlist = New List(Of String)

        CB_PropName.Items.Clear()

        strlist = SelectByProperty.getallproperties(CB_PSDNames.Items(CB_PSDNames.SelectedIndex))

        CB_PropName.Items.AddRange(strlist.ToArray)

        If strlist.Count > 0 Then CB_PropName.Text = CB_PropName.Items(0)

    End Sub

    Private Sub Bt_Select_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Bt_Select.Click

        Dim SelectionMode As String

        If RB_RemoveFromSS.Checked Then
            SelectionMode = "remove"

        ElseIf RB_Add2CurSS.Checked Then
            SelectionMode = "add"

        Else
            SelectionMode = "new"

        End If

        SelectByProperty.StartSelect(PSDName:=CB_PSDNames.Text,
                                     PropName:=CB_PropName.Text,
                                     txtSearch:=TB_Value.Text,
                                     WholeWord:=rb1zu1.Checked,
                                     Mode:=SelectionMode)

    End Sub

    Private Sub BT_Refresh_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BT_Refresh.Click

        Dim old As String = CB_PSDNames.Text

        CB_PSDNames.Items.Clear()

        CB_PSDNames.Items.AddRange(SelectByProperty.getallpropertysets().ToArray)

        If CB_PSDNames.Items.Contains(old) Then

            CB_PSDNames.Text = old

        ElseIf CB_PSDNames.Items.Count > 0 Then

            CB_PSDNames.Text = CB_PSDNames.Items(0)

        End If

    End Sub

End Class