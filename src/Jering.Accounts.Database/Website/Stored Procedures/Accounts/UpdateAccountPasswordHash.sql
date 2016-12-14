CREATE PROCEDURE [Website].[UpdateAccountPasswordHash]
	@AccountId INT,
	@PasswordHash NVARCHAR(84)
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [SecurityStamp] = NEWID(), 
		[PasswordLastChanged] = GETUTCDATE(),  	
		[PasswordHash] = @PasswordHash
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
