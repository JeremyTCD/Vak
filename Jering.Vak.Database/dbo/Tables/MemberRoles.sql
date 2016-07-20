CREATE TABLE [dbo].[MemberRoles] (
    [MemberId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    CONSTRAINT [PK_dbo_MemberRoles] PRIMARY KEY CLUSTERED ([MemberId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_dbo_MemberRoles_dbo_Roles] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE CASCADE,
    CONSTRAINT [FK_dbo_MemberRoles_dbo_Members] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([MemberId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_MemberRoles_MemberID]
    ON [dbo].[MemberRoles]([MemberId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_MemberRoles_RoleID]
    ON [dbo].[MemberRoles]([RoleId] ASC);
