CREATE PROCEDURE [dbo].[RegisterUser]
	@FirstName nvarchar(80),
	@LastName nvarchar(80),
	@RegistrationOrg int,
	@Email nvarchar(200),
	@phoneNumber char(14),
	@Address nvarchar(200),
	@FamilyID UniqueIdentifier,
	@IsMale bit
AS
IF @RegistrationOrg is null 
Set @RegistrationOrg=0

Insert into dbo.visitor
(RegistrationOrg,FirstName,LastName,Email,PhoneNumber,[Address],FamilyID,IsMale)
Values (@RegistrationOrg,@FirstName,@LastName,@Email,@phoneNumber,@Address,@FamilyID,@IsMale)
