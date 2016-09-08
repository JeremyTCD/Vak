CREATE PROCEDURE [Website].[UpdateAccountAlternativeEmail]
	@AccountId INT,
	@AlternativeEmail NVARCHAR(256)
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [SecurityStamp] = NEWID(), 
		[AlternativeEmail] = @AlternativeEmail
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
