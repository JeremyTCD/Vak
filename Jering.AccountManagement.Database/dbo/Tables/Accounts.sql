CREATE TABLE [dbo].[Accounts] (
    [AccountId]		  	        INT IDENTITY(1,1) NOT NULL,
    [DisplayName]		        NVARCHAR (256) NULL,
	[SecurityStamp]             UNIQUEIDENTIFIER CONSTRAINT [DF_dbo_Accounts_SecurityStamp] DEFAULT NEWID() NOT NULL,
    [PasswordHash]		        BINARY(32) NOT NULL,
	[Salt]					    UNIQUEIDENTIFIER NOT NULL,
	[PasswordLastChanged]		DateTime2(0) CONSTRAINT [DF_dbo_Accounts_PasswordLastChanged] DEFAULT GETUTCDATE() NOT NULL,
	[Email]				        NVARCHAR (256)   NOT NULL,
	[EmailVerified]	            BIT CONSTRAINT [DF_dbo_Accounts_EmailVerified] DEFAULT 0 NOT NULL,
	[AlternativeEmail]			NVARCHAR (256) NULL,
	[AlternativeEmailVerified]	BIT CONSTRAINT [DF_dbo_Accounts_AlternativeEmailVerified] DEFAULT 0 NOT NULL,
	[TwoFactorEnabled]          BIT CONSTRAINT [DF_dbo_Accounts_TwoFactorEnabled] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo_Accounts] PRIMARY KEY CLUSTERED ([AccountId] ASC),
	CONSTRAINT [UQ_dbo_Accounts_Email] UNIQUE ([Email])
);
