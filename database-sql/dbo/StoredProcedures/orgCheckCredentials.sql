﻿CREATE PROCEDURE [dbo].[orgCheckCredentials]
	@loginName varchar(100), @loginSecretHash varchar(100)
AS
	Select id from dbo.organization where loginName=@loginName and loginSecretHash=@loginSecretHash

RETURN 0
