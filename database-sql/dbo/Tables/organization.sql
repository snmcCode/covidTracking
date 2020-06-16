CREATE TABLE [dbo].[organization] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (150) NOT NULL,
    [Address]       NVARCHAR (400) NULL,
    [ContactName]   NVARCHAR (120) NULL,
    [ContactNumber] CHAR (14)      NULL,
    [ContanctEmail] NVARCHAR(100) NULL, 
    CONSTRAINT [PK_organization] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO
SET IDENTITY_INSERT dbo.organization ON;

INSERT INTO dbo.organization(id,Name) Values(0,'online')

SET IDENTITY_INSERT dbo.organization OFF;