CREATE TABLE [dbo].[Accounts] (
    [AccountId]		  	 INT IDENTITY(1,1) NOT NULL,
    [Username]		 	 NVARCHAR (256) CONSTRAINT [DF_dbo_Accounts_Username] DEFAULT NULL NULL,
	[SecurityStamp]      NVARCHAR (MAX) CONSTRAINT [DF_dbo_Accounts_SecurityStamp] DEFAULT NULL NULL,
    [PasswordHash]		 BINARY(32) NOT NULL,
	[Email]				 NVARCHAR (256)   NOT NULL,
	[EmailConfirmed]	 BIT CONSTRAINT [DF_dbo_Accounts_EmailConfirmed] DEFAULT 0 NOT NULL,
	[TwoFactorEnabled]   BIT CONSTRAINT [DF_dbo_Accounts_TwoFactorEnabled] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo_Accounts] PRIMARY KEY CLUSTERED ([AccountId] ASC),
	CONSTRAINT [UQ_dbo_Accounts_Email] UNIQUE ([Email])
);
