CREATE PROCEDURE [Accounts].[CreateAccount]
	@PasswordHash NVARCHAR(84),  
	@Email NVARCHAR(256),
	@PasswordLastChanged DATETIMEOFFSET(0),
	@SecurityStamp UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	INSERT INTO [dbo].[Accounts] ([PasswordHash], [Email], [PasswordLastChanged], [SecurityStamp])
	OUTPUT INSERTED.*
	VALUES (@PasswordHash, @Email, @PasswordLastChanged, @SecurityStamp);
END