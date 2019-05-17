<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MatchFrm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TV_PSD = New System.Windows.Forms.TreeView()
        Me.SuspendLayout()
        '
        'TV_PSD
        '
        Me.TV_PSD.CheckBoxes = True
        Me.TV_PSD.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TV_PSD.Location = New System.Drawing.Point(0, 0)
        Me.TV_PSD.Name = "TV_PSD"
        Me.TV_PSD.ShowNodeToolTips = True
        Me.TV_PSD.ShowRootLines = False
        Me.TV_PSD.Size = New System.Drawing.Size(164, 226)
        Me.TV_PSD.TabIndex = 0
        '
        'MatchFrm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(164, 226)
        Me.ControlBox = False
        Me.Controls.Add(Me.TV_PSD)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow
        Me.MinimumSize = New System.Drawing.Size(180, 100)
        Me.Name = "MatchFrm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.Text = "Match Properties"
        Me.TopMost = True
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents TV_PSD As System.Windows.Forms.TreeView
End Class
