using Microsoft.AspNetCore.Mvc;
using I3S_API.Lib;
using Dapper;
using I3S_API.Model;
using I3S_API.Filter;
using Microsoft.AspNetCore.Http;
using System.Data;
using System.Security.Cryptography;

namespace I3S_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class FolderController : ControllerBase
    {
        /// <summary>
        /// 取得資料夾CID目錄內容 (資料夾、股票)
        /// </summary>
        [HttpGet("{cid}")]
        [AuthFilter("R")]
        public IActionResult GetFolder(int cid)
        {
            string strsql = @"select * from vd_StockFolder where PCID = @cid order by [Rank]";

            using (var db = new AppDb())
            {
                var folder = db.Connection.Query(strsql, new { cid });

                strsql = @"select * from vd_StockFolder where CCID = @cid";
                var data = db.Connection.QueryFirstOrDefault(strsql, new { cid });

                return Ok(new{ data, folder });
            }
        }
        /// <summary>
        /// 新增資料夾
        /// </summary>
        [HttpPost("{cid}")]
        [AuthFilter("I")]
        public IActionResult AddFile(int cid, [FromBody] AddFolder body)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            bool status;
            string message;

            string strSql = "xp_addFolder";
            this.HttpContext.Items.Add("ObjectName", strSql);

            using (var db = new AppDb())
            {
                var p = new DynamicParameters();
                p.Add("@cid", cid);
                p.Add("@cname", body.cname);
                p.Add("@des", body.des);
                p.Add("@mid", mid);
                p.Add("@sid", sid);
                p.Add("@status", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                p.Add("@message", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                db.Connection.Execute(strSql, p, commandType: CommandType.StoredProcedure);
                status = p.Get<bool>("@status");
                message = p.Get<string>("@message");

                this.HttpContext.Items.Add("SP_InOut", p);

                return Ok(new { status, message });
            }
        }
        /// <summary>
        /// 更新資料夾
        /// </summary>
        [HttpPut("{cid}")]
        [AuthFilter("U")]
        public IActionResult EditFolder(int cid, [FromBody] EditFolder body)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            bool status;
            string message;

            string strSql = "xp_editFolder";
            this.HttpContext.Items.Add("ObjectName", strSql);

            using (var db = new AppDb())
            {
                var p = new DynamicParameters();
                p.Add("@cid", cid);
                p.Add("@cname", body.cname);
                p.Add("@des", body.des);
                p.Add("@mid", mid);
                p.Add("@sid", sid);
                p.Add("@status", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                p.Add("@message", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                db.Connection.Execute(strSql, p, commandType: CommandType.StoredProcedure);
                status = p.Get<bool>("@status");
                message = p.Get<string>("@message");

                this.HttpContext.Items.Add("SP_InOut", p);

                return Ok(new { status, message });
            }
        }
        /// <summary>
        /// 刪除資料夾
        /// </summary>
        [HttpDelete("{cid}")]
        [AuthFilter("D")]
        public IActionResult DelFolder(int cid)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            bool status;
            string message;

            string strSql = "xp_delFolder";
            this.HttpContext.Items.Add("ObjectName", strSql);

            using (var db = new AppDb())
            {
                var p = new DynamicParameters();
                p.Add("@cid", cid);
                p.Add("@mid", mid);
                p.Add("@sid", sid);
                p.Add("@status", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                p.Add("@message", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                db.Connection.Execute(strSql, p, commandType: CommandType.StoredProcedure);
                status = p.Get<bool>("@status");
                message = p.Get<string>("@message");

                this.HttpContext.Items.Add("SP_InOut", p);

                return Ok(new { status, message });
            }
        }
        /// <summary>
        /// 排序資料夾
        /// </summary>
        [HttpPut("sort/{cid}")]
        [AuthFilter("D")]
        public IActionResult SortFolder(int cid, [FromBody] SortFolder body)
        {
            int mid = (int)this.HttpContext.Items["MID"];
            int sid = (int)this.HttpContext.Items["SID"];

            bool status;
            string message;

            string strSql = "xp_sortFolder";
            this.HttpContext.Items.Add("ObjectName", strSql);

            using (var db = new AppDb())
            {
                var p = new DynamicParameters();
                p.Add("@cid", cid);
                p.Add("@seq", body.seq);
                p.Add("@mid", mid);
                p.Add("@sid", sid);
                p.Add("@status", dbType: DbType.Boolean, direction: ParameterDirection.Output);
                p.Add("@message", dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                db.Connection.Execute(strSql, p, commandType: CommandType.StoredProcedure);
                status = p.Get<bool>("@status");
                message = p.Get<string>("@message");

                this.HttpContext.Items.Add("SP_InOut", p);

                return Ok(new { status, message });
            }
        }
    }
}