CREATE TABLE [dbo].[Members] (
    [MemberId]		  	 INT IDENTITY(1,1) NOT NULL,
    [Username]		 	 NVARCHAR (256) CONSTRAINT [DF_dbo_Members_Username] DEFAULT NULL NULL,
	[NormalizedUsername] NVARCHAR (256) CONSTRAINT [DF_dbo_Members_NormalizedUsername] DEFAULT NULL NULL,
    [PasswordHash]		 NVARCHAR (MAX) NOT NULL,
    [SecurityStamp]		 NVARCHAR (MAX) NOT NULL,
    [Email]              NVARCHAR (256)   NOT NULL,
	[NormalizedEmail]    NVARCHAR (256)   NOT NULL,
	[EmailConfirmed]	 BIT CONSTRAINT [DF_dbo_Members_EmailConfirmed] DEFAULT 0 NOT NULL,
	[TwoFactorEnabled]   BIT CONSTRAINT [DF_dbo_Members_TwoFactorEnabled] DEFAULT 1 NOT NULL,
	[LockoutEndDateUTC]  DATETIME2(7) CONSTRAINT [DF_dbo_Members_LockoutEndDateUTC] DEFAULT NULL NULL,
    [LockoutEnabled]     BIT CONSTRAINT [DF_dbo_Members_LockoutEnabled] DEFAULT 0 NOT NULL,
    [AccessFailedCount]  INT CONSTRAINT [DF_dbo_Members_AccessFailedCount] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo_Members] PRIMARY KEY CLUSTERED ([MemberId] ASC),
	CONSTRAINT [UQ_dbo_Members_NormalizedEmail] UNIQUE ([NormalizedEmail])
);
