using Dapper;
using I3S_API.Model;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Data;
using System.Text.Json.Nodes;

namespace I3S_API.Lib
{
    public class Param
    {
        public T? getParamFromQuery<T>(HttpContext httpContext, string keyname)
        {
            var param = httpContext.Request.Query;
            string? t = null;
            if (param.ContainsKey(keyname))
            {
                t = param[keyname].ToString();
            }

            return t.IsNullOrEmpty() ? default(T?) : ConvertType<T?>(t);
        }

        public T? getValueFromRoute<T>(HttpContext httpContext, string keyname)
        {
            var param = httpContext.Request.RouteValues;
            string? t = null;
            if (param.ContainsKey(keyname))
            {
                t = param[keyname].ToString();
            }

            return t.IsNullOrEmpty() ? default(T?) : ConvertType<T?>(t);
        }
        public T? ConvertType<T>(string value)
        {
            try
            {
                TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(T?));
                object propValue = typeConverter.ConvertFromString(value);

                return ChangeType<T?>(propValue);
            }
            catch
            {
                return default(T?);
            }
        }

        public static T? ChangeType<T>(object value)
        {
            
            try
            {
                var t = typeof(T?);

                if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    if (value == null)
                    {
                        return default(T?);
                    }

                    t = Nullable.GetUnderlyingType(t);
                }
                return (T?)Convert.ChangeType(value, t);
            }
            catch
            {
                return default(T?);
            }

        }

        public T? getParamFromBody<T>(string checkparam, dynamic paramBody)
        {
            if (paramBody.ContainsKey(checkparam))
            {
                var t = paramBody[checkparam];
                if (t == null)
                    return default(T?);
                else
                    return ConvertType<T?>(t.ToString());
            }
            else
            {
                return default(T?);
            }
        }

        public int? getCIDFromAll(HttpContext httpContext, string checkparam)
        {
            int? cid = null;

            cid = getValueFromRoute<int?>(httpContext, checkparam);

            if (cid == null)
            {
                if (httpContext.Request.Method == "GET")
                {
                    cid = getParamFromQuery<int?>(httpContext, checkparam);
                }
                else if (httpContext.Request.ContentType == "application/json")
                {
                    httpContext.Request.EnableBuffering();

                    string RequestBody = new StreamReader(httpContext.Request.BodyReader.AsStream()).ReadToEnd();
                    httpContext.Request.Body.Position = 0;

                    dynamic columnjson = JsonConvert.DeserializeObject(RequestBody);
                    try
                    {
                        cid = (int)columnjson[checkparam];
                    }
                    catch
                    {
                        cid = null;
                    }
                }
                else
                {
                    try
                    {
                        cid = Int32.Parse(httpContext.Request.Form[checkparam][0]);
                    }
                    catch
                    {
                        cid = null;
                    }
                }
            }
            return cid;
        }

        public dynamic? getBodyParamByJson(HttpContext httpContext)
        {
            if (httpContext.Request.ContentType == "application/json")
            {
                httpContext.Request.EnableBuffering();

                string RequestBody = new StreamReader(httpContext.Request.BodyReader.AsStream()).ReadToEnd();
                httpContext.Request.Body.Position = 0;

                dynamic? columnjson = JsonConvert.DeserializeObject(RequestBody);
                return columnjson;
            }

            return null;
        }

        public void addSQLDynamicP(DynamicParameters p, string param, string value, string type)
        {
            switch (type)
            {
                case "int":
                    p.Add(param, ConvertType<int?>(value), DbType.Int32);
                    break;
                case "smallint":
                    p.Add(param, ConvertType<int?>(value), DbType.Int32);
                    break;
                case "tinyint":
                    p.Add(param, ConvertType<int?>(value), DbType.Int32);
                    break;
                case "float":
                    p.Add(param, ConvertType<double?>(value), DbType.Double);
                    break;
                case "bit":
                    p.Add(param, ConvertType<bool?>(value), DbType.Boolean);
                    break;
                case "uniqueidentifier":
                    p.Add(param, ConvertType<Guid?>(value), DbType.Guid);
                    break;
                case "date":
                    p.Add(param, ConvertType<DateTime?>(value), DbType.Date);
                    break;
                case "datetime":
                    p.Add(param, ConvertType<DateTime?>(value), DbType.DateTime);
                    break;
                default:
                    p.Add(param, ConvertType<string?>(value), DbType.String);
                    break;
            }

        }

        public void addSQLDynamicPOutput(DynamicParameters p, string param, string type)
        {
            switch (type)
            {
                case "int":
                    p.Add(param, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    break;
                case "smallint":
                    p.Add(param, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    break;
                case "tinyint":
                    p.Add(param, dbType: DbType.Int32, direction: ParameterDirection.Output);
                    break;
                case "float":
                    p.Add(param, dbType: DbType.Double, direction: ParameterDirection.Output);
                    break;
                case "bit":
                    p.Add(param, dbType: DbType.Boolean, direction: ParameterDirection.Output);
                    break;
                case "uniqueidentifier":
                    p.Add(param, dbType: DbType.Guid, direction: ParameterDirection.Output);
                    break;
                case "date":
                    p.Add(param, dbType: DbType.Date, direction: ParameterDirection.Output);
                    break;
                case "datetime":
                    p.Add(param, dbType: DbType.DateTime, direction: ParameterDirection.Output);
                    break;
                default:
                    p.Add(param, dbType: DbType.String, direction: ParameterDirection.Output, size: 255);
                    break;
            }

        }

        public void getSPOutputFromDynamicP(JsonObject json, DynamicParameters p, string param, string name, string type)
        {
            switch (type)
            {
                case "int":
                    json.Add(name, p.Get<int?>(param));
                    break;
                case "smallint":
                    json.Add(name, p.Get<int?>(param));
                    break;
                case "tinyint":
                    json.Add(name, p.Get<int?>(param));
                    break;
                case "float":
                    json.Add(name, p.Get<double?>(param));
                    break;
                case "bit":
                    json.Add(name, p.Get<bool?>(param));
                    break;
                case "uniqueidentifier":
                    json.Add(name, p.Get<Guid?>(param));
                    break;
                case "date":
                    json.Add(name, p.Get<DateTime?>(param));
                    break;
                case "datetime":
                    json.Add(name, p.Get<DateTime?>(param));
                    break;
                default:
                    json.Add(name, p.Get<string?>(param));
                    break;
            }

        }

    }
}
