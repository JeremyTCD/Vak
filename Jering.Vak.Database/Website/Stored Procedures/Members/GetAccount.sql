CREATE PROCEDURE [Website].[GetAccount]
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT AccountId, Username, Email, EmailConfirmed, TwoFactorEnabled, CONVERT(NVARCHAR(64), SecurityStamp) AS SecurityStamp
	FROM [dbo].[Accounts]
	WHERE [dbo].[Accounts].AccountId = @AccountId;
END

