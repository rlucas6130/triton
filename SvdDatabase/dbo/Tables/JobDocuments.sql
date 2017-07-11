CREATE TABLE [dbo].[JobDocuments] (
    [JobId]        INT NOT NULL,
    [DocumentId]   INT NOT NULL,
    [OrdinalIndex] INT NOT NULL,
    CONSTRAINT [PK_JobDocuments] PRIMARY KEY CLUSTERED ([JobId] ASC, [DocumentId] ASC),
    CONSTRAINT [FK_JobDocuments_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]),
    CONSTRAINT [FK_JobDocuments_Jobs] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);

