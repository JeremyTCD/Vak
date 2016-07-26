CREATE TABLE [dbo].[Roles] (
    [RoleId]   INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR (256)    NOT NULL,
    CONSTRAINT [PK_dbo_Roles] PRIMARY KEY CLUSTERED ([RoleId] ASC),
	CONSTRAINT [UQ_dbo_Roles_Name] UNIQUE ([Name])
);