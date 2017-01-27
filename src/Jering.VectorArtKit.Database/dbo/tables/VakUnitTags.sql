CREATE TABLE [dbo].[VakUnitTags] (
    [VakUnitId] INT NOT NULL,
    [TagId]     INT NOT NULL,
    CONSTRAINT [PK_VakUnitTags] PRIMARY KEY CLUSTERED ([VakUnitId] ASC, [TagId] ASC),
    CONSTRAINT [FK_dbo_VakUnitTags_dbo_Tags] FOREIGN KEY ([TagId]) REFERENCES [dbo].[Tags] ([TagId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo_VakUnitTags_dbo_VakUnits] FOREIGN KEY ([VakUnitId]) REFERENCES [dbo].[VakUnits] ([VakUnitId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_dbo_VakUnitTags_VakUnitId]
    ON [dbo].[VakUnitTags]([VakUnitId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_dbo_VakUnitTags_TagId]
    ON [dbo].[VakUnitTags]([TagId] ASC);