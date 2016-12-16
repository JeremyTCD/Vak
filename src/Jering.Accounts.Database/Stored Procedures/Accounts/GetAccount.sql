CREATE PROCEDURE [Accounts].[GetAccount]
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	SELECT *
	FROM [dbo].[Accounts]
	WHERE [dbo].[Accounts].[AccountId] = @AccountId;

	IF @@ROWCOUNT = 0
	BEGIN;
		THROW 51000, N'Invalid AccountId', 1;
	END;
END

