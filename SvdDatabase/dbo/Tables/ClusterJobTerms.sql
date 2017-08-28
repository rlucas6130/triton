CREATE TABLE [dbo].[ClusterJobTerms] (
    [Id]                   INT IDENTITY (1, 1) NOT NULL,
    [JobId]                INT NOT NULL,
    [ClusterCalculationId] INT NOT NULL,
    [JobTermId]            INT NOT NULL,
    [ClusterId]            INT NOT NULL,
    CONSTRAINT [PK_ClusterJobTerms] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClusterJobTerms_ClusterCalculation] FOREIGN KEY ([ClusterCalculationId]) REFERENCES [dbo].[ClusterCalculations] ([Id]),
    CONSTRAINT [FK_ClusterJobTerms_Clusters] FOREIGN KEY ([ClusterId]) REFERENCES [dbo].[Clusters] ([Id]),
    CONSTRAINT [FK_ClusterJobTerms_Jobs] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);

