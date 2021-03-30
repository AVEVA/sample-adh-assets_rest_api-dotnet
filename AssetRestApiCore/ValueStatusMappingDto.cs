namespace AssetRestApiCore
{
    public sealed class ValueStatusMappingDto
    {
        public object Value { get; set; }
        public StatusEnum Status { get; set; }
        public string DisplayName { get; set; }
    }
}
