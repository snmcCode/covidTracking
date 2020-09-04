CREATE OR ALTER PROCEDURE [dbo].[orgAddCredentials]
	@orgID int,@loginName varchar(100), @loginSecretHash varchar(100)
AS
	Update dbo.organization set loginName=@loginName, loginSecretHash=@loginSecretHash
	where Id=@orgID

	IF @@ROWCOUNT <> 1
	 Return -1

RETURN 0
