CREATE OR ALTER PROCEDURE dbo.getOrganization
		@Id [int]=NULL
AS
	SET NOCOUNT ON
	SET XACT_ABORT ON
	
	BEGIN TRANSACTION

	SELECT Id, Name, Address, ContactName, ContactNumber, ContactEmail, loginName, loginSecretHash
	FROM dbo.organization
	WHERE (Id = @Id OR @Id IS NULL)

	COMMIT
GO