using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Test_Login.Models;

namespace GoldRush.Controllers
{
    public class stocksController : Controller
    {
        private LabEntities db = new LabEntities();
        // GET: stocks
        public ActionResult Index()
        {
            // return View(db.stockPrice.ToList());
            return View();
        }


        public ActionResult Chart(string id)
        {
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
                }
                return View(stock);
            }
        }

        [HttpPost]
        public ActionResult Chart(string stockID, string id)
        {

            if (stockID == "")
            {
                return View("Index");
            }
            else
            {
                var stock = db.stockPrice.Where(x => x.stockID == stockID || x.stockName == stockID).OrderBy(x => x.stockDate).ToList();
                if (stock.Count != 0)
                {
                    ViewBag.id = stock.First().stockName + "(" + stock.First().stockID + ")";
                }
                return View(stock);
            }
        }


        public ActionResult Strategy(string str)
        {
            SqlConnection cn = new SqlConnection(@"Data Source=.;Initial Catalog=Lab;Integrated Security=True");
            string id = "Strategy";
            string stringID = "";
            string stockArray = "";

            #region 資料庫末5筆日期
            // 取得資料庫最後5筆日期, 反著讀取依序加入dayList
            SqlCommand getLast5Day = new SqlCommand();
            getLast5Day.Connection = cn;
            getLast5Day.CommandText = "select distinct top(5) stock_date from buy_and_sell_report order by stock_date desc ";
            cn.Open();
            SqlDataReader dayResult = getLast5Day.ExecuteReader();
            List<string> dayList = new List<string>();
            while (dayResult.Read()) { dayList.Add(Convert.ToString(dayResult[0])); }
            cn.Close();
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
                            "Where globalCompany > 100000 " +
                            $"and (stock_date between {dayList.Last()} and {dayList.First()})" +
                            "group by StockCode	" +
                            "order by countres desc";
                        SqlDataReader stockIDReader = getStockID.ExecuteReader();
                        //List<string> globalCompanyList = new List<string>();
                        while (stockIDReader.Read())
                        {
                            if (int.Parse(stockIDReader[1].ToString()) >= 5)    // stockIDReader[1] = count(StockCode)
                            {
                                //globalCompanyList.Add($"{stockIDReader[0]}");
                                stockArray += $", {stockIDReader[0]}";
                            }
                        }
                        cn.Close();
                    }
                    // 查詢出外資連買5天, 且成交張>100的股票代號, 再加入globalCompanyList
                    
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
                            "Where investmentTrust > 100000 " +
                            $"and (stock_date between {dayList.Last()} and {dayList.First()})" +
                            "group by StockCode	" +
                            "order by countres desc";
                        SqlDataReader stockIDReader = getStockID.ExecuteReader();
                        //List<string> globalCompanyList = new List<string>();
                        while (stockIDReader.Read())
                        {
                            if (int.Parse(stockIDReader[1].ToString()) >= 5)    // stockIDReader[1] = count(StockCode)
                            {
                                //globalCompanyList.Add($"{stockIDReader[0]}");
                                stockArray += $", {stockIDReader[0]}";
                            }
                        }
                        cn.Close();
                    }
                    // 查詢出外資連買5天, 且成交張>100的股票代號, 再加入globalCompanyList

                    #endregion
                    break;
                case "KD黃金交叉":
                    stockArray += ", 5608";
                    stockArray += ", 1439";
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
                    stockArray += "8069";
                    break;
                case "投信連賣":
                    stockArray += "3481";
                    break;
                case "KD死亡交叉":
                    stockArray += "6770";
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
                return View(db.stockPrice.Where(x => stockArray.Contains(x.stockID)).OrderBy(x => x.stockDate).ThenBy(x => x.stockID).ToList());
            }
        }

        public ActionResult Customize()
        {
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
                Thread.Sleep(5000);
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
