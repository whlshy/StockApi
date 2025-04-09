using Dapper;
using I3S_API.Lib;
using I3S_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace I3S_API.Filter
{
    public class UUID2TxViewAuthFilter : Attribute, IAuthorizationFilter
    {
        private UUIDModel _uuidModel;
        public UUID2TxViewAuthFilter(UUIDModel uuidModel)
        {
            _uuidModel = uuidModel;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            HttpContext httpContext = context.HttpContext;

            int mid = (int)httpContext.Items["MID"];
            if (mid <= 0)
            {
                context.Result = new myUnauthorizedResult("無權限.");
                return;
            }

            Guid? uuid = null;
            int? cid = null;
            Param fn_param = new Param();
            UUID fn_uuid = new UUID();


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

            string strsql = @$"select vName, PermissionType, TxType, CheckObject, vParam, requiredCID
                    from vd_UUID2Tx_View
                    where UUID = @uuid";

            using (var db = new AppDb())
            {
                dynamic data = db.Connection.QueryFirstOrDefault<dynamic>(strsql, new { uuid });

                if (data == null)
                {
                    context.Result = new BadRequestObjectResult(new ResponseModel("錯誤傳遞", 400));
                    return;
                }

                bool requiredCID = data.requiredCID ?? false;
                if (requiredCID  && cid == null)
                {
                    context.Result = new myUnauthorizedResult("無權限");
                    return;
                }

                //是否需權限驗證
                bool bPermission = true;

                //只檢查mid
                bool midmode = false;

                //權限驗證位置預設0，代表View
                int permissionPos = 0;

                if (data == null || data?.vName == null)
                {
                    context.Result = new myUnauthorizedResult("無權限");
                    return;
                }

                midmode =  data.PermissionType == null ? true : false;
                permissionPos = fn_uuid.getPermissionPos(data.PermissionType);

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

                    //若為Tx API，從data取得權限驗證種類，並判斷有無權限
                    allow = permissionModel.GetType().GetProperty(data.PermissionType).GetValue(permissionModel, null);

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
                _uuidModel.ObjectName = data.vName;
                _uuidModel.TxType = data.TxType;
            }


        }
    }
}
