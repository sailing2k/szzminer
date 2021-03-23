using Sunny.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using szzminer.Class;

namespace szzminer.Tools
{
    class getIncomeData
    {
        public static List<IncomeItem> incomeItems;
        public static double usdCny = 0;
        public static void getinfo(UIComboBox comboBox)
        {
            string html = getHtml("https://www.f2pool.com/");
            usdCny = PickUsdCny(html);
            incomeItems = PickIncomeItems(html);
            comboBox.Items.Clear();
            for (var i = 0; i < incomeItems.Count; i++)
            {
                comboBox.Items.Add(incomeItems[i].CoinCode);
            }
        }
        public static string getHtml(string url)
        {
            try
            {
                WebClient MyWebClient = new WebClient();
                MyWebClient.Credentials = CredentialCache.DefaultCredentials;
                Byte[] pageData = MyWebClient.DownloadData(url);
                string pageHtml = Encoding.UTF8.GetString(pageData);
                return pageHtml;
            }
            catch
            {
                return null;
            }
        }
        private static List<IncomeItem> PickIncomeItems(string html)
        {
            try
            {
                List<IncomeItem> results = new List<IncomeItem>();
                if (string.IsNullOrEmpty(html))
                {
                    return results;
                }
                string pattern = "data-code=\"(?<dataCode>[^\"]*)\"[\\s\\S]*data-hashrate=.*>\\s*(?<netSpeed>[\\d\\.]+)<span class=\"unit unit-right\">(?<netSpeedUnit>.+)</span>[\\s\\S]*class=\"form-control d-inline-block hash-val\" type=\"text\" value=\"(?<speed>\\d+\\.?\\d*)\"[\\s\\S]*<span class=\"unit\">(?<speedUnit>.*)</span>[\\s\\S]*class=\"pl-1 profit-val info-value\" data-profit=\"(?<incomeCoin>\\d+\\.?\\d*)\"[\\s\\S]*<span class=\"pl-1 info-value\">(?<coinCode>.*)</span>(?:[\\s\\S]*data-usd-per=\"(?<incomeUsd>\\d+\\.?\\d*)\")?";
                if (string.IsNullOrEmpty(pattern))
                {
                    return results;
                }
                List<int> indexList = new List<int>();
                const string splitText = "<tr class=\"row-common";
                int index = html.IndexOf(splitText);
                while (index != -1)
                {
                    indexList.Add(index);
                    index = html.IndexOf(splitText, index + splitText.Length);
                }
                Regex regex = new Regex(pattern);
                int maxLen = 0;
                for (int i = 0; i < indexList.Count; i++)
                {
                    IncomeItem incomeItem;
                    if (i + 1 < indexList.Count)
                    {
                        int len = indexList[i + 1] - indexList[i];
                        if (len > maxLen)
                        {
                            maxLen = len;
                        }
                        incomeItem = PickIncomeItem(regex, html.Substring(indexList[i], len));
                    }
                    else
                    {
                        string content = html.Substring(indexList[i]);
                        if (content.Length > maxLen)
                        {
                            content = content.Substring(0, maxLen);
                        }
                        incomeItem = PickIncomeItem(regex, content);
                    }
                    if (incomeItem != null)
                    {
                        results.Add(incomeItem);
                    }
                }
                return results;
            }
            catch (Exception e)
            {
                return new List<IncomeItem>();
            }
        }
        private static IncomeItem PickIncomeItem(Regex regex, string html)
        {
            Match match = regex.Match(html);
            if (match.Success)
            {
                IncomeItem incomeItem = new IncomeItem()
                {
                    DataCode = match.Groups["dataCode"].Value,
                    CoinCode = match.Groups["coinCode"].Value,
                    SpeedUnit = match.Groups["speedUnit"].Value,
                    NetSpeedUnit = match.Groups["netSpeedUnit"].Value,
                };
                if (incomeItem.DataCode == "grin-29")
                {
                    incomeItem.CoinCode = "grin";
                    incomeItem.SpeedUnit = "h/s";
                    if (incomeItem.NetSpeedUnit != null)
                    {
                        incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("g/s", "h/s");
                    }
                }
                else if (incomeItem.DataCode == "grin-31")
                {
                    incomeItem.CoinCode = "grin31";
                    incomeItem.SpeedUnit = "h/s";
                    if (incomeItem.NetSpeedUnit != null)
                    {
                        incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("g/s", "h/s");
                    }
                }
                else if (incomeItem.DataCode == "grin-32")
                {
                    incomeItem.CoinCode = "grin32";
                    incomeItem.SpeedUnit = "h/s";
                    if (incomeItem.NetSpeedUnit != null)
                    {
                        incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("g/s", "h/s");
                    }
                }
                if (incomeItem.DataCode == "ckb")
                {
                    incomeItem.CoinCode = "ckb";
                }
                double.TryParse(match.Groups["speed"].Value, out double speed);
                incomeItem.Speed = speed;
                double.TryParse(match.Groups["netSpeed"].Value, out double netSpeed);
                incomeItem.NetSpeed = netSpeed;
                double.TryParse(match.Groups["incomeCoin"].Value, out double incomeCoin);
                incomeItem.IncomeCoin = incomeCoin;
                double.TryParse(match.Groups["incomeUsd"].Value, out double incomeUsd);
                incomeItem.IncomeUsd = incomeUsd;
                if (incomeItem.DataCode == "ae")
                {
                    incomeItem.SpeedUnit = "h/s";
                    if (incomeItem.NetSpeedUnit != null)
                    {
                        incomeItem.NetSpeedUnit = incomeItem.NetSpeedUnit.Replace("g/s", "h/s");
                    }
                }
                return incomeItem;
            }
            return null;
        }
        private static double PickUsdCny(string html)
        {
            try
            {
                double result = 0;
                //var regex = VirtualRoot.GetRegex(@"CURRENCY_CONF\.usd_cny = Number\('(\d+\.?\d*)' \|\| \d+\.?\d*\);");
                Regex regex = new Regex(@"CURRENCY_CONF\.usd_cny = Number\('(\d+\.?\d*)' \|\| \d+\.?\d*\);");
                var matchs = regex.Match(html);
                if (matchs.Success)
                {
                    double.TryParse(matchs.Groups[1].Value, out result);
                }
                return result;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
    }
}
