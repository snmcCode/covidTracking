CREATE OR ALTER PROCEDURE [dbo].[getUserForLogVisit]
	@userID UniqueIdentifier
AS
--Blocked user
    IF EXISTS(SELECT * FROM blocked WHERE VisitorId=@userID)
    BEGIN
        THROW 51985, 'BLOCKED_USER',1;
    END
	Select FirstName,LastName,PhoneNumber,IsMale,IsVerified from dbo.visitor where id=@userID
	