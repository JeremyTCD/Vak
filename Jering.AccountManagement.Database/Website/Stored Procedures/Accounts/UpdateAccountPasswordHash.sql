CREATE PROCEDURE [Website].[UpdateAccountPasswordHash]
	@AccountId INT,
	@Password NVARCHAR(256)
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [SecurityStamp] = NEWID(), 
		[Salt] = NEWID(),
		[PasswordLastChanged] = GETUTCDATE(),  	
		[PasswordHash] = HASHBYTES(N'SHA2_256', @Password + CONVERT(char(36), [Salt]))
	WHERE AccountId = @AccountId;

	SELECT @@ROWCOUNT;
END
