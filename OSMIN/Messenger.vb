Public Class Messenger

    Public Shared Sub Desaster(ByVal Message As String)

        MsgBox(Message, CType(MsgBoxStyle.Critical + MsgBoxStyle.OkOnly, MsgBoxStyle), "Basel Tools Plugin Desaster")

    End Sub

    Public Shared Sub Info(ByVal Message As String)

        MsgBox(Message, CType(MsgBoxStyle.Information + MsgBoxStyle.OkOnly, MsgBoxStyle), "Basel Tools Plugin")

    End Sub

End Class
