CREATE TABLE [dbo].[Jobs] (
    [Id]            INT           IDENTITY (1, 1) NOT NULL,
    [DocumentCount] INT           NOT NULL,
    [Created]       SMALLDATETIME CONSTRAINT [DF_Table_1_CreationDate] DEFAULT (getdate()) NOT NULL,
    [Dimensions]    INT           CONSTRAINT [DF_Job_Dimensions] DEFAULT ((300)) NOT NULL,
    [Status]        INT           CONSTRAINT [DF_Job_Status] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Job] PRIMARY KEY CLUSTERED ([Id] ASC)
);

