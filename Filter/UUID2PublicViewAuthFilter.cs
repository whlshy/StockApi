using Dapper;
using I3S_API.Lib;
using I3S_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace I3S_API.Filter
{
    public class UUID2PublicViewAuthFilter : Attribute, IAuthorizationFilter
    {
        private UUIDModel _uuidModel;
        public UUID2PublicViewAuthFilter(UUIDModel uuidModel)
        {
            _uuidModel = uuidModel;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            HttpContext httpContext = context.HttpContext;

            int mid = (int)httpContext.Items["MID"];
            if (mid == null)
            {
                context.Result = new myUnauthorizedResult("無權限.");
                return;
            }

            Guid? uuid = null;
            int? cid = null;
            Param fn_param = new Param();
            UUID fn_uuid = new UUID(); ;

            if (httpContext.Request.Method == "GET")
            {
                uuid = fn_param.getValueFromRoute<Guid?>(httpContext, "uuid");
                cid = fn_param.getParamFromQuery<int?>(httpContext, "cid");
            }

            if (uuid == null)
            {
                context.Result = new BadRequestObjectResult(new ResponseModel("錯誤傳遞", 400));
                return;
            }

            string strsql = @$"select vName, pvName, CheckObject, vParam, pvParam, requiredCID
                    from vd_publicUUID2View
                    where UUID = @uuid";

            using (var db = new AppDb())
            {
                dynamic data = db.Connection.QueryFirstOrDefault<dynamic>(strsql, new { uuid });

                //是否需權限驗證
                bool bPermission = true;

                //只檢查mid
                bool midmode = false;

                //權限驗證位置預設0，代表View
                int permissionPos = 0;

                if (data == null)
                {
                    context.Result = new BadRequestObjectResult(new ResponseModel("錯誤傳遞", 400));
                    return;
                }

                bool requiredCID = data.requiredCID ?? false;
                if (requiredCID && cid == null)
                {
                    context.Result = new myUnauthorizedResult("無權限");
                    return;
                }


                //只走public view
                bool bPublicOnly = fn_param.getParamFromQuery<bool>(httpContext, "bPublicOnly");

                if (bPublicOnly)
                    bPermission = false;
                else
                    bPermission = (mid == 0 ? false : true);

                //檢查有無view與view_pub
                if (data?.pvName == null)
                {
                    context.Result = new myUnauthorizedResult("無權限...");
                    return;
                }
                else if (data?.vName == null)
                    bPermission = false;


                bool checkObject = data.CheckObject ?? false;
                //若為Object View需傳cid，否則無權限，midmode不考慮
                if (checkObject && cid == null && !midmode)
                {
                    context.Result = new myUnauthorizedResult("無權限....");
                    return;
                }

                //若有傳CID檢查權限，若為midmode不考慮
                PermissionModel permissionModel = new PermissionModel();
                if (cid != null && bPermission && !midmode)
                {
                    string checkPermissionSQL = @$"select * from fn_getCIDPermission(@cid, @mid)";

                    //取得會員在cid的所有權限
                    permissionModel = db.Connection.QueryFirstOrDefault<PermissionModel>(checkPermissionSQL, new { cid, mid });

                    //會員有無View權限
                    bool allow = permissionModel.V;

                    //無權限或為Object View判斷Read權限
                    if (!allow || (checkObject && !permissionModel.R))
                    {
                        context.Result = new myUnauthorizedResult("無權限.....");
                        return;
                    }
                }

                _uuidModel.UUID_data = data;
                _uuidModel.PermissionModel = permissionModel;
                _uuidModel.PermissionPos = permissionPos;
                _uuidModel.bPermission = bPermission;
                _uuidModel.CID = cid;
                _uuidModel.MIDMode = midmode;
                _uuidModel.ObjectName = (bPermission ? data.vName : data.pvName);
            }


        }
    }
}
