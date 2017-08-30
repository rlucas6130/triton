CREATE TABLE [dbo].[JobTerms] (
    [Id]           INT IDENTITY (1, 1) NOT NULL,
    [JobId]        INT NOT NULL,
    [TermId]       INT NOT NULL,
    [OrdinalIndex] INT NOT NULL,
    CONSTRAINT [PK_JobTerms] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_JobTerms_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id]),
    CONSTRAINT [FK_JobTerms_Terms] FOREIGN KEY ([TermId]) REFERENCES [dbo].[Terms] ([Id])
);





