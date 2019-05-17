<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class SelectByPropertyFrm
    Inherits System.Windows.Forms.Form

    'Das Formular überschreibt den Löschvorgang, um die Komponentenliste zu bereinigen.
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

    'Wird vom Windows Form-Designer benötigt.
    Private components As System.ComponentModel.IContainer

    'Hinweis: Die folgende Prozedur ist für den Windows Form-Designer erforderlich.
    'Das Bearbeiten ist mit dem Windows Form-Designer möglich.  
    'Das Bearbeiten mit dem Code-Editor ist nicht möglich.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.Bt_Select = New System.Windows.Forms.Button()
        Me.rbcontains = New System.Windows.Forms.RadioButton()
        Me.GroupBox3 = New System.Windows.Forms.GroupBox()
        Me.rb1zu1 = New System.Windows.Forms.RadioButton()
        Me.TB_Value = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.CB_PSDNames = New System.Windows.Forms.ComboBox()
        Me.CB_PropName = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.BT_Refresh = New System.Windows.Forms.Button()
        Me.GroupBox1 = New System.Windows.Forms.GroupBox()
        Me.RB_Add2CurSS = New System.Windows.Forms.RadioButton()
        Me.RB_RemoveFromSS = New System.Windows.Forms.RadioButton()
        Me.RB_Add2NewSS = New System.Windows.Forms.RadioButton()
        Me.GroupBox3.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'ToolTip1
        '
        Me.ToolTip1.ShowAlways = True
        '
        'Bt_Select
        '
        Me.Bt_Select.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Bt_Select.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Bt_Select.Location = New System.Drawing.Point(212, 188)
        Me.Bt_Select.Name = "Bt_Select"
        Me.Bt_Select.Size = New System.Drawing.Size(71, 23)
        Me.Bt_Select.TabIndex = 13
        Me.Bt_Select.Text = "Select"
        Me.Bt_Select.UseVisualStyleBackColor = True
        '
        'rbcontains
        '
        Me.rbcontains.AutoSize = True
        Me.rbcontains.Checked = True
        Me.rbcontains.Location = New System.Drawing.Point(6, 42)
        Me.rbcontains.Name = "rbcontains"
        Me.rbcontains.Size = New System.Drawing.Size(159, 17)
        Me.rbcontains.TabIndex = 4
        Me.rbcontains.TabStop = True
        Me.rbcontains.Text = "Substring (without wildcards)"
        Me.rbcontains.UseVisualStyleBackColor = True
        '
        'GroupBox3
        '
        Me.GroupBox3.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.GroupBox3.Controls.Add(Me.rb1zu1)
        Me.GroupBox3.Controls.Add(Me.rbcontains)
        Me.GroupBox3.Location = New System.Drawing.Point(8, 101)
        Me.GroupBox3.Name = "GroupBox3"
        Me.GroupBox3.Size = New System.Drawing.Size(275, 67)
        Me.GroupBox3.TabIndex = 11
        Me.GroupBox3.TabStop = False
        Me.GroupBox3.Text = "Search options"
        '
        'rb1zu1
        '
        Me.rb1zu1.AutoSize = True
        Me.rb1zu1.Location = New System.Drawing.Point(6, 19)
        Me.rb1zu1.Name = "rb1zu1"
        Me.rb1zu1.Size = New System.Drawing.Size(179, 17)
        Me.rb1zu1.TabIndex = 3
        Me.rb1zu1.Text = "Whole value (wildcards possible)"
        Me.rb1zu1.UseVisualStyleBackColor = True
        '
        'TB_Value
        '
        Me.TB_Value.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.TB_Value.Location = New System.Drawing.Point(8, 190)
        Me.TB_Value.Name = "TB_Value"
        Me.TB_Value.Size = New System.Drawing.Size(198, 20)
        Me.TB_Value.TabIndex = 12
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(5, 9)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(112, 13)
        Me.Label5.TabIndex = 18
        Me.Label5.Text = "Property Set Definition"
        '
        'CB_PSDNames
        '
        Me.CB_PSDNames.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CB_PSDNames.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CB_PSDNames.FormattingEnabled = True
        Me.CB_PSDNames.Location = New System.Drawing.Point(8, 27)
        Me.CB_PSDNames.Name = "CB_PSDNames"
        Me.CB_PSDNames.Size = New System.Drawing.Size(217, 21)
        Me.CB_PSDNames.Sorted = True
        Me.CB_PSDNames.TabIndex = 9
        '
        'CB_PropName
        '
        Me.CB_PropName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.CB_PropName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.CB_PropName.FormattingEnabled = True
        Me.CB_PropName.Location = New System.Drawing.Point(7, 72)
        Me.CB_PropName.Name = "CB_PropName"
        Me.CB_PropName.Size = New System.Drawing.Size(276, 21)
        Me.CB_PropName.Sorted = True
        Me.CB_PropName.TabIndex = 10
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(5, 171)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(69, 13)
        Me.Label7.TabIndex = 15
        Me.Label7.Text = "Value to find:"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(4, 54)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(46, 13)
        Me.Label6.TabIndex = 17
        Me.Label6.Text = "Property"
        '
        'BT_Refresh
        '
        Me.BT_Refresh.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.BT_Refresh.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.BT_Refresh.Location = New System.Drawing.Point(231, 25)
        Me.BT_Refresh.Name = "BT_Refresh"
        Me.BT_Refresh.Size = New System.Drawing.Size(52, 23)
        Me.BT_Refresh.TabIndex = 13
        Me.BT_Refresh.Text = "Refresh"
        Me.BT_Refresh.UseVisualStyleBackColor = True
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.RB_Add2CurSS)
        Me.GroupBox1.Controls.Add(Me.RB_RemoveFromSS)
        Me.GroupBox1.Controls.Add(Me.RB_Add2NewSS)
        Me.GroupBox1.Location = New System.Drawing.Point(8, 217)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(282, 92)
        Me.GroupBox1.TabIndex = 19
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Selection"
        '
        'RB_Add2CurSS
        '
        Me.RB_Add2CurSS.AutoSize = True
        Me.RB_Add2CurSS.Checked = True
        Me.RB_Add2CurSS.Location = New System.Drawing.Point(8, 42)
        Me.RB_Add2CurSS.Name = "RB_Add2CurSS"
        Me.RB_Add2CurSS.Size = New System.Drawing.Size(136, 17)
        Me.RB_Add2CurSS.TabIndex = 2
        Me.RB_Add2CurSS.TabStop = True
        Me.RB_Add2CurSS.Text = "add to current selection"
        Me.RB_Add2CurSS.UseVisualStyleBackColor = True
        '
        'RB_RemoveFromSS
        '
        Me.RB_RemoveFromSS.AutoSize = True
        Me.RB_RemoveFromSS.Location = New System.Drawing.Point(8, 65)
        Me.RB_RemoveFromSS.Name = "RB_RemoveFromSS"
        Me.RB_RemoveFromSS.Size = New System.Drawing.Size(164, 17)
        Me.RB_RemoveFromSS.TabIndex = 1
        Me.RB_RemoveFromSS.Text = "remove from current selection"
        Me.RB_RemoveFromSS.UseVisualStyleBackColor = True
        '
        'RB_Add2NewSS
        '
        Me.RB_Add2NewSS.AutoSize = True
        Me.RB_Add2NewSS.Location = New System.Drawing.Point(8, 19)
        Me.RB_Add2NewSS.Name = "RB_Add2NewSS"
        Me.RB_Add2NewSS.Size = New System.Drawing.Size(132, 17)
        Me.RB_Add2NewSS.TabIndex = 0
        Me.RB_Add2NewSS.Text = "add to a new selection"
        Me.RB_Add2NewSS.UseVisualStyleBackColor = True
        '
        'SelectByPropertyFrm
        '
        Me.AcceptButton = Me.Bt_Select
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(299, 314)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.BT_Refresh)
        Me.Controls.Add(Me.Bt_Select)
        Me.Controls.Add(Me.GroupBox3)
        Me.Controls.Add(Me.TB_Value)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.CB_PSDNames)
        Me.Controls.Add(Me.CB_PropName)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "SelectByPropertyFrm"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.Text = "Select by Property"
        Me.GroupBox3.ResumeLayout(False)
        Me.GroupBox3.PerformLayout()
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
    Friend WithEvents Bt_Select As System.Windows.Forms.Button
    Friend WithEvents rbcontains As System.Windows.Forms.RadioButton
    Friend WithEvents GroupBox3 As System.Windows.Forms.GroupBox
    Friend WithEvents rb1zu1 As System.Windows.Forms.RadioButton
    Friend WithEvents TB_Value As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents CB_PSDNames As System.Windows.Forms.ComboBox
    Friend WithEvents CB_PropName As System.Windows.Forms.ComboBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents BT_Refresh As System.Windows.Forms.Button
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents RB_Add2CurSS As System.Windows.Forms.RadioButton
    Friend WithEvents RB_RemoveFromSS As System.Windows.Forms.RadioButton
    Friend WithEvents RB_Add2NewSS As System.Windows.Forms.RadioButton
End Class
