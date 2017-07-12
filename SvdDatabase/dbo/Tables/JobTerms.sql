CREATE TABLE [dbo].[JobTerms] (
    [JobId]        INT NOT NULL,
    [TermId]       INT NOT NULL,
    [OrdinalIndex] INT NOT NULL,
    CONSTRAINT [PK_JobTerms] PRIMARY KEY CLUSTERED ([JobId] ASC, [TermId] ASC, [OrdinalIndex] ASC),
    CONSTRAINT [FK_JobTerms_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id]),
    CONSTRAINT [FK_JobTerms_Terms] FOREIGN KEY ([TermId]) REFERENCES [dbo].[Terms] ([Id])
);



