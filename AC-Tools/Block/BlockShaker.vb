Imports Autodesk.AutoCAD

Public Class BlockShaker
    Shared myDWG As ApplicationServices.Document
    Shared myDB As DatabaseServices.Database
    Shared myEd As EditorInput.Editor
    Shared myInsertedBlocks As New DatabaseServices.ObjectIdCollection
    Shared FillRate As Double = 100
    Shared XFuzzy As Double = 0
    Shared YFuzzy As Double = 0
    Shared ZFuzzy As Double = 0
    Shared AFuzzy As Double = 0
    Shared SFuzzy As Double = 0

    ''' <summary>
    ''' the main loop while input and preview the shaking process
    ''' </summary>
    Sub Shaker()
        myDWG = ApplicationServices.Application.DocumentManager.MdiActiveDocument
        myDB = myDWG.Database
        myEd = myDWG.Editor

        myInsertedBlocks = New DatabaseServices.ObjectIdCollection

        'only block insert are allowed
        Dim tvs As DatabaseServices.TypedValue() = {New DatabaseServices.TypedValue(0, "INSERT")}
        Dim sf As New EditorInput.SelectionFilter(tvs)

        Dim pso As New EditorInput.PromptSelectionOptions()
        pso.MessageForAdding = vbLf & "Select Blocks for the positions:"
        pso.AllowDuplicates = False

        Dim psrPos As EditorInput.PromptSelectionResult = myEd.GetSelection(pso, sf)

        If psrPos.Status = EditorInput.PromptStatus.Error Then
            Return
        End If

        If psrPos.Status = EditorInput.PromptStatus.Cancel Then
            Return
        End If

        pso.MessageForAdding = vbLf & "Select Blocks to insert:"
        pso.AllowDuplicates = False

        'we use the same selection filter beacuse for the insert we need blocks as well
        Dim psrIn As EditorInput.PromptSelectionResult = myEd.GetSelection(pso, sf)

        If psrIn.Status = EditorInput.PromptStatus.Error Then
            Return
        End If

        If psrIn.Status = EditorInput.PromptStatus.Cancel Then
            Return
        End If

        Dim Finish As Boolean = False

        Do

            Dim pKeyOpts As EditorInput.PromptKeywordOptions = New EditorInput.PromptKeywordOptions("")

            pKeyOpts.Keywords.Clear()
            pKeyOpts.Message = vbLf & "Mix Blocks (Enter to create):"
            pKeyOpts.Keywords.Add("x", "x", "X-fuzziness(" & XFuzzy & ")")
            pKeyOpts.Keywords.Add("y", "y", "Y-fuzziness(" & YFuzzy & ")")
            pKeyOpts.Keywords.Add("z", "z", "Z-fuzziness(" & ZFuzzy & ")")
            pKeyOpts.Keywords.Add("a", "a ", "Angle-fuzziness(" & AFuzzy & ")")
            pKeyOpts.Keywords.Add("s", "s", "Scale-fuzziness(" & SFuzzy & ")")
            pKeyOpts.Keywords.Add("r", "r ", "fill-Rate(" & FillRate & ")")
            pKeyOpts.Keywords.Add("f", "f", "Finish")
            pKeyOpts.AllowNone = True

            Dim PromptResult As EditorInput.PromptResult = myEd.GetKeywords(pKeyOpts)

            If PromptResult.Status = EditorInput.PromptStatus.Cancel Then

                myInsertedBlocks = New DatabaseServices.ObjectIdCollection

                Exit Sub

            End If

            Dim pdo As New EditorInput.PromptDoubleOptions("")

            pdo.AllowZero = True
            pdo.DefaultValue = 0
            pdo.UseDefaultValue = True

            Select Case PromptResult.StringResult

                Case "x"
                    pdo.Message = "X axis fuzziness:"
                    pdo.DefaultValue = XFuzzy
                    XFuzzy = myEd.GetDouble(pdo).Value

                Case "y"
                    pdo.Message = "Y axis fuzziness:"
                    pdo.DefaultValue = YFuzzy
                    YFuzzy = myEd.GetDouble(pdo).Value

                Case "z"
                    pdo.Message = "Z axis fuzziness:"
                    pdo.DefaultValue = ZFuzzy
                    ZFuzzy = myEd.GetDouble(pdo).Value

                Case "a"
                    pdo.Message = "angle fuzziness:"
                    pdo.DefaultValue = AFuzzy
                    AFuzzy = myEd.GetDouble(pdo).Value

                Case "r"
                    pdo.Message = "fill rate in %:"
                    pdo.DefaultValue = FillRate
                    FillRate = myEd.GetDouble(pdo).Value

                Case "s"
                    pdo.Message = "scale fuzziness:"
                    pdo.DefaultValue = SFuzzy
                    SFuzzy = myEd.GetDouble(pdo).Value

                Case ""
                    CreateMix(psrPos.Value, psrIn.Value)

                Case "f"
                    'otherwise on a posible new run our inserted blocks will delete
                    myInsertedBlocks = New DatabaseServices.ObjectIdCollection
                    Finish = True

            End Select

        Loop Until Finish

    End Sub

    ''' <summary>
    ''' the process to create the mixed arangement of the blocks
    ''' </summary>
    ''' <param name="ssPos">the selection set wtih the blocks for the position</param>
    ''' <param name="ssIn">the selection set with all blocks to insert</param>
    Private Shared Sub CreateMix(ByRef ssPos As EditorInput.SelectionSet, ByRef ssIn As EditorInput.SelectionSet)

        Try

            Dim IdsPos As List(Of DatabaseServices.ObjectId) = ssPos.GetObjectIds().ToList

            Dim idarrayIn As DatabaseServices.ObjectId() = ssIn.GetObjectIds()

            Dim numbersToFill As Integer

            numbersToFill = CInt((IdsPos.Count * FillRate) / 100)

            'removing blocks that not be filled
            Do While IdsPos.Count > numbersToFill
                Dim myRnd As Integer = CInt(Int((IdsPos.Count) * Rnd()))

                IdsPos.RemoveAt(myRnd)

            Loop

            Dim myBT As DatabaseServices.BlockTable
            Dim myBTR As DatabaseServices.BlockTableRecord

            Dim tr As DatabaseServices.Transaction = myDB.TransactionManager.StartTransaction()

            Using tr

                'delete the (maybe) previous created blocks before we create a new mix
                If myInsertedBlocks.Count > 0 Then

                    For Each bl As DatabaseServices.ObjectId In myInsertedBlocks

                        Dim myBlock2Erase As DatabaseServices.BlockReference = tr.GetObject(bl, DatabaseServices.OpenMode.ForWrite)

                        myBlock2Erase.Erase()

                        myInsertedBlocks = New DatabaseServices.ObjectIdCollection
                    Next

                End If

                myBT = myDWG.Database.BlockTableId.GetObject(DatabaseServices.OpenMode.ForWrite)

                myBTR = myBT(DatabaseServices.BlockTableRecord.ModelSpace).GetObject(DatabaseServices.OpenMode.ForWrite)

                For Each Id As DatabaseServices.ObjectId In IdsPos

                    Dim myBlockrefPos As DatabaseServices.BlockReference = tr.GetObject(Id, DatabaseServices.OpenMode.ForRead)
                    Dim myRnd As Integer = CInt(Int((idarrayIn.GetUpperBound(0) + 1) * Rnd()))
                    Dim id4insert As DatabaseServices.ObjectId = idarrayIn(myRnd)
                    Dim myBlock2Insert As DatabaseServices.BlockReference = tr.GetObject(id4insert, DatabaseServices.OpenMode.ForRead)
                    Dim myNewBlock As New DatabaseServices.BlockReference(myBlockrefPos.Position, myBlock2Insert.BlockTableRecord)
                    Dim myxRnd, myyRnd, myzRnd, myRoRnd, myScRnd As Double

                    If XFuzzy > 0 Then
                        myxRnd = ((XFuzzy * 2 + 1) * Rnd()) - XFuzzy

                    End If

                    If YFuzzy > 0 Then
                        myyRnd = ((YFuzzy * 2 + 1) * Rnd()) - YFuzzy

                    End If

                    If ZFuzzy > 0 Then
                        myzRnd = ((ZFuzzy * 2 + 1) * Rnd()) - ZFuzzy

                    End If

                    Dim Displacement As New Geometry.Vector3d(myxRnd, myyRnd, myzRnd)

                    myNewBlock.Position = myNewBlock.Position.TransformBy(Geometry.Matrix3d.Displacement(Displacement))

                    If AFuzzy > 0 Then
                        myRoRnd = ((AFuzzy * 2 + 1) * Rnd()) - AFuzzy

                    End If

                    myNewBlock.Rotation = myBlockrefPos.Rotation + (Math.PI / 180) * myRoRnd


                    If SFuzzy <> 0 Then
                        myScRnd = ((2 * SFuzzy)) * Rnd() + (1 - SFuzzy)

                    Else
                        myScRnd = 1

                    End If

                    myNewBlock.Color = myBlock2Insert.Color
                    myNewBlock.Layer = myBlock2Insert.Layer
                    myNewBlock.Linetype = myBlock2Insert.Linetype
                    myNewBlock.LinetypeScale = myBlock2Insert.LinetypeScale
                    myNewBlock.LineWeight = myBlock2Insert.LineWeight
                    myNewBlock.Transparency = myBlock2Insert.Transparency
                    myNewBlock.ScaleFactors = myBlock2Insert.ScaleFactors.MultiplyBy(myScRnd)

                    myInsertedBlocks.Add(myBTR.AppendEntity(myNewBlock))

                    tr.AddNewlyCreatedDBObject(myNewBlock, True)
                    tr.TransactionManager.QueueForGraphicsFlush()

                Next

                tr.Commit()

            End Using

        Catch ex As System.Exception

            myEd.WriteMessage("an error occured: " & ex.ToString)

        End Try

    End Sub

End Class
