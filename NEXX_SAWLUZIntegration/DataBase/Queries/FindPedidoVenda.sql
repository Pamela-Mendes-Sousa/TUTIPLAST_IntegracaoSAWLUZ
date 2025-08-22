SELECT TOP 1
	TO_INT((SELECT MAX("DocEntry") FROM ORDR WHERE TO_VARCHAR("DocNum") = IFNULL(T0."U_NEXX_IdDocLeg",''))) "DocEntry"
	,IFNULL(TO_INT(T0."U_NEXX_IdDocLeg"),0) "DocNum"
FROM "@NEXX_LOG" T0
WHERE  
	T0."U_NEXX_IdRet" = '{0}'
	AND T0."U_NEXX_TipoDoc" = 'PedidoVendas'
	AND T0."U_NEXX_Status" = '2'
ORDER BY T0."U_NEXX_IdDocLeg" DESC