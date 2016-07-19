CREATE TABLE [dbo].[MemberClaims] (
    [ClaimId]    INT NOT NULL,
    [MemberId]    INT NOT NULL,
    CONSTRAINT [PK_dbo_MemberClaims] PRIMARY KEY CLUSTERED ([ClaimId] ASC, [MemberId] ASC),
	CONSTRAINT [FK_dbo_MemberClaims_dbo_Claims] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[Claims] ([ClaimId]) ON DELETE CASCADE,
	CONSTRAINT [FK_dbo_MemberClaims_dbo_Members] FOREIGN KEY ([MemberId]) REFERENCES [dbo].[Members] ([MemberId]) ON DELETE CASCADE
);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_MemberClaims_MemberID]
    ON [dbo].[MemberClaims]([MemberId] ASC);

GO

CREATE NONCLUSTERED INDEX [IX_dbo_MemberClaims_ClaimID]
    ON [dbo].[MemberClaims]([ClaimId] ASC);
