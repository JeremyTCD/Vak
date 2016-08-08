CREATE PROCEDURE [Website].[GetAccountSecurityStamp]
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT [SecurityStamp]
	FROM [dbo].[Accounts]
	WHERE [AccountId] = @AccountId;
END

