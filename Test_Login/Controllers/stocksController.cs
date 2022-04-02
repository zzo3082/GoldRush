using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Test_Login;
using Test_Login.Models;

namespace GoldRush.Controllers
{
    public class stocksController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        private LabEntities db = new LabEntities();
        // GET: stocks
        public ActionResult Index()
        {
            // return View(db.stockPrice.ToList());
            int i = 0;
            return View();
        }


        public ActionResult Chart(string id)
        {

            //var q = db.stockPrice.Select(x => x.stockID + x.stockName).Distinct().OrderBy(x => x).ToList();
            //ViewBag.stockID = q;
            if (id == null)
            {
                return View("Index");
            }
            else
            {
                var stock = db.stockPrice.Where(x => x.stockID == id).OrderBy(x => x.stockDate).ToList();
                if (stock.Count != 0)
                {
                    ViewBag.id = stock.First().stockName + "(" + stock.First().stockID + ")";
                    ViewBag.stockID = stock.First().stockID;
                }
                return View(stock);
            }
        }

        //[HttpPost]
        //public ActionResult Chart(string stockID, string id)
        //{
        //    if (stockID == "")
        //    {
        //        return View("Index");
        //    }
        //    else
        //    {
        //        var stock = db.stockPrice.Where(x => x.stockID == stockID || x.stockName == stockID).OrderBy(x => x.stockDate).ToList();
        //        if (stock.Count != 0)
        //        {
        //            ViewBag.id = stock.First().stockName + "(" + stock.First().stockID + ")";
        //            ViewBag.stockID = stock.First().stockID;
        //        }
        //        return View(stock);
        //    }
        //}

        [HttpPost]
        public async Task<string> Chart(bool isLike, string stockID)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (isLike)
            {
                user.StockBag += stockID + " ";
            }
            else
            {
                user.StockBag = user.StockBag.Replace(stockID, "");
            }
            await UserManager.UpdateAsync(user);
            return "";
        }


        public ActionResult Strategy(string str)
        {
            // StockPrice的StockDate倒著排
            var dateList = db.stockPrice.OrderByDescending(x => x.stockDate).Select(x => x.stockDate).ToList().Distinct();
            var q = db.stockPrice.Select(x => x.stockID + x.stockName).Distinct().OrderBy(x => x).ToList();
            ViewBag.stockID = q;
            SqlConnection cn = new SqlConnection(@"Data Source=.;Initial Catalog=Lab;Integrated Security=True");
            string id = "Strategy";
            string stringID = "";
            string stockArray = "";
            int selectResultcount = 0;

            #region 資料庫末5筆日期(已不需要)
            // 取得資料庫最後5筆日期, 反著讀取依序加入dayList
            //方法1
            //SqlCommand getLast5Day = new SqlCommand();
            //getLast5Day.Connection = cn;
            //getLast5Day.CommandText = "select distinct top(5) stock_date from buy_and_sell_report order by stock_date desc ";
            //cn.Open();
            //SqlDataReader dayResult = getLast5Day.ExecuteReader();
            //List<string> dayList = new List<string>();
            //while (dayResult.Read()) { dayList.Add(Convert.ToString(dayResult[0])); }
            //cn.Close();

            //方法2
            //List<string> dayList = new List<string>();
            //for (int i = 0; i < 5; i++)
            //{
            //    dayList.Add(dateList.ElementAt(i));
            //}
            #endregion
            switch (str)
            {
                case "成交爆大量":
                    //foreach (string s in db.stockPrice.Select(x => x.stockID).Distinct().OrderBy(x => x))
                    //{
                    //    var dbs = db.stockPrice.Where(x => x.stockID == s).ToList();
                    //    try
                    //    {
                    //        if (float.Parse(dbs.Where(x => x.stockDate == "20210409").Select(x => x.endPrice).ToList()[0]) > 900)
                    //        {
                    //            stockArray = stockArray + s + " ";
                    //        }
                    //    }
                    //    catch
                    //    {
                    //    }
                    //}
                    foreach (string s in db.stockPrice.Select(x => x.stockID).Distinct().OrderBy(x => x))
                    {
                        var dbs = db.stockPrice.Where(x => x.stockID == s).OrderByDescending(x => x.stockDate).Take(6).ToList();
                        try
                        {
                            if (dbs[0].numOfSharesTrade > dbs.GetRange(1, 5).Select(x => x.numOfSharesTrade).Sum())
                            {
                                stockArray += s + ", ";
                            }
                        }
                        catch
                        {

                        }

                    }
                    break;
                case "四海遊龍":
                    var db2 = db.stockPrice.Where(x => x.stockID == "2330").OrderByDescending(x => x.stockDate).ToList();
                    double sma5 = db2.GetRange(1, 5).Select(x => Convert.ToDouble(x.endPrice)).Average();
                    double sma10 = db2.GetRange(1, 10).Select(x => Convert.ToDouble(x.endPrice)).Average();
                    double sma20 = db2.GetRange(1, 20).Select(x => Convert.ToDouble(x.endPrice)).Average();
                    double sma60 = db2.GetRange(1, 60).Select(x => Convert.ToDouble(x.endPrice)).Average();
                    double[] sma = new double[] { sma5, sma10, sma20, sma60 };

                    if (double.Parse(db2[0].endPrice) > sma.Max())
                    {
                        stockArray += ", 2330";
                    }
                    stockArray += ", 0050";
                    stockArray += ", 2603";
                    break;
                case "強勢股票":
                    // Convert.ToDouble??
                    var db3 = db.stockPrice.Where(x => x.stockDate == "20220210").ToList();
                    var dbs_Top10 = db3.Select(x => new { date = x.stockID, value = (Convert.ToDouble(x.endPrice) - Convert.ToDouble(x.openPrice)) }).OrderByDescending(x => x.value).ToList();
                    stockArray += ", 2609";
                    stockArray += ", 2409";
                    break;

                case "外資連買":
                    #region 外資查詢連買結果
                    using (SqlCommand getStockID = new SqlCommand())
                    {
                        getStockID.Connection = cn;
                        cn.Open();
                        getStockID.CommandText = "select StockCode, count(StockCode) as countres " +
                            "from buy_and_sell_report " +
                            "Where globalCompany > 500000 " +
                            $"and (stock_date between {dateList.ElementAt(4)} and {dateList.ElementAt(0)})" +
                            "group by StockCode	" +
                            "order by countres desc";
                        SqlDataReader stockIDReader = getStockID.ExecuteReader();
                        while (stockIDReader.Read())
                        {
                            if (int.Parse(stockIDReader[1].ToString()) >= 5)    // stockIDReader[1] = count(StockCode)
                            {
                                selectResultcount += 1;
                                stockArray += $", {stockIDReader[0]}";
                            }
                        }
                        cn.Close();
                    }
                    ViewBag.resultCount = selectResultcount;
                    #endregion
                    break;
                case "投信連買":
                    #region 投信查詢連買結果
                    using (SqlCommand getStockID = new SqlCommand())
                    {
                        getStockID.Connection = cn;
                        cn.Open();
                        getStockID.CommandText = "select StockCode, count(StockCode) as countres " +
                            "from buy_and_sell_report " +
                            "Where investmentTrust > 500000 " +
                            $"and (stock_date between {dateList.ElementAt(4)} and {dateList.ElementAt(0)})" +
                            "group by StockCode	" +
                            "order by countres desc";
                        SqlDataReader stockIDReader = getStockID.ExecuteReader();
                        while (stockIDReader.Read())
                        {
                            if (int.Parse(stockIDReader[1].ToString()) >= 5)    // stockIDReader[1] = count(StockCode)
                            {
                                selectResultcount += 1;
                                stockArray += $", {stockIDReader[0]}";
                            }
                        }
                        cn.Close();
                    }
                    #endregion
                    ViewBag.resultCount = selectResultcount;
                    break;
                case "外資投信同買":
                    #region 外資投信同買超過100張(含)且漲幅>2%
                    List<string> stockCodeList = new List<string>(); // 符合規則的股票代號
                    List<string> AmplitudeList = new List<string>(); // 漲幅>=2%以上的股票
                    using (SqlCommand sqlcmd = new SqlCommand()) // 外資投信同買100張+
                    {
                        sqlcmd.Connection = cn;
                        sqlcmd.CommandText = "select StockCode from buy_and_sell_report " +
                            "where globalCompany > 100000" +
                            "and investmentTrust > 100000" +
                            $"and stock_date = {dateList.First()}" +
                            $"order by StockCode";
                        cn.Open();
                        SqlDataReader stockIDReader = sqlcmd.ExecuteReader();
                        while (stockIDReader.Read()) { stockCodeList.Add(stockIDReader[0].ToString()); }
                        cn.Close();
                    }
                    foreach (string stockID in stockCodeList) //漲幅>=2%
                    {
                        using (SqlCommand sqlcmd = new SqlCommand()) // 最新一筆及前一日的收盤價
                        {
                            sqlcmd.Connection = cn;
                            sqlcmd.CommandText = "select endPrice, stockDate" +
                                " from stockPrice" +
                                $" where stockID = {stockID}" +
                                $" and (stockDate = {dateList.First()} or stockDate = {dateList.Skip(1).First()})" +
                                "order by stockDate desc";
                            cn.Open();
                            SqlDataReader amplitudeReader = sqlcmd.ExecuteReader();
                            double temp1 = 0; // 最新收盤價
                            double temp2 = 0; // 最新前一日收盤價
                            while (amplitudeReader.Read())     // 漲幅>2%加入清單
                            {
                                if (amplitudeReader[1].ToString() == dateList.First())
                                {
                                    temp1 = Convert.ToDouble(amplitudeReader[0]);
                                }
                                else temp2 = Convert.ToDouble(amplitudeReader[0]);

                                if (temp1 != 0 && temp2 != 0)
                                {
                                    if ((temp1 - temp2) / temp2 >= 0.02)
                                    {
                                        //AmplitudeList.Add(stockCodeTemp);
                                        AmplitudeList.Add(stockID);
                                        temp1 = 0; temp2 = 0; //stockCodeTemp = "";
                                    }
                                    else { temp1 = 0; temp2 = 0; }
                                }
                            }
                            cn.Close();
                        }
                    }
                    foreach(string result in AmplitudeList)
                    {
                        selectResultcount += 1;
                        stockArray += $", {result}";
                    }
                    ViewBag.resultCount = selectResultcount;
                    #endregion
                    break;

                case "EPS創新高":
                    stockArray += ", 2883";
                    stockArray += ", 2888";
                    break;
                case "營收由虧轉盈":
                    stockArray += ", 3037";
                    stockArray += ", 2371";
                    break;
                case "弱勢股票":
                    stockArray += "2883";
                    break;
                case "外資連賣":
                    #region 外資查詢連賣結果
                    using (SqlCommand getStockID = new SqlCommand())
                    {
                        getStockID.Connection = cn;
                        cn.Open();
                        getStockID.CommandText = "select StockCode, count(StockCode) as countres " +
                            "from buy_and_sell_report " +
                            "Where globalCompany < -1000000 " +
                            $"and (stock_date between {dateList.ElementAt(4)} and {dateList.ElementAt(0)})" +
                            "group by StockCode	" +
                            "order by countres desc";
                        SqlDataReader stockIDReader = getStockID.ExecuteReader();
                        while (stockIDReader.Read())
                        {
                            if (int.Parse(stockIDReader[1].ToString()) >= 5)    // stockIDReader[1] = count(StockCode)
                            {
                                selectResultcount += 1;
                                stockArray += $", {stockIDReader[0]}";
                            }
                        }
                        cn.Close();
                    }
                    #endregion
                    ViewBag.resultCount = selectResultcount;
                    break;
                case "投信連賣":
                    #region 投信查詢連賣結果
                    using (SqlCommand getStockID = new SqlCommand())
                    {
                        getStockID.Connection = cn;
                        cn.Open();
                        getStockID.CommandText = "select StockCode, count(StockCode) as countres " +
                            "from buy_and_sell_report " +
                            "Where investmentTrust < -500000 " +
                            $"and (stock_date between {dateList.ElementAt(4)} and {dateList.ElementAt(0)})" +
                            "group by StockCode	" +
                            "order by countres desc";
                        SqlDataReader stockIDReader = getStockID.ExecuteReader();
                        while (stockIDReader.Read())
                        {
                            if (int.Parse(stockIDReader[1].ToString()) >= 5)    // stockIDReader[1] = count(StockCode)
                            {
                                selectResultcount += 1;
                                stockArray += $", {stockIDReader[0]}";
                            }
                        }
                        cn.Close();
                    }
                    ViewBag.resultCount = selectResultcount;
                    #endregion
                    break;
                case "外資投信同賣":
                    #region 外資投信同賣超過100張(含)且跌幅>2%
                    List<string> stockCodeList2 = new List<string>(); // 符合規則的股票代號
                    List<string> AmplitudeList2 = new List<string>(); // 跌幅>=2%以上的股票
                    using (SqlCommand sqlcmd = new SqlCommand()) // 外資投信同賣100張+
                    {
                        sqlcmd.Connection = cn;
                        sqlcmd.CommandText = "select StockCode from buy_and_sell_report " +
                            "where globalCompany < -100000" +
                            "and investmentTrust < -100000" +
                            $"and stock_date = {dateList.First()}" +
                            $"order by StockCode";
                        cn.Open();
                        SqlDataReader stockIDReader = sqlcmd.ExecuteReader();
                        while (stockIDReader.Read()) { stockCodeList2.Add(stockIDReader[0].ToString()); }
                        cn.Close();
                    }
                    foreach (string stockID in stockCodeList2) //跌幅>=2%
                    {
                        using (SqlCommand sqlcmd = new SqlCommand()) // 最新一筆及前一日的收盤價
                        {
                            sqlcmd.Connection = cn;
                            sqlcmd.CommandText = "select endPrice, stockDate" +
                                " from stockPrice" +
                                $" where stockID = {stockID}" +
                                $" and (stockDate = {dateList.First()} or stockDate = {dateList.Skip(1).First()})" +
                                "order by stockDate desc";
                            cn.Open();
                            SqlDataReader amplitudeReader = sqlcmd.ExecuteReader();
                            double temp1 = 0; // 最新收盤價
                            double temp2 = 0; // 最新前一日收盤價
                            while (amplitudeReader.Read())     // 跌幅>2%加入清單
                            {
                                if (amplitudeReader[1].ToString() == dateList.First())
                                {
                                    temp1 = Convert.ToDouble(amplitudeReader[0]);
                                }
                                else temp2 = Convert.ToDouble(amplitudeReader[0]);

                                if (temp1 != 0 && temp2 != 0)
                                {
                                    if ((temp1 - temp2) / temp2 <= -0.02)
                                    {
                                        //AmplitudeList.Add(stockCodeTemp);
                                        AmplitudeList2.Add(stockID);
                                        temp1 = 0; temp2 = 0; //stockCodeTemp = "";
                                    }
                                    else { temp1 = 0; temp2 = 0; }
                                }
                            }
                            cn.Close();
                        }
                    }
                    foreach (string result in AmplitudeList2)
                    {
                        selectResultcount += 1;
                        stockArray += $", {result}";
                    }
                    ViewBag.resultCount = selectResultcount;
                    #endregion
                    break;
                default:
                    break;
            }
            ViewBag.id = id;
            ViewBag.stringID = stringID;
            if (stockArray == "")
            {
                return View();
            }
            else
            {
                return View(db.stockPrice.Where(x => stockArray.Contains(x.stockID)).OrderBy(x => x.stockID).ToList());
            }
        }

        public ActionResult Customize()
        {
            var q = db.stockPrice.Select(x => x.stockID + x.stockName).Distinct().OrderBy(x => x).ToList();
            ViewBag.stockID = q;
            return View();
        }

        [HttpPost]
        public ActionResult Customize(DateTime? selectDate, string priceType, string minValue, string maxValue)
        {
            if (selectDate == null || priceType == null || minValue == null || maxValue == null)
            {
                return View();
                // minValue and maxValue could only have one
            }
            else
            {
                ViewBag.priceType = priceType;
                ViewBag.minValue = minValue;
                ViewBag.maxValue = maxValue;
                string priceTypeEnglish = "";
                switch (priceType)
                {
                    case "開盤價":
                        priceTypeEnglish = "openPrice"; break;
                    case "最高價":
                        priceTypeEnglish = "highPrice"; break;
                    case "最低價":
                        priceTypeEnglish = "lowPrice"; break;
                    case "收盤價":
                        priceTypeEnglish = "endPrice"; break;
                    default:
                        break;
                }

                //dbs[0].GetType().GetProperty(priceTypeEnglish).GetValue(dbs[0])
                //float.Parse(dbs[0].GetType().GetProperty(priceTypeEnglish).GetValue(dbs[0]).ToString())
                string stockArray = "";
                string Date1 = string.Format("{0:yyyyMMdd}", selectDate);

                // date1 because that stock at the date could have no value
                ViewBag.date1 = Date1;
                float minV = float.Parse(minValue);
                foreach (string s in db.stockPrice.Select(x => x.stockID).Distinct().OrderBy(x => x))
                {

                    var dbs = db.stockPrice.Where(x => x.stockID == s).ToList();
                    try
                    {
                        if (float.Parse(dbs.Where(x => x.stockDate == Date1).Select(x => x.GetType().GetProperty(priceTypeEnglish).GetValue(x).ToString()).ToList()[0]) > minV)
                        {
                            stockArray = stockArray + s + " ";
                        }
                    }
                    catch
                    {
                    }
                }
                return View(db.stockPrice.Where(x => stockArray.Contains(x.stockID)).OrderBy(x => x.stockDate).ToList());
            }
        }

        [HttpPost]
        public PartialViewResult StockResult(string storedKDJ1, string storedKDJ2, string StockArray,
                                             string kdj1, string kdj1_kdj1, string kdj1_day1, string kdj1_dir1, string kdj1_value1,
                                             string kdj2, string kdj2_kdj1, string kdj2_dir1, string kdj2_day1, string kdj2_kdj2)
        {
            string stockCustomize = StockArray;
            // kdj1 strategy checked then calculate the corresponding stockID
            string resultKDJ1 = storedKDJ1;
            if (kdj1 == "true")
            {
                // Thread.Sleep(5000);
                resultKDJ1 = "2330";
                stockCustomize += resultKDJ1 + " ";
            }
            else if (storedKDJ1 != "")
            {
                stockCustomize = stockCustomize.Replace(storedKDJ1, "");
            }

            // kdj2 strategy checked then calculate the corresponding stockID
            string resultKDJ2 = storedKDJ2;
            if (kdj2 == "true")
            {
                resultKDJ2 = "0050";
                stockCustomize += resultKDJ2 + " ";
            }
            else if (storedKDJ2 != "")
            {
                stockCustomize = stockCustomize.Replace(storedKDJ2, "");
            }

            StoredKDJ result = new StoredKDJ
            {
                KDJ1 = resultKDJ1,
                KDJ2 = resultKDJ2,
                StockArray = stockCustomize
            };
            ViewBag.result = result;

            // string.split by " " then get the intersection
            return PartialView(db.stockPrice.Where(x => stockCustomize.Contains(x.stockID)).OrderBy(x => x.stockDate).ToList());
        }

        public ActionResult StockMarketIndex()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StockMarketIndex(string tech1, string test1, string test2)
        {

            ViewBag.tech1 = tech1;
            ViewBag.date1 = "20220210";
            var stock = db.stockPrice.Where(x => x.stockID == "2330").OrderBy(x => x.stockDate).ToList();

            List<StockKDJ> kdj = StockFunction.ComputationKDJ(9, 3, 3, stock);
            List<StockMACD> macd = StockFunction.ComputationMACD(12, 26, 9, stock);
            List<StockSMA> sma = StockFunction.ComputationSMA(stock, 9);
            List<StockSMA> sma20 = StockFunction.ComputationSMA(stock, 20);
            List<StockEMA> ema = StockFunction.ComputationEMA(stock, 9);

            ViewBag.test1 = test1;
            ViewBag.test2 = test2;

            var result = new
            {
                tech1 = tech1,
                test1 = test1,
                test2 = test2
            };
            return Json(result);
        }



    }
}
