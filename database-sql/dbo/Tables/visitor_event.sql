CREATE TABLE [dbo].[visitor_event] (
    [Id]        UNIQUEIDENTIFIER CONSTRAINT [DF_visitor_event_Id] DEFAULT (newid()) NOT NULL,
    [EventId]          INT NOT NULL,
    [VisitorId]       UNIQUEIDENTIFIER NOT NULL,
    [EventGroupid]  UNIQUEIDENTIFIER NOT NULL
    CONSTRAINT [PK_visitor_event] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_event] FOREIGN KEY (EventId) REFERENCES event(Id) ON DELETE CASCADE, 
    CONSTRAINT [FK_visitor] FOREIGN KEY (VisitorId) REFERENCES visitor(Id),
);
GO
CREATE UNIQUE NONCLUSTERED INDEX visitor_event_eventId_visitorId ON dbo.visitor_event
	(
    VisitorId,
    EventGroupid
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO