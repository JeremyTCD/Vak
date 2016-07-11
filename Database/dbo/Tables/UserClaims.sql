CREATE TABLE [dbo].[UserClaims] (
    [ClaimId]    INT NOT NULL,
    [UserId]    INT NOT NULL,
    CONSTRAINT [PK_dbo_UserClaims] PRIMARY KEY CLUSTERED ([ClaimId] ASC, [UserId] ASC),
	CONSTRAINT [FK_dbo_UserClaims_dbo_Claims] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[Claims] ([ClaimId]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo_UserClaims_dbo_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserClaims_UserID]
    ON [dbo].[UserClaims]([UserId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserClaims_ClaimID]
    ON [dbo].[UserClaims]([ClaimId] ASC);