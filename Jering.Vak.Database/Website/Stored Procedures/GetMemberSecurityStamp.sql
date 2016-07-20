CREATE PROCEDURE [Website].[GetMemberSecurityStamp]
	@Id INT
AS
BEGIN
	SELECT [SecurityStamp]
	FROM [dbo].[Members] 
	WHERE [MemberId]=@Id  
END

