Class OSM

    Property Nodes As Dictionary(Of Long, OSM_Node)

    Property Nodes2Insert As List(Of Long)

    Property Ways As Dictionary(Of Long, OSM_Way)

    Property Relations As Dictionary(Of Long, OSM_Relation)

    Public Property Settings As OSM.OSM_Settings

    Public Sub New()

        Nodes = New Dictionary(Of Long, OSM_Node)

        Ways = New Dictionary(Of Long, OSM_Way)

        Relations = New Dictionary(Of Long, OSM_Relation)

        Nodes2Insert = New List(Of Long)

    End Sub

    Class OSM_Settings

        Property BoundMin As OSM_Node

        Property BoundMax As OSM_Node

        Property FileFilter As String

        Property Styles As List(Of ImportStyle)

        Public Sub New()

            BoundMin = New OSM_Node

            BoundMax = New OSM_Node

        End Sub

    End Class

    ''' <summary>
    ''' the minimum number of ticks
    ''' </summary>
    ''' <remarks>later we upgrade with the number of nodes to insert</remarks>
    ReadOnly Property minimumTicks As Long
        Get
            Dim mt As Long

            'for each node
            'we tick while we removing them from the list to insert 
            'and whne we insert the rest
            mt += Nodes.Count

            'for each node in a way
            'we tick when the way is created
            For Each w As KeyValuePair(Of Long, OSM_Way) In Ways

                mt += (w.Value.NodeIds.Count)

            Next

            'for each node in a relation

            For Each r As KeyValuePair(Of Long, OSM_Relation) In Relations

                mt += (r.Value.NodesCount * 2)

            Next

            Return mt

        End Get

    End Property

    Public Function ReadFile(ByVal Filename As String) As Boolean

        Dim myOSMFile As XElement

        If IO.File.Exists(Filename) Then

            myOSMFile = XElement.Load(Filename)

            Try

                For Each XMLNode As XElement In myOSMFile.Elements

                    Select Case XMLNode.Name

                        Case "bounds"
                            Me.Settings.BoundMin.lon = XMLNode.@minlon
                            Me.Settings.BoundMin.lat = XMLNode.@minlat
                            Me.Settings.BoundMax.lon = XMLNode.@maxlon
                            Me.Settings.BoundMax.lat = XMLNode.@maxlat

                        Case "node"
                            Dim myNode As New OSM_Node

                            Dim myid As Long

                            myid = myNode.fromXML(XMLNode)

                            Nodes.Add(myid, myNode)

                        Case "way"
                            Dim myWay As New OSM_Way

                            Dim myid As Long

                            myid = myWay.fromXML(XMLNode)

                            Ways.Add(myid, myWay)

                        Case "relation"

                            Dim myRelation As New OSM_Relation

                            Dim id As Long

                            id = myRelation.fromXML(XMLNode)

                            Me.Relations.Add(id, myRelation)

                    End Select

                Next

            Catch ex As Exception

                MsgBox(ex.Message.ToString, MsgBoxStyle.Critical, "ReadFile - Error")

                Return False

            End Try
        Else

            MsgBox("Couldn't open: " & Filename, MsgBoxStyle.Critical, "ReadFile - Error")

            Return False

        End If

        myOSMFile.RemoveAll()

        Return True

    End Function

    Sub buildRelations_old()


        For Each Rel As KeyValuePair(Of Long, OSM_Relation) In Relations
            'fill the relation members and delete the not existing(downloaded) elements

            Dim mmtr As New List(Of Integer)

            For Each member As OSM_Relation.OSM_Member In Rel.Value.Members

                Select Case member.type

                    Case "way"

                        Ways.TryGetValue(member.ref, member.Way)

                        If member.Way Is Nothing Then

                            mmtr.Add(Rel.Value.Members.IndexOf(member))

                        End If

                End Select

            Next member

            'delete the not existing refrences
            Dim eletoremov As Integer = 0

            For Each mmm As Integer In mmtr

                Rel.Value.Members.RemoveAt(mmm - eletoremov)

                eletoremov += 1

            Next

            'mark them that the relastion is not completed anymore
            If mmtr.Count > 0 Then

                Rel.Value.isNotCompleted = True

            End If

            If Rel.Value.isMultipolygon Then

                Rel.Value.Loops4AutoCAD = New List(Of OSM_Relation.MuliPolyPart)

                Dim PreviosRole As String = ""

                Dim myPart As OSM_Relation.MuliPolyPart = Nothing

                For Each member In Rel.Value.Members

                    'is none is actually depracted, but never say never
                    If member.role = "none" Then

                        member.role = "outer"

                    End If

                    'this could happens sometimes and means is it an outer
                    If member.role = "" Then

                        member.role = "outer"

                    End If

                    'previos is initialzed with "", so this marks if that we have a change from outer to inner or vs.
                    If PreviosRole <> member.role Then
                        'add a ^new multipolypart

                        'we have a new role so we have to save the previus one
                        If myPart IsNot Nothing Then

                            Rel.Value.Loops4AutoCAD.Add(myPart)

                        End If
                        'the new role
                        myPart = New OSM_Relation.MuliPolyPart

                        PreviosRole = member.role

                    End If

                    If myPart IsNot Nothing Then

                        myPart.getNodes(member)

                    End If


                Next

                Rel.Value.Loops4AutoCAD.Add(myPart)

            End If

            lom.Tick()

            If lom.cancelled Then Exit Sub

        Next Rel

    End Sub

    Sub buildRelations()

        For Each Rel As KeyValuePair(Of Long, OSM_Relation) In Relations
            'fill the relation members and delete the not existing(downloaded) elements

            Dim mmtr As New List(Of Integer)

            For Each member As OSM_Relation.OSM_Member In Rel.Value.Members

                Select Case member.type

                    Case "way"

                        Ways.TryGetValue(member.ref, member.Way)

                        If member.Way Is Nothing Then

                            mmtr.Add(Rel.Value.Members.IndexOf(member))

                        End If

                End Select

            Next member

            'delete the not existing refrences
            Dim eletoremov As Integer = 0

            For Each mmm As Integer In mmtr

                Rel.Value.Members.RemoveAt(mmm - eletoremov)

                eletoremov += 1

            Next

            'mark them that the relastion is not completed anymore
            If mmtr.Count > 0 Then

                Rel.Value.isNotCompleted = True

            End If

            If Rel.Value.isMultipolygon Then

                Rel.Value.Loops4AutoCAD = New List(Of OSM_Relation.MuliPolyPart)

                Dim PreviosRole As String = ""

                Dim myPart As OSM_Relation.MuliPolyPart = Nothing

                For Each member In Rel.Value.Members

                    'none is actually depracted, but never say never
                    If member.role = "none" Then

                        member.role = "outer"

                    End If

                    'this could happens sometimes and means it is an outer
                    If member.role = "" Then

                        member.role = "outer"

                    End If

                    'previos is initialzed with "", so this marks if that we have a change from outer to inner or vs.
                    If PreviosRole <> member.role Then
                        'add a ^new multipolypart

                        'we have a new role so we have to save the previus one
                        If myPart IsNot Nothing Then

                            Rel.Value.Loops4AutoCAD.Add(myPart)

                        End If


                        'the new role
                        myPart = New OSM_Relation.MuliPolyPart

                        PreviosRole = member.role

                    End If

                    If myPart IsNot Nothing Then

                        If Not (member.type = "way") Then
                            Continue For
                        End If

                        If myPart.NodeIds.Count = 0 Then

                            ' myPart.getNodes(member)
                            myPart.NodeIds.AddRange(member.Way.NodeIds)
                        Else

                            If myPart.NodeIds.First = member.Way.NodeIds.First Then
                                'we have to reverting the way and insert at the start point that the loop 
                                'has after that the first point of the way as first point
                                member.Way.NodeIds.Reverse()
                                myPart.NodeIds.InsertRange(0, (member.Way.NodeIds))

                            ElseIf myPart.NodeIds.First = member.Way.NodeIds.Last Then
                                myPart.NodeIds.InsertRange(0, (member.Way.NodeIds))

                            ElseIf myPart.NodeIds.Last = member.Way.NodeIds.First Then
                                'this is perfect the first node of a part is last of aur ring
                                myPart.NodeIds.AddRange(member.Way.NodeIds)
                            ElseIf myPart.NodeIds.Last = member.Way.NodeIds.Last Then
                                member.Way.NodeIds.Reverse()
                                myPart.NodeIds.AddRange(member.Way.NodeIds)
                            Else
                                'if no one of the ends fits to each other it has to be a new polyring
                                'at first we have to save the old one
                                Rel.Value.Loops4AutoCAD.Add(myPart)
                                ' and craete now  a new one
                                myPart = New OSM_Relation.MuliPolyPart
                                'and add the nodes
                                myPart.NodeIds.AddRange(member.Way.NodeIds)
                            End If

                        End If

                    End If

                Next

                Rel.Value.Loops4AutoCAD.Add(myPart)

            End If

            lom.Tick()

            If lom.cancelled Then Exit Sub

        Next Rel

    End Sub

    ''' <summary>
    ''' read all settings from the *.workshop.xml
    ''' </summary>
    Function ReadConfigXML(ByVal Filename As String) As Boolean

        Me.Settings = New OSM_Settings

        Dim myReturn As Boolean = False

        Me.Settings.Styles = New List(Of ImportStyle)

        Dim myConfig As XElement = XElement.Load(Filename)

        For Each x As XElement In myConfig.<openfilefilter>

            Me.Settings.FileFilter = x.Value

        Next

        For Each x As XElement In myConfig.<defaultstyle>

            OSM_Painter.CurrentStyleName = x.Value

        Next

        If OSM_Painter.CurrentStyleName = "" Then Return False

        For Each x As XElement In myConfig.<importstyle>

            Dim myStyle As New ImportStyle()

            myStyle.Load(x)

            Me.Settings.Styles.Add(myStyle)

        Next

        For Each st In Me.Settings.Styles

            If st.Name = OSM_Painter.CurrentStyleName Then

                OSM_Painter.CurrentStyle = st

                myReturn = True

                Exit For

            End If

        Next

        Return myReturn

    End Function

End Class
