CREATE PROCEDURE dbo.InsertOrganization
	(
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

	INSERT INTO dbo.organization
	(
		Name, Address, ContactName, ContactNumber, ContactEmail, loginName, loginSecretHash
	)
	VALUES
	(
		@Name,
		@Address,
		@ContactName,
		@ContactNumber,
		@ContactEmail,
		@loginName,
		@loginSecretHash

	)
	SELECT Id, Name, Address, ContactName, ContactNumber, ContactEmail, loginName, loginSecretHash
	FROM dbo.organization
	WHERE (Id = SCOPE_IDENTITY())

	COMMIT