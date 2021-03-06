﻿CREATE TABLE [dbo].[ClusterCalculations] (
    [Id]                        INT           IDENTITY (1, 1) NOT NULL,
    [Created]                   DATETIME2 (7) CONSTRAINT [DF_ClusterCalculations_Created] DEFAULT (getdate()) NOT NULL,
    [Completed]                 DATETIME2 (7) NULL,
    [ClusterCount]              INT           NULL,
    [GlobalSi]                  REAL          NULL,
    [ClusterSi]                 REAL          NULL,
    [JobId]                     INT           NOT NULL,
    [MinimumClusterCount]       INT           CONSTRAINT [DF_ClusterCalculations_MinimumClusterCount] DEFAULT ((2)) NOT NULL,
    [MaximumClusterCount]       INT           CONSTRAINT [DF_ClusterCalculations_MaximumClusterCount] DEFAULT ((2)) NOT NULL,
    [IterationsPerCluster]      INT           CONSTRAINT [DF_ClusterCalculations_IterationsPerCluster] DEFAULT ((1)) NOT NULL,
    [MaximumOptimizationsCount] INT           CONSTRAINT [DF_ClusterCalculations_MaximumOptimizationsCount] DEFAULT ((100)) NOT NULL,
    [Status]                    INT           CONSTRAINT [DF_ClusterCalculations_Status] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_ClusterCalculation] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ClusterCalculation_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);











