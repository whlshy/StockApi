using I3S_API.Lib;
using I3S_API.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Dapper;
using I3S_API.Filter;
using System.Text.Json.Nodes;

namespace I3S_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TxController : ControllerBase
    {
        private UUIDModel _uuidModel;
        public TxController(UUIDModel uuidModel)
        {
            _uuidModel = uuidModel;
        }

        /// <summary>
        /// Tx View UUID API
        /// </summary>
        /// <param name="uuid">View對應的UUID</param>
        /// <remarks>
        /// ### 限定類型為view表，vd_UUID2Tx.TxType必需為False
        /// ### 使用Default帳號，請先設定連線資訊
        /// ### 判斷會員有無cid權限，權限總類為vd_UUID2Tx.PermissionType
        /// ### view表需包含小寫cid欄位
        /// ### 若vd_UUID2Tx.PermissionType為NULL，代表MID模式且不會判斷權限，view表需包含mid欄位且程式將自動帶入
        /// ### 若為訪客一律回傳無權限
        /// ### query動態參數名稱必需與SP參數名稱相同，程式將自動match，建議採用lower camel case
        /// </remarks>
        [HttpGet("{uuid}")]
        [ServiceFilter(typeof(UUID2TxViewAuthFilter))]
        public IActionResult Method(Guid uuid, [FromQuery] UUIDAPI req)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            if (_uuidModel.TxType)
                return BadRequest(new ResponseModel("錯誤傳遞..", 400));


            UUID fn_uuid = new UUID();

            string sqlstring, strsqltotal;
            DynamicParameters p = new DynamicParameters();
            bool first = req.first;

            SqlStrModel sqlStrModel = fn_uuid.getSqlString(_uuidModel, HttpContext, req, mid);
            sqlstring = sqlStrModel.strsqlview;
            p = sqlStrModel.p;
            first = sqlStrModel.first;
            strsqltotal = sqlStrModel.strsqltotal;

            

            using (var db = new AppDb())
            {

                if (first)
                {
                    var data = db.Connection.QueryFirstOrDefault(sqlstring, p);
                    return Ok(data ?? new { });
                }
                else
                {

                    if (req.bTotal)
                    {

                        var rep_totle = db.Connection.QueryFirstOrDefault(strsqltotal, p);

                        var data = db.Connection.Query(sqlstring, p);

                        return Ok(new { rep_totle.total, data });
                    }
                    else
                    {
                        var data = db.Connection.Query(sqlstring, p);
                        return Ok(data);
                    }

                }
            }
        }

        /// <summary>
        /// Tx UUID SP
        /// </summary>
        /// <param name="uuid">SP對應的UUID</param>
        /// <remarks>
        /// ### 使用Default帳號，請先設定連線資訊
        /// ### 判斷會員有無cid權限，權限總類為vd_UUID2Tx.PermissionType
        /// ### 需傳cid參數
        /// ### 請於SP內檢查是否為有效cid
        /// ### 若vd_UUID2Tx.PermissionType為null，將不判斷權限，請自行在SP內判斷權限
        /// ### 若SP參數包含mid、sid (Member.MID、MSession.SID)，程式將自動帶入並覆蓋無需多傳
        /// ### 自動回傳SP output
        /// ### 若為訪客一律回傳無權限
        /// ### body動態參數名稱必需與SP參數名稱相同，程式將自動match，建議採用lower camel case
        /// </remarks>
        [HttpPost("{uuid}")]
        [ServiceFilter(typeof(UUID2TxSPAuthFilter))]
        public IActionResult TxPostMethod([FromBody] SPModel req)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            UUID fn_uuid = new UUID();

            SPInOut spInOut = fn_uuid.UUIDSP_Param(_uuidModel, sid, mid);
            using (var db = new AppDb())
            {
                string sp = _uuidModel.UUID_data.Name;
                db.Connection.Execute(sp, spInOut.p, commandType: CommandType.StoredProcedure);

                JsonObject json = fn_uuid.getSPoutput(spInOut.outputs, spInOut.p);

                this.HttpContext.Items.Add("SP_InOut", spInOut.p);

                return Ok(json);
            }

        }

        /// <summary>
        /// Tx UUID SP
        /// </summary>
        /// <param name="uuid">SP對應的UUID</param>
        /// <remarks>
        /// ### 使用Default帳號，請先設定連線資訊
        /// ### 判斷會員有無cid權限，權限總類為vd_UUID2Tx.PermissionType
        /// ### 需傳cid參數
        /// ### 請於SP內檢查是否為有效cid
        /// ### 若vd_UUID2Tx.PermissionType為null，將不判斷權限，請自行在SP內判斷權限
        /// ### 若SP參數包含mid、sid (Member.MID、MSession.SID)，程式將自動帶入並覆蓋無需多傳
        /// ### 自動回傳SP output
        /// ### 若為訪客一律回傳無權限
        /// ### body動態參數名稱必需與SP參數名稱相同，程式將自動match，建議採用lower camel case
        /// </remarks>
        [HttpPut("{uuid}")]
        [ServiceFilter(typeof(UUID2TxSPAuthFilter))]
        public IActionResult TxPutMethod([FromBody] SPModel req)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            UUID fn_uuid = new UUID();

            SPInOut spInOut = fn_uuid.UUIDSP_Param(_uuidModel, sid, mid);
            using (var db = new AppDb())
            {
                string sp = _uuidModel.UUID_data.Name;
                db.Connection.Execute(sp, spInOut.p, commandType: CommandType.StoredProcedure);

                JsonObject json = fn_uuid.getSPoutput(spInOut.outputs, spInOut.p);

                this.HttpContext.Items.Add("SP_InOut", spInOut.p);

                return Ok(json);
            }

        }

        /// <summary>
        /// Tx UUID SP
        /// </summary>
        /// <param name="uuid">SP對應的UUID</param>
        /// <remarks>
        /// ### 使用Default帳號，請先設定連線資訊
        /// ### 判斷會員有無cid權限，權限總類為vd_UUID2Tx.PermissionType
        /// ### 需傳cid參數
        /// ### 請於SP內檢查是否為有效cid
        /// ### 若vd_UUID2Tx.PermissionType為null，將不判斷權限，請自行在SP內判斷權限
        /// ### 若SP參數包含mid、sid (Member.MID、MSession.SID)，程式將自動帶入並覆蓋無需多傳
        /// ### 自動回傳SP output
        /// ### 若為訪客一律回傳無權限
        /// ### body動態參數名稱必需與SP參數名稱相同，程式將自動match，建議採用lower camel case
        /// </remarks>
        [HttpDelete("{uuid}")]
        [ServiceFilter(typeof(UUID2TxSPAuthFilter))]
        public IActionResult TxDeleteMethod([FromBody] SPModel req)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            UUID fn_uuid = new UUID();

            SPInOut spInOut = fn_uuid.UUIDSP_Param(_uuidModel, sid, mid);
            using (var db = new AppDb())
            {
                string sp = _uuidModel.UUID_data.Name;
                db.Connection.Execute(sp, spInOut.p, commandType: CommandType.StoredProcedure);

                JsonObject json = fn_uuid.getSPoutput(spInOut.outputs, spInOut.p);

                this.HttpContext.Items.Add("SP_InOut", spInOut.p);

                return Ok(json);
            }

        }
    }
}
