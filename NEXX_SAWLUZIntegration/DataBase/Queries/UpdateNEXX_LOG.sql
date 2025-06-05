UPDATE "@NEXX_LOG" 
SET "U_NEXX_TipoDoc" = '{1}',
"U_NEXX_IdDoc"  = '{2}',
"U_NEXX_DtInteg" = current_date,
"U_NEXX_Status" = '{4}',
"U_NEXX_IdRet" = '{5}',
"U_NEXX_MsgRet" = '{6}',
"U_NEXX_JsonEnv" = '{7}',
"U_NEXX_JsonRet" = '{8}',
"U_NEXX_IdDocLeg" = '{9}',
"UpdateDate" = current_date,
"UpdateTime" = TO_INT(LPAD(REPLACE(TO_VARCHAR(current_Time),':',''),4))
WHERE "Name" = '{0}' 
  AND "U_NEXX_TipoDoc" = '{1}' 
  AND "U_NEXX_IdDoc" = '{2}'