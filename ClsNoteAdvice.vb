Public Class ClsNoteAdvice


    Public iint_NoteItem As Integer  ' numeero de item
    Public istr_strContainerId As String  ' numero contenedor
    Public istr_Text As String  ' texto de la nota
    Public int_NoteType As Integer  ' tipo de la nota
    Public str_Status As String  ' tipo de la nota
    Public int_Active As Integer ' activo
    Public int_Checked As Integer ' marcado
    Public str_AditionalComs As String ' comentarios adicionales 
    Public strNoteCreatedBy As String ' rechazado por
    Public dtmNoteCreationStamp As Date ' fecha rechazo
    Public strNoteCheckedBy As String  ' marcado por 
    Public dtmNoteCheckedStamp As Date ' fecha marca
    Public iint_operation As Integer 'tipo de operacion  -- 1 guardar, 2 actualizar , 3 borrar , 4 leer , 


End Class
