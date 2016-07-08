CREATE TABLE [dbo].[UserRoles] (
    [UserID] INT NOT NULL,
    [RoleID] INT NOT NULL,
    CONSTRAINT [PK_dbo_UserRoles] PRIMARY KEY CLUSTERED ([UserID] ASC, [RoleID] ASC),
    CONSTRAINT [FK_dbo_UserRoles_dbo_Roles] FOREIGN KEY ([RoleID]) REFERENCES [dbo].[Roles] ([RoleID]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo_UserRoles_dbo_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserRoles_UserID]
    ON [dbo].[UserRoles]([UserID] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserRoles_RoleID]
    ON [dbo].[UserRoles]([RoleID] ASC);