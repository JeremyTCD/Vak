﻿CREATE TABLE [dbo].[Accounts] (
    [AccountId]		  	        INT IDENTITY(1,1) NOT NULL,
    [DisplayName]		        NVARCHAR (256) CONSTRAINT [DF_dbo_Accounts_DisplayName] DEFAULT NULL NULL,
	[SecurityStamp]             UNIQUEIDENTIFIER NOT NULL,
    [PasswordHash]		        NVARCHAR (84) NOT NULL,
	[PasswordLastChanged]		DATETIMEOFFSET(0) NOT NULL,
	[Email]				        NVARCHAR (256)   NOT NULL,
	[EmailVerified]	            BIT CONSTRAINT [DF_dbo_Accounts_EmailVerified] DEFAULT 0 NOT NULL,
	[AltEmail]					NVARCHAR (256) CONSTRAINT [DF_dbo_Accounts_AltEmail] DEFAULT NULL NULL,
	[AltEmailVerified]			BIT CONSTRAINT [DF_dbo_Accounts_AltEmailVerified] DEFAULT 0 NOT NULL,
	[TwoFactorEnabled]          BIT CONSTRAINT [DF_dbo_Accounts_TwoFactorEnabled] DEFAULT 0 NOT NULL,
	[RowVersion]				ROWVERSION 
    CONSTRAINT [PK_dbo_Accounts] PRIMARY KEY CLUSTERED ([AccountId] ASC),
	CONSTRAINT [UQ_dbo_Accounts_Email] UNIQUE ([Email])
);
GO

CREATE UNIQUE INDEX [UQ_dbo_Accounts_DisplayName] on [dbo].[Accounts] ( [DisplayName] )
    WHERE [DisplayName] IS NOT NULL;
GO
