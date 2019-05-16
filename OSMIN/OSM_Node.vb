Public Class OSM_Node

    'Property id As Long

    ''' <summary>
    ''' y
    ''' </summary>
    Property lat As Double

    ''' <summary>
    ''' x
    ''' </summary>
    Property lon As Double

    Property tags As Dictionary(Of String, String)

    Public Sub New()

        tags = New Dictionary(Of String, String)

    End Sub

    Shared Operator =(ByVal Node1 As OSM_Node, ByVal Node2 As OSM_Node) As Boolean

        If Node1.lat = Node2.lat Then

            If Node1.lon = Node2.lon Then

                Return True

            Else

                Return False

            End If

        Else

            Return False

        End If

    End Operator

    Shared Operator <>(ByVal Node1 As OSM_Node, ByVal Node2 As OSM_Node) As Boolean

        Return Not Node1 = Node2

    End Operator

    ReadOnly Property X
        Get

            Return OSM_MercatorProjection.lonToX(lon)

        End Get

    End Property

    ReadOnly Property Y
        Get

            Return OSM_MercatorProjection.latToY(lat)

        End Get

    End Property

    Function fromXML(ByVal XMLNode As XElement) As Long

        Dim myId As Long

        If XMLNode.HasAttributes Then

            myId = XMLNode.@id

            lat = XMLNode.@lat

            lon = XMLNode.@lon

        End If

        If XMLNode.HasElements Then

            For Each node As XElement In XMLNode.Elements

                Select Case node.Name.LocalName.ToLower

                    Case "tag"

                        Dim k, v As String

                        k = node.@k

                        v = node.@v

                        tags.Add(k, v)

                End Select

            Next

        End If

        Return myId

    End Function

End Class
