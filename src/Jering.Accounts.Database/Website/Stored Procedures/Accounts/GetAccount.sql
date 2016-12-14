CREATE PROCEDURE [Website].[GetAccount]
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT *
	FROM [dbo].[Accounts]
	WHERE [dbo].[Accounts].[AccountId] = @AccountId;
END

