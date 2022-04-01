using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Test_Login.Models;

namespace Test_Login.Controllers
{
    public class BackTestingController : Controller
    {
        public LabEntities db = new LabEntities();

        // GET: BackTesting
        public ActionResult BackTesting001()
        {

            var q = db.stockPrice.Select(x => x.stockID + x.stockName).Distinct().OrderBy(x => x).ToList();
            ViewBag.stockID = q;
            return View();
        }
        string[] years = { "2021", "2022" };
        string[] months = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
        double s = 1.425 / 1000;    // 手續費千分之1.425 (不計網路下單折數)
        int total_cost = 0;         // 總付出成本
        [HttpPost]
        //定期定股Action
        public JsonResult BackTestingRes001(string stockID, string day, string totalUnit)
        {
            int stock_number = int.Parse(totalUnit);       // 統計一共持有幾股, 值暫定為每個月固定股數
            if (int.Parse(day) < 10 && day.Length < 2) { day = $"0{day}"; }  // 日期(日) 當<10 在十位數補上"0"
            string nowVal = db.stockPrice       // 取得資料庫中指定股票最後收盤價
                .Where(x => x.stockID == stockID)
                .OrderByDescending(x => x.stockDate).Select(x => x.endPrice).ToList()[0];
            string nowVal_Date = db.stockPrice       // 取得資料庫中最後收盤價的日期
                .Where(x => x.stockID == stockID)
                .OrderByDescending(x => x.stockDate).Select(x => x.stockDate).ToList()[0];
            List<double> endPrice_everyMon = new List<double>(); // 存放每個月指定日的收盤價

            foreach (string year in years)
            {
                foreach (string month in months)
                {
                    string temp = $"{year}{month}{day}";
                    var checkDate = db.stockPrice
                        .Where(x => x.stockID == stockID && x.stockDate == temp)
                        .Select(x => x.endPrice)
                        .ToList();
                    if (checkDate.Count != 0) { endPrice_everyMon.Add(Convert.ToDouble(checkDate[0])); }
                    else
                    {
                        if (int.Parse($"{year}{month}{day}") > int.Parse(nowVal_Date)) { break; }
                        SqlConnection cn = new SqlConnection(@"Data Source=.;Initial Catalog=Lab;Integrated Security=True");
                        SqlCommand cmd = new SqlCommand("select top (1)  stockDate from [stockPrice] " +
                                $"where　stockDate > '{year}{month}{day}'　order by stockDate", cn);
                        cn.Open();
                        string nextday = cmd.ExecuteScalar().ToString();
                        cn.Close();
                        checkDate = db.stockPrice
                        .Where(x => x.stockID == stockID && x.stockDate == nextday)
                        .Select(x => x.endPrice)
                        .ToList();
                        if (checkDate.Count != 0) { endPrice_everyMon.Add(Convert.ToDouble(checkDate[0])); }
                    }
                }
            }

            foreach (int i in endPrice_everyMon) // 把每月成本疊加到total_cost
            {
                // 每月成本 =               價格        *     數量      *手續費   (int)(+0.5四捨五入)
                total_cost += (int)(Convert.ToDouble(i) * stock_number * (1 + s) + 0.5);
            }
            stock_number *= endPrice_everyMon.Count();          //  總股數 = 每月買進股數 * 買進幾個月
            int nowTotalVal = (int)(stock_number * double.Parse(nowVal)); // 當前市值 = 總股數 * 資料庫最後一筆收盤價
            int profit_and_loss = nowTotalVal - total_cost;     //  未實現損益 = 市值 - 總成本
            double percentage = Math.Round((double)profit_and_loss / total_cost, 2);
            string stockName = db.stockPrice.Where(x => x.stockID == stockID).Select(x => x.stockName).ToList()[0];
            ViewBag.stockName = stockName;              // 股票名稱
            ViewBag.stockID = stockID;                  // 股票ID
            ViewBag.day = day;                          // 買進日
            ViewBag.count = totalUnit;                  // 買X股
            ViewBag.total_cost = total_cost;            // 總成本
            ViewBag.nowTotalVal = nowTotalVal;          // 當前市值
            ViewBag.profit_and_loss = profit_and_loss;  // 盈虧
            ViewBag.percentage = percentage * 100;      // 盈虧百分比
            var result = new
            {
                stockName = stockName,
                stockID = stockID,
                day = day,
                count = totalUnit,
                total_cost = total_cost,
                nowTotalVal = nowTotalVal,
                profit_and_loss = profit_and_loss,
                percentage = percentage * 100
            };
            return Json(result);
        }

        // 定期定額Action
        [HttpPost]
        public JsonResult BackTestingRes002(string stockID, string day, string totalUnit)
        {
            total_cost = 0;
            int stock_number = int.Parse(totalUnit);       // 每月投入之金額
            if (int.Parse(day) < 10 && day.Length < 2) { day = $"0{day}"; }  // 日期(日) 當<10 在十位數補上"0"
            string nowVal = db.stockPrice       // 取得資料庫中指定股票最後收盤價
                .Where(x => x.stockID == stockID)
                .OrderByDescending(x => x.stockDate).Select(x => x.endPrice).ToList()[0];
            string nowVal_Date = db.stockPrice       // 取得資料庫中最後收盤價的日期
                .Where(x => x.stockID == stockID)
                .OrderByDescending(x => x.stockDate).Select(x => x.stockDate).ToList()[0];
            List<double> endPrice_everyMon = new List<double>(); // 存放每個月指定日的收盤價

            foreach (string year in years)
            {
                foreach (string month in months)
                {
                    string temp = $"{year}{month}{day}";
                    var checkDate = db.stockPrice
                        .Where(x => x.stockID == stockID && x.stockDate == temp)
                        .Select(x => x.endPrice)
                        .ToList();
                    if (checkDate.Count != 0) { endPrice_everyMon.Add(Convert.ToDouble(checkDate[0])); } //如果.where日期 有資料,把收盤價加入
                    else //如果.where日期沒資料, 找出距離指定日期最近的下個交易日, 再把收盤價加入
                    {
                        if (int.Parse($"{year}{month}{day}") > int.Parse(nowVal_Date)) { break; }
                        SqlConnection cn = new SqlConnection(@"Data Source=.;Initial Catalog=Lab;Integrated Security=True");
                        SqlCommand cmd = new SqlCommand("select top (1)  stockDate from [stockPrice] " +
                                $"where　stockDate > '{year}{month}{day}'　order by stockDate", cn);
                        cn.Open();
                        string nextday = cmd.ExecuteScalar().ToString();
                        cn.Close();
                        checkDate = db.stockPrice
                        .Where(x => x.stockID == stockID && x.stockDate == nextday)
                        .Select(x => x.endPrice)
                        .ToList();
                        if (checkDate.Count != 0) { endPrice_everyMon.Add(Convert.ToDouble(checkDate[0])); }
                    }
                }
            }
            List<int> maxCountByMonth = new List<int>();
            foreach (int i in endPrice_everyMon) // 把每月成本疊加到total_cost
            {
                // 計算每月成本 = (int)(收盤價/可投入金額*(1+手續稅)+0.5) = 可以買x股
                // 成本(月) =  x股          * 收盤價(i) * 手續費(1+s)
                maxCountByMonth.Add((int)(int.Parse(totalUnit) / i));
                total_cost += (int)(int.Parse(totalUnit) / i * i * (1 + s) + 0.5);
            }
            maxCountByMonth.Sum();        //  總股數 = 每月買進股數 * 買進幾個月
            int nowTotalVal = (int)(maxCountByMonth.Sum() * double.Parse(nowVal)); // 當前市值 = 總股數 * 資料庫最後一筆收盤價
            int profit_and_loss = nowTotalVal - total_cost;     //  未實現損益 = 市值 - 總成本
            double percentage = Math.Round((double)profit_and_loss / total_cost, 2);
            string stockName = db.stockPrice.Where(x => x.stockID == stockID).Select(x => x.stockName).ToList()[0];
            ViewBag.stockName2 = stockName;              // 股票名稱
            ViewBag.stockID = stockID;                  // 股票ID
            ViewBag.day2 = day;                          // 買進日
            ViewBag.count2 = totalUnit;                  // 買X金額
            ViewBag.total_cost2 = total_cost;            // 總成本
            ViewBag.nowTotalVal2 = nowTotalVal;          // 當前市值
            ViewBag.profit_and_loss2 = profit_and_loss;  // 盈虧
            ViewBag.percentage2 = percentage * 100;            // 盈虧百分比
            var result = new
            {
                stockName = stockName,
                stockID = stockID,
                day = day,
                count = totalUnit,
                total_cost = total_cost,
                nowTotalVal = nowTotalVal,
                profit_and_loss = profit_and_loss,
                percentage = percentage * 100
            };
            return Json(result);
        }


    }

}