using Microsoft.AspNetCore.Mvc;
using I3S_API.Lib;
using Dapper;
using I3S_API.Model;
using I3S_API.Filter;

namespace I3S_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        /// <summary>
        /// 取得我的會員資訊
        /// </summary>
        [HttpGet("")]
        public IActionResult GetMember()
        {
            int mid = (int)this.HttpContext.Items["MID"];
            string strsql = @"select mid, name, account, img, email, sso, convert(varchar, LastLoginDT, 120) as lastLoginDT, classID 
                            from vs_member where mid = @mid";

            using (var db = new AppDb())
            {

                var data = db.Connection.QueryFirstOrDefault(strsql, new { mid });

                strsql = @"select CID, [Type], CName from vd_ClassMemberNext 
                           where dbo.fs_checkUserPermission(CID, MID, 0) = 1 and MID = @mid";
                var classes = db.Connection.Query(strsql, new { mid });

                return Ok(new{ data, classes });
            }
        }
    }
}
