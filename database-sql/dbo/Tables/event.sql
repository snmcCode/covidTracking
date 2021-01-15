CREATE TABLE [dbo].[event] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [OrgId]          INT NOT NULL,
    [Name]       NVARCHAR (100) NOT NULL,
    [DateTime]   DateTime2(7) NOT NULL,
    [Hall] NVARCHAR(50) NULL,
    [Capacity] TINYINT NOT NULL,
    [IsPrivate] BIT NOT NULL DEFAULT(0),
    [GroupId] [UniqueIdentifier] NOT NULL DEFAULT(NewID()),
    [TargetAudience] SMALLINT NULL,
    CONSTRAINT [PK_event] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_organization] FOREIGN KEY (orgId) REFERENCES organization(Id)
);
GO
CREATE UNIQUE NONCLUSTERED INDEX Ix_event_orgId_Name_DateTime_Hall ON dbo.event
	(
	OrgId,
	Name,
	DateTime,
    Hall
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO