Public Class ClsReceptionData


    Public ilng_VisitId As Long
    Public ilng_CarrierId As Long
    Public ilng_Customer As Long
    Public ilng_RequiredBy As Long
    Public iint_RequiredByType As Integer
    Public ilng_serviceOrder As Long
    Public istr_Chofer As String
    Public istr_Plates As String
    Public istr_Reference As String
    Public istr_DriverLicence As String
    Public istr_UserName As String
    Public istr_appointmentDate As String
    Public istr_AppointmetBlockId As String
    Public istr_service As String
    '
    Public iint_booking As Integer
    Public istr_STOCKBoking As String
    Public iobjs_VContainers() As ClsVisitContainer
End Class
