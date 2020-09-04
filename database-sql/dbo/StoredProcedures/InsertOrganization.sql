CREATE OR ALTER PROCEDURE dbo.InsertOrganization
	(
		@Name [nvarchar](150),
		@Address [nvarchar](400),
		@ContactName [nvarchar](120),
		@ContactNumber [char](14),
		@ContactEmail [nvarchar](100),
		@loginName [varchar](100),
		@loginSecretHash [varchar](100),
		@recordID int output
	)
AS
	

	Declare @insertedValue Table (Id int)

	Begin Try
		INSERT INTO dbo.organization
		(
			Name, Address, ContactName, ContactNumber, ContactEmail, loginName, loginSecretHash
		)Output inserted.Id into @insertedValue
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
	End Try
	Begin Catch 
		IF ERROR_NUMBER() = 2601 
		Begin 
			Insert into @insertedValue Select top 1 id from dbo.organization
			where [Name]=@Name
		End
		Else
			Throw;
	End Catch

	Select @recordId=id from @insertedValue

	