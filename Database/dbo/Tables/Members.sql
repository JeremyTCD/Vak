CREATE TABLE [dbo].[Members] (
    [MemberId]			INT IDENTITY(1,1) NOT NULL,
    [Username]			NVARCHAR (256) NULL,
    [PasswordHash]		NVARCHAR (MAX) NULL,
    [SecurityStamp]		NVARCHAR (MAX) NULL,
    [Email]             NVARCHAR (256)   NULL,
	[EmailConfirmed]	BIT NOT NULL,
	[TwoFactorEnabled]  BIT NOT NULL,
	[LockoutEndDateUTC] DATETIME2(7) CONSTRAINT [DF_dbo_Members_LockoutEndDateUTC] DEFAULT NULL NULL,
    [LockoutEnabled]    BIT         NOT NULL,
    [AccessFailedCount] INT CONSTRAINT [DF_dbo_Members_AccessFailedCount] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo_Members] PRIMARY KEY CLUSTERED ([MemberId] ASC)
);
