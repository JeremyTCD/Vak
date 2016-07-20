CREATE PROCEDURE [Website].[GetMemberByNormalizedUsername]
	@NormalizedUsername NVARCHAR(256)
AS
BEGIN
	SELECT *
	FROM [dbo].[Members] 
	WHERE [NormalizedUsername]=@NormalizedUsername  
END

