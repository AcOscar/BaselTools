Imports System.IO
Imports Autodesk.AutoCAD.ApplicationServices
Imports Microsoft.Office.Interop
Imports Microsoft.VisualBasic.FileIO

Namespace ExcelTools

    ''' <summary>
    ''' Class of Values (Table) from an ExcelFile
    ''' </summary>
    Public Class DataRead
        Implements IEquatable(Of DataRead)

        ''' <summary>
        ''' it could also a relative path
        ''' </summary>
        ''' <remarks>relativ to the drawing path</remarks>
        Private _FileName As String

        Private _DataName As String

        Private _Seperator As String

        Friend DataTable As List(Of xRColumn)

        Public ReadOnly Property Shortname() As String

            Get

                Return My.Computer.FileSystem.GetName(_FileName) & "!" & _DataName

            End Get

        End Property

        Public Sub New(ByVal FileName As String, ByVal DataName As String, Optional ByVal Seperator As String = vbTab)

            _FileName = FileName

            _DataName = DataName

            _Seperator = Seperator

        End Sub

        Public ReadOnly Property Filename() As String

            Get

                Return _FileName

            End Get

        End Property

        Public ReadOnly Property DataName() As String

            Get

                Return _DataName

            End Get

        End Property

        Public ReadOnly Property Seperator()

            Get

                Return _Seperator

            End Get

        End Property

        Private ReadOnly Property RealFilename

            Get

                Dim pathkind As String

                If _FileName.Length < 4 Then
                    Return ""

                End If

                If _FileName.Substring(1, 2) = ":\" OrElse _FileName.StartsWith("\\") Then
                    pathkind = "fullPath"

                ElseIf _FileName.Contains("\") Then
                    pathkind = "relativePath"

                Else
                    pathkind = "noPath"

                End If

                Dim dwgPath As String = My.Computer.FileSystem.GetParentPath(Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Name)

                Select Case pathkind

                    Case "relativePath"
                        If File.Exists(FileSystem.CombinePath(dwgPath, _FileName)) Then

                            Return FileSystem.CombinePath(dwgPath, _FileName)

                        End If

                    Case "fullPath"
                        If File.Exists(_FileName) Then

                            Return _FileName

                        End If

                    Case "noPath"
                        If File.Exists(FileSystem.CombinePath(dwgPath, _FileName)) Then

                            Return FileSystem.CombinePath(dwgPath, _FileName)

                        End If

                End Select

                Return ""

            End Get

        End Property

        ''' <summary>
        ''' read the values from an excelfile
        ''' </summary>
        ''' <remarks>if reader.datatable Nothing means the file does not exists
        ''' if datatable.count = 0 means there is no datarange</remarks>
        Public Sub readValues()

            If Not File.Exists(RealFilename) Then
                Exit Sub
            End If

            Dim FileInfo As System.IO.FileInfo

            FileInfo = My.Computer.FileSystem.GetFileInfo(RealFilename)

            Select Case FileInfo.Extension.ToLower

                Case ".xls", ".xlsx"

                    ReadExcel()

            End Select

        End Sub

        Sub Open()

            Dim xlApp As Excel.Application

            Dim xlBook As Excel.Workbook

            'reading excel
            xlApp = New Excel.Application

            'no clue what this is otherwise we get strange error message
            System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US")

            xlApp.Visible = True

            xlBook = xlApp.Workbooks.Open(RealFilename, , )

        End Sub

        Private Sub ReadExcel()

            Dim xlApp As Excel.Application

            Dim xlBook As Excel.Workbook

            'reading excel
            xlApp = New Excel.Application

            'no clue what this is otherwise we get strange error message
            System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("en-US")

            xlApp.Visible = False

            xlBook = xlApp.Workbooks.Open(RealFilename, False, True)

            DataTable = New List(Of xRColumn)

            'we need this to check that we have a named range twice
            'this could actually not happen but, is some circumstance it could
            Dim Done As Boolean = False

            For Each ExcelName As Excel.Name In xlBook.Names

                If ExcelName.Name.ToLower = _DataName.ToLower Then
                    'todo: check that this works
                    If Not Done Then

                        Dim myCellRange As Excel.Range = ExcelName.RefersToRange

                        Dim myDataRange As Array = myCellRange.Value

                        Dim rows = UBound(myDataRange, 1)

                        'if there not enough rows we abort
                        If rows < 2 Then

                            Exit For

                        End If

                        Dim cols = UBound(myDataRange, 2)

                        Try

                            For colNr As Integer = 1 To cols

                                Dim mySp As New xRColumn

                                Dim RowTitle As String

                                RowTitle = myDataRange(1, colNr)

                                'if is there no title we can not do the rest and we abort here
                                If RowTitle = "" Then

                                    Continue For

                                Else

                                    mySp.Name = RowTitle

                                End If

                                For Row As Integer = 2 To rows


                                    If myDataRange(Row, colNr) Is Nothing Then

                                        mySp.xRRows.Add("")

                                    Else

                                        Select Case (myDataRange(Row, colNr)).GetType.Name
                                            'Case "String"
                                            '    mySp.xRRows.Add((myDataRange(Row, colNr)))
                                            'Case "Int32"
                                            '    mySp.xRRows.Add("")
                                            'Case "Double"
                                            '    mySp.xRRows.Add((myDataRange(Row, colNr)))

                                            Case "DateTime"
                                                mySp.xRRows.Add(ExcelName.RefersToRange(Row, colNr).text)

                                            Case Else
                                                mySp.xRRows.Add((myDataRange(Row, colNr)))

                                        End Select
                                        ' mySp.xRRows.Add((myDataRange(Row, colNr).text))

                                    End If

                                Next

                                DataTable.Add(mySp)

                            Next

                        Catch ex As Exception

                        End Try
                    Else
                        MsgBox(String.Format("Warning, the related Execelfile {0} contains muliple Named-Ranges with the name:{1}", RealFilename, ExcelName.Name),
                               MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, "Excel Reader Warning")
                    End If

                    Done = True

                    Exit For  ' the name of the filtername exist just one time and we can escape here

                End If

            Next

            xlBook.Close(False)

            killProcess(xlApp.Hwnd)

        End Sub

        Public Function EqualsExcelRead(ByVal other As DataRead) As Boolean Implements System.IEquatable(Of DataRead).Equals

            If Me._FileName.ToLower = other._FileName.ToLower And Me._DataName = other._DataName Then

                Return True

            Else

                Return False

            End If

        End Function

        ''' <summary>
        ''' Open an exists Excel file, get Sheet1:A1 value
        ''' </summary>
        Public Shared Sub OpenXL(ByVal FileName As String, ByVal range As String)

            Dim xlsApp As Excel.Application

            Dim xlsWB As Excel.Workbook

            xlsApp = New Excel.Application

            xlsApp.Visible = True

            xlsWB = xlsApp.Workbooks.Open(FileName)

            For Each ExcelName As Excel.Name In xlsWB.Names

                If ExcelName.Name.ToLower = range.ToLower Then

                    Dim myCellRange As Excel.Range = ExcelName.RefersToRange

                    myCellRange.Worksheet.Activate()

                    myCellRange.Select()

                End If

            Next

        End Sub


#Region "killProcess"

        Private Declare Function GetWindowThreadProcessId Lib "user32" (ByVal hWnd As Integer, ByRef lpdwProcessId As IntPtr) As IntPtr

        Private Shared Sub killProcess(ByRef Handle2Window As Integer)

            Dim processId As IntPtr

            GetWindowThreadProcessId(Handle2Window, processId)

            Dim myProcess As Process = Process.GetProcessById(processId.ToInt32())

            myProcess.Kill()

        End Sub

#End Region

    End Class

End Namespace
