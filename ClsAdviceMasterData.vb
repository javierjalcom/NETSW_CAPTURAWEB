Public Class ClsAdviceMasterData

    Public iint_AdviceId As Integer
    Public istr_BookingId As String
    Public istr_Vessel As String
    Public istr_portText As String
    Public istr_portId As String
    Public istr_ETA_Date As String
    Public istr_CustomerTxt As String
    Public iint_CustomerId As Integer
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

    'campo boleano de misma mercancia para todo embargue
    Public intblnIsUniqueMerchType As Integer

    Public intInvoceToIdSO As Integer
    Public intInvoceToSOType As Integer
    Public intInvoceToIdDiscrp As Integer
    Public intInvoceToDiscType As Integer
    Public intInvoceToIdStorage As Integer
    Public intInvoceToStoraType As Integer

    'arreglo de productos 
    Public iobjs_ContProductList() As ClsAdviceContainerProduct ' productos de contenedores 
End Class
