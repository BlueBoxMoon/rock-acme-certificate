BEGIN TRANSACTION

-- Migration 8

-- The legacy Acme Challenge page, block, and block type are removed together with
-- the Migration 1 records below (the deletes are idempotent, so they succeed whether
-- or not Migration 8 has already removed them).

-- Migration 7

-- The Obsidian blocks reuse the same BlockType rows as the WebForms blocks (their
-- Guids were changed in place), so both the WebForms and Obsidian Guids are removed
-- with the Migration 1 block types below. The block EntityType rows are removed at
-- the end of this script, after the block types, because BlockType references EntityType.

-- Migration 6

DELETE
FROM [DefinedValue]
WHERE [Guid] = 'FFF39AB2-95E4-4C5F-930D-562CB88D5456'

-- Migration 5

-- No changes required

-- Migration 4

DELETE
FROM [ServiceJob]
WHERE [Guid] = 'B8CC5256-4DFB-45A1-B006-23656F0F0C67'

-- Migration 3

DELETE
FROM [Attribute]
WHERE [Guid] = 'ABA2024D-D3F5-4EBC-8974-2D4C69F28BC7'

-- Migration 2

-- No changes required

-- Migration 1

DELETE
FROM [Page]
WHERE [Guid] = '0846B6D8-5C6B-4783-BF2C-87C4D6F180C8'

DELETE
FROM [Page]
WHERE [Guid] = '21DF7462-F395-456D-9F40-90F1CE20DE9E'

DELETE
FROM [Page]
WHERE [Guid] = '10B795B3-F88C-490C-B7DC-2AAF3347B91B'

-- Acme Challenge (WebForms only; also removed by Migration 8)
DELETE
FROM [BlockType]
WHERE [Guid] = '0DE7B05B-DCB4-4A44-A8B5-F9B15E92355B'

-- Acme Certificate Detail (WebForms and Obsidian)
DELETE
FROM [BlockType]
WHERE [Guid] IN ( 'F2BB435E-53C6-46E8-867E-0807A7DD9691', 'E052A496-64E2-4BF5-8EA9-7FE4DFDF0D68' )

-- Acme Certificates list (WebForms and Obsidian)
DELETE
FROM [BlockType]
WHERE [Guid] IN ( 'A08FB359-E68E-46FC-85D6-53726EAE6E23', '93FC8A02-E0F5-485F-898B-57E1936245FA' )

-- Acme Config (WebForms and Obsidian)
DELETE
FROM [BlockType]
WHERE [Guid] IN ( '4F6285D0-6871-4C63-8829-A7F0F608E2D8', '1F324FFC-178D-4304-9B18-B1D3F3C2C854' )

DELETE
FROM [Attribute]
WHERE [Guid] = 'B0EFF9B2-9A6F-4EA9-AA10-080F8EB23398'

DELETE
FROM [Attribute]
WHERE [Guid] = 'CD6ACBB4-84A9-45D7-8DDD-853727AF1F03'

DELETE
FROM [Attribute]
WHERE [Guid] = '77A91E28-8210-433C-9BB3-0FE20ADD81CC'

DELETE
FROM [Attribute]
WHERE [Guid] = 'C3A28E09-E8A3-4CF9-8255-1645F3C21AFB'

DELETE
FROM [Attribute]
WHERE [Guid] = '64AC7F61-A968-48B6-BF94-66581F5CABFE'

DELETE
FROM [Attribute]
WHERE [Guid] = '5A7CDD38-5D90-413C-BCD4-2A4AE6903711'

DELETE
FROM [Group]
WHERE [GroupTypeId] = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '92BEDEAA-79BA-40CB-AE5E-DE63BB0B0381')

DELETE
FROM [GroupType]
WHERE [Guid] = '92BEDEAA-79BA-40CB-AE5E-DE63BB0B0381'

-- Migration 7 (continued)

-- Remove the Obsidian block entity types now that their block types are gone.
DELETE
FROM [EntityType]
WHERE [Guid] IN (
    '0C36975A-2597-47D5-A94C-D1D990507596', -- AcmeConfig
    '05D7EFC2-7442-4C13-8F5E-674B6DBD13C4', -- AcmeCertificateList
    '8FCC11F1-CDA6-4AE9-AF5A-F173CD15A27F'  -- AcmeCertificateDetail
)

ROLLBACK TRANSACTION
