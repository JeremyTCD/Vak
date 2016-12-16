CREATE PROCEDURE [Accounts].[UpdatePasswordHash]
	@AccountId INT,
	@PasswordHash NVARCHAR(256),
	@PasswordLastChanged DATETIMEOFFSET(0),
	@RowVersion ROWVERSION = NULL,
	@SecurityStamp UNIQUEIDENTIFIER
AS
BEGIN	
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	UPDATE [dbo].[Accounts]
	SET [PasswordHash] = @PasswordHash,
		[PasswordLastChanged] = @PasswordLastChanged,
		[SecurityStamp] = @SecurityStamp
	WHERE [AccountId] = @AccountId AND (@RowVersion IS NULL OR [RowVersion]=@RowVersion);

	IF @@ROWCOUNT = 0
	BEGIN;
		THROW 51000, N'Invalid RowVersion or AccountId', 1;
	END;
END;
