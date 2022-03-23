using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
            if(id == null)
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
                if(stock.Count != 0)
                {
                    ViewBag.id = stock.First().stockName + "(" + stock.First().stockID + ")";
                }
                return View(stock);
            }
        }

        public ActionResult Strategy(string str)
        {
            string id = "Strategy";
            string stringID = "";
            string stockArray =  "";
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
                    stockArray += ", 2303";
                    stockArray += ", 2330";
                    break;
                case "四海遊龍":
                    stockArray += ", 0050";
                    stockArray += ", 2603";
                    break;
                case "強弱勢股":
                    stockArray += ", 2609";
                    stockArray += ", 2409";
                    break;
                case "外資連買":
                    stockArray += ", 2610";
                    stockArray += ", 2618";
                    break;
                case "投信連買":
                    stockArray += ", 3481";
                    stockArray += ", 2002";
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
                default:
                    break;
            }
            ViewBag.id = id;
            ViewBag.stringID = stringID;
            if(stockArray == "")
            {
                return View();
            }
            else
            {
                return View(db.stockPrice.Where(x => stockArray.Contains(x.stockID)).OrderBy(x => x.stockDate).ToList());
            }
        }

        public ActionResult Customize()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Customize(DateTime? selectDate, string priceType, string minValue, string maxValue)
        {
            if(selectDate == null || priceType == null || minValue == null || maxValue == null)
            {
                return View();
                // minValue and maxValue could only have one
            }else
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

        public ActionResult StockMarketIndex()
        {
            return View();
        }

        [HttpPost]
        public ActionResult StockMarketIndex( string tech1)
        {

            ViewBag.tech1 = tech1;
            ViewBag.date1 = "20220210";
            var stock = db.stockPrice.Where(x => x.stockID == "2330").OrderBy(x => x.stockDate).ToList();

            List<StockKDJ> kdj = StockFunction.ComputationKDJ(9, 3, 3, stock);
            List<StockMACD> macd = StockFunction.ComputationMACD(12, 26, 9, stock);
            return View(stock);
        }
    }
}
