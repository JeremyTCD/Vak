CREATE PROCEDURE [Website].[DeleteMember]
	@MemberId INT
AS
BEGIN
	Delete from [dbo].[Members] where [MemberId] = @MemberId
END