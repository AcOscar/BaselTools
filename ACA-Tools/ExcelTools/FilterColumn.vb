Imports System.Drawing
Imports System.Windows.Forms

Namespace ExcelTools

    Namespace DatafilterHelp

        ''' <summary>
        ''' a filtered column
        ''' </summary>
        Class FilterColumn
            Inherits xRColumn

            Private myLabel As New Label

            Public WithEvents myTextB As New TextBox

            Public WithEvents myThawBt As New Button

            Private WithEvents myCont As New ContextMenuStrip

            Public FilterString As String

            Public Pos As Integer

            Public Frozen As Boolean

            Private isCompletition As Boolean

            Private myLoc As Point

            Private _left As Integer

            Private _top As Integer

            Private _width As Integer

            Private _height As Integer

            Sub New(ByVal completion As Boolean, ByVal withCheck As Boolean)

                If completion Then

                    isCompletition = True

                End If

                myLabel.AutoSize = True

                myLabel.Height = 14

                myThawBt.Text = "Thaw"

                If withCheck Then

                    myThawBt.Enabled = False

                End If

                myThawBt.TabStop = False

                If isCompletition Then

                    myThawBt.Visible = False

                Else

                    myThawBt.Visible = True

                End If

                myTextB.TabStop = False

                myCont.ShowImageMargin = False

                myCont.ShowCheckMargin = False

                myCont.UseWaitCursor = True

                myThawBt.Width = 0

                myTextB.Width = 0

                myLabel.Width = 0

            End Sub

            Property Height()

                Get
                    _height = myLabel.Height

                    _height += myTextB.Height

                    _height += myThawBt.Height

                    Return _height

                End Get

                Set(ByVal value)

                    _height = value

                End Set

            End Property

            Property Width()

                Get
                    'we extract the column width help of a label, so that the context box is width enough for all values
                    For Each Str As String In xRRows

                        Dim Graphics As Graphics = myLabel.CreateGraphics

                        Dim SizeF As SizeF = Graphics.MeasureString(Str, myTextB.Font)

                        If SizeF.Width > myTextB.Width Then myTextB.Width = SizeF.Width + 40

                    Next

                    _width = myLabel.Width

                    If _width < myTextB.Width Then _width = myTextB.Width

                    If _width < myLabel.Width Then _width = myLabel.Width

                    'the botton a little smaler because it looks better
                    myThawBt.Width = myTextB.Width - 4

                    Return _width

                End Get

                Set(ByVal value)

                    _width = value

                End Set

            End Property

            Property Left() As Integer

                Get

                    Return _left

                End Get

                Set(ByVal value As Integer)

                    _left = value

                End Set

            End Property

            Property Top() As Integer

                Get

                    Return _top

                End Get

                Set(ByVal value As Integer)

                    _top = value

                End Set

            End Property

            ReadOnly Property Controls()

                Get

                    myLabel.Text = Name

                    myLabel.Location = New Point(_left, _top + 0)

                    myTextB.Location = New Point(_left, _top + myLabel.Height)

                    myThawBt.Location = New Point(_left, _top + myLabel.Height + myTextB.Height)

                    myThawBt.Left += 2 'looks better

                    Dim myR As Control() = {myTextB, myThawBt, myLabel}

                    Return (myR)

                End Get

            End Property

            ''' <summary>
            ''' removing the other rows and sorting the result
            ''' </summary>
            Function pureList() As String()

                Dim mypureList = New List(Of String)

                For Each Line As Integer In DataFilterHelp.AllAllowedLines()

                    Dim str As String = xRRows(Line)

                    If Not mypureList.Contains(str) Then

                        mypureList.Add(str)

                    End If

                Next

                Return mypureList.ToArray

            End Function

#Region "Forms"
            Private Sub ThawBt_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles myThawBt.Click

                FilterString = ""

                myTextB.BackColor = Color.White

                myTextB.Text = ""

                Frozen = False

                myThawBt.Enabled = False

                DataFilterHelp.SetCols()

            End Sub

            Private Sub TextB_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles myTextB.Click

                If Frozen Then

                    FilterString = ""

                End If

                _myCont.Items.Clear()

                For Each Str As String In pureList()

                    _myCont.Items.Add(Str)

                Next

                _myCont.Show(myTextB, myTextB.Width / 2 - _myCont.Width / 2, myTextB.Height / 2 - myCont.Height / 2)

                If Frozen Then

                    FilterString = myTextB.Text

                End If

            End Sub

            Private Sub myCont_ItemClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles myCont.ItemClicked

                If Not isCompletition Then
                    Frozen = True
                End If

                myTextB.Text = e.ClickedItem.Text

                If Not isCompletition Then

                    FilterString = myTextB.Text

                    myTextB.BackColor = Color.LightSteelBlue

                    myThawBt.Enabled = True

                    DataFilterHelp.SetCols()

                End If

            End Sub

#End Region

        End Class

    End Namespace

End Namespace
