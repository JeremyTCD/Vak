CREATE TABLE [dbo].[Users] (
    [UserID]			INT IDENTITY(1,1) NOT NULL,
    [UserName]			NVARCHAR (256) NULL,
    [PasswordHash]		NVARCHAR (MAX) NULL,
    [SecurityStamp]		NVARCHAR (MAX) NULL,
    [Email]             NVARCHAR (256)   NULL,
	[EmailConfirmed]	BIT NOT NULL,
	[TwoFactorEnabled]  BIT NOT NULL,
	[LockoutEndDateUTC] DATETIME2 CONSTRAINT [DF_dbo_Users_LockoutEndDateUTC] DEFAULT NULL NULL,
    [LockoutEnabled]    BIT         NOT NULL,
    [AccessFailedCount] INT CONSTRAINT [DF_dbo_Users_AccessFailedCount] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo_Users] PRIMARY KEY CLUSTERED ([UserID] ASC)
);
