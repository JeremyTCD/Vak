CREATE TABLE [dbo].[Tags] (
    [Value] NVARCHAR (256) NOT NULL,
    [TagId] INT            NOT NULL,
    CONSTRAINT [PK_Tags] PRIMARY KEY CLUSTERED ([TagId] ASC)
);

