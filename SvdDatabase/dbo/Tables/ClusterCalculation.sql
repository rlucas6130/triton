﻿CREATE TABLE [dbo].[ClusterCalculation] (
    [Id]           INT  NOT NULL IDENTITY,
    [ClusterCount] INT  NOT NULL,
    [GlobalSi]     REAL NOT NULL,
    [ClusterSi]    REAL NOT NULL,
    CONSTRAINT [PK_ClusterCalculation] PRIMARY KEY CLUSTERED ([Id] ASC)
);

