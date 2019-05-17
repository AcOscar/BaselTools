Imports System.Drawing
Imports System.Windows.Forms

Namespace ExcelTools

    Namespace DatafilterHelp

        Class DataFilterHelp

            'the list with properties which should be set
            Public Shared PSList As ListValues

            'a collectionof all columns
            Private Shared AllCols As New List(Of FilterColumn)

            'the list of all lines

            Private Shared FullListofLines As List(Of Integer)

            Private Shared myOKBt As Button

            Private Shared myCancelBt As Button

            Private Shared myForm As Form

            Private Shared OldFileName As String

            Private Shared OldDataName As String

            Private Shared OldFileDate As Date

            Private Shared withCheck As Boolean

            Private Shared withConcurrent As Boolean

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <param name="Filename">an Excelfilename with the values to filter</param>
            ''' <param name="DataName">the Dataname in the Excelfilename with the values</param>
            ''' <param name="PreSetVals">the Values to show on loading</param>
            ''' <param name="Check">True/False as STRING, if "True" so the Form will accept only values from the Excelfiles, if "False" you can type own new values</param>
            ''' <param name="Concurrent">try to find as less rows as possible(false) or retunrs for every filed all options (true)</param>
            ''' <returns>the selected values, on errors or canceling the list will be empty (Nothing)</returns>
            ''' <remarks></remarks>
            Shared Function Main(ByVal Filename As String,
                                 ByVal DataName As String,
                                 ByVal PreSetVals As ListValues,
                                 ByVal Check As String,
                                 ByVal Concurrent As String) _
                             As ListValues

                If Check.ToLower = "false" Then

                    withCheck = False

                Else

                    withCheck = True

                End If

                If Concurrent.ToLower = "false" Then

                    withConcurrent = False

                Else

                    withConcurrent = True

                End If

                'our list for returning
                Dim myList As New ListValues

                'check the file exists
                If Not My.Computer.FileSystem.FileExists(Filename) Then

                    Return myList

                End If

                'if we have read already we do not read the file again 
                Dim curFileInfo As IO.FileInfo = My.Computer.FileSystem.GetFileInfo(Filename)

                If Not (OldDataName = DataName AndAlso OldFileName = Filename AndAlso OldFileDate = curFileInfo.LastWriteTimeUtc) Then

                    Dim myRead As New DataRead(Filename, DataName)
                    'this function works only if DataName exisits

                    myRead.readValues()

                    AllCols.Clear()

                    For Each col In myRead.DataTable

                        Dim myNewDFCol As New FilterColumn(withConcurrent, withCheck)

                        myNewDFCol.Name = col.Name

                        myNewDFCol.xRRows = col.xRRows

                        AllCols.Add(myNewDFCol)

                    Next

                    FullListofLines = New List(Of Integer)

                    OldFileName = Filename

                    OldDataName = DataName

                    OldFileDate = curFileInfo.LastWriteTimeUtc

                End If

                'only if all data was read
                If AllCols.Count > 0 Then
                    'biuld up controls 
                    Dim myWidth As Integer = 0

                    Dim myHeight As Integer = 0

                    Dim myMargin As Integer = 12

                    myForm = New Form

                    Dim Left As Integer = myMargin

                    Dim Top As Integer = myMargin

                    For Each FCol As FilterColumn In AllCols

                        FCol.FilterString = ""

                        'just onetime because it will call for every column
                        If FullListofLines.Count <> FCol.xRRows.Count Then

                            For i As Integer = 0 To FCol.xRRows.Count - 1

                                FullListofLines.Add(i)

                            Next

                        End If

                        FCol.myTextB.Text = ""

                        FCol.myTextB.BackColor = Color.White

                        FCol.myThawBt.Enabled = False

                        FCol.Frozen = False

                        FCol.Left = Left

                        FCol.Top = Top

                        myForm.Controls.AddRange(FCol.Controls)

                        Left += FCol.Width

                        myWidth += FCol.Width

                        myHeight = FCol.Height

                    Next

                    With myForm

                        .StartPosition = FormStartPosition.CenterParent

                        .FormBorderStyle = FormBorderStyle.FixedToolWindow

                        .Text = "Property Filter"

                        .ShowInTaskbar = True

                    End With

                    myOKBt = New Button

                    With myOKBt

                        .Width = 50

                        .Top = myHeight + 24

                        .DialogResult = DialogResult.OK

                        .Text = "OK"

                        .TabStop = False

                        .TabIndex = 0

                        .Enabled = Not withCheck

                    End With

                    myCancelBt = New Button

                    With myCancelBt

                        .Left = 150

                        .Width = 50

                        .Top = myHeight + 24

                        .DialogResult = DialogResult.Cancel

                        .Text = "Cancel"

                        .TabStop = False

                        .TabIndex = 0

                    End With

                    myForm.Controls.Add(myOKBt)

                    myForm.Controls.Add(myCancelBt)

                    myForm.Width = myWidth + 2 * myMargin

                    myForm.CancelButton = myCancelBt

                    '24 for the head row
                    myForm.Height = 24 + myMargin + myHeight + myMargin + myOKBt.Height + myMargin

                    myOKBt.Left = myForm.Width - myMargin - myCancelBt.Width - myMargin - myOKBt.Width

                    myCancelBt.Left = myForm.Width - myMargin - myCancelBt.Width

                    If withConcurrent Then

                        If PreSetVals.List.Count <> 0 Then

                            For Each FCol As FilterColumn In AllCols

                                Dim Value As String = PreSetVals.GetValue(FCol.Name)

                                If Value <> "" Then

                                    FCol.myTextB.Text = Value

                                End If

                            Next

                        End If

                    Else

                        If PreSetVals.List.Count <> 0 Then

                            For Each FCol As FilterColumn In AllCols

                                Dim Value As String = PreSetVals.GetValue(FCol.Name)

                                If Value <> "" Then

                                    FCol.myTextB.Text = Value

                                End If

                                Dim myFilter = FCol.myTextB.Text

                                If withCheck Then

                                    FCol.myTextB.ReadOnly = True

                                    FCol.myTextB.BackColor = Color.White

                                Else

                                    FCol.myTextB.ReadOnly = False

                                End If

                                Select Case FCol.pureList.Length

                                    Case 0 'not exist, we have to fix

                                        FCol.FilterString = ""

                                        FCol.Frozen = False

                                        FCol.myTextB.BackColor = Color.Red

                                        FCol.myTextB.Text = myFilter

                                        FCol.myThawBt.Enabled = False

                                    Case 1 'existss once

                                        FCol.FilterString = ""

                                        FCol.Frozen = False

                                        If FCol.pureList(0) = myFilter Then

                                            FCol.myTextB.BackColor = Color.LightGreen

                                        Else

                                            FCol.myTextB.BackColor = Color.Red

                                        End If

                                        FCol.myTextB.Text = myFilter

                                        FCol.myThawBt.Enabled = False

                                    Case Else 'exist several times, we hav to freeze the value

                                        If Not myFilter = "" Then

                                            FCol.FilterString = myFilter

                                            FCol.Frozen = True

                                            FCol.myTextB.BackColor = Color.LightSteelBlue

                                            FCol.myTextB.Text = myFilter

                                            FCol.myThawBt.Enabled = True

                                        End If

                                End Select

                            Next

                        End If

                    End If

                    Dim myRes As DialogResult = myForm.ShowDialog()

                    If myRes = DialogResult.OK Then

                        For Each Col As FilterColumn In AllCols

                            Dim myListEntry As New ListValues.ListEntry

                            myListEntry.Name = Col.Name

                            myListEntry.Value = Col.myTextB.Text

                            myList.List.Add(myListEntry)

                        Next

                    End If

                End If

                Return myList

            End Function

            Friend Shared Function AllAllowedLines() As List(Of Integer)

                Dim myList As New List(Of Integer)

                myList.Clear()

                myList.InsertRange(0, FullListofLines)

                For Each Col As FilterColumn In AllCols

                    If Col.FilterString <> "" Then

                        Dim match2 As String

                        For j As Integer = 0 To Col.xRRows.Count - 1 'the rows

                            match2 = Col.xRRows(j)

                            If match2 <> Col.FilterString Then

                                myList.Remove(j)

                            End If

                        Next

                    End If

                Next

                AllAllowedLines = myList

            End Function

            ''' <summary>
            ''' sets unfrozen colums to the already determinded values
            ''' </summary>
            Friend Shared Sub SetCols()

                Dim setedcols As Integer = 0

                For Each sp As FilterColumn In AllCols

                    If Not sp.Frozen Then

                        If sp.pureList.Length = 1 Then

                            sp.myTextB.Text = sp.pureList(0)

                            sp.myTextB.BackColor = Color.LightGreen

                            setedcols += 1
                        Else

                            sp.myTextB.Text = ""

                            sp.myTextB.BackColor = Color.White

                        End If

                    Else

                        sp.myTextB.BackColor = Color.LightSteelBlue

                        sp.myThawBt.Enabled = True

                        setedcols += 1

                    End If
                Next

                If setedcols = AllCols.Count Then

                    If withCheck Then

                        myOKBt.Enabled = True

                    End If

                    myOKBt.Select()

                Else

                    If withCheck Then

                        myOKBt.Enabled = False

                    End If

                    myForm.Select()

                End If

            End Sub

        End Class

    End Namespace

End Namespace

