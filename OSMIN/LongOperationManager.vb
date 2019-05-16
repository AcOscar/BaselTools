Imports Autodesk.AutoCAD.Runtime

Public Class LongOperationManager
    Implements IDisposable
    Implements System.Windows.Forms.IMessageFilter

    ''' <summary>
    ''' The message code corresponding to a keypress
    ''' </summary>
    Const WM_KEYDOWN As Integer = &H100

    ''' <summary>
    ''' The number of times to update the progress meter
    ''' (for some reason you need 600 to tick through for each percent)
    ''' </summary>
    Const progressMeterIncrements As Integer = 600

    ''' <summary>
    ''' Internal members for metering progress
    ''' </summary>
    Private pm As ProgressMeter
    Private updateIncrement As Long
    Private currentInc As Long

    ''' <summary>
    ''' External flag for checking cancelled status
    ''' </summary>
    Public cancelled As Boolean = False

    ' Constructor

    Public Sub New(ByVal message As String)
        System.Windows.Forms.Application.AddMessageFilter(Me)
        pm = New ProgressMeter()
        pm.Start(message)
        pm.SetLimit(progressMeterIncrements)
        currentInc = 0
    End Sub

    ''' <summary>
    ''' System.IDisposable.Dispose
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        pm.Stop()
        pm.Dispose()
        System.Windows.Forms.Application.RemoveMessageFilter(Me)
    End Sub

    'Public Sub SetTotalOperations(ByVal totalOps As Long)
    '    ' We really just care about when we need
    '    ' to update the timer
    '    updateIncrement = (If(totalOps > progressMeterIncrements, totalOps \ progressMeterIncrements, totalOps))
    'End Sub

    Private _totalOps As Long

    ''' <summary>
    ''' Get or Set the total number of operations
    ''' </summary>
    Public Property TotalOperations As Long

        Get

            Return _totalOps

        End Get

        Set(ByVal totalOps As Long)

            updateIncrement = (If(totalOps > progressMeterIncrements, totalOps \ progressMeterIncrements, totalOps))

        End Set

    End Property

    ' 
    ''' <summary>
    ''' This function is called whenever an operation is performed
    ''' </summary>
    Public Function Tick() As Boolean
        If System.Threading.Interlocked.Increment(currentInc) = updateIncrement Then
            pm.MeterProgress()
            currentInc = 0
            System.Windows.Forms.Application.DoEvents()
        End If
        ' Check whether the filter has set the flag
        If cancelled Then
            pm.Stop()
        End If

        Return Not cancelled
    End Function

    ''' <summary>
    ''' The message filter callback
    ''' </summary>
    Public Function PreFilterMessage(ByRef m As System.Windows.Forms.Message) As Boolean Implements System.Windows.Forms.IMessageFilter.PreFilterMessage
        If m.Msg = WM_KEYDOWN Then
            ' Check for the Escape keypress
            Dim kc As System.Windows.Forms.Keys = CType(CInt(m.WParam), System.Windows.Forms.Keys) And System.Windows.Forms.Keys.KeyCode

            If m.Msg = WM_KEYDOWN AndAlso kc = System.Windows.Forms.Keys.Escape Then
                cancelled = True
            End If

            ' Return true to filter all keypresses
            Return True
        End If
        ' Return false to let other messages through
        Return False
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
