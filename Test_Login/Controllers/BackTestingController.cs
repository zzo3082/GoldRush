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
            //var checkDate123456 = db.stockPrice
            //            .Where(x => x.stockID == stockID && x.stockDate == "20210101")
            //            .Select(x => x.endPrice)
            //            .ToList();
            //var gjfogdpsjfgpoji = 123252345;

            string[] years = { "2021", "2022" };
            string[] months = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
            double s = 1.425 / 1000;    // 手續費千分之1.425 (不計網路下單折數)
            int total_cost = 0;         // 總付出成本
            int stock_number = 0;       // 統計一共持有幾股
            if (int.Parse(day) < 10) { day = $"0{day}"; }  // 日期(日) 當<10 在十位數補上"0"
            string nowVal = db.stockPrice       // 取得資料庫中指定股票最後收盤價
                .Where(x => x.stockID == stockID)
                .OrderByDescending(x => x.stockDate).Select(x => x.endPrice).ToList()[0];
            string nowVal_Date = db.stockPrice       // 取得資料庫中最後收盤價的日期
                .Where(x => x.stockID == stockID)
                .OrderByDescending(x => x.stockDate).Select(x => x.stockDate).ToList()[0];
            List<double> endPrice_everyMon = new List<double>(); // 存每個月固定日的收盤價

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
                        SqlConnection cn = new SqlConnection("Data Source=localhost;Initial Catalog=Lab;Integrated Security=True");
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

            //SqlConnection cn = new SqlConnection("Data Source=localhost;Initial Catalog=institutionalInvestors;Integrated Security=True");
            //SqlCommand cmd = new SqlCommand($"select endPrice from stockPrice where stockID = {stockID} and stockDate = 202103{day}", cn);
            //cn.Open();
            //string result = cmd.ExecuteScalar().ToString();
            //ViewBag.result = result;
            //cn.Close();
            int asdjoajd = 0;
            foreach(int i in endPrice_everyMon)
            {
                asdjoajd += i;
            }
            return View();
        }
        public ActionResult BackTesting002()
        {
            return View();
        }



    }

}