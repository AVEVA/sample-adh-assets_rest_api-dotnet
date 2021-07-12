namespace AssetsRestApi
{
    public class StatusConfigurationDto
    {
        public StatusDefinitionType DefinitionType { get; set; } = StatusDefinitionType.StreamPropertyMapping;

        public StatusMappingDto Definition { get; set; }
    }
}
