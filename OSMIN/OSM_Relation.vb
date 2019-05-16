Class OSM_Relation

    Property Members As List(Of OSM_Member)

    Property tags As Dictionary(Of String, String)

    Property isNotCompleted As Boolean

    Sub New()

        Members = New List(Of OSM_Member)

        tags = New Dictionary(Of String, String)

    End Sub

    ReadOnly Property NodesCount As Integer
        Get
            Dim myCount As Integer

            For Each member As OSM_Relation.OSM_Member In Members

                Select Case member.type

                    Case "way"

                        If member.Way IsNot Nothing Then

                            myCount += member.Way.NodeIds.Count

                        End If

                    Case "node"

                        myCount += 1

                End Select

            Next

            Return myCount

        End Get
    End Property

    ReadOnly Property isMultipolygon As Boolean
        Get

            Dim Value As String = String.Empty

            If tags.TryGetValue("type", Value) Then

                If Value = "multipolygon" Then

                    Return True

                End If

            End If

            Return False

        End Get

    End Property

    ReadOnly Property isBuilding As Boolean
        Get

            Dim Value As String = String.Empty

            If tags.TryGetValue("building", Value) Then

                If Value <> "" Then

                    Return True

                End If

            End If

            Return False

        End Get

    End Property

    Function fromXML(ByVal XMLNode As XElement) As Long
        Dim myid As Long
        If XMLNode.HasAttributes Then

            myid = XMLNode.@id

        End If

        If XMLNode.HasElements Then

            For Each node As XElement In XMLNode.Elements

                Select Case node.Name.LocalName.ToLower

                    Case "member"
                        Dim myMember As New OSM_Member

                        myMember.fromXML(node)

                        Me.Members.Add(myMember)

                    Case "tag"

                        Dim k, v As String

                        k = node.@k

                        v = node.@v

                        tags.Add(k, v)

                End Select

            Next

        End If
        Return myid
    End Function

    Class OSM_Member

        Public type As String

        Public ref As Long

        Public role As String

        Sub fromXML(ByVal XMLNode As XElement)

            type = XMLNode.@type.ToLower

            ref = XMLNode.@ref.ToLower

            role = XMLNode.@role.ToLower

        End Sub

        Public Way As OSM_Way

        Public Node As OSM_Node

    End Class

    ReadOnly Property isArea As Boolean
        Get

            If isNotCompleted Then

                Return False

            End If

            If isMultipolygon Then

                Return True

            End If

            If tags.ContainsKey("barrier") Then

                If tags.ContainsKey("area") Then

                    If tags("area") = "yes" Then

                        Return True

                    Else
                        Return False

                    End If

                    Return True

                End If

            End If

            If tags.ContainsKey("highway") Then

                Return True

            End If

            If tags.ContainsKey("railway") Then

                If tags.ContainsKey("area") Then

                    If tags("area") = "platform" Then

                        If tags("area") = "yes" Then

                            Return True

                        Else
                            Return False

                        End If

                    End If

                    Return True

                End If

            End If

            If tags.ContainsKey("aeroway") Then

                If tags("aeroway") = "taxiway" Then

                    Return False

                End If

                Return True

            End If

            If tags.ContainsKey("building") Then

                Return True

            End If

            If tags.ContainsKey("landuse") Then

                Return True

            End If

            If tags.ContainsKey("leisure") Then

                Return True

            End If

            If tags.ContainsKey("natural") Then

                Return True

            End If

            Return False

        End Get

    End Property

    Property Loops4AutoCAD As List(Of MuliPolyPart)

    Class MuliPolyPart

        Public NodeIds As New List(Of Long)

        ''' <remarks>depracted for buildrelations2</remarks>
        ReadOnly Property isClosed
            Get

                If NodeIds.Last = NodeIds.First Then

                    Return True

                Else

                    Return False

                End If

            End Get

        End Property

        Public Outer As Boolean
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="member"></param>
        ''' <remarks>depracted for buildrelations2</remarks>
        Sub getNodes(ByVal member As OSM_Relation.OSM_Member)

            Select Case member.type

                Case "way"

                    If member.Way IsNot Nothing Then

                        NodeIds.AddRange(member.Way.NodeIds)

                    End If

                    'Case "node"

                    '    nodes.AddRange(member.Node)

            End Select

            If member.role = "outer" Then

                Outer = True

            End If

        End Sub

    End Class

End Class
