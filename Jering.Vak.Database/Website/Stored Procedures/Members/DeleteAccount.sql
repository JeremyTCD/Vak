CREATE PROCEDURE [Website].[DeleteAccount]
	@AccountId INT
AS
BEGIN
	Delete from [dbo].[Accounts] where [AccountId] = @AccountId
END