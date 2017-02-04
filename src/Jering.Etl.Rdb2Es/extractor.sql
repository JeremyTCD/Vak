SELECT CHANGE_TRACKING_CURRENT_VERSION() as synchronization_version, 
	Convert(BIGINT, vu1.RowVersion) as version,
	vu1.VakUnitId as id,
	(SELECT vu1.Name AS name, 
		 JSON_QUERY(dbo.ufnToRawJsonArray((SELECT Tags.Value
		 FROM Tags join VakUnitTags on Tags.TagId = VakUnitTags.TagId
		 WHERE vu1.VakUnitId = VakUnitTags.VakUnitId
		 FOR JSON PATH), 'Value')) as tags
	FROM  VakUnits as vu2
	WHERE vu1.VakUnitId = vu2.VakUnitId
	FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER ) as source
FROM  VakUnits as vu1