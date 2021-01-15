CREATE OR ALTER PROCEDURE [dbo].[status_create]
(@statusName nvarchar(50),@bitValue int)
AS
INSERT dbo.[status](Name, BitValue)Values(@statusName,@bitValue)

