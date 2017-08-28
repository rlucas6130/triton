CREATE TABLE [dbo].[Clusters] (
    [Id]                     INT             IDENTITY (1, 1) NOT NULL,
    [JobId]                  INT             NOT NULL,
    [ClusterCalculationId]   INT             NOT NULL,
    [CenterVectorSerialized] VARBINARY (MAX) NULL,
    [Si]                     REAL            NULL,
    CONSTRAINT [PK_Clusters] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Clusters_ClusterCalculations] FOREIGN KEY ([ClusterCalculationId]) REFERENCES [dbo].[ClusterCalculations] ([Id]),
    CONSTRAINT [FK_Clusters_Jobs] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);

