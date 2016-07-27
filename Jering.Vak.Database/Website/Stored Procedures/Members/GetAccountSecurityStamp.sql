CREATE PROCEDURE [Website].[GetAccountSecurityStamp]
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT CONVERT(NVARCHAR(64), [SecurityStamp]) AS SecurityStamp
	FROM [dbo].[Accounts]
	WHERE [AccountId] = @AccountId;
END

