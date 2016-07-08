CREATE TABLE [dbo].[Claims] (
    [ClaimID]   INT IDENTITY(1,1) NOT NULL,
    [ClaimValue] NVARCHAR (MAX)    NOT NULL,
    CONSTRAINT [PK_dbo_Claims] PRIMARY KEY CLUSTERED ([ClaimID] ASC),
);