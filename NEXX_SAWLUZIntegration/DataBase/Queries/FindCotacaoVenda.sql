SELECT IFNULL(MAX(OQUT."DocEntry"),0) "DocEntry", IFNULL(MAX(OQUT."DocNum"),0) "DocNum"
FROM 
	QUT1 
	INNER JOIN OQUT ON QUT1."DocEntry" = OQUT."DocEntry"
WHERE 
	OQUT."DocDate" between TO_DATE(YEAR(CURRENT_DATE)||'-'||MONTH(CURRENT_DATE)||'-01') and 
		add_days(Add_MONTHs(TO_DATE(YEAR(CURRENT_DATE)||'-'||MONTH(CURRENT_DATE)||'-01'),1),-1)
	and OQUT."CANCELED" = 'N'
	AND OQUT."DocStatus" = 'O'