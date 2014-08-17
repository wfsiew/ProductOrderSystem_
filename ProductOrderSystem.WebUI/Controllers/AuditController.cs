using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.WebUI.Abstract;
using ProductOrderSystem.WebUI.Concrete;
using ProductOrderSystem.WebUI.Helpers;
using ProductOrderSystem.WebUI.Models;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;

namespace ProductOrderSystem.WebUI.Controllers
{
    [Authorize]
    public class AuditController : Controller
    {
        private IOrderRepository repository;

        public AuditController(IOrderRepository repository)
        {
            this.repository = repository;
        }

        //
        // GET: /Audit/

        public ActionResult Index()
        {
            //if (!User.IsInRole(Constants.SUPER_ADMIN))
            //{
            //    ViewBag.ErrorMessage = "You are not authorize to view this site";
            //}

            ViewBag.Menu = Constants.EXPORT_LOG;
            //ViewBag.IsSuperAdmin = User.IsInRole(Constants.SUPER_ADMIN);

            return View();
        }

        public FileContentResult Export(string from, string to)
        {
            FileContentResult f = null;
            string contentType = "application/vnd.ms-excel";
            string filename = "Order_logs.xlsx";
            ExcelPackage pk = null;
            byte[] buff = null;
            int c = 0;
            int rx = 1;

            try
            {
                pk = new ExcelPackage();
                ExcelWorksheet ws = Utils.CreateSheet(pk, "Log", 1);

                SetHeaders(ws);

                DateTime dateFrom = Utils.GetDateTimeFMT(from);
                DateTime dateTo = Utils.GetDateTimeFMT(to);

                DateTime _dateFrom = new DateTime(dateFrom.Year, dateFrom.Month, dateFrom.Day);
                DateTime _dateTo = new DateTime(dateTo.Year, dateTo.Month, dateTo.Day);

                _dateTo = _dateTo.AddDays(1);

                List<StatusType> ls = repository.Context.StatusTypes.ToList();
                List<OrderType> lt = repository.Context.OrderTypes.ToList();

                var q = repository.Context.OrderAudits.Include(x => x.User).Include(x => x.OrderType);

                q = q.Where(x => x.ActionDatetime >= _dateFrom && x.ActionDatetime < _dateTo)
                    .OrderBy(x => x.ActionDatetime);

                List<OrderAudit> l = q.ToList();

                for (int i = 0; i < l.Count; i++ )
                {
                    c = 1;
                    ++rx;
                    OrderAudit z = l[i];

                    ExcelRange er = ws.Cells[rx, c++];
                    er.Style.Numberformat.Format = "0";
                    er.Value = z.OrderID;

                    ws.Cells[rx, c++].Value = z.CustName;

                    OrderType orderType = lt.Find(k => k.ID == z.OrderTypeID);
                    ws.Cells[rx, c++].Value = orderType.Name;

                    er = ws.Cells[rx, c++];
                    er.Style.Numberformat.Format = "yyyy-mm-dd";
                    er.Value = z.ActionDatetime;

                    ws.Cells[rx, c++].Value = Utils.GetTimeStr(z.ActionDatetime);

                    ws.Cells[rx, c++].Value = z.User.Name;

                    ws.Cells[rx, c++].Value = z.User.Roles.First().Name;

                    ws.Cells[rx, c++].Value = z.SalesPerson.Name;

                    StatusType statusType = ls.Find(k => k.ID == z.Status);
                    ws.Cells[rx, c++].Value = statusType.Name;

                    ws.Cells[rx, c++].Value = z.OverallStatus;

                    ws.Cells[rx, c++].Value = Utils.GetYesNo(z.IsInstallDateChange);

                    ws.Cells[rx, c++].Value = Utils.GetYesNo(z.IsInstallationPenalty);
                }

                FitColumns(ws);

                buff = pk.GetAsByteArray();
                f = File(buff, contentType, filename);
            }

            catch (Exception ex)
            {
                throw ex;
            }

            finally
            {
                if (pk != null)
                    pk.Dispose();
            }

            return f;
        }

        private void SetHeaders(ExcelWorksheet ws)
        {
            int r = 1;

            string[] headers = new string[] { "Order ID", "Customer Name", "Order Type", "Action Date", "Action Time", "Action By", "Role",
                "Sales Person", "Status", "Overall Status", "Installation Date Change", "Installation Penalty" };

            for (int c = 0; c < headers.Length; c++)
            {
                ws.Cells[r, c + 1].Value = headers[c];
            }
        }

        private void FitColumns(ExcelWorksheet ws)
        {
            for (int i = 1; i <= 12; i++)
                ws.Column(i).AutoFit();
        }
    }
}
