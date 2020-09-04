CREATE OR ALTER PROCEDURE [dbo].[GetUser]
	@userID UniqueIdentifier=null,
	@FirstName nvarchar(80)=null,
	@LastName nvarchar(80)=null,
	@Email nvarchar(200)=null,
	@phoneNumber char(14)=null
AS
	IF @userID IS not null 
	BEGIN 
		Select * from dbo.visitor where id=@userID
	END
	ELSE 
	BEGIN 
		Select * from dbo.visitor 
		where (@FirstName is null or FirstName=@FirstName) and 
		(@LastName is null or LastName=@LastName) and 
		(@phoneNumber is null or PhoneNumber=@phoneNumber ) and 
		(@Email is null or Email=@Email)
	END
	