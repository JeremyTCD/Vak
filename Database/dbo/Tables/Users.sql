CREATE TABLE [dbo].[Users] (
    [UserID]			INT IDENTITY(1,1) NOT NULL,
    [UserName]			NVARCHAR (256) NULL,
    [PasswordHash]		NVARCHAR (MAX) NULL,
    [SecurityStamp]		NVARCHAR (MAX) NULL,
    [Email]             NVARCHAR (256)   NULL,
	[EmailConfirmed]	BIT NOT NULL,
	[TwoFactorEnabled]  BIT NOT NULL,
	[LockoutEndDateUTC] DATETIME2         NOT NULL,
    [LockoutEnabled]    BIT         NOT NULL,
    [AccessFailedCount] INT              NOT NULL,
    CONSTRAINT [PK_dbo_Users] PRIMARY KEY CLUSTERED ([UserID] ASC)
);
