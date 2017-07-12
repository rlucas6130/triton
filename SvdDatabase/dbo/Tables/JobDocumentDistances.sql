CREATE TABLE [dbo].[JobDocumentDistances] (
    [JobId]               INT  NOT NULL,
    [SourceJobDocumentId] INT  NOT NULL,
    [TargetJobDocumentId] INT  NOT NULL,
    [Distance]            REAL NOT NULL,
    CONSTRAINT [PK_JobDocumentDistances] PRIMARY KEY CLUSTERED ([JobId] ASC, [SourceJobDocumentId] ASC, [TargetJobDocumentId] ASC),
    CONSTRAINT [FK_JobDocumentDistances_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id]),
    CONSTRAINT [FK_JobDocumentDistances_SourceJobDocument] FOREIGN KEY ([SourceJobDocumentId]) REFERENCES [dbo].[JobDocuments] ([Id]),
    CONSTRAINT [FK_JobDocumentDistances_TargetJobDocument] FOREIGN KEY ([TargetJobDocumentId]) REFERENCES [dbo].[JobDocuments] ([Id])
);

