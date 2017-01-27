CREATE TABLE [dbo].[VakUnits] (
    [VakUnitId] INT            IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (256) NOT NULL,
    [AccountId] INT            NOT NULL,
    [RowVersion] ROWVERSION NOT NULL, 
    CONSTRAINT [PK_VakUnits] PRIMARY KEY CLUSTERED ([VakUnitId] ASC),
    CONSTRAINT [FK_dbo_VakUnits_dbo_Accounts] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Accounts] ([AccountId]) ON DELETE CASCADE
);

GO
CREATE NONCLUSTERED INDEX [IX_dbo_VakUnits_AccountId]
    ON [dbo].[VakUnits]([AccountId]ASC);