CREATE TABLE [dbo].[JobDocuments] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [JobId]        INT NOT NULL,
    [DocumentId]   INT NOT NULL,
    [OrdinalIndex] INT NOT NULL,
    CONSTRAINT [PK_JobDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_JobDocuments_Documents] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]),
    CONSTRAINT [FK_JobDocuments_Jobs] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_JobDocuments]
    ON [dbo].[JobDocuments]([Id] ASC, [DocumentId] ASC, [JobId] ASC, [OrdinalIndex] ASC);

