CREATE TABLE [dbo].[Claims] (
    [ClaimId] INT IDENTITY(1,1) NOT NULL,
	[Type]    NVARCHAR (256) NOT NULL,
    [Value]	  NVARCHAR (256) NOT NULL,
    CONSTRAINT [PK_dbo_Claims] PRIMARY KEY CLUSTERED ([ClaimId] ASC),
	CONSTRAINT [UQ_dbo_Claims_Type_Value] UNIQUE ([Type], [Value])
);