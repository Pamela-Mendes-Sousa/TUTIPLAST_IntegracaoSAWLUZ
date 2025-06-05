INSERT INTO "@NEXX_LOG"(
	   "Code"
	 , "Name"
	 , "DocEntry"
	 , "CreateDate"
	 , "UpdateDate"
	 , "UpdateTime"
	 , "CreateTime"
	 , "U_NEXX_TipoDoc"
	 , "U_NEXX_IdDoc"
	 , "U_NEXX_DtInteg"
	 , "U_NEXX_Status"
	 , "U_NEXX_IdRet"
	 , "U_NEXX_MsgRet"
	 , "U_NEXX_JsonEnv"
	 , "U_NEXX_JsonRet"
	 , "U_NEXX_IdDocLeg"
) VALUES (
     "U_NEXX_LOG_S".NEXTVAL
   , '{0}'
   , "U_NEXX_LOG_S".NEXTVAL
   , CURRENT_DATE
   , CURRENT_DATE
   , TO_INT(LPAD(REPLACE(TO_VARCHAR(CURRENT_TIME), ':',''), 4))
   , TO_INT(LPAD(REPLACE(TO_VARCHAR(CURRENT_TIME), ':',''), 4))
   , '{1}'
   , '{2}'
   , CURRENT_DATE
   , '{4}'
   , '{5}'
   , '{6}'
   , '{7}'
   , '{8}'
   , '{9}'
)