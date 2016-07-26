CREATE PROCEDURE [Website].[DeleteMemberRole]
	@RoleId INT,
	@MemberId INT
AS
BEGIN
	Delete from [dbo].[MemberRoles] where [RoleId] = @RoleId AND [MemberId] = @MemberId
END