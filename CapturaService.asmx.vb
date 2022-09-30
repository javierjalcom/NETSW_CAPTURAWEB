Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel
Imports System
Imports System.Collections
Imports System.Data
Imports System.Data.Odbc
Imports System.Data.OleDb
Imports System.Security.Cryptography
Imports System.Linq
Imports System.Web
Imports System.Xml.Linq
Imports System.IO
Imports System.Text
Imports System.Math
Imports System.Threading
Imports System.Configuration


' Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la siguiente línea.
' <System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://jalcom.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class CapturaService
    Inherits System.Web.Services.WebService

    <WebMethod()>
    Public Function HelloWorld() As String
        Return "Hola a todos"
    End Function



    <WebMethod()>
    Public Function ObtenerError(ByVal cad As String, ByVal ex As Integer) As String
        If ((cad.Contains(ex.ToString()) = True) And (cad.Contains("Sybase Provider]") = True)) Then
            Dim idx = cad.LastIndexOf("]")
            idx = idx + 1
            If idx > 0 And idx <= cad.Length Then
                Return cad.Substring(idx)
            Else
                Return ""
            End If
        Else
            If cad.Contains("SSybase Provider]") = True Then
                Dim idx
                idx = cad.LastIndexOf("]")
                idx = idx + 1

                If idx > 0 And idx <= cad.Length Then
                    Return cad.Substring(idx)
                Else
                    Return ""
                End If

            End If
        End If

        Return ""
    End Function


    'Esta funcion retorna una tabla con un mensaje de error
    Public Function dt_RetrieveErrorTable(ByVal astr_Message As String) As DataTable

        Dim ldt_ErrorTable As DataTable
        Dim lrw_Error As DataRow

        ldt_ErrorTable = New DataTable("ErrorTable")
        ldt_ErrorTable.Columns.Add("Error", GetType(String))
        lrw_Error = ldt_ErrorTable.NewRow()

        lrw_Error("Error") = astr_Message
        ldt_ErrorTable.Rows.Add(lrw_Error)
        Return ldt_ErrorTable

    End Function

    Public Function of_HasOnlyDigits(ByRef astr_CheckString As String) As Boolean

        For Each caracter As Char In astr_CheckString
            If IsNumeric(caracter) = False Then
                Return False
            End If
        Next

        Return True

    End Function


    <WebMethod()>
    Public Function SearchCarrieLike(ByVal astr_CarrierFind As String) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("Carriers")
        strSQL = "spGetCarrierLike"

        iolecmd_comand.Parameters.Add("Carrier", OleDbType.Char)
        iolecmd_comand.Parameters("Carrier").Value = astr_CarrierFind

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandTimeout = 99999
        iolecmd_comand.CommandTimeout = of_getMaxTimeout()

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function Get_CarrierAllList() As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT intCarrierLineId," &
                 "  strCarrierLineIdentifier as 'Clave'," &
                 " strCarrierLineName as 'Nombre Transportista' , " &
                 " strCarrierLineDescription " &
                 " FROM tblclsCarrierLine " &
                 "  INNER JOIN tblclsCompany  ON  tblclsCompany.intCompanyId = tblclsCarrierLine.intCompanyId   " &
                 " WHERE blnCarrierLineActive = 1 " &
                 " AND tblclsCompany.blnCompanyActive = 1 "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function SearchContainerImpoWB(ByVal astr_Container As String, ByVal aint_RequiredByType As Integer, ByVal aint_RequiredBy As Integer, ByVal aint_Folio As Integer) As DataTable
        Dim ldtb_Result = New DataTable("result") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        astr_Container = astr_Container.ToUpper()

        If astr_Container.Length >= 4 Then

            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx
            iolecmd_comand = ioleconx_conexion.CreateCommand()

            ldtb_Result = New DataTable("Containers")
            strSQL = "spGetContainerImpoVisitWB"

            iolecmd_comand.Parameters.Add("Container", OleDbType.Char)
            iolecmd_comand.Parameters("Container").Value = astr_Container

            iolecmd_comand.Parameters.Add("intRequiredByType", OleDbType.Integer)
            iolecmd_comand.Parameters("intRequiredByType").Value = aint_RequiredByType

            iolecmd_comand.Parameters.Add("intRequiredBy", OleDbType.Integer)
            iolecmd_comand.Parameters("intRequiredBy").Value = aint_RequiredBy

            iolecmd_comand.Parameters.Add("aint_Folio", OleDbType.Integer)
            iolecmd_comand.Parameters("aint_Folio").Value = aint_Folio


            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_Result)
            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
            Finally
                ioleconx_conexion.Close()
            End Try

            '    Return ldtb_Result
            'Else
            '    Return ldtb_Result

        End If

        'Try
        '    If ldtb_Result.Rows.Count = 0 Then
        '        Return dt_RetrieveErrorTable("No se encontro el contenedor")
        '    End If
        'Catch ex As Exception

        'End Try

        Return ldtb_Result

    End Function


    <WebMethod()>
    Public Function SearchCustomerInvoiceLike(ByVal astr_CustomerFind As String) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("Carriers")
        strSQL = "spGetCustomerLike"

        iolecmd_comand.Parameters.Add("Customer", OleDbType.Char)
        iolecmd_comand.Parameters("Customer").Value = astr_CustomerFind


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function SearchCustomerInvoice_ById(ByVal aint_Customer As Integer) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("Carriers")

        strSQL = " SELECT TOP 1  tblclsCompanyEntity.intCompanyEntityId, " &
                 "               tblclsCompany.intCompanyId, " &
                 "               tblclsCustomer.intCustomerId as 'ID'," &
                 " 				 tblclsCustomer.strCustomerIdentifier   as 'Clave', " &
                 "               tblclsCompany.strCompanyName as 'Nombre',	" &
                 "	             ISNULL(tblclsCompany.strCompanyAddress1,'') AS 'Direccion' ," &
                 "               ISNULL(tblclsCompany.strCompanyCity,'') AS 'Ciudad' , " &
                 "               ISNULL(tblclsCompany.strCompanyState,'') as 'Estado' , " &
                 "	             tblclsCompany.strCompanyZipCode as 'Codigo Postal' , " &
                 "				 tblclsCompany.strCompanyFiscalIdentifier AS 'RFC' , " &
                 "               tblclsCompany.strCompanyName , " &
                 "	             ISNULL(tblclsCompany.strCompanyAddress1,'') AS 'strCompanyAddress' , " &
                 "               ISNULL(tblclsCompany.strCompanyCity,'') AS 'strCompanyCity' ,  " &
                 "               ISNULL(tblclsCompany.strCompanyState,'') AS 'strCompanyState', " &
                 "               ISNULL(tblclsCompany.strCompanyCountry,'') AS 'strCompanyCountry',  " &
                 "	             ISNULL(tblclsCompany.strCompanyZipCode ,'') AS 'strCompanyZipCode'," &
                 "	             ISNULL(tblclsCompany.strCompanyFiscalIdentifier,'') AS 'strCompanyFiscalIdentifier',	" &
                 "               ISNULL(tblclsCompany.intPaymentFormTypeId  ,0) AS  'intPaymentFormTypeId' , " &
                 "               ISNULL(tblclsCompany.strCFDIUsageTypeId ,'') AS 'strCFDIUsageTypeId', " &
                 "               ISNULL(tblclsCompany.strPaymentMethodTypeId ,'') AS 'strPaymentMethodTypeId' " &
                 "        FROM  tblclsCompany  " &
                 "			INNER JOIN  tblclsCompanyEntity           ON  tblclsCompany.intCompanyId = tblclsCompanyEntity.intCompanyId " &
                 "          INNER JOIN  tblclsCustomer      ON  tblclsCustomer.intCustomerId = tblclsCompanyEntity.intCompanyEntityId " &
                 "         INNER JOIN  tblclsCustomerType  ON  tblclsCompanyEntity.intCustomerTypeId = tblclsCustomerType.intCustomerTypeId" &
                 "                WHERE tblclsCustomerType.strCustomerTypeIdentifier = 'CUSTOMER' " &
                 "                           AND   tblclsCustomer.blnCustomerActive = 1" &
                 " AND   tblclsCustomer.intCustomerId =  " + aint_Customer.ToString()


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.Text
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function SearchCustomerInvoice_ById_All(ByVal aint_Customer As Integer) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("Carriers")

        strSQL = " SELECT TOP 1  tblclsCompanyEntity.intCompanyEntityId, " &
                 "               tblclsCompany.intCompanyId, " &
                 "               tblclsCustomer.intCustomerId as 'ID'," &
                 " 				 tblclsCustomer.strCustomerIdentifier   as 'Clave', " &
                 "               tblclsCompany.strCompanyName as 'Nombre',	" &
                 "	             ISNULL(tblclsCompany.strCompanyAddress1,'') AS 'Direccion' ," &
                 "               ISNULL(tblclsCompany.strCompanyCity,'') AS 'Ciudad' , " &
                 "               ISNULL(tblclsCompany.strCompanyState,'') as 'Estado' , " &
                 "	             tblclsCompany.strCompanyZipCode as 'Codigo Postal' , " &
                 "				 tblclsCompany.strCompanyFiscalIdentifier AS 'RFC' , " &
                 "               tblclsCompany.strCompanyName , " &
                 "	             ISNULL(tblclsCompany.strCompanyAddress1,'') AS 'strCompanyAddress' , " &
                 "               ISNULL(tblclsCompany.strCompanyCity,'') AS 'strCompanyCity' ,  " &
                 "               ISNULL(tblclsCompany.strCompanyState,'') AS 'strCompanyState', " &
                 "               ISNULL(tblclsCompany.strCompanyCountry,'') AS 'strCompanyCountry',  " &
                 "	             ISNULL(tblclsCompany.strCompanyZipCode ,'') AS 'strCompanyZipCode'," &
                 "	             ISNULL(tblclsCompany.strCompanyFiscalIdentifier,'') AS 'strCompanyFiscalIdentifier',	" &
                 "               ISNULL(tblclsCompany.intPaymentFormTypeId  ,0) AS  'intPaymentFormTypeId' , " &
                 "               ISNULL(tblclsCompany.strCFDIUsageTypeId ,'') AS 'strCFDIUsageTypeId', " &
                 "               ISNULL(tblclsCompany.strPaymentMethodTypeId ,'') AS 'strPaymentMethodTypeId' " &
                 "        FROM  tblclsCompany  " &
                 "			INNER JOIN  tblclsCompanyEntity           ON  tblclsCompany.intCompanyId = tblclsCompanyEntity.intCompanyId " &
                 "          INNER JOIN  tblclsCustomer      ON  tblclsCustomer.intCustomerId = tblclsCompanyEntity.intCompanyEntityId " &
                 "         INNER JOIN  tblclsCustomerType  ON  tblclsCompanyEntity.intCustomerTypeId = tblclsCustomerType.intCustomerTypeId" &
                 "                WHERE tblclsCustomerType.strCustomerTypeIdentifier = 'CUSTOMER' " &
                 " AND   tblclsCustomer.intCustomerId =  " + aint_Customer.ToString()


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.Text
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    <WebMethod()>
    Public Function Get_Customer_AllList() As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT tblclsCompanyEntity.intCompanyEntityId, " &
                 "        tblclsCompany.intCompanyId, " &
                 "        tblclsCustomer.intCustomerId as 'ID'," &
                 "        tblclsCustomer.strCustomerIdentifier   as 'Clave'," &
                 "        tblclsCompany.strCompanyName as 'Nombre' " &
                 "        , tblclsCompany.strCompanyAddress1 + ','+ tblclsCompany.strCompanyCity +',' + tblclsCompany.strCompanyState as 'Direccion' " &
                 "        , tblclsCompany.strCompanyZipCode as 'Codigo Postal'" &
                 "        , tblclsCompany.strCompanyFiscalIdentifier AS 'RFC' " &
                 "  FROM   tblclsCompany " &
                 "  INNER JOIN  tblclsCompanyEntity           ON  tblclsCompany.intCompanyId = tblclsCompanyEntity.intCompanyId " &
                 "  INNER JOIN  tblclsCustomer      ON  tblclsCustomer.intCompanyId = tblclsCompanyEntity.intCompanyId " &
                 "       AND tblclsCustomer.intCustomerId = tblclsCompanyEntity.intCompanyEntityId " &
                 "  INNER JOIN  tblclsCustomerType  ON  tblclsCompanyEntity.intCustomerTypeId = tblclsCustomerType.intCustomerTypeId " &
                 "   WHERE tblclsCustomerType.strCustomerTypeIdentifier = 'CUSTOMER' " &
                 "  AND   tblclsCompany.blnCompanyActive = 1" &
                 "  AND   tblclsCustomer.blnCustomerActive = 1  "


        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    <WebMethod()>
    Public Function SearchBrokerRequiredLike(ByVal astr_BrokerFind As String) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("Brokers")
        strSQL = "spGetBrokerLike"

        iolecmd_comand.Parameters.Add("Broker", OleDbType.Char)
        iolecmd_comand.Parameters("Broker").Value = astr_BrokerFind


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function


    <WebMethod()>
    Public Function SaveVisit(ByVal aobj_data As ClsVisitData) As DataTable
        'Public Function SaveVisit(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal alng_Customer As Long, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_serviceOrder As Long, ByVal astr_Chofer As String, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal astr_appointmentDate As String, ByVal adtb_VisitOperation As DataTable) As DataTable
        'Public Function SaveVisit(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal astr_Chofer As String, ByVal alng_Customer As Long, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal adtb_VisitOperation As DataTable, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_serviceOrder As Long, ByVal astr_appointmentDate As String, ByVal aint_ex As Integer) As DataTable
        'Public Function SaveVisit(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal astr_Chofer As String, ByVal alng_Customer As Long, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal adtb_VisitOperation As DataTable, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_serviceOrder As Long, ByVal adtm_appointmentDate As Date) As DataTable

        ''' validar la informacion nuevamente


        Dim ldtb_VisitResult As DataTable
        Dim ldtb_VisitDetailResult As DataTable
        Dim lstr_VisitMasterResult As String
        Dim lint_VisitResult As Long
        Dim lint_operationCounter As Integer
        Dim ldtb_TableVisitServiceOrder As DataTable = New DataTable("visitaret")
        Dim lrow As DataRow
        'Dim adtm_appointmentDate As Date = System.DateTime.Now()
        Dim lstr_appointmentDate As String
        Dim lstr_tempA As String
        Dim lstr_tempx1 As String
        Dim lint_counter As Integer
        Dim lint_idx As Integer


        ldtb_TableVisitServiceOrder.Columns.Add("intVisitId", GetType(Long))
        ldtb_TableVisitServiceOrder.Columns.Add("intServiceOrderId", GetType(Long))
        'ldtb_TableVisitServiceOrder.Columns.Add("extra", GetType(String))

        '''''''''''''''''''''''
        Dim alng_VisitId As Long
        Dim alng_CarrierId As Long
        Dim alng_Customer As Long
        Dim alng_RequiredBy As Long
        Dim aint_RequiredByType As Integer
        Dim alng_serviceOrder As Long
        Dim astr_Chofer As String
        Dim astr_Plates As String
        Dim astr_Reference As String
        Dim astr_DriverLicence As String
        Dim astr_UserName As String
        Dim astr_appointmentDate As String
        Dim adtb_VisitOperation As DataTable = New DataTable("tableop")
        Dim lint_retry As Integer
        Dim lint_limit As Integer
        Dim lstr_appointmentblock As String
        Dim lstr_ContainerMain As String
        Dim lstr_DeliveryType As String

        ldtb_VisitResult = New DataTable("result")
        'copiar los miembros de argumento a variables locales 

        alng_VisitId = aobj_data.ilng_VisitId
        alng_CarrierId = aobj_data.ilng_CarrierId
        alng_Customer = aobj_data.ilng_Customer
        alng_RequiredBy = aobj_data.ilng_RequiredBy
        aint_RequiredByType = aobj_data.iint_RequiredByType
        alng_serviceOrder = aobj_data.ilng_serviceOrder
        astr_Chofer = aobj_data.istr_Chofer
        astr_Plates = aobj_data.istr_Plates
        astr_Reference = aobj_data.istr_Reference
        astr_DriverLicence = aobj_data.istr_DriverLicence
        astr_UserName = aobj_data.istr_UserName
        astr_appointmentDate = aobj_data.istr_appointmentDate
        adtb_VisitOperation = aobj_data.idtb_VisitOperation

        alng_serviceOrder = aobj_data.ilng_serviceOrder
        lstr_appointmentblock = aobj_data.istr_AppointmetBlockId

        lstr_ContainerMain = aobj_data.istr_ContainerId
        lstr_DeliveryType = aobj_data.istr_DeliyveryType

        ''''''''''''''''''''''''''''''''''''

        ' si es visita nueva  
        If alng_CarrierId = 0 And alng_VisitId = 0 Then
            Return dt_RetrieveErrorTable("Se necesita Transportista")
        End If

        'chofer 
        'If astr_Chofer.Length = 0 Then
        'Return dt_RetrieveErrorTable("Se necesita capturar chofer")
        'End If
        'placas 
        'If astr_Plates.Length = 0 Then
        '    Return dt_RetrieveErrorTable("Se necesita capturar placas ")
        'End If

        'facturar
        If alng_Customer = 0 And alng_VisitId = 0 Then
            Return dt_RetrieveErrorTable("Se necesita capturar facturar a ")
        End If

        '' obtener el contenedor de las operaciones 
        lint_operationCounter = 0
        Try
            lint_operationCounter = adtb_VisitOperation.Rows.Count()
        Catch ex As Exception
            lint_operationCounter = 0
        End Try

        ''''
        ''''''''
        '''''''''''
        '''''''''''''''
        ''''''''''''''''''''

        ''''''''''''''''

        '''''''''''''''''''''''''''''''''
        ''''''''''''''''''''''''''''''''''''''

        '' primero coverir a fecha
        Dim adtm_appointmentDate As Date = New Date()
        Dim lstr_datetime As String = ""
        'Dim astr_appointmentDate As String
        lstr_datetime = astr_appointmentDate
        ''
        '' '' converitir a fecha
        'Dim timeFormat As String = "yyyy-MM-dd HH:mm:ss"
        'Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"
        Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"
        ' de string  con inicio dia  a objeto fecha
        Try
            adtm_appointmentDate = Convert.ToDateTime(astr_appointmentDate).ToString(timeFormat)
        Catch ex As Exception

        End Try



        lstr_tempx1 = ""
        ' revisar la fecha de la cita , 
        ' de fecha a string ocn inicio anio
        lstr_appointmentDate = of_ConvertDateToStringGeneralFormat(adtm_appointmentDate)
        ' lstr_appointmentDate = astr_appointmentDate
        '' guardar master 

        lstr_VisitMasterResult = of_SaveMasterVisit(alng_VisitId, alng_CarrierId, astr_Chofer, alng_Customer, astr_Reference, astr_Plates, lint_operationCounter, alng_RequiredBy, aint_RequiredByType, alng_serviceOrder, astr_DriverLicence, astr_UserName, lstr_appointmentDate, lstr_appointmentblock, lstr_ContainerMain, lstr_DeliveryType)
        'lstr_VisitMasterResult = of_SaveMasterVisit(alng_VisitId, alng_CarrierId, astr_Chofer, alng_Customer, astr_Reference, astr_Plates, aint_ServiceType, lint_operationCounter, alng_RequiredBy, aint_RequiredByType, alng_serviceOrder, astr_DriverLicence, astr_UserName, aint_CustomerType)

        If lstr_VisitMasterResult.Length = 0 Then
            Return dt_RetrieveErrorTable("Error 3002 al guardar visita ") ' no hay numero de visita 
        End If
        '''' validar el visita 
        If of_HasOnlyDigits(lstr_VisitMasterResult) = False Then
            Return dt_RetrieveErrorTable(lstr_VisitMasterResult)
        End If

        '' obtener el numero de visita 
        Try
            lint_VisitResult = CType(lstr_VisitMasterResult, Long)
            If alng_VisitId = 0 Then
                alng_VisitId = lint_VisitResult
            End If
        Catch ex As Exception
            Return dt_RetrieveErrorTable("Error 3004 al guardar visita") ' no es un valor numerico
        End Try
        ''''''''''''''''''''''''''''''

        ''  guardar detalle 
        ''''''''''''''''''''''''''''''''''''
        If lint_VisitResult > 0 Then

            'Return dt_RetrieveErrorTable("Error VALORES ALONG=" + alng_VisitId.ToString() + " - VIRESULT=" + lint_VisitResult.ToString()) ' no hay numero

            lint_VisitResult = lint_VisitResult

            'Return dt_RetrieveErrorTable("v=" + lint_VisitResult.ToString())

            '' si es que hay detalle por guardar 
            If adtb_VisitOperation.Rows.Count > 0 Then

                '' guardar detalle 
                Try

                    'ldtb_VisitDetailResult = of_SaveVisitDetail(lint_VisitResult, alng_Customer, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)


                    'ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, aint_CustomerType, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)
                    'ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)

                    ''2019-12-19, habilitar un otro intento 
                    lint_retry = 0

                    '2020, ciclo para hacer 5 intentos 
                    lint_limit = 10
                    lint_counter = 0

                    Do

                        Try
                            ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, astr_Reference, 0, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation, alng_serviceOrder, 40)
                            lint_counter = lint_limit
                        Catch ex As Exception
                            Dim lstr_ex As String
                            Dim lstr_exlow As String
                            lstr_ex = ex.Message
                            lstr_exlow = lstr_ex.ToLower()

                            If lstr_exlow.IndexOf("time") > -1 Or lstr_exlow.IndexOf("tiempo") > -1 Then
                                lint_retry = 1
                            Else
                                Return dt_RetrieveErrorTable(lstr_ex)
                            End If
                        End Try

                        lint_counter = lint_counter + 1

                    Loop While lint_counter < lint_limit



                    '' segundo intento
                    'If lint_retry > 0 Then
                    ' ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, astr_Reference, 0, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation, alng_serviceOrder)
                    ' End If


                    ' analisar la tabla , y si tiene los campos de maniobra y visita asignarla a la tabla temporal de maniobra con visita 
                    If ldtb_VisitDetailResult.Rows.Count > 0 And ldtb_VisitDetailResult.Columns.Count > 1 Then
                        Try
                            ''origen
                            Dim llng_Temp As Long
                            lrow = ldtb_TableVisitServiceOrder.NewRow()
                            llng_Temp = CType(ldtb_VisitDetailResult(0)("intVisitId"), Long)
                            lrow("intVisitId") = llng_Temp
                            llng_Temp = CType(ldtb_VisitDetailResult(0)("intServiceOrderId"), Long)
                            lrow("intServiceOrderId") = llng_Temp

                            ldtb_TableVisitServiceOrder.Rows.Add(lrow)
                            Return ldtb_TableVisitServiceOrder

                        Catch ex As Exception
                            Return dt_RetrieveErrorTable("Error al obtener el numero de maniobra")
                        End Try
                    Else
                        If ldtb_VisitDetailResult.Rows.Count = 1 And ldtb_VisitDetailResult.Rows.Count = 1 Then
                            Return ldtb_VisitDetailResult
                        End If
                    End If

                Catch ex As Exception
                    Dim lstr_ex As String
                    lstr_ex = ex.Message
                    Return dt_RetrieveErrorTable(lstr_ex)
                End Try
                'sino hay registros 
            Else
                '' retornar el numero de visita , sin la solicitud de servicio 
                lrow = ldtb_TableVisitServiceOrder.NewRow()

                lrow("intVisitId") = lint_VisitResult
                lrow("intServiceOrderId") = 0
                ' lrow("extra") = lstr_datetime

                'lrow("extra") = lstr_datetime + ".." + astr_appointmentDate + astr_appointmentDate.ToString.Length.ToString()
                'lrow("extra") = astr_appointmentDate.ToString()
                'lrow("extra") = adtm_appointmentDate.ToString() + "...." + astr_appointmentDate.ToString()
                'lrow("extra") = lstr_appointmentDate + "--" + adtm_appointmentDate.ToString() + "-" + adtm_appointmentDate.Month.ToString() + "-  " + adtm_appointmentDate.Hour.ToString() + ":" + adtm_appointmentDate.Month.ToString() + "...." + astr_appointmentDate.ToString()


                ldtb_TableVisitServiceOrder.Rows.Add(lrow)

                Return ldtb_TableVisitServiceOrder

            End If ''If adtb_VisitOperation.Rows.Count > 0 Then            

        Else ' If lint_VisitResult > 0 Then
            Return dt_RetrieveErrorTable("Error 3005 al guardar encabezado-" + lstr_VisitMasterResult) ' no hay numero
        End If  'If lint_VisitResult > 0 Then


        Return ldtb_VisitResult

    End Function

    ''visit_reception
    <WebMethod()>
    Public Function SaveVisit_Reception(ByVal aobj_data As ClsReceptionData) As DataTable
        'Public Function SaveVisit(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal alng_Customer As Long, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_serviceOrder As Long, ByVal astr_Chofer As String, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal astr_appointmentDate As String, ByVal adtb_VisitOperation As DataTable) As DataTable
        'Public Function SaveVisit(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal astr_Chofer As String, ByVal alng_Customer As Long, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal adtb_VisitOperation As DataTable, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_serviceOrder As Long, ByVal astr_appointmentDate As String, ByVal aint_ex As Integer) As DataTable
        'Public Function SaveVisit(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal astr_Chofer As String, ByVal alng_Customer As Long, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal adtb_VisitOperation As DataTable, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_serviceOrder As Long, ByVal adtm_appointmentDate As Date) As DataTable

        ''' validar la informacion nuevamente


        Dim ldtb_VisitResult As DataTable
        Dim ldtb_VisitDetailResult As DataTable
        Dim lstr_VisitMasterResult As String
        Dim lint_VisitResult As Long
        Dim lint_operationCounter As Integer
        Dim ldtb_TableVisitServiceOrder As DataTable = New DataTable("visitaret")
        Dim lrow As DataRow
        'Dim adtm_appointmentDate As Date = System.DateTime.Now()
        Dim lstr_appointmentDate As String
        Dim lstr_tempA As String
        Dim lstr_tempx1 As String

        Dim lint_limittrys As Integer
        Dim lint_tryscounter As Integer
        lint_limittrys = 20

        ldtb_TableVisitServiceOrder.Columns.Add("intVisitId", GetType(Long))
        ldtb_TableVisitServiceOrder.Columns.Add("intServiceOrderId", GetType(Long))
        'ldtb_TableVisitServiceOrder.Columns.Add("extra", GetType(String))

        '''''''''''''''''''''''
        Dim alng_VisitId As Long
        Dim alng_CarrierId As Long
        Dim alng_Customer As Long
        Dim alng_RequiredBy As Long
        Dim aint_RequiredByType As Integer
        Dim alng_serviceOrder As Long
        Dim astr_Chofer As String
        Dim astr_Plates As String
        Dim astr_Reference As String
        Dim astr_DriverLicence As String
        Dim astr_UserName As String
        Dim astr_appointmentDate As String
        Dim adtb_VisitOperation As DataTable = New DataTable("tableop")
        Dim lint_retry As Integer
        Dim lint_Isnewvisit As Integer

        ldtb_VisitResult = New DataTable("result")
        'copiar los miembros de argumento a variables locales 

        alng_VisitId = aobj_data.ilng_VisitId
        alng_CarrierId = aobj_data.ilng_CarrierId
        alng_Customer = aobj_data.ilng_Customer
        alng_RequiredBy = aobj_data.ilng_RequiredBy
        aint_RequiredByType = aobj_data.iint_RequiredByType
        alng_serviceOrder = aobj_data.ilng_serviceOrder
        astr_Chofer = aobj_data.istr_Chofer
        astr_Plates = aobj_data.istr_Plates
        astr_Reference = aobj_data.istr_Reference
        astr_DriverLicence = aobj_data.istr_DriverLicence
        astr_UserName = aobj_data.istr_UserName
        astr_appointmentDate = aobj_data.istr_appointmentDate
        'adtb_VisitOperation = aobj_data.idtb_VisitOperation

        alng_serviceOrder = aobj_data.ilng_serviceOrder

        'evaluar si es una visita nuea 
        lint_Isnewvisit = -1
        If alng_VisitId = 0 Then
            lint_Isnewvisit = 1
        End If
        ''''''''''''''''''''''''''''''''''''

        ' si es visita nueva  
        If alng_CarrierId = 0 And alng_VisitId = 0 Then
            Return dt_RetrieveErrorTable("Se necesita Transportista")
        End If

        'chofer 
        'If astr_Chofer.Length = 0 Then
        'Return dt_RetrieveErrorTable("Se necesita capturar chofer")
        'End If
        'placas 
        'If astr_Plates.Length = 0 Then
        '    Return dt_RetrieveErrorTable("Se necesita capturar placas ")
        'End If

        'facturar
        If alng_Customer = 0 And alng_VisitId = 0 Then
            Return dt_RetrieveErrorTable("Se necesita capturar facturar a ")
        End If

        '' obtener el contenedor de las operaciones 
        lint_operationCounter = 0
        Try
            lint_operationCounter = aobj_data.iobjs_VContainers.Length
        Catch ex As Exception
            lint_operationCounter = 0
        End Try

        ''''
        ''''''''
        '''''''''''
        '''''''''''''''
        ''''''''''''''''''''

        ''''''''''''''''

        '''''''''''''''''''''''''''''''''
        ''''''''''''''''''''''''''''''''''''''

        '' primero coverir a fecha
        Dim adtm_appointmentDate As Date = New Date()
        Dim lstr_datetime As String = ""
        'Dim astr_appointmentDate As String
        lstr_datetime = astr_appointmentDate
        ''
        '' '' converitir a fecha
        'Dim timeFormat As String = "yyyy-MM-dd HH:mm:ss"
        'Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"
        Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"
        ' de string  con inicio dia  a objeto fecha
        Try
            adtm_appointmentDate = Convert.ToDateTime(astr_appointmentDate).ToString(timeFormat)
        Catch ex As Exception

        End Try



        lstr_tempx1 = ""
        ' revisar la fecha de la cita , 
        ' de fecha a string ocn inicio anio
        lstr_appointmentDate = of_ConvertDateToStringGeneralFormat(adtm_appointmentDate)

        '' guardar master 
        '' si no hay visita 
        If aobj_data.ilng_VisitId = 0 Or alng_VisitId > 0 Then
            lint_tryscounter = 0

            Do While lint_tryscounter < lint_limittrys

                lstr_VisitMasterResult = of_SaveMasterVisit(alng_VisitId, alng_CarrierId, astr_Chofer, alng_Customer, astr_Reference, astr_Plates, lint_operationCounter, alng_RequiredBy, aint_RequiredByType, alng_serviceOrder, astr_DriverLicence, astr_UserName, lstr_appointmentDate, "", "", "")


                lstr_tempA = lstr_VisitMasterResult.ToLower()
                If lstr_tempA.IndexOf("tiempo") >= 0 Or lstr_tempA.IndexOf("time") >= 0 Then
                    lint_tryscounter = lint_tryscounter + 1
                Else
                    lint_tryscounter = lint_limittrys
                End If

            Loop

            'lstr_VisitMasterResult = of_SaveMasterVisit(alng_VisitId, alng_CarrierId, astr_Chofer, alng_Customer, astr_Reference, astr_Plates, lint_operationCounter, alng_RequiredBy, aint_RequiredByType, alng_serviceOrder, astr_DriverLicence, astr_UserName, lstr_appointmentDate)


            If lstr_VisitMasterResult.Length = 0 Then
                Return dt_RetrieveErrorTable("Error 3002 al guardar visita ") ' no hay numero de visita 
            End If
            '''' validar el visita 
            If of_HasOnlyDigits(lstr_VisitMasterResult) = False Then
                Return dt_RetrieveErrorTable(lstr_VisitMasterResult)
            End If

            '' obtener el numero de visita 
            alng_VisitId = 0
            Try
                alng_VisitId = CType(lstr_VisitMasterResult, Long)
                If alng_VisitId = 0 Then
                    lint_VisitResult = alng_VisitId

                    ' Return dt_RetrieveErrorTable("v=" + alng_VisitId.ToString())

                End If
                aobj_data.ilng_VisitId = alng_VisitId
                lint_VisitResult = alng_VisitId
            Catch ex As Exception

                Return dt_RetrieveErrorTable("Error 3004 al guardar visita") ' no es un valor numerico
                'Return dt_RetrieveErrorTable("Error master =" + ex.Message) ' no es un valor numerico
            End Try
            ''''''''''''''''''''''''''''''

            '' si hay visita  ???
            If aobj_data.ilng_VisitId = 0 Then
                Return dt_RetrieveErrorTable("Falta numero visita")
            End If

        End If 'If aobj_data.ilng_VisitId = 0 Then


        ''  guardar detalle 
        ''''''''''''''''''''''''''''''''''''
        If lint_VisitResult > 0 Then

            'Return dt_RetrieveErrorTable("Error VALORES ALONG=" + alng_VisitId.ToString() + " - VIRESULT=" + lint_VisitResult.ToString()) ' no hay numero

            lint_VisitResult = lint_VisitResult

            'Return dt_RetrieveErrorTable("v=" + lint_VisitResult.ToString())

            '' si es que hay detalle por guardar 
            ' Return dt_RetrieveErrorTable(aobj_data.iobjs_VContainers.Length.ToString())
            If aobj_data.iobjs_VContainers.Length > 0 Then

                '' guardar detalle 
                Try

                    'ldtb_VisitDetailResult = of_SaveVisitDetail(lint_VisitResult, alng_Customer, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)


                    'ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, aint_CustomerType, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)
                    'ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)

                    ''2019-12-19, habilitar un otro intento 
                    lint_retry = 0
                    lint_tryscounter = 0

                    Do While lint_tryscounter < lint_limittrys

                        lint_retry = 0

                        Try
                            'ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, astr_Reference, 0, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation, alng_serviceOrder)
                            ldtb_VisitDetailResult = of_SaveVisitDetailSingle_REC(aobj_data)
                        Catch ex As Exception
                            Dim lstr_ex As String
                            Dim lstr_exlow As String
                            lstr_ex = ex.Message
                            lstr_exlow = lstr_ex.ToLower()

                            If lstr_exlow.IndexOf("time") > -1 Or lstr_exlow.IndexOf("tiempo") > -1 Then
                                lint_retry = 1
                                lint_tryscounter = lint_tryscounter + 1
                            Else
                                Return dt_RetrieveErrorTable(lstr_ex)
                            End If
                        End Try

                        '' segundo intento
                        ''If lint_retry > 0 Then
                        ''ldtb_VisitDetailResult = of_SaveVisitDetailSingle_REC(aobj_data)
                        ''End If

                        If lint_retry = 0 Then
                            lint_tryscounter = lint_limittrys
                        End If
                    Loop



                    'Return dt_RetrieveErrorTable("cout=" + ldtb_VisitDetailResult.Rows.Count().ToString())

                    ' analisar la tabla , y si tiene los campos de maniobra y visita asignarla a la tabla temporal de maniobra con visita 
                    If ldtb_VisitDetailResult.Rows.Count > 0 And ldtb_VisitDetailResult.Columns.Count > 1 Then
                        Try
                            ''origen
                            Dim llng_Temp As Long
                            For Each litem As DataRow In ldtb_VisitDetailResult.Rows

                                lrow = ldtb_TableVisitServiceOrder.NewRow()
                                llng_Temp = CType(litem("VisitId"), Long)
                                lrow("intVisitId") = llng_Temp
                                llng_Temp = CType(litem("ServiceOrderId"), Long)
                                lrow("intServiceOrderId") = llng_Temp

                                ldtb_TableVisitServiceOrder.Rows.Add(lrow)

                            Next

                            'antes de retornar la visita ,ver si se manda llamar al metodo que obtiene ubicaciones
                            If lint_Isnewvisit = 1 And alng_VisitId > 0 Then
                                SetYardPositiontoVisit(alng_VisitId, astr_UserName)
                            End If

                            Return ldtb_TableVisitServiceOrder

                        Catch ex As Exception
                            'Return dt_RetrieveErrorTable("Error al obtener el numero de maniobra")
                            Return dt_RetrieveErrorTable(ex.Message)
                        End Try
                    Else
                        If ldtb_VisitDetailResult.Rows.Count = 1 And ldtb_VisitDetailResult.Rows.Count = 1 Then
                            Return ldtb_VisitDetailResult
                        End If
                    End If



                Catch ex As Exception
                    Dim lstr_ex As String
                    lstr_ex = ex.Message
                    Return dt_RetrieveErrorTable(lstr_ex)
                End Try
                'sino hay registros 
            Else
                '' retornar el numero de visita , sin la solicitud de servicio 
                lrow = ldtb_TableVisitServiceOrder.NewRow()

                lrow("intVisitId") = lint_VisitResult
                lrow("intServiceOrderId") = 0
                ' lrow("extra") = lstr_datetime

                'lrow("extra") = lstr_datetime + ".." + astr_appointmentDate + astr_appointmentDate.ToString.Length.ToString()
                'lrow("extra") = astr_appointmentDate.ToString()
                'lrow("extra") = adtm_appointmentDate.ToString() + "...." + astr_appointmentDate.ToString()
                'lrow("extra") = lstr_appointmentDate + "--" + adtm_appointmentDate.ToString() + "-" + adtm_appointmentDate.Month.ToString() + "-  " + adtm_appointmentDate.Hour.ToString() + ":" + adtm_appointmentDate.Month.ToString() + "...." + astr_appointmentDate.ToString()


                ldtb_TableVisitServiceOrder.Rows.Add(lrow)

                Return ldtb_TableVisitServiceOrder

            End If ''If adtb_VisitOperation.Rows.Count > 0 Then            

        Else ' If lint_VisitResult > 0 Then
            Return dt_RetrieveErrorTable("Error 3005 al guardar encabezado---" + lstr_VisitMasterResult) ' no hay numero
        End If  'If lint_VisitResult > 0 Then


        ldtb_VisitResult.TableName = "resulttable"



        Return ldtb_VisitResult

    End Function
    ''' end visit reception
    <WebMethod()>
    Public Function UpdateVisitDriver(ByVal alng_Visit As Long, ByVal aint_CarrierId As Integer, ByVal astr_Chofer As String, ByVal astr_plates As String, ByVal astr_licence As String, ByVal astr_user As String, ByVal adtm_appointdate As Date, ByVal astr_BlockIdentifier As String, ByVal astr_ContainerId As String, ByVal astr_DeliyveryType As String) As String

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow

        Dim lstr_appointmentDate As String


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultVisit"

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1

        ' conversion de la cadena 
        astr_Chofer = of_convertoasccistring(astr_Chofer)

        ''convertir fecha 
        lstr_appointmentDate = of_ConvertDateToStringGeneralFormat(adtm_appointdate)
        ''''''''''''''''''''

        '''''''''fin revisar la fecha

        'limpiar cadena sql
        lstr_SQL = ""

        ''''---

        'agregar parametros
        iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCarrierId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strChofer", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strPlate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strReference", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intOpCounter", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@requiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@requierbyType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@serviceOrderId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strDriverLicence", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strApointmentDate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppointBlock", OleDbType.Char)

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strDeliveryType", OleDbType.Char)


        '' agregar valores
        iolecmd_comand.Parameters("@intVisitId").Value = alng_Visit
        iolecmd_comand.Parameters("@intCarrierId").Value = aint_CarrierId
        iolecmd_comand.Parameters("@intCustomerId").Value = 0
        'iolecmd_comand.Parameters("@intCustomerType").Value = aintCustomerType
        iolecmd_comand.Parameters("@strChofer").Value = astr_Chofer
        iolecmd_comand.Parameters("@strPlate").Value = astr_plates
        iolecmd_comand.Parameters("@strReference").Value = ""
        iolecmd_comand.Parameters("@intOpCounter").Value = 0
        iolecmd_comand.Parameters("@requiredBy").Value = 0
        iolecmd_comand.Parameters("@requierbyType").Value = 0
        iolecmd_comand.Parameters("@serviceOrderId").Value = 0
        iolecmd_comand.Parameters("@strDriverLicence").Value = astr_licence
        iolecmd_comand.Parameters("@strUsername").Value = astr_user

        iolecmd_comand.Parameters("@strApointmentDate").Value = lstr_appointmentDate
        iolecmd_comand.Parameters("@strAppointBlock").Value = astr_BlockIdentifier


        iolecmd_comand.Parameters("@strContainerId").Value = astr_ContainerId
        iolecmd_comand.Parameters("@strDeliveryType").Value = astr_DeliyveryType

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 

        '"exec spAddEIRSeal " & dt.Rows(i)("intEIRId").ToString() & ", '" & strSeal & "', 0, '" & lstrusername & "'"
        'definir la cadena sql
        lstr_SQL = "spSaveVisitMasterWb"
        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            adapter.Fill(ldt_ReturnValueTable)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_ReturnValueTable.Rows.Count = 1 And ldt_ReturnValueTable.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_ReturnValueTable(0)(0).ToString
                If lstr_info.Length > 0 Then

                    If of_HasOnlyDigits(lstr_info) = False Then
                        Return lstr_info
                    Else
                        Return ""
                    End If
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return "error al actualizar informacion "
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''

    End Function

    'Public Function of_SaveMasterVisit(ByVal alng_Visit As Long, ByVal alng_Carrier As Long, ByVal astr_chofer As String, ByVal alng_Customer As Long, ByVal astr_Reference As String, ByVal astr_Plates As String, ByVal aint_operation As Integer, ByVal aint_operationCounter As Long, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Long, ByVal alng_ServiceOrder As Long, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal aintCustomerType As Integer) As String
    '    Public Function of_SaveMasterVisit(ByVal alng_Visit As Long, ByVal alng_Carrier As Long, ByVal astr_chofer As String, ByVal alng_Customer As Long, ByVal astr_Reference As String, ByVal astr_Plates As String, ByVal aint_operation As Integer, ByVal aint_operationCounter As Long, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Long, ByVal alng_ServiceOrder As Long, ByVal astr_DriverLicence As String, ByVal astr_UserName As String) As String
    '<WebMethod()> _
    Public Function of_SaveMasterVisit(ByVal alng_Visit As Long, ByVal alng_Carrier As Long, ByVal astr_chofer As String, ByVal alng_Customer As Long, ByVal astr_Reference As String, ByVal astr_Plates As String, ByVal aint_operationCounter As Long, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Long, ByVal alng_ServiceOrder As Long, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal astr_appointmentDate As String, ByVal astr_apointmentblock As String, ByVal astr_ContainerId As String, ByVal astr_DeliyveryType As String) As String

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_VisitResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultVisit"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        'Return alng_Visit

        ''' validaciones ---> ????

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""

        ''' conversion de la cadena de chofer a string 
        astr_chofer = of_convertoasccistring(astr_chofer)

        ''''--- 

        'agregar parametros
        iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCarrierId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        ''''''iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strChofer", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strPlate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strReference", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intOpCounter", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@requiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@requierbyType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@serviceOrderId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strDriverLicence", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strApointmentDate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppointBlock", OleDbType.Char)

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strDeliveryType", OleDbType.Char)



        ' agregar valores
        iolecmd_comand.Parameters("@intVisitId").Value = alng_Visit
        iolecmd_comand.Parameters("@intCarrierId").Value = alng_Carrier
        iolecmd_comand.Parameters("@intCustomerId").Value = alng_Customer
        '''''''''iolecmd_comand.Parameters("@intCustomerType").Value = aintCustomerType
        iolecmd_comand.Parameters("@strChofer").Value = astr_chofer
        iolecmd_comand.Parameters("@strPlate").Value = astr_Plates
        iolecmd_comand.Parameters("@strReference").Value = astr_Reference
        iolecmd_comand.Parameters("@intOpCounter").Value = aint_operationCounter
        iolecmd_comand.Parameters("@requiredBy").Value = alng_RequiredBy
        iolecmd_comand.Parameters("@requierbyType").Value = aint_RequiredByType
        iolecmd_comand.Parameters("@serviceOrderId").Value = alng_ServiceOrder
        iolecmd_comand.Parameters("@strDriverLicence").Value = astr_DriverLicence
        iolecmd_comand.Parameters("@strUsername").Value = astr_UserName

        iolecmd_comand.Parameters("@strApointmentDate").Value = astr_appointmentDate
        iolecmd_comand.Parameters("@strAppointBlock").Value = astr_apointmentblock

        iolecmd_comand.Parameters("@strContainerId").Value = astr_ContainerId
        iolecmd_comand.Parameters("@strDeliveryType").Value = astr_DeliyveryType

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 

        '''''''''''''''''''"exec spAddEIRSeal " & dt.Rows(i)("intEIRId").ToString() & ", '" & strSeal & "', 0, '" & lstrusername & "'"

        'definir la cadena sql
        lstr_SQL = "spSaveVisitMasterWb"


        ''''''' lstr_SQL = "execute spSaveVisitMasterWb  @intVisitId=" + alng_Visit.ToString() + ", @intCarrierId=" + alng_Carrier.ToString() + ", @intCustomerId=" + alng_Customer.ToString() + ", @strChofer='" + astr_chofer + "', @strPlate='" + astr_Plates + "' , @strReference='" + astr_Reference + "' , @intOpCounter=" + aint_operationCounter.ToString() + " , @intRequiredBy=" + alng_RequiredBy.ToString() + ", @intRequireByType=" + aint_RequiredByType.ToString() + ", @intServiceOrderId=" + alng_ServiceOrder.ToString() + ", @strDriverLicence='" + astr_DriverLicence + "', @strUsername='" + astr_UserName + "', @strApointmentDate='" + astr_appointmentDate + "'"




        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text


        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return "error al actualizar informacion "
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''
        Return ""
    End Function


    <WebMethod()>
    Public Function SaveAdvice(ByVal aobj_Advice As ClsAdviceMasterData, ByVal astr_userId As String) As List(Of ClsAdviceResult)
        Dim lstr_result As String
        Dim lint_BookingAdvice As Integer
        Dim ldtb_Table As DataTable = New DataTable("result")
        Dim ldtb_TableResult As DataTable = New DataTable("resultB")
        Dim lrow As DataRow
        Dim lobj_master As ClsAdviceResult = New ClsAdviceResult()
        Dim lobj_list() As ClsAdviceResult
        Dim llist_toReturn As List(Of ClsAdviceResult) = New List(Of ClsAdviceResult)
        Dim llist_tempresutl As List(Of ClsAdviceResult)
        Dim lobj_resultitem As ClsAdviceResult

        'agregar la columna 
        ldtb_Table.Columns.Add("intBookingAdviceId", GetType(Integer))


        '' si el numero de visita es 0, crear el master 

        lstr_result = ""

        '' primera validacion 
        ' If aobj_Advice.iint_AdviceId = 0 Then

        ''''------ revisar la conversion de la fehca 

        Dim adtm_ETADate As Date = New Date()
        Dim lstr_datetime As String = ""
        'Dim astr_appointmentDate As String
        lstr_datetime = adtm_ETADate
        ''
        '' '' converitir a fecha        
        Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"
        Dim lstr_ETAdate As String
        ' de string  con inicio dia  a objeto fecha
        Try
            adtm_ETADate = Convert.ToDateTime(aobj_Advice.istr_ETA_Date).ToString(timeFormat)
        Catch ex As Exception

        End Try
        ' de fecha a string 

        lstr_ETAdate = of_ConvertDateToStringGeneralFormat(adtm_ETADate)

        '''''''--------------------------------------------------

        lstr_result = ""
        lstr_result = of_SaveMasterAdvice(aobj_Advice.iint_AdviceId, aobj_Advice.istr_BookingId, aobj_Advice.istr_Vessel, aobj_Advice.istr_ExpoId, aobj_Advice.iint_VesselId, 0, aobj_Advice.istr_portText, aobj_Advice.istr_portId, aobj_Advice.istr_CountryTxt, aobj_Advice.istr_CountryId, lstr_ETAdate, aobj_Advice.istr_CustomerTxt, aobj_Advice.iint_CustomerId, aobj_Advice.iint_BrokerId, aobj_Advice.istr_ShippingLineTxt, aobj_Advice.iint_ShippingLineId, aobj_Advice.istr_product, aobj_Advice.iint_ProductId, aobj_Advice.iint_IMOCodeId, aobj_Advice.iint_UNCodeId, aobj_Advice.istr_serviceTipe, aobj_Advice.istr_AdviceComms, astr_userId, aobj_Advice.intblnIsUniqueMerchType)

        '' guardar master 

        If lstr_result.Length = 0 Then
            '   Return dt_RetrieveErrorTable("Error 3002 al guardar aviso ") ' no hay numero de visita 
            ReDim lobj_list(0)
            lobj_list(0) = New ClsAdviceResult()
            lobj_list(0).iint_AdviceId = 0
            lobj_list(0).iint_Succes = 0
            lobj_list(0).istr_Container = ""
            lobj_list(0).istr_Message = "Error 3002 al guardar aviso "
            'Return lobj_list

            lobj_resultitem = New ClsAdviceResult()
            lobj_resultitem.iint_AdviceId = 0
            lobj_resultitem.iint_Succes = 0
            lobj_resultitem.istr_Container = ""
            lobj_resultitem.istr_Message = "Error 3002 al guardar aviso "
            llist_toReturn.Add(lobj_resultitem)

            Return llist_toReturn

        End If
        '''' validar el visita 
        If of_HasOnlyDigits(lstr_result) = False Then
            ReDim lobj_list(0)
            lobj_list(0) = New ClsAdviceResult()
            lobj_list(0).iint_AdviceId = 0
            lobj_list(0).iint_Succes = 0
            lobj_list(0).istr_Container = ""
            lobj_list(0).istr_Message = lstr_result

            'Return lobj_list

            lobj_resultitem = New ClsAdviceResult()
            lobj_resultitem.iint_AdviceId = 0
            lobj_resultitem.iint_Succes = 0
            lobj_resultitem.istr_Container = ""
            lobj_resultitem.istr_Message = lstr_result
            llist_toReturn.Add(lobj_resultitem)

            Return llist_toReturn

        End If

        '' obtener el numero de aviso 
        Try
            lint_BookingAdvice = CType(lstr_result, Long)
            If lint_BookingAdvice = 0 Then
                lint_BookingAdvice = 0
            End If
        Catch ex As Exception
            '' Return dt_RetrieveErrorTable("Error" + lstr_result) ' no es un valor numerico
            ReDim lobj_list(0)
            lobj_list(0) = New ClsAdviceResult()
            lobj_list(0).iint_AdviceId = 0
            lobj_list(0).iint_Succes = 0
            lobj_list(0).istr_Message = "Error" + lstr_result
            'Return lobj_list

            lobj_resultitem = New ClsAdviceResult()
            lobj_resultitem.iint_AdviceId = 0
            lobj_resultitem.iint_Succes = 0
            lobj_resultitem.istr_Container = ""
            lobj_resultitem.istr_Message = "Error" + lstr_result
            llist_toReturn.Add(lobj_resultitem)
            Return llist_toReturn

        End Try

        'crear tabla y retornar 
        If lint_BookingAdvice > 0 Then
            aobj_Advice.iint_AdviceId = lint_BookingAdvice
            lrow = ldtb_Table.NewRow()
            lrow("intBookingAdviceId") = lint_BookingAdvice
            ldtb_Table.Rows.Add(lrow)
            '    Return ldtb_Table
            'Else
            '    Return dt_RetrieveErrorTable("RESULT=" + lstr_result)
        End If

        ' End If 'primea validacion 


        '' segunda validacion 
        If aobj_Advice.iint_AdviceId = 0 Then

            ' si el valor de retorno 
            If lstr_result.Length > 0 Then
                '    Return dt_RetrieveErrorTable("Error" + lstr_result)
                ReDim lobj_list(0)
                lobj_list(0) = New ClsAdviceResult()
                lobj_list(0).iint_AdviceId = 0
                lobj_list(0).iint_Succes = 0
                lobj_list(0).istr_Container = ""
                lobj_list(0).istr_Message = "Error" + lstr_result
                'Return lobj_list

                lobj_resultitem = New ClsAdviceResult()
                lobj_resultitem.iint_AdviceId = 0
                lobj_resultitem.iint_Succes = 0
                lobj_resultitem.istr_Container = ""
                lobj_resultitem.istr_Message = "Error" + lstr_result
                llist_toReturn.Add(lobj_resultitem)
                Return llist_toReturn

            Else
                'Return dt_RetrieveErrorTable("no se obtuvo id de aviso")
                ReDim lobj_list(0)
                lobj_list(0) = New ClsAdviceResult()
                lobj_list(0).iint_AdviceId = 0
                lobj_list(0).iint_Succes = 0
                lobj_list(0).istr_Container = ""
                lobj_list(0).istr_Message = "no se obtuvo id de aviso"
                'Return lobj_list

                lobj_resultitem = New ClsAdviceResult()
                lobj_resultitem.iint_AdviceId = 0
                lobj_resultitem.iint_Succes = 0
                lobj_resultitem.istr_Container = ""
                lobj_resultitem.istr_Message = "no se obtuvo id de aviso"
                llist_toReturn.Add(lobj_resultitem)

            End If


        End If ' segunda validacion 

        'ver los imos adicionesles del master 
        If aobj_Advice.iobjs_IMOList IsNot Nothing Then
            If aobj_Advice.iobjs_IMOList.Count > 0 Then
                For Each imoelement As ClsIMOAdvice In aobj_Advice.iobjs_IMOList
                    lstr_result = of_SaveDelGetIMOAdvice(aobj_Advice.iint_AdviceId, imoelement.iint_IMOItem, imoelement.iint_IMOCode, imoelement.iint_UNCode, imoelement.iint_operation, astr_userId)
                Next
            End If
        End If


        'si hay detalles por autorizar 
        If aobj_Advice.iobjs_ContainerList IsNot Nothing Then

            If aobj_Advice.iobjs_ContainerList.Count > 0 Then

                'ldtb_TableResult = of_saveDetailAdvice(aobj_Advice.iint_AdviceId, aobj_Advice.iobjs_ContainerList, astr_userId)
                llist_tempresutl = of_saveDetailAdvice(aobj_Advice.iint_AdviceId, aobj_Advice.iobjs_ContainerList, astr_userId)

                'If ldtb_TableResult.Rows.Count = 1 And ldtb_TableResult.Columns.Count = 1 Then
                'Return dt_RetrieveErrorTable(ldtb_TableResult(0)(0).ToString)
                'End If

                'Return dt_RetrieveErrorTable("Tablaxvv " + ldtb_TableResult.Rows.Count.ToString() + "-" + ldtb_TableResult.Columns.Count.ToString())
                'Return dt_RetrieveErrorTable("Tablaxvv")

                'Return ldtb_TableResult
                Return llist_tempresutl
                'Return dt_RetrieveErrorTable("Tablaxvv " + ldtb_TableResult(0)(0).ToString())
                'si no hay 
            Else

                If aobj_Advice.iint_AdviceId > 0 Then
                    lrow = ldtb_Table.NewRow()
                    lrow("intBookingAdviceId") = lint_BookingAdvice
                    ldtb_Table.Rows.Add(lrow)
                    '   Return ldtb_Table
                    ' Return lobj_list
                    lobj_resultitem = New ClsAdviceResult()
                    lobj_resultitem.iint_AdviceId = lint_BookingAdvice
                    lobj_resultitem.iint_Succes = 1
                    lobj_resultitem.istr_Container = ""
                    lobj_resultitem.istr_Message = ""
                    llist_toReturn.Add(lobj_resultitem)
                    Return llist_toReturn

                End If

            End If 'If aobj_Advice.iobjs_ContainerList.Count > 0 Then

        Else ' If aobj_Advice.iobjs_ContainerList Is Nothing Then

            If aobj_Advice.iint_AdviceId > 0 Then
                lrow = ldtb_Table.NewRow()
                lrow("intBookingAdviceId") = lint_BookingAdvice
                ldtb_Table.Rows.Add(lrow)

                ReDim lobj_list(0)
                lobj_list(0) = New ClsAdviceResult()
                lobj_list(0).iint_AdviceId = lint_BookingAdvice
                lobj_list(0).iint_Succes = 1
                lobj_list(0).istr_Container = ""
                lobj_list(0).istr_Message = ""
                ' Return ldtb_Table
                'Return lobj_list

                lobj_resultitem = New ClsAdviceResult()
                lobj_resultitem.iint_AdviceId = lint_BookingAdvice
                lobj_resultitem.iint_Succes = 1
                lobj_resultitem.istr_Container = ""
                lobj_resultitem.istr_Message = ""
                llist_toReturn.Add(lobj_resultitem)
                Return llist_toReturn
            End If

        End If  ' If aobj_Advice.iobjs_ContainerList Is Nothing Then



        'Return dt_RetrieveErrorTable("Tablaxvv")
        'Return New DataTable("vacia")
        Return llist_toReturn

    End Function

    ' Public Function ValidateMasterAdvice(ByVal aobj_Advice As ClsAdviceMasterData, ByVal astr_userId As String) As String
    <WebMethod()>
    Public Function ValidateMasterAdvice(ByVal aint_BookingAdviceId As Integer, ByVal astr_BookingId As String, ByVal aint_VesselId As Integer, ByVal alng_VesselVoyageId As Long, ByVal astr_PortId As String, ByVal astr_ETAtDate As String, ByVal aint_CustomerId As Integer, ByVal aint_CustomBrokerId As Integer, ByVal aint_ShippingLine As Integer, ByVal aint_ProductId As Integer, ByVal aint_IMOCode As Integer, ByVal aint_UNCode As Integer, ByVal astr_ServiceType As String, ByVal astr_AdviceComs As String, ByVal astr_User As String) As String

        Dim lstr_result As String
        Dim lint_BookingAdvice As Integer

        Dim lrow As DataRow


        lstr_result = ""

        '' primera validacion 
        ' If aobj_Advice.iint_AdviceId = 0 Then

        ''''------ revisar la conversion de la fehca 

        Dim adtm_ETADate As Date = New Date()
        Dim lstr_datetime As String = ""
        'Dim astr_appointmentDate As String
        lstr_datetime = adtm_ETADate
        ''
        '' '' converitir a fecha        
        Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"
        Dim lstr_ETAdate As String
        ' de string  con inicio dia  a objeto fecha
        Try
            adtm_ETADate = Convert.ToDateTime(astr_ETAtDate).ToString(timeFormat)
        Catch ex As Exception

        End Try
        ' de fecha a string 

        lstr_ETAdate = of_ConvertDateToStringGeneralFormat(adtm_ETADate)

        If aint_BookingAdviceId = 0 Then
            Return "Error:Falta ID aviso"
        End If

        '''''''--------------------------------------------------

        lstr_result = ""
        lstr_result = of_ValidateMasterAdvice(aint_BookingAdviceId, astr_BookingId, aint_VesselId, alng_VesselVoyageId, astr_PortId, lstr_ETAdate, aint_CustomerId, aint_CustomBrokerId, aint_ShippingLine, aint_ProductId, aint_IMOCode, aint_UNCode, astr_ServiceType, astr_AdviceComs, astr_User)

        'lstr_result = of_SaveMasterAdvice(aobj_Advice.iint_AdviceId, aobj_Advice.istr_BookingId, aobj_Advice.istr_Vessel, aobj_Advice.istr_ExpoId, aobj_Advice.iint_VesselId, 0, aobj_Advice.istr_portText, aobj_Advice.istr_portId, aobj_Advice.istr_CountryTxt, aobj_Advice.istr_CountryId, lstr_ETAdate, aobj_Advice.istr_CustomerTxt, aobj_Advice.iint_CustomerId, aobj_Advice.iint_BrokerId, aobj_Advice.istr_ShippingLineTxt, aobj_Advice.iint_ShippingLineId, aobj_Advice.istr_portText, aobj_Advice.iint_ProductId, aobj_Advice.iint_IMOCodeId, aobj_Advice.iint_UNCodeId, aobj_Advice.istr_serviceTipe, aobj_Advice.istr_AdviceComms, astr_userId)

        '' guardar master 

        'Return dt_RetrieveErrorTable("Tablaxvv")
        Return lstr_result

    End Function

    <WebMethod()>
    Public Function UpdateMasterAdviceV(ByVal aint_BookingAdviceId As Integer, ByVal astr_BookingId As String, ByVal aint_VesselId As Integer, ByVal alng_VesselVoyageId As Long, ByVal astr_PortId As String, ByVal astr_ETAtDate As String, ByVal aint_CustomerId As Integer, ByVal aint_CustomBrokerId As Integer, ByVal aint_ShippingLine As Integer, ByVal aint_ProductId As Integer, ByVal aint_IMOCode As Integer, ByVal aint_UNCode As Integer, ByVal astr_ServiceType As String, ByVal astr_AdviceComs As String, ByVal astr_ValidateStatus As String, ByVal astr_User As String, ByVal aintblnIsUniqueMerchType As Integer) As String

        Dim lstr_result As String
        Dim lint_BookingAdvice As Integer
        Dim lstr_ValidateMasterDev As String
        Dim lrow As DataRow


        lstr_result = ""

        '' primera validacion 
        ' If aobj_Advice.iint_AdviceId = 0 Then

        ''''------ revisar la conversion de la fehca 

        Dim adtm_ETADate As Date = New Date()
        Dim lstr_datetime As String = ""
        'Dim astr_appointmentDate As String
        lstr_datetime = adtm_ETADate
        ''
        '' '' converitir a fecha        
        Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"
        Dim lstr_ETAdate As String
        ' de string  con inicio dia  a objeto fecha
        Try
            adtm_ETADate = Convert.ToDateTime(astr_ETAtDate).ToString(timeFormat)
        Catch ex As Exception

        End Try
        ' de fecha a string 

        lstr_ETAdate = of_ConvertDateToStringGeneralFormat(adtm_ETADate)

        If aint_BookingAdviceId = 0 Then
            Return "Error:Falta ID aviso"
        End If

        '''''''--------------------------------------------------
        'lstr_ValidateMasterDev = "PENDVAL"

        '' si la validacion es 1  VALID
        'If aint_ValidateMasterDev = 1 Then
        '    lstr_ValidateMasterDev = "VALID"
        'End If

        '' si la validacion es 0  REJECT
        'If aint_ValidateMasterDev = 0 Then
        '    lstr_ValidateMasterDev = "REJECT"
        'End If

        'validar que si el estado es distinto de VALID O REJECT ,ponerlo en PENDVAL
        If astr_ValidateStatus <> "VALID" And astr_ValidateStatus <> "REJECT" Then
            astr_ValidateStatus = "PENDVAL"
        End If

        lstr_result = ""
        lstr_result = of_UpdateMasterAdviceV(aint_BookingAdviceId, astr_BookingId, aint_VesselId, alng_VesselVoyageId, astr_PortId, lstr_ETAdate, aint_CustomerId, aint_CustomBrokerId, aint_ShippingLine, aint_ProductId, aint_IMOCode, aint_UNCode, astr_ServiceType, astr_AdviceComs, astr_ValidateStatus, astr_User, aintblnIsUniqueMerchType)

        'lstr_result = of_ValidateMasterAdvice(aint_BookingAdviceId, astr_BookingId, aint_VesselId, alng_VesselVoyageId, astr_PortId, lstr_ETAdate, aint_CustomerId, aint_CustomBrokerId, aint_ShippingLine, aint_ProductId, aint_IMOCode, aint_UNCode, astr_ServiceType, astr_AdviceComs, astr_User)

        'lstr_result = of_SaveMasterAdvice(aobj_Advice.iint_AdviceId, aobj_Advice.istr_BookingId, aobj_Advice.istr_Vessel, aobj_Advice.istr_ExpoId, aobj_Advice.iint_VesselId, 0, aobj_Advice.istr_portText, aobj_Advice.istr_portId, aobj_Advice.istr_CountryTxt, aobj_Advice.istr_CountryId, lstr_ETAdate, aobj_Advice.istr_CustomerTxt, aobj_Advice.iint_CustomerId, aobj_Advice.iint_BrokerId, aobj_Advice.istr_ShippingLineTxt, aobj_Advice.iint_ShippingLineId, aobj_Advice.istr_portText, aobj_Advice.iint_ProductId, aobj_Advice.iint_IMOCodeId, aobj_Advice.iint_UNCodeId, aobj_Advice.istr_serviceTipe, aobj_Advice.istr_AdviceComms, astr_userId)

        '' guardar master 

        'Return dt_RetrieveErrorTable("Tablaxvv")
        Return lstr_result

    End Function

    '<WebMethod()> _
    Public Function of_SaveMasterAdvice(ByVal aint_BookingAdviceId As Integer, ByVal astr_BookingId As String, ByVal astr_VesselName As String, ByVal astr_VoyageExpoId As String, ByVal aint_VesselId As String, ByVal alng_VesselVoyageId As Long, ByVal astr_PortText As String, ByVal astr_PortId As String, ByVal astr_CountryTxt As String, ByVal astr_CountryId As String, ByVal astr_ETAtDate As String, ByVal astr_CustomerTxt As String, ByVal aint_CustomerId As Integer, ByVal aint_CustomBrokerId As Integer, ByVal astr_ShippingLinetxt As String, ByVal aint_ShippingLine As Integer, ByVal astr_ProductText As String, ByVal aint_ProductId As Integer, ByVal aint_IMOCode As Integer, ByVal aint_UNCode As Integer, ByVal astr_ServiceType As String, ByVal astr_AdviceComs As String, ByVal astr_User As String, ByVal aint_BlnIsSingleMerchaType As Integer) As String


        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)

        iolecmd_comand.Parameters.Add("@strBookingId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVoyageExpoId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intVesselVoyageId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strPortText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strPortId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strETAtDate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCustomerTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomBrokerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strShippingLinetxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strProductText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intIMOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUNCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strServiceType", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAdviceComms", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidBooking", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUser", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intblnIsUniqueMerchType", OleDbType.Integer)

        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdviceId
        iolecmd_comand.Parameters("@strBookingId").Value = astr_BookingId
        iolecmd_comand.Parameters("@strVesselName").Value = astr_VesselName
        iolecmd_comand.Parameters("@strVoyageExpoId").Value = astr_VoyageExpoId
        iolecmd_comand.Parameters("@intVesselId").Value = aint_VesselId
        iolecmd_comand.Parameters("@intVesselVoyageId").Value = alng_VesselVoyageId
        iolecmd_comand.Parameters("@strPortText").Value = astr_PortText
        iolecmd_comand.Parameters("@strPortId").Value = astr_PortId
        iolecmd_comand.Parameters("@strCountryTxt").Value = astr_CountryTxt
        iolecmd_comand.Parameters("@strCountryId").Value = astr_CountryId
        iolecmd_comand.Parameters("@strETAtDate").Value = astr_ETAtDate
        iolecmd_comand.Parameters("@strCustomerTxt").Value = astr_CustomerTxt
        iolecmd_comand.Parameters("@intCustomerId").Value = aint_CustomerId
        iolecmd_comand.Parameters("@intCustomBrokerId").Value = aint_CustomBrokerId
        iolecmd_comand.Parameters("@strShippingLinetxt").Value = astr_ShippingLinetxt
        iolecmd_comand.Parameters("@intShippingLine").Value = aint_ShippingLine
        iolecmd_comand.Parameters("@strProductText").Value = astr_ProductText
        iolecmd_comand.Parameters("@intProductId").Value = aint_ProductId
        iolecmd_comand.Parameters("@intIMOCode").Value = aint_IMOCode
        iolecmd_comand.Parameters("@intUNCode").Value = aint_UNCode
        iolecmd_comand.Parameters("@strAdviceComms").Value = astr_AdviceComs
        iolecmd_comand.Parameters("@strServiceType").Value = astr_ServiceType

        iolecmd_comand.Parameters("@blnIsValidBooking").Value = "PENDVAL"  '-1
        iolecmd_comand.Parameters("@blnIsValidByShipper").Value = -1
        iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1

        iolecmd_comand.Parameters("@strUser").Value = astr_User

        iolecmd_comand.Parameters("@intblnIsUniqueMerchType").Value = aint_BlnIsSingleMerchaType

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveMasterBookingAdv"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return "error al actualizar informacion "
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''
        Return ""
    End Function

    ' Public Function ValidateMasterAdvice(ByVal aobj_Advice As ClsAdviceMasterData, ByVal astr_userId As String) As String
    <WebMethod()>
    Public Function ValidateContainerAdvice(ByVal aint_AdviceBooking As Integer, ByVal astr_container As String, ByVal aint_ContainerType As Integer, ByVal aint_ContainerSize As Integer, ByVal aint_ContainerISOode As Integer, ByVal aint_ShippingLine As Integer, ByVal adec_VGM As Decimal, ByVal aint_Full As Integer, ByVal astr_user As String) As String

        Dim lstr_result As String
        Dim lint_BookingAdvice As Integer

        Dim lrow As DataRow
        Dim ldtb_table_result As DataTable = New DataTable("result")

        lstr_result = ""

        '' primera validacion 
        ' If aobj_Advice.iint_AdviceId = 0 Then

        ''''------ revisar la conversion de la fehca 




        '''''''--------------------------------------------------

        lstr_result = ""
        ldtb_table_result = of_ValidateContainer(aint_AdviceBooking, astr_container, aint_ContainerType, aint_ContainerSize, aint_ContainerISOode, aint_ShippingLine, adec_VGM, aint_Full, astr_user)

        'lstr_result = of_ValidateMasterAdvice(aint_BookingAdviceId, astr_BookingId, aint_VesselId, alng_VesselVoyageId, astr_PortId, lstr_ETAdate, aint_CustomerId, aint_CustomBrokerId, aint_ShippingLine, aint_ProductId, aint_IMOCode, aint_UNCode, astr_ServiceType, astr_AdviceComs, astr_User)
        '' analizar el resultado 
        Try

            If ldtb_table_result.Rows.Count > 0 Then
                ''
                If ldtb_table_result.Columns.Count = 2 Then
                    Return ldtb_table_result(0)(0).ToString()
                End If

                If ldtb_table_result.Columns.Count = 1 Then
                    Return "0"
                End If
            Else
                Return ""

            End If


        Catch ex As Exception
            Return "0"
        End Try


        '' guardar master 

        'Return dt_RetrieveErrorTable("Tablaxvv")
        Return lstr_result

    End Function

    Public Function of_saveDetailAdvice(ByVal aint_AdviceBooking As Integer, ByVal alistobj_container As ClsAdviceDetailDataBooking(), ByVal astr_user As String) As List(Of ClsAdviceResult) ' As ClsAdviceResult() ' As DataTable


        Dim ldt_AdviceBookingResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_resultdt As DataTable = New DataTable("dresult")
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnAdviceBRow As DataRow

        Dim larg_lint_AdviceBooking As Integer
        Dim larg_lstr_ContainerId As String
        Dim larg_lint_ContainerType As String
        Dim larg_lint_ContainerSize As String
        Dim larg_lint_ContainerISOode As Integer
        Dim larg_lint_ShippingLine As Integer
        Dim larg_ldec_VGM As Decimal
        Dim larg_lint_Full As Integer
        Dim larg_lint_Operation As Integer
        Dim larg_lstr_weigherId As String
        Dim larg_lstr_comments As String

        '' crear tabla de resultados 
        Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
        Dim lrow_Result As DataRow
        Dim lstr_serviceType As String
        Dim larg_lstr_SealNumber As String
        Dim larg_ldec_NETWeight As Decimal
        Dim larg_lstrISOCodeText As String
        Dim lint_counterSuccess As Integer
        'Dim lstr_message As String
        Dim lint_error As Integer
        Dim lobj_resultstruct(alistobj_container.Length - 1) As ClsAdviceResult
        Dim llist_return As List(Of ClsAdviceResult) = New List(Of ClsAdviceResult)
        Dim lobj_ResultITem As ClsAdviceResult

        ldtb_ResultData.TableName = "DataResult"
        'ldtb_ResultData.Columns.Add("intBookingAdviceId", GetType(Integer))
        'ldtb_ResultData.Columns.Add("strContainerId", GetType(String))
        ldtb_ResultData.Columns.Add("Mensaje", GetType(String))


        ''''''''''''''''''''''''''''''''''''''''''''''''
        ''' primer bloque 


        'Dim llng_ServiceOrderId As Long

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        '' validaciones --->
        '' numero de aviso
        If aint_AdviceBooking = 0 Then
            dt_RetrieveErrorTable("No existe numero de aviso ")
        End If


        '' tabla
        Try
            'si es nulo el listado
            If alistobj_container Is Nothing Then

                ''Return dt_RetrieveErrorTable(aint_AdviceBooking)

                'ReDim lobj_resultstruct(0)
                'lobj_resultstruct(0) = New ClsAdviceResult()
                'lobj_resultstruct(0).iint_AdviceId = aint_AdviceBooking
                'lobj_resultstruct(0).iint_Succes = 1
                'lobj_resultstruct(0).istr_Container = ""
                'lobj_resultstruct(0).istr_Message = ""
                'Return lobj_resultstruct

                lobj_ResultITem = New ClsAdviceResult()
                lobj_ResultITem.iint_AdviceId = aint_AdviceBooking
                lobj_ResultITem.iint_Succes = 1
                lobj_ResultITem.istr_Container = ""
                lobj_ResultITem.istr_Message = ""
                llist_return.Add(lobj_ResultITem)
                Return llist_return
            End If

            '' si el listado , no tiene items 
            If alistobj_container.Count = 0 Then
                'Return dt_RetrieveErrorTable(aint_AdviceBooking)
                ' Return lobj_resultstruct
                Return New List(Of ClsAdviceResult)

            End If

        Catch ex As Exception
            'Return dt_RetrieveErrorTable(aint_AdviceBooking)
            'ReDim lobj_resultstruct(0)
            'lobj_resultstruct(0) = New ClsAdviceResult()
            'lobj_resultstruct(0).iint_AdviceId = aint_AdviceBooking
            'lobj_resultstruct(0).iint_Succes = 1
            'lobj_resultstruct(0).istr_Container = ""
            'lobj_resultstruct(0).istr_Message = ""
            'Return lobj_resultstruct

            lobj_ResultITem = New ClsAdviceResult()
            lobj_ResultITem.iint_AdviceId = aint_AdviceBooking
            lobj_ResultITem.iint_Succes = 1
            lobj_ResultITem.istr_Container = ""
            lobj_ResultITem.istr_Message = ""
            llist_return.Add(lobj_ResultITem)
            Return llist_return



        End Try


        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerISOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@decVGM", OleDbType.Decimal)

        iolecmd_comand.Parameters.Add("@blnContainerIsFull", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsValidItem", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strSealNumber", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@decWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@strWeigherId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strISOCodeText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strComments", OleDbType.Char)


        'If aorigint_visit > 0 And alng_Visit > 0 Then
        '    Return dt_RetrieveErrorTable("-bloque CUAT ")
        'End If

        lint_counterSuccess = 0
        ''crear 
        Dim adapter As OleDbDataAdapter
        Dim lint_counter As Integer = 0
        Dim lint_idx As Integer = 0
        Dim lint_result As Integer = 0

        'Return dt_RetrieveErrorTable("antes de ciclo adv det")

        For lint_idx = 0 To alistobj_container.Count - 1

            Try

                larg_lint_AdviceBooking = aint_AdviceBooking
                larg_lstr_ContainerId = alistobj_container(lint_idx).istr_Container
                larg_lint_ContainerSize = alistobj_container(lint_idx).iint_containerSize
                larg_lint_ContainerType = alistobj_container(lint_idx).iint_ContainerType
                larg_lint_ContainerISOode = alistobj_container(lint_idx).iint_ContainerISOCode
                larg_ldec_VGM = alistobj_container(lint_idx).idec_VGM
                larg_lint_Full = alistobj_container(lint_idx).iint_IsFull
                larg_lint_ShippingLine = alistobj_container(lint_idx).iint_ShippingLine
                larg_lint_Operation = alistobj_container(lint_idx).iint_OperationType
                larg_lstr_SealNumber = alistobj_container(lint_idx).istr_SealNumber
                larg_ldec_NETWeight = alistobj_container(lint_idx).idec_NETWeight
                larg_lstr_weigherId = alistobj_container(lint_idx).istr_WeigherId
                larg_lstrISOCodeText = alistobj_container(lint_idx).istr_ISOCodeText
                larg_lstr_comments = alistobj_container(lint_idx).istr_Comments

                lobj_resultstruct(lint_idx) = New ClsAdviceResult

                lobj_resultstruct(lint_idx).iint_AdviceId = aint_AdviceBooking
                lobj_resultstruct(lint_idx).istr_Container = alistobj_container(lint_idx).istr_Container
                lobj_resultstruct(lint_idx).istr_Message = ""

                lobj_ResultITem = New ClsAdviceResult

                lobj_ResultITem.iint_AdviceId = aint_AdviceBooking
                lobj_ResultITem.istr_Container = alistobj_container(lint_idx).istr_Container
                lobj_ResultITem.istr_Message = ""

                iolecmd_comand.Parameters("@intBookingAdviceId").Value = larg_lint_AdviceBooking
                iolecmd_comand.Parameters("@strContainerId").Value = larg_lstr_ContainerId
                iolecmd_comand.Parameters("@intContainerSize").Value = larg_lint_ContainerSize
                iolecmd_comand.Parameters("@intContainerType").Value = larg_lint_ContainerType
                iolecmd_comand.Parameters("@intContainerISOCode").Value = larg_lint_ContainerISOode
                iolecmd_comand.Parameters("@intShippingLine").Value = larg_lint_ShippingLine
                iolecmd_comand.Parameters("@decVGM").Value = larg_ldec_VGM

                iolecmd_comand.Parameters("@blnContainerIsFull").Value = larg_lint_Full
                iolecmd_comand.Parameters("@intOperation").Value = larg_lint_Operation
                iolecmd_comand.Parameters("@blnIsValidItem").Value = "PENDVAL" '-1
                iolecmd_comand.Parameters("@blIsValidByShipper").Value = -1
                iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1
                iolecmd_comand.Parameters("@strUsername").Value = astr_user
                iolecmd_comand.Parameters("@strSealNumber").Value = larg_lstr_SealNumber
                iolecmd_comand.Parameters("@decWeight").Value = larg_ldec_NETWeight
                iolecmd_comand.Parameters("@strWeigherId").Value = larg_lstr_weigherId
                iolecmd_comand.Parameters("@strISOCodeText").Value = larg_lstrISOCodeText
                iolecmd_comand.Parameters("@strComments").Value = larg_lstr_comments
                '''''
                lstr_SQL = "spSaveContainerBookingAdv"
                'definir que tipo de comando se va a ejecutar
                iolecmd_comand.CommandType = CommandType.StoredProcedure
                iolecmd_comand.CommandText = lstr_SQL

                ''ejecutar 
                adapter = New OleDbDataAdapter(iolecmd_comand)
                ''''''''''''''''''''
                'Return dt_RetrieveErrorTable("antes try call det adv")

                ldt_resultdt = New DataTable("dresult")

                lint_result = -1
                lstr_Message = ""
                lobj_resultstruct(0).iint_Succes = -1
                lobj_ResultITem.iint_Succes = -1
                Try
                    ''conectar
                    iolecmd_comand.Connection.Open()
                    'If lint_counter > 0 Then
                    '    Return dt_RetrieveErrorTable("ya paso counter=" + lint_counter.ToString())
                    'End If
                    adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
                    adapter.Fill(ldt_resultdt)
                    ''desconectar
                Catch ex As Exception
                    lint_result = 0
                    lstr_Message = ObtenerError(ex.Message, 9999)
                    If lstr_Message.Length > 0 Then
                        'Return dt_RetrieveErrorTable(lstr_Message)
                        lstr_Message = lstr_Message
                    Else
                        'Return dt_RetrieveErrorTable(ex.Message)
                        lstr_Message = ex.Message
                    End If
                    lobj_resultstruct(lint_idx).iint_Succes = 0
                    lobj_resultstruct(lint_idx).istr_Message = lstr_Message

                    lobj_ResultITem.iint_Succes = 0
                    lobj_ResultITem.istr_Message = lstr_Message
                    llist_return.Add(lobj_ResultITem)
                    Continue For

                Finally
                    iolecmd_comand.Connection.Close()
                    ' iolecmd_comand.Connection.Dispose()
                    'ioleconx_conexion.close()
                End Try
                ' Return dt_RetrieveErrorTable("despuyes try call det adv")
                'Return dt_RetrieveErrorTable("despues try call det adv tab..row=" + ldt_resultdt.Rows.Count.ToString() + ",col=" + ldt_resultdt.Columns.Count.ToString())


                'iolecmd_comand = Nothing

                '' ver si la tabla trajo informacion 
                Try

                    If ldt_resultdt.Rows.Count = 1 And ldt_resultdt.Columns.Count = 1 Then

                        Dim lstr_info As String
                        Dim lstr_Value As String

                        ' Return dt_RetrieveErrorTable("columna con=" + ldt_resultdt.Columns.Count.ToString() + "(0)=" + ldt_resultdt.Rows(0)(0).ToString())

                        ' probar si es la columna de aviso 
                        Try
                            lstr_Value = ldt_resultdt(0)("intBookingAdviceId").ToString()

                            If lstr_Value.Length > 0 Then
                                'lrow_Result = ldtb_ResultData.NewRow()
                                'lrow_Result("intBookingAdviceId") = lstr_Value
                                'lrow_Result("strContainerId") = ""
                                'ldtb_ResultData.Rows.Add(lrow_Result)
                                lint_counterSuccess = lint_counterSuccess + 1
                                lint_result = 1

                                lobj_resultstruct(lint_idx).iint_Succes = 1
                                lobj_resultstruct(lint_idx).istr_Message = ""

                                lobj_ResultITem.iint_Succes = 1
                                lobj_ResultITem.istr_Message = ""
                                llist_return.Add(lobj_ResultITem)


                                Continue For

                            End If
                        Catch ex As Exception
                            'lstr_Message = "error=" + ex.Message
                            'Return dt_RetrieveErrorTable("e=" + lstr_Message)
                        End Try

                        ' si es cadena nula
                        If lstr_Value Is Nothing Then
                            lstr_Value = ""
                        End If
                        ' sino hay contenedot
                        If lstr_Value.Length = 0 Then

                            'lrow_Result = ldtb_ResultData.NewRow()
                            lstr_info = ldt_resultdt(0)(0).ToString



                            If lstr_info.Length > 1 Then
                                'Return dt_RetrieveErrorTable(lstr_info)

                                'REVISAR ERROR 2601
                                If lstr_info = "2601" Then
                                    lrow_Result = ldtb_ResultData.NewRow()
                                    lrow_Result("Mensaje") = "El contenedor " + larg_lstr_ContainerId + "ya existe en el preaviso"
                                    ldtb_ResultData.Rows.Add(lrow_Result)

                                    lobj_resultstruct(lint_idx).iint_Succes = 0
                                    lobj_resultstruct(lint_idx).istr_Message = "El contenedor " + larg_lstr_ContainerId + "ya existe en el preaviso"

                                    lobj_ResultITem.iint_Succes = 0
                                    lobj_ResultITem.istr_Message = "El contenedor " + larg_lstr_ContainerId + "ya existe en el preaviso"
                                Else
                                    lint_error = 0
                                    If Integer.TryParse(lstr_info, lint_error) = False Then
                                        lint_error = 0
                                    End If
                                    'si se obtuvo error
                                    If lint_error > 0 Then
                                        lstr_Message = "Error numero " + lint_error.ToString()
                                    Else
                                        lstr_Message = lstr_info
                                    End If

                                    lrow_Result = ldtb_ResultData.NewRow()
                                    lrow_Result("Mensaje") = lstr_info
                                    ldtb_ResultData.Rows.Add(lrow_Result)

                                    lobj_resultstruct(lint_idx).istr_Message = lstr_Message
                                    lobj_ResultITem.istr_Message = lstr_Message

                                End If

                                lobj_resultstruct(lint_idx).iint_Succes = 0
                                lobj_ResultITem.iint_Succes = 0

                                llist_return.Add(lobj_ResultITem)
                                Continue For

                            End If ' If lstr_info.Length > 1 Then

                        End If '   If lstr_Value.Length = 0 Then

                    Else
                        '' se espera un renglon con 4 columnas 
                        If ldt_resultdt.Columns.Count > 1 Then
                            'Return dt_RetrieveErrorTable("columna con=" + ldt_resultdt.Columns.Count.ToString() + "(0)=" + ldt_resultdt.Rows(0)(0).ToString() + "(1)=" + ldt_resultdt.Rows(0)(1).ToString())
                            ' pasar las 4 columnas 

                            'lrow_Result = ldtb_ResultData.NewRow()
                            'lrow_Result("intBookingAdviceId") = ldt_resultdt.Rows(0)(0).ToString()
                            'lrow_Result("strContainerId") = ldt_resultdt.Rows(0)(1).ToString()

                            'ldtb_ResultData.Rows.Add(lrow_Result)
                            lint_counterSuccess = lint_counterSuccess + 1
                            lobj_resultstruct(lint_idx).iint_Succes = 1
                            lobj_ResultITem.iint_Succes = 1
                            llist_return.Add(lobj_ResultITem)
                            Continue For
                        End If
                    End If
                Catch ex As Exception
                    Dim lstr_ex As String
                    lstr_ex = ex.Message
                    lstr_ex = lstr_ex
                    'Return dt_RetrieveErrorTable("wx=" + ex.Message)
                    'Return dt_RetrieveErrorTable("error al actualizar informacion ")
                    lobj_resultstruct(lint_idx).istr_Message = lstr_ex
                    lobj_resultstruct(lint_idx).iint_Succes = 0

                    lobj_ResultITem.istr_Message = lstr_ex
                    lobj_ResultITem.iint_Succes = 0
                    llist_return.Add(lobj_ResultITem)
                End Try

                ''''''''''''''''''''''


            Catch ex As Exception

                '  Return dt_RetrieveErrorTable("wx2=" + ex.Message)
            End Try

        Next
        ''''---
        'Return dt_RetrieveErrorTable("counter=" + lint_counter.ToString())

        ' si hay insertados 
        'If lint_counterSuccess > 0 Then
        lrow_Result = ldtb_ResultData.NewRow()
        'lrow_Result("Mensaje") = "insertados" + lint_counterSuccess.ToString()
        lrow_Result("Mensaje") = lint_counterSuccess.ToString()
        ldtb_ResultData.Rows.InsertAt(lrow_Result, 0)

        ' End If

        Try
            iolecmd_comand.Connection.Dispose()
            iolecmd_comand = Nothing
        Catch ex As Exception

            '  Return dt_RetrieveErrorTable("wx=" + ex.Message)
        End Try
        ''''''''
        Return llist_return
        'Return lobj_resultstruct
        'Return ldtb_ResultData

        '''''''''''''''''''''''''''''''''

    End Function
    ''
    ''

    Public Function of_saveProductContainerAdvice(ByVal aint_AdviceBooking As Integer, ByVal alistobj_containerProduct As ClsAdviceContainerProduct(), ByVal astr_user As String) As List(Of ClsAdviceResult) ' As ClsAdviceResult() ' As DataTable


        Dim ldt_AdviceBookingResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_resultdt As DataTable = New DataTable("dresult")
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnAdviceBRow As DataRow

        Dim larg_lint_AdviceBooking As Integer
        Dim larg_lstr_ContainerId As String
        Dim larg_lint_ProductId As Integer
        Dim larg_lint_ItemContProduct As Integer

        Dim larg_lint_Operation As Integer
        Dim larg_lstr_comments As String

        '' crear tabla de resultados 
        Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
        Dim lrow_Result As DataRow
        Dim lstr_serviceType As String

        Dim lint_counterSuccess As Integer
        'Dim lstr_message As String
        Dim lint_error As Integer
        Dim lobj_resultstruct(alistobj_containerProduct.Length - 1) As ClsAdviceResult
        Dim llist_return As List(Of ClsAdviceResult) = New List(Of ClsAdviceResult)
        Dim lobj_ResultITem As ClsAdviceResult

        ldtb_ResultData.TableName = "DataResult"
        'ldtb_ResultData.Columns.Add("intBookingAdviceId", GetType(Integer))
        'ldtb_ResultData.Columns.Add("strContainerId", GetType(String))
        ldtb_ResultData.Columns.Add("Mensaje", GetType(String))


        ''''''''''''''''''''''''''''''''''''''''''''''''
        ''' primer bloque 


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        '' validaciones --->
        '' numero de aviso
        If aint_AdviceBooking = 0 Then
            dt_RetrieveErrorTable("No existe numero de aviso ")
        End If


        '' tabla
        Try
            'si es nulo el listado
            If alistobj_containerProduct Is Nothing Then

                lobj_ResultITem = New ClsAdviceResult()
                lobj_ResultITem.iint_AdviceId = aint_AdviceBooking
                lobj_ResultITem.iint_Succes = 1
                lobj_ResultITem.istr_Container = ""
                lobj_ResultITem.istr_Message = ""
                llist_return.Add(lobj_ResultITem)
                Return llist_return
            End If

            '' si el listado , no tiene items 
            If alistobj_containerProduct.Count = 0 Then
                'Return dt_RetrieveErrorTable(aint_AdviceBooking)
                ' Return lobj_resultstruct
                Return New List(Of ClsAdviceResult)

            End If

        Catch ex As Exception

            lobj_ResultITem = New ClsAdviceResult()
            lobj_ResultITem.iint_AdviceId = aint_AdviceBooking
            lobj_ResultITem.iint_Succes = 1
            lobj_ResultITem.istr_Container = ""
            lobj_ResultITem.istr_Message = ""
            llist_return.Add(lobj_ResultITem)
            Return llist_return

        End Try


        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intBkAdvDetailProd", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)

        iolecmd_comand.Parameters.Add("@intProdQuantity", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intProdPackingId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@decProductWeight", OleDbType.Decimal)

        iolecmd_comand.Parameters.Add("@strBkAdvContComments", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strUser", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)



        lint_counterSuccess = 0
        ''crear 
        Dim adapter As OleDbDataAdapter
        Dim lint_counter As Integer = 0
        Dim lint_idx As Integer = 0
        Dim lint_result As Integer = 0

        'Return dt_RetrieveErrorTable("antes de ciclo adv det")

        For lint_idx = 0 To alistobj_containerProduct.Count - 1

            Try

                larg_lint_AdviceBooking = aint_AdviceBooking
                larg_lstr_ContainerId = alistobj_containerProduct(lint_idx).strContainerId
                larg_lint_ItemContProduct = alistobj_containerProduct(lint_idx).intBkAdvDetailProd
                larg_lint_ProductId = alistobj_containerProduct(lint_idx).intProductId
                larg_lint_Operation = alistobj_containerProduct(lint_idx).intMode
                larg_lstr_comments = ""

                lobj_resultstruct(lint_idx) = New ClsAdviceResult

                lobj_resultstruct(lint_idx).iint_AdviceId = aint_AdviceBooking
                lobj_resultstruct(lint_idx).istr_Container = alistobj_containerProduct(lint_idx).strContainerId
                lobj_resultstruct(lint_idx).istr_Message = ""

                lobj_ResultITem = New ClsAdviceResult

                lobj_ResultITem.iint_AdviceId = aint_AdviceBooking
                lobj_ResultITem.istr_Container = alistobj_containerProduct(lint_idx).strContainerId
                lobj_ResultITem.istr_Message = ""

                iolecmd_comand.Parameters("@intBookingAdviceId").Value = larg_lint_AdviceBooking
                iolecmd_comand.Parameters("@intBkAdvDetailProd").Value = larg_lint_ItemContProduct
                iolecmd_comand.Parameters("@strContainerId").Value = larg_lstr_ContainerId
                iolecmd_comand.Parameters("@intProductId").Value = larg_lint_ProductId

                iolecmd_comand.Parameters("@intProdQuantity").Value = 0
                iolecmd_comand.Parameters("@intProdPackingId").Value = 0
                iolecmd_comand.Parameters("@decProductWeight").Value = 0
                iolecmd_comand.Parameters("@strBkAdvContComments").Value = ""
                iolecmd_comand.Parameters("@strUser").Value = astr_user

                iolecmd_comand.Parameters("@intMode").Value = larg_lint_Operation


                '''''
                lstr_SQL = "spCRUDBookingAdviceProduct"
                'definir que tipo de comando se va a ejecutar
                iolecmd_comand.CommandType = CommandType.StoredProcedure
                iolecmd_comand.CommandText = lstr_SQL

                ''ejecutar 
                adapter = New OleDbDataAdapter(iolecmd_comand)
                ''''''''''''''''''''
                'Return dt_RetrieveErrorTable("antes try call det adv")

                ldt_resultdt = New DataTable("dresult")

                lint_result = -1
                lstr_Message = ""
                lobj_resultstruct(0).iint_Succes = -1
                lobj_ResultITem.iint_Succes = -1

                Try
                    ''conectar
                    iolecmd_comand.Connection.Open()
                    'If lint_counter > 0 Then
                    '    Return dt_RetrieveErrorTable("ya paso counter=" + lint_counter.ToString())
                    'End If
                    adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
                    adapter.Fill(ldt_resultdt)
                    ''desconectar
                Catch ex As Exception
                    lint_result = 0
                    lstr_Message = ObtenerError(ex.Message, 9999)
                    If lstr_Message.Length > 0 Then
                        'Return dt_RetrieveErrorTable(lstr_Message)
                        lstr_Message = lstr_Message
                    Else
                        'Return dt_RetrieveErrorTable(ex.Message)
                        lstr_Message = ex.Message
                    End If
                    lobj_resultstruct(lint_idx).iint_Succes = 0
                    lobj_resultstruct(lint_idx).istr_Message = lstr_Message

                    lobj_ResultITem.iint_Succes = 0
                    lobj_ResultITem.istr_Message = lstr_Message
                    llist_return.Add(lobj_ResultITem)
                    Continue For

                Finally
                    iolecmd_comand.Connection.Close()
                    ' iolecmd_comand.Connection.Dispose()
                    'ioleconx_conexion.close()
                End Try
                ' Return dt_RetrieveErrorTable("despuyes try call det adv")
                'Return dt_RetrieveErrorTable("despues try call det adv tab..row=" + ldt_resultdt.Rows.Count.ToString() + ",col=" + ldt_resultdt.Columns.Count.ToString())


                'iolecmd_comand = Nothing

                '' ver si la tabla trajo informacion 
                Try

                    If ldt_resultdt.Rows.Count = 1 And ldt_resultdt.Columns.Count = 1 Then

                        Dim lstr_info As String
                        Dim lstr_Value As String

                        ' Return dt_RetrieveErrorTable("columna con=" + ldt_resultdt.Columns.Count.ToString() + "(0)=" + ldt_resultdt.Rows(0)(0).ToString())

                        ' probar si es la columna de aviso 
                        Try
                            lstr_Value = ldt_resultdt(0)("intBookingAdviceId").ToString()

                            If lstr_Value.Length > 0 Then
                                'lrow_Result = ldtb_ResultData.NewRow()
                                'lrow_Result("intBookingAdviceId") = lstr_Value
                                'lrow_Result("strContainerId") = ""
                                'ldtb_ResultData.Rows.Add(lrow_Result)
                                lint_counterSuccess = lint_counterSuccess + 1
                                lint_result = 1

                                lobj_resultstruct(lint_idx).iint_Succes = 1
                                lobj_resultstruct(lint_idx).istr_Message = ""

                                lobj_ResultITem.iint_Succes = 1
                                lobj_ResultITem.istr_Message = ""
                                llist_return.Add(lobj_ResultITem)


                                Continue For

                            End If
                        Catch ex As Exception
                            'lstr_Message = "error=" + ex.Message
                            'Return dt_RetrieveErrorTable("e=" + lstr_Message)
                        End Try

                        ' si es cadena nula
                        If lstr_Value Is Nothing Then
                            lstr_Value = ""
                        End If
                        ' sino hay contenedot
                        If lstr_Value.Length = 0 Then

                            'lrow_Result = ldtb_ResultData.NewRow()
                            lstr_info = ldt_resultdt(0)(0).ToString



                            If lstr_info.Length > 1 Then
                                'Return dt_RetrieveErrorTable(lstr_info)

                                'REVISAR ERROR 2601
                                If lstr_info = "2601" Then
                                    lrow_Result = ldtb_ResultData.NewRow()
                                    lrow_Result("Mensaje") = "El contenedor " + larg_lstr_ContainerId + "ya existe en el preaviso"
                                    ldtb_ResultData.Rows.Add(lrow_Result)

                                    lobj_resultstruct(lint_idx).iint_Succes = 0
                                    lobj_resultstruct(lint_idx).istr_Message = "El contenedor " + larg_lstr_ContainerId + "ya existe en el preaviso"

                                    lobj_ResultITem.iint_Succes = 0
                                    lobj_ResultITem.istr_Message = "El contenedor " + larg_lstr_ContainerId + "ya existe en el preaviso"
                                Else
                                    lint_error = 0
                                    If Integer.TryParse(lstr_info, lint_error) = False Then
                                        lint_error = 0
                                    End If
                                    'si se obtuvo error
                                    If lint_error > 0 Then
                                        lstr_Message = "Error numero " + lint_error.ToString()
                                    Else
                                        lstr_Message = lstr_info
                                    End If

                                    lrow_Result = ldtb_ResultData.NewRow()
                                    lrow_Result("Mensaje") = lstr_info
                                    ldtb_ResultData.Rows.Add(lrow_Result)

                                    lobj_resultstruct(lint_idx).istr_Message = lstr_Message
                                    lobj_ResultITem.istr_Message = lstr_Message

                                End If

                                lobj_resultstruct(lint_idx).iint_Succes = 0
                                lobj_ResultITem.iint_Succes = 0

                                llist_return.Add(lobj_ResultITem)
                                Continue For

                            End If ' If lstr_info.Length > 1 Then

                        End If '   If lstr_Value.Length = 0 Then

                    Else
                        '' se espera un renglon con 4 columnas 
                        If ldt_resultdt.Columns.Count > 1 Then
                            'Return dt_RetrieveErrorTable("columna con=" + ldt_resultdt.Columns.Count.ToString() + "(0)=" + ldt_resultdt.Rows(0)(0).ToString() + "(1)=" + ldt_resultdt.Rows(0)(1).ToString())
                            ' pasar las 4 columnas 

                            'lrow_Result = ldtb_ResultData.NewRow()
                            'lrow_Result("intBookingAdviceId") = ldt_resultdt.Rows(0)(0).ToString()
                            'lrow_Result("strContainerId") = ldt_resultdt.Rows(0)(1).ToString()

                            'ldtb_ResultData.Rows.Add(lrow_Result)
                            lint_counterSuccess = lint_counterSuccess + 1
                            lobj_resultstruct(lint_idx).iint_Succes = 1
                            lobj_ResultITem.iint_Succes = 1
                            llist_return.Add(lobj_ResultITem)
                            Continue For
                        End If
                    End If
                Catch ex As Exception
                    Dim lstr_ex As String
                    lstr_ex = ex.Message
                    lstr_ex = lstr_ex
                    'Return dt_RetrieveErrorTable("wx=" + ex.Message)
                    'Return dt_RetrieveErrorTable("error al actualizar informacion ")
                    lobj_resultstruct(lint_idx).istr_Message = lstr_ex
                    lobj_resultstruct(lint_idx).iint_Succes = 0

                    lobj_ResultITem.istr_Message = lstr_ex
                    lobj_ResultITem.iint_Succes = 0
                    llist_return.Add(lobj_ResultITem)
                End Try

                ''''''''''''''''''''''


            Catch ex As Exception

                '  Return dt_RetrieveErrorTable("wx2=" + ex.Message)
            End Try

        Next
        ''''---

        'lrow_Result = ldtb_ResultData.NewRow()
        'lrow_Result("Mensaje") = lint_counterSuccess.ToString()
        'ldtb_ResultData.Rows.InsertAt(lrow_Result, 0)

        ' End If

        Try
            iolecmd_comand.Connection.Dispose()
            iolecmd_comand = Nothing
        Catch ex As Exception

            '  Return dt_RetrieveErrorTable("wx=" + ex.Message)
        End Try
        ''''''''
        Return llist_return
        'Return lobj_resultstruct
        'Return ldtb_ResultData

        '''''''''''''''''''''''''''''''''

    End Function
    ''
    ''


    Public Function of_ValidateMasterAdvice(ByVal aint_BookingAdviceId As Integer, ByVal astr_BookingId As String, ByVal aint_VesselId As Integer, ByVal alng_VesselVoyageId As Long, ByVal astr_PortId As String, ByVal astr_ETAtDate As String, ByVal aint_CustomerId As Integer, ByVal aint_CustomBrokerId As Integer, ByVal aint_ShippingLine As Integer, ByVal aint_ProductId As Integer, ByVal aint_IMOCode As Integer, ByVal aint_UNCode As Integer, ByVal astr_ServiceType As String, ByVal astr_AdviceComs As String, ByVal astr_User As String) As String


        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)

        iolecmd_comand.Parameters.Add("@strBookingId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVoyageExpoId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intVesselVoyageId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strPortText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strPortId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strETAtDate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCustomerTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomBrokerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strShippingLinetxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strProductText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intIMOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUNCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strServiceType", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAdviceComms", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidBooking", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUser", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intblnIsUniqueMerchType", OleDbType.Integer)

        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdviceId
        iolecmd_comand.Parameters("@strBookingId").Value = astr_BookingId
        iolecmd_comand.Parameters("@strVesselName").Value = ""
        iolecmd_comand.Parameters("@strVoyageExpoId").Value = ""
        iolecmd_comand.Parameters("@intVesselId").Value = aint_VesselId
        iolecmd_comand.Parameters("@intVesselVoyageId").Value = alng_VesselVoyageId
        iolecmd_comand.Parameters("@strPortText").Value = ""
        iolecmd_comand.Parameters("@strPortId").Value = astr_PortId
        iolecmd_comand.Parameters("@strCountryTxt").Value = ""
        iolecmd_comand.Parameters("@strCountryId").Value = ""
        iolecmd_comand.Parameters("@strETAtDate").Value = astr_ETAtDate
        iolecmd_comand.Parameters("@strCustomerTxt").Value = ""
        iolecmd_comand.Parameters("@intCustomerId").Value = aint_CustomerId
        iolecmd_comand.Parameters("@intCustomBrokerId").Value = aint_CustomBrokerId
        iolecmd_comand.Parameters("@strShippingLinetxt").Value = ""
        iolecmd_comand.Parameters("@intShippingLine").Value = aint_ShippingLine
        iolecmd_comand.Parameters("@strProductText").Value = ""
        iolecmd_comand.Parameters("@intProductId").Value = aint_ProductId
        iolecmd_comand.Parameters("@intIMOCode").Value = aint_IMOCode
        iolecmd_comand.Parameters("@intUNCode").Value = aint_UNCode
        iolecmd_comand.Parameters("@strAdviceComms").Value = astr_AdviceComs
        iolecmd_comand.Parameters("@strServiceType").Value = astr_ServiceType

        iolecmd_comand.Parameters("@blnIsValidBooking").Value = "VALID"
        iolecmd_comand.Parameters("@blnIsValidByShipper").Value = -1
        iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1

        iolecmd_comand.Parameters("@strUser").Value = astr_User


        iolecmd_comand.Parameters("@intblnIsUniqueMerchType").Value = -1

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveMasterBookingAdv"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return "error al actualizar informacion "
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''
        Return ""
    End Function


    Public Function of_ValidateContainer(ByVal aint_AdviceBooking As Integer, ByVal astr_container As String, ByVal aint_ContainerType As Integer, ByVal aint_ContainerSize As Integer, ByVal aint_ContainerISOode As Integer, ByVal aint_ShippingLine As Integer, ByVal adec_VGM As Decimal, ByVal aint_Full As Integer, ByVal astr_user As String) As DataTable


        Dim ldt_AdviceBookingResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_resultdt As DataTable = New DataTable("dresult")
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnAdviceBRow As DataRow


        '' crear tabla de resultados 
        Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
        Dim lrow_Result As DataRow
        Dim lstr_serviceType As String


        ldtb_ResultData.TableName = "DataResult"
        ldtb_ResultData.Columns.Add("intBookingAdviceId", GetType(Integer))
        ldtb_ResultData.Columns.Add("strContainerId", GetType(String))

        ''''''''''''''''''''''''''''''''''''''''''''''''
        ''' primer bloque 


        'Dim llng_ServiceOrderId As Long

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        '' validaciones --->
        '' numero de aviso
        If aint_AdviceBooking = 0 Then
            Return dt_RetrieveErrorTable("No existe numero de aviso ")
        End If


        '' tabla
        Try
            ' solo validar el nombre del contenedor 
            If astr_container.Length = 0 Then
                Return dt_RetrieveErrorTable("no esta el contenedor ")
            End If

        Catch ex As Exception
            Return dt_RetrieveErrorTable(aint_AdviceBooking)
        End Try


        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0

        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerISOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@decVGM", OleDbType.Decimal)

        iolecmd_comand.Parameters.Add("@blnContainerIsFull", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsValidItem", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strSealNumber", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@decWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@strWeigherId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strISOCodeText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strComments", OleDbType.Char)



        'If aorigint_visit > 0 And alng_Visit > 0 Then
        '    Return dt_RetrieveErrorTable("-bloque CUAT ")
        'End If

        ''crear 
        Dim adapter As OleDbDataAdapter
        Dim lint_counter As Integer = 0
        Dim lint_idx As Integer = 0



        Try


            iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_AdviceBooking
            iolecmd_comand.Parameters("@strContainerId").Value = astr_container
            iolecmd_comand.Parameters("@intContainerSize").Value = aint_ContainerSize
            iolecmd_comand.Parameters("@intContainerType").Value = aint_ContainerType
            iolecmd_comand.Parameters("@intContainerISOCode").Value = aint_ContainerISOode
            iolecmd_comand.Parameters("@intShippingLine").Value = aint_ShippingLine
            iolecmd_comand.Parameters("@decVGM").Value = adec_VGM

            iolecmd_comand.Parameters("@blnContainerIsFull").Value = aint_Full
            iolecmd_comand.Parameters("@intOperation").Value = 2
            iolecmd_comand.Parameters("@blnIsValidItem").Value = "VALID"
            iolecmd_comand.Parameters("@blIsValidByShipper").Value = -1
            iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1
            iolecmd_comand.Parameters("@strUsername").Value = astr_user
            iolecmd_comand.Parameters("@strSealNumber").Value = ""
            iolecmd_comand.Parameters("@decWeight").Value = 0
            iolecmd_comand.Parameters("@strWeigherId").Value = ""
            iolecmd_comand.Parameters("@strISOCodeText").Value = ""
            iolecmd_comand.Parameters("@strComments").Value = ""


            '''''
            lstr_SQL = "spSaveContainerBookingAdv"
            'definir que tipo de comando se va a ejecutar
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandText = lstr_SQL

            ''ejecutar 
            adapter = New OleDbDataAdapter(iolecmd_comand)
            ''''''''''''''''''''

            Try
                ''conectar
                iolecmd_comand.Connection.Open()
                'If lint_counter > 0 Then
                '    Return dt_RetrieveErrorTable("ya paso counter=" + lint_counter.ToString())
                'End If
                adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
                adapter.Fill(ldt_resultdt)
                ''desconectar
            Catch ex As Exception

                lstr_Message = ObtenerError(ex.Message, 9999)
                If lstr_Message.Length > 0 Then
                    Return dt_RetrieveErrorTable(lstr_Message)
                Else
                    Return dt_RetrieveErrorTable(ex.Message)
                End If
            Finally
                iolecmd_comand.Connection.Close()
                ' iolecmd_comand.Connection.Dispose()
                'ioleconx_conexion.close()
            End Try


            'iolecmd_comand = Nothing

            '' ver si la tabla trajo informacion 
            Try

                If ldt_resultdt.Rows.Count = 1 And ldt_resultdt.Columns.Count = 1 Then
                    Dim lstr_info As String
                    Dim lstr_Value As String

                    '' probar si es la columna de aviso 
                    'Try
                    '    lstr_Value = ldt_resultdt(0)("intBookingAdviceId").ToString()

                    '    If lstr_Value.Length > 0 Then
                    '        lrow_Result = ldtb_ResultData.NewRow()
                    '        lrow_Result("intBookingAdviceId") = lstr_Value
                    '        lrow_Result("strContainerId") = ""
                    '        ldtb_ResultData.Rows.Add(lrow_Result)

                    '    End If
                    'Catch ex As Exception

                    'End Try
                    ' sino hay contenedot
                    If lstr_Value.Length = 0 Then

                        lstr_info = ldt_resultdt(0)(0).ToString
                        If lstr_info.Length > 1 Then
                            Return dt_RetrieveErrorTable(lstr_info)
                        Else
                            Return dt_RetrieveErrorTable("error vacio")
                        End If

                    End If

                Else
                    '' se espera un renglon con 4 columnas 
                    If ldt_resultdt.Columns.Count > 1 Then
                        ' pasar las 4 columnas 
                        lrow_Result = ldtb_ResultData.NewRow()
                        lrow_Result("intBookingAdviceId") = ldt_resultdt.Rows(0)(0).ToString()
                        lrow_Result("strContainerId") = ldt_resultdt.Rows(0)(1).ToString()

                        ldtb_ResultData.Rows.Add(lrow_Result)

                    End If
                End If
            Catch ex As Exception
                Dim lstr_ex As String
                lstr_ex = ex.Message
                lstr_ex = lstr_ex
                Return dt_RetrieveErrorTable(lstr_ex)
                'Return dt_RetrieveErrorTable("error al actualizar informacion ")
            End Try

            ''''''''''''''''''''''


        Catch ex As Exception

        End Try


        ''''---
        'Return dt_RetrieveErrorTable("counter=" + lint_counter.ToString())

        Try
            iolecmd_comand.Connection.Dispose()
            iolecmd_comand = Nothing
        Catch ex As Exception

        End Try
        ''''''''
        Return ldtb_ResultData

        '''''''''''''''''''''''''''''''''

    End Function



    Public Function of_UpdateMasterAdviceV(ByVal aint_BookingAdviceId As Integer, ByVal astr_BookingId As String, ByVal aint_VesselId As Integer, ByVal alng_VesselVoyageId As Long, ByVal astr_PortId As String, ByVal astr_ETAtDate As String, ByVal aint_CustomerId As Integer, ByVal aint_CustomBrokerId As Integer, ByVal aint_ShippingLine As Integer, ByVal aint_ProductId As Integer, ByVal aint_IMOCode As Integer, ByVal aint_UNCode As Integer, ByVal astr_ServiceType As String, ByVal astr_AdviceComs As String, ByVal astr_ValidateMasterDev As String, ByVal astr_User As String, ByVal aint_blnIsSingleMerchandise As Integer) As String


        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)

        iolecmd_comand.Parameters.Add("@strBookingId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVoyageExpoId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intVesselVoyageId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strPortText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strPortId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strETAtDate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCustomerTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomBrokerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strShippingLinetxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strProductText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intIMOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUNCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strServiceType", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAdviceComms", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidBooking", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUser", OleDbType.Char)

        iolecmd_comand.Parameters.Add("@intblnIsUniqueMerchType", OleDbType.Integer)


        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdviceId
        iolecmd_comand.Parameters("@strBookingId").Value = astr_BookingId
        iolecmd_comand.Parameters("@strVesselName").Value = ""
        iolecmd_comand.Parameters("@strVoyageExpoId").Value = ""
        iolecmd_comand.Parameters("@intVesselId").Value = aint_VesselId
        iolecmd_comand.Parameters("@intVesselVoyageId").Value = alng_VesselVoyageId
        iolecmd_comand.Parameters("@strPortText").Value = ""
        iolecmd_comand.Parameters("@strPortId").Value = astr_PortId
        iolecmd_comand.Parameters("@strCountryTxt").Value = ""
        iolecmd_comand.Parameters("@strCountryId").Value = ""
        iolecmd_comand.Parameters("@strETAtDate").Value = astr_ETAtDate
        iolecmd_comand.Parameters("@strCustomerTxt").Value = ""
        iolecmd_comand.Parameters("@intCustomerId").Value = aint_CustomerId
        iolecmd_comand.Parameters("@intCustomBrokerId").Value = aint_CustomBrokerId
        iolecmd_comand.Parameters("@strShippingLinetxt").Value = ""
        iolecmd_comand.Parameters("@intShippingLine").Value = aint_ShippingLine
        iolecmd_comand.Parameters("@strProductText").Value = ""
        iolecmd_comand.Parameters("@intProductId").Value = aint_ProductId
        iolecmd_comand.Parameters("@intIMOCode").Value = aint_IMOCode
        iolecmd_comand.Parameters("@intUNCode").Value = aint_UNCode
        iolecmd_comand.Parameters("@strAdviceComms").Value = astr_AdviceComs
        iolecmd_comand.Parameters("@strServiceType").Value = astr_ServiceType

        iolecmd_comand.Parameters("@blnIsValidBooking").Value = astr_ValidateMasterDev
        iolecmd_comand.Parameters("@blnIsValidByShipper").Value = -1
        iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1

        iolecmd_comand.Parameters("@strUser").Value = astr_User


        iolecmd_comand.Parameters("@intblnIsUniqueMerchType").Value = aint_blnIsSingleMerchandise


        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveMasterBookingAdv"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return "error al actualizar informacion "
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''
        Return ""
    End Function


    '-----------
    Public Function of_UpdateStatusMasterAdvice(ByVal aint_BookingAdviceId As Integer, ByVal astr_Status As String, ByVal astr_User As String) As String


        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)

        iolecmd_comand.Parameters.Add("@strBookingId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strVoyageExpoId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intVesselVoyageId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strPortText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strPortId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCountryId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strETAtDate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strCustomerTxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomBrokerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strShippingLinetxt", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strProductText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intIMOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUNCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strServiceType", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAdviceComms", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidBooking", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUser", OleDbType.Char)


        iolecmd_comand.Parameters.Add("@intblnIsUniqueMerchType", OleDbType.Integer)


        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdviceId
        iolecmd_comand.Parameters("@strBookingId").Value = ""
        iolecmd_comand.Parameters("@strVesselName").Value = ""
        iolecmd_comand.Parameters("@strVoyageExpoId").Value = ""
        iolecmd_comand.Parameters("@intVesselId").Value = -1
        iolecmd_comand.Parameters("@intVesselVoyageId").Value = -1
        iolecmd_comand.Parameters("@strPortText").Value = ""
        iolecmd_comand.Parameters("@strPortId").Value = ""
        iolecmd_comand.Parameters("@strCountryTxt").Value = ""
        iolecmd_comand.Parameters("@strCountryId").Value = ""
        iolecmd_comand.Parameters("@strETAtDate").Value = ""
        iolecmd_comand.Parameters("@strCustomerTxt").Value = ""
        iolecmd_comand.Parameters("@intCustomerId").Value = -1
        iolecmd_comand.Parameters("@intCustomBrokerId").Value = -1
        iolecmd_comand.Parameters("@strShippingLinetxt").Value = ""
        iolecmd_comand.Parameters("@intShippingLine").Value = -1
        iolecmd_comand.Parameters("@strProductText").Value = ""
        iolecmd_comand.Parameters("@intProductId").Value = -1
        iolecmd_comand.Parameters("@intIMOCode").Value = -1
        iolecmd_comand.Parameters("@intUNCode").Value = -1
        iolecmd_comand.Parameters("@strAdviceComms").Value = ""
        iolecmd_comand.Parameters("@strServiceType").Value = ""

        iolecmd_comand.Parameters("@blnIsValidBooking").Value = astr_Status
        iolecmd_comand.Parameters("@blnIsValidByShipper").Value = -1
        iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1

        iolecmd_comand.Parameters("@strUser").Value = astr_User


        iolecmd_comand.Parameters("@intblnIsUniqueMerchType").Value = -1


        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveMasterBookingAdv"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return lstr_ex
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''
        Return ""
    End Function

    '' '
    Public Function of_UpdateStatusAdviceContainer(ByVal aint_AdviceBooking As Integer, ByVal astr_container As String, ByVal astr_Status As String, ByVal astr_user As String) As String


        Dim ldt_AdviceBookingResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_resultdt As DataTable = New DataTable("dresult")
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnAdviceBRow As DataRow


        '' crear tabla de resultados 
        Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
        Dim lrow_Result As DataRow
        Dim lstr_serviceType As String


        ldtb_ResultData.TableName = "DataResult"
        ldtb_ResultData.Columns.Add("intBookingAdviceId", GetType(Integer))
        ldtb_ResultData.Columns.Add("strContainerId", GetType(String))

        ''''''''''''''''''''''''''''''''''''''''''''''''
        ''' primer bloque 


        'Dim llng_ServiceOrderId As Long

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        '' validaciones --->
        '' numero de aviso
        If aint_AdviceBooking = 0 Then
            Return "No existe numero de aviso " ' dt_RetrieveErrorTable("No existe numero de aviso ")
        End If


        '' tabla
        Try
            ' solo validar el nombre del contenedor 
            If astr_container.Length = 0 Then
                Return "no esta el contenedor " ' dt_RetrieveErrorTable("no esta el contenedor ")
            End If

        Catch ex As Exception
            Return ex.Message ' dt_RetrieveErrorTable(aint_AdviceBooking)
        End Try


        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0

        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerISOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@decVGM", OleDbType.Decimal)

        iolecmd_comand.Parameters.Add("@blnContainerIsFull", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsValidItem", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strSealNumber", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@decWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@strWeigherId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strISOCodeText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strComments", OleDbType.Char)

        'If aorigint_visit > 0 And alng_Visit > 0 Then
        '    Return dt_RetrieveErrorTable("-bloque CUAT ")
        'End If

        ''crear 
        Dim adapter As OleDbDataAdapter
        Dim lint_counter As Integer = 0
        Dim lint_idx As Integer = 0



        Try


            iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_AdviceBooking
            iolecmd_comand.Parameters("@strContainerId").Value = astr_container
            iolecmd_comand.Parameters("@intContainerSize").Value = -1
            iolecmd_comand.Parameters("@intContainerType").Value = -1
            iolecmd_comand.Parameters("@intContainerISOCode").Value = -1
            iolecmd_comand.Parameters("@intShippingLine").Value = -1
            iolecmd_comand.Parameters("@decVGM").Value = -1

            iolecmd_comand.Parameters("@blnContainerIsFull").Value = -2
            iolecmd_comand.Parameters("@intOperation").Value = 2
            iolecmd_comand.Parameters("@blnIsValidItem").Value = astr_Status
            iolecmd_comand.Parameters("@blIsValidByShipper").Value = -1
            iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1
            iolecmd_comand.Parameters("@strUsername").Value = astr_user
            iolecmd_comand.Parameters("@strSealNumber").Value = ""
            iolecmd_comand.Parameters("@decWeight").Value = 0
            iolecmd_comand.Parameters("@strWeigherId").Value = ""
            iolecmd_comand.Parameters("@strISOCodeText").Value = ""
            iolecmd_comand.Parameters("@strComments").Value = ""

            '''''
            lstr_SQL = "spSaveContainerBookingAdv"
            'definir que tipo de comando se va a ejecutar
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandText = lstr_SQL

            ''ejecutar 
            adapter = New OleDbDataAdapter(iolecmd_comand)
            ''''''''''''''''''''

            Try
                ''conectar
                iolecmd_comand.Connection.Open()
                'If lint_counter > 0 Then
                '    Return dt_RetrieveErrorTable("ya paso counter=" + lint_counter.ToString())
                'End If
                adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
                adapter.Fill(ldt_resultdt)
                ''desconectar
            Catch ex As Exception

                lstr_Message = ObtenerError(ex.Message, 9999)
                If lstr_Message.Length > 0 Then
                    Return lstr_Message  'dt_RetrieveErrorTable(lstr_Message)
                Else
                    Return ex.Message ' dt_RetrieveErrorTable(ex.Message)
                End If
            Finally
                iolecmd_comand.Connection.Close()
                ' iolecmd_comand.Connection.Dispose()
                'ioleconx_conexion.close()
            End Try


            'iolecmd_comand = Nothing

            '' ver si la tabla trajo informacion 
            Try

                If ldt_resultdt.Rows.Count = 1 And ldt_resultdt.Columns.Count = 1 Then
                    Dim lstr_info As String
                    Dim lstr_Value As String

                    If lstr_Value.Length = 0 Then

                        lstr_info = ldt_resultdt(0)(0).ToString
                        If lstr_info.Length > 1 Then
                            Return lstr_info ' dt_RetrieveErrorTable(lstr_info)
                        Else
                            Return ""
                        End If

                    End If

                Else
                    '' se espera un renglon con 4 columnas 
                    If ldt_resultdt.Columns.Count > 1 Then
                        ' pasar las 4 columnas 
                        lrow_Result = ldtb_ResultData.NewRow()
                        lrow_Result("intBookingAdviceId") = ldt_resultdt.Rows(0)(0).ToString()
                        lrow_Result("strContainerId") = ldt_resultdt.Rows(0)(1).ToString()

                        ldtb_ResultData.Rows.Add(lrow_Result)
                        Return ""
                    End If
                End If
            Catch ex As Exception
                Dim lstr_ex As String
                lstr_ex = ex.Message
                lstr_ex = lstr_ex
                Return lstr_ex  'dt_RetrieveErrorTable(lstr_ex)
                'Return dt_RetrieveErrorTable("error al actualizar informacion ")
            End Try

            ''''''''''''''''''''''


        Catch ex As Exception

        End Try


        ''''---
        'Return dt_RetrieveErrorTable("counter=" + lint_counter.ToString())

        Try
            iolecmd_comand.Connection.Dispose()
            iolecmd_comand = Nothing
        Catch ex As Exception

        End Try
        ''''''''
        Return ""

        '''''''''''''''''''''''''''''''''

    End Function

    '''
    <WebMethod()>
    Public Function SetMasterAdvToPENDVAL(ByVal aint_AdviceBooking As Integer, ByVal astr_user As String) As String

        Dim lstr_result As String = ""

        lstr_result = of_UpdateStatusMasterAdvice(aint_AdviceBooking, "PENDVAL", astr_user)
        Return lstr_result

    End Function

    <WebMethod()>
    Public Function SetMasterAdvToREJECT(ByVal aint_AdviceBooking As Integer, ByVal aobj_note As ClsNoteAdvice, ByVal astr_user As String) As String

        Dim lstr_result As String = ""
        Dim lstr_resultnote As String = ""

        lstr_resultnote = of_SaveUpdateDelNote(aint_AdviceBooking, aobj_note, 1, astr_user)
        lstr_result = of_UpdateStatusMasterAdvice(aint_AdviceBooking, "REJECT", astr_user)

        Return lstr_resultnote

    End Function

    '''
    <WebMethod()>
    Public Function SetDetailContainerAdvToPENDVAL(ByVal aint_AdviceBooking As Integer, ByVal astr_container As String, ByVal astr_user As String) As String

        Dim lstr_result As String = ""
        lstr_result = of_UpdateStatusAdviceContainer(aint_AdviceBooking, astr_container, "PENDVAL", astr_user)

        Return lstr_result

    End Function

    <WebMethod()>
    Public Function SetDetailContainerAdvToREJECT(ByVal aint_AdviceBooking As Integer, ByVal astr_container As String, ByVal aobj_note As ClsNoteAdvice, ByVal astr_user As String) As String

        Dim lstr_result As String = ""
        Dim lstr_resultnote As String = ""

        aobj_note.istr_strContainerId = astr_container

        lstr_resultnote = of_SaveUpdateDelNote(aint_AdviceBooking, aobj_note, 1, astr_user)
        lstr_result = of_UpdateStatusAdviceContainer(aint_AdviceBooking, astr_container, "REJECT", astr_user)

        Return lstr_resultnote

    End Function


    '' '---

    ''' 
    '' ------------------

    ''---
    'Public Function of_SaveVisitDetail(ByVal alng_Visit As Long, ByVal alng_Customer As Long, ByVal aint_CustomerType As Integer, ByVal astr_Reference As String, ByVal aint_operation As Integer, ByVal aint_ServiceType As Integer, ByVal astr_UserName As String, ByVal alng_Requiredby As Long, ByVal aint_RequiredByType As Integer, ByVal adtb_VisitOperations As DataTable) As DataTable
    'Public Function of_SaveVisitDetail(ByVal alng_Visit As Long, ByVal alng_Customer As Long, ByVal astr_Reference As String, ByVal aint_operation As Integer, ByVal aint_ServiceType As Integer, ByVal astr_UserName As String, ByVal alng_Requiredby As Long, ByVal aint_RequiredByType As Integer, ByVal adtb_VisitOperations As DataTable) As DataTable

    '    ''''''''''''''''''''''''''
    '    '-----------------------------
    '    'Return dt_RetrieveErrorTable("start0 bloque")
    '    Dim ldt_VisitResult As DataTable 'tabla que guardara el resultado del query
    '    Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
    '    Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
    '    Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
    '    Dim istr_conx As String '' cadena de conexion
    '    Dim lint_operation As Integer = 0
    '    Dim lparamGeneric As OleDbParameter = New OleDbParameter()
    '    Dim ldtb_SaveResult As DataTable = New DataTable("")
    '    Dim llng_ServiceOrderId As Long

    '    Dim lstr_SQL As String
    '    Dim lstr_Message As String = ""
    '    Dim lint_itemscount As Integer = 0

    '    Dim ldt_TableResult As DataTable
    '    Dim ldt_ReturnValueTable As DataTable
    '    Dim ldr_ReturnTickeRow As DataRow

    '    Dim larg_lng_RequiredBy As Long
    '    Dim larg_lng_RequiredByType As Long
    '    Dim larg_int_ServiceType As Integer
    '    Dim larg_lng_UniversalId As Long
    '    Dim larg_int_OperationType As Integer
    '    Dim larg_lng_VisitId As Long
    '    Dim larg_lng_ServiceOrderId As Long
    '    Dim litem_lng_ServiceOrderId As Long
    '    Dim litem_int_VisitItemdId As Long
    '    Dim litem_int_operation As Long

    '    '' crear tabla de resultados 
    '    Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
    '    Dim lrow_Result As DataRow

    '    'Return dt_RetrieveErrorTable("bloque cero")

    '    ldtb_ResultData.TableName = "DataResult"
    '    ldtb_ResultData.Columns.Add("strContainerId", GetType(String))
    '    ldtb_ResultData.Columns.Add("intVisitId", GetType(Long))
    '    ldtb_ResultData.Columns.Add("lintVisitItemId", GetType(Integer))
    '    ldtb_ResultData.Columns.Add("intServiceOrderId", GetType(Integer))

    '    ''''''''''''''''''''''''''''''''''''''''''''''''
    '    ''' primer bloque 

    '    ' Return dt_RetrieveErrorTable("primer bloque")

    '    'Dim llng_ServiceOrderId As Long

    '    istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
    '    ldt_TableResult = New DataTable()

    '    ldt_ReturnValueTable = New DataTable()
    '    ldt_ReturnValueTable.TableName = "TableResultVisit"
    '    'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

    '    '' validaciones --->
    '    '' numero de visita 
    '    If alng_Visit = 0 Then
    '        dt_RetrieveErrorTable("No existe numero de visita")
    '    End If

    '    '' tabla
    '    Try
    '        If adtb_VisitOperations Is Nothing Then
    '            Return dt_RetrieveErrorTable(alng_Visit.ToString)
    '        End If

    '        If (adtb_VisitOperations.Rows.Count() = 0 Or adtb_VisitOperations.Columns.Count() < 2) Then
    '            dt_RetrieveErrorTable("No hay informacion a procesar ")
    '        End If
    '    Catch ex As Exception
    '        Return dt_RetrieveErrorTable(alng_Visit.ToString)
    '    End Try


    '    ''''''''''''''''''''''''''''

    '    ioleconx_conexion.ConnectionString = istr_conx
    '    iolecmd_comand = ioleconx_conexion.CreateCommand()
    '    iolecmd_comand.CommandTimeout = 0
    '    lint_itemscount = lint_itemscount + 1
    '    'limpiar cadena sql
    '    lstr_SQL = ""
    '    '' crear los parametros 
    '    'agregar parametros
    '    iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intRequieredBy", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intRequieredByType", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intServiceType", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intOperationType", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intServiceOrderId", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)

    '    ''crear 
    '    Dim adapter As OleDbDataAdapter

    '    For Each dataelement As DataRow In adtb_VisitOperations.Rows
    '        Try
    '            'larg_lng_VisitId = 
    '            larg_lng_UniversalId = CType(dataelement("intContainerUniversalId"), Long)
    '            litem_lng_ServiceOrderId = CType(dataelement("intServiceOrderId"), Long)
    '            litem_int_operation = CType(dataelement("intoperation"), Long)

    '            ''darle prioridad al id de la maniobra en la tabla 
    '            If litem_lng_ServiceOrderId > 0 Then
    '                larg_lng_ServiceOrderId = litem_lng_ServiceOrderId
    '            End If

    '            '' asignar los valores  '' agregar valores
    '            iolecmd_comand.Parameters("@intVisitId").Value = alng_Visit
    '            iolecmd_comand.Parameters("@intRequieredBy").Value = alng_Requiredby
    '            iolecmd_comand.Parameters("@intRequieredByType").Value = aint_RequiredByType
    '            iolecmd_comand.Parameters("@intCustomerId").Value = alng_Customer
    '            iolecmd_comand.Parameters("@intServiceType").Value = aint_ServiceType
    '            iolecmd_comand.Parameters("@intContainerUniversalId").Value = larg_lng_UniversalId
    '            iolecmd_comand.Parameters("@intOperationType").Value = litem_int_operation
    '            'iolecmd_comand.Parameters("@intOperationType").Value = larg_int_OperationType



    '            '' si se pone el valor de la solicitud de servicio de iteracion o actual 
    '            If larg_lng_ServiceOrderId > 0 Then
    '                iolecmd_comand.Parameters("@intServiceOrderId").Value = larg_lng_ServiceOrderId
    '            Else
    '                'actualizar el valor de la serviceorder, ya que en la primera iteeracion se obtendra el numero de solicutid 

    '                '' validar valores a asignar, primero llng_ServiceOrderId 
    '                If llng_ServiceOrderId > 0 Then
    '                    iolecmd_comand.Parameters("@intServiceOrderId").Value = llng_ServiceOrderId
    '                    larg_lng_ServiceOrderId = llng_ServiceOrderId
    '                Else
    '                    '' validar el valor de la itemorderid 
    '                    If litem_lng_ServiceOrderId > 0 Then
    '                        iolecmd_comand.Parameters("@intServiceOrderId").Value = litem_lng_ServiceOrderId
    '                        larg_lng_ServiceOrderId = litem_lng_ServiceOrderId
    '                    Else
    '                        iolecmd_comand.Parameters("@intServiceOrderId").Value = 0
    '                    End If
    '                End If


    '            End If

    '            iolecmd_comand.Parameters("@strUsername").Value = astr_UserName
    '            '''''
    '            lstr_SQL = "spSaveVisitDetailWB"
    '            'definir que tipo de comando se va a ejecutar
    '            iolecmd_comand.CommandType = CommandType.StoredProcedure
    '            iolecmd_comand.CommandText = lstr_SQL

    '            ''ejecutar 
    '            adapter = New OleDbDataAdapter(iolecmd_comand)
    '            ''''''''''''''''''''

    '            Try
    '                ''conectar
    '                iolecmd_comand.Connection.Open()
    '                adapter.Fill(ldt_TableResult)
    '                ''desconectar
    '            Catch ex As Exception
    '                lstr_Message = ObtenerError(ex.Message, 9999)
    '                If lstr_Message.Length > 0 Then
    '                    Return dt_RetrieveErrorTable(lstr_Message)
    '                Else
    '                    Return dt_RetrieveErrorTable(ex.Message)
    '                End If
    '            Finally
    '                iolecmd_comand.Connection.Close()
    '                iolecmd_comand.Connection.Dispose()
    '                'ioleconx_conexion.close()
    '            End Try

    '            ' Return lint_itemscount.ToString()
    '            iolecmd_comand = Nothing

    '            '' ver si la tabla trajo informacion 
    '            Try

    '                If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
    '                    Dim lstr_info As String
    '                    lstr_info = ldt_TableResult(0)(0).ToString
    '                    If lstr_info.Length > 1 Then
    '                        Return dt_RetrieveErrorTable(lstr_info)
    '                    Else
    '                        Return dt_RetrieveErrorTable("error vacio")
    '                    End If
    '                Else
    '                    '' se espera un renglon con 4 columnas 
    '                    If ldt_TableResult.Columns.Count = 4 Then
    '                        ' pasar las 4 columnas 
    '                        lrow_Result = ldtb_ResultData.NewRow()
    '                        lrow_Result("strContainerId") = ldt_TableResult.Rows(0)(0).ToString()
    '                        lrow_Result("intVisitId") = ldt_TableResult.Rows(0)(1).ToString()
    '                        lrow_Result("lintVisitItemId") = CType(ldt_TableResult.Rows(0)(2), Integer)
    '                        lrow_Result("intServiceOrderId") = CType(ldt_TableResult.Rows(0)(3), Long)
    '                        Try
    '                            larg_lng_ServiceOrderId = CType(ldt_TableResult.Rows(0)(3).ToString(), Long)
    '                        Catch ex As Exception
    '                            Dim lstr As String = ex.Message
    '                            lstr = lstr
    '                        End Try

    '                        ldtb_ResultData.Rows.Add(lrow_Result)

    '                    End If
    '                End If
    '            Catch ex As Exception
    '                Dim lstr_ex As String
    '                lstr_ex = ex.Message
    '                lstr_ex = lstr_ex
    '                Return dt_RetrieveErrorTable(lstr_ex)
    '                'Return dt_RetrieveErrorTable("error al actualizar informacion ")
    '            End Try

    '            ''''''''''''''''''''''


    '        Catch ex As Exception

    '        End Try

    '    Next
    '    ''''---

    '    Return ldtb_ResultData

    '    '''''''''''''''''''''''''''''''''

    'End Function
    ''---

    <WebMethod()>
    Public Function GetVisitData(ByVal alng_Visit As Long, ByVal aint_UserId As Integer) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("VisitData")
        strSQL = "spRetrieveVisitData"

        iolecmd_comand.Parameters.Add("intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters("intVisitId").Value = alng_Visit

        iolecmd_comand.Parameters.Add("intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserId").Value = aint_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()

            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    ''----

    'Public Function SaveVisitSingle(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal astr_Chofer As String, ByVal alng_Customer As Long, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal aint_ServiceType As Integer, ByVal astr_UserName As String, ByVal adtb_VisitOperation As DataTable, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_ServiceOrder As Long, ByVal astr_DriverLicence As String, ByVal aint_CustomerType As Integer) As String
    ''''-----
    <WebMethod()>
    Public Function SaveVisitSingle(ByVal alng_VisitId As Long, ByVal alng_CarrierId As Long, ByVal astr_Chofer As String, ByVal alng_Customer As Long, ByVal astr_Plates As String, ByVal astr_Reference As String, ByVal aint_ServiceType As Integer, ByVal astr_UserName As String, ByVal adtb_VisitOperation As DataTable, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Integer, ByVal alng_ServiceOrder As Long, ByVal astr_DriverLicence As String, ByVal astr_appointmentdate As String, ByVal astr_appointmentblock As String, ByVal astr_ContainerId As String, ByVal astr_DeliyveryType As String) As String

        Dim ldtb_VisitResult As DataTable
        Dim ldtb_VisitDetailResult As DataTable
        Dim lstr_VisitMasterResult As String
        Dim lint_VisitResult As Long
        Dim lint_operationCounter As Long

        ''' validar la informacion nuevamente

        ' carrier
        If alng_CarrierId = 0 Then
            Return -20
            'Return dt_RetrieveErrorTable("Se necesita Transportista")
        End If
        'chofer 
        If astr_Chofer.Length = 0 Then
            Return -21
            'Return dt_RetrieveErrorTable("Se necesita capturar chofer")
        End If
        'placas 
        If astr_Plates.Length = 0 Then
            Return -22
            'Return dt_RetrieveErrorTable("Se necesita capturar placas ")
        End If

        'facturar
        If alng_Customer = 0 Then
            Return -23
            'Return dt_RetrieveErrorTable("Se necesita capturar facturar a ")
        End If

        lint_operationCounter = 0
        'obtener contador de operaciones 
        Try
            lint_operationCounter = adtb_VisitOperation.Rows.Count
        Catch ex As Exception
            lint_operationCounter = 0
        End Try

        '     lstr_VisitMasterResult = of_SaveMasterVisit(alng_VisitId, alng_CarrierId, astr_Chofer, alng_Customer, astr_Reference, astr_Plates, aint_ServiceType, lint_operationCounter, alng_RequiredBy, aint_RequiredByType, alng_ServiceOrder, astr_DriverLicence, astr_UserName, aint_CustomerType)

        '' guardar master 
        lstr_VisitMasterResult = of_SaveMasterVisit(alng_VisitId, alng_CarrierId, astr_Chofer, alng_Customer, astr_Reference, astr_Plates, lint_operationCounter, alng_RequiredBy, aint_RequiredByType, alng_ServiceOrder, astr_DriverLicence, astr_UserName, astr_appointmentdate, astr_appointmentblock, astr_ContainerId, astr_DeliyveryType)

        If lstr_VisitMasterResult.Length = 0 Then
            'Return dt_RetrieveErrorTable("Error 3002 al guardar visita ") ' no hay numero de visita 
            Return -24
        End If
        '''' validar el visita 
        If of_HasOnlyDigits(lstr_VisitMasterResult) = False Then
            'Return dt_RetrieveErrorTable(lstr_VisitMasterResult)
            Return -25
        End If

        '' obtener el numero de visita 
        Try
            lint_VisitResult = CType(lstr_VisitMasterResult, Long)
        Catch ex As Exception
            Return -26
            ''Return dt_RetrieveErrorTable("Error 3004 al guardar visita") ' no es un valor numerico
        End Try
        ''''''''''''''''''''''''''''''

        ''  guardar detalle 
        ''''''''''''''''''''''''''''''''''''
        If lint_VisitResult > 0 Then
            lint_VisitResult = lint_VisitResult

            'Return dt_RetrieveErrorTable(lint_VisitResult.ToString())
            '' guardar detalle 
            Try
                Dim llng_VisitDetail As Long
                'If alng_VisitId > 0 Then
                '    Return "previo llamanda detail vez -" + alng_VisitId.ToString()
                'End If
                'ldtb_VisitDetailResult = of_SaveVisitDetail(lint_VisitResult, alng_Customer, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)


                'ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, aint_CustomerType, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)
                'ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, astr_Reference, 0, aint_ServiceType, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation)

                ldtb_VisitDetailResult = of_SaveVisitDetailSingle(alng_VisitId, lint_VisitResult, alng_Customer, astr_Reference, 0, astr_UserName, alng_RequiredBy, aint_RequiredByType, adtb_VisitOperation, 0, 180)
                'Return dt_RetrieveErrorTable("prev2")
                '  Return ldtb_VisitDetailResult
                'obtener el numero de visita 
                Try
                    llng_VisitDetail = CType(ldtb_VisitDetailResult(0)("intVisitId"), Long)
                    Return llng_VisitDetail
                Catch ex As Exception
                    Dim lstra As String
                    lstra = ex.Message
                    'Return lstra
                End Try

                Return ldtb_VisitDetailResult(0)(0).ToString

            Catch ex As Exception
                Dim lstr_ex As String
                lstr_ex = ex.Message
                ' Return dt_RetrieveErrorTable(lstr_ex)
                Return lstr_ex
                'Return -27
            End Try

        Else
            'Return dt_RetrieveErrorTable("Error 3005 al guardar encabezado") ' no hay numero
            Return -28
        End If

        'Return ldtb_VisitResult
        Return -29

    End Function

    <WebMethod()>
    Public Function UpdateContainerAdviceV(ByVal aint_AdviceBooking As Integer, ByVal astr_container As String, ByVal aint_ContainerType As Integer, ByVal aint_ContainerSize As Integer, ByVal aint_ContainerISOode As Integer, ByVal aint_ShippingLine As Integer, ByVal adec_VGM As Decimal, ByVal aint_Full As Integer, ByVal astr_ValidStatus As String, ByVal astr_user As String, ByVal astr_sealnumber As String, ByVal adec_NETWeight As Decimal, ByVal astr_WeigherId As String, ByVal aint_IMOCode As Integer) As String

        Dim lstr_result As String
        Dim lint_BookingAdvice As Integer

        Dim lrow As DataRow


        Dim ldt_AdviceBookingResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnAdviceBRow As DataRow
        Dim ldt_resultdt As DataTable = New DataTable("dresult")

        '' crear tabla de resultados 
        Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
        Dim lrow_Result As DataRow
        Dim lstr_serviceType As String
        Dim lstr_Valid As String

        ldtb_ResultData.TableName = "DataResult"
        ldtb_ResultData.Columns.Add("intBookingAdviceId", GetType(Integer))
        ldtb_ResultData.Columns.Add("strContainerId", GetType(String))


        lstr_result = ""

        '' primera validacion 
        ' If aobj_Advice.iint_AdviceId = 0 Then
        ''-----------------
        'Dim llng_ServiceOrderId As Long

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        '' validaciones --->
        '' numero de aviso
        If aint_AdviceBooking = 0 Then
            Return "No existe numero de aviso "
        End If


        '' tabla
        Try
            ' solo validar el nombre del contenedor 
            If astr_container.Length = 0 Then
                Return "no esta el contenedor "
            End If

        Catch ex As Exception
            Return ""
        End Try


        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0

        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerISOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@decVGM", OleDbType.Decimal)

        iolecmd_comand.Parameters.Add("@blnContainerIsFull", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsValidItem", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blIsValidByShipper", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnIsFromCalathus", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strSealNumber ", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@decWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@strWeigherId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strISOCodeText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strComments", OleDbType.Char)

        'If aorigint_visit > 0 And alng_Visit > 0 Then
        '    Return dt_RetrieveErrorTable("-bloque CUAT ")
        'End If

        ''crear 
        Dim adapter As OleDbDataAdapter
        Dim lint_counter As Integer = 0
        Dim lint_idx As Integer = 0


        If astr_ValidStatus <> "VALID" And astr_ValidStatus <> "REJECT" Then
            astr_ValidStatus = "PENDVAL"
        End If

        Try


            iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_AdviceBooking
            iolecmd_comand.Parameters("@strContainerId").Value = astr_container
            iolecmd_comand.Parameters("@intContainerSize").Value = aint_ContainerSize
            iolecmd_comand.Parameters("@intContainerType").Value = aint_ContainerType
            iolecmd_comand.Parameters("@intContainerISOCode").Value = aint_ContainerISOode
            iolecmd_comand.Parameters("@intShippingLine").Value = aint_ShippingLine
            iolecmd_comand.Parameters("@decVGM").Value = adec_VGM

            iolecmd_comand.Parameters("@blnContainerIsFull").Value = aint_Full
            iolecmd_comand.Parameters("@intOperation").Value = 2
            iolecmd_comand.Parameters("@blnIsValidItem").Value = astr_ValidStatus
            iolecmd_comand.Parameters("@blIsValidByShipper").Value = -1
            iolecmd_comand.Parameters("@blnIsFromCalathus").Value = -1
            iolecmd_comand.Parameters("@strUsername").Value = astr_user
            iolecmd_comand.Parameters("@strSealNumber ").Value = astr_sealnumber
            iolecmd_comand.Parameters("@decWeight").Value = adec_NETWeight
            iolecmd_comand.Parameters("@strWeigherId").Value = astr_WeigherId
            iolecmd_comand.Parameters("@strISOCodeText").Value = ""
            iolecmd_comand.Parameters("@strComments").Value = ""

            '''''
            lstr_SQL = "spSaveContainerBookingAdv"
            'definir que tipo de comando se va a ejecutar
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandText = lstr_SQL

            ''ejecutar 
            adapter = New OleDbDataAdapter(iolecmd_comand)
            ''''''''''''''''''''

            Try
                ''conectar
                iolecmd_comand.Connection.Open()
                'If lint_counter > 0 Then
                '    Return dt_RetrieveErrorTable("ya paso counter=" + lint_counter.ToString())
                'End If
                adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
                adapter.Fill(ldt_resultdt)
                ''desconectar
            Catch ex As Exception

                lstr_Message = ObtenerError(ex.Message, 9999)
                If lstr_Message.Length > 0 Then
                    Return lstr_Message
                Else
                    Return ex.Message
                End If
            Finally
                iolecmd_comand.Connection.Close()
                ' iolecmd_comand.Connection.Dispose()
                'ioleconx_conexion.close()
            End Try


            'iolecmd_comand = Nothing

            '' ver si la tabla trajo informacion 
            Try

                If ldt_resultdt.Rows.Count = 1 And ldt_resultdt.Columns.Count = 1 Then
                    Dim lstr_info As String
                    Dim lstr_Value As String

                    '' probar si es la columna de aviso 
                    'Try
                    '    lstr_Value = ldt_resultdt(0)("intBookingAdviceId").ToString()

                    '    If lstr_Value.Length > 0 Then
                    '        lrow_Result = ldtb_ResultData.NewRow()
                    '        lrow_Result("intBookingAdviceId") = lstr_Value
                    '        lrow_Result("strContainerId") = ""
                    '        ldtb_ResultData.Rows.Add(lrow_Result)

                    '    End If
                    'Catch ex As Exception

                    'End Try
                    ' sino hay contenedot
                    If lstr_Value.Length = 0 Then

                        lstr_info = ldt_resultdt(0)(0).ToString
                        If lstr_info.Length > 1 Then
                            Return lstr_info
                        Else
                            Return "error vacio"
                        End If

                    End If

                Else
                    '' se espera un renglon con 2 columnas 
                    If ldt_resultdt.Columns.Count > 1 Then
                        ' pasar las 4 columnas 
                        lrow_Result = ldtb_ResultData.NewRow()
                        lrow_Result("intBookingAdviceId") = ldt_resultdt.Rows(0)(0).ToString()
                        lrow_Result("strContainerId") = ldt_resultdt.Rows(0)(1).ToString()

                        ldtb_ResultData.Rows.Add(lrow_Result)

                    End If
                End If
            Catch ex As Exception
                Dim lstr_ex As String
                lstr_ex = ex.Message
                lstr_ex = lstr_ex
                Return lstr_ex
                'Return dt_RetrieveErrorTable("error al actualizar informacion ")
            End Try

            ''''''''''''''''''''''


        Catch ex As Exception

        End Try

        '' si tiene imo
        If aint_IMOCode > 0 Then

            Try
                Dim lstr_masterresult As String

                lstr_masterresult = of_UpdateMasterAdviceV(aint_AdviceBooking, "", -1, -1, "", "19000101 00:00", -1, -1, -1, -1, aint_IMOCode, 0, "", "", "IGNOREST", astr_user, -1)

            Catch ex As Exception
                lstr_result = ex.Message
            End Try

        End If '' si tiene codigo imo


        'Return dt_RetrieveErrorTable("Tablaxvv")
        Return lstr_result

    End Function

    ' Public Function of_SaveVisitDetailSingle(ByVal aorigint_visit As Long, ByVal alng_Visit As Long, ByVal alng_Customer As Long, ByVal aint_CustomerType As Integer, ByVal astr_Reference As String, ByVal aint_operation As Integer, ByVal aint_ServiceType As Integer, ByVal astr_UserName As String, ByVal alng_Requiredby As Long, ByVal aint_RequiredByType As Integer, ByVal adtb_VisitOperations As DataTable) As DataTable
    Public Function of_SaveVisitDetailSingle(ByVal aorigint_visit As Long, ByVal alng_Visit As Long, ByVal alng_Customer As Long, ByVal astr_Reference As String, ByVal aint_ServiceType As Integer, ByVal astr_UserName As String, ByVal alng_Requiredby As Long, ByVal aint_RequiredByType As Integer, ByVal adtb_VisitOperations As DataTable, ByVal alng_ServiceOrderId As Long, ByVal aint_timeout As Integer) As DataTable



        ''''''''''''''''''''''''''
        '-----------------------------
        'Return dt_RetrieveErrorTable("start0 bloque")
        Dim ldt_VisitResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow

        Dim larg_lng_RequiredBy As Long
        Dim larg_lng_RequiredByType As Long
        Dim larg_int_ServiceType As Integer
        Dim larg_lng_UniversalId As Long
        Dim larg_int_OperationType As Integer
        Dim larg_lng_VisitId As Long
        Dim larg_lng_ServiceOrderId As Long
        Dim litem_lng_ServiceOrderId As Long
        Dim litem_int_VisitItemdId As Long
        Dim litem_int_operation As Long

        '' crear tabla de resultados 
        Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
        Dim lrow_Result As DataRow
        Dim lstr_serviceType As String
        Dim llng_lastServiceOrderId As Long = 0
        Dim lstr_lastServicename As String = ""
        Dim lint_serviceCounter As Integer = 0
        Dim lint_blankServiceOrderCounter As Integer = 0
        Dim lint_lastoperationType As Integer = 0
        Dim lint_readValue As Integer = 0
        Dim llng_readValue As Long = 0
        Dim lstr_readValue As String = ""
        Dim lint_serviceTypeCounter As Integer = 0
        Dim lint_operationCounter As Integer = 0


        ldtb_ResultData.TableName = "DataResult"
        ldtb_ResultData.Columns.Add("strContainerId", GetType(String))
        ldtb_ResultData.Columns.Add("intVisitId", GetType(Long))
        ldtb_ResultData.Columns.Add("lintVisitItemId", GetType(Integer))
        ldtb_ResultData.Columns.Add("intServiceOrderId", GetType(Integer))

        ''''''''''''''''''''''''''''''''''''''''''''''''
        ''' primer bloque 

        ' Return dt_RetrieveErrorTable("primer bloque")



        'Dim llng_ServiceOrderId As Long

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultVisit"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        '' validaciones --->
        '' numero de visita 
        If alng_Visit = 0 Then
            dt_RetrieveErrorTable("No existe numero de visita")
        End If


        '' tabla
        Try
            If adtb_VisitOperations Is Nothing Then
                Return dt_RetrieveErrorTable(alng_Visit.ToString)
            End If

            If (adtb_VisitOperations.Rows.Count() = 0 Or adtb_VisitOperations.Columns.Count() < 2) Then
                Return dt_RetrieveErrorTable(alng_Visit.ToString)
                'dt_RetrieveErrorTable("No hay informacion a procesar ")
            End If
        Catch ex As Exception
            Return dt_RetrieveErrorTable(alng_Visit.ToString)
        End Try


        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intRequieredBy", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intRequieredByType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        ' iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strServiceType", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperationType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intServiceOrderId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)

        'If aorigint_visit > 0 And alng_Visit > 0 Then
        '    Return dt_RetrieveErrorTable("-bloque CUAT ")
        'End If

        '' recorrer tabla para saber si se reusa una maniobra
        For Each dataelement As DataRow In adtb_VisitOperations.Rows
            Try
                'obtener el numero de servicio
                If Long.TryParse(dataelement("intServiceOrderId"), llng_readValue) = False Then
                    llng_readValue = 0
                End If

                If llng_readValue = 0 Then
                    lint_blankServiceOrderCounter = lint_blankServiceOrderCounter + 1
                Else
                    If llng_readValue <> llng_lastServiceOrderId Then
                        lint_serviceCounter = lint_serviceCounter + 1
                    End If
                    'servicio 0
                    If llng_lastServiceOrderId = 0 Then
                        llng_lastServiceOrderId = llng_readValue
                    End If
                End If

                ' obtener el servicio
                If dataelement("strServiceType").ToString.Length > 1 Then
                    lstr_readValue = dataelement("strServiceType").ToString()
                    If lstr_readValue <> lstr_lastServicename Then
                        If lstr_lastServicename.Length < 1 Then
                            lstr_lastServicename = lstr_readValue
                        End If
                        lint_serviceTypeCounter = lint_serviceTypeCounter + 1
                    End If
                End If

                'obtener tipo de operacion 

                If Integer.TryParse(dataelement("intoperation"), lint_readValue) = False Then
                    lint_readValue = 0
                End If

                If lint_readValue = 0 Then
                    lint_readValue = 0
                Else
                    If lint_readValue <> lint_lastoperationType Then
                        lint_operationCounter = lint_operationCounter + 1

                    End If
                    'servicio 0
                    If lint_lastoperationType = 0 Then
                        lint_lastoperationType = lint_readValue
                    End If
                End If


            Catch ex As Exception

            End Try
        Next


        '' revisar los contadores 
        'si hay espacios blancos y si sola hay una maniobra, en total, reemplazar , y un contador de servicio ,reemplazar
        If lint_blankServiceOrderCounter > 0 And lint_serviceCounter = 1 And lint_operationCounter = 1 Then

            'marcar todos los servicios con la misma maniobra 
            For Each dataelement As DataRow In adtb_VisitOperations.Rows

                dataelement("intServiceOrderId") = llng_lastServiceOrderId
            Next

        End If


        ''crear 
        Dim adapter As OleDbDataAdapter
        Dim lint_counter As Integer = 0
        iolecmd_comand.CommandTimeout = aint_timeout
        For Each dataelement As DataRow In adtb_VisitOperations.Rows
            Try

                'larg_lng_VisitId = 
                larg_lng_UniversalId = CType(dataelement("intContainerUniversalId"), Long)
                litem_lng_ServiceOrderId = CType(dataelement("intServiceOrderId"), Long)
                litem_int_operation = CType(dataelement("intoperation"), Long)
                lstr_serviceType = CType(dataelement("strServiceType"), String)

                'If lint_counter > 0 Then
                '    'Return dt_RetrieveErrorTable("va a executar sigue de item=" + lint_counter.ToString())
                '    Return dt_RetrieveErrorTable("prepater after count=" + lint_counter.ToString())
                'End If

                '' asignar los valores  '' agregar valores
                iolecmd_comand.Parameters("@intVisitId").Value = alng_Visit
                iolecmd_comand.Parameters("@intRequieredBy").Value = alng_Requiredby
                iolecmd_comand.Parameters("@intRequieredByType").Value = aint_RequiredByType
                iolecmd_comand.Parameters("@intCustomerId").Value = alng_Customer
                'iolecmd_comand.Parameters("@intCustomerType").Value = aint_CustomerType
                iolecmd_comand.Parameters("@strServiceType").Value = lstr_serviceType 'aint_ServiceType
                iolecmd_comand.Parameters("@intContainerUniversalId").Value = larg_lng_UniversalId
                iolecmd_comand.Parameters("@intOperationType").Value = litem_int_operation
                'iolecmd_comand.Parameters("@intOperationType").Value = larg_int_OperationType

                '' si se pone el valor de la solicitud de servicio de iteracion o actual 
                'If larg_lng_ServiceOrderId > 0 Then
                '    iolecmd_comand.Parameters("@intServiceOrderId").Value = larg_lng_ServiceOrderId
                '    larg_lng_ServiceOrderId = llng_ServiceOrderId

                'agregado para tomar en cuenta la maniobra como parametro 25-11-2019
                If alng_ServiceOrderId > 0 Then
                    iolecmd_comand.Parameters("@intServiceOrderId").Value = alng_ServiceOrderId
                    larg_lng_ServiceOrderId = 0
                End If

                If litem_lng_ServiceOrderId > 0 Then
                    iolecmd_comand.Parameters("@intServiceOrderId").Value = litem_lng_ServiceOrderId
                    larg_lng_ServiceOrderId = litem_lng_ServiceOrderId
                    llng_ServiceOrderId = litem_lng_ServiceOrderId
                Else
                    'actualizar el valor de la serviceorder, ya que en la primera iteeracion se obtendra el numero de solicutid 
                    ' validar variables llng_ServiceOrderId
                    If larg_lng_ServiceOrderId > 0 Then
                        iolecmd_comand.Parameters("@intServiceOrderId").Value = larg_lng_ServiceOrderId
                        llng_ServiceOrderId = larg_lng_ServiceOrderId

                    Else
                        If litem_lng_ServiceOrderId > 0 Then
                            iolecmd_comand.Parameters("@intServiceOrderId").Value = litem_lng_ServiceOrderId
                            larg_lng_ServiceOrderId = litem_lng_ServiceOrderId
                        Else
                            larg_lng_ServiceOrderId = 0
                            llng_ServiceOrderId = 0
                        End If
                    End If

                End If

                iolecmd_comand.Parameters("@strUsername").Value = astr_UserName

                '''''
                lstr_SQL = "spSaveVisitDetailWB"
                'definir que tipo de comando se va a ejecutar
                iolecmd_comand.CommandType = CommandType.StoredProcedure
                iolecmd_comand.CommandText = lstr_SQL

                ''ejecutar 
                adapter = New OleDbDataAdapter(iolecmd_comand)
                ''''''''''''''''''''

                Try
                    ''conectar
                    iolecmd_comand.Connection.Open()
                    'If lint_counter > 0 Then
                    '    Return dt_RetrieveErrorTable("ya paso counter=" + lint_counter.ToString())
                    'End If
                    adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()

                    adapter.Fill(ldt_TableResult)
                    ''desconectar
                Catch ex As Exception

                    lstr_Message = ObtenerError(ex.Message, 9999)
                    If lstr_Message.Length > 0 Then
                        Return dt_RetrieveErrorTable(lstr_Message)
                    Else
                        Return dt_RetrieveErrorTable(ex.Message)
                    End If
                Finally
                    iolecmd_comand.Connection.Close()
                    ' iolecmd_comand.Connection.Dispose()
                    'ioleconx_conexion.close()
                End Try


                'iolecmd_comand = Nothing

                '' ver si la tabla trajo informacion 
                Try

                    If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                        Dim lstr_info As String
                        lstr_info = ldt_TableResult(0)(0).ToString
                        If lstr_info.Length > 1 Then
                            Return dt_RetrieveErrorTable(lstr_info)
                        Else
                            Return dt_RetrieveErrorTable("error vacio")
                        End If
                    Else
                        '' se espera un renglon con 4 columnas 
                        If ldt_TableResult.Columns.Count = 4 Then
                            ' pasar las 4 columnas 
                            lrow_Result = ldtb_ResultData.NewRow()
                            lrow_Result("strContainerId") = ldt_TableResult.Rows(0)(0).ToString()
                            lrow_Result("intVisitId") = ldt_TableResult.Rows(0)(1).ToString()
                            lrow_Result("lintVisitItemId") = CType(ldt_TableResult.Rows(0)(2), Integer)
                            lrow_Result("intServiceOrderId") = CType(ldt_TableResult.Rows(0)(3), Long)
                            Try
                                larg_lng_ServiceOrderId = CType(ldt_TableResult.Rows(0)(3).ToString(), Long)
                            Catch ex As Exception
                                Dim lstr As String = ex.Message
                                lstr = lstr
                            End Try

                            ldtb_ResultData.Rows.Add(lrow_Result)

                            ' agregados 
                            lint_counter = lint_counter + 1

                        End If
                    End If
                Catch ex As Exception
                    Dim lstr_ex As String
                    lstr_ex = ex.Message
                    lstr_ex = lstr_ex
                    Return dt_RetrieveErrorTable(lstr_ex)
                    'Return dt_RetrieveErrorTable("error al actualizar informacion ")
                End Try

                ''''''''''''''''''''''


            Catch ex As Exception

            End Try

        Next
        ''''---
        'Return dt_RetrieveErrorTable("counter=" + lint_counter.ToString())

        Try
            iolecmd_comand.Connection.Dispose()
            iolecmd_comand = Nothing
        Catch ex As Exception

        End Try
        ''''''''
        Return ldtb_ResultData

        '''''''''''''''''''''''''''''''''

    End Function


    '' inicio visita de entrega
    'Public Function of_SaveVisitDetailSingle_REC(ByVal aorigint_visit As Long, ByVal alng_Visit As Long, ByVal alng_Customer As Long, ByVal astr_Reference As String, ByVal aint_ServiceType As Integer, ByVal astr_UserName As String, ByVal alng_Requiredby As Long, ByVal aint_RequiredByType As Integer, ByVal adtb_VisitOperations As DataTable, ByVal alng_ServiceOrderId As Long) As DataTable
    Public Function of_SaveVisitDetailSingle_REC(ByVal aobj_Visit As ClsReceptionData) As DataTable

        ' Return dt_RetrieveErrorTable("inicio rec")
        ''''''''''''''''''''''''''
        '-----------------------------
        'Return dt_RetrieveErrorTable("start0 bloque")
        Dim ldt_VisitResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow

        Dim larg_lng_RequiredBy As Long
        Dim larg_lng_RequiredByType As Long
        Dim larg_int_ServiceType As Integer
        Dim larg_int_OperationType As Integer
        Dim larg_lng_VisitId As Long
        Dim larg_lng_ServiceOrderId As Long
        Dim litem_lng_ServiceOrderId As Long
        Dim litem_int_VisitItemdId As Long
        Dim litem_int_operation As Long
        Dim lint_tempbk As Long
        Dim lstr_tempbk As String

        '' crear tabla de resultados 
        Dim ldtb_ResultData As DataTable = New DataTable("DataResult")
        Dim lrow_Result As DataRow
        Dim lstr_serviceType As String

        Dim lstr_VGM As Decimal
        Dim lstr_strBookingExtra As String



        ldtb_ResultData.TableName = "DataResult"
        ldtb_ResultData.Columns.Add("strContainerId", GetType(String))
        ldtb_ResultData.Columns.Add("VisitId", GetType(Long))
        ldtb_ResultData.Columns.Add("VisitItemId", GetType(Integer))
        ldtb_ResultData.Columns.Add("ServiceOrderId", GetType(Integer))

        ''''''''''''''''''''''''''''''''''''''''''''''''
        ''' primer bloque 

        ' Return dt_RetrieveErrorTable("primer bloque")



        'Dim llng_ServiceOrderId As Long

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultVisit"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        '' validaciones --->
        '' numero de visita 
        If aobj_Visit.ilng_VisitId = 0 Then
            dt_RetrieveErrorTable("No existe numero de visita")
        End If


        '' tabla
        Try
            If aobj_Visit.iobjs_VContainers Is Nothing Then
                Return dt_RetrieveErrorTable(aobj_Visit.ilng_VisitId)
            End If

            If (aobj_Visit.iobjs_VContainers.Length = 0) Then
                Return dt_RetrieveErrorTable(aobj_Visit.ilng_VisitId)
                'dt_RetrieveErrorTable("No hay informacion a procesar ")
            End If
        Catch ex As Exception
            Return dt_RetrieveErrorTable(aobj_Visit.ilng_VisitId)
        End Try


        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intRequieredBy", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intRequieredByType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        ' iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strServiceType", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperationType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intServiceOrderId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intBookingAvd", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerISOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strReference", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strSealNumber", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@decWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@strStockBooking", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@decVGM", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@strWeigher", OleDbType.Char)


        'If aorigint_visit > 0 And alng_Visit > 0 Then
        '    Return dt_RetrieveErrorTable("-bloque CUAT ")
        'End If

        ''crear 
        Dim adapter As OleDbDataAdapter
        Dim lint_counter As Integer = 0
        Dim lint_done As Integer = 0

        ' Return dt_RetrieveErrorTable(" incount=" + aobj_Visit.iobjs_VContainers.Length.ToString())

        'Return dt_RetrieveErrorTable("antes del ciclo")

        For Each dataelement As ClsVisitContainer In aobj_Visit.iobjs_VContainers
            Try


                litem_lng_ServiceOrderId = aobj_Visit.ilng_serviceOrder
                litem_int_operation = dataelement.iint_Operation
                lstr_serviceType = aobj_Visit.istr_service

                'If lint_counter > 0 Then
                '    'Return dt_RetrieveErrorTable("va a executar sigue de item=" + lint_counter.ToString())
                '    Return dt_RetrieveErrorTable("prepater after count=" + lint_counter.ToString())
                'End If

                '' asignar los valores  '' agregar valores
                iolecmd_comand.Parameters("@intVisitId").Value = aobj_Visit.ilng_VisitId
                iolecmd_comand.Parameters("@intRequieredBy").Value = aobj_Visit.ilng_RequiredBy
                iolecmd_comand.Parameters("@intRequieredByType").Value = aobj_Visit.iint_RequiredByType
                iolecmd_comand.Parameters("@intCustomerId").Value = aobj_Visit.ilng_Customer
                'iolecmd_comand.Parameters("@intCustomerType").Value = aint_CustomerType
                iolecmd_comand.Parameters("@strServiceType").Value = aobj_Visit.istr_service 'aint_ServiceType
                iolecmd_comand.Parameters("@intOperationType").Value = litem_int_operation
                'iolecmd_comand.Parameters("@intOperationType").Value = larg_int_OperationType


                ''

                iolecmd_comand.Parameters("@intVisitId").Value = aobj_Visit.ilng_VisitId
                iolecmd_comand.Parameters("@intRequieredBy").Value = aobj_Visit.ilng_RequiredBy
                iolecmd_comand.Parameters("@intRequieredByType").Value = aobj_Visit.iint_RequiredByType
                iolecmd_comand.Parameters("@intCustomerId").Value = aobj_Visit.ilng_Customer

                ' iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
                iolecmd_comand.Parameters("@strServiceType").Value = aobj_Visit.istr_service
                iolecmd_comand.Parameters("@strContainerId").Value = dataelement.istr_Contenedor
                iolecmd_comand.Parameters("@intShippingLine").Value = dataelement.iint_ShippingLine
                iolecmd_comand.Parameters("@intOperationType").Value = dataelement.iint_Operation
                iolecmd_comand.Parameters("@intServiceOrderId").Value = aobj_Visit.ilng_serviceOrder
                iolecmd_comand.Parameters("@intBookingAvd").Value = aobj_Visit.iint_booking
                iolecmd_comand.Parameters("@intContainerType").Value = dataelement.iint_ContainerType
                iolecmd_comand.Parameters("@intContainerSize").Value = dataelement.iint_ContainerSize
                iolecmd_comand.Parameters("@intContainerISOCode").Value = dataelement.iint_ISOCOde
                iolecmd_comand.Parameters("@strReference").Value = dataelement.istr_ItemReference
                iolecmd_comand.Parameters("@strUsername").Value = aobj_Visit.istr_UserName
                iolecmd_comand.Parameters("@strSealNumber").Value = dataelement.istr_SealNumber
                iolecmd_comand.Parameters("@decWeight").Value = dataelement.idec_NetWeight

                iolecmd_comand.Parameters("@strStockBooking").Value = aobj_Visit.istr_STOCKBoking
                iolecmd_comand.Parameters("@decVGM").Value = dataelement.idec_VGM

                iolecmd_comand.Parameters("@strWeigher").Value = dataelement.istr_WeigherId



                '' si se pone el valor de la solicitud de servicio de iteracion o actual 
                'If larg_lng_ServiceOrderId > 0 Then
                '    iolecmd_comand.Parameters("@intServiceOrderId").Value = larg_lng_ServiceOrderId
                '    larg_lng_ServiceOrderId = llng_ServiceOrderId


                If litem_lng_ServiceOrderId > 0 Then
                    iolecmd_comand.Parameters("@intServiceOrderId").Value = litem_lng_ServiceOrderId
                    larg_lng_ServiceOrderId = litem_lng_ServiceOrderId
                    llng_ServiceOrderId = litem_lng_ServiceOrderId
                Else
                    'actualizar el valor de la serviceorder, ya que en la primera iteeracion se obtendra el numero de solicutid 
                    ' validar variables llng_ServiceOrderId
                    If larg_lng_ServiceOrderId > 0 Then
                        iolecmd_comand.Parameters("@intServiceOrderId").Value = larg_lng_ServiceOrderId
                        llng_ServiceOrderId = larg_lng_ServiceOrderId

                    Else
                        If litem_lng_ServiceOrderId > 0 Then
                            iolecmd_comand.Parameters("@intServiceOrderId").Value = litem_lng_ServiceOrderId
                            larg_lng_ServiceOrderId = litem_lng_ServiceOrderId
                        Else
                            larg_lng_ServiceOrderId = 0
                            llng_ServiceOrderId = 0
                        End If
                    End If

                End If

                iolecmd_comand.Parameters("@strUsername").Value = aobj_Visit.istr_UserName

                '''''
                lstr_SQL = "spSaveVisitDetRecWB"
                'definir que tipo de comando se va a ejecutar
                iolecmd_comand.CommandType = CommandType.StoredProcedure
                iolecmd_comand.CommandText = lstr_SQL

                ''ejecutar 
                adapter = New OleDbDataAdapter(iolecmd_comand)
                ''''''''''''''''''''

                Try
                    ''conectar
                    iolecmd_comand.Connection.Open()

                    'If lint_counter > 0 Then
                    '    Return dt_RetrieveErrorTable("ya paso counter=" + lint_counter.ToString())
                    'End If
                    adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
                    adapter.Fill(ldt_TableResult)
                    ''desconectar
                Catch ex As Exception

                    lstr_Message = ObtenerError(ex.Message, 9999)
                    If lstr_Message.Length > 0 Then
                        Return dt_RetrieveErrorTable(lstr_Message)
                    Else
                        Return dt_RetrieveErrorTable(ex.Message)
                    End If
                Finally
                    iolecmd_comand.Connection.Close()
                    ' iolecmd_comand.Connection.Dispose()
                    'ioleconx_conexion.close()
                End Try


                'iolecmd_comand = Nothing

                '' ver si la tabla trajo informacion 
                Try

                    If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                        Dim lstr_info As String
                        lstr_info = ldt_TableResult(0)(0).ToString
                        If lstr_info.Length > 1 Then
                            Return dt_RetrieveErrorTable(lstr_info)
                        Else
                            Return dt_RetrieveErrorTable("error vacio")
                        End If
                    Else
                        '' se espera un renglon con 4 columnas 
                        If ldt_TableResult.Columns.Count > 4 Then
                            ' pasar las 4 columnas 
                            lrow_Result = ldtb_ResultData.NewRow()
                            lrow_Result("strContainerId") = ldt_TableResult.Rows(0)(0).ToString()
                            lrow_Result("VisitId") = ldt_TableResult.Rows(0)(1).ToString()
                            lrow_Result("VisitItemId") = CType(ldt_TableResult.Rows(0)(2), Integer)
                            lrow_Result("ServiceOrderId") = CType(ldt_TableResult.Rows(0)(3), Long)
                            Try
                                larg_lng_ServiceOrderId = CType(ldt_TableResult.Rows(0)(3).ToString(), Long)
                            Catch ex As Exception
                                Dim lstr As String = ex.Message
                                lstr = lstr
                            End Try

                            ldtb_ResultData.Rows.Add(lrow_Result)


                            ' agregados 
                            lint_counter = lint_counter + 1

                            'Return dt_RetrieveErrorTable("(0)=" + ldt_TableResult.Rows(0)(0).ToString() + "(1)=" + ldt_TableResult.Rows(0)(1).ToString() + "(2)=" + ldt_TableResult(0)(2).ToString() + "3=" + ldt_TableResult(0)(3).ToString())

                            '' si el servicio es recvos
                            Try


                                If aobj_Visit.istr_service = "RECVOS" Then

                                    'obtener la 5qiuta columna de id de booking
                                    'lstr_tempbk = ldt_TableResult.Rows(0)(5).ToString()

                                    Try
                                        lstr_tempbk = ldt_TableResult.Rows(0)("intBookingAvd").ToString()
                                    Catch ex As Exception

                                    End Try



                                    If Integer.TryParse(lstr_tempbk, lint_tempbk) = False Then
                                        lint_tempbk = 0
                                    End If

                                    If lint_tempbk > 0 Then
                                        aobj_Visit.iint_booking = lint_tempbk
                                        'Return dt_RetrieveErrorTable(aobj_Visit.iint_booking)
                                    End If

                                End If ' If aobj_Visit.istr_service = "RECVOS" Then


                            Catch ex As Exception

                            End Try '' '' si el servicio es recvos


                        End If
                    End If
                Catch ex As Exception
                    Dim lstr_ex As String
                    lstr_ex = ex.Message
                    lstr_ex = lstr_ex
                    Return dt_RetrieveErrorTable(lstr_ex)
                    'Return dt_RetrieveErrorTable("error al actualizar informacion ")
                End Try

                ''''''''''''''''''''''


            Catch ex As Exception
                Return dt_RetrieveErrorTable(ex.Message)
            End Try

        Next
        ''''---
        'Return dt_RetrieveErrorTable("counter=" + lint_counter.ToString())

        Try
            iolecmd_comand.Connection.Dispose()
            iolecmd_comand = Nothing
        Catch ex As Exception

        End Try

        'Return dt_RetrieveErrorTable("resultcount=" + lint_counter.ToString())
        ''''''''
        'Return dt_RetrieveErrorTable(ldtb_ResultData.Rows.Count().ToString())
        Return ldtb_ResultData

        '''''''''''''''''''''''''''''''''

    End Function

    '''''''' fin visita de entrega 

    ''''''''''
    '''''''''''''''''

    ''---
    <WebMethod()>
    Public Function SearchUserWB(ByVal astr_UserName As String, ByVal astr_EncriptPassword As String) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        If (astr_UserName.Length >= 1 And astr_EncriptPassword.Length > 2) Then

            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx
            iolecmd_comand = ioleconx_conexion.CreateCommand()

            ldtb_Result = New DataTable("User")
            strSQL = "spGetUserCaptureWEB"

            iolecmd_comand.Parameters.Add("@strUserName", OleDbType.Char)
            iolecmd_comand.Parameters("@strUserName").Value = astr_UserName

            iolecmd_comand.Parameters.Add("@strUserPassword", OleDbType.Char)
            iolecmd_comand.Parameters("@strUserPassword").Value = astr_EncriptPassword

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_Result)
            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
            Finally
                ioleconx_conexion.Close()
            End Try

            ''validacion de permisos
            Try
                Dim lint_user As Integer = 0
                Dim ldset_OptionsWEB As DataSet = New DataSet("setresult")

                lint_user = Convert.ToInt32(ldtb_Result(0)("intUserId"))

                ldset_OptionsWEB = GetUserMenu(lint_user)

                If ldset_OptionsWEB.Tables.Count > 0 Then
                    If ldset_OptionsWEB.Tables(0).Rows.Count < 2 And lint_user > 0 Then
                        'ldtb_Result = dt_RetrieveErrorTable("no tiene perfil adecuado")
                        ldtb_Result = New DataTable("empty")
                    End If
                Else
                    'ldtb_Result = dt_RetrieveErrorTable("no tiene perfil adecuado")
                    ldtb_Result = New DataTable("empty")
                End If

            Catch ex As Exception
                Dim strError As String = ex.Message

            End Try

            '    Return ldtb_Result
            'Else
            '    Return ldtb_Result

        End If

        Return ldtb_Result

    End Function
    ''---
    ''''-----
    <WebMethod()>
    Public Function GetVisitReport(ByVal alng_Visit As Long) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("VisitData")
        strSQL = "spRptVisitWEB"

        iolecmd_comand.Parameters.Add("intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters("intVisitId").Value = alng_Visit

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    ''''---

    <WebMethod()>
    Public Function GetAppointmentList(ByVal alng_Visit As Long, ByVal adtm_datestart As Date, ByVal alng_UserId As Long) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lnew_Row As DataRow
        Dim lstr_Date As String
        Dim lstr_dateStart As String
        Dim ldtb_DateCheck As DataTable
        Dim ldtb_ReturnTable = New DataTable()
        ldtb_ReturnTable = New DataTable("ErrorTable")
        ldtb_ReturnTable.Columns.Add("Date", GetType(String))
        ldtb_ReturnTable.Columns.Add("Year", GetType(String))
        ldtb_ReturnTable.Columns.Add("Month", GetType(String))
        ldtb_ReturnTable.Columns.Add("Day", GetType(String))
        ldtb_ReturnTable.Columns.Add("Hour", GetType(String))
        ldtb_ReturnTable.Columns.Add("Minute", GetType(String))


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("AppointmentList")
        strSQL = "spGetAppointmentDate"

        'obtener el valor de la fecha 
        lstr_dateStart = of_ConvertDateToStringGeneralFormat(adtm_datestart)

        iolecmd_comand.Parameters.Add("intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters("intVisitId").Value = alng_Visit

        iolecmd_comand.Parameters.Add("dtmAppointDate", OleDbType.VarChar)
        iolecmd_comand.Parameters("dtmAppointDate").Value = lstr_dateStart


        iolecmd_comand.Parameters.Add("intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserId").Value = alng_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        'retornar la tabla sin analizar
        Return ldtb_Result

        ''''''''''''''''''''''''''''''''

        If ldtb_Result.Rows.Count > 0 Then
            For Each lrow_result As DataRow In ldtb_Result.Rows
                lnew_Row = ldtb_ReturnTable.NewRow()
                lnew_Row("Date") = lrow_result(0)
                lstr_Date = lrow_result(0)

                If lstr_Date.Length > 0 Then
                    '' revisar la cadena de texto 
                    ldtb_DateCheck = of_GetDateDivideTable(lstr_Date)

                    If ldtb_DateCheck.Rows.Count > 2 Then
                        lnew_Row("Year") = ldtb_DateCheck.Rows(2)(0)
                        lnew_Row("Month") = ldtb_DateCheck.Rows(1)(0)
                        lnew_Row("Day") = ldtb_DateCheck.Rows(0)(0)
                        lnew_Row("Hour") = ldtb_DateCheck.Rows(3)(0)
                        lnew_Row("Minute") = ldtb_DateCheck.Rows(4)(0)

                    End If

                End If
                ldtb_ReturnTable.Rows.Add(lnew_Row)

            Next
            If ldtb_ReturnTable.Rows.Count > 0 Then
                Return ldtb_ReturnTable
            Else
                Return ldtb_Result
            End If

        Else
            Return ldtb_Result
        End If
        Return ldtb_Result

    End Function

    ''''-----
    Public Function of_GetDateDivideTable(ByVal dtm_date As String) As DataTable
        Dim ldt_TableResult As DataTable
        Dim lrw_Row As DataRow
        Dim lstr_Read As String = ""
        Dim lstr_Inc As String = ""

        ldt_TableResult = New DataTable("ErrorTable")
        ldt_TableResult.Columns.Add("number", GetType(String))

        lstr_Read = ""
        '' recorrer cada caracter de la cadena, si ya no es digito, agregar el registro
        For Each itemchar As Char In dtm_date
            If Char.IsDigit(itemchar) = True Then
                lstr_Read = lstr_Read + itemchar
            Else
                '' si hay acumlado, agregar y reiniciar 
                If lstr_Read.Length > 0 Then
                    lrw_Row = ldt_TableResult.NewRow()
                    lrw_Row("number") = lstr_Read
                    ldt_TableResult.Rows.Add(lrw_Row)

                    lstr_Read = ""
                End If
            End If
        Next


        Return ldt_TableResult

    End Function
    ''''----

    <WebMethod()>
    Public Function UpdateVisitAppoint(ByVal alng_VisitId As Long, ByVal aint_AppointMent As Integer, ByVal astr_Year As String, ByVal astr_Month As String, ByVal astr_Day As String, ByVal astr_hour As String, ByVal astr_Minutre As String, ByVal astr_Seconds As String, ByVal astr_UserName As String) As String

        '-----------------------------

        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable




        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        '' validar 
        '' si no hay visita 
        If alng_VisitId = 0 Then
            Return "" '' no hay viista
        End If
        '''''''''''''

        '' tabla
        ''''''''''''''''''''''''''''

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intAppointmentId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strAppointDate", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppYear", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppMonth", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppDay", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppHour", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppMinute", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strAppSecond", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)

        iolecmd_comand.Parameters("@intVisitId").Value = alng_VisitId
        iolecmd_comand.Parameters("@intAppointmentId").Value = aint_AppointMent
        iolecmd_comand.Parameters("@strAppYear").Value = astr_Year
        iolecmd_comand.Parameters("@strAppMonth").Value = astr_Month
        iolecmd_comand.Parameters("@strAppDay").Value = astr_Day
        iolecmd_comand.Parameters("@strAppHour").Value = astr_hour
        iolecmd_comand.Parameters("@strAppMinute").Value = astr_Minutre
        iolecmd_comand.Parameters("@strAppSecond").Value = astr_Seconds
        iolecmd_comand.Parameters("@strUsername").Value = astr_UserName

        '''''
        lstr_SQL = "spUpdateVisitAppoint"
        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        ''ejecutar 
        adapter = New OleDbDataAdapter(iolecmd_comand)
        ''''''''''''''''''''

        Try
            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            adapter.Fill(ldt_TableResult)

            '' si hay resultado , retornar
            If ldt_TableResult.Rows.Count > 0 And ldt_TableResult.Columns.Count > 0 Then
                Return ldt_TableResult(0)(0).ToString()
            End If
            ''desconectar
        Catch ex As Exception

            lstr_Message = ObtenerError(ex.Message, 9999)

        Finally
            iolecmd_comand.Connection.Close()
            ' iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        Return ""
    End Function

    ''--------
    <WebMethod()>
    Public Function CheckVisitContainer(ByVal alng_Universal As Long) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("VisitData")
        strSQL = "spCheckVisitContainer"

        iolecmd_comand.Parameters.Add("@intContainerUniversaalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversaalId").Value = alng_Universal

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function



    <WebMethod()>
    Public Function GetVisitListForRequiredBy(ByVal astr_username As String, ByVal astr_Service As String) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("Visits")
        strSQL = "spGetVisitListForRequiredBy"

        iolecmd_comand.Parameters.Add("strUser", OleDbType.Char)
        iolecmd_comand.Parameters("strUser").Value = astr_username


        iolecmd_comand.Parameters.Add("strService", OleDbType.Char)
        iolecmd_comand.Parameters("strService").Value = astr_Service


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function


    '*******************************************************************
    <WebMethod()>
    Public Function Get_CompanyFiscalInfo(ByVal alng_CompanyEntityId As Long, ByVal astr_service As String, ByVal alng_ServiceOrderId As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String
        strSQL = "spGetCompanyFiscalInfo"

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure

        'agrega parametro
        iolecmd_comand.Parameters.Add("@int_CompanyEntityId", OleDbType.Integer)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@str_Service", OleDbType.VarChar)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@int_ServiceOrder", OleDbType.Integer)

        ' se pone valor al parametro

        iolecmd_comand.Parameters("@int_CompanyEntityId").Value = alng_CompanyEntityId
        iolecmd_comand.Parameters("@str_Service").Value = astr_service
        iolecmd_comand.Parameters("@int_ServiceOrder").Value = alng_ServiceOrderId

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function GetInvoiceFiscalInfo(ByVal alng_CompanyEntityId As Long, ByVal alng_CustomerTypeId As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String
        strSQL = "spGetEntityFiscalInfo"

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure

        'agrega parametro
        iolecmd_comand.Parameters.Add("@int_CompanyEntityId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@int_CustomerType", OleDbType.Integer)
        ' se pone valor al parametro

        iolecmd_comand.Parameters("@int_CompanyEntityId").Value = alng_CompanyEntityId
        iolecmd_comand.Parameters("@int_CustomerType").Value = alng_CustomerTypeId

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    '*******************************************************************
    '*******************************************************************
    <WebMethod()>
    Public Function SearchBroker_RequiredLikeSQL(ByVal astr_Broker As String) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String

        ' pasar a mayusculas
        astr_Broker = astr_Broker.ToUpper()

        'definir el valor SQL
        'strSQL = "exec spGetBrokerLike " + astr_Broker

        strSQL = "exec spGetBrokerLike " + astr_Broker


        'strSQL = "exec spGetBrokerLike CASTAÑE"
        ' valor SQL
        iolecmd_comand.CommandText = strSQL


        'agrega parametro
        'iolecmd_comand.Parameters.Add("@strBroker", OleDbType.VarChar)

        ' se pone valor al parametro
        'iolecmd_comand.Parameters("@strBroker").Value = astr_Broker

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function SearchBroker_ById(ByVal aint_brokerid As Integer) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String



        'definir el valor SQL
        'strSQL = "exec spGetBrokerLike " + astr_Broker

        strSQL = "  SELECT TOP 11  tblclsCustomBroker.intCustomBrokerId  as 'ID', " &
                 "                 tblclsCustomBroker.strCustomBrokerIdentifier as 'Clave', " &
                 "		           tblclsCompany.strCompanyName as 'Nombre'," &
                 "		           tblclsCompany.strCompanyAddress1 + ','+ tblclsCompany.strCompanyCity +',' + tblclsCompany.strCompanyState as 'Direccion'," &
                 "  	           tblclsCompany.strCompanyZipCode as 'Codigo Postal', " &
                 "		           tblclsCompany.strCompanyFiscalIdentifier AS 'RFC' " &
                 "  FROM tblclsCompany " &
                 "      INNER JOIN  tblclsCompanyEntity    ON  tblclsCompany.intCompanyId = tblclsCompanyEntity.intCompanyId " &
                 "      INNER JOIN  tblclsCustomBroker     ON  tblclsCustomBroker.intCustomBrokerId  = tblclsCompanyEntity.intCompanyEntityId " &
                 "      INNER JOIN  tblclsCustomerType     ON  tblclsCompanyEntity.intCustomerTypeId = tblclsCustomerType.intCustomerTypeId " &
                 "  WHERE  tblclsCustomerType.strCustomerTypeIdentifier = 'CUSTOMBROKER' " &
     "        AND   tblclsCustomBroker.blnCustomBrokerActive = 1 " &
     "        AND   tblclsCustomBroker.intCustomBrokerId = " + aint_brokerid.ToString


        'strSQL = "exec spGetBrokerLike CASTAÑE"
        ' valor SQL
        iolecmd_comand.CommandText = strSQL


        'agrega parametro
        'iolecmd_comand.Parameters.Add("@strBroker", OleDbType.VarChar)

        ' se pone valor al parametro
        'iolecmd_comand.Parameters("@strBroker").Value = astr_Broker

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function SearchCarrier_ByIdString(ByVal astr_CarrierIDString As String) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String

        ' pasar a mayusculas
        astr_CarrierIDString = astr_CarrierIDString.ToUpper()


        'definir el valor SQL
        'strSQL = "exec spGetBrokerLike " + astr_Broker

        strSQL = "  SELECT TOP 1 intCarrierLineId, " &
                 "         strCarrierLineIdentifier as 'Clave', " &
                 " 	       strCarrierLineName as 'Nombre Transportista'," &
                 "		   ISNULL(strCarrierLineDescription,'') AS 'strCarrierLineDescription' , " &
                 "		   ISNULL(tblclsCompany.strCompanyFiscalIdentifier,'') AS 'RFC'	 " &
                 "    FROM tblclsCompany  " &
                 "        INNER JOIN  tblclsCompanyEntity   ON  tblclsCompany.intCompanyId = tblclsCompanyEntity.intCompanyId " &
                 "   	  INNER JOIN  tblclsCarrierLine     ON  tblclsCarrierLine.intCarrierLineId = tblclsCompanyEntity.intCompanyEntityId " &
                 "    	  INNER JOIN  tblclsCustomerType  ON  tblclsCompanyEntity.intCustomerTypeId = tblclsCustomerType.intCustomerTypeId  " &
                 "	WHERE tblclsCustomerType.strCustomerTypeIdentifier = 'CARRIERLINE' " &
                 "	AND   tblclsCarrierLine.blnCarrierLineActive  = 1 " &
                 "	AND   tblclsCarrierLine.strCarrierLineIdentifier = '" + astr_CarrierIDString + "'"


        'strSQL = "exec spGetBrokerLike CASTAÑE"
        ' valor SQL
        iolecmd_comand.CommandText = strSQL


        'agrega parametro
        'iolecmd_comand.Parameters.Add("@strBroker", OleDbType.VarChar)

        ' se pone valor al parametro
        'iolecmd_comand.Parameters("@strBroker").Value = astr_Broker

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function


    '*******************************************************************
    '*******************************************************************





    <WebMethod()>
    Public Function GetPayMethodTypeList() As DataTable

        Dim ldt_PayMethodTyperesult As DataTable 'tabla que obtiene el 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim strSQL As String
        Dim ldrow_new As DataRow

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldt_PayMethodTyperesult = New DataTable("tblclsPaymentMethodType")

        Try
            strSQL = " SELECT strPaymentMethodTypeId , " &
                     "        strPayMethodDescription " &
                     " FROM tblclsPaymentMethodType "

            iolecmd_comand.CommandText = strSQL

            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldt_PayMethodTyperesult)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            If strError.Length = 0 Then
                strError = ex.Message
            End If
            Return dt_RetrieveErrorTable(strError)
        Finally
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()
        End Try

        If ldt_PayMethodTyperesult.Rows.Count > 0 Then

            'insertar un registro vacio, para que seleccione 0 , en un principio

            ldrow_new = ldt_PayMethodTyperesult.NewRow()
            ldrow_new("strPaymentMethodTypeId") = "NADA"
            ldrow_new("strPayMethodDescription") = "NINGUN VALOR"

            ldt_PayMethodTyperesult.Rows.Add(ldrow_new)

        End If

        iAdapt_comand = Nothing
        ioleconx_conexion = Nothing

        Return ldt_PayMethodTyperesult
    End Function

    <WebMethod()>
    Public Function GetPayFormTypeList() As DataTable

        Dim ldt_PayFormTyperesult As DataTable 'tabla que obtiene el 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim strSQL As String
        Dim ldrow_new As DataRow

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldt_PayFormTyperesult = New DataTable("tblclsPaymentForm")

        Try
            strSQL = " SELECT intPaymentFormTypeId , " &
                     "        strPayFormDescription " &
                     " FROM tblclsPaymentForm "

            iolecmd_comand.CommandText = strSQL

            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldt_PayFormTyperesult)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            If strError.Length = 0 Then
                strError = ex.Message
            End If
            Return dt_RetrieveErrorTable(strError)
        Finally
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()
        End Try

        If ldt_PayFormTyperesult.Rows.Count > 0 Then

            'insertar un registro vacio, para que seleccione 0 , en un principio

            ldrow_new = ldt_PayFormTyperesult.NewRow()
            ldrow_new("intPaymentFormTypeId") = "0"
            ldrow_new("strPayFormDescription") = "NINGUN VALOR"

            ldt_PayFormTyperesult.Rows.Add(ldrow_new)

        End If

        iAdapt_comand = Nothing
        ioleconx_conexion = Nothing

        Return ldt_PayFormTyperesult
    End Function


    <WebMethod()>
    Public Function GetCFDIUsageList() As DataTable

        Dim ldt_CFDIUsageresult As DataTable 'tabla que obtiene el 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim strSQL As String
        Dim ldrow_new As DataRow

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldt_CFDIUsageresult = New DataTable("tblclsCFDIUsage")

        Try
            strSQL = " SELECT strCFDIUsageTypeId , " &
                     "       strCFDIUsageDescription " &
                     " FROM tblclsCFDIUsage "

            iolecmd_comand.CommandText = strSQL

            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldt_CFDIUsageresult)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            If strError.Length = 0 Then
                strError = ex.Message
            End If
            Return dt_RetrieveErrorTable(strError)
        Finally
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()
        End Try

        If ldt_CFDIUsageresult.Rows.Count > 0 Then

            'insertar un registro vacio, para que seleccione 0 , en un principio

            ldrow_new = ldt_CFDIUsageresult.NewRow()
            ldrow_new("strCFDIUsageTypeId") = "NADA"
            ldrow_new("strCFDIUsageDescription") = "NINGUN VALOR"

            ldt_CFDIUsageresult.Rows.Add(ldrow_new)

        End If

        iAdapt_comand = Nothing
        ioleconx_conexion = Nothing

        Return ldt_CFDIUsageresult
    End Function

    '<WebMethod()> _
    'Public Function UpdateCompanyFiscalData(ByVal alng_CompanyId As Long, ByVal astr_ServiceType As String, ByVal aint_ServiceOrderId As Long, ByVal astr_CompanyAdress As String, ByVal strCompanyCity As String, ByVal strCompanyState As String, ByVal astr_ZipCode As String, ByVal astr_RFC As String, ByVal aint_PayForm As Integer, ByVal astr_PayMethod As String, ByVal astr_CFDIUsage As String, ByVal astr_Username As String) As String

    '    Dim myConnectionString = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
    '    Dim myConnection As New OleDbConnection(myConnectionString)
    '    Dim mySelectQuery As String

    '    Try
    '        mySelectQuery = " UPDATE tblclsCompany " & _
    '                        " SET  tblclsCompany.dtmCompanyLastModified = GETDATE()" & _
    '                        " ,tblclsCompany.strCompanyLastModifiedBy = '" + astr_Username + "'"

    '        If astr_CompanyAdress.Length > 1 Then
    '            mySelectQuery = mySelectQuery + " ,tblclsCompany.strCompanyAddress1 = '" + astr_CompanyAdress + "'"
    '        End If

    '        If astr_RFC.Length > 1 Then
    '            mySelectQuery = mySelectQuery + " ,tblclsCompany.strCompanyFiscalIdentifier = '" + astr_RFC + "'"
    '        End If

    '        If astr_ZipCode.Length > 1 Then
    '            mySelectQuery = mySelectQuery + " ,tblclsCompany.strCompanyZipCode = '" + astr_ZipCode + "'"
    '        End If

    '        If aint_PayForm > 0 Then
    '            mySelectQuery = mySelectQuery + " ,tblclsCompany.intPaymentFormTypeId= " + aint_PayForm.ToString()
    '        End If

    '        If astr_PayMethod.Length > 1 Then
    '            mySelectQuery = mySelectQuery + " ,tblclsCompany.strPaymentMethodTypeId = '" + astr_PayMethod.ToString() + "'"
    '        End If

    '        If astr_CFDIUsage.Length > 1 Then
    '            mySelectQuery = mySelectQuery + " ,tblclsCompany.strCFDIUsageTypeId ='" + astr_CFDIUsage.ToString() + "'"
    '        End If

    '        'ByVal astr_City As String, ByVal astr_state As String, ByVal astr_RFC As String, 
    '        mySelectQuery = mySelectQuery + "  WHERE tblclsCompany.intCompanyId =  " + alng_CompanyId.ToString()




    '        Dim myCommand As New OleDbCommand(mySelectQuery, myConnection)
    '        myConnection.Open()
    '        'myReader = myCommand.ExecuteReader()
    '        myCommand.ExecuteNonQuery()

    '    Catch ex As Exception
    '        Dim lstr_ex As String
    '        lstr_ex = ex.Message

    '        Return lstr_ex

    '        'Return 0
    '        'myReader.Close()
    '        'Exit Function
    '    Finally
    '        myConnection.Close()
    '        myConnection.Dispose()
    '    End Try
    '    myConnection = Nothing

    '    Return ""
    'End Function


    ''' ''
    <WebMethod()>
    Public Function UpdateCompanyFiscalData(ByVal alng_CompanyEntityId As Long, ByVal astr_ServiceType As String, ByVal aint_ServiceOrderId As Long, ByVal astr_CompanyAdress As String, ByVal astr_CompanyCity As String, ByVal astr_CompanyState As String, ByVal astr_ZipCode As String, ByVal astr_RFC As String, ByVal aint_PayForm As Integer, ByVal astr_PayMethod As String, ByVal astr_CFDIUsage As String, ByVal astr_Username As String, ByVal abln_IsDirectCredit As Integer) As String

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"



        '' se realizara un correcion de 
        astr_CompanyAdress = of_convertoasccistring(astr_CompanyAdress)
        astr_CompanyCity = of_convertoasccistring(astr_CompanyCity)
        astr_CompanyState = of_convertoasccistring(astr_CompanyState)
        astr_ZipCode = of_convertoasccistring(astr_ZipCode)
        astr_RFC = of_convertoasccistring(astr_RFC)

        astr_CompanyAdress = CorrectStringFromASCII(astr_CompanyAdress)
        astr_CompanyCity = CorrectStringFromASCII(astr_CompanyCity)
        astr_CompanyState = CorrectStringFromASCII(astr_CompanyState)


        Dim strSQL As String
        'Dim strcontainerid As String
        strSQL = "spUpdateCompanyFiscalInfo"

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure

        'agrega parametro
        iolecmd_comand.Parameters.Add("@int_CompanyEntityId", OleDbType.Integer)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@str_Service", OleDbType.VarChar)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@int_ServiceOrder", OleDbType.Integer)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@str_CompanyAdress", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@str_CompanyState", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@astr_ZipCode", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@astr_RFC", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@aint_PayForm", OleDbType.Integer)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@astr_PayMethod", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@astr_CFDIUsage", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@astr_Username", OleDbType.VarChar)

        ' se agrega el parametro si es credito directo
        iolecmd_comand.Parameters.Add("@int_IsDirectCredit", OleDbType.Integer)



        '''
        'agrega parametro
        iolecmd_comand.Parameters("@int_CompanyEntityId").Value = alng_CompanyEntityId

        'agrega parametro
        iolecmd_comand.Parameters("@str_Service").Value = astr_ServiceType

        'agrega parametro
        iolecmd_comand.Parameters("@int_ServiceOrder").Value = aint_ServiceOrderId

        'agrega parametro
        iolecmd_comand.Parameters("@str_CompanyAdress").Value = astr_CompanyAdress

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@strCompanyCity").Value = astr_CompanyCity

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@str_CompanyState").Value = astr_CompanyState

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@astr_ZipCode").Value = astr_ZipCode

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@astr_RFC").Value = astr_RFC

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@aint_PayForm").Value = aint_PayForm

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@astr_PayMethod").Value = astr_PayMethod

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@astr_CFDIUsage").Value = astr_CFDIUsage

        ' se pone valor al parametro
        iolecmd_comand.Parameters("@astr_Username").Value = astr_Username

        ' se agrega el parametro si es credito directo
        iolecmd_comand.Parameters("@int_IsDirectCredit").Value = abln_IsDirectCredit


        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            Return strError
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return ""
    End Function

    ''  '' '' 
    <WebMethod()>
    Public Function UpdateUserPassword(ByVal aint_userId As Long, ByVal astr_OldPassword As String, ByVal astr_NewPassword As String, ByVal astr_usernamemofied As String) As String

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String
        strSQL = "spChangeUserPassword"

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure

        'agrega parametro
        iolecmd_comand.Parameters.Add("@int_UserId", OleDbType.Integer)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@strOldPassword", OleDbType.VarChar)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@strNewPassword", OleDbType.VarChar)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@str_usermodified", OleDbType.VarChar)

        ' se pone valor al parametro
        iolecmd_comand.Parameters.Add("@int_UserId", OleDbType.Integer)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@strOldPassword", OleDbType.VarChar)

        'agrega parametro
        iolecmd_comand.Parameters.Add("@strNewPassword", OleDbType.VarChar)

        '''''
        'agrega parametro
        iolecmd_comand.Parameters("@int_UserId").Value = aint_userId
        iolecmd_comand.Parameters("@strOldPassword").Value = astr_OldPassword
        iolecmd_comand.Parameters("@strNewPassword").Value = astr_NewPassword
        iolecmd_comand.Parameters("@str_usermodified").Value = astr_usernamemofied



        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            Return strError
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try

        If idt_result.Rows.Count > 0 And idt_result.Columns.Count > 0 Then
            Return idt_result.Rows(0)(0).ToString()
        End If

        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return ""




        Return ""
    End Function

    <WebMethod()>
    Public Function GetVisitListForUserId(ByVal aint_UserId As Integer) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        'If aint_UserId >= 0 Then

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("visitlist")
        strSQL = "spGetVisitListForUserId"

        iolecmd_comand.Parameters.Add("intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserId").Value = aint_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        'End If

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function GetUserMenu(ByVal userid As Integer) As DataSet

        '-----------------------------
        Dim oleDBconnx As OleDbConnection
        Dim oleDBcom As OleDbCommand
        oleDBcom = New OleDbCommand()
        oleDBconnx = New OleDbConnection()
        Dim strconx As String
        strconx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        oleDBconnx.ConnectionString = strconx
        oleDBcom = oleDBconnx.CreateCommand
        '----------------------------------


        Dim strSQL As String = "spGetUserModuleOptionsWB"
        oleDBcom.CommandType = CommandType.StoredProcedure
        Dim ol_param As OleDbParameter = New OleDbParameter

        ol_param = oleDBcom.Parameters.Add("@aint_userid", OleDbType.Numeric)
        ol_param.Value = userid

        ol_param = oleDBcom.Parameters.Add("@aint_moduleid", OleDbType.Numeric)
        ol_param.Value = 9

        oleDBcom.CommandTimeout = 0
        oleDBcom.CommandText = "spGetUserModuleOptionsWB"

        Dim dataAdapta As OleDbDataAdapter = New OleDbDataAdapter(oleDBcom)
        Dim ds_resultado As DataSet = New DataSet()

        Try
            oleDBconnx.Open()
            dataAdapta.SelectCommand.CommandTimeout = of_getMaxTimeout()
            dataAdapta.Fill(ds_resultado)

        Catch ex As Exception
        Finally
            oleDBconnx.Close()
        End Try
        '    TK_MainMenu.DataTextField = "strModuleOptionDescription";
        '   TK_MainMenu.DataValueField = "strModuleOptionIdentifier";

        Return ds_resultado
    End Function

    <WebMethod()>
    Public Function SearchCustomer_RFC(ByVal astr_FiscalIdentier_RFC As String) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        astr_FiscalIdentier_RFC = astr_FiscalIdentier_RFC.ToUpper.Trim.ToString()
        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT ISNULL(tblclsCustomer.intCustomerId,0)  AS 'intCustomerId' , " &
                 "        ISNULL(tblclsCustomer.intCompanyId,0) AS 'intCompanyId'    , " &
                 "        ISNULL(tblclsCustomer.strCustomerIdentifier,'') AS 'strCustomerIdentifier',  " &
                 "        ISNULL(tblclsCompany.strCompanyName,'') AS 'strCompanyName' ,  " &
                 "        ISNULL(tblclsCompany.intCompanyStatusId,0) AS 'intCompanyStatusId'      ,  " &
                 "        ISNULL(tblclsCompany.strCompanyAddress1,'') AS 'strCompanyAddress1'       ,  " &
                 "        ISNULL(tblclsCompany.strCompanyAddress2,'') AS 'strCompanyAddress2'       ,  " &
                 "        ISNULL(tblclsCompany.strCompanyCity,'')  AS 'strCompanyCity'              ,  " &
                 "        ISNULL(tblclsCompany.strCompanyState,'') AS 'strCompanyState'             ,  " &
                 "        ISNULL(tblclsCompany.strCompanyCountry,'') AS 'strCompanyCountry'         ,  " &
                 "        ISNULL(tblclsCompany.strCompanyPhone1,'')  AS 'strCompanyPhone1'          ,  " &
                 "        ISNULL(tblclsCompany.strCompanyPhone2,'')  AS 'strCompanyPhone2'          ,  " &
                 "        ISNULL(tblclsCompany.strCompanyFax,'')     AS 'strCompanyFax'             ,  " &
                 "        ISNULL(tblclsCompany.strCompanyZipCode,'') AS 'strCompanyZipCode'         ,  " &
                 "        ISNULL(tblclsCompany.strCompanyEmail,'')   AS 'strCompanyEmail'           ,  " &
                 "        ISNULL(tblclsCompany.strCompanyMainContact,'') AS 'strCompanyMainContact' ,  " &
                 "        ISNULL(tblclsCompany.strCompanyBillingIdentifier,'') AS 'strCompanyBillingIdentifier', " &
                 "        ISNULL(tblclsCompany.strCompanyBillingAddress1,'') AS 'strCompanyBillingAddress1'    , " &
                 "        ISNULL(tblclsCompany.strCompanyBillingAddress2,'') AS 'strCompanyBillingAddress2'    , " &
                 "        ISNULL(tblclsCompany.strCompanyComments,'') AS 'strCompanyComments'                  , " &
                 "        ISNULL(tblclsCompany.strPaymentMethodTypeId,'') AS 'strPaymentMethodTypeId'          , " &
                 "        ISNULL(tblclsCompany.intPaymentFormTypeId,0) AS 'intPaymentFormTypeId'              , " &
                 "        ISNULL(tblclsCompany.strCFDIUsageTypeId,'') AS 'strCFDIUsageTypeId'                    " &
                 " FROM tblclsCustomer        " &
                 "      INNER JOIN tblclsCompany ON tblclsCompany.intCompanyId = tblclsCustomer.intCompanyId     " &
                 "   WHERE tblclsCustomer.blnCustomerActive = 1 " &
                 "    AND  tblclsCompany.strCompanyFiscalIdentifier ='" + astr_FiscalIdentier_RFC + "'"

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function


    <WebMethod()>
    Public Function SearchBookingAdviceMaster(ByVal astr_booking As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        astr_booking = astr_booking.Trim.ToUpper.Trim()
        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT ISNULL(tblclsBookingAdvice.intBookingAdviceId,0)  AS 'intBookingAdviceId' , " &
                 "        ISNULL(tblclsBookingAdvice.strBookingId,'')       AS 'strBookingId',   " &
                 "        ISNULL(tblclsBookingAdvice.strVesselName,'')      AS 'strVesselName',  " &
                 "        ISNULL(tblclsBookingAdvice.strVoyageExpoId,'')    AS 'strVoyageExpoId', " &
                 "        ISNULL(tblclsBookingAdvice.intVesselId,0)         AS 'intVesselId'  , " &
                 "        ISNULL(tblclsBookingAdvice.intVesselVoyageId,0)   AS 'intVesselVoyageId', " &
                 "        ISNULL(tblclsBookingAdvice.strPortText ,'')       AS 'strPortText',  " &
                 "        ISNULL(tblclsBookingAdvice.strPortId ,'')         AS 'strPortId', " &
                 "        ISNULL(tblclsBookingAdvice.strCountryTxt ,'')     AS 'strCountryTxt', " &
                 "        ISNULL(tblclsBookingAdvice.strCountryId ,'')      AS 'strCountryId'," &
                 "        ISNULL(tblclsBookingAdvice.dtmETADate ,'19000101 00:00')        AS 'dtmETADate'," &
                 "        ISNULL(tblclsBookingAdvice.strCustomerTxt ,'')    AS 'strCustomerTxt'," &
                 "        ISNULL(tblclsBookingAdvice.intCustomerId,0)       AS 'intCustomerId' ," &
                 "        ISNULL(tblclsBookingAdvice.intCustomBrokerId,0)   AS 'intCustomBrokerId'   ," &
                 "        ISNULL(tblclsBookingAdvice.strShippingLinetxt,'') AS 'strShippingLinetxt'  ," &
                 "        ISNULL(tblclsBookingAdvice.intShippingLine,0)     AS 'intShippingLine'     ," &
                 "        ISNULL(tblclsBookingAdvice.strProductText,'')     AS 'strProductText'      ," &
                 "        ISNULL(tblclsBookingAdvice.intProductId,0)        AS 'intProductId'        ," &
                 "        ISNULL(tblclsBookingAdvice.intIMOCode,0)          AS 'intIMOCode'          ," &
                 "        ISNULL(tblclsBookingAdvice.intUNCode,0)           AS 'intUNCode'           ," &
                 "        ISNULL(tblclsBookingAdvice.strServiceType,'')     AS 'strServiceType'      ," &
                 "        ISNULL(tblclsBookingAdvice.blnIsValidBooking,'')   AS 'blnIsValidBooking'   ," &
                 "        ISNULL(tblclsBookingAdvice.blnIsValidByShipper,0) AS 'blnIsValidByShipper' ," &
                 "        ISNULL(tblclsBookingAdvice.strAdviceComments, '')  AS 'strAdviceComments',   " &
                 "        ISNULL(tblclsBookingAdvice.blnIsFromCalathus, 0)  AS 'blnIsFromCalathus'   " &
                 "        FROM tblclsBookingAdvice                              " &
                 "  WHERE tblclsBookingAdvice.strBookingId = '" + astr_booking + "'"

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()

            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    ''----
    <WebMethod()>
    Public Function SearchBookingAdviceMasterById(ByVal aint_BookingAdvice As Integer) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        'strSQL = " SELECT ISNULL(tblclsBookAdviceContainer.intBookingAdviceId,0)  AS 'intBookingAdviceId' ," & _
        '         "        ISNULL(tblclsBookAdviceContainer.strContainerId,'') AS 'strContainerId', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intContainerSize,0) AS 'intContainerSize', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intContainerType,0) AS 'intContainerType', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intContainerISOCode,0) AS 'intContainerISOCode', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intVisitId,0) AS 'intVisitId'," & _
        '         "        ISNULL(tblclsBookAdviceContainer.decVGM,0) AS 'decVGM' ," & _
        '         "        ISNULL(tblclsBookAdviceContainer.blnContainerIsFull,0) AS 'blnContainerIsFull' , " & _
        '         "        ISNULL(tblclsBookAdviceContainer.blnIsValidItem,0) AS 'blnIsValidItem' , " & _
        '         "        ISNULL(tblclsBookAdviceContainer.blIsValidByShipper,0) AS 'blIsValidByShipper', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.blnIsFromCalathus, 0) AS 'blnIsFromCalathus' " & _
        '         "        FROM tblclsBookAdviceContainer " & _
        '         "        WHERE tblclsBookAdviceContainer.intBookingAdviceId = " + aint_BookingAdvice.ToString

        idt_result = New DataTable("VisitData")
        strSQL = "spGetBKMasterAdvice"

        iolecmd_comand.Parameters.Add("@aint_AdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@aint_AdviceId").Value = aint_BookingAdvice

        iolecmd_comand.Parameters.Add("@strBooking", OleDbType.Char)
        iolecmd_comand.Parameters("@strBooking").Value = ""

        iolecmd_comand.Parameters.Add("@intOption", OleDbType.Integer)
        iolecmd_comand.Parameters("@intOption").Value = 0

        iolecmd_comand.Parameters.Add("@aint_Param4", OleDbType.Integer)
        iolecmd_comand.Parameters("@aint_Param4").Value = 0

        iolecmd_comand.Parameters.Add("@astr_Param5", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_Param5").Value = ""


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function
    '''----

    <WebMethod()>
    Public Function SearchBookingAdviceDetail(ByVal aint_BookingAdvice As Integer) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        'strSQL = " SELECT ISNULL(tblclsBookAdviceContainer.intBookingAdviceId,0)  AS 'intBookingAdviceId' ," & _
        '         "        ISNULL(tblclsBookAdviceContainer.strContainerId,'') AS 'strContainerId', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intContainerSize,0) AS 'intContainerSize', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intContainerType,0) AS 'intContainerType', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intContainerISOCode,0) AS 'intContainerISOCode', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.intVisitId,0) AS 'intVisitId'," & _
        '         "        ISNULL(tblclsBookAdviceContainer.decVGM,0) AS 'decVGM' ," & _
        '         "        ISNULL(tblclsBookAdviceContainer.blnContainerIsFull,0) AS 'blnContainerIsFull' , " & _
        '         "        ISNULL(tblclsBookAdviceContainer.blnIsValidItem,0) AS 'blnIsValidItem' , " & _
        '         "        ISNULL(tblclsBookAdviceContainer.blIsValidByShipper,0) AS 'blIsValidByShipper', " & _
        '         "        ISNULL(tblclsBookAdviceContainer.blnIsFromCalathus, 0) AS 'blnIsFromCalathus' " & _
        '         "        FROM tblclsBookAdviceContainer " & _
        '         "        WHERE tblclsBookAdviceContainer.intBookingAdviceId = " + aint_BookingAdvice.ToString

        idt_result = New DataTable("VisitData")
        strSQL = "spGetDetailBookingAdv"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdvice

        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters("@intApplyFilter").Value = 0

        iolecmd_comand.Parameters.Add("@strContainer", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainer").Value = ""

        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUserId").Value = 0

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function


    <WebMethod()>
    Public Function GetAllBookingAdviceDetailNotValid() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        idt_result = New DataTable("VisitData")
        strSQL = "spGetDetailBookingAdv"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = 0

        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters("@intApplyFilter").Value = 1


        iolecmd_comand.Parameters.Add("@strContainer", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainer").Value = ""


        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUserId").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function


    <WebMethod()>
    Public Function GetBookingDetailValid(ByVal aintAdviceId As Integer, ByVal aintBrokerId As Integer) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        idt_result = New DataTable("VisitData")
        strSQL = "spGetDetailBookingAdv"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aintAdviceId

        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters("@intApplyFilter").Value = aintBrokerId


        iolecmd_comand.Parameters.Add("@strContainer", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainer").Value = ""


        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUserId").Value = 0

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function


    <WebMethod()>
    Public Function SearchBookingAdviceDetailValid(ByVal aint_BookingAdvice As Integer) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String

        idt_result = New DataTable("VisitData")
        strSQL = "spGetDetailBookingAdv"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdvice

        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters("@intApplyFilter").Value = 1


        iolecmd_comand.Parameters.Add("@strContainer", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainer").Value = ""


        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUserId").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function


    <WebMethod()>
    Public Function GetContainerBooking(ByVal aint_bookingId As Integer, ByVal astr_containerId As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        idt_result = New DataTable("VisitData")
        strSQL = "spGetDetailBookingAdv"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_bookingId

        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters("@intApplyFilter").Value = -1


        iolecmd_comand.Parameters.Add("@strContainer", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainer").Value = astr_containerId


        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUserId").Value = 0

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function

    <WebMethod()>
    Public Function GetContainerBookingByUserId(ByVal aint_UserId As Integer) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        idt_result = New DataTable("VisitData")
        strSQL = "spGetDetailBookingAdv"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = 0

        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters("@intApplyFilter").Value = -1


        iolecmd_comand.Parameters.Add("@strContainer", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainer").Value = ""


        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUserId").Value = aint_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function


    Public Function of_searchPortsLikeId(ByVal astr_PortLike As String) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"


        ''validar el valor del puerto 
        If astr_PortLike.Length < 3 Then
            Return New DataTable("Empty")
        End If

        astr_PortLike = astr_PortLike.Trim.ToUpper.ToString()
        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT ISNULL(tblclsPort.strPortId,'')          AS 'strPortId', " &
                "        ISNULL(tblclsPort.strPortName,'')        AS 'strPortName'," &
                "        ISNULL(tblclsPort.strCountryId,'')       AS 'strCountryId'," &
                "        ISNULL(tblclsPort.chrPortLetterId,'')    AS 'chrPortLetterId'," &
                "        ISNULL(tblclsPort.intRegionId,0)        AS 'intRegionId'     " &
                "        FROM tblclsPort  " &
                " WHERE tblclsPort.strPortId LIKE '%" + astr_PortLike + "%' "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result

    End Function

    Public Function of_searchPortsLikeFullname(ByVal astr_PortLike As String) As DataTable
        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"


        ''validar el valor del puerto 
        If astr_PortLike.Length < 3 Then
            Return New DataTable("Empty")
        End If

        astr_PortLike = astr_PortLike.Trim.ToUpper.ToString()
        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT ISNULL(tblclsPort.strPortId,'')          AS 'strPortId', " &
                "        ISNULL(tblclsPort.strPortName,'')        AS 'strPortName'," &
                "        ISNULL(tblclsPort.strCountryId,'')       AS 'strCountryId'," &
                "        ISNULL(tblclsPort.chrPortLetterId,'')    AS 'chrPortLetterId'," &
                "        ISNULL(tblclsPort.intRegionId,0)        AS 'intRegionId'     " &
                "        FROM tblclsPort  " &
                " WHERE UPPER(tblclsPort.strPortName) LIKE '%" + astr_PortLike + "%' "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function SearchPortsLike(ByVal astr_PortLike As String) As DataTable

        Dim idt_result As DataTable = New DataTable("result") ' Tabla con el query de resultados 
        Dim ldt_resuitA As DataTable = New DataTable("result")
        Dim ldt_resultB As DataTable = New DataTable("result")

        ''llamar el metodo de busqueda por clave iso
        ldt_resuitA = of_searchPortsLikeId(astr_PortLike)

        '' si tabla vacia
        ''llamar el meotodo web 
        If ldt_resuitA.Rows.Count = 0 Then
            ldt_resuitA = of_searchPortsLikeFullname(astr_PortLike)
        End If


        Return ldt_resuitA


    End Function

    '


    ''

    <WebMethod()>
    Public Function GetCountryList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT tblclsCountry.strCountryId,     " &
                 "        tblclsCountry.strISOIdentifier, " &
                 "        tblclsCountry.strCountryName " &
                 " FROM  tblclsCountry  " &
                 "        WHERE   tblclsCountry.blnCountryActive = 1  "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function


    <WebMethod()>
    Public Function GetIMOList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT tblclsIMOCode.intIMOCodeId , " &
                 "       tblclsIMOCode.strIMOCodeIdentifier, " &
                 "       tblclsIMOCode.strIMOCodeDescription " &
                 "  FROM tblclsIMOCode  " &
                 "       WHERE tblclsIMOCode.blnIMOCodeActive = 1 "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand


        iolecmd_comand.Connection.Open()

        iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
        iAdapt_comand.Fill(idt_result)

        iolecmd_comand.Connection.Close()
        iAdapt_comand.SelectCommand.Connection.Close()
        ioleconx_conexion.Close()


        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    ''
    <WebMethod()>
    Public Function SearchIMOCodeByIdentifier(ByVal astr_IMOCode As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"


        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT tblclsIMOCode.intIMOCodeId , " &
                 "       tblclsIMOCode.strIMOCodeIdentifier, " &
                 "       tblclsIMOCode.strIMOCodeDescription " &
                 "  FROM tblclsIMOCode  " &
                 "       WHERE tblclsIMOCode.blnIMOCodeActive = 1 " &
                 "       AND  tblclsIMOCode.strIMOCodeIdentifier =  '" + astr_IMOCode + "'"

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    ''
    <WebMethod()>
    Public Function SearchIMOCodeByDescription(ByVal astr_IMOCode_Description As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        astr_IMOCode_Description = astr_IMOCode_Description.ToUpper()


        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT tblclsIMOCode.intIMOCodeId , " &
                 "  tblclsIMOCode.strIMOCodeIdentifier, " &
                 "  tblclsIMOCode.strIMOCodeDescription " &
                 " FROM tblclsIMOCode " &
                 " WHERE tblclsIMOCode.blnIMOCodeActive = 1 " &
                 " AND  tblclsIMOCode.strIMOCodeDescription  LIKE '%" + astr_IMOCode_Description + "%'"
        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    '''
    <WebMethod()>
    Public Function GetUNList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT tblclsUNCode.intUNCodeId ,  " &
                 "        tblclsUNCode.strUNCodeIdentifier " &
                 " FROM tblclsUNCode "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function



    <WebMethod()>
    Public Function SearchContainerCatalog(ByVal astr_Container As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"


        ''validar el valor del puerto 
        astr_Container = astr_Container.Trim.ToUpper()

        If astr_Container.Length < 4 Then
            Return New DataTable("Empty")
        End If

        Dim strSQL As String
        'Dim strcontainerid As String



        'agregar parametros
        iolecmd_comand.Parameters.Add("@strContainerLike", OleDbType.Char)
        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@strContainerLike").Value = astr_Container

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        strSQL = "spGetContainerCatalogExpo"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand
        '   Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function


    <WebMethod()>
    Public Function SearchContainerCatalogForAll(ByVal astr_Container As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"


        ''validar el valor del puerto 
        astr_Container = astr_Container.Trim.ToUpper()

        If astr_Container.Length < 4 Then
            Return New DataTable("Empty")
        End If

        Dim strSQL As String
        'Dim strcontainerid As String



        'agregar parametros
        iolecmd_comand.Parameters.Add("@strContainerLike", OleDbType.Char)

        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@strContainerLike").Value = astr_Container
        ''
        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 1
        '''
        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        strSQL = "spGetContainerCatalog"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand
        '   Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    ''----
    '<WebMethod()> _
    'Public Function InsertContainersToAdvice(ByVal aint_AdviceBooking As Integer, ByVal alistobj_container As ClsAdviceDetailDataBooking(), ByVal astr_user As String) As DataTable

    '    Dim ldt_tableResult As DataTable = New DataTable("result")
    '    Dim lint_idx = 0
    '    'validcion de id booking
    '    If aint_AdviceBooking = 0 Then
    '        Return dt_RetrieveErrorTable("numero de aviso invalido")
    '    End If

    '    'validacion de items
    '    Try
    '        If alistobj_container Is Nothing Then
    '            Return dt_RetrieveErrorTable("lista de contenedores vacia")
    '        End If

    '        If alistobj_container.Length = 0 Then
    '            Return dt_RetrieveErrorTable("lista de contenedores vacia")
    '        End If

    '    Catch ex As Exception

    '    End Try

    '    ''recorrer la lista y poner iso 0 en numerico , y tipo y tamaño
    '    For lint_idx = 0 To alistobj_container.Count - 1
    '        alistobj_container(lint_idx).iint_containerSize = 0
    '        alistobj_container(lint_idx).iint_ContainerType = 0
    '        alistobj_container(lint_idx).iint_ContainerISOCode = 0

    '        If alistobj_container(lint_idx).idec_NETWeight > 1500 Or alistobj_container(lint_idx).idec_VGM > 1500 Then
    '            alistobj_container(lint_idx).iint_IsFull = 1
    '        Else
    '            alistobj_container(lint_idx).iint_IsFull = 0
    '        End If

    '    Next

    '    'llamar insertar el contenedores
    '    ldt_tableResult = of_saveDetailAdvice(aint_AdviceBooking, alistobj_container, astr_user)

    '    ldt_tableResult.TableName = "resultado"

    '    Return ldt_tableResult
    'End Function

    ''---
    <WebMethod()>
    Public Function GetContainerTypeList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 
        strSQL = "  SELECT tblclsContainerType.intContainerTypeId, " &
                  "         tblclsContainerType.strContainerTypeIdentifier, " &
                  "         tblclsContainerType.strContainerTypeDescription " &
                  "   FROM tblclsContainerType "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function


    <WebMethod()>
    Public Function GetContainerSizeList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 

        strSQL = "   SELECT tblclsContainerSize.intContainerSizeId , " &
                 "          tblclsContainerSize.strContainerSizeIdentifier ," &
                 "          tblclsContainerSize.strContainerSizeDescription " &
                 "    FROM tblclsContainerSize "




        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function


    <WebMethod()>
    Public Function GetShippingLineList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 
        strSQL = " SELECT tblclsShippingLine.intShippingLineId , " &
                 "        tblclsShippingLine.intCompanyId ,     " &
                 "        tblclsShippingLine.strShippingLineIdentifier ," &
                 "        tblclsCompany.strCompanyName,         " &
                 "        tblclsShippingLine.strShippingLineComments " &
                 "        FROM  tblclsShippingLine  " &
                 "          INNER JOIN  tblclsCompany  ON tblclsCompany.intCompanyId = tblclsShippingLine.intCompanyId " &
                 "  WHERE tblclsShippingLine.blnShippingLineActive = 1  "


        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function GetShippingLineById(ByVal aint_ShippingLine As Long) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 
        strSQL = " SELECT tblclsShippingLine.intShippingLineId , " &
                 "        tblclsShippingLine.intCompanyId ,     " &
                 "        tblclsShippingLine.strShippingLineIdentifier ," &
                 "        tblclsCompany.strCompanyName,         " &
                 "        tblclsShippingLine.strShippingLineComments " &
                 "        FROM  tblclsShippingLine  " &
                 "          INNER JOIN  tblclsCompany  ON tblclsCompany.intCompanyId = tblclsShippingLine.intCompanyId " &
                 "  WHERE tblclsShippingLine.intShippingLineId = " + aint_ShippingLine.ToString()


        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function GetAllShippingLineList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 
        strSQL = " SELECT tblclsShippingLine.intShippingLineId , " &
                 "        tblclsShippingLine.intCompanyId ,     " &
                 "        tblclsShippingLine.strShippingLineIdentifier ," &
                 "        tblclsCompany.strCompanyName,         " &
                 "        tblclsShippingLine.strShippingLineComments " &
                 "        FROM  tblclsShippingLine  " &
                 "          INNER JOIN  tblclsCompany  ON tblclsCompany.intCompanyId = tblclsShippingLine.intCompanyId "



        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function GetCurrentHolydays() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 
        strSQL = " SELECT  CONVERT(VARCHAR(20),tblclsHoliday.dtmHolidayDate,111)  " &
                 "   FROM  tblclsHoliday " &
                 " WHERE  tblclsHoliday.dtmHolidayDate > DateAdd(DD, -60, GETDATE()) " &
                 " AND   tblclsHoliday.dtmHolidayDate < DATEADD( DD,60, GETDATE() )"


        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function FillVessel() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 
        strSQL = " SELECT  ISNULL(tblclsVesselVoyage.intVesselVoyageId, 0 ) AS 'intVesselVoyageId' , " &
                  "         ISNULL(tblclsVessel.strVesselName,'') AS 'strVesselName'  , " &
                  "         ISNULL(tblclsVesselVoyage.intVesselId,0) AS 'intVesselId' , " &
                  "         ISNULL(tblclsVesselVoyage.strVesselVoyageNumber,0) AS 'strVesselVoyageNumber' , " &
                  "         ISNULL( tblclsVesselVoyage.dteVesselVoyageArrivalDate,'19000101 00:00') AS 'dteVesselVoyageArrivalDate', " &
                  "         ISNULL( tblclsVesselVoyage.dteVesselVoyageDepartureDate,'19000101 00:00') AS 'dteVesselVoyageDepartureDate' , " &
                  "         ISNULL( tblclsVesselVoyage.strVesselVoyageExpoIdentifier,'') AS 'strVesselVoyageExpoIdentifier' , " &
                  "         ISNULL( tblclsVesselRoute.strVesselRouteId, '' ) AS 'strVesselRouteId' , " &
                  "         ISNULL( tblclsVesselRoute.intVesselRouteId , 0 ) AS intVesselRouteId " &
                  " FROM  tblclsVesselVoyage Join tblclsVessel On tblclsVessel.intVesselId = tblclsVesselVoyage.intVesselId " &
                  "        Join tblVessel_VesselRoute On tblVessel_VesselRoute.intVesselId = tblclsVessel.intVesselId " &
                  "        Join tblclsVesselRoute On tblclsVesselRoute.intVesselRouteId = tblVessel_VesselRoute.intVesselRouteId	" &
                  " WHERE tblclsVesselVoyage.blnVesselVoyageActive = 1 " &
                  " And tblclsVesselVoyage.dteVesselVoyageDepartureDate >getdate() " &
                  " and DATEDIFF(DAY,getdate()  , tblclsVesselVoyage.dteVesselVoyageDepartureDate) < 30 " &
                  " order by tblclsVesselVoyage.dteVesselVoyageArrivalDate asc  "


        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function FillBooking() As DataTable
        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String


        'Sentencia SQL 
        strSQL = " SELECT  ISNULL(intBookingAdviceId,0) AS 'intBookingAdviceId', " &
                 " 	       ISNULL(strBookingId,'')     AS 'strBookingId',   " &
                 "         ISNULL(strVesselName,'')    AS 'strVesselName' ," &
                 "         ISNULL(strVoyageExpoId,'')  AS 'strVoyageExpoId', " &
                 "         ISNULL(intVesselId, 0 )     AS 'intVesselId', " &
                 "         ISNULL(intVesselVoyageId,0) AS 'intVesselVoyageId', " &
                 "	       ISNULL(strPortText,'')      AS 'strPortText', " &
                 "    	   ISNULL(strPortId, '')       AS 'strPortId', " &
                 "         ISNULL(strCountryTxt,'' )   AS 'strCountryTxt',  " &
                 "         ISNULL(strCountryId,'')     AS 'strCountryId', " &
                 "         ISNULL(dtmETADate,'19000101 00:00') AS 'dtmETADate', " &
                 "	       ISNULL(strCustomerTxt,'')  AS 'strCustomerTxt',  " &
                 "	       ISNULL(tblclsBookingAdvice.intCustomerId,0)    AS 'intCustomerId',  " &
                 "         ISNULL(tblclsBookingAdvice.intCustomBrokerId,0)   AS 'intCustomBrokerId',  " &
                 "         ISNULL(strShippingLinetxt,'') AS 'strShippingLinetxt', " &
                 "         ISNULL(intShippingLine, 0 )  AS 'intShippingLine', " &
                 "         ISNULL(	strProductText,'')  AS   'strProductText', " &
                 "         ISNULL(	intProductId, 0 )   AS   'intProductId', " &
                 "         ISNULL(	tblclsBookingAdvice.intIMOCode,   0 )   AS   'intIMOCode',   " &
                 "         ISNULL(  tblclsIMOCode.strIMOCodeIdentifier , '' ) AS 'strIMOCodeIdentifier' , " &
                 "         ISNULL(	tblclsBookingAdvice.intUNCode,    0 )   AS   'intUNCode', " &
                 "         ISNULL(	tblclsUNCode.strUNCodeIdentifier,    '' )   AS   'strUNCodeIdentifier', " &
                 "         ISNULL(	strServiceType,'')   AS   'strServiceType', " &
                 "         ISNULL(	blnIsValidBooking,'') AS 'blnIsValidBooking', " &
                 "         ISNULL( blnIsValidByShipper,0) AS 'blnIsValidByShipper', " &
                 "         ISNULL(	blnIsFromCalathus,0) AS 'blnIsFromCalathus',  " &
                 "         ISNULL(	strBkAdviceCreatedBy,'') AS 'strBkAdviceCreatedBy', " &
                 "         ISNULL(	dtmABkAdviceCreationStamp,'19000101 00:00') AS 'dtmABkAdviceCreationStamp',  " &
                 "         ISNULL(	strAdviceComments,'') AS 'strAdviceComments'  " &
                 "         ,ISNULL(tblclsCustomBroker.intCompanyId ,0) AS 'intCompanyId' " &
                 "         , ISNULL(CUSTCOMP.strCompanyName ,'') AS 'strCustomCompany' " &
                 "      ,ISNULL(tblclsVIPCustomers.blnActive ,0) as 'blnIsCustomVIP' " &
                 "      ,ISNULL(tblclsBookingAdvice.strUserToReview ,'') as 'strUserToReview' " &
                 "   FROM    tblclsBookingAdvice  " &
                 "   LEFT JOIN tblclsIMOCode ON tblclsBookingAdvice.intIMOCode = tblclsIMOCode.intIMOCodeId " &
                 "   LEFT JOIN tblclsUNCode  ON tblclsBookingAdvice.intUNCode =   tblclsUNCode.intUNCodeId " &
                 "   LEFT JOIN tblclsCustomBroker ON tblclsCustomBroker.intCustomBrokerId = tblclsBookingAdvice.intCustomBrokerId " &
                 "   LEFT JOIN tblclsCustomer ON tblclsBookingAdvice.intCustomerId = tblclsCustomer.intCustomerId " &
                 "   LEFT JOIN tblclsCompanyEntity ON  tblclsCompanyEntity.intCompanyEntityId =  tblclsCustomer.intCustomerId " &
                 "                     AND tblclsCompanyEntity.intCustomerTypeId =1 " &
                  "     LEFT JOIN tblclsCompany CUSTCOMP ON CUSTCOMP.intCompanyId = tblclsCompanyEntity.intCompanyId " &
                 "   LEFT JOIN tblclsVIPCustomers ON tblclsVIPCustomers.intCustomerId = tblclsCustomer.intCustomerId " &
                 " WHERE blnIsValidBooking = 'PENDVAL' " &
                 " AND   blnIsValidByShipper = 0 "



        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result
    End Function


    <WebMethod()>
    Public Function SearchVesselVoyage_ByVesselName(ByVal astr_VesselName As String) As DataTable
        Dim ldt_tableResult As DataTable = New DataTable("result")
        ldt_tableResult = of_GetVesselVoyage(astr_VesselName, "", "", 0)
        Return ldt_tableResult
    End Function

    <WebMethod()>
    Public Function SearchVesselVoyage_ByNumerId(ByVal astr_NumberID As String) As DataTable
        Dim ldt_tableResult As DataTable = New DataTable("result")
        ldt_tableResult = of_GetVesselVoyage("", astr_NumberID, "", 0)
        Return ldt_tableResult
    End Function

    <WebMethod()>
    Public Function SearchVesselVoyage_ByExpoId(ByVal astr_ExpoID As String) As DataTable
        Dim ldt_tableResult As DataTable = New DataTable("result")
        ldt_tableResult = of_GetVesselVoyage("", "", astr_ExpoID, 0)
        Return ldt_tableResult
    End Function

    <WebMethod()>
    Public Function GetPortByVesselVoyage(ByVal aint_vesselvoyage As Integer) As DataTable

        Return of_GetPortsByVessel_VesselVoyage(aint_vesselvoyage, 0)
    End Function


    <WebMethod()>
    Public Function GetPortByVessel(ByVal aint_vessel As Integer) As DataTable

        Return of_GetPortsByVessel_VesselVoyage(0, aint_vessel)
    End Function


    Public Function of_GetVesselVoyage(ByVal astr_VesselName As String, ByVal astr_VesselNumberId As String, ByVal astr_VeeselExpoId As String, ByVal aint_AllVeeselVoyage As Integer) As DataTable

        Dim lstr_MaxRange As String
        Dim lstr_MinRange As String

        Dim lint_MaxRange As Integer
        Dim lint_MinRange As Integer


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        ' obtener el rango
        Try
            lstr_MinRange = ConfigurationManager.AppSettings("DaysBefore")
            lstr_MaxRange = ConfigurationManager.AppSettings("DaysAfter")

            If Integer.TryParse(lstr_MinRange, lint_MinRange) = False Then
                lint_MinRange = 4
            End If

            If Integer.TryParse(lstr_MaxRange, lint_MaxRange) = False Then
                lint_MaxRange = 8
            End If

        Catch ex As Exception
            lstr_MinRange = "4"
            lstr_MaxRange = "8"
            lint_MinRange = 4
            lint_MaxRange = 8
        End Try


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        astr_VesselName = astr_VesselName.ToUpper()
        astr_VeeselExpoId = astr_VeeselExpoId.ToUpper()
        astr_VesselNumberId = astr_VesselNumberId.ToUpper()

        'Dim strcontainerid As String

        idt_result = New DataTable("Result")
        strSQL = "spSearchVesselVoyageWB"

        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.VarChar)
        iolecmd_comand.Parameters("@strVesselName").Value = astr_VesselName


        iolecmd_comand.Parameters.Add("@strVesselnumberId", OleDbType.VarChar)
        iolecmd_comand.Parameters("@strVesselnumberId").Value = astr_VesselNumberId


        iolecmd_comand.Parameters.Add("@strVesselExpoId", OleDbType.VarChar)
        iolecmd_comand.Parameters("@strVesselExpoId").Value = astr_VeeselExpoId


        iolecmd_comand.Parameters.Add("@intAllBVs", OleDbType.Integer)
        iolecmd_comand.Parameters("@intAllBVs").Value = aint_AllVeeselVoyage

        iolecmd_comand.Parameters.Add("@intMINRange", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMINRange").Value = lint_MinRange

        iolecmd_comand.Parameters.Add("@intMAXRange", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMAXRange").Value = lint_MaxRange

        iolecmd_comand.Parameters.Add("@intExtraParam", OleDbType.Integer)
        iolecmd_comand.Parameters("@intExtraParam").Value = 0

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    <WebMethod()>
    Public Function InsertContainerCatalog(ByVal astr_Containter As String, ByVal aint_ShippingLineId As Integer, ByVal aint_ContISOCodeId As Integer, ByVal astr_ContainerDescription As String, ByVal aint_ContainerVerifyingDigit As Integer, ByVal adec_ContainerTare As Decimal, ByVal adec_ContainerMaxGrossWeight As Decimal, ByVal aintbln_ContIsShiperOwn As Integer, ByVal aint_ContainerOwnerId As Integer, ByVal aint_ContainerOwnerTypeId As Integer, ByVal astr_ContainerComments As String, ByVal astr_User As String) As String



        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lstr_result As String

        ' obtener el rango
        Try
            If astr_Containter.Length = 0 Then
                Return "Valor contenedor vacio"
            End If

        Catch ex As Exception

        End Try


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String

        idt_result = New DataTable("Result")
        strSQL = "spInsertContainerCatalog"


        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@intShippingLineId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContISOCodeId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerDescription", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@intContainerVerifyingDigit", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@decContainerTare", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@decContainerMaxGrossWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters.Add("@blnContIsShiperOwn", OleDbType.SmallInt)
        iolecmd_comand.Parameters.Add("@intContainerOwnerId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerOwnerTypeId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerComments", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strUser", OleDbType.VarChar)

        astr_User = astr_User + "_w"
        astr_Containter = astr_Containter.ToUpper().Trim().ToString

        iolecmd_comand.Parameters("@strContainerId").Value = astr_Containter
        iolecmd_comand.Parameters("@intShippingLineId").Value = aint_ShippingLineId
        iolecmd_comand.Parameters("@intContISOCodeId").Value = aint_ContISOCodeId
        iolecmd_comand.Parameters("@strContainerDescription").Value = astr_ContainerDescription
        iolecmd_comand.Parameters("@intContainerVerifyingDigit").Value = aint_ContainerVerifyingDigit
        iolecmd_comand.Parameters("@decContainerTare").Value = adec_ContainerTare
        iolecmd_comand.Parameters("@decContainerMaxGrossWeight").Value = adec_ContainerMaxGrossWeight
        iolecmd_comand.Parameters("@blnContIsShiperOwn").Value = aintbln_ContIsShiperOwn
        iolecmd_comand.Parameters("@intContainerOwnerId").Value = aint_ContainerOwnerId
        iolecmd_comand.Parameters("@intContainerOwnerTypeId").Value = aint_ContainerOwnerTypeId
        iolecmd_comand.Parameters("@strContainerComments").Value = astr_ContainerComments
        iolecmd_comand.Parameters("@strUser").Value = astr_User



        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        lstr_result = ""
        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
            If idt_result.Rows.Count = 1 And idt_result.Columns.Count = 1 Then
                lstr_result = idt_result(0)(0).ToString
            End If

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_result = strError
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return lstr_result


    End Function

    <WebMethod()>
    Public Function GetMasterBooking_ByUserId(ByVal aint_Userid As Integer) As DataTable



        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        ' obtener el rango
        Try
            If aint_Userid = 0 Then
                Return dt_RetrieveErrorTable("No existe usuario")
            End If

        Catch ex As Exception
        End Try


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String

        idt_result = New DataTable("Result")
        strSQL = "spGetBookingMasterList"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intShowCountersIn", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)

        iolecmd_comand.Parameters("@intBookingAdviceId").Value = 0
        iolecmd_comand.Parameters("@intApplyFilter").Value = 0
        iolecmd_comand.Parameters("@intShowCountersIn").Value = 1
        iolecmd_comand.Parameters("@intUserId").Value = aint_Userid


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function



    '''   -----''''
    <WebMethod()>
    Public Function GetMasterBookingValid_ByUserId(ByVal aint_Userid As Integer) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        ' obtener el rango
        Try
            If aint_Userid = 0 Then
                Return dt_RetrieveErrorTable("No existe usuario")
            End If

        Catch ex As Exception
        End Try


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String

        idt_result = New DataTable("Result")
        strSQL = "spGetBookingMasterList"

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intApplyFilter", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intShowCountersIn", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUserId", OleDbType.Integer)

        iolecmd_comand.Parameters("@intBookingAdviceId").Value = 0
        iolecmd_comand.Parameters("@intApplyFilter").Value = 1
        iolecmd_comand.Parameters("@intShowCountersIn").Value = 1
        iolecmd_comand.Parameters("@intUserId").Value = aint_Userid


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    ''' ------------'''


    <WebMethod()>
    Public Function GetISOCodeList() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion




        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String

        idt_result = New DataTable("Result")
        strSQL = "spGetISOCode"

        iolecmd_comand.Parameters.Add("@strContISOCodeAlias", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContISOCodeIdentifier", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContISOCodeDescription", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContainerTypeIdentifier", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContainerSizeIdentifier", OleDbType.VarChar)

        iolecmd_comand.Parameters("@strContISOCodeAlias").Value = ""
        iolecmd_comand.Parameters("@strContISOCodeIdentifier").Value = ""
        iolecmd_comand.Parameters("@strContISOCodeDescription").Value = ""
        iolecmd_comand.Parameters("@strContainerTypeIdentifier").Value = ""


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    ''
    <WebMethod()>
    Public Function SearchISOCodeByIdentifier(ByVal astr_ISOIdentifier As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion




        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String

        idt_result = New DataTable("Result")
        strSQL = "spGetISOCode"

        iolecmd_comand.Parameters.Add("@strContISOCodeAlias", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContISOCodeIdentifier", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContISOCodeDescription", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContainerTypeIdentifier", OleDbType.VarChar)
        iolecmd_comand.Parameters.Add("@strContainerSizeIdentifier", OleDbType.VarChar)

        iolecmd_comand.Parameters("@strContISOCodeAlias").Value = ""
        iolecmd_comand.Parameters("@strContISOCodeIdentifier").Value = astr_ISOIdentifier
        iolecmd_comand.Parameters("@strContISOCodeDescription").Value = ""
        iolecmd_comand.Parameters("@strContainerTypeIdentifier").Value = ""


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    ''
    <WebMethod()>
    Public Function SearchContainerToReception(ByVal aint_BookingAdvice As Integer, ByVal astr_Container As String) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String


        idt_result = New DataTable("VisitData")
        strSQL = "spGetContainerForVisit"

        iolecmd_comand.Parameters.Add("@strContainerLike", OleDbType.VarChar)
        iolecmd_comand.Parameters("@strContainerLike").Value = astr_Container

        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdvice

        iolecmd_comand.Parameters.Add("@strBooking", OleDbType.VarChar)
        iolecmd_comand.Parameters("@strBooking").Value = ""

        iolecmd_comand.Parameters.Add("@strService", OleDbType.VarChar)
        iolecmd_comand.Parameters("@strService").Value = ""


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result



    End Function


    <WebMethod()>
    Public Function GetServiceOrderData(ByVal aint_ServiceOrderId As Integer) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        'If aint_UserId >= 0 Then

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("datalist")
        strSQL = "spGetServiceOrderData"

        iolecmd_comand.Parameters.Add("intServiceOrderId", OleDbType.Integer)
        iolecmd_comand.Parameters("intServiceOrderId").Value = aint_ServiceOrderId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        'End If

        Return ldtb_Result

    End Function

    ''

    <WebMethod()>
    Public Function SearchUser_ByName(ByVal astr_Username As String) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT 	intUserId   AS 'intUserId'  , " &
                 " 	        strUserName AS 'strUserName', " &
                 " ( CASE WHEN  blnUserDisabled = 1 THEN 1 " &
                 "	  ELSE 0 " &
                 "    END " &
                 "	)  AS 	'blnUserDisabled' ," &
                 " ( CASE WHEN blnUserEntityIsEmployee = 1 THEN 1 " &
                 "     ELSE 0 " &
                 "    END  )   AS 'blnUserEntityIsEmployee' , " &
                 "  intUserEntityTypeId AS 'intUserEntityTypeId', " &
                 "  intUserEntityId AS 'intUserEntityId', " &
                 "  strUserEmail AS 'strUserEmail', " &
                 "  ( CASE WHEN blnUserActive =1 THEN 1 " &
                 "      ELSE 0 " &
                 "    END ) AS 'blnUserActive', " &
                 "  strUserComments AS 'strUserComments', " &
                 "  dtmUserCreationStamp AS 'dtmUserCreationStamp', " &
                 "  strUserCreatedBy AS 'strUserCreatedBy', " &
                 "  dtmUserLastModified AS 'dtmUserLastModified', " &
                 "  strUserLastModifiedBy AS 'strUserLastModifiedBy', " &
                 "  tblclsCompany.intCompanyId  AS 'intCompanyId', " &
                 "  tblclsCustomerType.strCustomerTypeIdentifier AS 'strCustomerTypeIdentifier', " &
                 "  tblclsCompany.strCompanyName AS 'strCompanyName' " &
                 " FROM  tblclsUser " &
                 "  	 LEFT JOIN tblclsCompanyEntity ON  tblclsCompanyEntity.intCompanyEntityId = tblclsUser.intUserEntityId " &
                 " 	                                   AND tblclsCompanyEntity.intCustomerTypeId  = tblclsUser.intUserEntityTypeId " &
                 "	     LEFT JOIN tblclsCustomerType  ON  tblclsCompanyEntity.intCustomerTypeId = tblclsCustomerType.intCustomerTypeId " &
                 "  	 LEFT JOIN tblclsCompany       ON  tblclsCompanyEntity.intCompanyId = tblclsCompany.intCompanyId " &
                 "	 WHERE tblclsUser.strUserName = '" + astr_Username + "'"

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function



    ''
    ''

    <WebMethod()>
    Public Function GetWEBGroups(ByVal aint_userProfileId As Integer) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String



        'Sentencia SQL 
        strSQL = " SELECT tblclsUserGroup.strUserGroupName, tblclsUserGroup.intUserGroupId  " &
                 "  FROM tblclsModuleOption MO " &
                 "   INNER JOIN tblclsModule ON tblclsModule.intModuleId = MO.intModuleId " &
                 "   INNER JOIN tblModuleOption_UserProfile ON tblModuleOption_UserProfile.intModuleOptionId =  MO.intModuleOptionId " &
                 "   INNER JOIN tblclsUserProfile ON tblclsUserProfile.intUserProfileId = tblModuleOption_UserProfile.intUserProfileId " &
                 "   INNER JOIN tblclsUserGroup   ON tblclsUserGroup.intUserGroupId     = tblclsUserProfile.intUserGroupId  " &
                 "   INNER JOIN tblUser_UserGroup ON tblUser_UserGroup.intUserGroupId = tblclsUserGroup.intUserGroupId  " &
                 " WHERE tblclsModule.intModuleId = 9 " &
                 "  AND  (  " &
                 "          SELECT COUNT(MOPx.intModuleOptionId) " &
                 "          FROM tblclsModuleOption MOPx " &
                 "              INNER JOIN tblModuleOption_UserProfile PROFBx ON PROFBx.intModuleOptionId =MOPx.intModuleOptionId  " &
                 "          WHERE PROFBx.intUserProfileId = tblclsUserProfile.intUserProfileId " &
                 "       ) = " &
                 "       (  " &
                 "         SELECT COUNT(MOPa.intModuleOptionId) " &
                 "          FROM tblclsModuleOption MOPa " &
                 "              INNER JOIN tblModuleOption_UserProfile PROFB ON PROFB.intModuleOptionId =MOPa.intModuleOptionId " &
                 "          WHERE PROFB.intUserProfileId = tblclsUserProfile.intUserProfileId " &
                 "           and MOPa.intModuleId = tblclsModule.intModuleId " &
                 "       )      " &
                 " AND  tblUser_UserGroup.intUserId =" + aint_userProfileId.ToString() &
                 "   GROUP BY tblclsUserGroup.strUserGroupName, tblclsUserGroup.intUserGroupId "


        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    '''
    <WebMethod()>
    Public Function SaveUser(ByVal astr_User As String, ByVal astr_Password As String, ByVal aint_CompanyEntity As Integer, ByVal aint_CompanyType As Integer, ByVal astr_UserMail As String, ByVal astr_UserComments As String, ByVal astr_CreatedBy As String) As DataTable

        Dim ldtb_Result = New DataTable("result") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        astr_User = astr_User.Trim()

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("user")
        strSQL = "spInsertUser"


        iolecmd_comand.Parameters.Add("strUserName", OleDbType.Char)
        iolecmd_comand.Parameters("strUserName").Value = astr_User

        iolecmd_comand.Parameters.Add("strPassword", OleDbType.Char)
        iolecmd_comand.Parameters("strPassword").Value = astr_Password

        iolecmd_comand.Parameters.Add("intCompanyEntity", OleDbType.Integer)
        iolecmd_comand.Parameters("intCompanyEntity").Value = aint_CompanyEntity

        iolecmd_comand.Parameters.Add("intCompanyType", OleDbType.Integer)
        iolecmd_comand.Parameters("intCompanyType").Value = aint_CompanyType

        iolecmd_comand.Parameters.Add("strUserEmail", OleDbType.Char)
        iolecmd_comand.Parameters("strUserEmail").Value = astr_UserMail

        iolecmd_comand.Parameters.Add("strUserComments", OleDbType.Char)
        iolecmd_comand.Parameters("strUserComments").Value = astr_UserComments

        iolecmd_comand.Parameters.Add("strCreatedBy", OleDbType.Char)
        iolecmd_comand.Parameters("strCreatedBy").Value = astr_CreatedBy


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        Try
            If ldtb_Result.Rows.Count = 0 Then
                Return dt_RetrieveErrorTable("No se encontro el contenedor")
            End If
        Catch ex As Exception

        End Try


        Return ldtb_Result


    End Function


    <WebMethod()>
    Public Function GetLikeProductList(ByVal astr_ProductName As String) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        If astr_ProductName.Length > 1 Then

            astr_ProductName = astr_ProductName.ToUpper()

            strSQL = " SELECT tblclsProduct.intProductId , tblclsProduct.strProductName " &
                     "  FROM tblclsProduct " &
                     " WHERE tblclsProduct.strProductName LIKE '%" + astr_ProductName + "%'" &
                     " AND tblclsProduct.blnProductActive = 1 "

            iolecmd_comand.CommandText = strSQL

            iAdapt_comand.SelectCommand = iolecmd_comand

            Try
                iolecmd_comand.Connection.Open()

                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(idt_result)
            Catch ex As Exception
                Dim strError As String
                strError = ObtenerError(ex.Message, 99999)
            Finally
                iolecmd_comand.Connection.Close()
                iAdapt_comand.SelectCommand.Connection.Close()
                ioleconx_conexion.Close()

                iolecmd_comand.Connection.Dispose()
                iAdapt_comand.SelectCommand.Connection.Dispose()
                ioleconx_conexion.Dispose()

            End Try

        End If

        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result

    End Function

    ''

    <WebMethod()>
    Public Function SetUserToGroup(ByVal aint_User As Integer, ByVal aint_GroupId As Integer, ByVal astr_CreatedBy As String) As DataTable

        Dim ldtb_Result = New DataTable("result") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("user")
        strSQL = "spSetUserToProfile"


        iolecmd_comand.Parameters.Add("intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserId").Value = aint_User

        iolecmd_comand.Parameters.Add("intUserGroup", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserGroup").Value = aint_GroupId

        iolecmd_comand.Parameters.Add("strCreatedBy", OleDbType.Char)
        iolecmd_comand.Parameters("strCreatedBy").Value = astr_CreatedBy


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        Try
            If ldtb_Result.Rows.Count = 0 Then
                Return dt_RetrieveErrorTable("No se encontro el contenedor")
            End If
        Catch ex As Exception

        End Try


        Return ldtb_Result


    End Function


    <WebMethod()>
    Public Function SetUserToProfile(ByVal aint_User As Integer, ByVal aint_GroupId As Integer, ByVal astr_CreatedBy As String) As DataTable

        Dim ldtb_Result = New DataTable("result") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("user")
        strSQL = "spSetUserToProfile"


        iolecmd_comand.Parameters.Add("intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserId").Value = aint_User

        iolecmd_comand.Parameters.Add("intUserGroup", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserGroup").Value = aint_GroupId

        iolecmd_comand.Parameters.Add("strCreatedBy", OleDbType.Char)
        iolecmd_comand.Parameters("strCreatedBy").Value = astr_CreatedBy


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        Try
            If ldtb_Result.Rows.Count = 0 Then
                Return dt_RetrieveErrorTable("No se encontro el contenedor")
            End If
        Catch ex As Exception

        End Try


        Return ldtb_Result


    End Function



    <WebMethod()>
    Public Function GetUserInformation(ByVal astr_Username As String) As DataTable

        Dim ldtb_Result = New DataTable("result") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("user")
        strSQL = "spGetUserData"


        iolecmd_comand.Parameters.Add("strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("strUsername").Value = astr_Username

        iolecmd_comand.Parameters.Add("intOption", OleDbType.Integer)
        iolecmd_comand.Parameters("intOption").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        Try
            If ldtb_Result.Rows.Count = 0 Then
                Return dt_RetrieveErrorTable("No se encontro el contenedor")
            End If
        Catch ex As Exception

        End Try


        Return ldtb_Result


    End Function



    ''

    <WebMethod()>
    Public Function teststruct(ByVal obj_Clas As ClsAdviceMasterData) As String
        Return ""
    End Function


    Public Function of_GetPortsByVessel_VesselVoyage(ByVal aint_VeselVoyage As Integer, ByVal aint_Vessel As Integer) As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String

        ' si no hay parametros con valores , retronar tabla vacia 
        If aint_VeselVoyage = 0 And aint_Vessel = 0 Then
            Return dt_RetrieveErrorTable("invalido")
        End If

        strSQL = " SELECT  DISTINCT tblclsPort.strPortId,  " &
                 "                 tblclsPort.strPortName, " &
                 "                tblclsPort.strCountryId  " &
                 "  FROM tblclsVesselVoyage " &
                 "  INNER JOIN tblclsVessel      ON tblclsVessel.intVesselId = tblclsVesselVoyage.intVesselId " &
                 "  INNER JOIN tblVessel_VesselRoute ON tblVessel_VesselRoute.intVesselId = tblclsVessel.intVesselId " &
                 "  INNER JOIN tblVesselRoute_Ports ON tblVesselRoute_Ports.intVesselRouteId = tblVessel_VesselRoute.intVesselRouteId " &
                 "  INNER JOIN tblclsPort ON tblclsPort.strPortId = tblVesselRoute_Ports.strPortId "

        If aint_VeselVoyage > 0 Then
            strSQL = strSQL + " WHERE tblclsVesselVoyage.intVesselVoyageId = " + aint_VeselVoyage.ToString()
        End If

        If aint_VeselVoyage = 0 And aint_Vessel > 0 Then
            strSQL = strSQL + " WHERE tblclsVessel.intVesselId = " + aint_Vessel.ToString()
        End If


        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    ''--->> of_getContainersTovisit

    <WebMethod()>
    Public Function GetContainersToInsert() As DataTable


        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        'Dim strcontainerid As String



        'strSQL = "  SELECT TOP 30 tblclsContainerInventory.intContRecepRequiredById, " & _
        '         "              tblclsContainerInventory.strContainerId , " & _
        '         "              tblclsContainerInventory.intContainerUniversalId " & _
        '         "  FROM(tblclsContainerInventory " & _
        '         " INNER JOIN tblclsContainerFiscalStatus ON tblclsContainerFiscalStatus.intContFisStatusId = tblclsContainerInventory.intContFisStatusId " & _
        '         "    LEFT JOIN tblclsContainerDeliveryDetail ON tblclsContainerDeliveryDetail.intContainerUniversalId = tblclsContainerInventory.intContainerUniversalId " & _
        '         "    LEFT JOIN tblclsContainerDelivery ON tblclsContainerDelivery.intContainerDeliveryId = tblclsContainerDeliveryDetail.intContainerDeliveryId " & _
        '         " WHERE(tblclsContainerInventory.blnContainerIsFull = 1 " & _
        '         " AND tblclsContainerInventory.blnContainerInvActive =1 " & _
        '         " And tblclsContainerInventory.intFiscalMovementId = 1 " & _
        '         " AND tblclsContainerFiscalStatus.strContFisStatusIdentifier IN ('LIBERADO','DESPACHADO') " & _
        '         " AND (" & _
        '         "      (" & _
        '         "          tblclsContainerDelivery.intContainerDeliveryId >0 " & _
        '         "          AND tblclsContainerDelivery.intContDelRequiredById = tblclsContainerInventory.intContRecepRequiredById " & _
        '         "          AND tblclsContainerDelivery.intContDelRequiredTypeId = tblclsContainerInventory.intContRecepRequiredById " & _
        '         "          AND tblclsContainerDeliveryDetail.intVisitId =0 " & _
        '         "      )" & _
        '         "    OR " & _
        '         "     ( " & _
        '         "         ISNULL(tblclsContainerDelivery.intContainerDeliveryId, 0) = 0 " & _
        '         "      ) " & _
        '         "    ) "


        strSQL = " SELECT TOP 30 tblclsContainerInventory.intContRecepRequiredById, " &
                 "    tblclsContainerInventory.strContainerId , " &
                 "    tblclsContainerInventory.intContainerUniversalId " &
                 " FROM  tblclsContainerInventory  " &
                 "    INNER JOIN tblclsContainerFiscalStatus ON tblclsContainerFiscalStatus.intContFisStatusId = tblclsContainerInventory.intContFisStatusId " &
                 " WHERE tblclsContainerInventory.blnContainerIsFull = 1 " &
                 " AND tblclsContainerInventory.blnContainerInvActive =1 " &
                 " AND tblclsContainerInventory.intFiscalMovementId = 1 " &
                 " AND tblclsContainerFiscalStatus.strContFisStatusIdentifier IN ('LIBERADO','DESPACHADO') " &
                 " AND ( NOT EXISTS  ( SELECT tblclsContainerDeliveryDetail.intContainerDeliveryId " &
                 "                       FROM tblclsContainerDeliveryDetail " &
                 "                       WHERE tblclsContainerDeliveryDetail.intContainerUniversalId = tblclsContainerInventory.intContainerUniversalId  " &
                 "                      ) " &
                 "    ) " &
                 " AND tblclsContainerInventory.intContRecepRequiredById > 0 "

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function


    <WebMethod()>
    Public Function DeleteVisitMaster(ByVal alng_visit As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        If alng_visit > 0 Then

            strSQL = " DELETE FROM tblclsVisit " &
                 " WHERE tblclsVisit.intVisitId = " + alng_visit.ToString()

            iolecmd_comand.CommandText = strSQL

            iAdapt_comand.SelectCommand = iolecmd_comand

            Try
                iolecmd_comand.Connection.Open()

                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(idt_result)
            Catch ex As Exception
                Dim strError As String
                strError = ObtenerError(ex.Message, 99999)
            Finally
                iolecmd_comand.Connection.Close()
                iAdapt_comand.SelectCommand.Connection.Close()
                ioleconx_conexion.Close()

                iolecmd_comand.Connection.Dispose()
                iAdapt_comand.SelectCommand.Connection.Dispose()
                ioleconx_conexion.Dispose()

            End Try

        End If

        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result

    End Function


    <WebMethod()>
    Public Function DeleteDeliverytMaster(ByVal alng_Delivery As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        If alng_Delivery > 0 Then

            strSQL = " DELETE FROM tblclsContainerDelivery " &
                 " WHERE  tblclsContainerDelivery.intContainerDeliveryId  = " + alng_Delivery.ToString()

            iolecmd_comand.CommandText = strSQL

            iAdapt_comand.SelectCommand = iolecmd_comand

            Try
                iolecmd_comand.Connection.Open()

                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(idt_result)
            Catch ex As Exception
                Dim strError As String
                strError = ObtenerError(ex.Message, 99999)
            Finally
                iolecmd_comand.Connection.Close()
                iAdapt_comand.SelectCommand.Connection.Close()
                ioleconx_conexion.Close()

                iolecmd_comand.Connection.Dispose()
                iAdapt_comand.SelectCommand.Connection.Dispose()
                ioleconx_conexion.Dispose()

            End Try

        End If

        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result

    End Function


    '''-------
    <WebMethod()>
    Public Function GetVisitReceptionData(ByVal alng_intVisitId As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String

        '  If alng_Delivery > 0 Then

        strSQL = " SELECT tblclsVisitContainer.intVisitId  as 'intvisitid', " &
                 " tblclsVisitContainer.strContainerId  as 'strcontainerid', " &
                 " tblclsContainerRecepDetail.intContainerReceptionId AS 'intreception'  , " &
                 " tblclsContainerRecepDetail.decContRecDetailWeight AS 'netweight'," &
                 " (tblclsContainerRecepDetail.decContRecDetailWeight + tblclsContainer.decContainerTare ) as 'bruteweight' ," &
                 " tblclsContainerType.strContainerTypeIdentifier as 'type' , " &
                 " tblclsContainerSize.strContainerSizeIdentifier as 'size', " &
                 " tblclsVisitContainer.intContainerUniversalId as 'intContainerUniversalId' " &
                 "  FROM tblclsVisitContainer " &
                 "  INNER JOIN tblclsVisit ON tblclsVisit.intVisitId = tblclsVisitContainer.intVisitId " &
                 "  INNER JOIN tblclsContainerRecepDetail ON tblclsContainerRecepDetail.intContainerReceptionId = tblclsVisitContainer.intServiceOrderId " &
                 "                                        AND tblclsVisitContainer.strContainerId = tblclsContainerRecepDetail.strContainerId " &
                 "  INNER JOIN tblclsContainerReception   ON tblclsContainerReception.intContainerReceptionId =  tblclsContainerRecepDetail.intContainerReceptionId " &
                 "                                       AND tblclsContainerReception.intServiceId = tblclsVisitContainer.intServiceId " &
                 "  INNER JOIN tblclsContainer   ON tblclsContainer.strContainerId = tblclsVisitContainer.strContainerId " &
                 "  INNER JOIN tblclsContainerISOCode ON tblclsContainerISOCode.intContISOCodeId = tblclsContainer.intContISOCodeId " &
                 "  INNER JOIN tblclsContainerType  ON tblclsContainerType.intContainerTypeId = tblclsContainerISOCode.intContainerTypeId " &
                 "  INNER JOIN tblclsContainerSize ON tblclsContainerSize.intContainerSizeId = tblclsContainerISOCode.intContainerSizeId " &
                 " WHERE tblclsVisit.intVisitId = " + alng_intVisitId.ToString()

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = 999999999
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try

        ' End If

        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result

    End Function


    ''' 
    '''
    <WebMethod()>
    Public Function SelectVisitRecepPosition(ByVal alng_intVisitId As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim ldt_groupVisit As DataTable = New DataTable("visitgroup")
        Dim ldt_visitResult As DataTable = New DataTable("visitresult")

        Dim ldt_visit_table As DataTable = New DataTable("tableresult")

        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        Dim strSQL As String

        Dim ldrow As DataRow
        Dim larow As DataRow
        Dim lint_find As Integer = 0
        Dim lint_counterA As Integer = 0
        Dim lint_counterB As Integer = 0
        Dim lint_tempA As Integer = 0
        Dim lint_tempB As Integer = 0
        Dim lint_tempC As Integer = 0
        Dim lint_tempD As Integer = 0
        Dim lint_tempE As Integer = 0
        Dim lint_tempF As Integer = 0
        Dim lint_tempG As Integer = 0

        Dim lint_indxA As Integer = 0
        Dim lint_indxB As Integer = 0
        Dim lint_indxC As Integer = 0
        Dim lint_indxD As Integer = 0
        Dim lint_indxE As Integer = 0
        Dim lint_indxF As Integer = 0
        Dim lint_indxG As Integer = 0
        Dim lint_indxH As Integer = 0

        Dim lstr_container As String = 0
        Dim lstr_containerType As String
        Dim lstr_ContainerSize As String
        Dim ldec_weight As Decimal
        Dim lint_countOthers As Integer
        Dim lint_countUse As Integer
        Dim lint_counteritemsinvisit


        Dim lrows_result() As DataRow
        Dim lrow_resultB() As DataRow
        Dim lrow_resultC() As DataRow
        '' crear una tabla con la estructura de los resultados filtrados 
        Dim ldt_filter_positions As DataTable = New DataTable("filterpositions")
        Dim ldt_filter_positionB As DataTable = New DataTable("filterpositions")
        Dim ldt_filter_tableB As DataTable = New DataTable("filterb")
        Dim ldt_filter_tableC As DataTable = New DataTable("filterc")
        Dim lstr_finalposicion As String
        Dim lrow_newrow As DataRow
        Dim llng_UniversalId As Long
        Dim lstr_row_value As String
        Dim lint_hasrows As String
        Dim lstr_limitcountervalue As String
        Dim lint_limitcountervalue As Integer



        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        'obtener los elementos del a visita 
        ldt_visit_table = GetVisitReceptionData(alng_intVisitId)

        '  If alng_Delivery > 0 Then

        strSQL = " exec spGetYardLocationForVisit  @intVisitId= " + alng_intVisitId.ToString()

        iolecmd_comand.CommandText = strSQL

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = 999999999

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            'Return dt_RetrieveErrorTable(ex.Message)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try

        ' End If
        ' Return idt_result
        'revisar la informacion 
        ' si no tiene renglones ni columnas , reotrnarlo la tabla vacia 
        Try
            If idt_result.Rows.Count = 0 And idt_result.Columns.Count = 0 Then
                Return idt_result
            End If
        Catch ex As Exception

        End Try
        'Return idt_result

        '' crear 2 nuevas tablas
        ldt_groupVisit.Columns.Add("strType", GetType(String))
        ldt_groupVisit.Columns.Add("strSize", GetType(String))
        ldt_groupVisit.Columns.Add("intCounter", GetType(Integer))


        '' tabla para insertar tipos y tamaños , y la columna de cantidad, primero se insertan las combinaciones tipo y tamaño, y luego se cuentan en visita las combinaciones

        lint_counterA = 0
        lint_indxA = 0
        lint_counteritemsinvisit = ldt_visit_table.Rows.Count()

        '' recorrer tabla 
        For Each dtvisititem As DataRow In ldt_visit_table.Rows
            lint_find = 0

            'lint_counterB = 0
            lint_indxB = 0
            '' recorrer  tabla de grupos
            For Each dtgroupitem As DataRow In ldt_groupVisit.Rows

                If lint_find = 0 Then

                    'leer y comparar
                    If dtgroupitem("strType") = dtvisititem("type") And dtgroupitem("strSize") = dtvisititem("size") Then
                        '' si existe 
                        '' incrementar

                        If Integer.TryParse(ldt_groupVisit.Rows(lint_indxB)("intCounter").ToString(), lint_tempA) = False Then
                            lint_tempA = 0
                        End If

                        lint_tempA = lint_tempA + 1

                        'asignarlo 
                        ldt_groupVisit.Rows(lint_indxB)("intCounter") = lint_tempA

                        'marcar como encontrado 
                        lint_find = 1
                    End If

                End If

                'lint_counterB = lint_counterB + 1
                lint_indxB = lint_indxB + 1

            Next

            'si no lo encontro agregarlo 
            If lint_find = 0 Then
                larow = ldt_groupVisit.NewRow
                larow("strType") = dtvisititem("type").ToString()
                larow("strSize") = dtvisititem("size").ToString()

                larow("intCounter") = 1
                ldt_groupVisit.Rows.Add(larow)

            End If

            'lint_counterA = lint_counterA + 1
            lint_indxA = lint_indxA + 1

        Next

        '' segunda tabla 
        ' ldt_visitResult(

        '' con columnas visita, contenedor, posicion , final 


        'ciclo externo, recorrer visita 
        lint_indxA = 0

        'agregar columnas para tabla de resultado
        ldt_visitResult.Columns.Add("intVisitId", GetType(Integer))
        ldt_visitResult.Columns.Add("strContainerId", GetType(String))
        ldt_visitResult.Columns.Add("intContainerUniversalId", GetType(Integer))
        ldt_visitResult.Columns.Add("strFinalPosition", GetType(String))



        'ldt_filter_positions.Columns.Add("strSize", GetType(String))
        'ldt_filter_positions.Columns.Add("intCounterUse", GetType(String))
        'ldt_filter_positions.Columns.Add("strPosition", GetType(String))
        'ldt_filter_positions.Columns.Add("intContainerType", GetType(String))
        'ldt_filter_positions.Columns.Add("strContainerType", GetType(String))
        'ldt_filter_positions.Columns.Add("decMinWeight", GetType(String))
        'ldt_filter_positions.Columns.Add("decMaxWeight", GetType(String))
        'ldt_filter_positions.Columns.Add("intcountothers", GetType(String))


        For Each itemVisit As DataRow In ldt_visit_table.Rows

            'obtener informacion del contenedor en la visita 
            lstr_container = itemVisit("strcontainerid").ToString
            lstr_ContainerSize = itemVisit("size").ToString
            lstr_containerType = itemVisit("type").ToString
            If Long.TryParse(itemVisit("intContainerUniversalId").ToString, llng_UniversalId) = False Then
                llng_UniversalId = 0
            End If


            '' filtrar por todos los resultados con tipo y tamaño correspondiente 

            'lrows_result = idt_result.Select("strSize='" + lstr_ContainerSize + "' and  strContainerType='" + lstr_containerType + "'")

            If idt_result.Columns.Count > 1 And idt_result.Rows.Count > 0 Then


                lrows_result = idt_result.Select("strSize='" + lstr_ContainerSize + "'")

                ldt_filter_positions = New DataTable("table")
                ldt_filter_positions = of_generatetableFromRowsList(lrows_result)
            Else
                ldt_filter_positions = New DataTable("table")
                Continue For
            End If

            If ldt_filter_positions.Columns.Count > 1 And ldt_filter_positions.Columns.Count > 1 Then
                lrows_result = ldt_filter_positions.Select("strContainerType='" + lstr_containerType + "'")



                '' generar una tabla apartir de los resultados filtrados
                ldt_filter_positions = New DataTable("table")
                If lrows_result.Length > 0 Then
                    ldt_filter_positions = of_generatetableFromRowsList(lrows_result)
                End If
            Else
                ldt_filter_positions = New DataTable("table")
                Continue For
            End If

            ' Return ldt_filter_positions


            'obtener la cantidad de contenedores del mismo tipo en la visita 
            lrow_resultB = ldt_groupVisit.Select("strType='" + lstr_containerType + "' and  strSize='" + lstr_ContainerSize + "'")

            If Integer.TryParse(lrow_resultB(0)("intCounter").ToString(), lint_counterA) = False Then
                lint_counterA = 0
            End If



            ' Return ldt_filter_positions
            ' primero a la tabla de filtrados ldt_filter_positions, agregar dos columnas numericas , que tengan los valores numericos de contadores 

            ldt_filter_positions.Columns.Add("intCounterNum", GetType(Integer))

            ldt_filter_positions.Columns.Add("intCounterOtherNum", GetType(Integer))

            ldt_filter_positions.Columns.Add("intCounter", GetType(Integer))

            ldt_filter_positions.Columns.Add("intAllCounter", GetType(Integer))

            'pasar los valores string a valores numericos en la tabla de poisiciones filtradas 
            lint_indxC = 0
            lint_tempB = 0

            ' se aprovechara para saber si tiene posiciones de row o solo llega a bays
            lint_hasrows = -1

            For lint_indxC = 0 To ldt_filter_positions.Rows.Count - 1

                lint_tempB = 0
                lint_tempC = 0
                ''
                If Integer.TryParse(ldt_filter_positions.Rows(lint_indxC)("intCounterUse").ToString(), lint_tempB) = False Then
                    lint_tempB = 0
                End If

                ldt_filter_positions.Rows(lint_indxC)("intCounterNum") = lint_tempB

                ''
                If Integer.TryParse(ldt_filter_positions.Rows(lint_indxC)("intcountothers").ToString(), lint_tempC) = False Then
                    lint_tempC = 0
                End If

                ldt_filter_positions.Rows(lint_indxC)("intCounterOtherNum") = lint_tempC
                ldt_filter_positions.Rows(lint_indxC)("intAllCounter") = lint_tempB + lint_tempC

                'ver si hay posiciones de row
                If lint_hasrows = -1 Then
                    Try
                        lint_indxF = -1
                        'obtener el rowid
                        lstr_row_value = ldt_filter_positions.Rows(lint_indxC)("intRowId").ToString()
                        If Integer.TryParse(lstr_row_value, lint_tempF) = False Then
                            lint_indxF = -1
                        End If

                        If lint_indxF > 0 Then
                            lint_hasrows = 1
                        End If
                    Catch ex As Exception

                    End Try

                End If


            Next

            ' Return ldt_filter_positions

            'revisar si tiene row y definir el contador
            If lint_hasrows > 0 Then
                'obtener contador 
                lstr_limitcountervalue = ConfigurationManager.AppSettings("CounterLimitForRow")
                'convertir
                If Integer.TryParse(lstr_limitcountervalue, lint_limitcountervalue) = False Then
                    lint_limitcountervalue = 0
                End If
            Else
                ' si no hay rows, obtener el limite para bahias 

                'obtener contador 
                lstr_limitcountervalue = ConfigurationManager.AppSettings("CounterLimitForBay")
                'convertir
                If Integer.TryParse(lstr_limitcountervalue, lint_limitcountervalue) = False Then
                    lint_limitcountervalue = 0
                End If
            End If

            'si se econtro valor 0, por default ponerle 10 
            If lint_limitcountervalue = 0 Then
                lint_limitcountervalue = 10
            End If

            'filtrar las posiciones que la cantidad de usados + los agrupados dan menores a 5 , en las dos columnas numericas se evaluara la suma 
            'lrow_resultC = ldt_filter_positions.Select(" ( intAllCounter + " + lint_counterA.ToString() + ") < 5 ")
            lrow_resultC = ldt_filter_positions.Select(" ( intAllCounter + " + lint_counterA.ToString() + ") < " + lint_limitcountervalue.ToString())

            'el primer registro es la posicion final 
            'reemplazar la tabla otra vez 
            If lrow_resultC.Length > 0 Then
                ldt_filter_positions = of_generatetableFromRowsList(lrow_resultC)
            End If

            'Return ldt_filter_positions
            'obtener el registro que tiene mayor cantidad de usados  del registro ldt_filter_positions.Select(" intAllCounter + " 
            lint_indxE = 0
            lint_indxD = -1
            lint_tempD = -1
            lint_tempE = -1

            For lint_indxE = 0 To ldt_filter_positions.Rows.Count - 1
                'obtener el valor total
                If Integer.TryParse(ldt_filter_positions(lint_indxE)("intAllCounter").ToString(), lint_tempE) = False Then
                    lint_tempE = -1
                End If

                If lint_tempE > lint_tempD Then
                    'indice nuevo
                    lint_indxD = lint_indxE

                    'vslor nuevo
                    lint_tempD = lint_tempE

                End If

            Next
            'el indice D, es el del renglon mayort
            'asignarla en lstr_finalposicion =
            If ldt_filter_positions.Rows.Count > 0 And lint_indxD >= 0 Then
                lstr_finalposicion = ldt_filter_positions(lint_indxD)("strPosition")

                lrow_newrow = ldt_visitResult.NewRow

                lrow_newrow("intVisitId") = alng_intVisitId
                lrow_newrow("strContainerId") = lstr_container
                lrow_newrow("intContainerUniversalId") = llng_UniversalId
                lrow_newrow("strFinalPosition") = lstr_finalposicion

                ldt_visitResult.Rows.Add(lrow_newrow)
                'insertar el nuevo registro en la tabla de resultados , tambien insertar con universal 
                lint_indxA = lint_indxA + 1
            End If

        Next
        'ciclo interno recorrer vista 

        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        'RETORNA 
        Return ldt_visitResult

    End Function

    '''
    <WebMethod()>
    Public Function UpdateVisitPositionFromList(ByVal alng_VisitId As Long, ByVal adtb_PositionList As DataTable, ByVal astr_UserName As String) As String

        '-----------------------------

        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()
        Dim ldtb_SaveResult As DataTable = New DataTable("")
        Dim llng_ServiceOrderId As Long

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim lstr_Container As String
        Dim lstr_Position As String
        Dim lstr_error As String
        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        '' validar 
        '' si no hay visita 
        If alng_VisitId = 0 Then
            Return "" '' no hay viista
        End If
        '''''''''''''

        '' tabla
        ''''''''''''''''''''''''''''
        ''revisar tabla 
        If adtb_PositionList.Rows.Count = 0 Then
            Return "tabla vacia"
        End If

        Try
            lstr_error = ""
            For Each litem As DataRow In adtb_PositionList.Rows
                lstr_Container = litem("strContainerId").ToString()
                lstr_Position = litem("strFinalPosition").ToString()
            Next

        Catch ex As Exception
            lstr_error = ex.Message
            Return lstr_error
        End Try

        '' fin revision tabla 
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""
        '' crear los parametros 
        'agregar parametros
        iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strYardPosition", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)

        lstr_SQL = "spUpdateVisitYardPosItem"

        '''''

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandText = lstr_SQL




        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        ''ejecutar 
        adapter = New OleDbDataAdapter(iolecmd_comand)
        ''''''''''''''''''''

        For Each litem_row As DataRow In adtb_PositionList.Rows

            iolecmd_comand.Parameters("@intVisitId").Value = alng_VisitId

            lstr_Container = litem_row("strContainerId").ToString()
            lstr_Position = litem_row("strFinalPosition").ToString()

            iolecmd_comand.Parameters("@strContainerId").Value = lstr_Container
            iolecmd_comand.Parameters("@strYardPosition").Value = lstr_Position
            iolecmd_comand.Parameters("@strUsername").Value = astr_UserName


            Try
                ''conectar
                iolecmd_comand.Connection.Open()

                adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
                adapter.Fill(ldt_TableResult)

                '' si hay resultado , retornar
                'If ldt_TableResult.Rows.Count > 0 And ldt_TableResult.Columns.Count > 0 Then
                '    Return ldt_TableResult(0)(0).ToString()
                'End If
                ''desconectar
            Catch ex As Exception

                lstr_Message = ObtenerError(ex.Message, 9999)

            Finally
                iolecmd_comand.Connection.Close()
                ' iolecmd_comand.Connection.Dispose()
                'ioleconx_conexion.close()
            End Try

        Next


        Return ""
    End Function

    <WebMethod()>
    Public Function SetYardPositiontoVisit(ByVal alng_VisitId As Long, ByVal astr_UserName As String) As String

        Dim ldt_tablePositions As DataTable = New DataTable("data")
        Dim lstr_pos As String
        Dim lstr_result As String
        Dim lint_table_ok As Integer
        lint_table_ok = -1

        ldt_tablePositions = SelectVisitRecepPosition(alng_VisitId)

        '' si hay posiciones ( renglon y columnas ,y el primer renglon tiene 
        Try
            If ldt_tablePositions.Rows.Count > 0 And ldt_tablePositions.Columns.Count > 0 Then

                lstr_pos = ""
                lstr_pos = ldt_tablePositions.Rows(0)("strFinalPosition").ToString()

                If lstr_pos.Length > 1 Then
                    lint_table_ok = 1
                End If
            End If

        Catch ex As Exception

        End Try

        '' si la tabla e informacion es valida

        lstr_result = UpdateVisitPositionFromList(alng_VisitId, ldt_tablePositions, astr_UserName)
        Return ""
    End Function

    ''--

    Public Function of_SaveDelGetIMOAdvice(ByVal aint_BookingAdviceId As Integer, ByVal aint_Item As Integer, ByVal aint_IMOCode As Integer, ByVal aint_UNCode As Integer, ByVal aint_Operation As Integer, ByVal astr_UserName As String) As String

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intItem", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intIMOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUNCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUserName", OleDbType.Char)

        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdviceId
        iolecmd_comand.Parameters("@intItem").Value = aint_Item
        iolecmd_comand.Parameters("@intIMOCode").Value = aint_IMOCode
        iolecmd_comand.Parameters("@intUNCode").Value = aint_UNCode
        iolecmd_comand.Parameters("@intOperation").Value = aint_Operation
        iolecmd_comand.Parameters("@strUserName").Value = astr_UserName

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveReadAdviceIMO"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return "error al actualizar informacion "
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''
        Return ""
    End Function

    <WebMethod()>
    Public Function GetIMOAdviceAditionalList(ByVal aint_BookingAdviceId As Integer) As DataTable

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intItem", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intIMOCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intUNCode", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUserName", OleDbType.Char)

        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_BookingAdviceId
        iolecmd_comand.Parameters("@intItem").Value = 0
        iolecmd_comand.Parameters("@intIMOCode").Value = 0
        iolecmd_comand.Parameters("@intUNCode").Value = 0
        iolecmd_comand.Parameters("@intOperation").Value = 4
        iolecmd_comand.Parameters("@strUserName").Value = ""

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveReadAdviceIMO"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ldt_TableResult.TableName = "resultimo"
            Return ldt_TableResult
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return dt_RetrieveErrorTable(lstr_Message)
            Else
                Return dt_RetrieveErrorTable(ex.Message)
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        '''''''''''''''''''''''''''''''''
        Return dt_RetrieveErrorTable("Vacio")
    End Function

    ''''---
    Public Function of_SaveUpdateDelNote(ByVal aint_bookingid As Integer, ByVal aobj_Note As ClsNoteAdvice, ByVal aint_Operation As Integer, ByVal astr_UserName As String) As String


        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)

        ''''
        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_bookingid
        iolecmd_comand.Parameters("@intNoteItem").Value = aobj_Note.iint_NoteItem
        iolecmd_comand.Parameters("@strContainerId").Value = aobj_Note.istr_strContainerId
        iolecmd_comand.Parameters("@strText").Value = aobj_Note.istr_Text
        iolecmd_comand.Parameters("@intNoteType").Value = aobj_Note.int_NoteType
        iolecmd_comand.Parameters("@strStatus").Value = aobj_Note.str_Status
        iolecmd_comand.Parameters("@blnActive").Value = aobj_Note.int_Active
        iolecmd_comand.Parameters("@blnChecked").Value = aobj_Note.int_Checked
        iolecmd_comand.Parameters("@strAditionalComs").Value = aobj_Note.str_AditionalComs

        'la operacion 
        If aint_Operation = 0 Then
            iolecmd_comand.Parameters("@intOperation").Value = aobj_Note.iint_operation
        Else
            iolecmd_comand.Parameters("@intOperation").Value = aint_Operation
        End If

        iolecmd_comand.Parameters("@strUsername").Value = astr_UserName

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveReadAdviceNote"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return lstr_ex
        End Try

        '''''''''''''''''''''''''''''''''
        Return ""

    End Function


    Public Function of_ReadNote(ByVal aint_bookingid As Integer, ByVal aobj_Note As ClsNoteAdvice, ByVal aint_Operation As Integer) As DataTable


        ''''''''''''''''''''''''''
        '-----------------------------


        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_TableResult = New DataTable()
        ldt_TableResult.TableName = "TableResult"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intBookingAdviceId", OleDbType.Integer)

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)

        ''''
        ''''''''''''''
        ''validar campos textos
        If aobj_Note.istr_Text Is Nothing Then
            aobj_Note.istr_Text = ""
        End If
        ''
        If aobj_Note.str_Status Is Nothing Then
            aobj_Note.str_Status = ""
        End If
        ''
        If aobj_Note.str_AditionalComs Is Nothing Then
            aobj_Note.str_AditionalComs = ""
        End If

        '''
        'asignar valores 
        iolecmd_comand.Parameters("@intBookingAdviceId").Value = aint_bookingid
        iolecmd_comand.Parameters("@intNoteItem").Value = aobj_Note.iint_NoteItem
        iolecmd_comand.Parameters("@strContainerId").Value = aobj_Note.istr_strContainerId
        iolecmd_comand.Parameters("@strText").Value = aobj_Note.istr_Text
        iolecmd_comand.Parameters("@intNoteType").Value = aobj_Note.int_NoteType
        iolecmd_comand.Parameters("@strStatus").Value = aobj_Note.str_Status
        iolecmd_comand.Parameters("@blnActive").Value = aobj_Note.int_Active
        iolecmd_comand.Parameters("@blnChecked").Value = aobj_Note.int_Checked
        iolecmd_comand.Parameters("@strAditionalComs").Value = aobj_Note.str_AditionalComs

        'la operacion 
        If aint_Operation = 0 Then
            iolecmd_comand.Parameters("@intOperation").Value = aobj_Note.iint_operation
        Else
            iolecmd_comand.Parameters("@intOperation").Value = aint_Operation
        End If

        iolecmd_comand.Parameters("@strUsername").Value = ""

        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveReadAdviceNote"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            Return ldt_TableResult
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            Return dt_RetrieveErrorTable(lstr_Message)

        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"


        '''''''''''''''''''''''''''''''''
        Return ldt_TableResult

    End Function

    <WebMethod()>
    Public Function InsertNote(ByVal aint_Booking As Integer, ByVal aobj_note As ClsNoteAdvice, ByVal astr_username As String) As String
        Dim lstr_result As String

        If aint_Booking > 0 Then
            If aobj_note.iint_NoteItem = 0 Then
                lstr_result = of_SaveUpdateDelNote(aint_Booking, aobj_note, 1, astr_username)
            Else
                lstr_result = of_SaveUpdateDelNote(aint_Booking, aobj_note, 2, astr_username)
            End If

            Return lstr_result

        End If

        Return ""
    End Function

    <WebMethod()>
    Public Function DeleteNote(ByVal aint_Booking As Integer, ByVal aint_itemBooking As Integer, ByVal astr_username As String) As String
        Dim lstr_result As String


        Dim aobj_note As ClsNoteAdvice = New ClsNoteAdvice()
        aobj_note.iint_NoteItem = aint_itemBooking
        aobj_note.iint_operation = 3


        If aint_Booking > 0 And aint_itemBooking > 0 Then

            lstr_result = of_SaveUpdateDelNote(aint_Booking, aobj_note, 3, astr_username)
            Return lstr_result

        End If

        Return ""
    End Function

    <WebMethod()>
    Public Function MarkCheckedNote(ByVal aint_Booking As Integer, ByVal aint_itemBooking As Integer, ByVal astr_username As String) As String
        Dim lstr_result As String


        Dim aobj_note As ClsNoteAdvice = New ClsNoteAdvice()
        aobj_note.iint_NoteItem = aint_itemBooking
        aobj_note.iint_operation = 2
        aobj_note.int_Checked = 1
        aobj_note.str_Status = "CHECKED"


        If aint_Booking > 0 And aint_itemBooking > 0 Then

            lstr_result = of_SaveUpdateDelNote(aint_Booking, aobj_note, 2, astr_username)
            Return lstr_result

        End If

        Return ""
    End Function


    <WebMethod()>
    Public Function UpdateNote(ByVal aint_Booking As Integer, ByVal aobj_note As ClsNoteAdvice, ByVal astr_username As String) As String

        Dim lstr_result As String

        If aint_Booking > 0 Then

            lstr_result = of_SaveUpdateDelNote(aint_Booking, aobj_note, 2, astr_username)
            Return lstr_result

        End If

        Return ""
    End Function

    <WebMethod()>
    Public Function GetNotesForMaster(ByVal aint_Booking As Integer) As DataTable

        Dim ldt_table_result As DataTable = New DataTable("tableResult")

        Dim aobj_note As ClsNoteAdvice = New ClsNoteAdvice()

        aobj_note.iint_NoteItem = 0
        aobj_note.iint_operation = 4
        aobj_note.istr_strContainerId = ""


        If aint_Booking > 0 Then

            ldt_table_result = of_ReadNote(aint_Booking, aobj_note, 4)
            Return ldt_table_result

        End If

        Return ldt_table_result
    End Function

    <WebMethod()>
    Public Function GetNotesForContainer(ByVal aint_Booking As Integer, ByVal astr_Container As String) As DataTable

        Dim ldt_table_result As DataTable = New DataTable("tableResult")

        Dim aobj_note As ClsNoteAdvice = New ClsNoteAdvice()

        aobj_note.iint_NoteItem = 0
        aobj_note.istr_strContainerId = astr_Container
        aobj_note.iint_operation = 4

        If aint_Booking > 0 Then

            ldt_table_result = of_ReadNote(aint_Booking, aobj_note, 4)
            Return ldt_table_result

        End If

        Return ldt_table_result

    End Function


    <WebMethod()>
    Public Function IsReadyItemOnNotes(ByVal aint_Booking As Integer, ByVal astr_Container As String, ByVal aint_ItemNoteId As Integer) As Integer

        Dim ldt_table_result As DataTable = New DataTable("tableResult")

        Dim aobj_note As ClsNoteAdvice = New ClsNoteAdvice()

        Dim lint_count_idx As Integer = 0
        Dim lint_count_checked As Integer = 0


        aobj_note.iint_NoteItem = 0
        aobj_note.istr_strContainerId = astr_Container
        aobj_note.iint_NoteItem = aint_ItemNoteId
        aobj_note.iint_operation = 6

        If aint_Booking > 0 Then

            ldt_table_result = of_ReadNote(aint_Booking, aobj_note, 6)

            ' si no hay resutltado retornar -1 
            If ldt_table_result.Rows.Count = 0 Then
                Return -1
            End If

            Try
                '  recorrer todas la notas en su status CHECKED , si todas las notas estan CHECKED, retornar 1 sino 0 
                For lint_count_idx = 0 To ldt_table_result.Rows.Count - 1
                    If ldt_table_result(lint_count_idx)("strStatus") = "CHECKED" Then
                        lint_count_checked = lint_count_checked + 1
                    End If

                Next

                If lint_count_checked = ldt_table_result.Rows.Count Then
                    Return 1
                Else
                    Return 0
                End If

            Catch ex As Exception
                Return -1

            End Try

            Return 0

        End If

        Return -1

    End Function

    <WebMethod()>
    Public Function GetAllNotesForMaster(ByVal aint_Booking As Integer) As DataTable

        Dim ldt_table_result As DataTable = New DataTable("tableResult")

        Dim aobj_note As ClsNoteAdvice = New ClsNoteAdvice()

        aobj_note.iint_NoteItem = 0
        aobj_note.istr_strContainerId = ""
        aobj_note.iint_operation = 5

        If aint_Booking > 0 Then

            ldt_table_result = of_ReadNote(aint_Booking, aobj_note, 5)
            Return ldt_table_result

        End If

        Return ldt_table_result

    End Function

    <WebMethod()>
    Public Function CorrectStringFromASCII(ByVal astr_StringParameter As String) As String

        Dim lstr_tempstring As String = ""
        Dim lstr_finalString As String = ""
        Dim lobj_char As Char

        lstr_tempstring = of_convertoasccistring(astr_StringParameter)

        'recorrer que 
        For Each itemchar As Char In lstr_tempstring

            If Char.IsLetter(itemchar) = True Then
                lstr_finalString = lstr_finalString + itemchar
            End If

            If Char.IsNumber(itemchar) = True Then
                lstr_finalString = lstr_finalString + itemchar
            End If

            'If Char.IsPunctuation(itemchar) = True Then
            '    lstr_finalString = lstr_finalString + itemchar
            'End If

            '  If Char.IsWhiteSpace(itemchar) = True Then
            'lstr_finalString = lstr_finalString + itemchar
            ' End If

            If itemchar = "." Or itemchar = "," Or itemchar = " " Or itemchar = "." Or itemchar = "-" Or itemchar = "_" Then
                lstr_finalString = lstr_finalString + itemchar
            End If
        Next

        Return lstr_finalString

    End Function


    <WebMethod()>
    Public Function SaveDetailAdviceCustomer(ByVal aint_iintAdviceId As Integer, ByVal acontainers_list As ClsAdviceDetailDataBooking(), ByVal astr_userId As String) As List(Of ClsAdviceResult) ' As ClsAdviceResult() 'As DataTable

        Dim lstr_result As String
        Dim lint_BookingAdvice As Integer
        Dim ldtb_Table As DataTable = New DataTable("result")
        Dim ldtb_TableResult As DataTable = New DataTable("resultB")
        Dim lrow As DataRow
        Dim lobjlist() As ClsAdviceResult
        Dim lobj_resultitem As ClsAdviceResult
        Dim llist_toReturn As List(Of ClsAdviceResult) = New List(Of ClsAdviceResult)
        Dim llist_returncall As List(Of ClsAdviceResult)
        ''''''''''''----
        ''''----

        'si hay detalles por autorizar 
        If acontainers_list IsNot Nothing Then

            If acontainers_list.Count > 0 And aint_iintAdviceId > 0 Then

                For lint_idxA As Integer = 0 To acontainers_list.Count - 1
                    acontainers_list(lint_idxA).iint_OperationType = 4
                Next

                'ldtb_TableResult = of_saveDetailAdvice(aint_iintAdviceId, acontainers_list, astr_userId)
                llist_returncall = of_saveDetailAdvice(aint_iintAdviceId, acontainers_list, astr_userId)

                ' If ldtb_TableResult.Rows.Count = 1 And ldtb_TableResult.Columns.Count = 1 Then
                'Return dt_RetrieveErrorTable(ldtb_TableResult(0)(0).ToString)
                'End If

                'Return dt_RetrieveErrorTable("Tablaxvv " + ldtb_TableResult.Rows.Count.ToString() + "-" + ldtb_TableResult.Columns.Count.ToString())
                'Return dt_RetrieveErrorTable("Tablaxvv")
                '   Return ldtb_TableResult
                Return llist_returncall
                'Return dt_RetrieveErrorTable("Tablaxvv " + ldtb_TableResult(0)(0).ToString())
                'si no hay 
            Else

                'Return dt_RetrieveErrorTable("datos invalidos")
                ReDim lobjlist(1)
                lobjlist(0) = New ClsAdviceResult
                lobjlist(0).iint_Succes = 0
                lobjlist(0).istr_Message = "Datos invalidos"

                lobj_resultitem = New ClsAdviceResult()
                lobj_resultitem.iint_AdviceId = 0
                lobj_resultitem.iint_Succes = 0
                lobj_resultitem.istr_Container = ""
                lobj_resultitem.istr_Message = "Datos invalidos"
                llist_toReturn.Add(lobj_resultitem)
            End If 'If aobj_Advice.iobjs_ContainerList.Count > 0 Then

            'Return dt_RetrieveErrorTable("datos invalidos")
            Return llist_toReturn

        End If
        ''''''''

        ''
        '''

        ''<<<------- of_getContainersTovisit

        '     Return ldtb_TableResult
        Return llist_toReturn
    End Function

    <WebMethod()>
    Public Function SetAdvicesToUserName(ByVal astr_UserName As String, ByVal astr_type As String) As DataTable

        Dim lstr_result As String
        Dim lint_BookingAdvice As Integer
        Dim ldtb_Table As DataTable = New DataTable("result")
        Dim ldtb_TableResult As DataTable = New DataTable("resultB")
        Dim lrow As DataRow


        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lint_operation As Integer = 0
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_TableResult = New DataTable()
        ldt_TableResult.TableName = "TableResult"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""


        'agregar parametros
        iolecmd_comand.Parameters.Add("@strUser", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strType", OleDbType.Char)


        ''''''''''''''
        'asignar valores 
        iolecmd_comand.Parameters("@strUser").Value = astr_UserName
        iolecmd_comand.Parameters("@strType").Value = astr_type


        'definir la cadena sql
        lstr_SQL = "spSetAdvicesToUser"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            Return ldt_TableResult
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            Return dt_RetrieveErrorTable(lstr_Message)

        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"


        '''''''''''''''''''''''''''''''''
        Return ldt_TableResult

        Return ldtb_TableResult
    End Function


    ''

    <WebMethod()>
    Public Function CheckStockRestriction(ByVal aobj_ClsStockRestriction As ClsStockRestriction) As String

        Dim ldtb_Result = New DataTable("result") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("user")
        strSQL = "spSaveReadStockRestricions"


        iolecmd_comand.Parameters.Add("intRestrictionId", OleDbType.Integer)
        iolecmd_comand.Parameters("intRestrictionId").Value = 0

        iolecmd_comand.Parameters.Add("intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters("intShippingLine").Value = aobj_ClsStockRestriction.intShippingLineId

        iolecmd_comand.Parameters.Add("intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters("intContainerType").Value = aobj_ClsStockRestriction.intContainerType

        iolecmd_comand.Parameters.Add("intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters("intContainerSize").Value = aobj_ClsStockRestriction.intContainerSize

        iolecmd_comand.Parameters.Add("intISOCOde", OleDbType.Integer)
        iolecmd_comand.Parameters("intISOCOde").Value = 0

        iolecmd_comand.Parameters.Add("strStartYear", OleDbType.Char)
        iolecmd_comand.Parameters("strStartYear").Value = ""

        iolecmd_comand.Parameters.Add("strStartMonth", OleDbType.Char)
        iolecmd_comand.Parameters("strStartMonth").Value = ""

        iolecmd_comand.Parameters.Add("strStarDay", OleDbType.Char)
        iolecmd_comand.Parameters("strStarDay").Value = ""

        iolecmd_comand.Parameters.Add("strEndYear", OleDbType.Char)
        iolecmd_comand.Parameters("strEndYear").Value = ""

        iolecmd_comand.Parameters.Add("strEndMonth", OleDbType.Char)
        iolecmd_comand.Parameters("strEndMonth").Value = ""

        iolecmd_comand.Parameters.Add("strEndDay", OleDbType.Char)
        iolecmd_comand.Parameters("strEndDay").Value = ""

        iolecmd_comand.Parameters.Add("strUserName", OleDbType.Char)
        iolecmd_comand.Parameters("strUserName").Value = ""

        iolecmd_comand.Parameters.Add("intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters("intOperation").Value = 5

        iolecmd_comand.Parameters.Add("strComments", OleDbType.Char)
        iolecmd_comand.Parameters("strComments").Value = ""



        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            Dim lstr_result = ldtb_Result(0)(0)
            Return lstr_result.ToString.Trim()
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        Return ""


    End Function

    'Public Function GetStockRestrictions() As DataTable
    <WebMethod()>
    Public Function GetStockRestrictions() As List(Of ClsStockRestriction)

        Dim ldtb_Result = New DataTable("result") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llist_restriction As List(Of ClsStockRestriction) = New List(Of ClsStockRestriction)
        Dim lobj_restriction As ClsStockRestriction
        Dim lint_day, lint_month, lint_year As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("user")
        strSQL = "spSaveReadStockRestricions"


        iolecmd_comand.Parameters.Add("intRestrictionId", OleDbType.Integer)
        iolecmd_comand.Parameters("intRestrictionId").Value = 0

        iolecmd_comand.Parameters.Add("intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters("intShippingLine").Value = 0

        iolecmd_comand.Parameters.Add("intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters("intContainerType").Value = 0

        iolecmd_comand.Parameters.Add("intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters("intContainerSize").Value = 0

        iolecmd_comand.Parameters.Add("intISOCOde", OleDbType.Integer)
        iolecmd_comand.Parameters("intISOCOde").Value = 0

        iolecmd_comand.Parameters.Add("strStartYear", OleDbType.Char)
        iolecmd_comand.Parameters("strStartYear").Value = ""

        iolecmd_comand.Parameters.Add("strStartMonth", OleDbType.Char)
        iolecmd_comand.Parameters("strStartMonth").Value = ""

        iolecmd_comand.Parameters.Add("strStarDay", OleDbType.Char)
        iolecmd_comand.Parameters("strStarDay").Value = ""

        iolecmd_comand.Parameters.Add("strEndYear", OleDbType.Char)
        iolecmd_comand.Parameters("strEndYear").Value = ""

        iolecmd_comand.Parameters.Add("strEndMonth", OleDbType.Char)
        iolecmd_comand.Parameters("strEndMonth").Value = ""

        iolecmd_comand.Parameters.Add("strEndDay", OleDbType.Char)
        iolecmd_comand.Parameters("strEndDay").Value = ""

        iolecmd_comand.Parameters.Add("strUserName", OleDbType.Char)
        iolecmd_comand.Parameters("strUserName").Value = ""

        iolecmd_comand.Parameters.Add("intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters("intOperation").Value = 4


        iolecmd_comand.Parameters.Add("strComments", OleDbType.Char)
        iolecmd_comand.Parameters("strComments").Value = ""


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand

            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            ''recorrer la tabla 
            For Each item As DataRow In ldtb_Result.Rows
                lobj_restriction = New ClsStockRestriction()
                lobj_restriction.intContainerSize = 0
                lobj_restriction.intContainerType = 0
                lobj_restriction.intShippingLineId = 0
                lobj_restriction.strShippingLineIdentifier = ""
                lobj_restriction.strStartDate = ""
                lobj_restriction.strEndDate = ""

                'id
                Integer.TryParse(item("ID").ToString(), lobj_restriction.intRestrictionId)
                'tipo
                Integer.TryParse(item("intContainerType").ToString(), lobj_restriction.intContainerType)
                'tam
                Integer.TryParse(item("intContainerSize").ToString(), lobj_restriction.intContainerSize)
                'linea
                Integer.TryParse(item("intShippingLineId").ToString(), lobj_restriction.intShippingLineId)
                'nombre linea
                lobj_restriction.strShippingLineIdentifier = item("strShippingLineIdentifier").ToString()

                lint_day = 0
                lint_month = 0
                lint_year = 0
                Integer.TryParse(item("STARTD").ToString(), lint_day)
                Integer.TryParse(item("STARTM").ToString(), lint_month)
                Integer.TryParse(item("STARTY").ToString(), lint_year)
                lobj_restriction.dtmStartDate = New Date(lint_year, lint_month, lint_day)


                lint_day = 0
                lint_month = 0
                lint_year = 0
                Integer.TryParse(item("ENDD").ToString(), lint_day)
                Integer.TryParse(item("ENDM").ToString(), lint_month)
                Integer.TryParse(item("ENDY").ToString(), lint_year)
                lobj_restriction.dtmEndDate = New Date(lint_year, lint_month, lint_day)

                llist_restriction.Add(lobj_restriction)
            Next
            ''''''''''''''
            'Return ldtb_Result(0)(0)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        'Return ldtb_Result
        Return llist_restriction


    End Function
    ''
    <WebMethod()>
    Public Function SaveUpdateStockRestriction(ByVal aobj_StockRestriction As ClsStockRestriction, ByVal aint_Operation As Integer, ByVal astr_UserName As String) As String

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim ldt_AdviceResult As DataTable 'tabla que guardara el resultado del query
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion
        Dim lparamGeneric As OleDbParameter = New OleDbParameter()

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_TableResult As DataTable
        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow
        Dim lstr_StatDate As String
        Dim lstr_EndDate As String
        Dim ldtm_NullDate As Date = New Date(1900, 1, 1)

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ldt_TableResult = New DataTable()

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultAdvice"
        'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

        ' Return "holax"


        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0
        lint_itemscount = lint_itemscount + 1
        'limpiar cadena sql
        lstr_SQL = ""

        'validar fechas 
        lstr_StatDate = ""
        lstr_StatDate = of_ConvertDateToStringGeneralFormat(aobj_StockRestriction.dtmStartDate)
        If lstr_StatDate.Length < 2 Then
            aobj_StockRestriction.dtmStartDate = ldtm_NullDate
        End If


        'validar fechas fin
        lstr_EndDate = ""
        lstr_EndDate = of_ConvertDateToStringGeneralFormat(aobj_StockRestriction.dtmEndDate)
        If lstr_EndDate.Length < 2 Then
            aobj_StockRestriction.dtmEndDate = ldtm_NullDate
        End If


        'agregar parametros
        iolecmd_comand.Parameters.Add("@intRestrictionId", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intShippingLine", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerType", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intContainerSize", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@intISOCOde", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strStartYear", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strStartMonth", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strStarDay", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strEndYear", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strEndMonth", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strEndDay", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@strUserName", OleDbType.Char)
        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Integer)
        iolecmd_comand.Parameters.Add("@strComments", OleDbType.Char)

        ''''
        ''''''''''''''
        'asignar valores 
        ''
        iolecmd_comand.Parameters("@intRestrictionId").Value = aobj_StockRestriction.intRestrictionId
        iolecmd_comand.Parameters("@intShippingLine").Value = aobj_StockRestriction.intShippingLineId
        iolecmd_comand.Parameters("@intContainerType").Value = aobj_StockRestriction.intContainerType
        iolecmd_comand.Parameters("@intContainerSize").Value = aobj_StockRestriction.intContainerSize
        iolecmd_comand.Parameters("@intISOCOde").Value = aobj_StockRestriction.intContainerISOCode
        iolecmd_comand.Parameters("@strStartYear").Value = of_getDatePartStr(aobj_StockRestriction.dtmStartDate, "YEAR")
        iolecmd_comand.Parameters("@strStartMonth").Value = of_getDatePartStr(aobj_StockRestriction.dtmStartDate, "MONTH")
        iolecmd_comand.Parameters("@strStarDay").Value = of_getDatePartStr(aobj_StockRestriction.dtmStartDate, "DAY")
        iolecmd_comand.Parameters("@strEndYear").Value = of_getDatePartStr(aobj_StockRestriction.dtmEndDate, "YEAR")
        iolecmd_comand.Parameters("@strEndMonth").Value = of_getDatePartStr(aobj_StockRestriction.dtmEndDate, "MONTH")
        iolecmd_comand.Parameters("@strEndDay").Value = of_getDatePartStr(aobj_StockRestriction.dtmEndDate, "DAY")
        iolecmd_comand.Parameters("@strUserName").Value = astr_UserName
        iolecmd_comand.Parameters("@intOperation").Value = aint_Operation
        iolecmd_comand.Parameters("@strComments").Value = aobj_StockRestriction.strRestricComments


        '''' -parametros del sp 
        ''''''''''''-- fin lista parametros del sp 
        'definir la cadena sql
        lstr_SQL = "spSaveReadStockRestricions"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandType = CommandType.Text

        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.Fill(ldt_TableResult)
            ''  Return ldt_AdviceResult(0)(0).ToString
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        ' Return lint_itemscount.ToString()
        iolecmd_comand = Nothing
        'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
        '' ver si la tabla trajo informacion 
        Try

            If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
                Dim lstr_info As String
                lstr_info = ldt_TableResult(0)(0).ToString
                If lstr_info.Length > 0 Then
                    Return lstr_info
                Else
                    Return ""
                End If
            Else
                Return "="
            End If
        Catch ex As Exception
            Dim lstr_ex As String
            lstr_ex = ex.Message
            lstr_ex = lstr_ex
            Return "error al actualizar informacion "
        End Try

        '''''''''''''''''''''''''''''''''
        Return ""
    End Function

    ''inventario contenedores por usuario
    <WebMethod()>
    Public Function GetContainerInvWEB(ByVal aint_UserId As Integer) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim ldtb_ResultCopy = New DataTable("userresultCopy") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spInventoryUserWEB"

        iolecmd_comand.Parameters.Add("@aintUserID", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintUserID").Value = aint_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            CopyTableAndCheckLatin(ldtb_Result, ldtb_ResultCopy)
            ldtb_ResultCopy.TableName = "Inventory"
            Return ldtb_ResultCopy
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''''''''

    ''inventario contenedores por usuario
    <WebMethod()>
    Public Function GetGeneralCargoInvWEB(ByVal aint_UserId As Integer) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spGCInventoryUserWEB"

        iolecmd_comand.Parameters.Add("@aintUserID", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintUserID").Value = aint_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function

    ''
    <WebMethod()>
    Public Function GetGeneralCargoTransactWEB(ByVal along_UniversalD As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String



        'definir el valor SQL
        'strSQL = "exec spGetBrokerLike " + astr_Broker

        strSQL = "  SELECT tblclsGeneralCargoTransaction.intGCTransacHistId AS 'TRANSID',   " &
"         tblclsGeneralCargoTransaction.intGeneralCargoUniversalId AS 'UNVERSALID', " &
"         tblclsGCTransactionType.strGCTransTypeDescription AS 'TRANSACCION',      " &
"         tblclsGeneralCargoTransaction.intGCInventoryItemId AS 'ITEM',            " &
"         tblclsGeneralCargoTransaction.intGCInvItemQuantity AS 'CANTIDAD',        " &
"         tblclsGeneralCargoTransaction.decGCInvItemWeight AS 'PESO',              " &
"         convert( varchar(22),  tblclsGeneralCargoTransaction.dtmGCTransacHistCreationStamp, 112 )+' ' +CONVERT(VARCHAR(22),  tblclsGeneralCargoTransaction.dtmGCTransacHistCreationStamp ,18)   AS 'FECHA' , " &
" 		 (CASE	 WHEN  tblclsGCTransactionType.strGCTransTypeIdentifier = 'CGFISCAL' " &
" 			     THEN (SELECT strGCFisStatHistComments FROM tblclsGCFiscalStatHistory " &
"					   WHERE intGCTransacHistId = tblclsGeneralCargoTransaction.intGCTransacHistId)		" &
"			     WHEN  tblclsGCTransactionType.strGCTransTypeIdentifier = 'CGLIBER' " &
"				  THEN 'Carga Liberado - '+ ' Cantidad: [' + CONVERT(VARCHAR(10), intGCInvItemQuantity) + '] - Peso: [' + CONVERT(VARCHAR(50), decGCInvItemWeight) + ']'  " &
"			    WHEN  tblclsGCTransactionType.strGCTransTypeIdentifier = 'CGDESP' " &
"			      THEN 'Carga Despachada - '+ ' Cantidad: [' + CONVERT(VARCHAR(10), intGCInvItemQuantity) + '] - Peso: [' + CONVERT(VARCHAR(50), decGCInvItemWeight) + ']' " &
"			 WHEN  tblclsGCTransactionType.strGCTransTypeIdentifier = 'ECGRF' " &
"			      THEN (SELECT (CASE WHEN Visit.intServiceOrderId IS NULL " &
"	   				   		  THEN 'Item dividido al ingresar,  - Cantidad: [' + CONVERT(VARCHAR(10), Trans.intGCInvItemQuantity) + '] - Peso: [' + CONVERT(VARCHAR(50), Trans.decGCInvItemWeight) + ']' " &
"			   		   		  ELSE  'Maniobra: [' + CONVERT(VARCHAR(10), Visit.intServiceOrderId) + '] - Visita: [ ' + CONVERT(VARCHAR(10), Visit.intVisitId) + ']' + ' - Cantidad: [' + CONVERT(VARCHAR(10), Trans.intGCInvItemQuantity) + '] - Peso: [' + CONVERT(VARCHAR(50), Trans.decGCInvItemWeight) + ']' " &
" 					   		END) " &
"			              FROM    tblclsGeneralCargoTransaction Trans, " &
"				                  tblclsVisitGeneralCargo Visit " &
"					      WHERE   Trans.intGeneralCargoUniversalId = tblclsGeneralCargoTransaction.intGeneralCargoUniversalId  AND " &
"								Trans.intGCInventoryItemId 		= tblclsGeneralCargoTransaction. intGCInventoryItemId 		AND " &
"					   		    Visit.intGeneralCargoUniversalId =* Trans.intGeneralCargoUniversalId 						AND  " &
"								Visit.intGCInventoryItemId       =* Trans.intGCInventoryItemId 								AND  " &
"                    		    Trans. intGCTransacHistId   		= tblclsGeneralCargoTransaction.intGCTransacHistId  	AND  " &
"								Visit.intServiceId = (SELECT intServiceId FROM tblclsService WHERE strServiceIdentifier   ='RECCG'        ) ) " &
"			 WHEN  tblclsGCTransactionType.strGCTransTypeIdentifier = 'SCGRF' " &
"                     THEN  (SELECT 'Maniobra: [' + CONVERT(VARCHAR(10), Visit.intServiceOrderId) + '] - Visita: [ ' + CONVERT(VARCHAR(10), Visit.intVisitId) + ']' + ' - Cantidad: [' + CONVERT(VARCHAR(10), Trans.intGCInvItemQuantity) + '] - Peso: [' + CONVERT(VARCHAR(50), Trans.decGCInvItemWeight) + ']' " &
"		 			         FROM    tblclsGeneralCargoTransaction Trans,  " &
" 				                     tblclsVisitGeneralCargo Visit  " &
"					         WHERE   Trans.intGeneralCargoUniversalId = tblclsGeneralCargoTransaction.intGeneralCargoUniversalId  AND  " &
"								     Trans.intGCInventoryItemId 		= tblclsGeneralCargoTransaction. intGCInventoryItemId 		AND " &
"					   		         Visit.intGeneralCargoUniversalId =* Trans.intGeneralCargoUniversalId 								AND  " &
"								     Visit.intGCInventoryItemId       =* Trans.intGCInventoryItemId 										AND  " &
"                    		        Trans. intGCTransacHistId   		= tblclsGeneralCargoTransaction.intGCTransacHistId   			AND  " &
"								    Visit.intServiceId = (SELECT intServiceId FROM tblclsService WHERE strServiceIdentifier   ='ENTCG'        ) )  " &
"			WHEN tblclsGCTransactionType.strGCTransTypeIdentifier = 'CGDEBU' THEN  " &
"				  'Fecha de Ingreso : '+ CONVERT(VARCHAR(11),tblclsGeneralCargoHistory.dtmGCHistoryReceptionDate)  " &
"			WHEN tblclsGCTransactionType.strGCTransTypeIdentifier = 'CGEBU' THEN   " &
"				  'Fecha de Embarque : '+ CONVERT(VARCHAR(11),tblclsGeneralCargoHistory.dtmGCHistoryDeliveryDate)  " &
"		    ELSE 'Sin Comentarios ' END )  AS 'COMMENTS'  " &
"    FROM tblclsGeneralCargoTransaction,    " &
"         tblclsGCTransactionType,          " &
"		  tblclsGeneralCargoHistory         " &
"   WHERE ( tblclsGCTransactionType.intGCTransTypeId = tblclsGeneralCargoTransaction.intGCTransTypeId ) and   " &
"	  	 ( tblclsGeneralCargoTransaction.intGeneralCargoUniversalId = tblclsGeneralCargoHistory.intGeneralCargoUniversalId  ) and " &
"		 ( tblclsGCTransactionType.blnOpVisibleWEB = 1   )  AND 	 " &
"         ( ( tblclsGeneralCargoTransaction.intGeneralCargoUniversalId = " + along_UniversalD.ToString() + " ) ) "

        'strSQL = "exec spGetBrokerLike CASTAÑE"
        ' valor SQL
        iolecmd_comand.CommandText = strSQL


        'agrega parametro
        'iolecmd_comand.Parameters.Add("@strBroker", OleDbType.VarChar)

        ' se pone valor al parametro
        'iolecmd_comand.Parameters("@strBroker").Value = astr_Broker

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    ''--

    <WebMethod()>
    Public Function GetContainerMainData(ByVal astr_Container As String) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String

        ' pasar a mayusculas
        astr_Container = astr_Container.ToUpper()


        'definir el valor SQL
        'strSQL = "exec spGetBrokerLike " + astr_Broker

        strSQL = "SELECT   tblclsContainerInventory.intContainerUniversalId AS 'intContainerUniversalId', " &
                 " tblclsContainerInventory.strContainerId AS 'strContainerId', " &
                 " tblclsContainerInventory.blnContainerInvActive AS 'ACTIVO', " &
                 " tblclsContainerType.strContainerTypeIdentifier AS 'TIPO' , " &
                 " tblclsContainerSize.strContainerSizeIdentifier AS 'TAMANO' , " &
                 " tblclsContainerInventory.strContainerInvYardPositionId AS 'POSICION', " &
                 " ISNULL(tblclsContainerCategory.strContainerCatIdentifier,'') AS 'CATEGORIA', " &
                 " tblclsContainerInventory.blnContainerIsFull AS 'LLENO'," &
                 " tblclsShippingLine.strShippingLineIdentifier AS 'LINEA', " &
                 " tblclsContainerInventory.intContainerInvOperatorId  AS 'IDSHIPPINGLINE', " &
                 " tblclsFiscalMovement.strFiscalMovementIdentifier AS 'TRAFICO',   " &
                 " tblclsContainerInventory.decContainerInventoryWeight AS 'P_NETO', " &
                 " tblclsContainer.decContainerTare AS 'TARA', " &
                 " tblclsContainerInventory.decContainerInventoryWeight + tblclsContainer.decContainerTare AS 'P_BRUTO', " &
                 " ISNULL(tblclsContainerInvBooking.strBookingId, '') AS 'strBooking', " &
                 " DATEDIFF(dd, dtmContainerInvReceptionDate, GETDATE()) AS 'ESTADIA', " &
                 " dtmContainerInvReceptionDate AS 'FECHA_INGRESO',  " &
                 " tblclsContainerInventory.strContainerInvComments AS 'COMENTARIOS', " &
                 " tblclsCompany.strCompanyName AS 'SOLICITADO', " &
                 " tblclsContainerInventory.intContRecepRequiredById AS 'IDBROKER', " &
                 " COMPCLI.strCompanyName AS'STRCLIENTNAME', " &
                 " tblclsContainerInventory.intCustomerId  AS 'IDCLIENTE', " &
                 " tblclsContainerInventory.intHas20PercentAuthority AS 'AUTORIDAD', " &
                 " ISNULL(VESSEL.strVesselName,'' ) AS 'VESSELNAME' " &
                 " ,(convert(varchar(12),VVOY.strVesselVoyageNumIdentifier) + '-' +VESSEL.strVesselIdentifier ) as 'BV'" &
                 " FROM     tblclsContainerInventory " &
                 " JOIN tblclsContainer ON tblclsContainerInventory.strContainerId = tblclsContainer.strContainerId " &
                 " JOIN tblclsContainerISOCode ON tblclsContainerISOCode.intContISOCodeId = tblclsContainer.intContISOCodeId " &
                 " LEFT JOIN tblclsContainerInvBooking ON tblclsContainerInventory.intContainerUniversalId = tblclsContainerInvBooking.intContainerUniversalId " &
                 " JOIN tblclsContainerType ON tblclsContainerType.intContainerTypeId = tblclsContainerISOCode.intContainerTypeId " &
                 " JOIN tblclsContainerSize ON tblclsContainerSize.intContainerSizeId = tblclsContainerISOCode.intContainerSizeId " &
                 " LEFT JOIN tblclsContainerCategory ON tblclsContainerCategory.intContainerCategoryId = tblclsContainerInventory.intContainerCategoryId " &
                 " LEFT JOIN tblclsShippingLine ON tblclsShippingLine.intShippingLineId = tblclsContainerInventory.intContainerInvOperatorId " &
                 " LEFT JOIN tblclsFiscalMovement  ON tblclsFiscalMovement.intFiscalMovementId = tblclsContainerInventory.intFiscalMovementId " &
                 " LEFT JOIN tblclsCompanyEntity on tblclsContainerInventory.intContRecepRequiredById =tblclsCompanyEntity.intCompanyEntityId " &
                 "                              and tblclsContainerInventory.intContRecepRequiredTypeId =tblclsCompanyEntity.intCustomerTypeId " &
                 " LEFT JOIN tblclsCompany ON tblclsCompany.intCompanyId  = tblclsCompanyEntity.intCompanyId " &
                 " LEFT JOIN tblclsCustomer ON tblclsCustomer.intCustomerId  = tblclsContainerInventory.intCustomerId " &
                 " LEFT JOIN tblclsCompany COMPCLI ON COMPCLI.intCompanyId = tblclsCustomer.intCompanyId " &
                 " LEFT JOIN tblclsVesselVoyage VVOY ON VVOY.intVesselVoyageId = tblclsContainerInventory.intContainerInvVesselVoyageId " &
                 " LEFT JOIN tblclsVessel VESSEL ON VESSEL.intVesselId = VVOY.intVesselId " &
                 "  WHERE    tblclsContainerInventory.intContainerUniversalId = (SELECT MAX(intContainerUniversalId ) " &
                 "               FROM tblclsContainerInventory  Inv     WHERE 	strContainerId = '" + astr_Container + "' )"


        'strSQL = "exec spGetBrokerLike CASTAÑE"
        ' valor SQL
        iolecmd_comand.CommandText = strSQL


        'agrega parametro
        'iolecmd_comand.Parameters.Add("@strBroker", OleDbType.VarChar)

        ' se pone valor al parametro
        'iolecmd_comand.Parameters("@strBroker").Value = astr_Broker

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function

    ''''''
    <WebMethod()>
    Public Function GetContainerTransactWEB(ByVal along_UniversalD As Long) As DataTable

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"

        Dim strSQL As String
        'Dim strcontainerid As String



        'definir el valor SQL
        'strSQL = "exec spGetBrokerLike " + astr_Broker

        strSQL = " SELECT  tblclsContainerTransaction.intContTransHistId AS 'TRANSID'  ," &
                  "         tblclsContainerTransaction.intContainerUniversalId AS 'UNVERSALID', " &
                  "         tblclsContainerTransacType.strContTransTypeDescription AS 'TRANSACCION', " &
                  "         tblclsContainerTransComments.strContTransHistComments AS 'COMENTARIOS',  " &
                  "          convert( varchar(22),  tblclsContainerTransaction.dtmContTransHistCreationStamp, 112 )+' ' +CONVERT(VARCHAR(22),   tblclsContainerTransaction.dtmContTransHistCreationStamp ,18)   AS 'FECHA' " &
                  " FROM tblclsContainerTransaction  " &
                  "     INNER JOIN tblclsContainerTransComments ON tblclsContainerTransComments.intContTransHistId = tblclsContainerTransaction.intContTransHistId " &
                  "     INNER JOIN tblclsContainerTransacType   ON tblclsContainerTransacType.intContTransTypeId = tblclsContainerTransaction.intContTransTypeId " &
                  "   WHERE tblclsContainerTransaction.intContainerUniversalId = " + along_UniversalD.ToString + " " &
                  " and ISNULL(tblclsContainerTransacType.blnOpVisibleWEB, 0) = 1 "


        'strSQL = "exec spGetBrokerLike CASTAÑE"
        ' valor SQL
        iolecmd_comand.CommandText = strSQL


        'agrega parametro
        'iolecmd_comand.Parameters.Add("@strBroker", OleDbType.VarChar)

        ' se pone valor al parametro
        'iolecmd_comand.Parameters("@strBroker").Value = astr_Broker

        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing

        Return idt_result


    End Function
    ''''''

    ''''''''''''
    <WebMethod()>
    Public Function GetArrivingVoyages() As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDVesselVoyage"

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 2

        iolecmd_comand.Parameters.Add("@intVesselVoyageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselVoyageId").Value = 0

        iolecmd_comand.Parameters.Add("@strETADate", OleDbType.Char)
        iolecmd_comand.Parameters("@strETADate").Value = " "

        iolecmd_comand.Parameters.Add("@strCloseReleaseDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strCloseReleaseDate").Value = " "


        iolecmd_comand.Parameters.Add("@strVVoyageDepartureTime", OleDbType.Char)
        iolecmd_comand.Parameters("@strVVoyageDepartureTime").Value = ""

        iolecmd_comand.Parameters.Add("@strVOpDischargeStartDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDischargeStartDate").Value = ""

        iolecmd_comand.Parameters.Add("@strdteVVoyageDepartureDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strdteVVoyageDepartureDate").Value = ""

        iolecmd_comand.Parameters.Add("@strVOpDischargeEndDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDischargeEndDate").Value = ""

        iolecmd_comand.Parameters.Add("@strVOpDockingDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDockingDate").Value = ""

        iolecmd_comand.Parameters.Add("@strVVoyageArrivalTime", OleDbType.Char)
        iolecmd_comand.Parameters("@strVVoyageArrivalTime").Value = ""

        iolecmd_comand.Parameters.Add("@strVOpLoadingStartDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpLoadingStartDate").Value = ""

        iolecmd_comand.Parameters.Add("@strVesselOpUndockingDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselOpUndockingDate").Value = ""

        iolecmd_comand.Parameters.Add("@strVOpLoadingEndDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpLoadingEndDate").Value = ""

        iolecmd_comand.Parameters.Add("@intParamA", OleDbType.Integer)
        iolecmd_comand.Parameters("@intParamA").Value = 0

        iolecmd_comand.Parameters.Add("@strParamB", OleDbType.Char)
        iolecmd_comand.Parameters("@strParamB").Value = " "

        iolecmd_comand.Parameters.Add("@aintUserID", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintUserID").Value = 0

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    '''''''''''''''''''''
    '''''''''''''''''
    <WebMethod()>
    Public Function UpdaateVoyageDates(ByVal aint_vesselVoyage As Long, ByVal adtm_VesselArrival As Date, ByVal adtm_CloseRelease As Date, ByVal astrVVoyageDepartureTime As String, ByVal adtmVOpDischargeStartDate As Date, ByVal adtmdteVVoyageDepartureDate As Date, ByVal adtmVOpDischargeEndDate As Date, ByVal adtmVOpDockingDate As Date, ByVal strVVoyageArrivalTime As String, ByVal dtmVOpLoadingStartDate As Date, ByVal dtmVesselOpUndockingDate As Date, ByVal dtmVOpLoadingEndDate As Date, ByVal aint_UserId As Integer) As String

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDVesselVoyage"

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 1

        iolecmd_comand.Parameters.Add("@intVesselVoyageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselVoyageId").Value = aint_vesselVoyage

        iolecmd_comand.Parameters.Add("@strETADate", OleDbType.Char)
        iolecmd_comand.Parameters("@strETADate").Value = of_ConvertDateToStringGeneralFormat(adtm_VesselArrival)

        iolecmd_comand.Parameters.Add("@strCloseReleaseDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strCloseReleaseDate").Value = of_ConvertDateToStringGeneralFormat(adtm_CloseRelease)

        iolecmd_comand.Parameters.Add("@strVVoyageDepartureTime", OleDbType.Char)
        iolecmd_comand.Parameters("@strVVoyageDepartureTime").Value = astrVVoyageDepartureTime

        iolecmd_comand.Parameters.Add("@strVOpDischargeStartDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDischargeStartDate").Value = of_ConvertDateToStringGeneralFormat(adtmVOpDischargeStartDate)

        iolecmd_comand.Parameters.Add("@strdteVVoyageDepartureDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strdteVVoyageDepartureDate").Value = of_ConvertDateToStringGeneralFormat(adtmdteVVoyageDepartureDate)

        iolecmd_comand.Parameters.Add("@strVOpDischargeEndDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDischargeEndDate").Value = of_ConvertDateToStringGeneralFormat(adtmVOpDischargeEndDate)

        iolecmd_comand.Parameters.Add("@strVOpDockingDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDockingDate").Value = of_ConvertDateToStringGeneralFormat(adtmVOpDockingDate)

        iolecmd_comand.Parameters.Add("@strVVoyageArrivalTime", OleDbType.Char)
        iolecmd_comand.Parameters("@strVVoyageArrivalTime").Value = strVVoyageArrivalTime

        iolecmd_comand.Parameters.Add("@strVOpLoadingStartDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpLoadingStartDate").Value = of_ConvertDateToStringGeneralFormat(dtmVOpLoadingStartDate)

        iolecmd_comand.Parameters.Add("@strVesselOpUndockingDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselOpUndockingDate").Value = of_ConvertDateToStringGeneralFormat(dtmVesselOpUndockingDate)

        iolecmd_comand.Parameters.Add("@strVOpLoadingEndDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpLoadingEndDate").Value = of_ConvertDateToStringGeneralFormat(dtmVOpLoadingEndDate)

        iolecmd_comand.Parameters.Add("@intParamA", OleDbType.Integer)
        iolecmd_comand.Parameters("@intParamA").Value = 0

        iolecmd_comand.Parameters.Add("@strParamB", OleDbType.Char)
        iolecmd_comand.Parameters("@strParamB").Value = ""

        iolecmd_comand.Parameters.Add("@aintUserID", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintUserID").Value = aint_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return strError
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ""

    End Function
    '''''''''''''''''''
    <WebMethod()>
    Public Function UpdaateVoyageDatesString(ByVal aint_vesselVoyage As Long, ByVal adtm_VesselArrival As Date, ByVal adtm_CloseRelease As Date, ByVal astrVVoyageDepartureTime As String, ByVal adtmVOpDischargeStartDate As Date, ByVal adtmdteVVoyageDepartureDate As Date, ByVal adtmVOpDischargeEndDate As Date, ByVal adtmVOpDockingDate As Date, ByVal strVVoyageArrivalTime As String, ByVal dtmVOpLoadingStartDate As Date, ByVal dtmVesselOpUndockingDate As Date, ByVal dtmVOpLoadingEndDate As Date, ByVal aint_UserId As Integer) As String

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDVesselVoyage"

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 1

        iolecmd_comand.Parameters.Add("@intVesselVoyageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselVoyageId").Value = aint_vesselVoyage

        iolecmd_comand.Parameters.Add("@strETADate", OleDbType.Char)
        iolecmd_comand.Parameters("@strETADate").Value = of_ConvertDateToStringGeneralFormat(adtm_VesselArrival)

        iolecmd_comand.Parameters.Add("@strCloseReleaseDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strCloseReleaseDate").Value = of_ConvertDateToStringGeneralFormat(adtm_CloseRelease)

        iolecmd_comand.Parameters.Add("@strVVoyageDepartureTime", OleDbType.Char)
        iolecmd_comand.Parameters("@strVVoyageDepartureTime").Value = astrVVoyageDepartureTime

        iolecmd_comand.Parameters.Add("@strVOpDischargeStartDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDischargeStartDate").Value = of_ConvertDateToStringGeneralFormat(adtmVOpDischargeStartDate)

        iolecmd_comand.Parameters.Add("@strdteVVoyageDepartureDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strdteVVoyageDepartureDate").Value = of_ConvertDateToStringGeneralFormat(adtmdteVVoyageDepartureDate)

        iolecmd_comand.Parameters.Add("@strVOpDischargeEndDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDischargeEndDate").Value = of_ConvertDateToStringGeneralFormat(adtmVOpDischargeEndDate)

        iolecmd_comand.Parameters.Add("@strVOpDockingDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpDockingDate").Value = of_ConvertDateToStringGeneralFormat(adtmVOpDockingDate)

        iolecmd_comand.Parameters.Add("@strVVoyageArrivalTime", OleDbType.Char)
        iolecmd_comand.Parameters("@strVVoyageArrivalTime").Value = strVVoyageArrivalTime

        iolecmd_comand.Parameters.Add("@strVOpLoadingStartDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpLoadingStartDate").Value = of_ConvertDateToStringGeneralFormat(dtmVOpLoadingStartDate)

        iolecmd_comand.Parameters.Add("@strVesselOpUndockingDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselOpUndockingDate").Value = of_ConvertDateToStringGeneralFormat(dtmVesselOpUndockingDate)

        iolecmd_comand.Parameters.Add("@strVOpLoadingEndDate", OleDbType.Char)
        iolecmd_comand.Parameters("@strVOpLoadingEndDate").Value = of_ConvertDateToStringGeneralFormat(dtmVOpLoadingEndDate)

        iolecmd_comand.Parameters.Add("@intParamA", OleDbType.Integer)
        iolecmd_comand.Parameters("@intParamA").Value = 0

        iolecmd_comand.Parameters.Add("@strParamB", OleDbType.Char)
        iolecmd_comand.Parameters("@strParamB").Value = ""

        iolecmd_comand.Parameters.Add("@aintUserID", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintUserID").Value = aint_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return strError
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ""

    End Function
    ''''''''''''''''''''''''''
    <WebMethod()>
    Public Function GetEirMainData(ByVal aint_EIR As Long) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spPrintEIRWeb"

        iolecmd_comand.Parameters.Add("EIR", OleDbType.Integer)
        iolecmd_comand.Parameters("EIR").Value = aint_EIR

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function


    ''''''''''''''''''''''''''''''''
    ''''''''''''''''''''''''''
    <WebMethod()>
    Public Function GetEirDamageData(ByVal aint_EIR As Long) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spGetDamage"

        iolecmd_comand.Parameters.Add("EIR", OleDbType.Integer)
        iolecmd_comand.Parameters("EIR").Value = aint_EIR

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''''''''''''''''''''''''''
    ''''''''''''''''''''''''''
    <WebMethod()>
    Public Function GetEirMainDataEVisitCont(ByVal alng_EIR As Long, ByVal alng_Visit As Long, ByVal astr_Container As String, ByVal alng_UserId As Long) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spPrintEIRWebC"

        iolecmd_comand.Parameters.Add("EIR", OleDbType.Integer)
        iolecmd_comand.Parameters("EIR").Value = alng_EIR

        iolecmd_comand.Parameters.Add("Visit", OleDbType.Integer)
        iolecmd_comand.Parameters("Visit").Value = alng_Visit

        iolecmd_comand.Parameters.Add("strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("strContainerId").Value = astr_Container

        iolecmd_comand.Parameters.Add("intUserId", OleDbType.Integer)
        iolecmd_comand.Parameters("intUserId").Value = alng_UserId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''''''''''''''''''''''    
    ''''''''''''''''''''''''
    <WebMethod()>
    Public Function GetContReservBookingInfo(ByVal aint_UserId As Integer, ByVal astr_BookingName As String) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spFindBookingWEB"

        iolecmd_comand.Parameters.Add("@aintUserID", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintUserID").Value = aint_UserId

        iolecmd_comand.Parameters.Add("@aintReservationId", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintReservationId").Value = 0

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 1

        iolecmd_comand.Parameters.Add("@strBooking", OleDbType.Char)
        iolecmd_comand.Parameters("@strBooking").Value = astr_BookingName

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''''''''''''''''''''''''
    <WebMethod()>
    Public Function GetContReservBookingDetailInfo(ByVal aint_BookingId As Integer) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spFindBookingWEB"

        iolecmd_comand.Parameters.Add("@aintUserID", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintUserID").Value = 0

        iolecmd_comand.Parameters.Add("@aintReservationId", OleDbType.Integer)
        iolecmd_comand.Parameters("@aintReservationId").Value = aint_BookingId

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 2

        iolecmd_comand.Parameters.Add("@strBooking", OleDbType.Char)
        iolecmd_comand.Parameters("@strBooking").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''''''''''
    <WebMethod()>
    Public Function InsertMasterStorageCriteria(ByVal aintContainerStorageId As Integer, ByVal astrContStorageDescription As String, ByVal adtmContStorageAppStartDate As Date, ByVal adtmContStorageAppEndDate As Date, ByVal adecContStorageWeightFraction As Decimal, ByVal aintContStoRequiredById As Integer, ByVal aintContStoRequiredTypeId As Integer, ByVal astrUserName As String) As String

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_date As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageRateM"

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 1

        iolecmd_comand.Parameters.Add("@intContainerStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerStorageId").Value = aintContainerStorageId

        iolecmd_comand.Parameters.Add("@strContStorageDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strContStorageDescription").Value = astrContStorageDescription

        iolecmd_comand.Parameters.Add("@strContStorageAppStartDate", OleDbType.Char)
        lstr_date = ""
        lstr_date = of_ConvertDateToStringGeneralFormat(adtmContStorageAppStartDate)
        iolecmd_comand.Parameters("@strContStorageAppStartDate").Value = lstr_date

        iolecmd_comand.Parameters.Add("@strContStorageAppEndDate", OleDbType.Char)
        lstr_date = ""
        lstr_date = of_ConvertDateToStringGeneralFormat(adtmContStorageAppEndDate)
        iolecmd_comand.Parameters("@strContStorageAppEndDate").Value = lstr_date


        iolecmd_comand.Parameters.Add("@decContStorageWeightFraction", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decContStorageWeightFraction").Value = adecContStorageWeightFraction.ToString()

        iolecmd_comand.Parameters.Add("@intblnContStorageActive", OleDbType.Integer)
        iolecmd_comand.Parameters("@intblnContStorageActive").Value = 1

        iolecmd_comand.Parameters.Add("@intContStoRequiredById", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContStoRequiredById").Value = aintContStoRequiredById

        iolecmd_comand.Parameters.Add("@intContStoRequiredTypeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContStoRequiredTypeId").Value = aintContStoRequiredTypeId

        iolecmd_comand.Parameters.Add("@astrUserName", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUserName").Value = astrUserName


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return strError
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ""

    End Function
    '''''''''''''''
    '''''''''''''''
    ''''''''''
    <WebMethod()>
    Public Function UpdateMasterStorageCriteria(ByVal aintContainerStorageId As Integer, ByVal astrContStorageDescription As String, ByVal adtmContStorageAppStartDate As Date, ByVal adtmContStorageAppEndDate As Date, ByVal adecContStorageWeightFraction As Decimal, ByVal aintContStoRequiredById As Integer, ByVal aintContStoRequiredTypeId As Integer, ByVal astrUserName As String) As String

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_date As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageRateM"

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 2

        iolecmd_comand.Parameters.Add("@intContainerStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerStorageId").Value = aintContainerStorageId

        iolecmd_comand.Parameters.Add("@strContStorageDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strContStorageDescription").Value = astrContStorageDescription

        iolecmd_comand.Parameters.Add("@strContStorageAppStartDate", OleDbType.Char)
        lstr_date = ""
        lstr_date = of_ConvertDateToStringGeneralFormat(adtmContStorageAppStartDate)
        iolecmd_comand.Parameters("@strContStorageAppStartDate").Value = lstr_date

        iolecmd_comand.Parameters.Add("@strContStorageAppEndDate", OleDbType.Char)
        lstr_date = ""
        lstr_date = of_ConvertDateToStringGeneralFormat(adtmContStorageAppEndDate)
        iolecmd_comand.Parameters("@strContStorageAppEndDate").Value = lstr_date


        iolecmd_comand.Parameters.Add("@decContStorageWeightFraction", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decContStorageWeightFraction").Value = adecContStorageWeightFraction.ToString()

        iolecmd_comand.Parameters.Add("@intblnContStorageActive", OleDbType.Integer)
        iolecmd_comand.Parameters("@intblnContStorageActive").Value = 1

        iolecmd_comand.Parameters.Add("@intContStoRequiredById", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContStoRequiredById").Value = aintContStoRequiredById

        iolecmd_comand.Parameters.Add("@intContStoRequiredTypeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContStoRequiredTypeId").Value = aintContStoRequiredTypeId

        iolecmd_comand.Parameters.Add("@astrUserName", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUserName").Value = astrUserName


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return strError
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ""

    End Function
    '''''''''''''''''''''
    '''
    <WebMethod()>
    Public Function InsertStorageRule(ByVal aintContStorageRuleId As Integer, ByVal aintContainerStorageId As Integer, ByVal aintContStorageRuleDayRange As Integer, ByVal amonContStorageRuleRate As Decimal, ByVal ablnContStoRuleIsUnderCover As Integer, ByVal astrContStorageComments As String, ByVal ablnContStorageActive As Integer, ByVal aintContStartRangeDay As Integer, ByVal aintContEndRangeDay As Integer, ByVal astrUserName As String) As String

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_date As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStoRule"

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 1

        iolecmd_comand.Parameters.Add("@intContStorageRuleId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContStorageRuleId").Value = aintContStorageRuleId

        iolecmd_comand.Parameters.Add("@intContStorageRuleDayRange", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContStorageRuleDayRange").Value = aintContStorageRuleDayRange

        iolecmd_comand.Parameters.Add("@intContainerStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerStorageId").Value = aintContainerStorageId

        iolecmd_comand.Parameters.Add("@monContStorageRuleRate", OleDbType.Decimal)
        iolecmd_comand.Parameters("@monContStorageRuleRate").Value = amonContStorageRuleRate

        iolecmd_comand.Parameters.Add("@blnContStoRuleIsUnderCover", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnContStoRuleIsUnderCover").Value = ablnContStoRuleIsUnderCover

        iolecmd_comand.Parameters.Add("@strContStorageComments", OleDbType.VarChar)
        iolecmd_comand.Parameters("@strContStorageComments").Value = astrContStorageComments

        iolecmd_comand.Parameters.Add("@blnContStorageActive", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnContStorageActive").Value = 1

        iolecmd_comand.Parameters.Add("@astrUserName", OleDbType.VarChar)
        iolecmd_comand.Parameters("@astrUserName").Value = astrUserName

        iolecmd_comand.Parameters.Add("@intContStartRangeDay", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContStartRangeDay").Value = aintContStartRangeDay

        iolecmd_comand.Parameters.Add("@intContEndRangeDay", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContEndRangeDay").Value = aintContEndRangeDay


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return strError
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ""

    End Function


    '''''
    ''''''''''''''
    '''
    <WebMethod()>
    Public Function InsertStorageFreeDays(ByVal aintContStorageRuleId As Integer, ByVal aintFicalMovement As Integer, ByVal aintConStoFreeDayQuantity As Integer, ByVal blnConStoFreeDayIsWorkableDay As Integer, ByVal ablnContStoRuleIsUnderCover As Integer, ByVal astrContStorageComments As String, ByVal ablnContStorageActive As Integer, ByVal aintContStartRangeDay As Integer, ByVal aintContEndRangeDay As Integer, ByVal astrUserName As String) As String
        '''

        Return ""


        '' CREATE PROCEDURE spCRUDContStorageFreeDays     @intMode int ,@intContainerStorageId numeric(18) , @intFiscalMovementId int , @intConStoFreeDayQuantity int 
        ''											  , @blnConStoFreeDayIsWorkableDay int , @strUsername varchar(16)

        '''''

        'Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        'Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        'Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        'Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        'Dim istr_conx As String = "" ' cadena de conexion
        'Dim strSQL As String = ""
        'Dim lstr_date As String = ""


        'istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        'ioleconx_conexion.ConnectionString = istr_conx
        'iolecmd_comand = ioleconx_conexion.CreateCommand()

        'ldtb_Result = New DataTable("User")
        'strSQL = "spCRUDContainerStoRule"

        'iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        'iolecmd_comand.Parameters("@intMode").Value = 1

        'iolecmd_comand.Parameters.Add("@intContStorageRuleId", OleDbType.Integer)
        'iolecmd_comand.Parameters("@intContStorageRuleId").Value = aintContStorageRuleId

        'iolecmd_comand.Parameters.Add("@intContStorageRuleDayRange", OleDbType.Integer)
        'iolecmd_comand.Parameters("@intContStorageRuleDayRange").Value = aintContStorageRuleDayRange

        'iolecmd_comand.Parameters.Add("@intContainerStorageId", OleDbType.Integer)
        'iolecmd_comand.Parameters("@intContainerStorageId").Value = aintContainerStorageId

        'iolecmd_comand.Parameters.Add("@monContStorageRuleRate", OleDbType.Decimal)
        'iolecmd_comand.Parameters("@monContStorageRuleRate").Value = amonContStorageRuleRate

        'iolecmd_comand.Parameters.Add("@blnContStoRuleIsUnderCover", OleDbType.Integer)
        'iolecmd_comand.Parameters("@blnContStoRuleIsUnderCover").Value = ablnContStoRuleIsUnderCover

        'iolecmd_comand.Parameters.Add("@strContStorageComments", OleDbType.VarChar)
        'iolecmd_comand.Parameters("@strContStorageComments").Value = astrContStorageComments

        'iolecmd_comand.Parameters.Add("@blnContStorageActive", OleDbType.Integer)
        'iolecmd_comand.Parameters("@blnContStorageActive").Value = 1

        'iolecmd_comand.Parameters.Add("@astrUserName", OleDbType.VarChar)
        'iolecmd_comand.Parameters("@astrUserName").Value = astrUserName

        'iolecmd_comand.Parameters.Add("@intContStartRangeDay", OleDbType.Integer)
        'iolecmd_comand.Parameters("@intContStartRangeDay").Value = aintContStartRangeDay

        'iolecmd_comand.Parameters.Add("@intContEndRangeDay", OleDbType.Integer)
        'iolecmd_comand.Parameters("@intContEndRangeDay").Value = aintContEndRangeDay


        'iolecmd_comand.CommandText = strSQL
        'iolecmd_comand.CommandType = CommandType.StoredProcedure
        'iolecmd_comand.CommandTimeout = 99999

        'Try
        '    iAdapt_comand.SelectCommand = iolecmd_comand
        '    iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
        '    iAdapt_comand.Fill(ldtb_Result)
        'Catch ex As Exception
        '    Dim strError As String = ObtenerError(ex.Message, 99999)
        '    strError = strError
        '    strError = ex.Message
        '    Return strError
        'Finally
        '    ioleconx_conexion.Close()
        'End Try


        'Return ""

    End Function



    <WebMethod()>
    Public Function GetReportAppointmentDate(ByVal adtm_StartDate As Date) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        Dim lstr_appointmentDate As String

        lstr_appointmentDate = of_ConvertDateToStringGeneralFormat(adtm_StartDate)

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spRptAppointmentDate"
        ' lstr_appointmentDate = lstr_appointmentDate.Substring(0, 8)
        iolecmd_comand.Parameters.Add("@dtmStartAppointDate", OleDbType.Char)
        iolecmd_comand.Parameters("@dtmStartAppointDate").Value = lstr_appointmentDate

        iolecmd_comand.Parameters.Add("@dtmEndtAppointDate", OleDbType.Char)
        iolecmd_comand.Parameters("@dtmEndtAppointDate").Value = lstr_appointmentDate


        iolecmd_comand.Parameters.Add("@strService", OleDbType.Char)
        iolecmd_comand.Parameters("@strService").Value = "ENTLL"

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''''''''''''''''
    ''

    <WebMethod()>
    Public Function GetReportAppointmentBlocks(ByVal adtm_StartDate As Date) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        Dim lstr_appointmentDate As String

        lstr_appointmentDate = of_ConvertDateToStringGeneralFormat(adtm_StartDate)

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spRptAppointmentsDateBlocks"

        ' lstr_appointmentDate = lstr_appointmentDate.Substring(0, 8)
        iolecmd_comand.Parameters.Add("@dtmAppointDate", OleDbType.Char)
        iolecmd_comand.Parameters("@dtmAppointDate").Value = lstr_appointmentDate


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''
    ''
    ''
    <WebMethod()>
    Public Function SearchContainerStorage(ByVal astr_ContainerId As String, ByVal astr_FiscalMov As String) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = astr_ContainerId

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 1

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = 0

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = astr_FiscalMov

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    ''
    <WebMethod()>
    Public Function SearchGeneralCargoStorage(ByVal aobj_GeneralCargoStorage As ClsGeneralCargo) As DataTable


        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoStorageFee"


        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = aobj_GeneralCargoStorage.intGeneralCargoUniversalId

        iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCItemId").Value = aobj_GeneralCargoStorage.intGCInventoryItemId

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = aobj_GeneralCargoStorage.intGCRecepRequiredById

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = aobj_GeneralCargoStorage.intGCRecepRequiredTypeId

        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerId").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerType").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 1

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = aobj_GeneralCargoStorage.intGCStorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = aobj_GeneralCargoStorage.strFiscalMovementIdentifier

        iolecmd_comand.Parameters.Add("@intFiscalMov", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFiscalMov").Value = 0

        iolecmd_comand.Parameters.Add("@decMinWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMinWeight").Value = aobj_GeneralCargoStorage.decGCInvItemWeight

        iolecmd_comand.Parameters.Add("@decMaxWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMaxWeight").Value = 0

        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselId").Value = aobj_GeneralCargoStorage.intVesselId

        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselName").Value = ""

        iolecmd_comand.Parameters.Add("@strProductIdName", OleDbType.Char)
        iolecmd_comand.Parameters("@strProductIdName").Value = aobj_GeneralCargoStorage.strProductName

        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intProductId").Value = 0

        iolecmd_comand.Parameters.Add("@strNumbers", OleDbType.Char)
        iolecmd_comand.Parameters("@strNumbers").Value = aobj_GeneralCargoStorage.strGCInvItemNumbers

        iolecmd_comand.Parameters.Add("@strMarks", OleDbType.Char)
        iolecmd_comand.Parameters("@strMarks").Value = aobj_GeneralCargoStorage.strGCInvItemMarks

        iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
        iolecmd_comand.Parameters("@strBLName").Value = aobj_GeneralCargoStorage.strDocumentFolio

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = aobj_GeneralCargoStorage.strContainerId

        iolecmd_comand.Parameters.Add("@intQty", OleDbType.Integer)
        iolecmd_comand.Parameters("@intQty").Value = aobj_GeneralCargoStorage.intGCInvItemQuantity

        ''


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result


    End Function
    '''
    <WebMethod()>
    Public Function SentStorageFee(ByVal alng_StorageFeeId As Long, ByVal astr_username As String, ByVal astrContainerCargoType As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 2

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_StorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = astrContainerCargoType

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function

    <WebMethod()>
    Public Function ValidateStorageFee(ByVal alng_StorageFeeId As Long, ByVal astr_username As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 3

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_StorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function

    ''
    <WebMethod()>
    Public Function FactStorageFee(ByVal alng_StorageFeeId As Long, ByVal astr_username As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 4

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_StorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function

    <WebMethod()>
    Public Function UpdateStatusStorageFeeByUser(ByVal alng_StorageFeeId As Long, ByVal astr_username As String, ByVal astrContainerCargoType As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 31

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_StorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = astrContainerCargoType

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            If ldtb_Result.Columns.Count = 1 And ldtb_Result.Rows.Count = 1 Then
                Dim lstr_resultString As String
                lstr_resultString = ldtb_Result.Rows(0)(0)
                If lstr_resultString.Length > 0 Then
                    Return lstr_resultString
                End If
            End If

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''
    <WebMethod()>
    Public Function UpdateStatusGeneralCargoStorageFeeByUser(ByVal alng_StorageFeeId As Long, ByVal astr_username As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoStorageFee"

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCItemId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerId").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerType").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 31

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_StorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@intFiscalMov", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFiscalMov").Value = 0

        iolecmd_comand.Parameters.Add("@decMinWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMinWeight").Value = 0

        iolecmd_comand.Parameters.Add("@decMaxWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMaxWeight").Value = 0

        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselId").Value = 0

        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselName").Value = ""

        iolecmd_comand.Parameters.Add("@strProductIdName", OleDbType.Char)
        iolecmd_comand.Parameters("@strProductIdName").Value = ""

        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intProductId").Value = 0

        iolecmd_comand.Parameters.Add("@strNumbers", OleDbType.Char)
        iolecmd_comand.Parameters("@strNumbers").Value = ""

        iolecmd_comand.Parameters.Add("@strMarks", OleDbType.Char)
        iolecmd_comand.Parameters("@strMarks").Value = ""

        iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
        iolecmd_comand.Parameters("@strBLName").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intQty", OleDbType.Integer)
        iolecmd_comand.Parameters("@intQty").Value = 0




        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            If ldtb_Result.Columns.Count = 1 And ldtb_Result.Rows.Count = 1 Then
                Dim lstr_resultString As String
                lstr_resultString = ldtb_Result.Rows(0)(0)
                If lstr_resultString.Length > 0 Then
                    Return lstr_resultString
                End If
            End If

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''
    <WebMethod()>
    Public Function CancelStorageFee(ByVal alng_StorageFeeId As Long, ByVal astr_username As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 5

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_StorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            If ldtb_Result.Rows.Count = 1 And ldtb_Result.Columns.Count = 1 Then
                lstr_error = ldtb_Result.Rows(0)(0).ToString()
            End If

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''
    <WebMethod()>
    Public Function CancelStorageGeneralCargoFee(ByVal alng_StorageFeeId As Long, ByVal astr_username As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoStorageFee"

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCItemId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerId").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerType").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 5

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_StorageFeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@intFiscalMov", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFiscalMov").Value = 0

        iolecmd_comand.Parameters.Add("@decMinWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMinWeight").Value = 0

        iolecmd_comand.Parameters.Add("@decMaxWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMaxWeight").Value = 0

        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselId").Value = 0

        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselName").Value = ""

        iolecmd_comand.Parameters.Add("@strProductIdName", OleDbType.Char)
        iolecmd_comand.Parameters("@strProductIdName").Value = ""

        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intProductId").Value = 0

        iolecmd_comand.Parameters.Add("@strNumbers", OleDbType.Char)
        iolecmd_comand.Parameters("@strNumbers").Value = ""

        iolecmd_comand.Parameters.Add("@strMarks", OleDbType.Char)
        iolecmd_comand.Parameters("@strMarks").Value = ""

        iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
        iolecmd_comand.Parameters("@strBLName").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intQty", OleDbType.Integer)
        iolecmd_comand.Parameters("@intQty").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            If ldtb_Result.Rows.Count = 1 And ldtb_Result.Columns.Count = 1 Then
                lstr_error = ldtb_Result.Rows(0)(0).ToString()
            End If

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''

    Public Function of_validateFeeContNoteItem(ByVal aobj_FeenoteItem As ClsNoteFee, ByVal astr_username As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado


        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        'agregar la tabla columna de retorno 


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = aobj_FeenoteItem.intFeeStorageId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = aobj_FeenoteItem.intNoteItem

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = ""

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 1

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 10

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999



        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            'ver si hay informacion de retorno
            Try
                If ldtb_Result.Rows.Count > 0 And ldtb_Result.Columns.Count > 0 Then
                    lstr_error = ldtb_Result.Rows(0)(0).ToString()

                    If lstr_error.Length > 0 Then
                        Return lstr_error
                    End If

                End If

            Catch ex As Exception
                Dim lstre_mesage As String = ex.Message
            End Try



        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    '''

    ''
    Public Function of_validateFeeGcargoNoteItem(ByVal aobj_FeenoteItem As ClsNoteFee, ByVal astr_username As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado


        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        'agregar la tabla columna de retorno 


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = aobj_FeenoteItem.intFeeStorageId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = aobj_FeenoteItem.intNoteItem

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intGCInventoryItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCInventoryItemId").Value = 0

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = ""

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 1

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 10

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999



        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            'ver si hay informacion de retorno
            Try
                If ldtb_Result.Rows.Count > 0 And ldtb_Result.Columns.Count > 0 Then
                    lstr_error = ldtb_Result.Rows(0)(0).ToString()

                    If lstr_error.Length > 0 Then
                        Return lstr_error
                    End If

                End If

            Catch ex As Exception
                Dim lstre_mesage As String = ex.Message
            End Try



        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''
    Public Function of_InsertFeeContNote(ByVal aobj_Storage As ClsStorage, ByVal astr_Textnote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = aobj_Storage.intContainerStorageFeeId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = 0

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = aobj_Storage.intContainerUniversalId

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = astr_Textnote

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 1

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = astr_HeaderNote

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999



        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


            If Integer.TryParse(ldtb_Result.Rows(0)(0).ToString(), lint_result) = False Then
                lint_result = -1
            End If

            If lint_result > 0 Then
                Return lint_result.ToString()
            End If
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''
    Public Function of_InsertFeeGCargoNote(ByVal aobj_Storage As ClsStorage, ByVal astr_Textnote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = aobj_Storage.intContainerStorageFeeId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = 0

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = aobj_Storage.intGeneralCargoUniversalId

        iolecmd_comand.Parameters.Add("@intGCInventoryItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCInventoryItemId").Value = aobj_Storage.intGCInventoryItemId

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = astr_Textnote

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 1

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = astr_HeaderNote

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999



        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


            If Integer.TryParse(ldtb_Result.Rows(0)(0).ToString(), lint_result) = False Then
                lint_result = -1
            End If

            If lint_result > 0 Then
                Return lint_result.ToString()
            End If
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''
    Public Function of_SetFeeNoteToDocAndUpdateRelated(ByVal aobj_Storage As ClsStorage, ByVal astr_Textnote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String, ByVal astr_Document As String, ByVal aint_NoteItem As Integer) As String
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = aobj_Storage.intContainerStorageFeeId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = aint_NoteItem

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = aobj_Storage.intContainerUniversalId

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = astr_Textnote

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = "PENDCHECK"

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 9

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = astr_HeaderNote

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = astr_Document

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999



        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            Try
                If ldtb_Result.Rows.Count = 1 And ldtb_Result.Columns.Count = 1 Then
                    lstr_error = ldtb_Result.Rows(0)(0).ToString()
                    If lstr_error.Length > 0 Then
                        Return lstr_error
                    End If

                End If 'If ldtb_Result.Rows.Count = 1 And ldtb_Result.Columns.Count = 1 Then

            Catch ex As Exception

            End Try

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    ''
    Public Function of_SetFeeGCargoNoteToDocAndUpdateRelated(ByVal aobj_Storage As ClsStorage, ByVal astr_Textnote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String, ByVal astr_Document As String, ByVal aint_NoteItem As Integer) As String

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = aobj_Storage.intContainerStorageFeeId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = aint_NoteItem

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = aobj_Storage.intGeneralCargoUniversalId

        iolecmd_comand.Parameters.Add("@intGCInventoryItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCInventoryItemId").Value = aobj_Storage.intGCInventoryItemId

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = astr_Textnote

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = "PENDCHECK"

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 9

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = astr_HeaderNote

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = astr_Document

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999



        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""

            Try
                If ldtb_Result.Rows.Count = 1 And ldtb_Result.Columns.Count = 1 Then
                    lstr_error = ldtb_Result.Rows(0)(0).ToString()
                    If lstr_error.Length > 0 Then
                        Return lstr_error
                    End If

                End If 'If ldtb_Result.Rows.Count = 1 And ldtb_Result.Columns.Count = 1 Then

            Catch ex As Exception

            End Try

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return lstr_error

    End Function
    '''

    ''
    <WebMethod()>
    Public Function BK_UpdateCancelStorageFee(ByVal astruc_StorageList As ClsStorage(), ByVal astr_Status As String, ByVal astr_TextNote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String, ByVal astr_DocumentNameArray As String()) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim ldtb_innerResult = New DataTable("inner")
        Dim ldtb_FormatedTabResult As DataTable = New DataTable("updateresult")
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lintResult As Integer = 0
        Dim lstr_tempresult As String = ""
        Dim lstr_result As String = ""
        Dim lrow As DataRow
        Dim lint_note As Integer
        '''
        Dim lstr_Container As String
        Dim lstr_Service As String
        Dim lint_UniversalId As Integer
        Dim ldtb_TableResultDocs As New DataTable("resultDoc")
        Dim lrowDoc As DataRow
        Dim lstr_ContainerCargoType As String



        '' crear resultado 
        ldtb_FormatedTabResult.Columns.Add("intNoteId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intFeeId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strMessage", GetType(String))

        ldtb_FormatedTabResult.Columns.Add("strContainerId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strService", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intContainerUniversalId", GetType(String))

        '' para resultado de llamada a documento
        ldtb_TableResultDocs.Columns.Add("strResultDate", GetType(String))
        ldtb_TableResultDocs.Columns.Add("strDocName", GetType(String))


        ''' primero revisar si es carga
        Try
            If astruc_StorageList(0).strContainerCargoType = "CARGO" Then

                Return UpdateCancelGeneralCargoStorageFee(astruc_StorageList, astr_Status, astr_TextNote, astr_username, astr_HeaderNote, astr_DocumentNameArray)

            End If
        Catch ex As Exception

        End Try

        ''''''''


        ' ver el modo, si es UPDATE, actualizar el estatus por usuario
        If astr_Status = "UPDATE" Then

            For Each lobstorage As ClsStorage In astruc_StorageList

                ldtb_innerResult = New DataTable("innerresult")

                lstr_Container = ""
                lstr_Service = ""
                lint_UniversalId = 0

                ldtb_innerResult = ReadFeeContInfo(lobstorage.intContainerStorageFeeId)

                'ver si se puede obtener la informacion 
                Try
                    lstr_Container = ldtb_innerResult(0)("strContainerId").ToString
                    lstr_Service = ldtb_innerResult(0)("strService").ToString
                    lint_UniversalId = ldtb_innerResult(0)("intContainerUniversalId").ToString()

                Catch ex As Exception

                    lstr_Container = ""
                    lstr_Service = ""
                    lint_UniversalId = 0
                End Try

                lstr_tempresult = UpdateStatusStorageFeeByUser(lobstorage.intContainerStorageFeeId, astr_username, lobstorage.strContainerCargoType)

                'If lstr_result.Length = 0 Then
                '    lstr_result = lstr_tempresult

                'End If
                lrow = ldtb_FormatedTabResult.NewRow()
                lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                lrow("intNoteId") = 0
                lrow("strMessage") = lstr_tempresult


                lrow("strContainerId") = lstr_Container
                lrow("strService") = lstr_Service
                lrow("intContainerUniversalId") = lint_UniversalId



                ldtb_FormatedTabResult.Rows.Add(lrow)

            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult

        End If ''        If astr_Status = "UPDATE" Then

        '' cancelar
        If astr_Status = "CANCELAR" Then

            '' si es cancelar, llamar al memtodo de cancelar, y despues al metodo de insertar nota 
            For Each lobstorage As ClsStorage In astruc_StorageList


                ''  obtener la informacion adicional del almacenaje
                ldtb_innerResult = New DataTable("innerresult")

                lstr_Container = ""
                lstr_Service = ""
                lint_UniversalId = 0

                ldtb_innerResult = ReadFeeContInfo(lobstorage.intContainerStorageFeeId)

                'ver si se puede obtener la informacion 
                Try
                    lstr_Container = ldtb_innerResult(0)("strContainerId").ToString
                    lstr_Service = ldtb_innerResult(0)("strService").ToString
                    lint_UniversalId = ldtb_innerResult(0)("intContainerUniversalId").ToString()

                Catch ex As Exception

                    lstr_Container = ""
                    lstr_Service = ""
                    lint_UniversalId = 0
                End Try
                '''  fin informacion almacenaje

                lrow = ldtb_FormatedTabResult.NewRow()
                lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                lrow("intNoteId") = 0
                lrow("strMessage") = lstr_tempresult

                lint_note = 0

                lstr_tempresult = CancelStorageFee(lobstorage.intContainerStorageFeeId, astr_username)

                If lstr_tempresult.Length = 0 Then

                    lstr_tempresult = of_InsertFeeContNote(lobstorage, astr_TextNote, astr_username, astr_HeaderNote)

                    If Integer.TryParse(lstr_tempresult, lint_note) = False Then
                        lint_note = 0
                    End If

                End If ''If lstr_tempresult.Length = 0 Then

                'si creo la nota 
                If lint_note > 0 Then
                    '' hacer la llamada de asociacion de notas con documentos, si es que hay lista de documentos y el nombre de documento tiene valor
                    For Each lstr_DocumentElement As String In astr_DocumentNameArray

                        lstr_tempresult = of_SetFeeNoteToDocAndUpdateRelated(lobstorage, astr_TextNote, astr_username, astr_HeaderNote, lstr_DocumentElement, lint_note)

                        If lstr_tempresult.Length > 0 Then
                            lrowDoc = ldtb_TableResultDocs.NewRow()
                            lrowDoc("strResultDate") = lstr_tempresult
                            lrowDoc("strDocName") = lstr_DocumentElement

                            ldtb_TableResultDocs.Rows.Add(lrowDoc)
                        End If
                    Next
                    ''' For Each lstr_DocumentElement As String In astr_DocumentNameArray
                    ''' 
                End If
                '' fin si creo la nota 


                If lint_note > 0 Then
                    lrow("strMessage") = ""
                    lrow("intNoteId") = lint_note

                    '' si hubo errores de asociacion de documento , insertarle el primero
                    If ldtb_TableResultDocs.Rows.Count > 0 And ldtb_TableResultDocs.Columns.Count > 0 Then
                        lrow("strMessage") = ldtb_TableResultDocs(0)(0).ToString()
                    End If ''If ldtb_FormatedTabResult.Rows.Count > 0 Then

                Else
                    lrow("strMessage") = lstr_tempresult
                    lrow("intNoteId") = 0

                End If


                lrow("strContainerId") = lstr_Container
                lrow("strService") = lstr_Service
                lrow("intContainerUniversalId") = lint_UniversalId



                ldtb_FormatedTabResult.Rows.Add(lrow)

            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult
        End If '' fin cancelar 

        Return ldtb_Result

    End Function
    ''
    <WebMethod()>
    Public Function UpdateCancelStorageFee(ByVal astruc_StorageList As ClsStorage(), ByVal astr_Status As String, ByVal astr_TextNote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String, ByVal astr_DocumentNameArray As String()) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim ldtb_innerResult = New DataTable("inner")
        Dim ldtb_FormatedTabResult As DataTable = New DataTable("updateresult")
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lintResult As Integer = 0
        Dim lstr_tempresult As String = ""
        Dim lstr_result As String = ""
        Dim lrow As DataRow
        Dim lint_note As Integer
        '''
        Dim lstr_Container As String
        Dim lstr_Service As String
        Dim lint_UniversalId As Integer
        Dim ldtb_TableResultDocs As New DataTable("resultDoc")
        Dim lrowDoc As DataRow
        Dim lstr_ContainerCargoType As String
        Dim llng_GCUniversalId As Long
        Dim lint_GCitem As Integer



        '' crear resultado 
        ldtb_FormatedTabResult.Columns.Add("intNoteId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intFeeId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strMessage", GetType(String))

        ldtb_FormatedTabResult.Columns.Add("strContainerId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strService", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intContainerUniversalId", GetType(String))
        ''
        '' para resultado de llamada a documento
        ldtb_TableResultDocs.Columns.Add("strResultDate", GetType(String))
        ldtb_TableResultDocs.Columns.Add("strDocName", GetType(String))

        '' adicion en la carga general
        ldtb_FormatedTabResult.Columns.Add("intGeneralCargoUniversalId", GetType(String))

        ldtb_FormatedTabResult.Columns.Add("intGCInventoryItemId", GetType(String))


        ''' primero revisar si es carga
        'Try
        '    If astruc_StorageList(0).strContainerCargoType = "CARGO" Then

        '        Return UpdateCancelGeneralCargoStorageFee(astruc_StorageList, astr_Status, astr_TextNote, astr_username, astr_HeaderNote, astr_DocumentNameArray)

        '    End If
        'Catch ex As Exception

        'End Try

        ''''''''


        ' ver el modo, si es UPDATE, actualizar el estatus por usuario
        If astr_Status = "UPDATE" Then

            For Each lobstorage As ClsStorage In astruc_StorageList

                ldtb_innerResult = New DataTable("innerresult")

                lstr_Container = ""
                lstr_Service = ""
                lint_UniversalId = 0

                lstr_Service = ""
                llng_GCUniversalId = 0
                lint_GCitem = 0

                If lobstorage.strContainerCargoType = "CARGO" Then
                    ldtb_innerResult = ReadFeeGCargoInfo(lobstorage.intContainerStorageFeeId)
                    'ver si se puede obtener la informacion 
                    Try
                        If Long.TryParse(ldtb_innerResult(0)("intGeneralCargoUniversalId").ToString, llng_GCUniversalId) = False Then
                            llng_GCUniversalId = 0
                        End If
                        If Integer.TryParse(ldtb_innerResult(0)("intGCInventoryItemId").ToString, lint_GCitem) = False Then
                            lint_GCitem = 0
                        End If

                        lstr_Service = ldtb_innerResult(0)("strService").ToString

                    Catch ex As Exception


                        lstr_Service = ""
                        llng_GCUniversalId = 0
                        lint_GCitem = 0
                    End Try
                    lstr_tempresult = UpdateStatusGeneralCargoStorageFeeByUser(lobstorage.intContainerStorageFeeId, astr_username)

                    'If lstr_result.Length = 0 Then
                    '    lstr_result = lstr_tempresult

                    'End If
                    lrow = ldtb_FormatedTabResult.NewRow()
                    lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                    lrow("intNoteId") = 0
                    lrow("strMessage") = lstr_tempresult

                    lrow("intGeneralCargoUniversalId") = llng_GCUniversalId
                    lrow("intGCInventoryItemId") = lint_GCitem
                    lrow("strService") = lstr_Service

                    lrow("strContainerId") = ""
                    lrow("strService") = lstr_Service
                    lrow("intContainerUniversalId") = "0"

                    ldtb_FormatedTabResult.Rows.Add(lrow)



                Else ' contenedor
                    ldtb_innerResult = ReadFeeContInfo(lobstorage.intContainerStorageFeeId)
                    'ver si se puede obtener la informacion 
                    Try
                        lstr_Container = ldtb_innerResult(0)("strContainerId").ToString
                        lstr_Service = ldtb_innerResult(0)("strService").ToString
                        lint_UniversalId = ldtb_innerResult(0)("intContainerUniversalId").ToString()

                    Catch ex As Exception

                        lstr_Container = ""
                        lstr_Service = ""
                        lint_UniversalId = 0
                    End Try

                    lstr_tempresult = UpdateStatusStorageFeeByUser(lobstorage.intContainerStorageFeeId, astr_username, lobstorage.strContainerCargoType)

                    'If lstr_result.Length = 0 Then
                    '    lstr_result = lstr_tempresult

                    'End If
                    lrow = ldtb_FormatedTabResult.NewRow()
                    lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                    lrow("intNoteId") = 0
                    lrow("strMessage") = lstr_tempresult


                    lrow("strContainerId") = lstr_Container
                    lrow("strService") = lstr_Service
                    lrow("intContainerUniversalId") = lint_UniversalId

                    lrow("intGeneralCargoUniversalId") = 0
                    lrow("intGCInventoryItemId") = 0

                    ldtb_FormatedTabResult.Rows.Add(lrow)

                End If



            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult

        End If ''        If astr_Status = "UPDATE" Then

        '' cancelar
        If astr_Status = "CANCELAR" Then

            '' si es cancelar, llamar al memtodo de cancelar, y despues al metodo de insertar nota 
            For Each lobstorage As ClsStorage In astruc_StorageList

                If lobstorage.strContainerCargoType = "CARGO" Then

                    ''  obtener la informacion adicional del almacenaje
                    ldtb_innerResult = New DataTable("innerresult")


                    lstr_Service = ""
                    lint_UniversalId = 0

                    ldtb_innerResult = ReadFeeGCargoInfo(lobstorage.intContainerStorageFeeId)

                    'ver si se puede obtener la informacion 
                    Try
                        If Long.TryParse(ldtb_innerResult(0)("intGeneralCargoUniversalId").ToString, llng_GCUniversalId) = False Then
                            llng_GCUniversalId = 0
                        End If
                        If Integer.TryParse(ldtb_innerResult(0)("intGCInventoryItemId").ToString, lint_GCitem) = False Then
                            lint_GCitem = 0
                        End If

                        lstr_Service = ldtb_innerResult(0)("strService").ToString

                    Catch ex As Exception

                        lint_GCitem = 0
                        lstr_Service = ""
                        llng_GCUniversalId = 0

                    End Try
                    '''  fin informacion almacenaje

                    lrow = ldtb_FormatedTabResult.NewRow()
                    lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                    lrow("intNoteId") = 0
                    lrow("strMessage") = lstr_tempresult


                    lrow("strContainerId") = ""
                    lrow("intContainerUniversalId") = ""

                    lint_note = 0

                    lstr_tempresult = CancelStorageGeneralCargoFee(lobstorage.intContainerStorageFeeId, astr_username)

                    If lstr_tempresult.Length = 0 Then

                        lstr_tempresult = of_InsertFeeGCargoNote(lobstorage, astr_TextNote, astr_username, astr_HeaderNote)

                        If Integer.TryParse(lstr_tempresult, lint_note) = False Then
                            lint_note = 0
                        End If

                    End If ''If lstr_tempresult.Length = 0 Then

                    'si creo la nota 
                    If lint_note > 0 Then
                        '' hacer la llamada de asociacion de notas con documentos, si es que hay lista de documentos y el nombre de documento tiene valor
                        For Each lstr_DocumentElement As String In astr_DocumentNameArray

                            lstr_tempresult = of_SetFeeGCargoNoteToDocAndUpdateRelated(lobstorage, astr_TextNote, astr_username, astr_HeaderNote, lstr_DocumentElement, lint_note)

                            If lstr_tempresult.Length > 0 Then
                                lrowDoc = ldtb_TableResultDocs.NewRow()
                                lrowDoc("strResultDate") = lstr_tempresult
                                lrowDoc("strDocName") = lstr_DocumentElement

                                ldtb_TableResultDocs.Rows.Add(lrowDoc)
                            End If
                        Next
                        ''' For Each lstr_DocumentElement As String In astr_DocumentNameArray
                        ''' 
                    End If
                    '' fin si creo la nota 


                    If lint_note > 0 Then
                        lrow("strMessage") = ""
                        lrow("intNoteId") = lint_note

                        '' si hubo errores de asociacion de documento , insertarle el primero
                        If ldtb_TableResultDocs.Rows.Count > 0 And ldtb_TableResultDocs.Columns.Count > 0 Then
                            lrow("strMessage") = ldtb_TableResultDocs(0)(0).ToString()
                        End If ''If ldtb_FormatedTabResult.Rows.Count > 0 Then

                    Else
                        lrow("strMessage") = lstr_tempresult
                        lrow("intNoteId") = 0

                    End If


                    lrow("intGCInventoryItemId") = lint_GCitem
                    lrow("strService") = lstr_Service
                    lrow("intGeneralCargoUniversalId") = llng_GCUniversalId



                    ldtb_FormatedTabResult.Rows.Add(lrow)


                Else ' contenedor
                    ''  obtener la informacion adicional del almacenaje
                    ldtb_innerResult = New DataTable("innerresult")

                    lstr_Container = ""
                    lstr_Service = ""
                    lint_UniversalId = 0

                    ldtb_innerResult = ReadFeeContInfo(lobstorage.intContainerStorageFeeId)

                    'ver si se puede obtener la informacion 
                    Try
                        lstr_Container = ldtb_innerResult(0)("strContainerId").ToString
                        lstr_Service = ldtb_innerResult(0)("strService").ToString
                        lint_UniversalId = ldtb_innerResult(0)("intContainerUniversalId").ToString()

                    Catch ex As Exception

                        lstr_Container = ""
                        lstr_Service = ""
                        lint_UniversalId = 0
                    End Try
                    '''  fin informacion almacenaje

                    lrow = ldtb_FormatedTabResult.NewRow()
                    lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                    lrow("intNoteId") = 0
                    lrow("strMessage") = lstr_tempresult


                    lrow("intGCInventoryItemId") = 0
                    lrow("intGeneralCargoUniversalId") = 0


                    lint_note = 0

                    lstr_tempresult = CancelStorageFee(lobstorage.intContainerStorageFeeId, astr_username)

                    If lstr_tempresult.Length = 0 Then

                        lstr_tempresult = of_InsertFeeContNote(lobstorage, astr_TextNote, astr_username, astr_HeaderNote)

                        If Integer.TryParse(lstr_tempresult, lint_note) = False Then
                            lint_note = 0
                        End If

                    End If ''If lstr_tempresult.Length = 0 Then

                    'si creo la nota 
                    If lint_note > 0 Then
                        '' hacer la llamada de asociacion de notas con documentos, si es que hay lista de documentos y el nombre de documento tiene valor
                        For Each lstr_DocumentElement As String In astr_DocumentNameArray

                            lstr_tempresult = of_SetFeeNoteToDocAndUpdateRelated(lobstorage, astr_TextNote, astr_username, astr_HeaderNote, lstr_DocumentElement, lint_note)

                            If lstr_tempresult.Length > 0 Then
                                lrowDoc = ldtb_TableResultDocs.NewRow()
                                lrowDoc("strResultDate") = lstr_tempresult
                                lrowDoc("strDocName") = lstr_DocumentElement

                                ldtb_TableResultDocs.Rows.Add(lrowDoc)
                            End If
                        Next
                        ''' For Each lstr_DocumentElement As String In astr_DocumentNameArray
                        ''' 
                    End If
                    '' fin si creo la nota 


                    If lint_note > 0 Then
                        lrow("strMessage") = ""
                        lrow("intNoteId") = lint_note

                        '' si hubo errores de asociacion de documento , insertarle el primero
                        If ldtb_TableResultDocs.Rows.Count > 0 And ldtb_TableResultDocs.Columns.Count > 0 Then
                            lrow("strMessage") = ldtb_TableResultDocs(0)(0).ToString()
                        End If ''If ldtb_FormatedTabResult.Rows.Count > 0 Then

                    Else
                        lrow("strMessage") = lstr_tempresult
                        lrow("intNoteId") = 0

                    End If


                    lrow("strContainerId") = lstr_Container
                    lrow("strService") = lstr_Service
                    lrow("intContainerUniversalId") = lint_UniversalId



                    ldtb_FormatedTabResult.Rows.Add(lrow)


                End If



            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult
        End If '' fin cancelar 

        Return ldtb_Result

    End Function

    ''
    <WebMethod()>
    Public Function xyzUpdateCancelStorageFee(ByVal astruc_StorageList As ClsStorage(), ByVal astr_Status As String, ByVal astr_TextNote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String, ByVal astr_DocumentNameArray As String()) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim ldtb_innerResult = New DataTable("inner")
        Dim ldtb_FormatedTabResult As DataTable = New DataTable("updateresult")
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lintResult As Integer = 0
        Dim lstr_tempresult As String = ""
        Dim lstr_result As String = ""
        Dim lrow As DataRow
        Dim lint_note As Integer
        '''
        Dim lstr_Container As String
        Dim lstr_Service As String
        Dim lint_UniversalId As Integer
        Dim ldtb_TableResultDocs As New DataTable("resultDoc")
        Dim lrowDoc As DataRow



        '' crear resultado 
        ldtb_FormatedTabResult.Columns.Add("intNoteId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intFeeId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strMessage", GetType(String))

        ldtb_FormatedTabResult.Columns.Add("strContainerId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strService", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intContainerUniversalId", GetType(String))

        '' para resultado de llamada a documento
        ldtb_TableResultDocs.Columns.Add("strResultDate", GetType(String))
        ldtb_TableResultDocs.Columns.Add("strDocName", GetType(String))



        ' ver el modo, si es UPDATE, actualizar el estatus por usuario
        If astr_Status = "UPDATE" Then

            For Each lobstorage As ClsStorage In astruc_StorageList

                ldtb_innerResult = New DataTable("innerresult")

                lstr_Container = ""
                lstr_Service = ""
                lint_UniversalId = 0

                ldtb_innerResult = ReadFeeContInfo(lobstorage.intContainerStorageFeeId)

                'ver si se puede obtener la informacion 
                Try
                    lstr_Container = ldtb_innerResult(0)("strContainerId").ToString
                    lstr_Service = ldtb_innerResult(0)("strService").ToString
                    lint_UniversalId = ldtb_innerResult(0)("intContainerUniversalId").ToString()

                Catch ex As Exception

                    lstr_Container = ""
                    lstr_Service = ""
                    lint_UniversalId = 0
                End Try

                lstr_tempresult = UpdateStatusStorageFeeByUser(lobstorage.intContainerStorageFeeId, astr_username, lobstorage.strContainerCargoType)

                'If lstr_result.Length = 0 Then
                '    lstr_result = lstr_tempresult

                'End If
                lrow = ldtb_FormatedTabResult.NewRow()
                lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                lrow("intNoteId") = 0
                lrow("strMessage") = lstr_tempresult


                lrow("strContainerId") = lstr_Container
                lrow("strService") = lstr_Service
                lrow("intContainerUniversalId") = lint_UniversalId



                ldtb_FormatedTabResult.Rows.Add(lrow)

            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult

        End If ''        If astr_Status = "UPDATE" Then

        '' cancelar
        If astr_Status = "CANCELAR" Then

            '' si es cancelar, llamar al memtodo de cancelar, y despues al metodo de insertar nota 
            For Each lobstorage As ClsStorage In astruc_StorageList


                ''  obtener la informacion adicional del almacenaje
                ldtb_innerResult = New DataTable("innerresult")

                lstr_Container = ""
                lstr_Service = ""
                lint_UniversalId = 0

                ldtb_innerResult = ReadFeeContInfo(lobstorage.intContainerStorageFeeId)

                'ver si se puede obtener la informacion 
                Try
                    lstr_Container = ldtb_innerResult(0)("strContainerId").ToString
                    lstr_Service = ldtb_innerResult(0)("strService").ToString
                    lint_UniversalId = ldtb_innerResult(0)("intContainerUniversalId").ToString()

                Catch ex As Exception

                    lstr_Container = ""
                    lstr_Service = ""
                    lint_UniversalId = 0
                End Try
                '''  fin informacion almacenaje

                lrow = ldtb_FormatedTabResult.NewRow()
                lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                lrow("intNoteId") = 0
                lrow("strMessage") = lstr_tempresult

                lint_note = 0

                lstr_tempresult = CancelStorageFee(lobstorage.intContainerStorageFeeId, astr_username)



                If lstr_tempresult.Length = 0 Then

                    lstr_tempresult = of_InsertFeeContNote(lobstorage, astr_TextNote, astr_username, astr_HeaderNote)

                    If Integer.TryParse(lstr_tempresult, lint_note) = False Then
                        lint_note = 0
                    End If

                End If ''If lstr_tempresult.Length = 0 Then

                Return dt_RetrieveErrorTable("cancelacion :" + lobstorage.intContainerStorageFeeId.ToString + ":" + lstr_tempresult + "_")

                'si creo la nota 
                If lint_note > 0 Then
                    '' hacer la llamada de asociacion de notas con documentos, si es que hay lista de documentos y el nombre de documento tiene valor
                    For Each lstr_DocumentElement As String In astr_DocumentNameArray

                        lstr_tempresult = of_SetFeeNoteToDocAndUpdateRelated(lobstorage, astr_TextNote, astr_username, astr_HeaderNote, lstr_DocumentElement, lint_note)

                        If lstr_tempresult.Length > 0 Then
                            lrowDoc = ldtb_TableResultDocs.NewRow()
                            lrowDoc("strResultDate") = lstr_tempresult
                            lrowDoc("strDocName") = lstr_DocumentElement

                            ldtb_TableResultDocs.Rows.Add(lrowDoc)
                        End If
                    Next
                    ''' For Each lstr_DocumentElement As String In astr_DocumentNameArray
                    ''' 
                End If
                '' fin si creo la nota 


                If lint_note > 0 Then
                    lrow("strMessage") = ""
                    lrow("intNoteId") = lint_note

                    '' si hubo errores de asociacion de documento , insertarle el primero
                    If ldtb_TableResultDocs.Rows.Count > 0 And ldtb_TableResultDocs.Columns.Count > 0 Then
                        lrow("strMessage") = ldtb_TableResultDocs(0)(0).ToString()
                    End If ''If ldtb_FormatedTabResult.Rows.Count > 0 Then

                Else
                    lrow("strMessage") = lstr_tempresult
                    lrow("intNoteId") = 0

                End If


                lrow("strContainerId") = lstr_Container
                lrow("strService") = lstr_Service
                lrow("intContainerUniversalId") = lint_UniversalId



                ldtb_FormatedTabResult.Rows.Add(lrow)

            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult
        End If '' fin cancelar 

        Return ldtb_Result

    End Function
    ''

    <WebMethod()>
    Public Function UpdateCancelGeneralCargoStorageFee(ByVal astruc_StorageList As ClsStorage(), ByVal astr_Status As String, ByVal astr_TextNote As String, ByVal astr_username As String, ByVal astr_HeaderNote As String, ByVal astr_DocumentNameArray As String()) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim ldtb_innerResult = New DataTable("inner")
        Dim ldtb_FormatedTabResult As DataTable = New DataTable("updateresult")
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lintResult As Integer = 0
        Dim lstr_tempresult As String = ""
        Dim lstr_result As String = ""
        Dim lrow As DataRow
        Dim lint_note As Integer
        '''
        Dim llng_GCUniversalId As Long
        Dim lint_GCitem As Integer
        Dim lstr_Service As String
        Dim lint_UniversalId As Integer
        Dim ldtb_TableResultDocs As New DataTable("resultDoc")
        Dim lrowDoc As DataRow



        '' crear resultado 
        ldtb_FormatedTabResult.Columns.Add("intNoteId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intFeeId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strMessage", GetType(String))

        ldtb_FormatedTabResult.Columns.Add("intGCInventoryItemId", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("strService", GetType(String))
        ldtb_FormatedTabResult.Columns.Add("intGeneralCargoUniversalId", GetType(String))

        '' para resultado de llamada a documento
        ldtb_TableResultDocs.Columns.Add("strResultDate", GetType(String))
        ldtb_TableResultDocs.Columns.Add("strDocName", GetType(String))



        ' ver el modo, si es UPDATE, actualizar el estatus por usuario
        If astr_Status = "UPDATE" Then

            For Each lobstorage As ClsStorage In astruc_StorageList

                ldtb_innerResult = New DataTable("innerresult")


                lstr_Service = ""
                lint_UniversalId = 0

                ldtb_innerResult = ReadFeeGCargoInfo(lobstorage.intContainerStorageFeeId)


                'ver si se puede obtener la informacion 
                Try
                    If Long.TryParse(ldtb_innerResult(0)("intGeneralCargoUniversalId").ToString, llng_GCUniversalId) = False Then
                        llng_GCUniversalId = 0
                    End If
                    If Integer.TryParse(ldtb_innerResult(0)("intGCInventoryItemId").ToString, lint_GCitem) = False Then
                        lint_GCitem = 0
                    End If

                    lstr_Service = ldtb_innerResult(0)("strService").ToString

                Catch ex As Exception


                    lstr_Service = ""
                    llng_GCUniversalId = 0
                    lint_GCitem = 0
                End Try
                lstr_tempresult = UpdateStatusGeneralCargoStorageFeeByUser(lobstorage.intContainerStorageFeeId, astr_username)

                'If lstr_result.Length = 0 Then
                '    lstr_result = lstr_tempresult

                'End If
                lrow = ldtb_FormatedTabResult.NewRow()
                lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                lrow("intNoteId") = 0
                lrow("strMessage") = lstr_tempresult

                lrow("intGeneralCargoUniversalId") = llng_GCUniversalId
                lrow("intGCInventoryItemId") = lint_GCitem
                lrow("strService") = lstr_Service

                ldtb_FormatedTabResult.Rows.Add(lrow)

            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult

        End If ''        If astr_Status = "UPDATE" Then

        '' cancelar
        If astr_Status = "CANCELAR" Then

            '' si es cancelar, llamar al memtodo de cancelar, y despues al metodo de insertar nota 
            For Each lobstorage As ClsStorage In astruc_StorageList


                ''  obtener la informacion adicional del almacenaje
                ldtb_innerResult = New DataTable("innerresult")


                lstr_Service = ""
                lint_UniversalId = 0

                ldtb_innerResult = ReadFeeGCargoInfo(lobstorage.intContainerStorageFeeId)

                'ver si se puede obtener la informacion 
                Try
                    If Long.TryParse(ldtb_innerResult(0)("intGeneralCargoUniversalId").ToString, llng_GCUniversalId) = False Then
                        llng_GCUniversalId = 0
                    End If
                    If Integer.TryParse(ldtb_innerResult(0)("intGCInventoryItemId").ToString, lint_GCitem) = False Then
                        lint_GCitem = 0
                    End If

                    lstr_Service = ldtb_innerResult(0)("strService").ToString

                Catch ex As Exception

                    lint_GCitem = 0
                    lstr_Service = ""
                    llng_GCUniversalId = 0

                End Try
                '''  fin informacion almacenaje

                lrow = ldtb_FormatedTabResult.NewRow()
                lrow("intFeeId") = lobstorage.intContainerStorageFeeId.ToString()
                lrow("intNoteId") = 0
                lrow("strMessage") = lstr_tempresult

                lint_note = 0

                lstr_tempresult = CancelStorageGeneralCargoFee(lobstorage.intContainerStorageFeeId, astr_username)

                If lstr_tempresult.Length = 0 Then

                    lstr_tempresult = of_InsertFeeGCargoNote(lobstorage, astr_TextNote, astr_username, astr_HeaderNote)

                    If Integer.TryParse(lstr_tempresult, lint_note) = False Then
                        lint_note = 0
                    End If

                End If ''If lstr_tempresult.Length = 0 Then

                'si creo la nota 
                If lint_note > 0 Then
                    '' hacer la llamada de asociacion de notas con documentos, si es que hay lista de documentos y el nombre de documento tiene valor
                    For Each lstr_DocumentElement As String In astr_DocumentNameArray

                        lstr_tempresult = of_SetFeeGCargoNoteToDocAndUpdateRelated(lobstorage, astr_TextNote, astr_username, astr_HeaderNote, lstr_DocumentElement, lint_note)

                        If lstr_tempresult.Length > 0 Then
                            lrowDoc = ldtb_TableResultDocs.NewRow()
                            lrowDoc("strResultDate") = lstr_tempresult
                            lrowDoc("strDocName") = lstr_DocumentElement

                            ldtb_TableResultDocs.Rows.Add(lrowDoc)
                        End If
                    Next
                    ''' For Each lstr_DocumentElement As String In astr_DocumentNameArray
                    ''' 
                End If
                '' fin si creo la nota 


                If lint_note > 0 Then
                    lrow("strMessage") = ""
                    lrow("intNoteId") = lint_note

                    '' si hubo errores de asociacion de documento , insertarle el primero
                    If ldtb_TableResultDocs.Rows.Count > 0 And ldtb_TableResultDocs.Columns.Count > 0 Then
                        lrow("strMessage") = ldtb_TableResultDocs(0)(0).ToString()
                    End If ''If ldtb_FormatedTabResult.Rows.Count > 0 Then

                Else
                    lrow("strMessage") = lstr_tempresult
                    lrow("intNoteId") = 0

                End If


                lrow("intGCInventoryItemId") = lint_GCitem
                lrow("strService") = lstr_Service
                lrow("intGeneralCargoUniversalId") = llng_GCUniversalId



                ldtb_FormatedTabResult.Rows.Add(lrow)

            Next 'For Each lobstorage As ClsStorage In astruc_StorageList

            Return ldtb_FormatedTabResult
        End If '' fin cancelar 

        Return ldtb_Result

    End Function
    ''
    <WebMethod()>
    Public Function ValideteNotessFee(ByVal aobj_NoteItemsList As ClsNoteFee(), ByVal astr_Username As String) As DataTable
        Dim ldtb_Return = New DataTable("DtbMessge") ' la tabla de retorno
        Dim lrow_rw As DataRow
        Dim lstr_result As String
        Dim lstr_TypeContGeneralCargo As String = ""
        ldtb_Return.Columns.Add("strMessge", GetType(String))


        ' si no hay elementos retornar 
        If aobj_NoteItemsList.Count > 0 Then

            For Each noteitem As ClsNoteFee In aobj_NoteItemsList

                lrow_rw = ldtb_Return.NewRow
                '' obtener el tipo de  nota si es que no se ha marcado como algun tipo
                If lstr_TypeContGeneralCargo.Length < 2 Then
                    If noteitem.strNoteType = "GCARGO" Then
                        lstr_TypeContGeneralCargo = noteitem.strNoteType
                    Else
                        lstr_TypeContGeneralCargo = "CONT"
                    End If
                End If

                If lstr_TypeContGeneralCargo = "GCARGO" Then

                    lstr_result = of_validateFeeGcargoNoteItem(noteitem, astr_Username)
                Else

                    lstr_result = of_validateFeeContNoteItem(noteitem, astr_Username)
                End If


                lrow_rw("strMessge") = lstr_result
                ldtb_Return.Rows.Add(lrow_rw)

            Next


        Else

            lrow_rw = ldtb_Return.NewRow
            lrow_rw("strMessge") = "No hay elementos en el listado de notas "
            ldtb_Return.Rows.Add(lrow_rw)

        End If ' If aobj_NoteItemsList.Count > 0 Then

        Return ldtb_Return

    End Function
    '
    <WebMethod()>
    Public Function ReadFeeContInfo(ByVal aint_FeeId As Integer) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 6

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = aint_FeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    '''
    <WebMethod()>
    Public Function ReadFeeGCargoInfo(ByVal aint_FeeId As Integer) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoStorageFee"

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCItemId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerId").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerType").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 6

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = aint_FeeId

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@intFiscalMov", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFiscalMov").Value = 0

        iolecmd_comand.Parameters.Add("@decMinWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMinWeight").Value = 0

        iolecmd_comand.Parameters.Add("@decMaxWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMaxWeight").Value = 0

        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselId").Value = 0

        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselName").Value = ""

        iolecmd_comand.Parameters.Add("@strProductIdName", OleDbType.Char)
        iolecmd_comand.Parameters("@strProductIdName").Value = ""

        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intProductId").Value = 0

        iolecmd_comand.Parameters.Add("@strNumbers", OleDbType.Char)
        iolecmd_comand.Parameters("@strNumbers").Value = ""

        iolecmd_comand.Parameters.Add("@strMarks", OleDbType.Char)
        iolecmd_comand.Parameters("@strMarks").Value = ""

        iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
        iolecmd_comand.Parameters("@strBLName").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intQty", OleDbType.Integer)
        iolecmd_comand.Parameters("@intQty").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    '''
    <WebMethod()>
    Public Function SearchBrokerAndCustomerCompany(ByVal astr_CompanyName As String) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spSearchCompanyEntitys"

        iolecmd_comand.Parameters.Add("@strCompanyName", OleDbType.Char)
        iolecmd_comand.Parameters("@strCompanyName").Value = astr_CompanyName

        iolecmd_comand.Parameters.Add("@intCompanyEntiTyId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCompanyEntiTyId").Value = 0

        iolecmd_comand.Parameters.Add("@intCompanyType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCompanyType").Value = 0

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 0

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function GenerateStorageFeeId(ByVal astruc_FeeRequest As ClsStorage(), ByVal astr_username As String, ByVal arrDocument As ClsDocument(), ByVal astr_Device As String, ByVal adtm_dateEnd As Date) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim lddt_Spresult = New DataTable("spresult")
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim lstr_datetime As String = ""
        Dim lstr_datetimefinal As String = ""
        Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lrow As DataRow
        '' variables de datos fiscales 
        Dim lstr_CompanyRFC As String = ""
        Dim lstr_FiscalAddress As String = ""
        Dim lstr_CompanyCity As String = ""
        Dim lstr_CompanyState As String = ""
        Dim lstr_CompanyZipCode As String = ""
        Dim lstr_CFDIUsageType As String = ""
        Dim lstr_PaymentMethodTypeId As String = ""
        Dim lint_PaymentForm As Integer = 0
        Dim lint_HasToUpdateFiscalData As Integer = 0
        Dim lint_RequiredBy As Integer
        Dim lint_RequiredByType As Integer
        Dim lint_InvoiceToId As Integer
        Dim lint_InvoiceTypeId As Integer
        Dim lobj_ItemDocument As ClsDocument = New ClsDocument()
        Dim lint_itemIndex As Integer
        Dim lint_intemFound As Integer
        Dim llng_TempDoc As Long
        Dim llng_FeeId As Long
        Dim lstr_DateEndParam As String

        ldtb_Result = New DataTable("User")
        lddt_Spresult = New DataTable("spresutl")
        ' agregar una columna 
        ldtb_Result.Columns.Add("UniversalId", GetType(String))
        ldtb_Result.Columns.Add("FeeId", GetType(String))
        ldtb_Result.Columns.Add("strType", GetType(String))
        ldtb_Result.Columns.Add("strDocInfo", GetType(String))
        ldtb_Result.Columns.Add("intQuantityExcepDays", GetType(String))


        'istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        'ioleconx_conexion.ConnectionString = istr_conx
        'iolecmd_comand = ioleconx_conexion.CreateCommand()

        ''' validar cantidad de registros
        If astruc_FeeRequest.Length = 0 Then
            Return dt_RetrieveErrorTable("No tiene elementos solicitados")
        End If


        '' recorres el arreglo
        For Each lobj_item As ClsStorage In astruc_FeeRequest

            'resetar conexion
            ioleconx_conexion = New OleDbConnection() '' objeto de conexion que se usara para conectar 

            'resetear comando
            iolecmd_comand = New OleDbCommand()

            'configurar comando y conexion
            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx
            iolecmd_comand = ioleconx_conexion.CreateCommand()


            Select Case lobj_item.str_ServiceType
                Case "ALM" ' almacenaje"


                    'analizar la variable objeto
                    If Not (lobj_item.iobj_FiscalObj Is Nothing) Then
                        lstr_CompanyRFC = lobj_item.iobj_FiscalObj.strCompanyFiscalIdentifier
                        lstr_FiscalAddress = lobj_item.iobj_FiscalObj.strCompanyAddress1
                        lstr_CompanyCity = lobj_item.iobj_FiscalObj.strCompanyCity
                        lstr_CompanyState = lobj_item.iobj_FiscalObj.strCompanyState
                        lstr_CompanyZipCode = lobj_item.iobj_FiscalObj.strCompanyZipCode
                        lstr_CFDIUsageType = lobj_item.iobj_FiscalObj.strCFDIUsageTypeId
                        lstr_PaymentMethodTypeId = lobj_item.iobj_FiscalObj.strPaymentMethodTypeId
                        lint_PaymentForm = lobj_item.iobj_FiscalObj.intPaymentFormTypeId
                        lint_HasToUpdateFiscalData = 1
                        lint_RequiredBy = lobj_item.int_RequiredBy
                        lint_RequiredByType = lobj_item.int_RequiredByType
                        lint_InvoiceToId = lobj_item.int_Invoiceid
                        lint_InvoiceTypeId = lobj_item.int_InvoiceTypeId
                    End If



                    iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intContainerUniversalId").Value = lobj_item.intContainerUniversalId

                    iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intRequiredBy").Value = lint_RequiredBy

                    iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intRequiredByType").Value = lint_RequiredByType

                    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
                    iolecmd_comand.Parameters("@strUsername").Value = astr_username

                    iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intmode").Value = 0

                    iolecmd_comand.Parameters.Add("@intInvoicetoId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intInvoicetoId").Value = lint_InvoiceToId

                    iolecmd_comand.Parameters.Add("@intInvoceTypeId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intInvoceTypeId").Value = lint_InvoiceTypeId



                    '' pasar la informacion fiscal

                    iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@intPaymentForm", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intPaymentForm").Value = lint_PaymentForm

                    iolecmd_comand.Parameters.Add("@strCFDIUsageTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCFDIUsageTypeId").Value = lstr_CFDIUsageType

                    iolecmd_comand.Parameters.Add("@blnIsDirectCredit", OleDbType.Integer)
                    iolecmd_comand.Parameters("@blnIsDirectCredit").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalAddress", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalAddress").Value = lstr_FiscalAddress

                    iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyCity").Value = lstr_CompanyCity

                    iolecmd_comand.Parameters.Add("@strCompanyState", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyState").Value = lstr_CompanyState

                    iolecmd_comand.Parameters.Add("@strCompanyZipCode", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyZipCode").Value = lstr_CompanyZipCode

                    iolecmd_comand.Parameters.Add("@strCompanyRFC", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyRFC").Value = lstr_CompanyRFC

                    iolecmd_comand.Parameters.Add("@strDevice", OleDbType.Char)
                    iolecmd_comand.Parameters("@strDevice").Value = astr_Device

                    iolecmd_comand.Parameters.Add("@intFiscalMovementId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intFiscalMovementId").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalMovement", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalMovement").Value = lobj_item.strFiscalMovement

                    'obtener parametros de fecha final
                    lstr_DateEndParam = of_ConvertDateToStringGeneralFormat(adtm_dateEnd)

                    iolecmd_comand.Parameters.Add("@adtmCalcEndDate", OleDbType.Char)
                    iolecmd_comand.Parameters("@adtmCalcEndDate").Value = lstr_DateEndParam

                    '''
                    iolecmd_comand.Parameters.Add("@strSatusToSave", OleDbType.Char)
                    iolecmd_comand.Parameters("@strSatusToSave").Value = lobj_item.strSaveStatus

                    ''''''



                    '' nombre del sp 
                    strSQL = "spGenerateContainerStorageFee"

                Case "MUELLAJE" ' muellaje


                    'analizar la variable objeto
                    If Not (lobj_item.iobj_FiscalObj Is Nothing) Then
                        lstr_CompanyRFC = lobj_item.iobj_FiscalObj.strCompanyFiscalIdentifier
                        lstr_FiscalAddress = lobj_item.iobj_FiscalObj.strCompanyAddress1
                        lstr_CompanyCity = lobj_item.iobj_FiscalObj.strCompanyCity
                        lstr_CompanyState = lobj_item.iobj_FiscalObj.strCompanyState
                        lstr_CompanyZipCode = lobj_item.iobj_FiscalObj.strCompanyZipCode
                        lstr_CFDIUsageType = lobj_item.iobj_FiscalObj.strCFDIUsageTypeId
                        lstr_PaymentMethodTypeId = lobj_item.iobj_FiscalObj.strPaymentMethodTypeId
                        lint_PaymentForm = lobj_item.iobj_FiscalObj.intPaymentFormTypeId
                        lint_HasToUpdateFiscalData = 1
                        lint_InvoiceToId = lobj_item.int_InvoiceTypeId
                        lint_InvoiceTypeId = lobj_item.int_InvoiceTypeId
                        lint_RequiredBy = lobj_item.int_RequiredBy
                        lint_RequiredByType = lobj_item.int_RequiredByType
                    End If


                    iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strContainerId").Value = lobj_item.str_ContainerId

                    iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intContainerUniversalId").Value = lobj_item.intContainerUniversalId

                    iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intRequiredBy").Value = lint_RequiredBy

                    iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intRequiredByType").Value = lint_RequiredByType

                    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
                    iolecmd_comand.Parameters("@strUsername").Value = astr_username

                    iolecmd_comand.Parameters.Add("@intmode", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intmode").Value = 1

                    iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intStorageFeeId").Value = lobj_item.intContainerStorageFeeId

                    iolecmd_comand.Parameters.Add("@strFiscalPetitionName ", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalPetitionName ").Value = lobj_item.strFiscalPetitionName

                    iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
                    iolecmd_comand.Parameters("@strBLName").Value = lobj_item.strBLName

                    'fecha
                    lstr_datetime = of_ConvertDateToStringGeneralFormat(lobj_item.dtmFiscalPetitionDate)

                    iolecmd_comand.Parameters.Add("@dtmFiscalPetitionDate", OleDbType.Char)
                    iolecmd_comand.Parameters("@dtmFiscalPetitionDate").Value = lstr_datetime

                    '' PESO
                    iolecmd_comand.Parameters.Add("@decFiscalPetitionWeight", OleDbType.Decimal)
                    iolecmd_comand.Parameters("@decFiscalPetitionWeight").Value = lobj_item.decFiscalPetitionWeight
                    '''''''''

                    '' nombre del sp 
                    strSQL = "spCRUDContainerDockingPay"





                    iolecmd_comand.Parameters.Add("@intInvoicetoId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intInvoicetoId").Value = lint_InvoiceToId

                    iolecmd_comand.Parameters.Add("@intInvoceTypeId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intInvoceTypeId").Value = lint_InvoiceTypeId

                    iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@intPaymentForm", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intPaymentForm").Value = lint_PaymentForm

                    iolecmd_comand.Parameters.Add("@strCFDIUsageTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCFDIUsageTypeId").Value = lstr_CFDIUsageType

                    iolecmd_comand.Parameters.Add("@blnIsDirectCredit", OleDbType.Integer)
                    iolecmd_comand.Parameters("@blnIsDirectCredit").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalAddress", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalAddress").Value = lstr_FiscalAddress

                    iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyCity").Value = lstr_CompanyCity

                    iolecmd_comand.Parameters.Add("@strCompanyState", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyState").Value = lstr_CompanyState

                    iolecmd_comand.Parameters.Add("@strCompanyZipCode", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyZipCode").Value = lstr_CompanyZipCode

                    iolecmd_comand.Parameters.Add("@strCompanyRFC", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyRFC").Value = lstr_CompanyRFC


                    iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@strDevice", OleDbType.Char)
                    iolecmd_comand.Parameters("@strDevice").Value = astr_Device






                Case "PBIP" 'pbip 

                    'analizar la variable objeto
                    If Not (lobj_item.iobj_FiscalObj Is Nothing) Then
                        lstr_CompanyRFC = lobj_item.iobj_FiscalObj.strCompanyFiscalIdentifier
                        lstr_FiscalAddress = lobj_item.iobj_FiscalObj.strCompanyAddress1
                        lstr_CompanyCity = lobj_item.iobj_FiscalObj.strCompanyCity
                        lstr_CompanyState = lobj_item.iobj_FiscalObj.strCompanyState
                        lstr_CompanyZipCode = lobj_item.iobj_FiscalObj.strCompanyZipCode
                        lstr_CFDIUsageType = lobj_item.iobj_FiscalObj.strCFDIUsageTypeId
                        lstr_PaymentMethodTypeId = lobj_item.iobj_FiscalObj.strPaymentMethodTypeId
                        lint_PaymentForm = lobj_item.iobj_FiscalObj.intPaymentFormTypeId
                        lint_HasToUpdateFiscalData = 1
                        lint_InvoiceToId = lobj_item.int_InvoiceTypeId
                        lint_InvoiceTypeId = lobj_item.int_InvoiceTypeId
                        lint_RequiredBy = lobj_item.int_RequiredBy
                        lint_RequiredByType = lobj_item.int_RequiredByType
                    End If


                    '' nombre del sp 
                    strSQL = "spCRUDContainerPBIP"


                    iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strContainerId").Value = lobj_item.str_ContainerId

                    iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intContainerUniversalId").Value = lobj_item.intContainerUniversalId

                    iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intRequiredBy").Value = lint_RequiredBy

                    iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intRequiredByType").Value = lint_RequiredByType

                    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
                    iolecmd_comand.Parameters("@strUsername").Value = astr_username

                    iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intmode").Value = 1

                    iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intStorageFeeId").Value = lobj_item.intContainerStorageFeeId

                    iolecmd_comand.Parameters.Add("@strFiscalPetitionName", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalPetitionName").Value = lobj_item.strFiscalPetitionName

                    iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
                    iolecmd_comand.Parameters("@strBLName").Value = lobj_item.strBLName

                    iolecmd_comand.Parameters.Add("@intInvoicetoId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intInvoicetoId").Value = lint_InvoiceToId

                    iolecmd_comand.Parameters.Add("@intInvoceTypeId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intInvoceTypeId").Value = lint_InvoiceTypeId

                    iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@intPaymentForm", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intPaymentForm").Value = lint_PaymentForm

                    iolecmd_comand.Parameters.Add("@strCFDIUsageTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCFDIUsageTypeId").Value = lstr_CFDIUsageType

                    iolecmd_comand.Parameters.Add("@blnIsDirectCredit", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@blnIsDirectCredit").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalAddress", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalAddress").Value = lstr_FiscalAddress

                    iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyCity").Value = lstr_CompanyCity

                    iolecmd_comand.Parameters.Add("@strCompanyState", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyState").Value = lstr_CompanyState

                    iolecmd_comand.Parameters.Add("@strCompanyZipCode", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyZipCode").Value = lstr_CompanyZipCode

                    iolecmd_comand.Parameters.Add("@strCompanyRFC", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyRFC").Value = lstr_CompanyRFC

                    iolecmd_comand.Parameters.Add("@strDevice", OleDbType.Char)
                    iolecmd_comand.Parameters("@strDevice").Value = astr_Device





            End Select

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            'ejecutar 
            ' cerrar comando
            'destruir conexion 



            Try
                iAdapt_comand = New OleDbDataAdapter()
                ' ioleconx_conexion.ConnectionString = istr_conx
                'iolecmd_comand = ioleconx_conexion.CreateCommand()
                iolecmd_comand.CommandText = strSQL
                iolecmd_comand.CommandType = CommandType.StoredProcedure
                iolecmd_comand.CommandTimeout = 99999
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()

                iAdapt_comand.Fill(lddt_Spresult)
                'insertar resultado 
                ''agregar resultado 
                lrow = ldtb_Result.NewRow()
                lrow("UniversalId") = lobj_item.intContainerUniversalId
                lrow("FeeId") = lddt_Spresult.Rows(0)(0)
                lrow("strType") = lobj_item.str_ServiceType
                lrow("strDocInfo") = ""
                lrow("intQuantityExcepDays") = "0"

                '' si el servicio es almacenaje buscar la columna de excecion de dias 
                If lobj_item.str_ServiceType = "ALM" Then
                    Try
                        lrow("intQuantityExcepDays") = lddt_Spresult.Rows(0)("intQuantityExcepDays")
                    Catch ex As Exception

                    End Try
                End If '' '' si el servicio es almacenaje buscar la columna de excecion de dias 

                '''

                ' lrow(0) = lddt_Spresult.Rows(0)(0)
                ldtb_Result.Rows.Add(lrow)
                llng_FeeId = 0
                Long.TryParse(lddt_Spresult.Rows(0)(0).ToString, llng_FeeId)

            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
            Finally
                ioleconx_conexion.Close()
            End Try

            'renovar tabla de resultados 
            lddt_Spresult = New DataTable("spresult")

            '' si hay que actualizar datos fiscales 
            If lint_HasToUpdateFiscalData > 0 Then
                UpdateInvoiceFiscalData(lint_InvoiceToId, lint_InvoiceTypeId, lobj_item.iobj_FiscalObj, astr_username)
                lint_HasToUpdateFiscalData = 0
            End If


            ''ver que onda con los documentos 
            Try
                lint_itemIndex = -2
                lint_intemFound = -2
                'si hay arreglo de documento
                If arrDocument.Length > 0 And lobj_item.intContainerUniversalId > 0 Then

                    'buscar el universal
                    'For lint_idx = 0 To arrDocument.Length - 1

                    '    If arrDocument(lint_idx).intUniversalId = lobj_item.intContainerUniversalId Then
                    '        lint_intemFound = lint_idx
                    '        lobj_ItemDocument = arrDocument(lint_idx)
                    '    End If

                    'Next '' ciclo busqueda

                    '' asociar cada documento a cada item 
                    For lint_idx = 0 To arrDocument.Length - 1

                        'obtener el obejto del item del documento
                        lobj_ItemDocument = arrDocument(lint_idx)

                        '' primero buscar el documento , si no existe insertarlo 
                        llng_TempDoc = SearchAndSaveDocument(lobj_ItemDocument, astr_username)

                        ''si se creo el documento
                        If llng_TempDoc > 0 Then
                            '' asociarlo al universal 

                            lobj_ItemDocument = New ClsDocument()
                            lobj_ItemDocument.intDocumentId = llng_TempDoc
                            lobj_ItemDocument.intUniversalId = lobj_item.intContainerUniversalId
                            lobj_ItemDocument.strDescription = ""
                            lobj_ItemDocument.strDocumentFolio = ""
                            lobj_ItemDocument.strDocumentType = ""

                            lddt_Spresult = New DataTable("savedoc")
                            'asocia el docuemnto a el universal
                            lddt_Spresult = SaveDocumentContainer(lobj_ItemDocument, lobj_item.intContainerUniversalId, astr_username)
                            '' asociarlo al master de almacenaje
                            lddt_Spresult = New DataTable("setdoc")
                            lddt_Spresult = SetDocumentToFee(llng_TempDoc, llng_FeeId)

                            '' respaldar el id que se guardo 
                            Try
                                ldtb_Result(ldtb_Result.Rows.Count - 1)("strDocInfo") = llng_TempDoc.ToString()
                            Catch exb As Exception
                                Dim lstr_errorbEx As String = exb.Message
                            End Try


                        End If ' If llng_TempDoc  > 0 Then                        


                    Next '' asociar cada documento a cada item 
                    ''''''''''''''''''''''''''''''''''''''''''''''''

                    '' si el universal del item de storage esta en el arreglo de documento, crear un documento con el universal , si se encontro
                    'If lint_intemFound > -1 Then

                    '    '' primero buscar el documento , si no existe insertarlo 
                    '    llng_TempDoc = SearchAndSaveDocument(lobj_ItemDocument, astr_username)

                    '    ''si se creo el documento
                    '    If llng_TempDoc > 0 Then
                    '        '' asociarlo al universal 

                    '        lobj_ItemDocument = New ClsDocument()
                    '        lobj_ItemDocument.intDocumentId = llng_TempDoc
                    '        lobj_ItemDocument.intUniversalId = lobj_item.intContainerUniversalId
                    '        lobj_ItemDocument.strDescription = ""
                    '        lobj_ItemDocument.strDocumentFolio = ""
                    '        lobj_ItemDocument.strDocumentType = ""

                    '        lddt_Spresult = New DataTable("savedoc")
                    '        'asocia el docuemnto a el universal
                    '        lddt_Spresult = SaveDocumentContainer(lobj_ItemDocument, lobj_item.intContainerUniversalId, astr_username)
                    '        '' asociarlo al master de almacenaje
                    '        lddt_Spresult = New DataTable("setdoc")
                    '        lddt_Spresult = SetDocumentToFee(llng_TempDoc, llng_FeeId)


                    '    End If ' If llng_TempDoc  > 0 Then                        

                    'End If ''lint_intemFound > 0 Then


                Else ''If arrDocument.Length > 0 and  lobj_item.intContainerUniversalId > 0 Then

                    Try
                        ldtb_Result(ldtb_Result.Rows.Count - 1)("strDocInfo") = " no hay : arrDocument.Length=" + arrDocument.Length.ToString() + "lobj_item.intContainerUniversalId=" + lobj_item.intContainerUniversalId.ToString()
                    Catch exb As Exception
                        Dim lstr_errorbEx As String = exb.Message
                    End Try

                End If ''If arrDocument.Length > 0 and  lobj_item.intContainerUniversalId > 0 Then

            Catch ex As Exception
                Dim lstr_errorEx As String = ex.Message
                lstr_errorEx = lstr_errorEx

                Try
                    ldtb_Result(ldtb_Result.Rows.Count - 1)("strDocInfo") = lstr_errorEx
                Catch exb As Exception
                    Dim lstr_errorbEx As String = exb.Message
                End Try
            End Try
            ''' fin analisis documentos 


        Next



        Return ldtb_Result


    End Function
    ''
    ''
    ''

    <WebMethod()>
    Public Function GenerateGeneralCargoStorageFeeId(ByVal astruc_FeeRequest As ClsStorage(), ByVal astr_username As String, ByVal arrDocument As ClsDocument(), ByVal astr_Device As String, ByVal adtm_dateEnd As Date) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim lddt_Spresult = New DataTable("spresult")
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim lstr_datetime As String = ""
        Dim lstr_datetimefinal As String = ""
        Dim timeFormat As String = "dd/MM/yyyy HH:mm:ss"

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lrow As DataRow
        '' variables de datos fiscales 
        Dim lstr_CompanyRFC As String = ""
        Dim lstr_FiscalAddress As String = ""
        Dim lstr_CompanyCity As String = ""
        Dim lstr_CompanyState As String = ""
        Dim lstr_CompanyZipCode As String = ""
        Dim lstr_CFDIUsageType As String = ""
        Dim lstr_PaymentMethodTypeId As String = ""
        Dim lint_PaymentForm As Integer = 0
        Dim lint_HasToUpdateFiscalData As Integer = 0
        Dim lint_RequiredBy As Integer
        Dim lint_RequiredByType As Integer
        Dim lint_InvoiceToId As Integer
        Dim lint_InvoiceTypeId As Integer
        Dim lobj_ItemDocument As ClsDocument = New ClsDocument()
        Dim lint_itemIndex As Integer
        Dim lint_intemFound As Integer
        Dim llng_TempDoc As Long
        Dim llng_FeeId As Long
        Dim lstr_DateEndParam As String

        ldtb_Result = New DataTable("User")
        lddt_Spresult = New DataTable("spresutl")
        ' agregar una columna 
        ldtb_Result.Columns.Add("intGeneralCargoUniversalId", GetType(String))
        ldtb_Result.Columns.Add("intGCInventoryItemId", GetType(String))
        ldtb_Result.Columns.Add("FeeId", GetType(String))
        ldtb_Result.Columns.Add("strType", GetType(String))
        ldtb_Result.Columns.Add("strDocInfo", GetType(String))


        'istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        'ioleconx_conexion.ConnectionString = istr_conx
        'iolecmd_comand = ioleconx_conexion.CreateCommand()

        ''' validar cantidad de registros
        If astruc_FeeRequest.Length = 0 Then
            Return dt_RetrieveErrorTable("No tiene elementos solicitados")
        End If


        '' recorres el arreglo
        For Each lobj_item As ClsStorage In astruc_FeeRequest

            'resetar conexion
            ioleconx_conexion = New OleDbConnection() '' objeto de conexion que se usara para conectar 

            'resetear comando
            iolecmd_comand = New OleDbCommand()

            'configurar comando y conexion
            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx
            iolecmd_comand = ioleconx_conexion.CreateCommand()


            Select Case lobj_item.str_ServiceType
                Case "ALM" ' almacenaje"


                    'analizar la variable objeto
                    If Not (lobj_item.iobj_FiscalObj Is Nothing) Then
                        lstr_CompanyRFC = lobj_item.iobj_FiscalObj.strCompanyFiscalIdentifier
                        lstr_FiscalAddress = lobj_item.iobj_FiscalObj.strCompanyAddress1
                        lstr_CompanyCity = lobj_item.iobj_FiscalObj.strCompanyCity
                        lstr_CompanyState = lobj_item.iobj_FiscalObj.strCompanyState
                        lstr_CompanyZipCode = lobj_item.iobj_FiscalObj.strCompanyZipCode
                        lstr_CFDIUsageType = lobj_item.iobj_FiscalObj.strCFDIUsageTypeId
                        lstr_PaymentMethodTypeId = lobj_item.iobj_FiscalObj.strPaymentMethodTypeId
                        lint_PaymentForm = lobj_item.iobj_FiscalObj.intPaymentFormTypeId
                        lint_HasToUpdateFiscalData = 1
                        lint_RequiredBy = lobj_item.int_RequiredBy
                        lint_RequiredByType = lobj_item.int_RequiredByType
                        lint_InvoiceToId = lobj_item.int_Invoiceid
                        lint_InvoiceTypeId = lobj_item.int_InvoiceTypeId
                    End If

                    '

                    iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = lobj_item.intGeneralCargoUniversalId

                    iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intGCItemId").Value = lobj_item.intGCInventoryItemId

                    iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intRequiredBy").Value = lint_RequiredBy

                    iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intRequiredByType").Value = lint_RequiredByType

                    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
                    iolecmd_comand.Parameters("@strUsername").Value = astr_username

                    iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intmode").Value = 1

                    iolecmd_comand.Parameters.Add("@intInvoicetoId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intInvoicetoId").Value = lint_InvoiceToId

                    iolecmd_comand.Parameters.Add("@intInvoceTypeId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intInvoceTypeId").Value = lint_InvoiceTypeId



                    '' pasar la informacion fiscal

                    iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@intPaymentForm", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intPaymentForm").Value = lint_PaymentForm

                    iolecmd_comand.Parameters.Add("@strCFDIUsageTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCFDIUsageTypeId").Value = lstr_CFDIUsageType

                    iolecmd_comand.Parameters.Add("@blnIsDirectCredit", OleDbType.Integer)
                    iolecmd_comand.Parameters("@blnIsDirectCredit").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalAddress", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalAddress").Value = lstr_FiscalAddress

                    iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyCity").Value = lstr_CompanyCity

                    iolecmd_comand.Parameters.Add("@strCompanyState", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyState").Value = lstr_CompanyState

                    iolecmd_comand.Parameters.Add("@strCompanyZipCode", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyZipCode").Value = lstr_CompanyZipCode

                    iolecmd_comand.Parameters.Add("@strCompanyRFC", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyRFC").Value = lstr_CompanyRFC

                    iolecmd_comand.Parameters.Add("@strDevice", OleDbType.Char)
                    iolecmd_comand.Parameters("@strDevice").Value = astr_Device

                    iolecmd_comand.Parameters.Add("@intFiscalMovementId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intFiscalMovementId").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalMovement", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalMovement").Value = lobj_item.strFiscalMovement

                    'lstr_DateEndParam

                    'obtener parametros de fecha final
                    lstr_DateEndParam = of_ConvertDateToStringGeneralFormat(adtm_dateEnd)

                    iolecmd_comand.Parameters.Add("@adtmCalcEndDate", OleDbType.Char)
                    iolecmd_comand.Parameters("@adtmCalcEndDate").Value = lstr_DateEndParam


                    ''''''
                    iolecmd_comand.Parameters.Add("@strSatusToSave", OleDbType.Char)
                    iolecmd_comand.Parameters("@strSatusToSave").Value = lobj_item.strSaveStatus

                    '' nombre del sp 
                    strSQL = "spGenerateGCItemStorageFee"

                Case "MUELLAJE" ' muellaje


                    'analizar la variable objeto
                    If Not (lobj_item.iobj_FiscalObj Is Nothing) Then
                        lstr_CompanyRFC = lobj_item.iobj_FiscalObj.strCompanyFiscalIdentifier
                        lstr_FiscalAddress = lobj_item.iobj_FiscalObj.strCompanyAddress1
                        lstr_CompanyCity = lobj_item.iobj_FiscalObj.strCompanyCity
                        lstr_CompanyState = lobj_item.iobj_FiscalObj.strCompanyState
                        lstr_CompanyZipCode = lobj_item.iobj_FiscalObj.strCompanyZipCode
                        lstr_CFDIUsageType = lobj_item.iobj_FiscalObj.strCFDIUsageTypeId
                        lstr_PaymentMethodTypeId = lobj_item.iobj_FiscalObj.strPaymentMethodTypeId
                        lint_PaymentForm = lobj_item.iobj_FiscalObj.intPaymentFormTypeId
                        lint_HasToUpdateFiscalData = 1
                        lint_InvoiceToId = lobj_item.int_InvoiceTypeId
                        lint_InvoiceTypeId = lobj_item.int_InvoiceTypeId
                        lint_RequiredBy = lobj_item.int_RequiredBy
                        lint_RequiredByType = lobj_item.int_RequiredByType
                    End If


                    iolecmd_comand.Parameters.Add("@intGCargoUniversalId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intGCargoUniversalId").Value = lobj_item.intGeneralCargoUniversalId

                    iolecmd_comand.Parameters.Add("@intGCItem", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intGCItem").Value = lobj_item.intGCInventoryItemId

                    iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intRequiredBy").Value = lint_RequiredBy

                    iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intRequiredByType").Value = lint_RequiredByType

                    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
                    iolecmd_comand.Parameters("@strUsername").Value = astr_username

                    iolecmd_comand.Parameters.Add("@intmode", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intmode").Value = 1

                    iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intStorageFeeId").Value = lobj_item.intContainerStorageFeeId

                    iolecmd_comand.Parameters.Add("@strFiscalPetitionName ", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalPetitionName ").Value = lobj_item.strFiscalPetitionName

                    iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
                    iolecmd_comand.Parameters("@strBLName").Value = lobj_item.strBLName

                    'fecha
                    lstr_datetime = of_ConvertDateToStringGeneralFormat(lobj_item.dtmFiscalPetitionDate)

                    iolecmd_comand.Parameters.Add("@dtmFiscalPetitionDate", OleDbType.Char)
                    iolecmd_comand.Parameters("@dtmFiscalPetitionDate").Value = lstr_datetime

                    '' PESO
                    iolecmd_comand.Parameters.Add("@decFiscalPetitionWeight", OleDbType.Decimal)
                    iolecmd_comand.Parameters("@decFiscalPetitionWeight").Value = lobj_item.decFiscalPetitionWeight
                    '''''''''

                    '' nombre del sp 
                    strSQL = "spCRUDGCargoDockingPay"



                    iolecmd_comand.Parameters.Add("@intInvoicetoId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intInvoicetoId").Value = lint_InvoiceToId

                    iolecmd_comand.Parameters.Add("@intInvoceTypeId", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intInvoceTypeId").Value = lint_InvoiceTypeId

                    iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@intPaymentForm", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intPaymentForm").Value = lint_PaymentForm

                    iolecmd_comand.Parameters.Add("@strCFDIUsageTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCFDIUsageTypeId").Value = lstr_CFDIUsageType

                    iolecmd_comand.Parameters.Add("@blnIsDirectCredit", OleDbType.Integer)
                    iolecmd_comand.Parameters("@blnIsDirectCredit").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalAddress", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalAddress").Value = lstr_FiscalAddress

                    iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyCity").Value = lstr_CompanyCity

                    iolecmd_comand.Parameters.Add("@strCompanyState", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyState").Value = lstr_CompanyState

                    iolecmd_comand.Parameters.Add("@strCompanyZipCode", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyZipCode").Value = lstr_CompanyZipCode

                    iolecmd_comand.Parameters.Add("@strCompanyRFC", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyRFC").Value = lstr_CompanyRFC


                    'iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    'iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@strDevice", OleDbType.Char)
                    iolecmd_comand.Parameters("@strDevice").Value = astr_Device






                Case "PBIP" 'pbip 

                    'analizar la variable objeto
                    If Not (lobj_item.iobj_FiscalObj Is Nothing) Then
                        lstr_CompanyRFC = lobj_item.iobj_FiscalObj.strCompanyFiscalIdentifier
                        lstr_FiscalAddress = lobj_item.iobj_FiscalObj.strCompanyAddress1
                        lstr_CompanyCity = lobj_item.iobj_FiscalObj.strCompanyCity
                        lstr_CompanyState = lobj_item.iobj_FiscalObj.strCompanyState
                        lstr_CompanyZipCode = lobj_item.iobj_FiscalObj.strCompanyZipCode
                        lstr_CFDIUsageType = lobj_item.iobj_FiscalObj.strCFDIUsageTypeId
                        lstr_PaymentMethodTypeId = lobj_item.iobj_FiscalObj.strPaymentMethodTypeId
                        lint_PaymentForm = lobj_item.iobj_FiscalObj.intPaymentFormTypeId
                        lint_HasToUpdateFiscalData = 1
                        lint_InvoiceToId = lobj_item.int_InvoiceTypeId
                        lint_InvoiceTypeId = lobj_item.int_InvoiceTypeId
                        lint_RequiredBy = lobj_item.int_RequiredBy
                        lint_RequiredByType = lobj_item.int_RequiredByType
                    End If


                    '' nombre del sp 
                    strSQL = "spCRUDGCargoPBIP"



                    iolecmd_comand.Parameters.Add("@intGCargoUniversalId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intGCargoUniversalId").Value = lobj_item.intGeneralCargoUniversalId

                    iolecmd_comand.Parameters.Add("@intGCItem", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intGCItem").Value = lobj_item.intGCInventoryItemId

                    iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intRequiredBy").Value = lint_RequiredBy

                    iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intRequiredByType").Value = lint_RequiredByType

                    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
                    iolecmd_comand.Parameters("@strUsername").Value = astr_username

                    iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intmode").Value = 1

                    iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intStorageFeeId").Value = 0

                    iolecmd_comand.Parameters.Add("@strFiscalPetitionName", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalPetitionName").Value = ""

                    iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
                    iolecmd_comand.Parameters("@strBLName").Value = ""

                    iolecmd_comand.Parameters.Add("@intInvoicetoId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intInvoicetoId").Value = lint_InvoiceToId

                    iolecmd_comand.Parameters.Add("@intInvoceTypeId", OleDbType.Integer)
                    iolecmd_comand.Parameters("@intInvoceTypeId").Value = lint_InvoiceTypeId



                    ''

                    iolecmd_comand.Parameters.Add("@strPaymentMethodTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strPaymentMethodTypeId").Value = lstr_PaymentMethodTypeId

                    iolecmd_comand.Parameters.Add("@intPaymentForm", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@intPaymentForm").Value = lint_PaymentForm

                    iolecmd_comand.Parameters.Add("@strCFDIUsageTypeId", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCFDIUsageTypeId").Value = lstr_CFDIUsageType

                    iolecmd_comand.Parameters.Add("@blnIsDirectCredit", OleDbType.Numeric)
                    iolecmd_comand.Parameters("@blnIsDirectCredit").Value = 0


                    iolecmd_comand.Parameters.Add("@strFiscalAddress", OleDbType.Char)
                    iolecmd_comand.Parameters("@strFiscalAddress").Value = lstr_FiscalAddress

                    iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyCity").Value = lstr_CompanyCity

                    iolecmd_comand.Parameters.Add("@strCompanyState", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyState").Value = lstr_CompanyState

                    iolecmd_comand.Parameters.Add("@strCompanyZipCode", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyZipCode").Value = lstr_CompanyZipCode

                    iolecmd_comand.Parameters.Add("@strCompanyRFC", OleDbType.Char)
                    iolecmd_comand.Parameters("@strCompanyRFC").Value = lstr_CompanyRFC

                    iolecmd_comand.Parameters.Add("@strDevice", OleDbType.Char)
                    iolecmd_comand.Parameters("@strDevice").Value = astr_Device





            End Select

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            'ejecutar 
            ' cerrar comando
            'destruir conexion 



            Try
                iAdapt_comand = New OleDbDataAdapter()
                ' ioleconx_conexion.ConnectionString = istr_conx
                'iolecmd_comand = ioleconx_conexion.CreateCommand()
                iolecmd_comand.CommandText = strSQL
                iolecmd_comand.CommandType = CommandType.StoredProcedure
                iolecmd_comand.CommandTimeout = 99999
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()

                iAdapt_comand.Fill(lddt_Spresult)
                'insertar resultado 
                ''agregar resultado 
                lrow = ldtb_Result.NewRow()
                lrow("intGeneralCargoUniversalId") = lobj_item.intGeneralCargoUniversalId
                lrow("intGCInventoryItemId") = lobj_item.intGCInventoryItemId
                lrow("FeeId") = lddt_Spresult.Rows(0)(0)
                lrow("strType") = lobj_item.str_ServiceType
                lrow("strDocInfo") = ""

                ' lrow(0) = lddt_Spresult.Rows(0)(0)
                ldtb_Result.Rows.Add(lrow)
                llng_FeeId = 0
                Long.TryParse(lddt_Spresult.Rows(0)(0).ToString, llng_FeeId)

            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
            Finally
                ioleconx_conexion.Close()
            End Try

            'renovar tabla de resultados 
            lddt_Spresult = New DataTable("spresult")

            '' si hay que actualizar datos fiscales 
            If lint_HasToUpdateFiscalData > 0 Then
                UpdateInvoiceFiscalData(lint_InvoiceToId, lint_InvoiceTypeId, lobj_item.iobj_FiscalObj, astr_username)
                lint_HasToUpdateFiscalData = 0
            End If


            ''ver que onda con los documentos 
            Try
                lint_itemIndex = -2
                lint_intemFound = -2
                'si hay arreglo de documento
                If arrDocument.Length > 0 And lobj_item.intGeneralCargoUniversalId > 0 Then



                    '' asociar cada documento a cada item 
                    For lint_idx = 0 To arrDocument.Length - 1

                        'obtener el obejto del item del documento
                        lobj_ItemDocument = arrDocument(lint_idx)

                        '' primero buscar el documento , si no existe insertarlo 
                        llng_TempDoc = SearchAndSaveDocument(lobj_ItemDocument, astr_username)

                        ''si se creo el documento
                        If llng_TempDoc > 0 Then
                            '' asociarlo al universal 

                            lobj_ItemDocument = New ClsDocument()
                            lobj_ItemDocument.intDocumentId = llng_TempDoc
                            lobj_ItemDocument.intGeneralCargoUniversalId = lobj_item.intGeneralCargoUniversalId
                            lobj_ItemDocument.intGCInventoryItemId = lobj_item.intGCInventoryItemId
                            lobj_ItemDocument.strDescription = ""
                            lobj_ItemDocument.strDocumentFolio = ""
                            lobj_ItemDocument.strDocumentType = ""

                            lddt_Spresult = New DataTable("savedoc")
                            'asocia el docuemnto carga general
                            lddt_Spresult = SaveDocumentGeneralCargo(lobj_ItemDocument, astr_username)

                            '' asociarlo al master de almacenaje
                            lddt_Spresult = New DataTable("setdoc")
                            lddt_Spresult = SetDocumentToFeeGC(llng_TempDoc, llng_FeeId, astr_username)

                            '' respaldar el id que se guardo 
                            Try
                                ldtb_Result(ldtb_Result.Rows.Count - 1)("strDocInfo") = llng_TempDoc.ToString()
                            Catch exb As Exception
                                Dim lstr_errorbEx As String = exb.Message
                            End Try


                        End If ' If llng_TempDoc  > 0 Then                        


                    Next '' asociar cada documento a cada item 
                    ''''''''''''''''''''''''''''''''''''''''''''''''



                Else ''If arrDocument.Length > 0 and  lobj_item.intContainerUniversalId > 0 Then

                    Try
                        ldtb_Result(ldtb_Result.Rows.Count - 1)("strDocInfo") = " no hay : arrDocument.Length=" + arrDocument.Length.ToString() + "lobj_item.intContainerUniversalId=" + lobj_item.intContainerUniversalId.ToString()
                    Catch exb As Exception
                        Dim lstr_errorbEx As String = exb.Message
                    End Try

                End If ''If arrDocument.Length > 0 and  lobj_item.intContainerUniversalId > 0 Then

            Catch ex As Exception
                Dim lstr_errorEx As String = ex.Message
                lstr_errorEx = lstr_errorEx

                Try
                    ldtb_Result(ldtb_Result.Rows.Count - 1)("strDocInfo") = lstr_errorEx
                Catch exb As Exception
                    Dim lstr_errorbEx As String = exb.Message
                End Try
            End Try
            ''' fin analisis documentos 


        Next



        Return ldtb_Result


    End Function

    '''


    '    <WebMethod()> _
    'Public Function GeneratePBIP(ByVal alng_FeeId As Long, ByVal astr_StatusName As String, ByVal astr_UserName As String) As DataTable

    '    End Function

    <WebMethod()>
    Public Function UpdateFeeStatus(ByVal alng_FeeId As Long, ByVal astr_StatusName As String, ByVal astr_UserName As String) As String

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_ReturnValueTable As DataTable


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultVisit"

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0

        'limpiar cadena sql
        lstr_SQL = ""

        ''''---

        'agregar parametros y valores
        '' nombre del estatus 
        iolecmd_comand.Parameters.Add("@strStatusname", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatusname").Value = astr_StatusName
        '' id de calculo
        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_FeeId
        '' variable complement0 x
        iolecmd_comand.Parameters.Add("@strCalcName", OleDbType.Char)
        iolecmd_comand.Parameters("@strCalcName").Value = ""
        '' tipo de calculo
        iolecmd_comand.Parameters.Add("@intCalType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCalType").Value = 0
        '' modo
        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 0
        '' usuario
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_UserName



        'definir la cadena sql
        lstr_SQL = "spUpdateCalcStatus"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            adapter.Fill(ldt_ReturnValueTable)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''

    End Function
    ''

    <WebMethod()>
    Public Function UpdateDockingStatus(ByVal alng_FeeId As Long, ByVal astr_StatusName As String, ByVal astr_UserName As String) As String

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultVisit"

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0

        'limpiar cadena sql
        lstr_SQL = ""

        ''''---

        'agregar parametros y valores
        '' nombre del estatus 
        iolecmd_comand.Parameters.Add("@strStatusname", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatusname").Value = astr_StatusName
        '' id de calculo
        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_FeeId
        '' variable complement0 x
        iolecmd_comand.Parameters.Add("@strCalcName", OleDbType.Char)
        iolecmd_comand.Parameters("@strCalcName").Value = ""
        '' tipo de calculo
        iolecmd_comand.Parameters.Add("@intCalType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCalType").Value = 0
        '' modo
        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 0
        '' usuario
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_UserName

        'definir la cadena sql
        lstr_SQL = "spUpdateCalcStatus"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            adapter.Fill(ldt_ReturnValueTable)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''

    End Function
    '''

    <WebMethod()>
    Public Function UpdatePBIPStatus(ByVal alng_FeeId As Long, ByVal astr_StatusName As String, ByVal astr_UserName As String) As String

        ''''''''''''''''''''''''''
        '-----------------------------

        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        Dim lstr_SQL As String
        Dim lstr_Message As String = ""
        Dim lint_itemscount As Integer = 0

        Dim ldt_ReturnValueTable As DataTable
        Dim ldr_ReturnTickeRow As DataRow


        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString

        ldt_ReturnValueTable = New DataTable()
        ldt_ReturnValueTable.TableName = "TableResultVisit"

        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        iolecmd_comand.CommandTimeout = 0

        'limpiar cadena sql
        lstr_SQL = ""

        ''''---

        'agregar parametros y valores
        '' nombre del estatus 
        iolecmd_comand.Parameters.Add("@strStatusname", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatusname").Value = astr_StatusName
        '' id de calculo
        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alng_FeeId
        '' variable complement0 x
        iolecmd_comand.Parameters.Add("@strCalcName", OleDbType.Char)
        iolecmd_comand.Parameters("@strCalcName").Value = ""
        '' tipo de calculo
        iolecmd_comand.Parameters.Add("@intCalType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCalType").Value = 0
        '' modo
        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 0
        '' usuario
        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_UserName

        'definir la cadena sql
        lstr_SQL = "spUpdateCalcStatus"

        'definir que tipo de comando se va a ejecutar
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandText = lstr_SQL

        ''ejecutar 
        Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

        Try

            ''conectar
            iolecmd_comand.Connection.Open()
            'iolecmd_comand.ExecuteNonQuery()
            adapter.SelectCommand.CommandTimeout = of_getMaxTimeout()
            adapter.Fill(ldt_ReturnValueTable)
            ''desconectar
        Catch ex As Exception
            lstr_Message = ObtenerError(ex.Message, 9999)
            If lstr_Message.Length > 0 Then
                Return lstr_Message
            Else
                Return ex.Message
            End If
        Finally
            iolecmd_comand.Connection.Close()
            iolecmd_comand.Connection.Dispose()
            'ioleconx_conexion.close()
        End Try

        Return ""

        '''''''''''''''''''''''''''''''''

    End Function

    <WebMethod()>
    Public Function UpdateInvoiceFiscalData(ByVal aint_InvoiceToId As Integer, ByVal aintInvoiceTypeId As Integer, ByVal aobj_fisc As ClsFiscalData, ByVal astr_Username As String) As String

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"



        '' se realizara un correcion de 
        aobj_fisc.strCompanyAddress1 = of_convertoasccistring(aobj_fisc.strCompanyAddress1)
        aobj_fisc.strCompanyCity = of_convertoasccistring(aobj_fisc.strCompanyCity)
        aobj_fisc.strCompanyState = of_convertoasccistring(aobj_fisc.strCompanyState)
        aobj_fisc.strCompanyZipCode = of_convertoasccistring(aobj_fisc.strCompanyZipCode)
        aobj_fisc.strCompanyFiscalIdentifier = of_convertoasccistring(aobj_fisc.strCompanyFiscalIdentifier)

        aobj_fisc.strCompanyAddress1 = CorrectStringFromASCII(aobj_fisc.strCompanyAddress1)
        aobj_fisc.strCompanyCity = CorrectStringFromASCII(aobj_fisc.strCompanyCity)
        aobj_fisc.strCompanyState = CorrectStringFromASCII(aobj_fisc.strCompanyState)


        Dim strSQL As String
        'Dim strcontainerid As String
        strSQL = "spUpdateInvoiceFiscalInfo"

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure


        iolecmd_comand.Parameters.Add("@int_CompanyEntityId", OleDbType.Integer)
        iolecmd_comand.Parameters("@int_CompanyEntityId").Value = aint_InvoiceToId

        iolecmd_comand.Parameters.Add("@int_CompanyEntityIdType", OleDbType.Integer)
        iolecmd_comand.Parameters("@int_CompanyEntityIdType").Value = aintInvoiceTypeId

        iolecmd_comand.Parameters.Add("@str_CompanyAdress", OleDbType.Char)
        iolecmd_comand.Parameters("@str_CompanyAdress").Value = aobj_fisc.strCompanyAddress1

        iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
        iolecmd_comand.Parameters("@strCompanyCity").Value = aobj_fisc.strCompanyCity

        iolecmd_comand.Parameters.Add("@str_CompanyState", OleDbType.Char)
        iolecmd_comand.Parameters("@str_CompanyState").Value = aobj_fisc.strCompanyState

        iolecmd_comand.Parameters.Add("@astr_ZipCode", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_ZipCode").Value = aobj_fisc.strCompanyZipCode

        iolecmd_comand.Parameters.Add("@astr_RFC", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_RFC").Value = aobj_fisc.strCompanyFiscalIdentifier

        iolecmd_comand.Parameters.Add("@aint_PayForm", OleDbType.Integer)
        iolecmd_comand.Parameters("@aint_PayForm").Value = aobj_fisc.intPaymentFormTypeId

        iolecmd_comand.Parameters.Add("@astr_PayMethod", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_PayMethod").Value = aobj_fisc.strPaymentMethodTypeId

        iolecmd_comand.Parameters.Add("@astr_CFDIUsage", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_CFDIUsage").Value = aobj_fisc.strCFDIUsageTypeId

        iolecmd_comand.Parameters.Add("@astr_Username", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_Username").Value = astr_Username

        iolecmd_comand.Parameters.Add("@int_StorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@int_StorageFeeId").Value = 0

        iolecmd_comand.Parameters.Add("@strExtraService", OleDbType.Char)
        iolecmd_comand.Parameters("@strExtraService").Value = ""




        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            Return strError
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing


        Return ""
    End Function
    ''''

    <WebMethod()>
    Public Function UpdateStorageFeeContFiscalData(ByVal aint_ContainerStorageFeeId As Integer, ByVal aint_InvoiceToId As Integer, ByVal aintInvoiceTypeId As Integer, ByVal aobj_fisc As ClsFiscalData, ByVal astr_Username As String) As String

        Dim idt_result As DataTable = New DataTable ' Tabla con el query de resultados 
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
        Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 
        Dim istr_conx As String '' cadena de conexion

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()
        idt_result.TableName = "TrearDatos"



        '' se realizara un correcion de 
        aobj_fisc.strCompanyAddress1 = of_convertoasccistring(aobj_fisc.strCompanyAddress1)
        aobj_fisc.strCompanyCity = of_convertoasccistring(aobj_fisc.strCompanyCity)
        aobj_fisc.strCompanyState = of_convertoasccistring(aobj_fisc.strCompanyState)
        aobj_fisc.strCompanyZipCode = of_convertoasccistring(aobj_fisc.strCompanyZipCode)
        aobj_fisc.strCompanyFiscalIdentifier = of_convertoasccistring(aobj_fisc.strCompanyFiscalIdentifier)

        aobj_fisc.strCompanyAddress1 = CorrectStringFromASCII(aobj_fisc.strCompanyAddress1)
        aobj_fisc.strCompanyCity = CorrectStringFromASCII(aobj_fisc.strCompanyCity)
        aobj_fisc.strCompanyState = CorrectStringFromASCII(aobj_fisc.strCompanyState)


        Dim strSQL As String
        'Dim strcontainerid As String
        strSQL = "spUpdateInvoiceFiscalInfo"

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure


        iolecmd_comand.Parameters.Add("@int_CompanyEntityId", OleDbType.Integer)
        iolecmd_comand.Parameters("@int_CompanyEntityId").Value = aint_InvoiceToId

        iolecmd_comand.Parameters.Add("@int_CompanyEntityIdType", OleDbType.Integer)
        iolecmd_comand.Parameters("@int_CompanyEntityIdType").Value = aintInvoiceTypeId

        iolecmd_comand.Parameters.Add("@str_CompanyAdress", OleDbType.Char)
        iolecmd_comand.Parameters("@str_CompanyAdress").Value = aobj_fisc.strCompanyAddress1

        iolecmd_comand.Parameters.Add("@strCompanyCity", OleDbType.Char)
        iolecmd_comand.Parameters("@strCompanyCity").Value = aobj_fisc.strCompanyCity

        iolecmd_comand.Parameters.Add("@str_CompanyState", OleDbType.Char)
        iolecmd_comand.Parameters("@str_CompanyState").Value = aobj_fisc.strCompanyState

        iolecmd_comand.Parameters.Add("@astr_ZipCode", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_ZipCode").Value = aobj_fisc.strCompanyZipCode

        iolecmd_comand.Parameters.Add("@astr_RFC", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_RFC").Value = aobj_fisc.strCompanyFiscalIdentifier

        iolecmd_comand.Parameters.Add("@aint_PayForm", OleDbType.Integer)
        iolecmd_comand.Parameters("@aint_PayForm").Value = aobj_fisc.intPaymentFormTypeId

        iolecmd_comand.Parameters.Add("@astr_PayMethod", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_PayMethod").Value = aobj_fisc.strPaymentMethodTypeId

        iolecmd_comand.Parameters.Add("@astr_CFDIUsage", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_CFDIUsage").Value = aobj_fisc.strCFDIUsageTypeId

        iolecmd_comand.Parameters.Add("@astr_Username", OleDbType.Char)
        iolecmd_comand.Parameters("@astr_Username").Value = astr_Username

        iolecmd_comand.Parameters.Add("@int_StorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@int_StorageFeeId").Value = aint_ContainerStorageFeeId

        iolecmd_comand.Parameters.Add("@strExtraService", OleDbType.Char)
        iolecmd_comand.Parameters("@strExtraService").Value = ""




        iAdapt_comand.SelectCommand = iolecmd_comand

        Try
            iolecmd_comand.Connection.Open()
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(idt_result)
        Catch ex As Exception
            Dim strError As String
            strError = ObtenerError(ex.Message, 99999)
            Return strError
        Finally
            iolecmd_comand.Connection.Close()
            iAdapt_comand.SelectCommand.Connection.Close()
            ioleconx_conexion.Close()

            iolecmd_comand.Connection.Dispose()
            iAdapt_comand.SelectCommand.Connection.Dispose()
            ioleconx_conexion.Dispose()

        End Try


        iAdapt_comand = Nothing
        iolecmd_comand = Nothing
        ioleconx_conexion = Nothing


        Return ""
    End Function

    '''''''
    ''''-----
    <WebMethod()>
    Public Function GetContStorageReportByFeeId(ByVal alng_StorageFeeId As Integer) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")
        strSQL = "spRptContainerStoFeeById"

        iolecmd_comand.Parameters.Add("intStoFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("intStoFeeId").Value = alng_StorageFeeId

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function GetStorageContReportMasterByUniv(ByVal alng_ContainerUniversalId As Long) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")
        strSQL = "spRptContainerStoFeeModes"

        iolecmd_comand.Parameters.Add("intStoFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("intStoFeeId").Value = 0

        iolecmd_comand.Parameters.Add("intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("intContainerUniversalId").Value = alng_ContainerUniversalId

        iolecmd_comand.Parameters.Add("strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("strContainerId").Value = ""


        iolecmd_comand.Parameters.Add("intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("intMode").Value = 1

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    ''
    <WebMethod()>
    Public Function GetStorageGCargoReportMasterByUniv(ByVal alng_GeneralCargoUniv As Long, ByVal aint_GCItem As Integer) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")
        strSQL = "spRptGCargoStoFeeModes"

        iolecmd_comand.Parameters.Add("intStoFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("intStoFeeId").Value = 0

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = alng_GeneralCargoUniv

        iolecmd_comand.Parameters.Add("@intGCInventoryItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCInventoryItemId").Value = aint_GCItem

        iolecmd_comand.Parameters.Add("intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("intMode").Value = 1

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function


    ''
    <WebMethod()>
    Public Function GetStorageContReportDetail(ByVal alng_StorageFeeid As Long) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")
        strSQL = "spRptContainerStoFeeModes"

        iolecmd_comand.Parameters.Add("intStoFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("intStoFeeId").Value = alng_StorageFeeid

        iolecmd_comand.Parameters.Add("intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("strContainerId").Value = ""


        iolecmd_comand.Parameters.Add("intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("intMode").Value = 2

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    '''
    <WebMethod()>
    Public Function GetStorageGCargoReportDetail(ByVal alng_StorageFeeid As Long) As DataTable
        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")
        strSQL = "spRptGCargoStoFeeModes"

        iolecmd_comand.Parameters.Add("@intStoFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStoFeeId").Value = alng_StorageFeeid

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intGCInventoryItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCInventoryItemId").Value = 0


        iolecmd_comand.Parameters.Add("intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("intMode").Value = 2

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    ''''''
    <WebMethod()>
    Public Function SaveDocumentContainer(ByVal aobj_Document As ClsDocument, ByVal alng_UniversalId As Long, ByVal astr_username As String) As DataTable

        Dim ldtb_Result As DataTable = New DataTable() ' la tabla que obtiene el resultado
        Dim ldtb_ResultB As DataTable
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer
        Dim lint_hasimage As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        'incia banderas
        lint_recentsave = 0
        lint_hasimage = 0

        '' si no tiene id de documento, generarlo
        If aobj_Document.intDocumentId = 0 Then

            strSQL = "spCRUDDocumentFile"
            iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intDocumentId").Value = 0

            iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
            ''iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio.ToUpper()
            iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio

            iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentType").Value = aobj_Document.strDocumentType.ToUpper()

            iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intUniversalId").Value = alng_UniversalId

            iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentDescription").Value = aobj_Document.strDescription

            iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
            iolecmd_comand.Parameters("@intmode").Value = 1

            iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
            iolecmd_comand.Parameters("@astrUsername").Value = astr_username

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            llng_Document = 0

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_Result)

                '' ver si el resutaldo es un renglon y es enterio
                '' hacer la conversion directa 
                llng_Document = ldtb_Result(0)(0)
                aobj_Document.intDocumentId = llng_Document
                lint_recentsave = 1
            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
                Return dt_RetrieveErrorTable(ex.Message)
            Finally
                ioleconx_conexion.Close()
            End Try


        End If ' If aobj_Document.intDocumentId = 0 Then

        '' checar si tiene imagen 
        Try
            If aobj_Document.iobj_image IsNot Nothing Then
                If aobj_Document.iobj_image.Length > 1 And aobj_Document.iobj_image.LongLength > 1 Then
                    lint_hasimage = 1
                End If
            End If
        Catch ex As Exception

        End Try

        '' si se obtuvo numero de documento , actualizar imagen
        If llng_Document > 0 And lint_hasimage = 1 Then

            ''' actualizar la imagen  del archivo 

            iAdapt_comand = New OleDbDataAdapter()
            iolecmd_comand = New OleDbCommand()
            ioleconx_conexion = New OleDbConnection() '' objeto de conexion que se usara para conectar 

            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx

            iolecmd_comand = ioleconx_conexion.CreateCommand()

            ldtb_ResultB = New DataTable("FeeData")

            strSQL = " UPDATE  tblclsDocument "
            strSQL = strSQL + " SET tblclsDocument.imgDocumentImageFile =? "
            strSQL = strSQL + " WHERE intDocumentId = ? "

            ' OleDbType.LongVarBinary()
            ' OleDbType.Binary()}

            iolecmd_comand.Parameters.Add("@imgdata", OleDbType.LongVarBinary)
            iolecmd_comand.Parameters("@imgdata").Value = aobj_Document.iobj_image

            iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intDocumentId").Value = aobj_Document.intDocumentId

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.Text
            iolecmd_comand.CommandTimeout = 99999

            llng_Document = 0

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_ResultB)

            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
                Return dt_RetrieveErrorTable(ex.Message)
            Finally
                ioleconx_conexion.Close()
            End Try

        End If ''If llng_Document = 0 Then

        '' If llng_Document > 0 Then


        '' si hay universal asociarlo a un universal
        If alng_UniversalId > 0 And (llng_Document > 0 Or aobj_Document.intDocumentId > 0) Then

            ''' actualizar la imagen  del archivo 
            ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
            iAdapt_comand = New OleDbDataAdapter()
            iolecmd_comand = New OleDbCommand()
            ioleconx_conexion = New OleDbConnection() '' objeto de conexion que se usara para conectar 


            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx

            iolecmd_comand = ioleconx_conexion.CreateCommand()

            ldtb_Result = New DataTable("FeeData")

            'condicion de valores para ejecutarse 
            If aobj_Document.intDocumentId > 0 Then
                llng_Document = aobj_Document.intDocumentId
            End If


            strSQL = "spCRUDDocumentFile"

            iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intDocumentId").Value = llng_Document

            iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio

            iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentType").Value = aobj_Document.strDocumentType

            iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intUniversalId").Value = alng_UniversalId

            iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentDescription").Value = aobj_Document.strDescription

            iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
            iolecmd_comand.Parameters("@intmode").Value = 3

            iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
            iolecmd_comand.Parameters("@astrUsername").Value = astr_username

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            llng_Document = 0

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_Result)

            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
                Return dt_RetrieveErrorTable(ex.Message)
            Finally
                ioleconx_conexion.Close()
            End Try

        End If ''If alng_UniversalId > 0 And llng_Document > 0  Then


        Return ldtb_Result

    End Function
    ''
    ''''''
    <WebMethod()>
    Public Function SaveDocumentGeneralCargo(ByVal aobj_Document As ClsDocument, ByVal astr_username As String) As DataTable

        Dim ldtb_Result As DataTable = New DataTable() ' la tabla que obtiene el resultado
        Dim ldtb_ResultB As DataTable
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer
        Dim lint_hasimage As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        'incia banderas
        lint_recentsave = 0
        lint_hasimage = 0

        '' si no tiene id de documento, generarlo
        If aobj_Document.intDocumentId = 0 Then

            strSQL = "spCRUDGCargoDocumentFile"

            iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intDocumentId").Value = 0

            iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
            ''iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio.ToUpper()
            iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio

            iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentType").Value = aobj_Document.strDocumentType.ToUpper()

            iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = aobj_Document.intGeneralCargoUniversalId

            iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intGCItemId").Value = aobj_Document.intGCInventoryItemId

            iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentDescription").Value = aobj_Document.strDescription

            iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
            iolecmd_comand.Parameters("@intmode").Value = 1

            iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
            iolecmd_comand.Parameters("@astrUsername").Value = astr_username

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            llng_Document = 0

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_Result)

                '' ver si el resutaldo es un renglon y es enterio
                '' hacer la conversion directa 
                llng_Document = ldtb_Result(0)(0)
                aobj_Document.intDocumentId = llng_Document
                lint_recentsave = 1
            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
                Return dt_RetrieveErrorTable(ex.Message)
            Finally
                ioleconx_conexion.Close()
            End Try


        End If ' If aobj_Document.intDocumentId = 0 Then

        '' checar si tiene imagen 
        Try
            If aobj_Document.iobj_image IsNot Nothing Then
                If aobj_Document.iobj_image.Length > 1 And aobj_Document.iobj_image.LongLength > 1 Then
                    lint_hasimage = 1
                End If
            End If
        Catch ex As Exception

        End Try

        '' si se obtuvo numero de documento , actualizar imagen
        If llng_Document > 0 And lint_hasimage = 1 Then

            ''' actualizar la imagen  del archivo 

            iAdapt_comand = New OleDbDataAdapter()
            iolecmd_comand = New OleDbCommand()
            ioleconx_conexion = New OleDbConnection() '' objeto de conexion que se usara para conectar 

            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx

            iolecmd_comand = ioleconx_conexion.CreateCommand()

            ldtb_ResultB = New DataTable("FeeData")

            strSQL = " UPDATE  tblclsDocument "
            strSQL = strSQL + " SET tblclsDocument.imgDocumentImageFile =? "
            strSQL = strSQL + " WHERE intDocumentId = ? "

            ' OleDbType.LongVarBinary()
            ' OleDbType.Binary()}

            iolecmd_comand.Parameters.Add("@imgdata", OleDbType.LongVarBinary)
            iolecmd_comand.Parameters("@imgdata").Value = aobj_Document.iobj_image

            iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intDocumentId").Value = aobj_Document.intDocumentId

            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.Text
            iolecmd_comand.CommandTimeout = 99999

            llng_Document = 0

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_ResultB)

            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
                Return dt_RetrieveErrorTable(ex.Message)
            Finally
                ioleconx_conexion.Close()
            End Try

        End If ''If llng_Document = 0 Then

        '' If llng_Document > 0 Then


        '' si hay universal asociarlo a un universal
        If aobj_Document.intGeneralCargoUniversalId > 0 And (llng_Document > 0 Or aobj_Document.intDocumentId > 0) Then

            ''' actualizar la imagen  del archivo 
            ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
            iAdapt_comand = New OleDbDataAdapter()
            iolecmd_comand = New OleDbCommand()
            ioleconx_conexion = New OleDbConnection() '' objeto de conexion que se usara para conectar 


            istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
            ioleconx_conexion.ConnectionString = istr_conx

            iolecmd_comand = ioleconx_conexion.CreateCommand()

            ldtb_Result = New DataTable("FeeData")

            'condicion de valores para ejecutarse 
            If aobj_Document.intDocumentId > 0 Then
                llng_Document = aobj_Document.intDocumentId
            End If


            strSQL = "spCRUDGCargoDocumentFile"

            iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intDocumentId").Value = llng_Document

            iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
            ''iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio.ToUpper()
            iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio

            iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentType").Value = aobj_Document.strDocumentType.ToUpper()

            iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = aobj_Document.intGeneralCargoUniversalId

            iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Integer)
            iolecmd_comand.Parameters("@intGCItemId").Value = aobj_Document.intGCInventoryItemId

            iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
            iolecmd_comand.Parameters("@strDocumentDescription").Value = ""

            iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
            iolecmd_comand.Parameters("@intmode").Value = 3

            iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
            iolecmd_comand.Parameters("@astrUsername").Value = astr_username



            iolecmd_comand.CommandText = strSQL
            iolecmd_comand.CommandType = CommandType.StoredProcedure
            iolecmd_comand.CommandTimeout = 99999

            llng_Document = 0

            Try
                iAdapt_comand.SelectCommand = iolecmd_comand
                iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
                iAdapt_comand.Fill(ldtb_Result)

            Catch ex As Exception
                Dim strError As String = ObtenerError(ex.Message, 99999)
                strError = strError
                strError = ex.Message
                Return dt_RetrieveErrorTable(ex.Message)
            Finally
                ioleconx_conexion.Close()
            End Try

        End If ''If alng_UniversalId > 0 And llng_Document > 0  Then


        Return ldtb_Result

    End Function

    ''' 

    <WebMethod()>
    Public Function SearchDocument(ByVal aobj_Document As ClsDocument) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        lint_recentsave = 0


        strSQL = "spCRUDDocumentFile"
        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = aobj_Document.intDocumentId

        iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio.ToUpper()

        iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentType").Value = ""

        iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentDescription").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 2

        iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUsername").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        llng_Document = 0

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function SearchDocumentByFolio(ByVal astr_Folio As String) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        lint_recentsave = 0


        strSQL = "spCRUDDocumentFile"
        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentFolio").Value = astr_Folio

        iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentType").Value = ""

        iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentDescription").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 2

        iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUsername").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        ''''''''''''''''''''

        'strSQL = "select * from tblclsDocument where strDocumentFolio = ?"
        'iolecmd_comand.Parameters.Add("@strDocumentId", OleDbType.Char)
        'iolecmd_comand.Parameters("@strDocumentId").Value = astr_Folio



        'iolecmd_comand.CommandText = strSQL
        'iolecmd_comand.CommandType = CommandType.Text
        'iolecmd_comand.CommandTimeout = 99999

        '''''''''''''''''''''''''''

        llng_Document = 0

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            'Dim writeinfo() As Byte
            'Dim lint_val As Integer

            'writeinfo = CType(ldtb_Result(0)("imgDocumentImageFile"), Byte())

            'lint_val = writeinfo.Length

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function SearchDocumentById(ByVal alng_Id As Long) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        lint_recentsave = 0


        strSQL = "spCRUDDocumentFile"
        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = alng_Id

        iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentFolio").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentType").Value = ""

        iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentDescription").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 2

        iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUsername").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        llng_Document = 0

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    ''----------
    <WebMethod()>
    Public Function SearchAndSaveDocument(ByVal aobj_Document As ClsDocument, ByVal astr_username As String) As Long

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        lint_recentsave = 0


        strSQL = "spCRUDDocumentFile"
        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = aobj_Document.intDocumentId

        iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentFolio").Value = aobj_Document.strDocumentFolio.ToUpper()

        iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentType").Value = ""

        iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentDescription").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 2

        iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUsername").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        llng_Document = 0

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

            'obtener el numero de documento
            If Long.TryParse(ldtb_Result(0)("intDocumentId"), llng_Document) = False Then
                llng_Document = 0
            End If


        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            ' Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        '' si no se ha encontrado el documento
        If llng_Document = 0 Then

            ''crear el documento
            ldtb_Result = New DataTable("document")

            '' ver si es de  carga general 
            If aobj_Document.intGeneralCargoUniversalId > 0 Then

                ldtb_Result = SaveDocumentGeneralCargo(aobj_Document, astr_username)

            Else
                ldtb_Result = SaveDocumentContainer(aobj_Document, 0, astr_username)
            End If

            '' el primer renglon primera columna retorna el numero de documento

            If Long.TryParse(ldtb_Result(0)(0), llng_Document) = False Then
                llng_Document = 0
            End If



        End If ''If llng_Document = 0 Then

        Return llng_Document

    End Function

    <WebMethod()>
    Public Function GetDocumentsForContFee(ByVal alng_ContFee As Long, ByVal astr_ContCargoType As String) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim ldtb_ReturnTable = New DataTable()
        Dim llist_DocLng As New List(Of Long)
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer
        Dim llng_Documentid As Long
        Dim lrow_newrow As DataRow
        Dim lbyteArr() As Byte

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        '' tabla de retorno analizada
        ldtb_ReturnTable = New DataTable("returnTable")

        ldtb_ReturnTable.Columns.Add("intDocumentId", GetType(String))
        ldtb_ReturnTable.Columns.Add("strDocumentFolio", GetType(String))
        ldtb_ReturnTable.Columns.Add("imgDocumentImageFile", GetType(Byte()))
        ldtb_ReturnTable.Columns.Add("strDocumentCreatedBy", GetType(String))
        ldtb_ReturnTable.Columns.Add("dtmDocumentLastModified", GetType(String))
        ldtb_ReturnTable.Columns.Add("strDocumentTypeIdentifier", GetType(String))
        ldtb_ReturnTable.Columns.Add("strDocumentTypeDescription", GetType(String))

        lint_recentsave = 0


        strSQL = "spCRUDDocumentFile"
        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = alng_ContFee

        iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentFolio").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentType").Value = ""

        iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentDescription").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 4

        iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUsername").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = astr_ContCargoType

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        llng_Document = 0

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

            ''recorrer la tabla para obtener los logns 
            Try
                If ldtb_Result.Rows.Count > 0 And ldtb_Result.Columns.Count > 1 Then
                    For Each datainfo As DataRow In ldtb_Result.Rows

                        llng_Documentid = 0

                        llng_Documentid = Convert.ToInt64(datainfo("intDocumentId").ToString())

                        If llng_Documentid > 0 Then
                            llist_DocLng.Add(llng_Documentid)
                        End If

                        '' incializar valores  de byte
                        lbyteArr = New Byte(0) {}
                        lbyteArr(0) = New Byte()
                        '''
                        '' de una vez crear el registro en la tabla de retorno 

                        lrow_newrow = ldtb_ReturnTable.NewRow()
                        lrow_newrow("intDocumentId") = datainfo("intDocumentId").ToString()
                        lrow_newrow("strDocumentFolio") = datainfo("strDocumentFolio").ToString()
                        lrow_newrow("strDocumentCreatedBy") = datainfo("strDocumentCreatedBy").ToString()
                        lrow_newrow("dtmDocumentLastModified") = datainfo("dtmDocumentLastModified").ToString()

                        Try
                            lbyteArr = datainfo("imgDocumentImageFile")
                            lrow_newrow("imgDocumentImageFile") = datainfo("imgDocumentImageFile")
                        Catch ex As Exception
                            lbyteArr = New Byte(0) {}
                            lbyteArr(0) = New Byte()
                            lrow_newrow("imgDocumentImageFile") = lbyteArr
                        End Try
                        ''resetear objeto
                        lbyteArr = New Byte(0) {}
                        lbyteArr(0) = New Byte()

                        '' campos de tipo de documento
                        Try
                            lrow_newrow("strDocumentTypeIdentifier") = ""
                            lrow_newrow("strDocumentTypeDescription") = ""

                            lrow_newrow("strDocumentTypeIdentifier") = datainfo("strDocumentTypeIdentifier").ToString()
                            lrow_newrow("strDocumentTypeDescription") = datainfo("strDocumentTypeDescription").ToString()


                        Catch ex As Exception

                        End Try
                        '' fin -tipo de documento
                        '' insercion renglon 
                        ldtb_ReturnTable.Rows.Add(lrow_newrow)
                    Next

                    ' si la lista tiene items 
                    'If llist_DocLng.Count > 0 Then

                    'End If

                    ldtb_Result = ldtb_ReturnTable
                End If

            Catch exb As Exception
                Dim strinfob As String

            End Try

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    '' --- 
    <WebMethod()>
    Public Function GetDocumentTypesCatalogForFee() As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim ldtb_ReturnTable = New DataTable()
        Dim llist_DocLng As New List(Of Long)
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim llng_Document As Long
        Dim lint_recentsave As Integer
        Dim llng_Documentid As Long
        Dim lrow_newrow As DataRow
        Dim lbyteArr() As Byte

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("FeeData")

        '' tabla de retorno analizada
        ldtb_ReturnTable = New DataTable("returnTable")



        strSQL = "spCRUDDocumentFile"
        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentFolio", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentFolio").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentType", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentType").Value = ""

        iolecmd_comand.Parameters.Add("@intUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strDocumentDescription", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentDescription").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 5

        iolecmd_comand.Parameters.Add("@astrUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@astrUsername").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        llng_Document = 0

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)

        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            ' Return dt_RetrieveErrorTable(ex.Message)
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    ''--------------------------
    <WebMethod()>
    Public Function SetDocumentToFee(ByVal alng_Document As Long, ByVal alngstoFeeid As Long) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContainerStorageFee"

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@blnDokingCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnDokingCharge").Value = 0

        iolecmd_comand.Parameters.Add("@blnPbipCharge", OleDbType.Integer)
        iolecmd_comand.Parameters("@blnPbipCharge").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = ""

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 21

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alngstoFeeid

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = alng_Document

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""


        iolecmd_comand.Parameters.Add("@strContainerCargoType", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerCargoType").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    '''''''
    ''
    <WebMethod()>
    Public Function SetDocumentToFeeGC(ByVal alng_Document As Long, ByVal alngstoFeeid As Long, ByVal astr_Username As String) As DataTable
        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoStorageFee"


        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intGCItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCItemId").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredBy", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredBy").Value = 0

        iolecmd_comand.Parameters.Add("@intRequiredByType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intRequiredByType").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerId").Value = 0

        iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
        iolecmd_comand.Parameters("@intCustomerType").Value = 0

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_Username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 21

        iolecmd_comand.Parameters.Add("@intStorageFeeId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intStorageFeeId").Value = alngstoFeeid

        iolecmd_comand.Parameters.Add("@intDocumentId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intDocumentId").Value = alng_Document

        iolecmd_comand.Parameters.Add("@strFiscalMov", OleDbType.Char)
        iolecmd_comand.Parameters("@strFiscalMov").Value = ""

        iolecmd_comand.Parameters.Add("@intFiscalMov", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFiscalMov").Value = 0

        iolecmd_comand.Parameters.Add("@decMinWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMinWeight").Value = 0

        iolecmd_comand.Parameters.Add("@decMaxWeight", OleDbType.Decimal)
        iolecmd_comand.Parameters("@decMaxWeight").Value = 0

        iolecmd_comand.Parameters.Add("@intVesselId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intVesselId").Value = 0

        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselName").Value = ""

        iolecmd_comand.Parameters.Add("@strProductIdName", OleDbType.Char)
        iolecmd_comand.Parameters("@strProductIdName").Value = ""

        iolecmd_comand.Parameters.Add("@intProductId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intProductId").Value = 0

        iolecmd_comand.Parameters.Add("@strNumbers", OleDbType.Char)
        iolecmd_comand.Parameters("@strNumbers").Value = ""

        iolecmd_comand.Parameters.Add("@strMarks", OleDbType.Char)
        iolecmd_comand.Parameters("@strMarks").Value = ""

        iolecmd_comand.Parameters.Add("@strBLName", OleDbType.Char)
        iolecmd_comand.Parameters("@strBLName").Value = ""

        iolecmd_comand.Parameters.Add("@strContainerId", OleDbType.Char)
        iolecmd_comand.Parameters("@strContainerId").Value = ""

        iolecmd_comand.Parameters.Add("@intQty", OleDbType.Integer)
        iolecmd_comand.Parameters("@intQty").Value = 0

        ''


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try


        Return ldtb_Result

    End Function
    '''
    <WebMethod()>
    Public Function GetSentCalcs(ByVal astr_Username As String) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        'If aint_UserId >= 0 Then

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("calclist")
        strSQL = "spGetContainerFeeList"

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_Username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 2

        iolecmd_comand.Parameters.Add("@intFeeId", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intFeeId").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        'End If

        Return ldtb_Result

    End Function

    ''''
    <WebMethod()>
    Public Function GetValidCalcs(ByVal astr_Username As String) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        'If aint_UserId >= 0 Then

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("calclist")
        strSQL = "spGetContainerFeeList"

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_Username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 3

        iolecmd_comand.Parameters.Add("@intFeeId", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intFeeId").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        'End If

        Return ldtb_Result

    End Function
    ''''''''''''''''''''
    ''''
    <WebMethod()>
    Public Function GetCalcs(ByVal astr_Username As String, ByVal astr_Status As String) As DataTable

        Dim ldtb_Result = New DataTable() ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""

        'If aint_UserId >= 0 Then

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("calclist")
        strSQL = "spGetContainerFeeList"

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_Username

        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)

        'si el nombre de usuario es menor a caracteres tomar en cuenta el estado enviado
        If astr_Username.Length < 2 Then

            astr_Status = astr_Status.ToUpper()
            Select Case astr_Status
                Case "CAP"
                    iolecmd_comand.Parameters("@intmode").Value = 1
                Case "SEND"
                    iolecmd_comand.Parameters("@intmode").Value = 2
                Case "VALID"
                    iolecmd_comand.Parameters("@intmode").Value = 3
                Case "FACT"
                    iolecmd_comand.Parameters("@intmode").Value = 4
                Case Else
                    Return New DataTable("error")

            End Select

        Else ' si el nombre de usuario es de mas de 2 caracteres , evaluarlo en modo variable, modo 11
            iolecmd_comand.Parameters("@intmode").Value = 11
        End If



        iolecmd_comand.Parameters.Add("@intFeeId", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intFeeId").Value = 0


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999

        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
        Finally
            ioleconx_conexion.Close()
        End Try

        '    Return ldtb_Result
        'Else
        '    Return ldtb_Result

        'End If

        Return ldtb_Result

    End Function
    ''

    ''
    <WebMethod()>
    Public Function GetFeeContAllPendingNotes(ByVal astr_Username As String) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = 0

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = 0

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = ""

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)

        If astr_Username.Length > 0 Then '' si tiene usuario , se restrige que busque por su entidad de usuario
            iolecmd_comand.Parameters("@intOperation").Value = 8
        Else ' si no tiene usuario es modalidad 7 , sin restriccion de usuario
            iolecmd_comand.Parameters("@intOperation").Value = 7
        End If



        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_Username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    ''
    <WebMethod()>
    Public Function GetFeeGCargoAllPendingNotes(ByVal astr_Username As String) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDGCargoFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = 0

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = 0

        iolecmd_comand.Parameters.Add("@intGeneralCargoUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGeneralCargoUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@intGCInventoryItemId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intGCInventoryItemId").Value = 0

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = ""

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 0

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)

        If astr_Username.Length > 0 Then '' si tiene usuario , se restrige que busque por su entidad de usuario
            iolecmd_comand.Parameters("@intOperation").Value = 8
        Else ' si no tiene usuario es modalidad 7 , sin restriccion de usuario
            iolecmd_comand.Parameters("@intOperation").Value = 7
        End If



        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = astr_Username

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function
    ''
    <WebMethod()>
    Public Function GetFeeContNotesForFee(ByVal intFeeStorageId As Integer) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = intFeeStorageId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = 0

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = ""

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 1

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 1

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 5

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = ""

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function

    <WebMethod()>
    Public Function GetFeeContNotesElement(ByVal intFeeStorageId As Integer, ByVal intNoteItem As Integer) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spCRUDContFeeNotes"

        ''

        iolecmd_comand.Parameters.Add("@intFeeStorageId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intFeeStorageId").Value = intFeeStorageId

        iolecmd_comand.Parameters.Add("@intNoteItem", OleDbType.Integer)
        iolecmd_comand.Parameters("@intNoteItem").Value = intNoteItem

        iolecmd_comand.Parameters.Add("@intContainerUniversalId", OleDbType.Integer)
        iolecmd_comand.Parameters("@intContainerUniversalId").Value = 0

        iolecmd_comand.Parameters.Add("@strText", OleDbType.Char)
        iolecmd_comand.Parameters("@strText").Value = ""

        iolecmd_comand.Parameters.Add("@intNoteType", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intNoteType").Value = 0

        iolecmd_comand.Parameters.Add("@strStatus", OleDbType.Char)
        iolecmd_comand.Parameters("@strStatus").Value = ""

        iolecmd_comand.Parameters.Add("@blnActive", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnActive").Value = 0

        iolecmd_comand.Parameters.Add("@blnChecked", OleDbType.Numeric)
        iolecmd_comand.Parameters("@blnChecked").Value = 0

        iolecmd_comand.Parameters.Add("@strAditionalComs", OleDbType.Char)
        iolecmd_comand.Parameters("@strAditionalComs").Value = ""

        iolecmd_comand.Parameters.Add("@intOperation", OleDbType.Numeric)
        iolecmd_comand.Parameters("@intOperation").Value = 6

        iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
        iolecmd_comand.Parameters("@strUsername").Value = ""

        iolecmd_comand.Parameters.Add("@strHeaderNote", OleDbType.Char)
        iolecmd_comand.Parameters("@strHeaderNote").Value = ""

        iolecmd_comand.Parameters.Add("@strDocumentName", OleDbType.Char)
        iolecmd_comand.Parameters("@strDocumentName").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        Return ldtb_Result

    End Function


    ''
    <WebMethod()>
    Public Function GetFiscalMovementForStorage() As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spGetFiscalMovement"

        ''

        iolecmd_comand.Parameters.Add("@intMode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intMode").Value = 1

        iolecmd_comand.Parameters.Add("@strFilterValue", OleDbType.Char)
        iolecmd_comand.Parameters("@strFilterValue").Value = ""

        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        'Return ldtb_Result
        Return of_translateDatableMX(ldtb_Result)

    End Function
    ''

    <WebMethod()>
    Public Function GetVesselList(ByVal astr_vesselName As String) As DataTable

        Dim ldtb_Result = New DataTable("userresult") ' la tabla que obtiene el resultado
        Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter()
        Dim iolecmd_comand As OleDbCommand = New OleDbCommand()
        Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

        Dim istr_conx As String = "" ' cadena de conexion
        Dim strSQL As String = ""
        Dim lstr_error As String = ""
        Dim lint_result As Integer

        istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ToString()
        ioleconx_conexion.ConnectionString = istr_conx
        iolecmd_comand = ioleconx_conexion.CreateCommand()

        ldtb_Result = New DataTable("User")
        strSQL = "spGetVesselList"

        ''
        iolecmd_comand.Parameters.Add("@strVesselName", OleDbType.Char)
        iolecmd_comand.Parameters("@strVesselName").Value = astr_vesselName

        ''
        iolecmd_comand.Parameters.Add("@intmode", OleDbType.Integer)
        iolecmd_comand.Parameters("@intmode").Value = 1


        iolecmd_comand.CommandText = strSQL
        iolecmd_comand.CommandType = CommandType.StoredProcedure
        iolecmd_comand.CommandTimeout = 99999


        Try
            iAdapt_comand.SelectCommand = iolecmd_comand
            iAdapt_comand.SelectCommand.CommandTimeout = of_getMaxTimeout()
            iAdapt_comand.Fill(ldtb_Result)
            lstr_error = ""


        Catch ex As Exception
            Dim strError As String = ObtenerError(ex.Message, 99999)
            strError = strError
            strError = ex.Message
            lstr_error = strError
        Finally
            ioleconx_conexion.Close()
        End Try

        'Return ldtb_Result
        Return of_translateDatableMX(ldtb_Result)

    End Function
    ''

    Public Function of_translateDatableMX(ByVal adtb_source As DataTable) As DataTable

        Dim lstr_Valueitem As String
        Dim lstr_newString As String
        Dim lint_Rowidx = 0
        ' renglones 
        For Each lrow As DataRow In adtb_source.Rows



            'columnas 
            For lintColIdx As Integer = 0 To adtb_source.Columns.Count - 1
                'intento de conversion
                Try
                    lstr_Valueitem = lrow(lintColIdx).ToString()

                    lstr_newString = ""
                    '' analisar cada caracter
                    For Each lchar_item As Char In lstr_Valueitem
                        If lchar_item = "¢" Then
                            lchar_item = "ó"
                        End If

                        If lchar_item = "£" Then
                            lchar_item = "ú"
                        End If

                        If lchar_item = "µ" Then
                            lchar_item = "Á"
                        End If


                        If lchar_item = "Ö" Then
                            lchar_item = "Í"
                        End If

                        If lchar_item = "à" Then
                            lchar_item = "Ó"
                        End If


                        If lchar_item = "é" Then
                            lchar_item = "Ú"
                        End If

                        If lchar_item = "¤" Then
                            lchar_item = "ñ"
                        End If

                        If lchar_item = "¥" Then
                            lchar_item = "Ñ"
                        End If

                        lstr_newString = lstr_newString + lchar_item
                    Next

                    'asignar la cadena convertida
                    adtb_source.Rows(lint_Rowidx)(lintColIdx) = lstr_newString

                Catch ex As Exception
                    Dim lstr_Error As String = ex.Message
                    lstr_Error = lstr_Error
                End Try
            Next

            lint_Rowidx = lint_Rowidx + 1

        Next ''For Each lrow As DataRow In adtb_source.Rows

        Return adtb_source

    End Function


    Public Sub CopyTableAndCheckLatin(ByVal atb_Original As DataTable, ByRef atb_Destiny As DataTable)

        atb_Destiny = New DataTable()
        Dim lcolum_new As DataColumn = New DataColumn()
        Dim lrow_new As DataRow

        Dim lstr_eñe_min As String
        Dim lstr_eñe_max As String
        Dim lstr_stringElement As String
        Dim lstr_stringItermadiate As String
        Dim lchar_item As Char
        Dim lint_isokitem As Integer

        lstr_eñe_min = "¤"
        lstr_eñe_max = "¥"
        lstr_stringElement = ""

        For Each lcolum_table As DataColumn In atb_Original.Columns
            lcolum_new = New DataColumn(lcolum_table.ColumnName)
            lcolum_new.DataType = lcolum_table.DataType
            lcolum_new.Caption = lcolum_table.Caption
            atb_Destiny.Columns.Add(lcolum_new)
        Next

        For Each lrow_original As DataRow In atb_Original.Rows

            lrow_new = atb_Destiny.NewRow()

            For lint_index = 0 To atb_Original.Columns.Count - 1

                If atb_Original.Columns(lint_index).DataType.Name.ToString.ToLower = "string" Then
                    lstr_stringElement = lrow_original(lint_index).ToString

                    lstr_stringItermadiate = ""

                    'pasar caracter por caracter
                    For lint_Charitem = 0 To lstr_stringElement.Length - 1
                        lint_isokitem = 1

                        Try
                            ' si es caracter validos 
                            If of_IsValidCharacter(lstr_stringElement(lint_Charitem)) > 0 Then
                                lstr_stringItermadiate = lstr_stringItermadiate + lstr_stringElement(lint_Charitem)

                            End If


                        Catch ex As Exception

                        End Try

                    Next

                    If lstr_stringItermadiate.Length > 0 Then
                        lstr_stringElement = lstr_stringItermadiate
                    End If

                    ''revision de ñs
                    If lstr_stringElement.IndexOf(lstr_eñe_min) > -1 Then
                        lstr_stringElement = lstr_stringElement.Replace(lstr_eñe_min, "ñ")
                    End If

                    If lstr_stringElement.IndexOf(lstr_eñe_max) > -1 Then
                        lstr_stringElement = lstr_stringElement.Replace(lstr_eñe_max, "Ñ")
                    End If

                    lrow_new(lint_index) = lstr_stringElement
                Else
                    lrow_new(lint_index) = lrow_original(lint_index)
                End If

                ' lrow_new(lint_index) = lrow_original(lint_index)
            Next

            atb_Destiny.Rows.Add(lrow_new)

        Next

    End Sub

    Public Function of_IsValidCharacter(ByVal achar_Element As Char) As Integer

        If Char.IsDigit(achar_Element) = True Then
            Return 1
        End If

        If Char.IsLetter(achar_Element) = True Then
            Return 1
        End If


        If Char.IsNumber(achar_Element) = True Then
            Return 1
        End If

        If Char.IsPunctuation(achar_Element) = True Then
            Return 1
        End If

        If Char.IsSeparator(achar_Element) = True Then
            Return 1
        End If

        If Char.IsSymbol(achar_Element) = True Then
            Return 1
        End If

        '''
        '''

        If achar_Element = "¢" Then
            'lchar_item = "ó"
            Return 1
        End If

        If achar_Element = "£" Then
            'lchar_item = "ú"
            Return 1
        End If

        If achar_Element = "µ" Then
            'lchar_item = "Á"
            Return 1
        End If


        If achar_Element = "Ö" Then
            Return 1
        End If

        If achar_Element = "à" Then
            ' lchar_item = "Ó"
            Return 1
        End If


        If achar_Element = "é" Then
            'lchar_item = "Ú"
            Return 1
        End If

        If achar_Element = "¤" Then
            'lchar_item = "ñ"
            Return 1
        End If

        If achar_Element = "¥" Then
            'lchar_item = "Ñ"
            Return 1
        End If

        'asccii
        If Asc(achar_Element) >= 32 And Asc(achar_Element) <= 126 Then
            Return 1
        End If

        Return 0
    End Function
    '''

    '''''''''''''''''''

    ''------------

    ''''---
    ''''''''''''''''''
    ''''''''''''''''''''''''
    '''''''''''''''''''''''''''
    '''''''''''''''''''''''''''''''

    ''''''''''''''''''''''''''''''''''
    '''''''''''''''

    ''''''''''''''''''''
    '''''''''''''''''
    ''

    Public Function of_getDatasetToTable(ByVal aadapt_adapter As DataSet) As DataTable

        Dim ldt_tableresult As DataTable = New DataTable("result")
        Dim ldt_tablecolumns As DataTable = New DataTable("columns")
        Dim ldr_datarowName As DataRow
        Dim ldr_datarowex As DataRow

        ' crea tabla de nombres de columnas 
        ldt_tablecolumns.Columns.Add("namecol", GetType(String))

        '''''''''''''
        ' analizar la primera tabla y obtener el listado de nombres de columnas 
        If aadapt_adapter.Tables.Count > 0 Then
            ' oobtener los nombres de columnas 
            For Each idx_datacolumn As DataColumn In aadapt_adapter.Tables(0).Columns


            Next

        Else
            'retornar tabla vacia 

        End If 'If aadapt_adapter.Tables.Count > 0 Then


        ''''''''''''''''''
        ' recoorer las demas tablas si es que hay , 
        If aadapt_adapter.Tables.Count > 1 Then

        End If 'If aadapt_adapter.Tables.Count > 1 Then


        Return ldt_tableresult

    End Function


    Public Function of_ConvertDateToStringGeneralFormat(ByVal adtm_param As Date) As String

        '''''''''''''''

        Dim lstr_appointmentDate As String
        Dim lstr_tempA As String

        ''''''''''''''''''''
        ' revisar la fecha de la cita 
        Try
            '' obtener el año
            lstr_tempA = adtm_param.Year.ToString()

            If lstr_tempA.Length > 1 Then
                lstr_appointmentDate = lstr_tempA
            Else
                lstr_appointmentDate = ""
            End If 'If lstr_tempA.Length > 1 Then

            'validar cadena de fecha 
            If lstr_appointmentDate.Length > 1 Then
                'obtener mes 
                lstr_tempA = adtm_param.Month.ToString()

                'agregar 0, si es de un digito
                If lstr_tempA.Length = 1 Then
                    lstr_tempA = "0" + lstr_tempA
                End If

                'validacion 
                If lstr_tempA.Length < 2 Then
                    lstr_tempA = ""
                    lstr_appointmentDate = ""
                Else
                    lstr_appointmentDate = lstr_appointmentDate + lstr_tempA
                    'lstr_appointmentDate = ""
                End If

                If adtm_param.Year = 1 Then
                    lstr_appointmentDate = ""
                End If

                If adtm_param.Year < 1910 Then
                    lstr_appointmentDate = ""
                End If
            End If 'If lstr_appointmentDate.Length > 1 Then

            If lstr_appointmentDate.Length > 1 Then
                'obtener el dia 
                lstr_tempA = adtm_param.Day.ToString()

                'agregar 0, si es de un digito
                If lstr_tempA.Length = 1 Then
                    lstr_tempA = "0" + lstr_tempA
                End If

                'validacion
                If lstr_tempA.Length < 2 Then
                    lstr_tempA = ""
                    lstr_appointmentDate = ""
                Else
                    lstr_appointmentDate = lstr_appointmentDate + lstr_tempA
                    'lstr_appointmentDate = ""
                End If ' lstr_tempA.lenght

            End If 'If lstr_appointmentDate.Length > 1 Then

            'hora
            ''''''
            If lstr_appointmentDate.Length > 1 Then
                'obtener el hora
                lstr_tempA = adtm_param.Hour.ToString()

                'agregar 0, si es de un digito
                If lstr_tempA.Length = 1 Then
                    lstr_tempA = "0" + lstr_tempA
                End If

                'validacion
                If lstr_tempA.Length < 2 Then
                    lstr_tempA = ""
                    lstr_appointmentDate = ""
                Else
                    lstr_appointmentDate = lstr_appointmentDate + " " + lstr_tempA
                    'lstr_appointmentDate = ""
                End If ' lstr_tempA.lenght

            End If 'If lstr_appointmentDate.Length > 1 Then


            ''minutos
            ''''''''''
            If lstr_appointmentDate.Length > 1 Then
                'obtener el minutos
                lstr_tempA = adtm_param.Minute.ToString()

                'agregar 0, si es de un digito
                If lstr_tempA.Length = 1 Then
                    lstr_tempA = "0" + lstr_tempA
                End If

                'validacion
                If lstr_tempA.Length < 2 Then
                    lstr_tempA = ""
                    lstr_appointmentDate = ""
                Else
                    lstr_appointmentDate = lstr_appointmentDate + ":" + lstr_tempA
                    'lstr_appointmentDate = ""
                End If ' lstr_tempA.lenght

            End If 'If lstr_appointmentDate.Length > 1 Then


            ''segundos
            ''''''''''
            If lstr_appointmentDate.Length > 1 Then
                'obtener el segundos
                lstr_tempA = adtm_param.Second.ToString()

                'agregar 0, si es de un digito
                If lstr_tempA.Length = 1 Then
                    lstr_tempA = "0" + lstr_tempA
                End If

                'validacion
                If lstr_tempA.Length < 2 Then
                    lstr_tempA = ""
                    lstr_appointmentDate = ""
                Else
                    lstr_appointmentDate = lstr_appointmentDate + ":" + lstr_tempA
                    'lstr_appointmentDate = ""
                End If ' lstr_tempA.lenght

            End If 'If lstr_appointmentDate.Length > 1 Then

        Catch ex As Exception
            lstr_appointmentDate = ""
        End Try

        Return lstr_appointmentDate

        Return ""

    End Function

    Public Function of_getDatePartStr(ByVal adtm_param As Date, ByVal astr_Part As String) As String

        '''''''''''''''

        Dim lstr_appointmentDate As String
        Dim lstr_Datepart As String
        Dim lstr_tempA As String

        ''''''''''''''''''''
        ' revisar la fecha de la cita 
        Try
            If astr_Part = "YEAR" Then
                '' obtener el año
                lstr_tempA = adtm_param.Year.ToString()

                If lstr_tempA.Length > 1 Then
                    lstr_Datepart = lstr_tempA
                Else
                    lstr_Datepart = ""
                End If 'If lstr_tempA.Length > 1 Then
                Return lstr_Datepart
            End If

            If astr_Part = "DAY" Then

                'obtener el dia 
                lstr_tempA = adtm_param.Day.ToString()

                'agregar 0, si es de un digito
                If lstr_tempA.Length = 1 Then
                    lstr_tempA = "0" + lstr_tempA
                End If

                'validacion
                If lstr_tempA.Length < 2 Then
                    lstr_tempA = ""

                End If ' lstr_tempA.lenght

                Return lstr_tempA
            End If

            If astr_Part = "MONTH" Then

                'obtener mes 
                lstr_tempA = adtm_param.Month.ToString()

                'agregar 0, si es de un digito
                If lstr_tempA.Length = 1 Then
                    lstr_tempA = "0" + lstr_tempA
                End If

                'validacion 
                If lstr_tempA.Length < 2 Then
                    lstr_tempA = ""

                End If

                Return lstr_tempA
            End If

        Catch ex As Exception
            lstr_appointmentDate = ""
        End Try

        Return lstr_appointmentDate

        Return ""

    End Function

    Public Function of_convertoasccistring(ByVal astr_original As String) As String

        Dim lstr_newstring As String

        Try
            If astr_original.Length = 0 Then
                astr_original = ""
                Return ""
            End If
        Catch ex As Exception
            Return ""
        End Try

        lstr_newstring = astr_original.ToUpper()
        'a
        lstr_newstring = lstr_newstring.Replace("Á", "A")
        'e
        lstr_newstring = lstr_newstring.Replace("É", "E")
        'i
        lstr_newstring = lstr_newstring.Replace("Í", "I")
        'o
        lstr_newstring = lstr_newstring.Replace("Ó", "O")
        'u
        lstr_newstring = lstr_newstring.Replace("Ú", "U")
        'ñ
        lstr_newstring = lstr_newstring.Replace("Ñ", "N")


        Return lstr_newstring

    End Function

    Public Function of_generatetableFromRowsList(ByVal arws_list() As DataRow) As DataTable
        Dim lstr_columname As String
        Dim ldt_datatable As DataTable = New DataTable("tableresult")
        Dim lrow As DataRow
        Dim lrow_new As DataRow
        Dim lcol As DataColumn

        If arws_list.Length = 0 Then
            Return ldt_datatable
        End If

        lrow = arws_list(0)

        'copiar las columnas
        For Each ltemp As DataColumn In lrow.Table.Columns
            lcol = New DataColumn(ltemp.ColumnName, ltemp.DataType)
            'lrow_new.Table.Columns.Add(lcol)
            ldt_datatable.Columns.Add(lcol)
        Next

        Dim lint_idx As Integer = 0

        For lint_idx = 0 To arws_list.Length - 1
            lrow_new = ldt_datatable.NewRow

            For Each ltemp As DataColumn In lrow.Table.Columns
                lrow_new(ltemp.ColumnName) = arws_list(lint_idx)(ltemp.ColumnName)
            Next
            '' agregar el renglon a la tabla 
            ldt_datatable.Rows.Add(lrow_new)
        Next

        'insertar los renglones
        Return ldt_datatable

    End Function



    Public Function of_getMaxTimeout() As Integer
        Return 999999999
    End Function
    'Public Function of_tempSaveVisitMaster(ByVal alng_Visit As Long, ByVal alng_Carrier As Long, ByVal astr_chofer As String, ByVal alng_Customer As Long, ByVal astr_Reference As String, ByVal astr_Plates As String, ByVal aint_operationCounter As Long, ByVal alng_RequiredBy As Long, ByVal aint_RequiredByType As Long, ByVal alng_ServiceOrder As Long, ByVal astr_DriverLicence As String, ByVal astr_UserName As String, ByVal astr_appointmentDate As String) As String

    '    ''''''''''''''''''''''''''
    '    '-----------------------------

    '    Dim ldt_VisitResult As DataTable 'tabla que guardara el resultado del query


    '    ' Dim iAdapt_comand As OleDbDataAdapter = New OleDbDataAdapter() '' Adaptador que ejecuta la tabla y el comando
    '    'Dim iolecmd_comand As OleDbCommand '' objeto comando que se ejecutara
    '    'Dim ioleconx_conexion As OleDbConnection = New OleDbConnection() '' objeto de conexion que se usara para conectar 

    '    Dim sybConn As ADODB.Connection 'ADO Connection object
    '    Dim sybCmd As ADODB.Command 'ADO Command object
    '    Dim errLoop As ADODB.Error 'ADO Error object
    '    Dim sybRst As ADODB.Recordset 'ADO Recordset object
    '    Dim sybFld As ADODB.Field 'ADO Field object
    '    Dim sybFld2 As ADODB.Field 'ADO Field Object to collect column info
    '    Dim sybParameter As ADODB.Parameter 'ADO Parameter object

    '    Dim istr_conx As String '' cadena de conexion
    '    Dim lint_operation As Integer = 0
    '    'Dim lparamGeneric As OleDbParameter = New OleDbParameter()

    '    Dim lstr_SQL As String
    '    Dim lstr_Message As String = ""
    '    Dim lint_itemscount As Integer = 0

    '    Dim ldt_TableResult As DataTable
    '    Dim ldt_ReturnValueTable As DataTable
    '    Dim ldr_ReturnTickeRow As DataRow


    '    istr_conx = ConfigurationManager.ConnectionStrings("dbCalathus").ConnectionString
    '    ldt_TableResult = New DataTable()

    '    ldt_ReturnValueTable = New DataTable()
    '    ldt_ReturnValueTable.TableName = "TableResultVisit"
    '    'ldt_ReturnValueTable.Columns.Add("ServiceOrderId", GetType(Long))

    '    'Return alng_Visit

    '    ''' validaciones ---> ????

    '    ioleconx_conexion.ConnectionString = istr_conx
    '    iolecmd_comand = ioleconx_conexion.CreateCommand()
    '    iolecmd_comand.CommandTimeout = 0
    '    lint_itemscount = lint_itemscount + 1
    '    'limpiar cadena sql
    '    lstr_SQL = ""

    '    ''' conversion de la cadena de chofer a string 
    '    astr_chofer = of_convertoasccistring(astr_chofer)

    '    ''''--- 

    '    'agregar parametros
    '    iolecmd_comand.Parameters.Add("@intVisitId", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intCarrierId", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@intCustomerId", OleDbType.Integer)
    '    ''''''iolecmd_comand.Parameters.Add("@intCustomerType", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@strChofer", OleDbType.Char)
    '    iolecmd_comand.Parameters.Add("@strPlate", OleDbType.Char)
    '    iolecmd_comand.Parameters.Add("@strReference", OleDbType.Char)
    '    iolecmd_comand.Parameters.Add("@intOpCounter", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@requiredBy", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@requierbyType", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@serviceOrderId", OleDbType.Integer)
    '    iolecmd_comand.Parameters.Add("@strDriverLicence", OleDbType.Char)
    '    iolecmd_comand.Parameters.Add("@strUsername", OleDbType.Char)
    '    iolecmd_comand.Parameters.Add("@strApointmentDate", OleDbType.Char)




    '    ' agregar valores
    '    iolecmd_comand.Parameters("@intVisitId").Value = alng_Visit
    '    iolecmd_comand.Parameters("@intCarrierId").Value = alng_Carrier
    '    iolecmd_comand.Parameters("@intCustomerId").Value = alng_Customer
    '    '''''''''iolecmd_comand.Parameters("@intCustomerType").Value = aintCustomerType
    '    iolecmd_comand.Parameters("@strChofer").Value = astr_chofer
    '    iolecmd_comand.Parameters("@strPlate").Value = astr_Plates
    '    iolecmd_comand.Parameters("@strReference").Value = astr_Reference
    '    iolecmd_comand.Parameters("@intOpCounter").Value = aint_operationCounter
    '    iolecmd_comand.Parameters("@requiredBy").Value = alng_RequiredBy
    '    iolecmd_comand.Parameters("@requierbyType").Value = aint_RequiredByType
    '    iolecmd_comand.Parameters("@serviceOrderId").Value = alng_ServiceOrder
    '    iolecmd_comand.Parameters("@strDriverLicence").Value = astr_DriverLicence
    '    iolecmd_comand.Parameters("@strUsername").Value = astr_UserName

    '    iolecmd_comand.Parameters("@strApointmentDate").Value = astr_appointmentDate




    '    '''' -parametros del sp 
    '    ''''''''''''-- fin lista parametros del sp 

    '    '''''''''''''''''''"exec spAddEIRSeal " & dt.Rows(i)("intEIRId").ToString() & ", '" & strSeal & "', 0, '" & lstrusername & "'"

    '    'definir la cadena sql
    '    lstr_SQL = "spSaveVisitMasterWb"


    '    ''''''' lstr_SQL = "execute spSaveVisitMasterWb  @intVisitId=" + alng_Visit.ToString() + ", @intCarrierId=" + alng_Carrier.ToString() + ", @intCustomerId=" + alng_Customer.ToString() + ", @strChofer='" + astr_chofer + "', @strPlate='" + astr_Plates + "' , @strReference='" + astr_Reference + "' , @intOpCounter=" + aint_operationCounter.ToString() + " , @intRequiredBy=" + alng_RequiredBy.ToString() + ", @intRequireByType=" + aint_RequiredByType.ToString() + ", @intServiceOrderId=" + alng_ServiceOrder.ToString() + ", @strDriverLicence='" + astr_DriverLicence + "', @strUsername='" + astr_UserName + "', @strApointmentDate='" + astr_appointmentDate + "'"




    '    'definir que tipo de comando se va a ejecutar
    '    iolecmd_comand.CommandType = CommandType.StoredProcedure
    '    'iolecmd_comand.CommandType = CommandType.Text


    '    iolecmd_comand.CommandText = lstr_SQL

    '    ''ejecutar 
    '    Dim adapter As OleDbDataAdapter = New OleDbDataAdapter(iolecmd_comand)

    '    Try

    '        ''conectar
    '        iolecmd_comand.Connection.Open()
    '        'iolecmd_comand.ExecuteNonQuery()
    '        adapter.Fill(ldt_TableResult)
    '        ''desconectar
    '    Catch ex As Exception
    '        lstr_Message = ObtenerError(ex.Message, 9999)
    '        If lstr_Message.Length > 0 Then
    '            Return lstr_Message
    '        Else
    '            Return ex.Message
    '        End If
    '    Finally
    '        iolecmd_comand.Connection.Close()
    '        iolecmd_comand.Connection.Dispose()
    '        'ioleconx_conexion.close()
    '    End Try

    '    ' Return lint_itemscount.ToString()
    '    iolecmd_comand = Nothing
    '    'Return "despues ex--" + ldt_TableResult.Rows.Count.ToString() + "-" + ldt_TableResult.Columns.Count.ToString() + "<"
    '    '' ver si la tabla trajo informacion 
    '    Try

    '        If ldt_TableResult.Rows.Count = 1 And ldt_TableResult.Columns.Count = 1 Then
    '            Dim lstr_info As String
    '            lstr_info = ldt_TableResult(0)(0).ToString
    '            If lstr_info.Length > 0 Then
    '                Return lstr_info
    '            Else
    '                Return ""
    '            End If
    '        Else
    '            Return "="
    '        End If
    '    Catch ex As Exception
    '        Dim lstr_ex As String
    '        lstr_ex = ex.Message
    '        lstr_ex = lstr_ex
    '        Return "error al actualizar informacion "
    '    End Try

    '    Return ""

    '    '''''''''''''''''''''''''''''''''
    '    Return ""
    'End Function


    '''
    '       Try
    ' '' obtener el año
    '        lstr_tempA = adtm_appointmentDate.Year.ToString()

    '        If lstr_tempA.Length > 1 Then
    '            lstr_appointmentDate = lstr_tempA
    '            lstr_tempx1 = lstr_tempA
    '        Else
    '            lstr_appointmentDate = ""
    '            lstr_tempx1 = "y0"
    '        End If 'If lstr_tempA.Length > 1 Then

    ''validar cadena de fecha 
    '        If lstr_appointmentDate.Length > 1 Then
    ''obtener mes 
    '            lstr_tempA = adtm_appointmentDate.Month.ToString()

    ''agregar 0, si es de un digito
    '            If lstr_tempA.Length = 1 Then
    '                lstr_tempA = "0" + lstr_tempA
    '            End If

    ''validacion 
    '            If lstr_tempA.Length < 2 Then
    '                lstr_tempA = ""
    '                lstr_appointmentDate = ""
    '                lstr_tempx1 = lstr_tempx1 + "m=0"
    '            Else
    '                lstr_appointmentDate = lstr_appointmentDate + lstr_tempA
    '                lstr_tempx1 = lstr_tempx1 + lstr_tempA
    ''lstr_appointmentDate = ""
    '            End If

    '            If adtm_appointmentDate.Year = 1 Then
    '                lstr_appointmentDate = ""
    '                lstr_tempx1 = lstr_tempx1 + "year=1"
    '            End If


    '            If adtm_appointmentDate.Year < 1910 Then
    '                lstr_appointmentDate = ""
    '                lstr_tempx1 = lstr_tempx1 + "--" + adtm_appointmentDate.Year.ToString()
    '            End If

    '        End If 'If lstr_appointmentDate.Length > 1 Then

    '        If lstr_appointmentDate.Length > 1 Then
    ''obtener el dia 
    '            lstr_tempA = adtm_appointmentDate.Day.ToString()

    ''agregar 0, si es de un digito
    '            If lstr_tempA.Length = 1 Then
    '                lstr_tempA = "0" + lstr_tempA
    '            End If

    ''validacion
    '            If lstr_tempA.Length < 2 Then
    '                lstr_tempA = ""
    '                lstr_appointmentDate = ""
    '                lstr_tempx1 = lstr_tempx1 + "d-" + lstr_tempA
    '            Else
    '                lstr_appointmentDate = lstr_appointmentDate + lstr_tempA
    '                lstr_tempx1 = lstr_tempx1 + lstr_tempA
    ''lstr_appointmentDate = ""
    '            End If ' lstr_tempA.lenght

    '        End If 'If lstr_appointmentDate.Length > 1 Then

    ''hora
    '''''''
    '        If lstr_appointmentDate.Length > 1 Then
    ''obtener el hora
    '            lstr_tempA = adtm_appointmentDate.Hour.ToString()

    ''agregar 0, si es de un digito
    '            If lstr_tempA.Length = 1 Then
    '                lstr_tempA = "0" + lstr_tempA
    '            End If

    ''validacion
    '            If lstr_tempA.Length < 2 Then
    '                lstr_tempA = ""
    '                lstr_appointmentDate = ""
    '                lstr_tempx1 = lstr_tempx1 + "H-"
    '            Else
    '                lstr_appointmentDate = lstr_appointmentDate + " " + lstr_tempA
    '                lstr_tempx1 = lstr_tempx1 + lstr_tempA
    ''lstr_appointmentDate = ""
    '            End If ' lstr_tempA.lenght

    '        End If 'If lstr_appointmentDate.Length > 1 Then


    ' ''minutos
    '''''''''''
    '        If lstr_appointmentDate.Length > 1 Then
    ''obtener el minutos
    '            lstr_tempA = adtm_appointmentDate.Minute.ToString()

    ''agregar 0, si es de un digito
    '            If lstr_tempA.Length = 1 Then
    '                lstr_tempA = "0" + lstr_tempA
    '            End If

    ''validacion
    '            If lstr_tempA.Length < 2 Then
    '                lstr_tempA = ""
    '                lstr_appointmentDate = ""
    '                lstr_tempx1 = lstr_tempx1 + "min="
    '            Else
    '                lstr_appointmentDate = lstr_appointmentDate + ":" + lstr_tempA
    '                lstr_tempx1 = lstr_tempx1 + ":" + lstr_tempA
    ''lstr_appointmentDate = ""
    '            End If ' lstr_tempA.lenght

    '        End If 'If lstr_appointmentDate.Length > 1 Then


    ' ''segundos
    '''''''''''
    '        If lstr_appointmentDate.Length > 1 Then
    ''obtener el segundos
    '            lstr_tempA = adtm_appointmentDate.Second.ToString()

    ''agregar 0, si es de un digito
    '            If lstr_tempA.Length = 1 Then
    '                lstr_tempA = "0" + lstr_tempA
    '            End If

    ''validacion
    '            If lstr_tempA.Length < 2 Then
    '                lstr_tempA = ""
    '                lstr_appointmentDate = ""
    '                lstr_tempx1 = lstr_tempx1 + "S=0"
    '            Else
    '                lstr_appointmentDate = lstr_appointmentDate + ":" + lstr_tempA
    '                lstr_tempx1 = lstr_tempx1 + lstr_tempA
    ''lstr_appointmentDate = ""
    '            End If ' lstr_tempA.lenght

    '        End If 'If lstr_appointmentDate.Length > 1 Then

    '    Catch ex As Exception
    '        lstr_appointmentDate = ""
    '    End Try


    '    astr_Reference = astr_Reference + lstr_tempx1
    ''''

    ''-------
    ' 
    ''''----
End Class