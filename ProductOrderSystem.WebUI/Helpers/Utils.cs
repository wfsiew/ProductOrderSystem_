using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Globalization;
using OfficeOpenXml;
using System.Web.Mvc;

namespace ProductOrderSystem.WebUI.Helpers
{
    public class Utils
    {
        public const string EXPORT_DATE_FMT = "yyyy-MM-dd";

        public static string GetItemMessage(int total, int pagenum, int pagesize)
        {
            int x = (pagenum - 1) * pagesize + 1;
            int y = pagenum * pagesize;

            if (total < y)
                y = total;

            if (total < 1)
                return "";

            string s = total > 1 ? "items" : "item";

            return string.Format("{0} to {1} of {2} {3}", x, y, total, s);
        }

        public static string GetDateStr1(DateTime? date)
        {
            if (date.GetValueOrDefault() == DateTime.MinValue)
                return "";

            return date.GetValueOrDefault().ToString("MMMM dd, yyyy");
        }

        public static string GetDateStr(DateTime? date)
        {
            if (date.GetValueOrDefault() == DateTime.MinValue)
                return "N/A";

            return date.GetValueOrDefault().ToString("dd MMMM yyyy");
        }

        public static string GetTimeStr(DateTime? time)
        {
            if (time.GetValueOrDefault() == DateTime.MinValue)
                return "N/A";

            return time.GetValueOrDefault().ToString("h:mm tt");
        }

        public static DateTime? GetDateTime(DateTime? date, DateTime? time)
        {
            DateTime? dt = null;
            DateTime _date;
            DateTime _time;

            //if (date == null && time != null)
            //{
            //    _time = time.GetValueOrDefault();
            //    dt = new DateTime(1, 1, 1, _time.Hour, _time.Minute, _time.Second);
            //}

            //else if (date != null && time == null)
            //{
            //    _date = date.GetValueOrDefault();
            //    dt = new DateTime(_date.Year, _date.Month, _date.Day);
            //}

            if (date != null && time != null)
            {
                _date = date.GetValueOrDefault();
                _time = time.GetValueOrDefault();
                dt = new DateTime(_date.Year, _date.Month, _date.Day, _time.Hour, _time.Minute, _time.Second);
            }

            return dt;
        }

        public static DateTime? GetDate(DateTime? datetime)
        {
            DateTime? dt = null;

            if (datetime != null)
                dt = datetime.GetValueOrDefault().Date;

            return dt;
        }

        public static DateTime? GetTime(DateTime? datetime)
        {
            DateTime? dt = null;

            if (datetime != null)
                dt = datetime.GetValueOrDefault();

            return dt;
        }

        public static string Role(IPrincipal User)
        {
            string role = null;

            if (User.IsInRole(Constants.SALES_COORDINATOR))
                role = Constants.SALES_COORDINATOR;

            else if (User.IsInRole(Constants.CREDIT_CONTROL))
                role = Constants.CREDIT_CONTROL;

            else if (User.IsInRole(Constants.FIXED_LINE))
                role = Constants.FIXED_LINE;

            else if (User.IsInRole(Constants.BILLING))
                role = Constants.BILLING;

            else if (User.IsInRole(Constants.INSTALLERS))
                role = Constants.INSTALLERS;

            else if (User.IsInRole(Constants.SALES))
                role = Constants.SALES;

            if (User.IsInRole(Constants.SUPER_ADMIN))
                role = Constants.SUPER_ADMIN;

            return role;
        }

        public static string GetYesNo(bool b)
        {
            return b ? "Yes" : "No";
        }

        public static ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName, int idx)
        {
            p.Workbook.Worksheets.Add(sheetName);
            ExcelWorksheet ws = p.Workbook.Worksheets[idx];
            ws.Name = sheetName;
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

            return ws;
        }

        public static DateTime GetDateTimeFMT(string q)
        {
            DateTime dt = default(DateTime);

            if (string.IsNullOrEmpty(q))
                return dt;

            dt = DateTime.ParseExact(q, EXPORT_DATE_FMT, CultureInfo.InvariantCulture);

            return dt;
        }

        public static string FormatHtml(string a)
        {
            string r = "";

            if (string.IsNullOrEmpty(a))
                return r;

            r = EncodeHtml(a);

            r = r.Replace("\r\n", "<br/>")
                .Replace("\n", "<br/>");

            return r;
        }

        public static string EncodeHtml(string a)
        {
            string r = "";

            if (string.IsNullOrEmpty(a))
                return r;

            r = HttpUtility.HtmlEncode(a);

            return r;
        }
    }
}