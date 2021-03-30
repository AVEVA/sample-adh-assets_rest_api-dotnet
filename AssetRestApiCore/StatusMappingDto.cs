using System.Collections.Generic;

namespace AssetsRestApi
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Data Transfer Object (DTO) requires setter")]
    public class StatusMappingDto
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string StreamReferenceId { get; set; }

        public string StreamPropertyId { get; set; }

        public List<ValueStatusMappingDto> ValueStatusMappings { get; set; } = new List<ValueStatusMappingDto>();
    }
}
