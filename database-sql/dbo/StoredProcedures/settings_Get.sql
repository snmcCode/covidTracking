CREATE OR ALTER PROCEDURE [dbo].[settings_Get]
	@domain varchar(100)='Default',
	@key varchar(50)
AS
	Declare @value NVARCHAR(2000)
	SELECT @value=[Value] FROM setting WHERE [Domain]=@domain AND [Key]=@key
	IF @value IS NULL
		SELECT @value=[Value] FROM setting WHERE [Domain]='Default' AND [Key]=@key
	
	SELECT @value as [Value];
