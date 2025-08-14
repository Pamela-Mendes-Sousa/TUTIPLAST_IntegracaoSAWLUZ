SELECT IFNULL(MAX(ORDR."DocEntry"),0) "DocEntry", IFNULL(MAX(ORDR."DocNum"),0) "DocNum"
FROM 
	RDR1 
	INNER JOIN ORDR ON RDR1."DocEntry" = ORDR."DocEntry"
WHERE 
	ORDR."CANCELED" = 'N'
	AND TO_VARCHAR("U_CallDelivery") in ({0})