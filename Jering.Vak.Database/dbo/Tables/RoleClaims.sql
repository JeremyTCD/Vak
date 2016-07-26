CREATE TABLE [dbo].[RoleClaims] (
    [ClaimId]    INT NOT NULL,
    [RoleId]    INT NOT NULL,
    CONSTRAINT [PK_dbo_RoleClaims] PRIMARY KEY CLUSTERED ([ClaimId] ASC, [RoleId] ASC),
	CONSTRAINT [FK_dbo_RoleClaims_dbo_Claims] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[Claims] ([ClaimId]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo_RoleClaims_dbo_Members] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[Roles] ([RoleId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_RoleClaims_RoleId]
    ON [dbo].[RoleClaims]([RoleId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_RoleClaims_ClaimId]
    ON [dbo].[RoleClaims]([ClaimId] ASC);
