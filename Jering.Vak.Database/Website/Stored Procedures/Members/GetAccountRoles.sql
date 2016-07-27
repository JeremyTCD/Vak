CREATE PROCEDURE [Website].[GetAccountRoles]
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT Roles.RoleId, Roles.Name
	FROM [dbo].[Accounts] AS Accounts 
		INNER JOIN [dbo].[AccountRoles] AS AccountRoles
		ON Accounts.AccountId = AccountRoles.AccountId
		INNER JOIN [dbo].[Roles] AS Roles
		ON AccountRoles.RoleId = Roles.RoleId
	WHERE Accounts.AccountId = @AccountId  
END

