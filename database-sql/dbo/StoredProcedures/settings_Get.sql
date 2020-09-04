CREATE OR ALTER PROCEDURE [dbo].[settings_Get]
	@domain varchar(100)='Default',
	@key varchar(50)
AS
	SELECT [Value] FROM settings WHERE [Domain]=@domain AND [Key]=@key

