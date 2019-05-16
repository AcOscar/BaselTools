Imports Autodesk.AutoCAD.EditorInput
Imports Autodesk.AutoCAD.ApplicationServices

Class ImportOSMFormSelectImportStyle

    Property Styles As List(Of ImportStyle)

    Property CurrentStyleName As String

    Property OSM_FileName As String

    Property FileFilter As String

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click

        If System.IO.File.Exists(TextBox1.Text) Then

            Me.DialogResult = System.Windows.Forms.DialogResult.OK

        ElseIf Label1.Text.StartsWith("http") Then

            Me.DialogResult = System.Windows.Forms.DialogResult.OK

        Else

            Me.DialogResult = System.Windows.Forms.DialogResult.Cancel

        End If

        Me.Close()

    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click

        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel

        Me.Close()

    End Sub

    Private Sub ImportOSMFormSelectImportStyle_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        For Each style In Styles

            ComboBox1.Items.Add(style.Name)

        Next

        ComboBox1.SelectedText = CurrentStyleName

    End Sub

    Private Sub bt_browse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles bt_browse.Click

        Dim oldName As String = TextBox1.Text

        Dim ed As Editor = Core.Application.DocumentManager.MdiActiveDocument.Editor

        Dim opts As New PromptOpenFileOptions("Select a file to import")

        opts.Filter = FileFilter

        Dim pr As PromptFileNameResult = ed.GetFileNameForOpen(opts)

        If pr.Status = PromptStatus.OK Then

            OSM_FileName = pr.StringResult

            TextBox1.Text = OSM_FileName

        Else

            TextBox1.Text = oldName

        End If

    End Sub

    Private Sub ComboBox1_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ComboBox1.TextChanged

        For Each s In Styles

            If s.Name = ComboBox1.Text Then

                CurrentStyleName = ComboBox1.Text

            End If

        Next

    End Sub

End Class
