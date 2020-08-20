CREATE TABLE [dbo].[settings]
(
    [Domain] VARCHAR(100) NOT NULL, 
    [Key] VARCHAR(50) NOT NULL, 
    [Value] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_settings] PRIMARY KEY ([Domain], [Key], [Value]) 
)
