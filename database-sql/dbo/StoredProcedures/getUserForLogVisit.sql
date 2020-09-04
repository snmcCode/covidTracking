CREATE OR ALTER PROCEDURE [dbo].[getUserForLogVisit]
	@userID UniqueIdentifier
AS
	Select FirstName,LastName,PhoneNumber,IsMale,IsVerified from dbo.visitor where id=@userID
	