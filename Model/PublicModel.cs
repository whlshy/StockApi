using Dapper;

namespace I3S_API.Model
{
    public class PublicModel
    {
        public string where { get; set; }
        public DynamicParameters p { get; set; }
        public string ordersql { get; set; }
        public bool border { get; set; }
        public string like { get; set; }
    }
    public class OrderListModel
    {
        public int index { get; set; }
        public string orderstr { get; set; }
        public string column { get; set; }
        public string ordertype { get; set; }
    }
    public class PermissionModel2
    {
        public bool Subscribe { get; set; }
        public bool Manage { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Insert { get; set; }
        public bool Read { get; set; }
        public bool View { get; set; }

    }

    public class PermissionModel
    {
        public bool S { get; set; }
        public bool M { get; set; }
        public bool U { get; set; }
        public bool D { get; set; }
        public bool I { get; set; }
        public bool R { get; set; }
        public bool V { get; set; }

    }
    public class SqlStrModel
    {
        public string strsqlview { get; set; }
        public string strsqltotal { get; set; }
        public DynamicParameters p { get; set; }
        public bool first { get; set; }

    }
    public class UUIDAPI
    {
        /// <summary>
        /// queryfirst回傳Json，若有傳start與counts，則強制設為false
        /// </summary>
        public bool first { get; set; }

        /// <summary>
        /// 從第start筆開始，往後counts筆資料 (必需搭配start、counts、order參數)
        /// </summary>
        public int? start{ get; set; }

        /// <summary>
        ///  從第start筆開始，往後counts筆資料 (必需搭配start、counts、order參數)
        /// </summary>
        public int? counts{ get; set; }

        /// <summary>
        ///  {欄位}_a,{欄位}_d (欄位 asc,欄位 desc)
        /// </summary>
        public string? order{ get; set; }

        /// <summary>
        ///  多回傳資料總筆數，可用於分頁功能
        /// </summary>
        public bool bTotal{ get; set; }

        /// <summary>
        ///  前top筆資料，預設100
        /// </summary>
        public int? top{ get; set; }

        /// <summary>
        ///  like的欄位
        /// </summary>
        public string? like_column { get; set; }

        /// <summary>
        ///  like字串
        /// </summary>
        public string? like { get; set; }

        /// <summary>
        ///  0|1|2 (cotain|startWith|endWith)
        /// </summary>
        public int? likeMode { get; set; }

        /// <summary>
        /// True：限定Public
        /// </summary>
        public bool bPublicOnly { get; set; }
        public UUIDAPI()
        {
            bPublicOnly = false;
        }
    }
}
