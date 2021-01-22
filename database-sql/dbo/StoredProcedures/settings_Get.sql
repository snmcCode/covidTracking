CREATE OR ALTER PROCEDURE [dbo].[settings_Get]
	@domain varchar(100)='Default',
	@key varchar(50)
AS
	SELECT [Value] FROM setting WHERE [Domain]=@domain AND [Key]=@key

