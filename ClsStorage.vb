Public Class ClsStorage
    Public intContainerStorageFeeId As Long
    Public str_ServiceType As String
    Public str_ContainerId As String
    Public intContainerUniversalId As Long
    Public intGeneralCargoUniversalId As Long
    Public intGCInventoryItemId As Long
    Public dtmFiscalPetitionDate As Date
    Public strBLName As String
    Public strFiscalPetitionName As String
    Public decFiscalPetitionWeight As Decimal
    Public LLENO As Integer
    Public TRAFICO As String
    Public ESTADIA As String
    Public RESTDAYS As String
    Public TIPOVIGENCIA As String
    Public HASDOCKING As Integer
    Public HASPBIP As Integer
    Public int_RequiredBy As Integer
    Public int_RequiredByType As Integer
    Public int_Invoiceid As Integer
    Public int_InvoiceTypeId As Integer
    Public FEESTATUS As String
    Public DOCKINGSTATUS As String
    Public PBIPSTATUS As String
    Public strFiscalMovement As String
    Public intQuantityExcepDays As Integer
    Public intUsedExcepDays As Integer

    Public strProductName As String
    Public strGCInvItemNumbers As String
    Public strGCInvItemMarks As String
    Public decGCInvItemWeight As String

    Public iobj_FiscalObj As ClsFiscalData

    Public strContainerCargoType As String

    Public strSaveStatus As String

End Class
