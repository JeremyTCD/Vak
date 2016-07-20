CREATE PROCEDURE [Website].[GetMemberPasswordHash]
	@Id INT
AS
BEGIN
	Select [PasswordHash] from [dbo].[Members] where [MemberId]=@Id;
END
