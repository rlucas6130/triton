CREATE TABLE [dbo].[Jobs] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [DocumentCount] INT           NOT NULL,
    [Created]       DATETIME2 (7) CONSTRAINT [DF_Table_1_CreationDate] DEFAULT (sysdatetime()) NOT NULL,
    [Dimensions]    INT           CONSTRAINT [DF_Job_Dimensions] DEFAULT ((300)) NOT NULL,
    [Status]        INT           CONSTRAINT [DF_Job_Status] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Job] PRIMARY KEY CLUSTERED ([Id] ASC)
);







