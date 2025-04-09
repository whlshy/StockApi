namespace I3S_API.Model
{
    public class UUIDModel
    {
        public int? CID { get; set; }
        public bool MIDMode { get; set; }
        public string ObjectName { get; set; }
        public dynamic? RequestBodyJson { get; set; }
        public dynamic UUID_data { get; set; }
        public PermissionModel PermissionModel { get; set; }
        public int PermissionPos { get; set; }
        public bool bPermission { get; set; }
        public bool TxType { get; set; }
    }
}
