using Dapper;
using I3S_API.Filter;
using I3S_API.Lib;
using I3S_API.Model;
using Microsoft.AspNetCore.Mvc;

namespace I3S_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class Public2Controller : ControllerBase
    {
        private UUIDModel _uuidModel;
        public Public2Controller(UUIDModel uuidModel)
        {
            _uuidModel = uuidModel;
        }
        /// <summary>
        /// Public GET SP UUID API
        /// </summary>
        /// <param name="uuid">GET SP對應的UUID</param>
        /// <param name="first">queryfirst回傳Json，預設false</param>
        /// <remarks>
        /// ### 使用Public帳號，請先設定連線資訊
        /// ### 當Public API無法達成目的，可改用執行SP內select的方法
        /// ### 記得grant execute on object::{dbo.xp_getXXX_Pub} to {Public帳號} 
        /// ###  query動態參數名稱必需與SP參數名稱相同，程式將自動match，建議採用lower camel case
        /// ### 若SP參數包含mid、sid (Member.MID、MSession.SID)，程式將自動帶入並覆蓋無需多傳
        /// </remarks>
        [HttpGet("{uuid}")]
        [ServiceFilter(typeof(UUID2PublicSPAuthFilter))]
        public IActionResult Method(Guid uuid, bool first = false)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            UUID fn_uuid = new UUID();

            string sqlstring, strsqltotal;
            DynamicParameters p = new DynamicParameters();


            SPView spView = fn_uuid.UUIDSPViewParam(_uuidModel, sid, mid, HttpContext);
            sqlstring = spView.sqlstring;
            p = spView.p;


            using (var db = new AppDb("PublicRead"))
            {
                dynamic data;
                if (first)
                {
                    data = db.Connection.QueryFirstOrDefault(sqlstring, p);
                    data = data ?? new { };
                }
                else
                {
                    data = db.Connection.Query(sqlstring, p);
                }

                return Ok(data);
            }
        }
    }
}
