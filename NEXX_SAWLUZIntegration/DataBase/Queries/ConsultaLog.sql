/*SELECT FROM "@NEXX_LOG" t0 WHERE t0."UpdateDate">= [%0];*/
/*SELECT FROM "@NEXX_LOG" t0 WHERE t0."UpdateDate">= [%1];*/
/*SELECT FROM "@NEXX_LOG" t0 WHERE t0."U_NEXX_Status" = '[%2]';*/

/*NEXX LOGS - Integração SAWLUZ*/
SELECT
    IFNULL(T0."U_NEXX_TipoDoc",'') "Documento"
    , IFNULL(TO_VARCHAR(T0."UpdateDate",'DD/MM/YYYY'),'') "Data"
	 ,(SUBSTRING(LPAD(TO_VARCHAR(T0."UpdateTime"), 4, '0'), 1, 2)||
    	':' ||
    	SUBSTRING(LPAD(TO_VARCHAR(T0."UpdateTime"), 4, '0'), 3, 2)) "Hora"
	 , IFNULL(T0."U_NEXX_IdDoc",'')  "ID Sap"
	 , CASE WHEN IFNULL(T0."U_NEXX_IdDocLeg",'') = '' OR IFNULL(T0."U_NEXX_IdDocLeg",'') = 'NULL' THEN '' ELSE  T0."U_NEXX_IdDocLeg" END "ID Legado"  
	 , IFNULL(T0."U_NEXX_IdRet",'') "Arquivo"
	 , CASE WHEN T0."U_NEXX_Status" = '1' THEN 'Pronto para Integrar' 
		    WHEN T0."U_NEXX_Status" = '2' THEN 'Sucesso' 
			ELSE 'Erro' 
	   END AS  "Status"
	 , IFNULL(T0."U_NEXX_MsgRet",'')  "Mensagem"
	 , IFNULL(TO_VARCHAR(T0."U_NEXX_JsonRet"),'')  "Objeto Retornado"
	 , IFNULL(TO_VARCHAR(T0."U_NEXX_JsonEnv"),'')  "Objeto Enviado" 
FROM 
	"@NEXX_LOG" T0
WHERE 
	T0."Name" = 'NEXX_SAWLUZIntegration'
	AND T0."U_NEXX_TipoDoc" = 'PedidoVendas'
AND "UpdateDate" >= [%0]  AND  "UpdateDate" <= [%1]
AND (T0."U_NEXX_Status" = '[%2]' OR '[%2]'= '')

