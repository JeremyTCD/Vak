CREATE PROCEDURE [Website].[GetMemberId]
	@Username NVARCHAR(256)
AS
BEGIN
	Select [MemberId] from [dbo].[Members] where [Username]=@Username;
END
