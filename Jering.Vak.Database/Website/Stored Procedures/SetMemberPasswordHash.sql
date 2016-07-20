CREATE PROCEDURE [Website].[SetMemberPasswordHash]
	@Id INT,
	@PasswordHash NVARCHAR(MAX)
AS
BEGIN
	UPDATE [dbo].[Members] 
	SET [PasswordHash] = @PasswordHash
	WHERE [MemberId]=@Id  
END

