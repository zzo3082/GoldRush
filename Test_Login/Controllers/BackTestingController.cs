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
            return View();
        }

        [HttpPost]
        public ActionResult BackTesting001(string stockID, string day, string totalUnit)
        {
            string[] years = { "2021", "2022" };
            string[] months = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            double s = 1.425 / 1000;    // 手續費千分之1.425 (不計網路下單折數)
            int total_cost = 0;         // 總付出成本
            int stock_number = int.Parse(totalUnit);       // 統計一共持有幾股, 值暫定為每個月固定股數

            if (int.Parse(day) < 10) { day = $"0{day}"; }  // 日期(日) 當<10 在十位數補上"0"
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
                    if (checkDate.Count != 0)
                    {
                        endPrice_everyMon.Add(Convert.ToDouble(checkDate[0]));
                    }
                    else
                    {
                        if (int.Parse($"{year}{month}{day}") > int.Parse(nowVal_Date)) { break; }
                        SqlConnection cn = new SqlConnection(@"Data Source=.\SQLEXPRESS;Initial Catalog=Lab;Integrated Security=True");
                        SqlCommand cmd = new SqlCommand("select top (1)  stockDate from [stockPrice] " +
                                $"where　stockDate > '{year}{month}{day}'　order by stockDate", cn);
                        cn.Open();
                        string nextday = cmd.ExecuteScalar().ToString();
                        cn.Close();
                        checkDate = db.stockPrice
                        .Where(x => x.stockID == stockID && x.stockDate == nextday)
                        .Select(x => x.endPrice)
                        .ToList();
                        if (checkDate.Count != 0)
                        {
                            endPrice_everyMon.Add(Convert.ToDouble(checkDate[0]));
                        }
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
            ViewBag.total_cost = total_cost;
            ViewBag.nowTotalVal = nowTotalVal;
            ViewBag.profit_and_loss = profit_and_loss;
            ViewBag.percentage = percentage;
            return View();
        }
        public ActionResult BackTesting002()
        {
            return View();
        }

    }

}