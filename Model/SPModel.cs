using Dapper;

namespace I3S_API.Model
{

    public class SPModel
    {
        public string? wke { get; set; }
        public SPModel()
        {
            wke = "做中學";
        }

    }

    public class SPInOut
    {
        public DynamicParameters p { get; set; }
        public List<dynamic> outputs { get; set; }

        public dynamic columnjson { get; set; }

    }

    public class SPView
    {
        public DynamicParameters p { get; set; }
        public string sqlstring { get; set; }

    }

}
