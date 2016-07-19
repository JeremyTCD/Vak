CREATE PROCEDURE [Website].[GetMemberUsername]
	@Id int
AS
BEGIN
	Select [Username] from [dbo].[Members] where [MemberId]=@Id;
END
