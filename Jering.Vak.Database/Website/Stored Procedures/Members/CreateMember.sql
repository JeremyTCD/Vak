CREATE PROCEDURE [Website].[CreateMember]
	@Password NVARCHAR(256),  
	@Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

	DECLARE @PasswordHash BINARY(32) = HASHBYTES(N'SHA2_256', @Password + @Email);

    INSERT INTO [dbo].[Members] ([PasswordHash], [Email])
	OUTPUT INSERTED.*
	VALUES (@PasswordHash, @Email)
END