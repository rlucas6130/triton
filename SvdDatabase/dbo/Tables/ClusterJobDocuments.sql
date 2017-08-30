CREATE TABLE [dbo].[ClusterJobDocuments] (
    [Id]                   INT  IDENTITY (1, 1) NOT NULL,
    [JobId]                INT  NOT NULL,
    [ClusterCalculationId] INT  NOT NULL,
    [JobDocumentId]        INT  NOT NULL,
    [ClusterId]            INT  NOT NULL,
    [Si]                   REAL NOT NULL,
    CONSTRAINT [PK_ClusterJobDocuments] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClusterJobDocuments_ClusterCalculation] FOREIGN KEY ([ClusterCalculationId]) REFERENCES [dbo].[ClusterCalculations] ([Id]),
    CONSTRAINT [FK_ClusterJobDocuments_Clusters] FOREIGN KEY ([ClusterId]) REFERENCES [dbo].[Clusters] ([Id]),
    CONSTRAINT [FK_ClusterJobDocuments_JobDocument] FOREIGN KEY ([JobDocumentId]) REFERENCES [dbo].[JobDocuments] ([Id]),
    CONSTRAINT [FK_ClusterJobDocuments_Jobs] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);



