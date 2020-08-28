CREATE PROCEDURE [dbo].[getUserForLogVisit]
	@userID UniqueIdentifier
AS
	Select FirstName,LastName,PhoneNumber,IsMale from dbo.visitor where id=@userID
	