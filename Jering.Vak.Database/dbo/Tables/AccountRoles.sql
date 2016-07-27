CREATE TABLE [dbo].[AccountRoles] (
    [AccountId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    CONSTRAINT [PK_dbo_AccountRoles] PRIMARY KEY CLUSTERED ([AccountId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo_AccountRoles_dbo_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo_AccountRoles_dbo_Accounts] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Accounts] ([AccountId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_AccountRoles_AccountID]
    ON [dbo].[AccountRoles]([AccountId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_AccountRoles_RoleID]
    ON [dbo].[AccountRoles]([RoleId] ASC);
