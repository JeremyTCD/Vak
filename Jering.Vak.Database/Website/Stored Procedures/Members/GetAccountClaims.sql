CREATE PROCEDURE [Website].[GetAccountClaims]
	@AccountId INT
AS
BEGIN
	SELECT Claims.ClaimId, Claims.Type, Claims.Value
	FROM [dbo].[Accounts] AS Accounts 
		INNER JOIN [dbo].[AccountClaims] AS AccountClaims
		ON Accounts.AccountId = AccountClaims.AccountId
		INNER JOIN [dbo].[Claims] AS Claims
		ON AccountClaims.ClaimId = Claims.ClaimId
	WHERE Accounts.AccountId = @AccountId  
END

