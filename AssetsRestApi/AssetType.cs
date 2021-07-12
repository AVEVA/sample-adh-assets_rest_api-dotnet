using System.Collections.Generic;

namespace AssetsRestApi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data Transfer Object (DTO) requires setter")]
    public class AssetType
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<MetadataDto> Metadata { get; set; } = new List<MetadataDto>();

        public List<TypeReferenceDto> TypeReferences { get; set; } = new List<TypeReferenceDto>();

        public StatusConfigurationDto Status { get; set; }
    }
}
