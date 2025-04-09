using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace I3S_API.Lib
{
    public class AppDb : IDisposable
    {
        public SqlConnection Connection;
        public AppDb(string sqlmode = "Default")
        {
            string ConnectionStrings = AppConfig.Config[$"ConnectionStrings:{sqlmode}"];
            
            //若帳號連線資訊為null，則使用預設連線，以下先註解以免Public API使用到具有Owner權限帳號
            //if (ConnectionStrings.IsNullOrEmpty()) {
            //    ConnectionStrings = AppConfig.Config[$"ConnectionStrings:Default"];
            //}

            Connection = new SqlConnection(ConnectionStrings);
        }
        public void Dispose()
        {

            Connection.Close();
            Connection.Dispose();
        }
    }
}
