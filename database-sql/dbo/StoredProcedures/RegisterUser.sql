CREATE OR ALTER PROCEDURE [dbo].[RegisterUser]
	@FirstName nvarchar(80),
	@LastName nvarchar(80),
	@RegistrationOrg int,
	@Email nvarchar(200),
	@phoneNumber varchar(15),
	@Address nvarchar(200),
	@FamilyID UniqueIdentifier,
	@IsMale bit,
	@IsVerified bit,
	@recordID UniqueIdentifier output
AS
IF @RegistrationOrg is null 
Set @RegistrationOrg=0

Declare @insertedValue Table(Id UniqueIdentifier)
Begin Try
	Insert dbo.visitor(RegistrationOrg,FirstName,LastName,Email,PhoneNumber,[Address],FamilyID,IsMale,IsVerified)
	OUTPUT inserted.Id into @insertedValue 
	Values (@RegistrationOrg,@FirstName,@LastName,@Email,@phoneNumber,@Address,@FamilyID,@IsMale,@IsVerified)
End Try
Begin Catch
	IF ERROR_NUMBER() = 2601 or ERROR_NUMBER()=2627 
	Begin 
		Insert into @insertedValue Select top 1 id from dbo.visitor 
		where FirstName=@FirstName and LastName=@LastName and PhoneNumber=@phoneNumber
	End
	Else
		Throw;
End Catch 

Select @recordID=id from @insertedValue