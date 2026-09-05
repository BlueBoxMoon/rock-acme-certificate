using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.blueboxmoon.AcmeCertificate.SystemGuid
{
    public class BlockType
    {
        public const string ACME_CONFIG = "4F6285D0-6871-4C63-8829-A7F0F608E2D8";

        public const string ACME_CERTIFICATES = "A08FB359-E68E-46FC-85D6-53726EAE6E23";

        public const string ACME_CERTIFICATE_DETAIL = "F2BB435E-53C6-46E8-867E-0807A7DD9691";

        public const string ACME_CHALLENGE = "0DE7B05B-DCB4-4A44-A8B5-F9B15E92355B";

        // Obsidian replacements for the WebForms block types above. Existing
        // block instances are re-pointed to these during migration.
        public const string ACME_CERTIFICATE_LIST_OBSIDIAN = "93FC8A02-E0F5-485F-898B-57E1936245FA";

        public const string ACME_CONFIG_OBSIDIAN = "1F324FFC-178D-4304-9B18-B1D3F3C2C854";

        public const string ACME_CERTIFICATE_DETAIL_OBSIDIAN = "E052A496-64E2-4BF5-8EA9-7FE4DFDF0D68";
    }
}
