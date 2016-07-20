CREATE TABLE [dbo].[Roles] (
    [RoleId]   INT IDENTITY(1,1) NOT NULL,
    [RoleName] NVARCHAR (256)    NOT NULL,
    CONSTRAINT [PK_dbo_Roles] PRIMARY KEY CLUSTERED ([RoleId] ASC),
);