CREATE OR ALTER PROCEDURE dbo.UpdateOrganization
	(
		@Id [int],
		@Name [nvarchar](150),
		@Address [nvarchar](400),
		@ContactName [nvarchar](120),
		@ContactNumber [char](14),
		@ContactEmail [nvarchar](100),
		@loginName [varchar](100),
		@loginSecretHash [varchar](100)
	)
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON
	
	BEGIN TRANSACTION
		UPDATE dbo.organization
		SET  Name = COALESCE(@Name,Name),
		Address = COALESCE(@Address,[Address]),
		ContactName = COALESCE(@ContactName,ContactName),
		ContactNumber = COALESCE(@ContactNumber,ContactNumber),
		ContactEmail = COALESCE(@ContactEmail,ContactEmail),
		loginName = COALESCE(@loginName,loginName),
		loginSecretHash = COALESCE(@loginSecretHash,loginSecretHash)
		WHERE (Id = @Id )
	COMMIT
