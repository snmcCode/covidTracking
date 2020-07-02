CREATE TABLE [dbo].[organizationDoor](
	[orgID] [int] NOT NULL,
	[Door] [varchar](50) NOT NULL,
	CONSTRAINT [FK_organization_organizatoinDoor] FOREIGN KEY ([orgID]) REFERENCES [dbo].[organization] ([Id])
) 

ON [PRIMARY]