CREATE TABLE [dbo].[UserRoles] (
    [UserId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    CONSTRAINT [PK_dbo_UserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo_UserRoles_dbo_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo_UserRoles_dbo_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserRoles_UserID]
    ON [dbo].[UserRoles]([UserId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserRoles_RoleID]
    ON [dbo].[UserRoles]([RoleId] ASC);