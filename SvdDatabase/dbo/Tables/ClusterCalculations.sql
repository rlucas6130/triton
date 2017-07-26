CREATE TABLE [dbo].[ClusterCalculations] (
    [Id]           INT  IDENTITY (1, 1) NOT NULL,
    [ClusterCount] INT  NOT NULL,
    [GlobalSi]     REAL NOT NULL,
    [ClusterSi]    REAL NOT NULL,
    [JobId]        INT  NOT NULL,
    CONSTRAINT [PK_ClusterCalculation] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClusterCalculation_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);



