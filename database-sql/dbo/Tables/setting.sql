CREATE TABLE [dbo].[setting]
(
    [Domain] VARCHAR(100) NOT NULL, 
    [Key] VARCHAR(50) NOT NULL, 
    [Value] NVARCHAR(2000) NOT NULL, 
    CONSTRAINT [PK_settings] PRIMARY KEY ([Domain], [Key]) 
)
