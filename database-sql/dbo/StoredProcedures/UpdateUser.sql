CREATE OR ALTER PROCEDURE [dbo].[UpdateUser]
	@id UniqueIdentifier,
	@FirstName nvarchar(80),
	@LastName nvarchar(80),
	@RegistrationOrg int,
	@Email nvarchar(200),
	@phoneNumber varchar(15),
	@Address nvarchar(200),
	@FamilyID UniqueIdentifier,
	@IsMale bit,
	@isVerified bit,
	@LastInfectionDate Date
AS
	Update dbo.visitor
	SET FirstName=COALESCE(@FirstName,FirstName),
	LastName=COALESCE(@LastName,LastName),
	RegistrationOrg=COALESCE(@RegistrationOrg,RegistrationOrg),
	Email=COALESCE(@Email,Email),
	phoneNumber=COALESCE(@phoneNumber,PhoneNumber),
	[Address]=COALESCE(@Address,[Address]),
	FamilyID=COALESCE(@FamilyID,FamilyID),
	IsMale=COALESCE(@IsMale,IsMale),
	IsVerified=COALESCE(@isVerified,IsVerified),
	LastInfectionDate=COALESCE(@LastInfectionDate,LastInfectionDate)
	WHERE id=@id
RETURN 0

