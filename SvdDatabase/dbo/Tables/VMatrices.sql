CREATE TABLE [dbo].[VMatrices] (
    [JobId]            INT             NOT NULL,
    [SerializedValues] VARBINARY (MAX) NOT NULL,
    CONSTRAINT [PK_V_1] PRIMARY KEY CLUSTERED ([JobId] ASC),
    CONSTRAINT [FK_VMatrices_Jobs] FOREIGN KEY ([JobId]) REFERENCES [dbo].[Jobs] ([Id])
);



