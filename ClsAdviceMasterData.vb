Public Class ClsAdviceMasterData

    Public iint_AdviceId As Integer
    Public istr_BookingId As String
    Public istr_Vessel As String
    Public istr_portText As String
    Public istr_portId As String
    Public istr_ETA_Date As String
    Public iint_BrokerId As Integer
    Public istr_product As String
    Public iint_ShippingLineId As Integer
    Public astr_userId As String
    Public istr_serviceTipe As String
    Public istr_CountryId As String
    Public istr_CountryTxt As String
    Public iint_VesselId As Integer
    Public istr_ShippingLineTxt As String
    Public iint_ProductId As Integer
    Public iint_IMOCodeId As Integer
    Public iint_UNCodeId As Integer
    Public istr_AdviceComms As String
    ' Public ilng_VesselVoyageId As Long
    Public istr_ExpoId As String
    Public iobjs_ContainerList() As ClsAdviceDetailDataBooking ' CONTENEDORES
    Public iobjs_IMOList() As ClsIMOAdvice 'IMOS extra


    Public intInvoceToIdSO As Integer
    Public intInvoceToSOType As Integer
    Public intInvoceToIdDiscrp As Integer
    Public intInvoceToDiscType As Integer
    Public intInvoceToIdStorage As Integer
    Public intInvoceToStoraType As Integer

    Public intVesselVoyageId As Integer
    Public strVesselVoyageExpoIdentifier As String

    Public strCompanyInvoce As String
    Public strCompanyDiscrepancy As String
    Public strCompanyStorage As String

    Public istrKeyToken As String

    Public intRequiredByType As Integer
    Public intRequiredBy As Integer

    Public intExporterEntityId As Integer
    Public intExporterType As Integer

    Public intConsigAgencyId As Integer

    Public strCompanyExporter As String

    'arreglo de productos 
    Public iobjs_ContProductList() As ClsAdviceContainerProduct ' productos de contenedores 
End Class
