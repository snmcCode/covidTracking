﻿CREATE TABLE [dbo].[visitor] (
    [Id]              UNIQUEIDENTIFIER CONSTRAINT [DF_visitor_Id] DEFAULT (newid()) NOT NULL,
    [RegistrationOrg] INT              NULL,
    [FirstName]       NVARCHAR (80)    NOT NULL,
    [LastName]        NVARCHAR (80)    NOT NULL,
    [Email]           NVARCHAR (200)   NOT NULL,
    [PhoneNumber]     CHAR (14)        NOT NULL,
    [Address]         NVARCHAR (200)   NULL,
    [FamilyID]        UNIQUEIDENTIFIER NULL,
    [IsMale]          BIT              NOT NULL,
    CONSTRAINT [PK_visitors] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_visitors_organization] FOREIGN KEY ([RegistrationOrg]) REFERENCES [dbo].[organization] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX visitor_firstName_lastName_phone ON dbo.visitor
	(
	FirstName,
	LastName,
	PhoneNumber
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
