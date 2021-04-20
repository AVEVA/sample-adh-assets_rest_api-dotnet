namespace AssetsRestApi
{
    public class MetadataDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SdsTypeCode SdsTypeCode { get; set; }
        public object Value { get; set; }
        public string Uom { get; set; }
    }
}
