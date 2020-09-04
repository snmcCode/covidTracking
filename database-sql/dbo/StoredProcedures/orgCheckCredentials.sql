CREATE OR ALTER PROCEDURE [dbo].[orgCheckCredentials]
	@loginName varchar(100), @loginSecretHash varchar(100)
AS
	Select id,Name from dbo.organization where loginName=@loginName and loginSecretHash=@loginSecretHash

RETURN 0
