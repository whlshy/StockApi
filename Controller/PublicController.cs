using I3S_API.Lib;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using I3S_API.Model;
using I3S_API.Filter;

namespace I3S_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private UUIDModel _uuidModel;
        public PublicController(UUIDModel uuidModel)
        {
            _uuidModel = uuidModel;
        }
        /// <summary>
        /// Public View UUID API
        /// </summary>
        /// <param name="uuid">View對應的UUID</param>
        /// <remarks>
        /// ### 使用Public帳號，請先設定連線資訊
        /// ### 記得grant select on {vd_XXX_Pub} to {Public帳號} 
        /// ### query動態參數部分需與view表欄位名稱相同，程式將自動match，建議採用lower camel case
        /// </remarks>
        [HttpGet("{uuid}")]
        [ServiceFilter(typeof(UUID2PublicViewAuthFilter))]
        public IActionResult Method(Guid uuid, [FromQuery] UUIDAPI req)
        {
            int mid = (int)this.HttpContext.Items["MID"];

            UUID fn_uuid = new UUID();

            SqlStrModel sqlStrModel = fn_uuid.getSqlString(_uuidModel, HttpContext, req,  mid);

            using (var db = new AppDb(_uuidModel.bPermission ? "Default" : "PublicRead"))
            {
                
                if (sqlStrModel.first)
                {
                    var data = db.Connection.QueryFirstOrDefault(sqlStrModel.strsqlview, sqlStrModel.p);
                    return Ok(data ?? new { });
                }
                else
                {
                    var data = db.Connection.Query(sqlStrModel.strsqlview, sqlStrModel.p);

                    if (req.bTotal)
                    {
                        var rep_totle = db.Connection.QueryFirstOrDefault(sqlStrModel.strsqltotal, sqlStrModel.p);

                        return Ok(new { rep_totle.total, data });
                    }
                    else
                    {
                        return Ok(data);
                    }

                }

            }

        }
    }
}
