CREATE TABLE [dbo].[Terms] (
    [Id]    INT            IDENTITY (1, 1) NOT NULL,
    [Value] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Terms] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Value]
    ON [dbo].[Terms]([Value] ASC);

