CREATE TABLE [dbo].[UserClaims] (
    [ClaimID]    INT NOT NULL,
    [UserID]    INT NOT NULL,
    CONSTRAINT [PK_dbo_UserClaims] PRIMARY KEY CLUSTERED ([ClaimID] ASC, [UserID] ASC),
	CONSTRAINT [FK_dbo_UserClaims_dbo_Claims] FOREIGN KEY ([ClaimID]) REFERENCES [dbo].[Claims] ([ClaimID]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo_UserClaims_dbo_Users] FOREIGN KEY ([UserID]) REFERENCES [dbo].[Users] ([UserID]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserClaims_UserID]
    ON [dbo].[UserClaims]([UserID] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_UserClaims_ClaimID]
    ON [dbo].[UserClaims]([ClaimID] ASC);