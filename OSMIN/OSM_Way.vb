Public Class OSM_Way

    'Property Id As Long

    Property NodeIds As List(Of Long)

    Property isCorrupt As Boolean

    ReadOnly Property isClosed As Boolean

        Get
            If NodeIds(0) = NodeIds(NodeIds.Count - 1) Then

                Return True

            End If

            Return False

        End Get

    End Property

    ReadOnly Property isOpen As Boolean

        Get

            Return Not isClosed

        End Get

    End Property

    ReadOnly Property isArea As Boolean
        Get
            If isClosed Then

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

            End If

            Return False

        End Get

    End Property

    Property tags As Dictionary(Of String, String)

    Function fromXML(ByVal XMLNode As XElement) As Long


        Dim myId As Long

        If XMLNode.HasAttributes Then

            myId = XMLNode.@id

        End If

        If XMLNode.HasElements Then

            For Each xnode As XElement In XMLNode.Elements

                Select Case xnode.Name.LocalName.ToLower

                    Case "nd"
                        Dim myNode As New OSM_Node

                        If xnode.HasAttributes Then

                            ' myNode.id = node.GetAttribute("ref")
                            'myNode.id = xnode.@ref

                            NodeIds.Add(xnode.@ref)

                        End If

                    Case "tag"

                        Dim k, v As String

                        k = xnode.@k

                        v = xnode.@v

                        tags.Add(k, v)

                End Select

            Next

        End If

        Return myId

    End Function

    Public Sub New()

        NodeIds = New List(Of Long)

        tags = New Dictionary(Of String, String)

    End Sub

End Class
