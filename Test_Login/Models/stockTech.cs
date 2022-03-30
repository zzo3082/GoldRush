using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Test_Login.Models
{
    public class StockFunction
    {
        public static List<StockKDJ> ComputationKDJ(int N, int M1, int M2, List<stockPrice> stockList)
        {
            List<StockKDJ> result = new List<StockKDJ>();
            for (int i = 0; i < stockList.Count; i++)
            {
                double RSV = 0;
                double a = 0;
                double b = 0;
                double e = 0;

                List<double> mM = GetMinMaxPrice(i + 1, N, stockList);
                if (i < N)
                {
                    RSV = 50;
                    a = RSV;
                    b = RSV;
                    e = RSV;
                }
                else
                {
                    RSV = (double.Parse(stockList[i].endPrice) - mM[0]) / (mM[1] - mM[0]) * 100;
                    a = (RSV + (M1 - 1) * result[i - 1].Kvalue) / M1;
                    b = (a + (M2 - 1) * result[i - 1].Dvalue) / M2;
                    e = 3 * a - 2 * b;
                }

                result.Add(new StockKDJ
                {
                    stockDate = stockList[i].stockDate,
                    RSV = RSV,
                    Kvalue = a,
                    Dvalue = b,
                    Jvalue = e,
                });

                if (a < 0) result[i].Kvalue = 0;
                if (a > 100) result[i].Kvalue = 100;
                if (b < 0) result[i].Dvalue = 0;
                if (b > 100) result[i].Dvalue = 100;
                if (e < 0) result[i].Jvalue = 0;
                if (e > 100) result[i].Jvalue = 100;


            }
            return result;
        }

        public static List<double> GetMinMaxPrice(int Index, int N, List<stockPrice> stockList)
        {
            // Index 目前日期， N 需要往前幾天的資料
            // 如果Index太小，有可能不足N天的資料，則使用0至Index全部資料
            var stockRange = new List<stockPrice> { };
            List<double> MinMaxPrice = new List<double> { };
            if (Index < N)
            {
                stockRange = stockList.GetRange(0, Index);
            }
            else
            {
                stockRange = stockList.GetRange(Index - N, N);
            }

            if (stockRange.Count != 0)
            {
                MinMaxPrice.Add(double.Parse(stockRange.Select(x => x.lowPrice).Min()));
                MinMaxPrice.Add(double.Parse(stockRange.Select(x => x.highPrice).Max()));
            }
            return MinMaxPrice;
        }

        public static List<StockMACD> ComputationMACD(int Short, int Long, int M, List<stockPrice> stockList)
        {
            List<StockMACD> result = new List<StockMACD>();
            for (int i = 0; i < stockList.Count; i++)
            {
                if (i == 0)
                {
                    result.Add(new StockMACD
                    {
                        stockDate = stockList[i].stockDate,
                        EmaShortValue = double.Parse(stockList[i].endPrice),
                        EmaLongValue = double.Parse(stockList[i].endPrice),
                        DifValue = 0,
                        DeaValue = 0,
                        MacdValue = 0
                    });
                }
                else
                {
                    double s = (2 * double.Parse(stockList[i].endPrice) + (Short - 1) * result[i - 1].EmaShortValue) / (Short + 1);
                    double l = (2 * double.Parse(stockList[i].endPrice) + (Long - 1) * result[i - 1].EmaLongValue) / (Long + 1);
                    double dif = s - l;
                    double dea = (2 * dif + (M - 1) * result[i - 1].DeaValue) / (M + 1);
                    result.Add(new StockMACD
                    {
                        stockDate = stockList[i].stockDate,
                        EmaShortValue = s,
                        EmaLongValue = l,
                        DifValue = dif,
                        DeaValue = dea,
                        MacdValue = 2.0 * (dif - dea)
                    });
                }
            }


            return result;
        }

        public static List<StockEMA> ComputationEMA(List<stockPrice> stockList, int period)
        {
            List<StockEMA> result = new List<StockEMA>();
            double multiplier = (2.0 / (period + 1));
            double initialSMA = stockList.Select(x => double.Parse(x.endPrice)).Take(period).Average();
            result.Add(new StockEMA
            {
                stockDate = stockList[period - 1].stockDate,
                ema = initialSMA
            });
            for(int i = period; i < stockList.Count; i++)
            {
                var emaValue = (Convert.ToDouble(stockList[i].endPrice) - result.Last().ema) * multiplier + result.Last().ema;
                result.Add(new StockEMA
                {
                    stockDate = stockList[i].stockDate,
                    ema =emaValue
                });
            }
            return result;
        }

        public static List<StockSMA> ComputationSMA(List<stockPrice> StockList, int period)
        {
            List<StockSMA> result = new List<StockSMA>();
            for(int i = 0; i < StockList.Count - period; i++)
            {
                result.Add(new StockSMA
                {
                    stockDate = StockList[i + period - 1].stockDate,
                    sma = StockList.GetRange(i, period).Select(x => Convert.ToDouble(x.endPrice)).Average()
                });
            }
            return result;
        }
    }
    public class StockKDJ
    {
        public string stockDate { get; set; }
        public double RSV { get; set; }
        public double Kvalue { get; set; }
        public double Dvalue { get; set; }
        public double Jvalue { get; set; }
    }

    public class StockMA
    {
        public string stockDate { get; set; }
        public double SMA5 { get; set; }
        public double SMA10 { get; set; }
        public double SMA20 { get; set; }
        public double SMA60 { get; set; }
    }

    public class StockMACD
    {
        public string stockDate { get; set; }
        public double EmaShortValue { get; set; }
        public double EmaLongValue { get; set; }
        public double DifValue { get; set; }
        public double DeaValue { get; set; }
        public double MacdValue { get; set; }
    }

    public class StockSMA
    {
        public string stockDate { get; set; }
        public double sma { get; set; }
    }

    public class StockEMA
    {
        public string stockDate { get; set; }
        public double ema { get; set; }
    }

    public class StoredKDJ
    {
        public string KDJ1 { get; set; }
        public string KDJ2 { get; set; }
        //public string KDJ3 { get; set; }
        //public string KDJ4 { get; set; }
        //public string KDJ5 { get; set; }
        //public string KDJ6 { get; set; }
        public string StockArray { get; set; }
    }
}