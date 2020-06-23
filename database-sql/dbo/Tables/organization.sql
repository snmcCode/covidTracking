CREATE TABLE [dbo].[organization] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (150) NOT NULL,
    [Address]       NVARCHAR (400) NULL,
    [ContactName]   NVARCHAR (120) NULL,
    [ContactNumber] CHAR (14)      NULL,
    [ContactEmail] NVARCHAR(100) NULL, 
    [loginName] VARCHAR(100) NULL, 
    [loginSecretHash] VARCHAR(100) NULL, 
    CONSTRAINT [PK_organization] PRIMARY KEY CLUSTERED ([Id] ASC)
);
