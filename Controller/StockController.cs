using Microsoft.AspNetCore.Mvc;
using I3S_API.Lib;
using Dapper;
using I3S_API.Model;
using I3S_API.Filter;

namespace I3S_API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        /// <summary>
        /// 取得股票K棒
        /// </summary>
        [HttpGet("Candles")]
        public IActionResult GetCandles(string code)
        {
            string strsql = @"select [code], [time], [open], [high], [low], [close], [value], [color] from vd_StockCandle where Code = @code";

            using (var db = new AppDb())
            {

                var data = db.Connection.Query(strsql, new { code });
                return Ok(data);
            }
        }

        /// <summary>
        /// 取得股票資訊
        /// </summary>
        [HttpGet("Info")]
        public IActionResult GetStockInfo(string code)
        {
            string strsql = @"select top 1 S.*, E.Eps, Round(S.ClosingPrice / E.Eps, 2) 'PerdictPE'
                                from vd_StockDayInfo S
                                left join [Raw].[dbo].[StockEpsPredict_2025] E
                                on S.Code = E.Code
                                where S.Code = @code
                                order by Date desc";

            using (var db = new AppDb())
            {
                var data = db.Connection.QueryFirstOrDefault(strsql, new { code });

                if(data is null)
                {
                    bool status = false;
                    string message = "股票代碼不存在！";
                    return NotFound(new { status, message });
                }

                return Ok(data);
            }
        }

        /// <summary>
        /// 搜尋股票
        /// </summary>
        [HttpGet("Search")]
        public IActionResult SearchStock(string searchstr)
        {
            string strsql = @"declare @text nvarchar(max) = dbo.fn_ConvertQuanPin(@searchstr);
                            select top 50 len(Code) len, Code, (Code + ' ' + Name + ' (' + isNull(Market, '') + iif(Industry is not null, ' ' + Industry, '') + ')') as 'text'
                            from vd_Stock 
                            where (Code + ' ' + Pinyin) like '%' + @text + '%'
                            order by len";

            using (var db = new AppDb())
            {
                var data = db.Connection.Query(strsql, new { searchstr });
                return Ok(data);
            }
        }
    }
}