CREATE TABLE [dbo].[UMatrices] (
    [JobId]            INT             NOT NULL,
    [SerializedValues] VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_U_1] PRIMARY KEY CLUSTERED ([JobId] ASC),
    CONSTRAINT [FK_U_Job] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);

