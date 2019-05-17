Imports System.Windows.Forms

Public Class XMLREPAIRFORM
    Private Constructs As String
    Private Elements As String
    Private Sheets As String
    Private Checked As Integer
    Private Repaired As Integer
    Private CreatedFiles As Integer

    Dim _RootPathConstruct As String
    Dim _RootPathElements As String
    Dim _RootPathSheets As String
    Dim _RepairPath As String

    Public Property RootPathConstruct() As String
        Get
            Return _RootPathConstruct
        End Get
        Set(ByVal value As String)
            If My.Computer.FileSystem.DirectoryExists(value) Then
                If value.EndsWith("\") Then
                    value = value.Substring(0, value.Length - 1)
                End If
                _RootPathConstruct = value
            End If
        End Set
    End Property
    Public Property RootPathElements() As String
        Get
            Return _RootPathElements
        End Get
        Set(ByVal value As String)
            If My.Computer.FileSystem.DirectoryExists(value) Then
                If value.EndsWith("\") Then
                    value = value.Substring(0, value.Length - 1)
                End If
                _RootPathElements = value
            End If
        End Set
    End Property
    Public Property RootPathSheets() As String
        Get
            Return _RootPathSheets
        End Get
        Set(ByVal value As String)
            If My.Computer.FileSystem.DirectoryExists(value) Then
                If value.EndsWith("\") Then
                    value = value.Substring(0, value.Length - 1)
                End If
                _RootPathSheets = value
            End If
        End Set
    End Property
    Public ReadOnly Property RepairPath()
        Get
            Return _RepairPath
        End Get
    End Property


    Private Sub Form3_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        TreeView1.Nodes.Clear()
        Dim myNode As TreeNode
        Dim myString As String

        myString = _RootPathConstruct
        myString = myString.Substring(myString.LastIndexOf("\") + 1)
        myNode = TreeView1.Nodes.Add(myString)
        myNode.Tag = _RootPathConstruct
        myNode.ToolTipText = _RootPathConstruct
        AddDirs(_RootPathConstruct, myNode)

        myString = _RootPathElements
        myString = myString.Substring(myString.LastIndexOf("\") + 1)
        myNode = TreeView1.Nodes.Add(myString)
        myNode.Tag = _RootPathElements
        myNode.ToolTipText = _RootPathElements
        AddDirs(_RootPathElements, myNode)

        myString = _RootPathSheets
        myString = myString.Substring(myString.LastIndexOf("\") + 1)
        myNode = TreeView1.Nodes.Add(myString)
        myNode.Tag = _RootPathSheets
        myNode.ToolTipText = _RootPathSheets
        AddDirs(_RootPathSheets, myNode)

    End Sub

    Private Sub AddDirs(ByVal path As String, ByVal node As TreeNode)
        For Each dirp In My.Computer.FileSystem.GetDirectories(node.Tag.ToString, FileIO.SearchOption.SearchTopLevelOnly)
            Dim myString As String = dirp
            myString = myString.Substring(myString.LastIndexOf("\") + 1)
            Dim myNode As TreeNode = node.Nodes.Add(myString)
            myNode.Tag = dirp
            myNode.ToolTipText = dirp
            AddDirs(myNode.Tag, myNode)
        Next
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim myNode As TreeNode = TreeView1.SelectedNode
        If myNode IsNot Nothing Then
            _RepairPath = myNode.Tag.ToString
            repair(_RepairPath)
            Me.DialogResult = Windows.Forms.DialogResult.OK
            Me.Close()
        End If
    End Sub

    Private Sub TreeView1_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles TreeView1.DoubleClick
        If TypeOf (sender) Is TreeView Then
            Dim myNode As TreeNode = TreeView1.SelectedNode
            _RepairPath = myNode.Tag.ToString
            repair(_RepairPath)

            Me.DialogResult = Windows.Forms.DialogResult.OK
        End If
    End Sub

    Private Sub repair(ByVal path As String)
        Checked = 0
        Repaired = 0
        CreatedFiles = 0
        If IO.Directory.Exists(path) Then
            For Each FileName As String In IO.Directory.GetFiles(path, "*.dwg", IO.SearchOption.AllDirectories)
                Debug.Print(FileName)
                Checked += 1
                Dim myXml As New XMLcomplement
                myXml.XmlFileName = FileName.Remove(FileName.Length - 4) & ".xml"
                If myXml.Exists Then
                    If myXml.CheckRepairXML() Then
                        Repaired += 1
                    End If
                Else
                    myXml.Create()
                    CreatedFiles += 1
                End If
            Next
        End If
        MsgBox(Checked & " Files. " & Repaired & " Files repaired and " & CreatedFiles & " Files created.")

    End Sub

    Private Function strippath(ByVal arg1 As String) As String
        Dim path As String = arg1
        Dim startpos As Integer
        Dim lastpos As Integer
        startpos = path.IndexOf("""") + 1
        lastpos = path.LastIndexOf("""")
        path = path.Substring(startpos, lastpos - startpos)
        Return path
    End Function

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.Close()
    End Sub

End Class