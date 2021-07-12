using System.Collections.Generic;

namespace AssetsRestApi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data Transfer Object (DTO) requires setter")]
    public class Asset
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string AssetTypeId { get; set; }

        public List<MetadataDto> Metadata { get; set; } = new List<MetadataDto>();

        public List<StreamReferenceDto> StreamReferences { get; set; } = new List<StreamReferenceDto>();

        public StatusConfigurationDto Status { get; set; }
    }
}
