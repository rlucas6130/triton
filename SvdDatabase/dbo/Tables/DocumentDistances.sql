CREATE TABLE [dbo].[DocumentDistances] (
    [JobId]            INT  NOT NULL,
    [SourceDocumentId] INT  NOT NULL,
    [TargetDocumentId] INT  NOT NULL,
    [Distance]         REAL NOT NULL,
    CONSTRAINT [PK_DocumentDistances] PRIMARY KEY CLUSTERED ([JobId] ASC, [SourceDocumentId] ASC, [TargetDocumentId] ASC),
    CONSTRAINT [FK_DocumentDistances_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id]),
    CONSTRAINT [FK_SourceDocument_Documents] FOREIGN KEY ([SourceDocumentId]) REFERENCES [dbo].[Documents] ([Id]),
    CONSTRAINT [FK_TargetDocument_Documents] FOREIGN KEY ([TargetDocumentId]) REFERENCES [dbo].[Documents] ([Id])
);



