using Dapper;
using I3S_API.Lib;
using I3S_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace I3S_API.Filter
{
    public class UUID2PublicSPAuthFilter : Attribute, IAuthorizationFilter
    {
        private UUIDModel _uuidModel;
        public UUID2PublicSPAuthFilter(UUIDModel uuidModel)
        {
            _uuidModel = uuidModel;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            HttpContext httpContext = context.HttpContext;

            Guid? uuid = null;
            Param fn_param = new Param();

            if (httpContext.Request.Method == "GET")
            {
                uuid = fn_param.getValueFromRoute<Guid?>(httpContext, "uuid");
            }

            if (uuid == null)
            {
                context.Result = new BadRequestObjectResult(new ResponseModel("錯誤傳遞", 400));
                return;
            }

            string strsql = @$"select spName, param as vParam
                    from vd_PublicUUID2SP
                    where UUID = @uuid";


            using (var db = new AppDb())
            {
                dynamic data = db.Connection.QueryFirstOrDefault<dynamic>(strsql, new { uuid });

                if (data == null || data.vParam == null)
                {
                    context.Result = new BadRequestObjectResult(new ResponseModel("錯誤傳遞", 400));
                    return;
                }


                _uuidModel.UUID_data = data;
                _uuidModel.ObjectName =  data.spName;

            }
        }
    }
}
