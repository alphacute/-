Sub ExportText()
  Dim oPres As Presentation
  Dim oSlides As Slides
  Dim oSld As Slide         'Slide Object
  Dim oShp As Shape         'Shape Object
  Dim iFile As Integer      'File handle for output
  iFile = FreeFile          'Get a free file number
  Dim PathSep As String
  Dim FileNum As Integer
  Dim sTempString As String

  #If Mac Then
    PathSep = ":"
  #Else
    PathSep = "\"
  #End If

  Set oPres = ActivePresentation
  Set oSlides = oPres.Slides

  FileNum = FreeFile

  'Open output file
  ' NOTE:  errors here if file hasn't been saved
  Open oPres.Path & PathSep & "AllText.txt" For Output As FileNum
  
  num_slides = ActivePresentation.Slides.Count

  For i = 1 To num_slides
    Set oSld = ActivePresentation.Slides(i)
    Print #iFile, "Slide:" & vbTab & CStr(oSld.SlideNumber)

    For Each oShp In oSld.Shapes
      'Check to see if shape has a text frame and text
      If oShp.HasTextFrame And oShp.TextFrame.HasText Then
        If oShp.Type = msoPlaceholder Then
            Select Case oShp.PlaceholderFormat.Type
                Case Is = ppPlaceholderTitle, ppPlaceholderCenterTitle
                    Print #iFile, "标题：" & vbTab & oShp.TextFrame.TextRange
                Case Is = ppPlaceholderBody
                    Print #iFile, "正文：" & vbTab & oShp.TextFrame.TextRange
                Case Is = ppPlaceholderSubtitle
                    Print #iFile, "副标题：" & vbTab & oShp.TextFrame.TextRange
                Case Else
                    Print #iFile, "其他占位符：" & vbTab & oShp.TextFrame.TextRange
            End Select
        Else
            Print #iFile, vbTab & oShp.TextFrame.TextRange
        End If  ' msoPlaceholder
      Else  ' it doesn't have a textframe - it might be a group that contains text so:
        If oShp.Type = msoGroup Then
            sTempString = TextFromGroupShape(oShp)
            If Len(sTempString) > 0 Then
                Print #iFile, sTempString
            End If
        End If
      End If    ' Has text frame/Has text
    Next oShp
    
    Print #iFile, vbCrLf
    Next i
  Close #iFile
End Sub

Function TextFromGroupShape(oSh As Shape) As String
' Returns the text from the shapes in a group
' and recursively, text within shapes within groups within groups etc.

    Dim oGpSh As Shape
    Dim sTempText As String

    If oSh.Type = msoGroup Then
        For Each oGpSh In oSh.GroupItems
            With oGpSh
                If .Type = msoGroup Then
                    sTempText = sTempText & TextFromGroupShape(oGpSh)
                Else
                    If .HasTextFrame Then
                        If .TextFrame.HasText Then
                            sTempText = sTempText & "(Gp:) " & .TextFrame.TextRange.Text & vbCrLf
                        End If
                    End If
                End If
            End With
        Next
    End If

    TextFromGroupShape = sTempText

NormalExit:
    Exit Function

Errorhandler:
    Resume Next

End Function
