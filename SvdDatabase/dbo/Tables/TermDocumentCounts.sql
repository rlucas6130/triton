CREATE TABLE [dbo].[TermDocumentCounts] (
    [DocumentId] INT NOT NULL,
    [TermId]     INT NOT NULL,
    [Count]      INT CONSTRAINT [DF_TermDocumentCounts_Count] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TermDocumentCounts] PRIMARY KEY CLUSTERED ([DocumentId] ASC, [TermId] ASC),
    CONSTRAINT [FK_TermDocumentCounts_Document] FOREIGN KEY ([DocumentId]) REFERENCES [dbo].[Documents] ([Id]),
    CONSTRAINT [FK_TermDocumentCounts_Terms] FOREIGN KEY ([TermId]) REFERENCES [dbo].[Terms] ([Id])
);

