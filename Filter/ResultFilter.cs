using Azure.Core;
using Dapper;
using I3S_API.Lib;
using I3S_API.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

namespace I3S_API.Filter
{
    public class ResultFilter : Attribute, IResultFilter
    {
        private UUIDModel _uuidModel;
        public ResultFilter(UUIDModel uuidModel)
        {
            _uuidModel = uuidModel;
        }
        public void OnResultExecuting(ResultExecutingContext context)
        {
            //Result 執行前執行
            

        }
        public void OnResultExecuted(ResultExecutedContext context)
        {
            //Result 執行後執行
            HttpContext httpContext = context.HttpContext;

            string method = httpContext.Request.Method;

            bool bLog = false;
            if (httpContext.Items.ContainsKey("bLog"))
                bLog = (bool)httpContext.Items["bLog"];

            string? objectName = _uuidModel.ObjectName;
            if (httpContext.Items.ContainsKey("ObjectName"))
            {
                objectName = httpContext.Items["ObjectName"].ToString();
            }

            if (httpContext.Items.ContainsKey("SID") && !objectName.IsNullOrEmpty()
                 && httpContext.Items.ContainsKey("SP_InOut") && (bLog || method != "GET"))
            {
                int sid = (int)httpContext.Items["SID"];
                DynamicParameters sp_InOut = (DynamicParameters)httpContext.Items["SP_InOut"];

                using (var db = new AppDb())
                {
                    new Log().insertLogManTx(method, objectName, sp_InOut, sid, db);
                }
            }
        }
    }
}
