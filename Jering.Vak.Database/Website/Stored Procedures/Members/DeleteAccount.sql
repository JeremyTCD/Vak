CREATE PROCEDURE [Website].[DeleteAccount]
	@AccountId INT
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	Delete from [dbo].[Accounts] where [AccountId] = @AccountId
END