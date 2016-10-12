CREATE TABLE [dbo].[AccountClaims] (
    [ClaimId]    INT NOT NULL,
    [AccountId]    INT NOT NULL,
    CONSTRAINT [PK_dbo_AccountClaims] PRIMARY KEY CLUSTERED ([ClaimId] ASC, [AccountId] ASC),
	CONSTRAINT [FK_dbo_AccountClaims_dbo_Claims] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[Claims] ([ClaimId]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo_AccountClaims_dbo_Accounts] FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Accounts] ([AccountId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_AccountClaims_AccountID]
    ON [dbo].[AccountClaims]([AccountId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_AccountClaims_ClaimID]
    ON [dbo].[AccountClaims]([ClaimId] ASC);
