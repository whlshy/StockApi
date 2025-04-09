using Dapper;
using I3S_API.Model;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Text.Json.Nodes;

namespace I3S_API.Lib
{
    public class UUID
    {
        public List<OrderListModel> normalizeOrderstr(string order)
        {
            if (order.IsNullOrEmpty())
                return null;

            string[] ordersplit = order.Split(",");

            List<OrderListModel> orderList = new List<OrderListModel>();

            bool allow = true;
            for (int i = 0; i< ordersplit.Length; i++)
            {
                string str = ordersplit[i];
                string[] item = str.Split("_");

                //長度不等於2且不是a或d
                if (item.Length != 2 || !(item[1] == "a" || item[1] == "d"))
                {
                    allow = false;
                    break;
                }

                string column = item[0];
                string ordertype = item[1] == "a" ? "asc" : "desc";

                orderList.Add(new OrderListModel()
                {
                    index = i,
                    column = column,
                    ordertype = ordertype,
                    orderstr = $"[{column}] {ordertype}"
                });
            }

            return allow ? orderList : null;
        }

        public PublicModel getSqlandParam(HttpContext context, dynamic columnjson, int mid, string order, string like_column, string like, int? likeMode)
        {
            var param = context.Request.Query;

            int l = columnjson.Count;
            var p = new DynamicParameters();

            string where = @"";
            string ordersql = "";

            List<OrderListModel> orderList = normalizeOrderstr(order);
            List<OrderListModel> correctOrderList = new List<OrderListModel>();

            bool border = orderList.IsNullOrEmpty() ? false : true;

            string liketstr = "";

            Param fn_param = new Param();

            foreach (var item in columnjson)
            {

                //取得欄位名稱與型態
                string name = (string)item["name"];
                string type = (string)item["type"];

                if (name == "mid" || name == "sid")
                {
                    continue;
                }

                //比對order
                if (border && !orderList.IsNullOrEmpty())
                {
                    for (int j = 0; j < orderList.Count; j++)
                    {
                        if (orderList[j].column == name)
                        {
                            correctOrderList.Add(orderList[j]);
                            orderList.Remove(orderList[j]);
                        }
                    }
                }

                if (like_column == name && !like.IsNullOrEmpty())
                {
                    string tmp = "";
                    switch (likeMode)
                    {
                        case 0:
                            tmp = "'%' + @like + '%'";
                            break;
                        case 1:
                            tmp = "@like + '%'";
                            break;
                        case 2:
                            tmp = "'%' + @like";
                            break;
                        default:
                            tmp = "'%' + @like + '%'";
                            break;
                    }
                    liketstr = $" [{name}] like " + tmp;
                    p.Add("like", like, DbType.String);
                }


                string? value = param[name];
                if (value == null)
                {
                    continue;
                }
                else
                {
                    where = where + $"{(where == "" ? " where" : " and")} [{name}] = @{name}";

                    fn_param.addSQLDynamicP(p, name, value, type);
                }

            }

            if (correctOrderList.Count > 0)
            {
                correctOrderList.Sort((x, y) => x.index.CompareTo(y.index));

                correctOrderList.ForEach(v => ordersql += $"{(ordersql == "" ? "order by" : ",")} {v.orderstr}");
            }
            else
            {
                border = false;
            }

            if (liketstr.Length > 0)
                where = (where == "" ? " where " : where + " and ") + liketstr;

            PublicModel response = new PublicModel();
            response.where = where;
            response.p = p;
            response.ordersql = ordersql;
            response.border = border;
            return response;
        }

        public SqlStrModel getSqlString(UUIDModel uuidModel, HttpContext context, dynamic req, int mid)
        {
            dynamic columnjson = JsonConvert.DeserializeObject(uuidModel.bPermission ? uuidModel.UUID_data.vParam : uuidModel.UUID_data.pvParam);

            PublicModel publicModel = getSqlandParam(context, columnjson, mid, req.order, req.like_column, req.like, req.likeMode);

            //fetch 與 start，counts
            string fetch = "";
            if (req.start != null && req.counts != null && publicModel.border)
            {
                fetch = "offset @start - 1 row fetch next @counts rows only";
                req.first = false;
                publicModel.p.Add("start", req.start, DbType.Int32);
                publicModel.p.Add("counts", req.counts, DbType.Int32);
            }

            string permission = @"";
            if (uuidModel.bPermission && !uuidModel.MIDMode)
            {
                //狀況一 : 無傳CID需檢查權限
                if (uuidModel.CID == null)
                {
                    permission = $"where dbo.fs_checkUserPermission(CID, @mid, @permissionPos) = 1";
                    publicModel.p.Add("mid", mid, DbType.Int32);
                    publicModel.p.Add("permissionPos", uuidModel.PermissionPos, DbType.Int32);
                }
                //狀況二 : 有傳CID且CheckObject為true (CheckObject為true一定有傳CID)，檢查是否可以檢視隱藏Object
                else if (uuidModel.UUID_data.CheckObject == true)
                {
                    permission = $"where iif(hide = 1, @manage, 1) = 1";
                    publicModel.p.Add("manage", uuidModel.PermissionModel.M, DbType.Boolean);
                }
                //狀況三 : 有傳CID，上方已檢查過權限不需動做
            }
            string topdefault = fetch == "" ? "top 100" : "";

            string midmodesql = @"";
            if (uuidModel.MIDMode)
            {
                midmodesql = $@"where mid = @mid";
                publicModel.p.Add("mid", mid, DbType.Int32);
            }

            string strsqlview = @$"with t as(
                                        select * 
                                        from {uuidModel.ObjectName} 
                                        {publicModel.where}
                                    )select {(req.top != null ? $"top {req.top}" : topdefault)} * 
                                    from t
                                    {(uuidModel.MIDMode ? midmodesql : permission)} {(publicModel.border ? publicModel.ordersql : "")} {fetch}";

            string strsqltotal = @$"with t as(
                                        select * 
                                        from {uuidModel.ObjectName} 
                                        {publicModel.where}
                                    )
                                    select count(*) 'total'
                                    from t 
                                    {(uuidModel.MIDMode ? midmodesql : permission)}";

            SqlStrModel sqlStrModel = new SqlStrModel
            {
                strsqlview = strsqlview,
                strsqltotal = strsqltotal,
                first = req.first,
                p = publicModel.p
            };

            return sqlStrModel;
        }

        public int getPermissionPos(string? permission)
        {
            int pos;
            switch (permission)
            {
                case ("V"):
                    pos = 0;
                    break;
                case ("R"):
                    pos = 1;
                    break;
                case ("I"):
                    pos = 2;
                    break;
                case ("D"):
                    pos = 3;
                    break;
                case ("U"):
                    pos = 4;
                    break;
                case ("M"):
                    pos = 5;
                    break;
                case ("S"):
                    pos = 6;
                    break;
                default:
                    pos = -1;
                    break;
            }
            return pos;
        }
        public myUnauthorizedResult checkCIDPermission(string permission, int mid, int? cid)
        {
            int pos = getPermissionPos(permission);

            if (pos == -1)
            {
                return new myUnauthorizedResult("權限有誤");
            }
            else
            {
                string strsql = @$"select dbo.fs_checkUserPermission(@cid, @mid, @pos) 'permission'";
                using (var db = new AppDb())
                {
                    var check = db.Connection.QueryFirstOrDefault(strsql, new { cid, mid, pos });
                    if (!check.permission)
                    {

                        return new myUnauthorizedResult(mid == 0 ? "無權限，請嘗試重新登入或按F5重新整理網頁" : "無權限");
                    }
                }
            }

            return null;
        }

        public SPInOut UUIDSP_Param(UUIDModel uuidModel, int sid, int mid)
        {
            SPInOut spInOut = new SPInOut();

            spInOut.p = new DynamicParameters();

            spInOut.outputs = new List<dynamic>();

            Param param = new Param();

            dynamic columnjson = JsonConvert.DeserializeObject(uuidModel.UUID_data.Param);

            foreach (var item in columnjson)
            {
                //取得欄位名稱與型態
                string declare = (string)item["Param"];
                string name = declare.Substring(1);

                string type = (string)item["Type"];

                string mode = (string)item["Mode"];
                if (mode == "IN")
                {
                    switch (name)
                    {
                        case "mid":
                            spInOut.p.Add(declare, mid, DbType.Int32);
                            break;
                        case "sid":
                            spInOut.p.Add(declare, sid, DbType.Int32);
                            break;
                        default:
                            param.addSQLDynamicP(spInOut.p, declare, param.getParamFromBody<string?>(name, uuidModel.RequestBodyJson), type);
                            break;
                    }
                }
                else if (mode == "INOUT")
                {
                    spInOut.outputs.Add(item);

                    param.addSQLDynamicPOutput(spInOut.p, declare, type);
                }
            }
            spInOut.columnjson = columnjson;

            return spInOut;
        }
        public SPView UUIDSPViewParam(UUIDModel uuidModel, int sid, int mid, HttpContext httpContext)
        {
            SPView spView = new SPView();

            spView.p = new DynamicParameters();

            spView.sqlstring = $"exec [{uuidModel.ObjectName}]";
            string paramstring = "";

            Param param = new Param();

            dynamic columnjson = JsonConvert.DeserializeObject(uuidModel.UUID_data.vParam);

            foreach (var item in columnjson)
            {
                //取得欄位名稱與型態
                string declare = (string)item["Param"];
                string name = declare.Substring(1);

                string type = (string)item["Type"];

                string mode = (string)item["Mode"];
                if (mode == "IN")
                {
                    switch (name)
                    {
                        case "mid":
                            spView.p.Add(declare, mid, DbType.Int32);
                            break;
                        case "sid":
                            spView.p.Add(declare, sid, DbType.Int32);
                            break;
                        default:
                            param.addSQLDynamicP(spView.p, declare, param.getParamFromQuery<string?>(httpContext, name), type);
                            break;
                    }

                    paramstring += @$"{(paramstring.IsNullOrEmpty() ? " " : ", ")}{declare}";
                }

            }

            spView.sqlstring += paramstring;

            return spView;
        }
        public JsonObject getSPoutput(List<dynamic> outputs, DynamicParameters p)
        {
            JsonObject json = new JsonObject();
            Param param = new Param();
            foreach (var item in outputs)
            {
               string declare = (string)item["Param"];
                string type = (string)item["Type"];
                string name = declare.Substring(1);

                param.getSPOutputFromDynamicP(json, p, declare, name, type);
            }

            return json;

        }

        public int? getMethodType(string method)
        {
            int? type;

            switch (method)
            {
                case "GET":
                    type = 0;
                    break;
                case "POST":
                    type = 1;
                    break;
                case "PUT":
                    type = 2;
                    break;
                case "DELETE":
                    type = 3;
                    break;
                default:
                    type = null;
                    break;
            }

            return type;
        }

        public void insertLogManTx(string method, string objectName, DynamicParameters sp_InOut, int? sid, AppDb db)
        {
            if (method != "GET" && sid != null)
            {
                string strsql = @"select parameter_id, parameter_name 
                            from vd_SystemObjectParametersSL 
                            where name = @objectName
                            order by parameter_id";

                var data = db.Connection.Query(strsql, new { objectName });

                if (data == null)
                    return;

                var list = new Dictionary<string, string>();

                foreach (var item in data)
                {
                    string value = Convert.ToString(sp_InOut.Get<dynamic>($"{item.parameter_name.Substring(1)}"));
                    list.Add(item.parameter_name, value);
                }

                string jsonInOut = JsonConvert.SerializeObject(list);

                string strSql = "[xp_insertLogManTxFromApi]";
                var p = new DynamicParameters();
                p.Add("@json", jsonInOut);
                p.Add("@sid", sid);
                p.Add("@name", objectName);
                p.Add("@method", method);
                db.Connection.Execute(strSql, p, commandType: CommandType.StoredProcedure);

            }
        }
    }
}
