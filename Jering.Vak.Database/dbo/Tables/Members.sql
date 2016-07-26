CREATE TABLE [dbo].[Members] (
    [MemberId]		  	 INT IDENTITY(1,1) NOT NULL,
    [Username]		 	 NVARCHAR (256) CONSTRAINT [DF_dbo_Members_Username] DEFAULT NULL NULL,
	[SecurityStamp]      NVARCHAR (MAX) CONSTRAINT [DF_dbo_Members_SecurityStamp] DEFAULT NULL NULL,
    [PasswordHash]		 BINARY(32) NOT NULL,
	[Email]				 NVARCHAR (256)   NOT NULL,
	[EmailConfirmed]	 BIT CONSTRAINT [DF_dbo_Members_EmailConfirmed] DEFAULT 0 NOT NULL,
	[TwoFactorEnabled]   BIT CONSTRAINT [DF_dbo_Members_TwoFactorEnabled] DEFAULT 0 NOT NULL,
    CONSTRAINT [PK_dbo_Members] PRIMARY KEY CLUSTERED ([MemberId] ASC),
	CONSTRAINT [UQ_dbo_Members_Email] UNIQUE ([Email])
);
