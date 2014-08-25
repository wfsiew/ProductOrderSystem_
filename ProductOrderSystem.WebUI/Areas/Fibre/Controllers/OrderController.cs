using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Transactions;
using System.Data.Entity.SqlServer;
using GoogleAppHelper.Models;
using GoogleAppHelper.Services;
using ProductOrderSystem.Domain.Models;
using ProductOrderSystem.Domain.Fibre.Models;
using ProductOrderSystem.WebUI.Abstract;
using ProductOrderSystem.WebUI.Concrete;
using ProductOrderSystem.WebUI.Helpers;
using ProductOrderSystem.WebUI.Models;
using ProductOrderSystem.WebUI.Areas.Fibre.Models;
using ProductOrderSystem.WebUI.Controllers;

namespace ProductOrderSystem.WebUI.Areas.Fibre.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private IOrderRepository repository;

        public OrderController(IOrderRepository repository)
        {
            this.repository = repository;
        }

        //
        // GET: /Order/

        public ActionResult Create(LogonUser logonUser, int? id, int? ordertypeid)
        {
            ViewBag.Menu = Constants.CREATE_ORDER;

            if (!User.IsInRole(Constants.SALES_COORDINATOR) && !User.IsInRole(Constants.SUPER_ADMIN))
            {
                ViewBag.ErrorMessage = "You are not authorize to view this site";
            }

            LoadSalesPersons();
            LoadOrderTypes();

            Order_Fibre o = new Order_Fibre();

            if (id != null)
            {
                Order_Fibre x = repository.Orders.Where(k => k.ID == id).FirstOrDefault();

                if (x != null)
                {
                    o.ContactPerson = x.ContactPerson;
                    o.ContactPersonNo = x.ContactPersonNo;
                    o.CustAddr = x.CustAddr;
                    o.CustID = x.CustID;
                    o.CustName = x.CustName;
                    o.SalesPersonID = x.SalesPersonID;
                    o.OrderTypeID = ordertypeid.GetValueOrDefault();
                }
            }

            return View(o);
        }

        [HttpPost]
        public JsonResult Create(LogonUser logonUser, OrderCreateModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (ModelState.IsValid)
                {
                    ValidateInstallDate(o.BookedInstallDate);
                    ValidateInstallTime(o.BookedInstallTime);

                    DateTime? installDatetime = Utils.GetDateTime(o.BookedInstallDate, o.BookedInstallTime);

                    Order_Fibre x = new Order_Fibre();

                    if (installDatetime != null)
                        x.InstallDatetime = installDatetime;

                    x.ContactPerson = o.ContactPerson;
                    x.ContactPersonNo = o.ContactPersonNo;
                    x.CustAddr = o.CustAddr;
                    x.CustID = o.CustID;
                    x.CustName = o.CustName;
                    x.IsCeoApproved = o.IsCeoApproved;
                    x.IsCoverageAvailable = o.IsCoverageAvailable;
                    x.IsDemandList = o.IsDemandList;
                    x.IsReqFixedLine = o.IsReqFixedLine;
                    x.IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq;
                    x.IsServiceUpgrade = o.IsServiceUpgrade;
                    x.OrderTypeID = o.OrderTypeID;
                    x.ReasonWithdraw = o.ReasonWithdraw;
                    x.ReceivedDatetime = Utils.GetDateTime(o.ReceivedDate, o.ReceivedTime).GetValueOrDefault();
                    x.SalesPersonID = o.SalesPersonID;
                    x.StatusSC = o.StatusSC;
                    x.IsKIV = o.IsKIV;
                    x.IsBTUInstalled = o.IsBTUInstalled;
                    x.CreateDatetime = DateTime.Now;
                    x.LastUpdateDatetime = DateTime.Now;
                    x.UserID = logonUser.UserID;
                    x.LastActionDatetime = DateTime.Now;

                    if (o.OrderTypeID != 4)
                        x.ActionTypeID = 1;

                    else
                        x.ActionTypeID = 4;

                    OrderAudit_Fibre z = new OrderAudit_Fibre();
                    z.ActionDatetime = x.LastActionDatetime;
                    z.CustName = x.CustName;
                    z.ID = Guid.NewGuid();
                    z.IsInstallationPenalty = false;
                    z.IsInstallDateChange = false;
                    z.OrderTypeID = x.OrderTypeID;
                    z.OverallStatus = GetOverallStatus(x);
                    z.SalesPersonID = x.SalesPersonID;
                    z.Status = x.StatusSC;
                    z.UserID = x.UserID;

                    using (TransactionScope tx = new TransactionScope())
                    {
                        repository.Insert(x);
                        repository.Save();
                        z.OrderID = x.ID;
                        repository.InsertOrderAudit(z);
                        repository.Save();

                        tx.Complete();
                    }

                    User u = repository.Context.Users.Where(k => k.ID == o.SalesPersonID).FirstOrDefault();
                    x.SalesPerson = u;

                    User user = repository.Context.Users.Where(k => k.ID == x.UserID).FirstOrDefault();
                    x.User = user;

                    if (!x.IsDemandList)
                    {
                        if (o.OrderTypeID != 4)
                            SendNewOrderCreatedEmailNotification(x, 1);

                        else
                            TerminateOrder(x);
                    }

                    res["success"] = 1;
                    res["message"] = "Order successfully created";
                    res["orderid"] = x.ID;
                }

                else
                {
                    throw new UIException("Form is incomplete");
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UploadFile(LogonUser logonUser, int id, HttpPostedFileBase file)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                DocumentService service = new DocumentService();
                GoogleDocument document = null;

                if (file == null)
                    throw new Exception("No file to upload");

                //todo: for local test upload
                //string filename = Path.GetFileName(file.FileName);
                //string path = Path.Combine("c:\\uploads", filename);
                //file.SaveAs(path);

                string[] userEmails = new string[] { "" };
                //todo: remove the comment below
                document = service.UploadFile2LO(file.InputStream, file.FileName, file.ContentType, true, logonUser.Email, userEmails);

                OrderFile_Fibre o = new OrderFile_Fibre();
                o.FileName = file.FileName;
                o.FileSize = file.ContentLength;
                o.FileUniqueKey = null;
                o.GoogleFileID = document.ResourceID;
                o.GoogleFileUrl = document.AlternateUri;
                o.OrderID = id;

                repository.InsertOrderFile(o);
                repository.Save();

                res["success"] = 1;
                res["message"] = "Upload success";
                res["filename"] = file.FileName;
                res["url"] = document.AlternateUri.ToString();
                res["fileID"] = o.ID;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Search(LogonUser logonUser)
        {
            ViewBag.Menu = Constants.SEARCH_ORDER;
            ViewBag.Role = Utils.Role(User);
            ViewBag.SalesPersonID = "";

            if (ViewBag.Role == Constants.SALES)
                ViewBag.SalesPersonID = logonUser.UserID;

            LoadSalesPersons();
            LoadOrderTypes1();

            return View();
        }

        [HttpPost]
        public JsonResult Search(LogonUser user, OrderSearchModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            var q = repository.Orders.Include(x => x.User).Include(x => x.OrderType);

            string custName = string.IsNullOrEmpty(o.CustName) ? null : o.CustName.ToUpper();
            string custID = string.IsNullOrEmpty(o.CustID) ? null : o.CustID.ToUpper();
            string custAddr = string.IsNullOrEmpty(o.CustAddr) ? null : o.CustAddr.ToUpper();
            DateTime? dateFrom = null;
            DateTime? dateTo = null;
            //int[] act = null;

            if (!string.IsNullOrEmpty(custName))
                q = q.Where(x => x.CustName.Contains(custName));

            if (!string.IsNullOrEmpty(custID))
                q = q.Where(x => x.CustID.Contains(custID));

            if (!string.IsNullOrEmpty(custAddr))
                q = q.Where(x => x.CustAddr.Contains(custAddr));

            if (o.Status != null)
            {
                if (o.Status == 0)
                {
                    q = q.Where(x => x.StatusSC == 0 ||
                        x.StatusSC == null);
                }

                else if (o.Status == 1)
                {
                    q = q.Where(x => x.StatusSC == 1);
                }

                else if (o.Status == 2)
                {
                    q = q.Where(x => x.StatusSC == 2 ||
                        x.StatusCC == 2 ||
                        x.StatusFL == 2 ||
                        x.StatusAC == 2 ||
                        x.StatusInstall == 2);
                }

                else if (o.Status == 3)
                {
                    q = q.Where(x => x.StatusSC == 3 &&
                        (x.StatusInstall == 0 || x.StatusInstall == null));
                }

                else if (o.Status == 4)
                {
                    q = q.Where(x => x.StatusSC == 4 ||
                        x.StatusCC == 4 ||
                        x.StatusFL == 4 ||
                        x.StatusAC == 4 ||
                        x.StatusInstall == 4);
                }
            }

            if (o.OrderID != null)
                q = q.Where(x => x.ID == o.OrderID);

            if (o.SalesPersonID != null)
                q = q.Where(x => x.SalesPersonID == o.SalesPersonID);

            if (o.DateFrom != null)
            {
                DateTime df = o.DateFrom.GetValueOrDefault();
                dateFrom = new DateTime(df.Year, df.Month, df.Day, 0, 0, 0);
                q = q.Where(x => x.ReceivedDatetime >= dateFrom);
            }

            if (o.DateTo != null)
            {
                DateTime dt = o.DateTo.GetValueOrDefault();
                dateTo = new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
                q = q.Where(x => x.ReceivedDatetime <= dateTo);
            }

            if (o.OrderTypeID != null)
                q = q.Where(x => x.OrderTypeID == o.OrderTypeID);

            q = q.Where(x => x.IsDemandList == o.IsDemandList);

            //if (User.IsInRole(Constants.CREDIT_CONTROL))
            //{
            //    act = new int[] { 1, 2, 3, 4 };
            //    q = q.Where(x => x.ActionTypeID >= 1 && x.ActionTypeID <= 4);
            //}

            //else if (User.IsInRole(Constants.BILLING))
            //{
            //    act = new int[] { 5, 6, 7, 8 };
            //    q = q.Where(x => x.ActionTypeID >= 5 && x.ActionTypeID <= 8);
            //}

            //else if (User.IsInRole(Constants.FIXED_LINE))
            //{
            //    act = new int[] { 9, 10, 11, 12 };
            //    q = q.Where(x => x.ActionTypeID >= 9 && x.ActionTypeID <= 12);
            //}

            //else if (User.IsInRole(Constants.HOD) || User.IsInRole(Constants.INSTALLERS))
            //{
            //    act = new int[] { 13, 14, 15, 16, 17 };
            //    q = q.Where(x => x.ActionTypeID >= 13 && x.ActionTypeID <= 17);
            //}

            bool sorted = GetSortQuery(ref q, o.Sort);

            if (!sorted)
                q = q.OrderBy(x => x.ID);

            var s = q.ToString();

            int pageSize = 10;
            int pageNumber = o.Page ?? 1;
            int total = q.Count();

            Pager pager = new Pager(total, pageNumber, pageSize);
            var l = q.Skip(pager.LowerBound).Take(pager.PageSize).ToList();
            var lx = GetListObject(l);

            res["list"] = lx;
            res["pager"] = pager;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListOverdue(LogonUser user, OrderSearchModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            var q = repository.Orders.Include(x => x.User).Include(x => x.OrderType);

            if (User.IsInRole(Constants.CREDIT_CONTROL))
            {
                q = q.Where(x => x.ActionTypeID >= 1 && x.ActionTypeID <= 4 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1);
            }

            else if (User.IsInRole(Constants.BILLING))
            {
                q = q.Where(x => x.ActionTypeID >= 5 && x.ActionTypeID <= 8 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1);
            }

            else if (User.IsInRole(Constants.FIXED_LINE))
            {
                q = q.Where(x => x.ActionTypeID >= 9 && x.ActionTypeID <= 12 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1);
            }

            else if (User.IsInRole(Constants.HOD) || User.IsInRole(Constants.INSTALLERS))
            {
                q = q.Where(x => (x.ActionTypeID == 13 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 3) ||
                    (x.ActionTypeID == 14 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1) ||
                    (x.ActionTypeID == 15 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 3) ||
                    (x.ActionTypeID == 16 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1) ||
                    (x.ActionTypeID == 17 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 3));
            }

            else if (User.IsInRole(Constants.SALES))
            {
                q = q.Where(x => (x.ActionTypeID == 18 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 2) ||
                    (x.ActionTypeID == 19 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 1) ||
                    (x.ActionTypeID == 20 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) > 2));
            }

            bool sorted = GetSortQuery(ref q, o.Sort);

            if (!sorted)
                q = q.OrderBy(x => x.LastActionDatetime);

            var s = q.ToString();

            int pageSize = 10;
            int pageNumber = o.Page ?? 1;
            int total = q.Count();

            Pager pager = new Pager(total, pageNumber, pageSize);
            var l = q.Skip(pager.LowerBound).Take(pager.PageSize).ToList();
            var lx = GetListObject(l);

            res["list"] = lx;
            res["pager"] = pager;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListPending(LogonUser user, OrderSearchModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            var q = repository.Orders.Include(x => x.User).Include(x => x.OrderType);

            if (User.IsInRole(Constants.CREDIT_CONTROL))
            {
                q = q.Where(x => x.ActionTypeID >= 1 && x.ActionTypeID <= 4 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 1);
            }

            else if (User.IsInRole(Constants.BILLING))
            {
                q = q.Where(x => x.ActionTypeID >= 5 && x.ActionTypeID <= 8 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 1);
            }

            else if (User.IsInRole(Constants.FIXED_LINE))
            {
                q = q.Where(x => x.ActionTypeID >= 9 && x.ActionTypeID <= 12 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 1);
            }

            else if (User.IsInRole(Constants.HOD) || User.IsInRole(Constants.INSTALLERS))
            {
                q = q.Where(x => (x.ActionTypeID == 13 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 3) ||
                    (x.ActionTypeID == 14 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 1) ||
                    (x.ActionTypeID == 15 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 3) ||
                    (x.ActionTypeID == 16 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 1) ||
                    (x.ActionTypeID == 17 &&
                    SqlFunctions.DateDiff("day", x.LastActionDatetime, DateTime.Now) <= 3));
            }

            bool sorted = GetSortQuery(ref q, o.Sort);

            if (!sorted)
                q = q.OrderBy(x => x.LastActionDatetime);

            var s = q.ToString();

            int pageSize = 10;
            int pageNumber = o.Page ?? 1;
            int total = q.Count();

            Pager pager = new Pager(total, pageNumber, pageSize);
            var l = q.Skip(pager.LowerBound).Take(pager.PageSize).ToList();
            var lx = GetListObject(l);

            res["list"] = lx;
            res["pager"] = pager;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // not used
        [HttpPost]
        public JsonResult List(LogonUser user, OrderSearchModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            var q = repository.Orders.Include(x => x.User).Include(x => x.OrderType);

            int[] act = null;

            if (User.IsInRole(Constants.CREDIT_CONTROL))
            {
                act = new int[] { 1, 2, 3, 4 };
                q = q.Where(x => x.ActionTypeID >= 1 && x.ActionTypeID <= 4);
            }

            else if (User.IsInRole(Constants.BILLING))
            {
                act = new int[] { 5, 6, 7, 8 };
                q = q.Where(x => x.ActionTypeID >= 5 && x.ActionTypeID <= 8);
            }

            else if (User.IsInRole(Constants.FIXED_LINE))
            {
                act = new int[] { 9, 10, 11, 12 };
                q = q.Where(x => x.ActionTypeID >= 9 && x.ActionTypeID <= 12);
            }

            else if (User.IsInRole(Constants.HOD) || User.IsInRole(Constants.INSTALLERS))
            {
                act = new int[] { 13, 14, 15, 16, 17 };
                q = q.Where(x => x.ActionTypeID >= 13 && x.ActionTypeID <= 17);
            }

            bool sorted = GetSortQuery(ref q, o.Sort);

            if (!sorted)
                q = q.OrderBy(x => x.StatusSC);

            var s = q.ToString();

            int pageSize = 10;
            int pageNumber = o.Page ?? 1;
            int total = q.Count();

            Pager pager = new Pager(total, pageNumber, pageSize);
            var l = q.Skip(pager.LowerBound).Take(pager.PageSize).ToList();
            var lx = GetListObject(l);

            res["list"] = lx;
            res["pager"] = pager;

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Delete(LogonUser logonUser, int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                if (o == null)
                    throw new UIException("Order not found");

                bool a = User.IsInRole(Constants.SUPER_ADMIN);
                //bool b = logonUser.UserID != o.UserID;

                if (!a)
                    throw new UIException("You are not administrator");

                List<OrderFile_Fibre> lf = o.OrderFiles.Where(f => f.OrderID == o.ID).ToList();

                using (TransactionScope tx = new TransactionScope())
                {
                    foreach (OrderFile_Fibre f in lf)
                    {
                        repository.DeleteOrderFile(f);
                    }

                    repository.Delete(o);
                    repository.Save();
                    tx.Complete();
                }

                res["success"] = 1;
                res["message"] = string.Format("Order [{0}] has been successfully deleted", o.CustName);
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult View(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                if (o == null)
                    throw new UIException("Order not found");

                var m = GetViewOrderObject(o);
                res["success"] = 1;
                res["model"] = m;
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditSC(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SALES_COORDINATOR) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                if (o == null)
                    throw new UIException("Order not found");

                var m = GetEditOrderObjectSC(o);
                res["success"] = 1;
                res["model"] = m;
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Variation(LogonUser logonUser, OrderEditSCModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SALES_COORDINATOR) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    else
                    {
                        ValidateInstallDate(o.BookedInstallDate);
                        ValidateInstallTime(o.BookedInstallTime);

                        DateTime? installDatetime = Utils.GetDateTime(o.BookedInstallDate, o.BookedInstallTime);

                        Order_Fibre y = new Order_Fibre();

                        if (installDatetime != null)
                            y.InstallDatetime = installDatetime;

                        y.ContactPerson = x.ContactPerson;
                        y.ContactPersonNo = x.ContactPersonNo;
                        y.CustAddr = x.CustAddr;
                        y.CustID = x.CustID;
                        y.CustName = x.CustName;
                        y.IsCeoApproved = o.IsCeoApproved;
                        y.IsCoverageAvailable = o.IsCoverageAvailable;
                        y.IsDemandList = o.IsDemandList;
                        y.IsReqFixedLine = o.IsReqFixedLine;
                        y.IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq;
                        y.IsServiceUpgrade = o.IsServiceUpgrade;
                        y.OrderTypeID = o.OrderTypeID;
                        y.ReasonWithdraw = o.ReasonWithdraw;
                        y.ReceivedDatetime = Utils.GetDateTime(o.ReceivedDate, o.ReceivedTime).GetValueOrDefault();
                        y.SalesPersonID = x.SalesPersonID;
                        y.StatusSC = 0;
                        y.IsKIV = o.IsKIV;
                        y.IsBTUInstalled = o.IsBTUInstalled;
                        y.CreateDatetime = DateTime.Now;
                        y.LastUpdateDatetime = DateTime.Now;
                        y.UserID = logonUser.UserID;
                        y.LastActionDatetime = DateTime.Now;

                        res["success"] = 1;
                    }
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Terminate(LogonUser logonUser, OrderEditSCModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SALES_COORDINATOR) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    else
                    {
                        ValidateInstallDate(o.BookedInstallDate);
                        ValidateInstallTime(o.BookedInstallTime);

                        DateTime? installDatetime = Utils.GetDateTime(o.BookedInstallDate, o.BookedInstallTime);

                        Order_Fibre y = new Order_Fibre();

                        if (installDatetime != null)
                            y.InstallDatetime = installDatetime;

                        y.ContactPerson = x.ContactPerson;
                        y.ContactPersonNo = x.ContactPersonNo;
                        y.CustAddr = x.CustAddr;
                        y.CustID = x.CustID;
                        y.CustName = x.CustName;
                        y.IsCeoApproved = o.IsCeoApproved;
                        y.IsCoverageAvailable = o.IsCoverageAvailable;
                        y.IsDemandList = o.IsDemandList;
                        y.IsReqFixedLine = o.IsReqFixedLine;
                        y.IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq;
                        y.IsServiceUpgrade = o.IsServiceUpgrade;
                        y.OrderTypeID = o.OrderTypeID;
                        y.ReasonWithdraw = o.ReasonWithdraw;
                        y.ReceivedDatetime = Utils.GetDateTime(o.ReceivedDate, o.ReceivedTime).GetValueOrDefault();
                        y.SalesPersonID = x.SalesPersonID;
                        y.StatusSC = 0;
                        y.IsKIV = o.IsKIV;
                        y.IsBTUInstalled = o.IsBTUInstalled;
                        y.CreateDatetime = DateTime.Now;
                        y.LastUpdateDatetime = DateTime.Now;
                        y.UserID = logonUser.UserID;
                        y.LastActionDatetime = DateTime.Now;

                        res["success"] = 1;
                    }
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditSC(LogonUser logonUser, OrderEditSCModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();
            int? kivroleID = null;
            int statusSC = 0;

            try
            {
                if (!User.IsInRole(Constants.SALES_COORDINATOR) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    else
                    {
                        kivroleID = x.KIVRoleID;
                        statusSC = x.StatusSC;
                        DateTime? oldInstallDatetime = x.InstallDatetime;

                        ValidateInstallDate(o.BookedInstallDate);
                        ValidateInstallTime(o.BookedInstallTime);

                        DateTime? installDatetime = Utils.GetDateTime(o.BookedInstallDate, o.BookedInstallTime);

                        if (installDatetime != null)
                            x.InstallDatetime = installDatetime;

                        x.ContactPerson = o.ContactPerson;
                        x.ContactPersonNo = o.ContactPersonNo;
                        x.CustAddr = o.CustAddr;
                        x.CustID = o.CustID;
                        x.CustName = o.CustName;
                        x.IsCeoApproved = o.IsCeoApproved;
                        x.IsCoverageAvailable = o.IsCoverageAvailable;
                        x.IsDemandList = o.IsDemandList;
                        x.IsReqFixedLine = o.IsReqFixedLine;
                        x.IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq;
                        x.IsServiceUpgrade = o.IsServiceUpgrade;
                        x.OrderTypeID = 1;
                        x.ReasonWithdraw = o.ReasonWithdraw;
                        x.Comments = o.Comments;
                        x.ReceivedDatetime = Utils.GetDateTime(o.ReceivedDate, o.ReceivedTime).GetValueOrDefault();
                        x.SalesPersonID = o.SalesPersonID;
                        x.StatusSC = 0; // pending
                        x.IsKIV = o.IsKIV;
                        x.IsBTUInstalled = o.IsBTUInstalled;
                        x.LastUpdateDatetime = DateTime.Now;
                        x.UserID = logonUser.UserID;
                        x.LastActionDatetime = DateTime.Now;

                        //if (o.OrderTypeID == 4)
                        //{
                        //    x.ActionTypeID = 4;
                        //    ResetStatus(x);

                        //    repository.Update(x);
                        //    repository.Save();

                        //    TerminateOrder(x);
                        //}

                        OrderAudit_Fibre z = new OrderAudit_Fibre();
                        z.ActionDatetime = x.LastActionDatetime;
                        z.CustName = x.CustName;
                        z.ID = Guid.NewGuid();
                        z.IsInstallationPenalty = false;
                        z.IsInstallDateChange = false;
                        z.OrderID = x.ID;
                        z.OrderTypeID = x.OrderTypeID;
                        z.OverallStatus = GetOverallStatus(x);
                        z.SalesPersonID = x.SalesPersonID;
                        z.Status = x.StatusSC;
                        z.UserID = logonUser.UserID;

                        bool a = false; // skip check on change installation date

                        if (a)
                        {
                            x.StatusSC = 2;
                            x.ActionTypeID = 19;
                            z.IsInstallDateChange = true;
                        }

                        else
                        {
                            bool b = x.StatusInstall == 1;

                            if (b)
                            {
                                x.StatusSC = 2;
                                x.ActionTypeID = 19;
                            }

                            else
                            {
                                x.ActionTypeID = 14;
                                ResetStatus(x);
                            }
                        }

                        repository.Update(x);
                        repository.InsertOrderAudit(z);

                        if (kivroleID != null)
                            x.KIVRoleID = null;

                        repository.Save();

                        if (!x.IsDemandList)
                        {
                            if (kivroleID != null && statusSC == 4)
                            {
                                SendUpdatedOrderEmailNotification(x, 34, kivroleID);
                            }

                            else
                            {
                                CheckInstallDateChanged(oldInstallDatetime, x);
                            }
                        }

                        res["success"] = 1;
                        res["message"] = string.Format("Order [{0}] has been successfully updated", x.ID);
                    }
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditFL(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.FIXED_LINE) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                if (o == null)
                    throw new UIException("Order not found");

                var m = GetEditOrderObjectFL(o);
                res["success"] = 1;
                res["model"] = m;
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditFL(LogonUser logonUser, OrderEditFLModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.FIXED_LINE) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    if (x.ActionTypeID == 9)
                        ProcFLUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 10)
                        ProcFLResubmitUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 11)
                        ProcFLWithdrawUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 12)
                        ProcFLTerminateUpdate(logonUser, o, x);

                    res["success"] = 1;
                    res["message"] = string.Format("Order [{0}] has been successfully updated", x.ID);
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditCC(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.CREDIT_CONTROL) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                if (o == null)
                    throw new UIException("Order not found");

                var m = GetEditOrderObjectCC(o);
                res["success"] = 1;
                res["model"] = m;
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditCC(LogonUser logonUser, OrderEditCCModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.CREDIT_CONTROL) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    if (x.ActionTypeID == 1)
                        ProcCCUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 2)
                        ProcCCResubmitUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 3)
                        ProcCCWithdrawUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 4)
                        ProcCCTerminateUpdate(logonUser, o, x);

                    res["success"] = 1;
                    res["message"] = string.Format("Order [{0}] has been successfully updated", x.ID);
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditAC(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.BILLING) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                if (o == null)
                    throw new UIException("Order not found");

                var m = GetEditOrderObjectAC(o);
                res["success"] = 1;
                res["model"] = m;
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditAC(LogonUser logonUser, OrderEditACModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.BILLING) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    if (x.ActionTypeID == 5)
                        ProcAcUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 6)
                        ProcACResubmitUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 7)
                        ProcACWithdrawUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 8)
                        ProcACTerminateUpdate(logonUser, o, x);

                    res["success"] = 1;
                    res["message"] = string.Format("Order [{0}] has been successfully updated", x.ID);
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditInstall(int id)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.INSTALLERS) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                if (o == null)
                    throw new UIException("Order not found");

                var m = GetEditOrderObjectInstall(o);
                res["success"] = 1;
                res["model"] = m;
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EditInstall(LogonUser logonUser, OrderEditInstallModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.INSTALLERS) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    if (x.ActionTypeID == 13)
                        ProcInstallUpdate(logonUser, o, x);

                    if (x.ActionTypeID == 14)
                        ProcInstallResubmitUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 15)
                        ProcInstallResubmit1Update(logonUser, o, x);

                    else if (x.ActionTypeID == 16)
                        ProcInstallWithdrawUpdate(logonUser, o, x);

                    else if (x.ActionTypeID == 17)
                        ProcInstallTerminateUpdate(logonUser, o, x);

                    res["success"] = 1;
                    res["message"] = string.Format("Order [{0}] has been successfully updated", x.ID);
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Withdraw(LogonUser logonUser, OrderEditSCModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SALES_COORDINATOR) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    else
                    {
                        if (x.StatusSC == 3 && x.OrderTypeID == 5)
                            throw new UIException("Order has been withdrawn");

                        DateTime? oldBookedInstallDatetime = x.InstallDatetime;

                        ValidateInstallDate(o.BookedInstallDate);
                        ValidateInstallTime(o.BookedInstallTime);

                        DateTime? installDatetime = Utils.GetDateTime(o.BookedInstallDate, o.BookedInstallTime);

                        if (installDatetime != null)
                            x.InstallDatetime = installDatetime;

                        x.ContactPerson = o.ContactPerson;
                        x.ContactPersonNo = o.ContactPersonNo;
                        x.CustAddr = o.CustAddr;
                        x.CustID = o.CustID;
                        x.CustName = o.CustName;
                        x.IsCeoApproved = o.IsCeoApproved;
                        x.IsCoverageAvailable = o.IsCoverageAvailable;
                        x.IsDemandList = o.IsDemandList;
                        x.IsReqFixedLine = o.IsReqFixedLine;
                        x.IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq;
                        x.IsServiceUpgrade = o.IsServiceUpgrade;
                        x.OrderTypeID = 5;
                        x.ReasonWithdraw = o.ReasonWithdraw;
                        x.ReceivedDatetime = Utils.GetDateTime(o.ReceivedDate, o.ReceivedTime).GetValueOrDefault();
                        x.SalesPersonID = o.SalesPersonID;
                        x.StatusSC = 0; // pending
                        x.IsKIV = o.IsKIV;
                        x.IsBTUInstalled = o.IsBTUInstalled;
                        x.LastUpdateDatetime = DateTime.Now;
                        x.UserID = logonUser.UserID;
                        x.LastActionDatetime = DateTime.Now;

                        bool b = x.StatusInstall == 1;

                        if (b)
                        {
                            x.StatusSC = 2;
                            x.ActionTypeID = 20;
                        }

                        else
                        {
                            x.ActionTypeID = 16;
                            ResetStatus(x);
                        }

                        repository.Update(x);
                        repository.Save();

                        WithdrawOrder(x);

                        res["success"] = 1;
                        res["message"] = string.Format("Order [{0}] has been successfully updated", x.ID);
                    }
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Terminate_(LogonUser logonUser, OrderEditSCModel o)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                if (!User.IsInRole(Constants.SALES_COORDINATOR) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    else
                    {
                        DateTime? oldBookedInstallDate = x.InstallDatetime;

                        ValidateInstallDate(o.BookedInstallDate);
                        ValidateInstallTime(o.BookedInstallTime);

                        DateTime? installDatetime = Utils.GetDateTime(o.BookedInstallDate, o.BookedInstallTime);

                        if (installDatetime != null)
                            x.InstallDatetime = installDatetime;

                        x.ContactPerson = o.ContactPerson;
                        x.ContactPersonNo = o.ContactPersonNo;
                        x.CustAddr = o.CustAddr;
                        x.CustID = o.CustID;
                        x.CustName = o.CustName;
                        x.IsCeoApproved = o.IsCeoApproved;
                        x.IsCoverageAvailable = o.IsCoverageAvailable;
                        x.IsDemandList = o.IsDemandList;
                        x.IsReqFixedLine = o.IsReqFixedLine;
                        x.IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq;
                        x.IsServiceUpgrade = o.IsServiceUpgrade;
                        x.OrderTypeID = 4;
                        x.ReasonWithdraw = o.ReasonWithdraw;
                        x.ReceivedDatetime = Utils.GetDateTime(o.ReceivedDate, o.ReceivedTime).GetValueOrDefault();
                        x.SalesPersonID = o.SalesPersonID;
                        x.StatusSC = 0; // pending
                        x.IsKIV = o.IsKIV;
                        x.IsBTUInstalled = o.IsBTUInstalled;
                        x.LastUpdateDatetime = DateTime.Now;
                        x.UserID = logonUser.UserID;
                        x.LastActionDatetime = DateTime.Now;

                        x.ActionTypeID = 4;

                        ResetStatus(x);

                        repository.Update(x);
                        repository.Save();

                        TerminateOrder(x);

                        res["success"] = 1;
                        res["message"] = string.Format("Order [{0}] has been successfully updated", x.ID);
                    }
                }
            }

            catch (UIException ex)
            {
                res["error"] = 1;
                res["message"] = ex.Message;
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult RemoveFile(int id, string filename)
        {
            Dictionary<string, object> res = new Dictionary<string, object>();

            try
            {
                OrderFile_Fibre o = repository.OrderFiles.Where(x => x.ID == id && x.FileName == filename).FirstOrDefault();

                if (o != null)
                {
                    repository.DeleteOrderFile(o);
                    repository.Save();
                }

                res["success"] = 1;
                res["message"] = string.Format("Order File {0} [{1}] has been successfully deleted", filename, id);
            }

            catch (Exception ex)
            {
                res["error"] = 1;
                res["message"] = ex.ToString();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Details(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        public ActionResult DetailsCC(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = 1;
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? 1 : statusCC.ID;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsCC(LogonUser logonUser, OrderEditCCModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.CREDIT_CONTROL) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcCCUpdate(logonUser, o, x);

                    if (o.StatusCC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusCC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsCC");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsCC(o.ID);
        }

        public ActionResult DetailsCCResubmit(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = 1;
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? 1 : statusCC.ID;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsCCResubmit(LogonUser logonUser, OrderEditCCModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.CREDIT_CONTROL) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcCCResubmitUpdate(logonUser, o, x);

                    if (o.StatusCC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusCC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsCCResubmit");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsCCResubmit(o.ID);
        }

        public ActionResult DetailsCCWithdraw(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = 1;
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? 1 : statusCC.ID;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsCCWithdraw(LogonUser logonUser, OrderEditCCModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.CREDIT_CONTROL) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcCCWithdrawUpdate(logonUser, o, x);

                    if (o.StatusCC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusCC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsCCWithdraw");
                }
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsCCWithdraw(o.ID);
        }

        public ActionResult DetailsCCTerminate(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = 1;
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? 1 : statusCC.ID;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsCCTerminate(LogonUser logonUser, OrderEditCCModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.CREDIT_CONTROL) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcCCTerminateUpdate(logonUser, o, x);

                    if (o.StatusCC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusCC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsCCTerminate");
                }
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsCCTerminate(o.ID);
        }

        public ActionResult DetailsFL(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = 1;
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? 1 : statusFL.ID;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsFL(LogonUser logonUser, OrderEditFLModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.FIXED_LINE) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcFLUpdate(logonUser, o, x);

                    if (o.StatusFL == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusFL == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsFL");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsFL(o.ID);
        }

        public ActionResult DetailsFLResubmit(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = 1;
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? 1 : statusFL.ID;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsFLResubmit(LogonUser logonUser, OrderEditFLModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.FIXED_LINE) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcFLResubmitUpdate(logonUser, o, x);

                    if (o.StatusFL == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusFL == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsFLResubmit");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsFLResubmit(o.ID);
        }

        public ActionResult DetailsFLWithdraw(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = 1;
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? 1 : statusFL.ID;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsFLWithdraw(LogonUser logonUser, OrderEditFLModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.FIXED_LINE) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcFLWithdrawUpdate(logonUser, o, x);

                    if (o.StatusFL == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusFL == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsFLWithdraw");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsFLWithdraw(o.ID);
        }

        public ActionResult DetailsFLTerminate(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = 1;
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? 1 : statusFL.ID;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsFLTerminate(LogonUser logonUser, OrderEditFLModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.FIXED_LINE) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcFLTerminateUpdate(logonUser, o, x);

                    if (o.StatusFL == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusFL == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsFLTerminate");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsFLTerminate(o.ID);
        }

        public ActionResult DetailsAC(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = 1;
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? 1 : statusAC.ID;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsAC(LogonUser logonUser, OrderEditACModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.BILLING) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcAcUpdate(logonUser, o, x);

                    if (o.StatusAC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusAC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsAC");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsAC(o.ID);
        }



        public ActionResult DetailsACResubmit(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = 1;
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? 1 : statusAC.ID;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsACResubmit(LogonUser logonUser, OrderEditACModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.BILLING) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcACResubmitUpdate(logonUser, o, x);

                    if (o.StatusAC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusAC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsACResubmit");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsACResubmit(o.ID);
        }

        public ActionResult DetailsACWithdraw(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = 1;
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? 1 : statusAC.ID;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsACWithdraw(LogonUser logonUser, OrderEditACModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.BILLING) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcACWithdrawUpdate(logonUser, o, x);

                    if (o.StatusAC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusAC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsACWithdraw");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsACWithdraw(o.ID);
        }

        public ActionResult DetailsACTerminate(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = 1;
            ViewBag.StatusInstall = "";

            LoadStatusTypesCC();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? 1 : statusAC.ID;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? null : statusInstall.Name;

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsACTerminate(LogonUser logonUser, OrderEditACModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.BILLING) && !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcACTerminateUpdate(logonUser, o, x);

                    if (o.StatusAC == 1) // success
                    {
                        TempData["message"] = "Order has been successfully approved";
                    }

                    else if (o.StatusAC == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsACTerminate");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsACTerminate(o.ID);
        }

        public ActionResult DetailsInstall(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = 0;

            LoadStatusTypesInstall();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? 1 : statusInstall.ID;

                ViewBag.OrderFiles = GetOrderFiles1(o.ID);

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsInstall(LogonUser logonUser, OrderEditInstallModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.HOD) && !User.IsInRole(Constants.INSTALLERS) &&
                    !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcInstallUpdate(logonUser, o, x);

                    if (o.StatusInstall == 1) // success
                    {
                        TempData["message"] = "Order has been successfully updated";
                    }

                    else if (o.StatusInstall == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsInstall");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsInstall(o.ID);
        }

        public ActionResult DetailsInstallResubmit(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = 0;

            LoadStatusTypesInstall();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? 1 : statusInstall.ID;

                ViewBag.OrderFiles = GetOrderFiles1(o.ID);

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsInstallResubmit(LogonUser logonUser, OrderEditInstallModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.HOD) && !User.IsInRole(Constants.INSTALLERS) &&
                    !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcInstallResubmitUpdate(logonUser, o, x);

                    if (o.StatusInstall == 1) // success
                    {
                        TempData["message"] = "Order has been successfully updated";
                    }

                    else if (o.StatusInstall == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsInstallResubmit");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsInstallResubmit(o.ID);
        }

        public ActionResult DetailsInstallResubmit1(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = 0;

            LoadStatusTypesInstall();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? 1 : statusInstall.ID;

                ViewBag.OrderFiles = GetOrderFiles1(o.ID);

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsInstallResubmit1(LogonUser logonUser, OrderEditInstallModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.HOD) && !User.IsInRole(Constants.INSTALLERS) &&
                    !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcInstallResubmit1Update(logonUser, o, x);

                    if (o.StatusInstall == 1) // success
                    {
                        TempData["message"] = "Order has been successfully updated";
                    }

                    else if (o.StatusInstall == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsInstallResubmit1");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsInstallResubmit1(o.ID);
        }

        public ActionResult DetailsInstallWithdraw(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = 0;

            LoadStatusTypesInstall();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? 1 : statusInstall.ID;

                ViewBag.OrderFiles = GetOrderFiles1(o.ID);

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsInstallWithdraw(LogonUser logonUser, OrderEditInstallModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.HOD) && !User.IsInRole(Constants.INSTALLERS) &&
                    !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcInstallWithdrawUpdate(logonUser, o, x);

                    if (o.StatusInstall == 1) // success
                    {
                        TempData["message"] = "Order has been successfully updated";
                    }

                    else if (o.StatusInstall == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsInstallWithdraw");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsInstallWithdraw(o.ID);
        }

        public ActionResult DetailsInstallTerminate(int id)
        {
            ViewBag.StatusSC = "";
            ViewBag.StatusCC = "";
            ViewBag.StatusFL = "";
            ViewBag.StatusAC = "";
            ViewBag.StatusInstall = 0;

            LoadStatusTypesInstall();

            try
            {
                Order_Fibre o = repository.Orders.Where(x => x.ID == id).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                ViewBag.StatusSC = statusSC == null ? null : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                ViewBag.StatusCC = statusCC == null ? null : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                ViewBag.StatusFL = statusFL == null ? null : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                ViewBag.StatusAC = statusAC == null ? null : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                ViewBag.StatusInstall = statusInstall == null ? 1 : statusInstall.ID;

                ViewBag.OrderFiles = GetOrderFiles1(o.ID);

                return View(o);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return View(new Order_Fibre());
        }

        [HttpPost]
        public ActionResult DetailsInstallTerminate(LogonUser logonUser, OrderEditInstallModel o)
        {
            try
            {
                if (!User.IsInRole(Constants.HOD) && !User.IsInRole(Constants.INSTALLERS) &&
                    !User.IsInRole(Constants.SUPER_ADMIN))
                    throw new UIException("You are not authorize to view this order");

                if (ModelState.IsValid)
                {
                    Order_Fibre x = repository.Orders.Where(k => k.ID == o.ID).FirstOrDefault();

                    if (x == null)
                        throw new UIException("Order not found");

                    ProcInstallTerminateUpdate(logonUser, o, x);

                    if (o.StatusInstall == 1) // success
                    {
                        TempData["message"] = "Order has been successfully updated";
                    }

                    else if (o.StatusInstall == 2) // reject
                    {
                        TempData["message"] = "Order has been successfully rejected";
                    }

                    return RedirectToAction("DetailsInstallTerminate");
                }

                else
                    throw new UIException("Form is invalid");
            }

            catch (UIException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.ToString();
            }

            return DetailsInstallTerminate(o.ID);
        }

        private object GetViewOrderObject(Order_Fibre o)
        {
            List<StatusType> ls = repository.Context.StatusTypes.ToList();

            StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
            string statusSCstr = statusSC == null ? null : statusSC.Name;

            StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
            string statusCCstr = statusCC == null ? null : statusCC.Name;

            StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
            string statusFLstr = statusFL == null ? null : statusFL.Name;

            StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
            string statusACstr = statusAC == null ? null : statusAC.Name;

            StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
            string statusIntallstr = statusInstall == null ? null : statusInstall.Name;

            var m = new
            {
                ID = o.ID,
                SalesPerson = o.SalesPerson.Name,
                OrderType = o.OrderType.Name,
                OrderTypeID = o.OrderTypeID,
                StatusSC = statusSCstr,
                ReceivedDate = o.ReceivedDatetime,
                ReceivedTime = o.ReceivedDatetime,
                CustID = o.CustID,
                CustName = o.CustName,
                CustAddr = Utils.FormatHtml(o.CustAddr),
                ContactPerson = o.ContactPerson,
                ContactPersonNo = o.ContactPersonNo,
                IsCoverageAvailable = o.IsCoverageAvailable,
                IsDemandList = o.IsDemandList,
                IsReqFixedLine = o.IsReqFixedLine,
                IsCeoApproved = o.IsCeoApproved,
                IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq,
                IsServiceUpgrade = o.IsServiceUpgrade,
                ReasonWithdraw = Utils.FormatHtml(o.ReasonWithdraw),
                BookedInstallDate = o.InstallDatetime,
                BookedInstallTime = o.InstallDatetime,
                IsKIV = o.IsKIV,
                IsBTUInstalled = o.IsBTUInstalled,

                StatusCC = statusCCstr,
                ReasonRejectCC = Utils.FormatHtml(o.ReasonRejectCC),
                RemarksCC = Utils.FormatHtml(o.RemarksCC),
                LastUpdateDatetimeCC = o.LastUpdateDatetimeCC,
                CCUser = o.CCUser == null ? "" : o.CCUser.Name,

                StatusFL = statusFLstr,
                ReasonRejectFL = Utils.FormatHtml(o.ReasonRejectFL),
                AllocatedFixedLineNo = o.AllocatedFixedLineNo,
                RemarksFL = Utils.FormatHtml(o.RemarksFL),
                LastUpdateDatetimeFL = o.LastUpdateDatetimeFL,
                FLUser = o.FLUser == null ? "" : o.FLUser.Name,

                StatusAC = statusACstr,
                ReasonRejectAC = Utils.FormatHtml(o.ReasonRejectAC),
                RemarksAC = Utils.FormatHtml(o.RemarksAC),
                IsFormReceived = o.IsFormReceived,
                LastUpdateDatetimeAC = o.LastUpdateDatetimeAC,
                ACUser = o.ACUser == null ? "" : o.ACUser.Name,

                StatusInstall = statusIntallstr,
                ReasonRejectInstall = Utils.FormatHtml(o.ReasonRejectInstall),
                InstallDate = o.InstallDatetime,
                InstallTime = o.InstallDatetime,
                BTUID = o.BTUID,
                BTUInstalled = o.BTUInstalled,
                SIPPort = o.SIPPort,
                LastUpdateDatetimeInstall = o.LastUpdateDatetimeInstall,
                InstallUser = o.InstallUser == null ? "" : o.InstallUser.Name,

                OrderFiles = GetOrderFiles(o.ID),
                CreateDatetime = o.CreateDatetime,
                LastUpdateDatetime = o.LastUpdateDatetime,
                User = o.User.Name
            };

            return m;
        }

        private object GetEditOrderObjectSC(Order_Fibre o)
        {
            List<StatusType> ls = repository.Context.StatusTypes.ToList();

            StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
            string statusSCstr = statusSC == null ? null : statusSC.Name;

            StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
            string statusCCstr = statusCC == null ? null : statusCC.Name;

            StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
            string statusFLstr = statusFL == null ? null : statusFL.Name;

            StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
            string statusACstr = statusAC == null ? null : statusAC.Name;

            StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
            string statusIntallstr = statusInstall == null ? null : statusInstall.Name;

            var m = new
            {
                ID = o.ID,
                SalesPersonID = o.SalesPersonID,
                OrderTypeID = o.OrderTypeID,
                StatusSC = statusSCstr,
                ReceivedDate = o.ReceivedDatetime,
                ReceivedTime = o.ReceivedDatetime,
                CustID = o.CustID,
                CustName = o.CustName,
                CustAddr = o.CustAddr,
                ContactPerson = o.ContactPerson,
                ContactPersonNo = o.ContactPersonNo,
                IsCoverageAvailable = o.IsCoverageAvailable,
                IsDemandList = o.IsDemandList,
                IsReqFixedLine = o.IsReqFixedLine,
                IsCeoApproved = o.IsCeoApproved,
                IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq,
                IsServiceUpgrade = o.IsServiceUpgrade,
                ReasonWithdraw = o.ReasonWithdraw,
                Comments = o.Comments,
                BookedInstallDate = o.InstallDatetime,
                BookedInstallTime = o.InstallDatetime,
                IsKIV = o.IsKIV,
                IsBTUInstalled = o.IsBTUInstalled,

                StatusCC = statusCCstr,
                ReasonRejectCC = o.ReasonRejectCC,
                RemarksCC = o.RemarksCC,
                LastUpdateDatetimeCC = o.LastUpdateDatetimeCC,
                CCUser = o.CCUser == null ? "" : o.CCUser.Name,

                StatusFL = statusFLstr,
                ReasonRejectFL = o.ReasonRejectFL,
                AllocatedFixedLineNo = o.AllocatedFixedLineNo,
                RemarksFL = o.RemarksFL,
                LastUpdateDatetimeFL = o.LastUpdateDatetimeFL,
                FLUser = o.FLUser == null ? "" : o.FLUser.Name,

                StatusAC = statusACstr,
                ReasonRejectAC = o.ReasonRejectAC,
                RemarksAC = o.RemarksAC,
                IsFormReceived = o.IsFormReceived,
                LastUpdateDatetimeAC = o.LastUpdateDatetimeAC,
                ACUser = o.ACUser == null ? "" : o.ACUser.Name,

                StatusInstall = statusIntallstr,
                ReasonRejectInstall = o.ReasonRejectInstall,
                InstallDate = o.InstallDatetime,
                InstallTime = o.InstallDatetime,
                BTUID = o.BTUID,
                BTUInstalled = o.BTUInstalled,
                SIPPort = o.SIPPort,
                LastUpdateDatetimeInstall = o.LastUpdateDatetimeInstall,
                InstallUser = o.InstallUser == null ? "" : o.InstallUser.Name,

                SalesPersons = GetSalesPersons(),
                OrderTypes = GetOrderTypes(),
                StatusTypes = GetStatusTypes(),
                OrderFiles = GetOrderFiles(o.ID),
                CreateDatetime = o.CreateDatetime,
                LastUpdateDatetime = o.LastUpdateDatetime,
                User = o.User.Name
            };

            return m;
        }

        private object GetEditOrderObjectCC(Order_Fibre o)
        {
            List<StatusType> ls = repository.Context.StatusTypes.ToList();

            StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
            string statusSCstr = statusSC == null ? null : statusSC.Name;

            StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
            int statusCCID = statusCC == null ? 1 : statusCC.ID;

            StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
            string statusFLstr = statusFL == null ? null : statusFL.Name;

            StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
            string statusACstr = statusAC == null ? null : statusAC.Name;

            StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
            string statusIntallstr = statusInstall == null ? null : statusInstall.Name;

            var m = new
            {
                ID = o.ID,
                SalesPerson = o.SalesPerson.Name,
                OrderType = o.OrderType.Name,
                OrderTypeID = o.OrderTypeID,
                StatusSC = statusSCstr,
                ReceivedDate = o.ReceivedDatetime,
                ReceivedTime = o.ReceivedDatetime,
                CustID = o.CustID,
                CustName = o.CustName,
                CustAddr = o.CustAddr,
                ContactPerson = o.ContactPerson,
                ContactPersonNo = o.ContactPersonNo,
                IsCoverageAvailable = o.IsCoverageAvailable,
                IsDemandList = o.IsDemandList,
                IsReqFixedLine = o.IsReqFixedLine,
                IsCeoApproved = o.IsCeoApproved,
                IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq,
                IsServiceUpgrade = o.IsServiceUpgrade,
                ReasonWithdraw = o.ReasonWithdraw,
                BookedInstallDate = o.InstallDatetime,
                BookedInstallTime = o.InstallDatetime,
                IsKIV = o.IsKIV,
                IsBTUInstalled = o.IsBTUInstalled,

                StatusCC = statusCCID,
                ReasonRejectCC = o.ReasonRejectCC,
                RemarksCC = o.RemarksCC,
                LastUpdateDatetimeCC = o.LastUpdateDatetimeCC,
                CCUser = o.CCUser == null ? "" : o.CCUser.Name,

                StatusFL = statusFLstr,
                ReasonRejectFL = o.ReasonRejectFL,
                AllocatedFixedLineNo = o.AllocatedFixedLineNo,
                RemarksFL = o.RemarksFL,
                LastUpdateDatetimeFL = o.LastUpdateDatetimeFL,
                FLUser = o.FLUser == null ? "" : o.FLUser.Name,

                StatusAC = statusACstr,
                ReasonRejectAC = o.ReasonRejectAC,
                RemarksAC = o.RemarksAC,
                IsFormReceived = o.IsFormReceived,
                LastUpdateDatetimeAC = o.LastUpdateDatetimeAC,
                ACUser = o.ACUser == null ? "" : o.ACUser.Name,

                StatusInstall = statusIntallstr,
                ReasonRejectInstall = o.ReasonRejectInstall,
                InstallDate = o.InstallDatetime,
                InstallTime = o.InstallDatetime,
                BTUID = o.BTUID,
                BTUInstalled = o.BTUInstalled,
                SIPPort = o.SIPPort,
                LastUpdateDatetimeInstall = o.LastUpdateDatetimeInstall,
                InstallUser = o.InstallUser == null ? "" : o.InstallUser.Name,

                StatusTypes = GetStatusTypesCC(),
                OrderFiles = GetOrderFiles(o.ID),
                CreateDatetime = o.CreateDatetime,
                LastUpdateDatetime = o.LastUpdateDatetime,
                User = o.User.Name
            };

            return m;
        }

        private object GetEditOrderObjectFL(Order_Fibre o)
        {
            List<StatusType> ls = repository.Context.StatusTypes.ToList();

            StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
            string statusSCstr = statusSC == null ? null : statusSC.Name;

            StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
            string statusCCstr = statusCC == null ? null : statusCC.Name;

            StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
            int statusFLID = statusFL == null ? 1 : statusFL.ID;

            StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
            string statusACstr = statusAC == null ? null : statusAC.Name;

            StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
            string statusIntallstr = statusInstall == null ? null : statusInstall.Name;

            var m = new
            {
                ID = o.ID,
                SalesPerson = o.SalesPerson.Name,
                OrderType = o.OrderType.Name,
                OrderTypeID = o.OrderTypeID,
                StatusSC = statusSCstr,
                ReceivedDate = o.ReceivedDatetime,
                ReceivedTime = o.ReceivedDatetime,
                CustID = o.CustID,
                CustName = o.CustName,
                CustAddr = o.CustAddr,
                ContactPerson = o.ContactPerson,
                ContactPersonNo = o.ContactPersonNo,
                IsCoverageAvailable = o.IsCoverageAvailable,
                IsDemandList = o.IsDemandList,
                IsReqFixedLine = o.IsReqFixedLine,
                IsCeoApproved = o.IsCeoApproved,
                IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq,
                IsServiceUpgrade = o.IsServiceUpgrade,
                ReasonWithdraw = o.ReasonWithdraw,
                BookedInstallDate = o.InstallDatetime,
                BookedInstallTime = o.InstallDatetime,
                IsKIV = o.IsKIV,
                IsBTUInstalled = o.IsBTUInstalled,

                StatusCC = statusCCstr,
                ReasonRejectCC = o.ReasonRejectCC,
                RemarksCC = o.RemarksCC,
                LastUpdateDatetimeCC = o.LastUpdateDatetimeCC,
                CCUser = o.CCUser == null ? "" : o.CCUser.Name,

                StatusFL = statusFLID,
                ReasonRejectFL = o.ReasonRejectFL,
                AllocatedFixedLineNo = o.AllocatedFixedLineNo,
                RemarksFL = o.RemarksFL,
                LastUpdateDatetimeFL = o.LastUpdateDatetimeFL,
                FLUser = o.FLUser == null ? "" : o.FLUser.Name,

                StatusAC = statusACstr,
                ReasonRejectAC = o.ReasonRejectAC,
                RemarksAC = o.RemarksAC,
                IsFormReceived = o.IsFormReceived,
                LastUpdateDatetimeAC = o.LastUpdateDatetimeAC,
                ACUser = o.ACUser == null ? "" : o.ACUser.Name,

                StatusInstall = statusIntallstr,
                ReasonRejectInstall = o.ReasonRejectInstall,
                InstallDate = o.InstallDatetime,
                InstallTime = o.InstallDatetime,
                BTUID = o.BTUID,
                BTUInstalled = o.BTUInstalled,
                SIPPort = o.SIPPort,
                LastUpdateDatetimeInstall = o.LastUpdateDatetimeInstall,
                InstallUser = o.InstallUser == null ? "" : o.InstallUser.Name,

                StatusTypes = GetStatusTypesCC(),
                OrderFiles = GetOrderFiles(o.ID),
                CreateDatetime = o.CreateDatetime,
                LastUpdateDatetime = o.LastUpdateDatetime,
                User = o.User.Name
            };

            return m;
        }

        private object GetEditOrderObjectAC(Order_Fibre o)
        {
            List<StatusType> ls = repository.Context.StatusTypes.ToList();

            StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
            string statusSCstr = statusSC == null ? null : statusSC.Name;

            StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
            string statusCCstr = statusCC == null ? null : statusCC.Name;

            StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
            string statusFLstr = statusFL == null ? null : statusFL.Name;

            StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
            int statusACID = statusAC == null ? 1 : statusAC.ID;

            StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
            string statusIntallstr = statusInstall == null ? null : statusInstall.Name;

            var m = new
            {
                ID = o.ID,
                SalesPerson = o.SalesPerson.Name,
                OrderType = o.OrderType.Name,
                OrderTypeID = o.OrderTypeID,
                StatusSC = statusSCstr,
                ReceivedDate = o.ReceivedDatetime,
                ReceivedTime = o.ReceivedDatetime,
                CustID = o.CustID,
                CustName = o.CustName,
                CustAddr = o.CustAddr,
                ContactPerson = o.ContactPerson,
                ContactPersonNo = o.ContactPersonNo,
                IsCoverageAvailable = o.IsCoverageAvailable,
                IsDemandList = o.IsDemandList,
                IsReqFixedLine = o.IsReqFixedLine,
                IsCeoApproved = o.IsCeoApproved,
                IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq,
                IsServiceUpgrade = o.IsServiceUpgrade,
                ReasonWithdraw = o.ReasonWithdraw,
                BookedInstallDate = o.InstallDatetime,
                BookedInstallTime = o.InstallDatetime,
                IsKIV = o.IsKIV,
                IsBTUInstalled = o.IsBTUInstalled,

                StatusCC = statusCCstr,
                ReasonRejectCC = o.ReasonRejectCC,
                RemarksCC = o.RemarksCC,
                LastUpdateDatetimeCC = o.LastUpdateDatetimeCC,
                CCUser = o.CCUser == null ? "" : o.CCUser.Name,

                StatusFL = statusFLstr,
                ReasonRejectFL = o.ReasonRejectFL,
                AllocatedFixedLineNo = o.AllocatedFixedLineNo,
                RemarksFL = o.RemarksFL,
                LastUpdateDatetimeFL = o.LastUpdateDatetimeFL,
                FLUser = o.FLUser == null ? "" : o.FLUser.Name,

                StatusAC = statusACID,
                ReasonRejectAC = o.ReasonRejectAC,
                RemarksAC = o.RemarksAC,
                IsFormReceived = o.IsFormReceived,
                LastUpdateDatetimeAC = o.LastUpdateDatetimeAC,
                ACUser = o.ACUser == null ? "" : o.ACUser.Name,

                StatusInstall = statusIntallstr,
                ReasonRejectInstall = o.ReasonRejectInstall,
                InstallDate = o.InstallDatetime,
                InstallTime = o.InstallDatetime,
                BTUID = o.BTUID,
                BTUInstalled = o.BTUInstalled,
                SIPPort = o.SIPPort,
                LastUpdateDatetimeInstall = o.LastUpdateDatetimeInstall,
                InstallUser = o.InstallUser == null ? "" : o.InstallUser.Name,

                StatusTypes = GetStatusTypesCC(),
                OrderFiles = GetOrderFiles(o.ID),
                CreateDatetime = o.CreateDatetime,
                LastUpdateDatetime = o.LastUpdateDatetime,
                User = o.User.Name
            };

            return m;
        }

        private object GetEditOrderObjectInstall(Order_Fibre o)
        {
            List<StatusType> ls = repository.Context.StatusTypes.ToList();

            StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
            string statusSCstr = statusSC == null ? null : statusSC.Name;

            StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
            string statusCCstr = statusCC == null ? null : statusCC.Name;

            StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
            string statusFLstr = statusFL == null ? null : statusFL.Name;

            StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
            string statusACstr = statusAC == null ? null : statusAC.Name;

            StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
            int statusIntallID = statusInstall == null ? 0 : statusInstall.ID;

            var m = new
            {
                ID = o.ID,
                SalesPerson = o.SalesPerson.Name,
                OrderType = o.OrderType.Name,
                OrderTypeID = o.OrderTypeID,
                StatusSC = statusSCstr,
                ReceivedDate = o.ReceivedDatetime,
                ReceivedTime = o.ReceivedDatetime,
                CustID = o.CustID,
                CustName = o.CustName,
                CustAddr = o.CustAddr,
                ContactPerson = o.ContactPerson,
                ContactPersonNo = o.ContactPersonNo,
                IsCoverageAvailable = o.IsCoverageAvailable,
                IsDemandList = o.IsDemandList,
                IsReqFixedLine = o.IsReqFixedLine,
                IsCeoApproved = o.IsCeoApproved,
                IsWithdrawFixedLineReq = o.IsWithdrawFixedLineReq,
                IsServiceUpgrade = o.IsServiceUpgrade,
                ReasonWithdraw = o.ReasonWithdraw,
                BookedInstallDate = o.InstallDatetime,
                BookedInstallTime = o.InstallDatetime,
                IsKIV = o.IsKIV,
                IsBTUInstalled = o.IsBTUInstalled,

                StatusCC = statusCCstr,
                ReasonRejectCC = o.ReasonRejectCC,
                RemarksCC = o.RemarksCC,
                LastUpdateDatetimeCC = o.LastUpdateDatetimeCC,
                CCUser = o.CCUser == null ? "" : o.CCUser.Name,

                StatusFL = statusFLstr,
                ReasonRejectFL = o.ReasonRejectFL,
                AllocatedFixedLineNo = o.AllocatedFixedLineNo,
                RemarksFL = o.RemarksFL,
                LastUpdateDatetimeFL = o.LastUpdateDatetimeFL,
                FLUser = o.FLUser == null ? "" : o.FLUser.Name,

                StatusAC = statusACstr,
                ReasonRejectAC = o.ReasonRejectAC,
                RemarksAC = o.RemarksAC,
                IsFormReceived = o.IsFormReceived,
                LastUpdateDatetimeAC = o.LastUpdateDatetimeAC,
                ACUser = o.ACUser == null ? "" : o.ACUser.Name,

                StatusInstall = statusIntallID,
                ReasonRejectInstall = o.ReasonRejectInstall,
                InstallDate = o.InstallDatetime,
                InstallTime = o.InstallDatetime,
                BTUID = o.BTUID,
                BTUInstalled = o.BTUInstalled,
                SIPPort = o.SIPPort,
                LastUpdateDatetimeInstall = o.LastUpdateDatetimeInstall,
                InstallUser = o.InstallUser == null ? "" : o.InstallUser.Name,

                StatusTypes = GetStatusTypesInstall(),
                OrderFiles = GetOrderFiles(o.ID),
                CreateDatetime = o.CreateDatetime,
                LastUpdateDatetime = o.LastUpdateDatetime,
                User = o.User.Name
            };

            return m;
        }

        public JsonResult SalesPersons()
        {
            var lx = GetSalesPersons();
            return Json(lx, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            repository.Dispose();
            base.Dispose(disposing);
        }

        private void LoadSalesPersons(object sel = null)
        {
            var q = repository.Context.Users.Where(x => (x.Roles.Any(r => r.Name == Constants.SALES)));
            var l = q.ToList();
            q = repository.Context.Users.Where(x => x.Name == Constants.STAFF_APPLICATION);
            var lk = q.ToList();
            lk.AddRange(l);
            ViewBag.SalesPersons = new SelectList(lk, "ID", "Name", sel);
        }

        private void LoadOrderTypes(object sel = null)
        {
            var l = GetOrderTypesCreate();
            ViewBag.OrderTypes = new SelectList(l, "ID", "Name", sel);
        }

        private void LoadOrderTypes1(object sel = null)
        {
            var l = GetOrderTypes();
            ViewBag.OrderTypes = new SelectList(l, "ID", "Name", sel);
        }

        private void LoadStatusTypesCC(object sel = null)
        {
            var l = GetStatusTypesCC();
            ViewBag.StatusTypesCC = new SelectList(l, "ID", "Name", sel);
        }

        private void LoadStatusTypesInstall(object sel = null)
        {
            var l = GetStatusTypesInstall();
            ViewBag.StatusTypesInstall = new SelectList(l, "ID", "Name", sel);
        }

        private dynamic GetSalesPersons()
        {
            var q = repository.Context.Users.Where(x => (x.Roles.Any(r => r.Name == Constants.SALES)));
            var l = q.ToList();
            q = repository.Context.Users.Where(x => x.Name == Constants.STAFF_APPLICATION);
            var lk = q.ToList();
            lk.AddRange(l);
            var lx = lk.Select(x => new { ID = x.ID, Name = x.Name });
            return lx;
        }

        private List<OrderType> GetOrderTypesCreate()
        {
            int[] a = new int[] { 1, 3, 4 };
            var q = repository.Context.OrderTypes.Where(x => a.Contains(x.ID));
            var l = q.ToList();
            return l;
        }

        private List<OrderType> GetOrderTypes()
        {
            var q = repository.Context.OrderTypes;
            var l = q.ToList();
            return l;
        }

        private List<StatusType> GetStatusTypes()
        {
            var q = repository.Context.StatusTypes;
            var l = q.ToList();
            return l;
        }

        private List<StatusType> GetStatusTypesCC()
        {
            return GetStatusTypesInstall();
        }

        private List<StatusType> GetStatusTypesInstall()
        {
            int[] a = new[] { 0, 1, 2, 4 };
            var q = repository.Context.StatusTypes.Where(x => a.Contains(x.ID));
            var l = q.ToList();
            return l;
        }

        private object GetOrderFiles(int id)
        {
            var q = repository.OrderFiles.Where(x => x.OrderID == id);
            var l = q.ToList();
            var o = l.Select(x => new { filename = x.FileName, url = x.GoogleFileUrl, fileID = x.ID });
            return o;
        }

        private List<OrderFile_Fibre> GetOrderFiles1(int id)
        {
            var q = repository.OrderFiles.Where(x => x.OrderID == id);
            var l = q.ToList();
            return l;
        }

        private void SendChargeNotification(Order_Fibre o)
        {
            try
            {
                List<string> l = null;

                var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));
                var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                l = new List<string>();

                l.AddRange(acUsers.Select(x => x.UserEmail));
                l.AddRange(salesUsers.Select(x => x.UserEmail));
                l.AddRange(scUsers.Select(x => x.UserEmail));

                EmailInfo emailInfo = new EmailInfo
                {
                    ToList = l,
                    DisplayName = "ProductOrderSystem",
                    Subject = "Installation Date change less than 3 days"
                };

                //todo: test send charge order email
                //l = new List<string>();

                if (l != null)
                {
                    l.Add("siewwingfei@hotmail.com");
                    //l.Add("siewwah.tham@redtone.com");
                }

                new OrderMailController().OrderInstallDateChangedEmail(o, emailInfo, ViewData).DeliverAsync();
            }

            catch (Exception)
            {
            }
        }

        private void SendUpdatedOrderEmailNotification(Order_Fibre o, int type, int? kivroleID = null)
        {
            try
            {
                if (o.CustID.IndexOf("_test_") >= 0)
                    return;

                OrderType orderType = repository.Context.OrderTypes.Where(x => x.ID == o.OrderTypeID).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                string statusSCstr = statusSC == null ? "" : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                string statusCCstr = statusCC == null ? "" : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                string statusFLstr = statusFL == null ? "" : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                string statusACstr = statusAC == null ? "" : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                string statusInstallstr = statusInstall == null ? "" : statusInstall.Name;

                ViewData["Status"] = GetOverallStatus(o);
                ViewData["Message"] = string.Format("Order has been updated [{0}]", o.CustName);
                ViewData["OrderType"] = orderType == null ? "" : orderType.Name;
                ViewData["StatusSC"] = statusSCstr;
                ViewData["StatusCC"] = statusCCstr;
                ViewData["StatusFL"] = statusFLstr;
                ViewData["StatusAC"] = statusACstr;
                ViewData["StatusInstall"] = statusInstallstr;

                List<string> l = null;

                EmailInfo emailInfo = new EmailInfo
                {
                    ToList = l,
                    DisplayName = "Order updated",
                    Subject = string.Format("Order has been updated for {0}", o.CustName)
                };

                // type 1
                // submit
                // credit control approve - is Fixed Line Req
                if (type == 1)
                {
                    var fixedLineUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.FIXED_LINE));

                    l = fixedLineUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsFL";
                }

                // type 2
                // submit
                // credit control reject
                else if (type == 2)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(ccUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(acUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 3
                // submit
                // credit control approve - not Fixed Line Req
                else if (type == 3)
                {
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = acUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsAC";
                }

                // type 4
                // submit
                // fixed line approve
                else if (type == 4)
                {
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = acUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsAC";
                }

                // type 5
                // submit
                // fixed line reject
                else if (type == 5)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(ccUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(acUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 6
                // submit
                // billing approve
                else if (type == 6)
                {
                    var installUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.INSTALLERS));
                    var hodUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.HOD));

                    l = new List<string>();

                    l.AddRange(installUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(hodUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "DetailsInstall";
                }

                // type 7
                // submit
                // billing reject
                else if (type == 7)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 8
                // submit
                // installer reject
                else if (type == 8)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 9
                // re-submit
                // install date changed / installed
                else if (type == 9)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 10
                // re-submit
                // not installed yet
                else if (type == 10)
                {
                    var installUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.INSTALLERS));
                    var hodUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.HOD));

                    l = new List<string>();

                    l.AddRange(installUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(hodUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "DetailsInstallResubmit";
                }

                // type 11
                // re-submit
                // installer reject
                else if (type == 11)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(ccUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 12
                // re-submit
                // installer approve
                else if (type == 12)
                {
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l = ccUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsCCResubmit";
                }

                // type 13
                // re-submit
                // credit control reject
                else if (type == 13)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 14
                // re-submit
                // credit control approve - is Fixed Line
                else if (type == 14)
                {
                    var fixedLineUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.FIXED_LINE));

                    l = fixedLineUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsFLResubmit";
                }

                // type 15
                // re-submit
                // credit control approve - not Fixed Line
                else if (type == 15)
                {
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = acUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsACResubmit";
                }

                // type 16
                // re-submit
                // fixed line reject
                else if (type == 16)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(ccUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(acUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 17
                // re-submit
                // fixed line approve
                else if (type == 17)
                {
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = acUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsACResubmit";
                }

                // type 18
                // re-submit
                // billing reject
                else if (type == 18)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(ccUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 19
                // re-submit
                // billing approve
                else if (type == 19)
                {
                    var installUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.INSTALLERS));
                    var hodUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.HOD));

                    l = new List<string>();

                    l.AddRange(installUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(hodUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "DetailsInstallResubmit1";
                }

                // type 20
                // re-submit
                // install date changed
                else if (type == 20)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 21
                // re-submit
                // installer reject
                else if (type == 21)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 22
                // withdraw
                // installed
                else if (type == 22)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(ccUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                    ViewData["Message"] = string.Format("Order has been updated - withdraw [{0}]", o.CustName);
                }

                // type 23
                // withdraw
                // not installed yet
                else if (type == 23)
                {
                    var installUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.INSTALLERS));
                    var hodUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.HOD));

                    l = new List<string>();

                    l.AddRange(installUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(hodUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "DetailsInstallWithdraw";
                    ViewData["Message"] = string.Format("Order has been updated - withdraw [{0}]", o.CustName);
                }

                // type 24
                // withdraw
                // installer approve
                else if (type == 24)
                {
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l = ccUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsCCWithdraw";
                }

                // type 25
                // withdraw
                // installer approve
                else if (type == 25)
                {
                    var fixedLineUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.FIXED_LINE));

                    l = fixedLineUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsFLWithdraw";
                }

                // type 26
                // withdraw
                // credit control & fixed line approve
                else if (type == 26)
                {
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = acUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsACWithdraw";
                }

                // type 27
                // withdraw
                // billing reject
                else if (type == 27)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 28
                // terminate
                // sales coordinator terminate
                else if (type == 28)
                {
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l = ccUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsCCTerminate";
                }

                // type 29
                // terminate
                // sales coordinator terminate
                else if (type == 29)
                {
                    var fixedLineUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.FIXED_LINE));

                    l = fixedLineUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsFLTerminate";
                }

                // type 30
                // terminate
                // credit control & fixed line approve
                else if (type == 30)
                {
                    var acUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.BILLING));

                    l = acUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsACTerminate";
                }

                // type 31
                // terminate
                // billing approve
                else if (type == 31)
                {
                    var installUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.INSTALLERS));
                    var hodUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.HOD));

                    l = new List<string>();

                    l.AddRange(installUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(hodUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "DetailsInstallTerminate";
                }

                // type 32
                // terminate
                // installer reject
                else if (type == 32)
                {
                    var salesUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES));
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(salesUsers.Select(x => x.UserEmail).ToList());
                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 33
                // kiv
                else if (type == 33)
                {
                    var scUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.SALES_COORDINATOR));

                    l = new List<string>();

                    l.AddRange(scUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = "Details";
                }

                // type 34
                // kiv return
                else if (type == 34)
                {
                    Role role = repository.Context.Roles.Where(k => k.ID == kivroleID).FirstOrDefault();
                    var kivUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == role.Name));

                    l = new List<string>();

                    l.AddRange(kivUsers.Select(x => x.UserEmail).ToList());

                    ViewData["Action"] = GetAction(o);
                }

                //todo: test send updated order email
                //l = new List<string>();

                if (l != null)
                {
                    l.Add("siewwingfei@hotmail.com");
                    //l.Add("siewwah.tham@redtone.com");
                }

                emailInfo.ToList = l;
                new OrderMailController().OrderNotificationEmail(o, emailInfo, ViewData, true).DeliverAsync();
            }

            catch (Exception)
            {
            }
        }

        private void SendNewOrderCreatedEmailNotification(Order_Fibre o, int type)
        {
            try
            {
                if (o.CustID.IndexOf("_test_") >= 0)
                    return;

                OrderType orderType = repository.Context.OrderTypes.Where(x => x.ID == o.OrderTypeID).FirstOrDefault();

                List<StatusType> ls = repository.Context.StatusTypes.ToList();

                StatusType statusSC = ls.Find(x => x.ID == o.StatusSC);
                string statusSCstr = statusSC == null ? "" : statusSC.Name;

                StatusType statusCC = ls.Find(x => x.ID == o.StatusCC);
                string statusCCstr = statusCC == null ? "" : statusCC.Name;

                StatusType statusFL = ls.Find(x => x.ID == o.StatusFL);
                string statusFLstr = statusFL == null ? "" : statusFL.Name;

                StatusType statusAC = ls.Find(x => x.ID == o.StatusAC);
                string statusACstr = statusAC == null ? "" : statusAC.Name;

                StatusType statusInstall = ls.Find(x => x.ID == o.StatusInstall);
                string statusIntallstr = statusInstall == null ? "" : statusInstall.Name;

                ViewData["Status"] = GetOverallStatus(o);
                ViewData["Message"] = string.Format("New Order has been created [{0}]", o.CustName);
                ViewData["OrderType"] = orderType == null ? "" : orderType.Name;
                ViewData["StatusSC"] = statusSCstr;

                List<string> l = null;

                EmailInfo emailInfo = new EmailInfo
                {
                    ToList = l,
                    DisplayName = "New order created",
                    Subject = string.Format("New order created for {0}", o.CustName)
                };

                // type 1
                // new order / variation
                // sales coordinator submit new order / variation
                if (type == 1)
                {
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l = ccUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsCC";
                }

                // type 2
                // terminate
                // sales coordinator terminate
                else if (type == 2)
                {
                    var ccUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.CREDIT_CONTROL));

                    l = ccUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsCCTerminate";
                }

                // type 3
                // terminate
                // sales coordinator terminate
                else if (type == 3)
                {
                    var fixedLineUsers = repository.Context.Users.Where(x => x.Roles.Any(k => k.Name == Constants.FIXED_LINE));

                    l = fixedLineUsers.Select(x => x.UserEmail).ToList();

                    ViewData["Action"] = "DetailsFLTerminate";
                }

                //todo: test send created order email
                //l = new List<string>();

                if (l != null)
                {
                    l.Add("siewwingfei@hotmail.com");
                    //l.Add("siewwah.tham@redtone.com");
                }

                emailInfo.ToList = l;
                new OrderMailController().OrderNotificationEmail(o, emailInfo, ViewData).DeliverAsync();
            }

            catch (Exception ex)
            {
                string b = ex.ToString();
            }
        }

        private string GetOverallStatus(Order_Fibre o)
        {
            string a = "";

            if (o.StatusSC == 0)
                a = "Pending";

            else if (o.StatusSC == 1)
                a = "Success";

            else if (o.StatusSC == 2 ||
                o.StatusCC == 2 ||
                o.StatusFL == 2 ||
                o.StatusAC == 2 ||
                o.StatusInstall == 2)
                a = "Reject";

            else if (o.StatusSC == 3 &&
                (o.StatusInstall == 0 || o.StatusInstall == null))
                a = "Withdrawn";

            else if (o.StatusSC == 4 ||
                o.StatusCC == 4 ||
                o.StatusFL == 4 ||
                o.StatusAC == 4 ||
                o.StatusInstall == 4)
                a = "KIV";

            return a;
        }

        private bool IsInstallDateValid(DateTime? a, DateTime? b)
        {
            bool r = true;

            if (a == null || b == null)
                return r;

            TimeSpan t = b.GetValueOrDefault() - a.GetValueOrDefault();
            int v = t.Days;

            int days = Math.Abs(v);

            if (days > 0 && days < 3)
                r = false;

            return r;
        }

        private bool IsInstallDateChanged(DateTime? a, DateTime? b)
        {
            bool r = false;

            if (a == null || b == null)
                return r;

            DateTime x = a.GetValueOrDefault();
            DateTime y = b.GetValueOrDefault();

            if ((x.Year != y.Year) ||
                (x.Month != y.Month) ||
                (x.Day != y.Day))
                r = true;

            return r;
        }

        private void CheckInstallDate(DateTime? oldInstallDatetime, Order_Fibre o)
        {
            bool a = IsInstallDateValid(oldInstallDatetime, o.InstallDatetime);

            if (a)
            {
                if (o.StatusInstall == 2)
                    SendUpdatedOrderEmailNotification(o, 8);
            }

            else // less than 3 days
            {
                SendChargeNotification(o);
                SendUpdatedOrderEmailNotification(o, 8);
            }
        }

        private void CheckInstallDateResubmit(DateTime? oldInstallDatetime, Order_Fibre o)
        {
            bool b = IsInstallDateChanged(oldInstallDatetime, o.InstallDatetime);

            if (b)
            {
                bool a = IsInstallDateValid(oldInstallDatetime, o.InstallDatetime);

                if (a)
                {
                    if (o.StatusInstall == 2)
                        SendUpdatedOrderEmailNotification(o, 21);
                }

                else // less than 3 days
                {
                    SendChargeNotification(o);
                    SendUpdatedOrderEmailNotification(o, 21);
                }
            }
        }

        private void CheckInstallDateChanged(DateTime? oldBookedInstallDatetime, Order_Fibre o)
        {
            bool a = IsInstallDateChanged(oldBookedInstallDatetime, o.InstallDatetime);

            if (a)
            {
                SendUpdatedOrderEmailNotification(o, 9);
            }

            else
            {
                bool b = o.StatusInstall == 1;

                if (b)
                {
                    SendUpdatedOrderEmailNotification(o, 9);
                }

                else
                {
                    SendUpdatedOrderEmailNotification(o, 10);
                }
            }
        }

        private object IsInstallDatePassed(Order_Fibre o)
        {
            bool r = false;

            if (o.InstallDatetime == null)
                return r;

            DateTime x = o.InstallDatetime.GetValueOrDefault();
            DateTime y = DateTime.Now;

            if (y > x && o.StatusSC == 0)
                r = true;

            return r;
        }

        private void WithdrawOrder(Order_Fibre o)
        {
            bool b = o.StatusInstall == 1;

            if (b)
            {
                SendUpdatedOrderEmailNotification(o, 22);
            }

            else
            {
                SendUpdatedOrderEmailNotification(o, 23);
            }
        }

        private void TerminateOrder(Order_Fibre o)
        {
            //SendUpdatedOrderEmailNotification(o, 28);
            //SendUpdatedOrderEmailNotification(o, 29);
            SendNewOrderCreatedEmailNotification(o, 2);
            SendNewOrderCreatedEmailNotification(o, 3);
        }

        private void ResetStatus(Order_Fibre x)
        {
            x.StatusAC = null;
            x.StatusCC = null;
            x.StatusFL = null;
            x.StatusInstall = null;
        }

        private void ValidateInstallDate(DateTime? dt)
        {
            if (dt != null)
            {
                DateTime x = dt.GetValueOrDefault();

                if (x.DayOfWeek == DayOfWeek.Saturday || x.DayOfWeek == DayOfWeek.Sunday)
                    throw new UIException("Please select Monday to Friday only");
            }
        }

        private void ValidateInstallTime(DateTime? dt)
        {
            bool b = false;

            if (dt != null)
            {
                DateTime x = dt.GetValueOrDefault();

                if (x.Hour < 9 || x.Hour > 17)
                    b = true;

                if (x.Hour == 17 && x.Minute > 0)
                    b = true;
            }

            if (b)
                throw new UIException("Please select 9 AM to 5 PM only");
        }

        private void ProcCCUpdate(LogonUser logonUser, OrderEditCCModel o, Order_Fibre x)
        {
            x.StatusCC = o.StatusCC;
            x.ReasonRejectCC = o.ReasonRejectCC;
            x.RemarksCC = o.RemarksCC;
            x.LastUpdateDatetimeCC = DateTime.Now;
            x.CCUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusCC == 1)
            {
                if (x.IsReqFixedLine)
                    x.ActionTypeID = 9;

                else
                    x.ActionTypeID = 5;
            }

            else if (o.StatusCC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 18;
            }

            else if (o.StatusCC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 1;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusCC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusCC == 1) // success
            {
                if (x.IsReqFixedLine)
                {
                    SendUpdatedOrderEmailNotification(x, 1);
                }

                else
                {
                    SendUpdatedOrderEmailNotification(x, 3);
                }
            }

            else if (o.StatusCC == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 2);
            }

            else if (o.StatusCC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcCCResubmitUpdate(LogonUser logonUser, OrderEditCCModel o, Order_Fibre x)
        {
            x.StatusCC = o.StatusCC;
            x.ReasonRejectCC = o.ReasonRejectCC;
            x.RemarksCC = o.RemarksCC;
            x.LastUpdateDatetimeCC = DateTime.Now;
            x.CCUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusCC == 1)
            {
                if (x.IsReqFixedLine)
                    x.ActionTypeID = 10;

                else
                    x.ActionTypeID = 6;
            }

            else if (o.StatusCC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 19;
            }

            else if (o.StatusCC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 2;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusCC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusCC == 1) // success
            {
                if (x.IsReqFixedLine)
                {
                    SendUpdatedOrderEmailNotification(x, 14);
                }

                else
                {
                    SendUpdatedOrderEmailNotification(x, 15);
                }
            }

            else if (o.StatusCC == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 13);
            }

            else if (o.StatusCC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcCCWithdrawUpdate(LogonUser logonUser, OrderEditCCModel o, Order_Fibre x)
        {
            if (x == null)
                throw new UIException("Order not found");

            x.StatusCC = o.StatusCC;
            x.ReasonRejectCC = o.ReasonRejectCC;
            x.RemarksCC = o.RemarksCC;
            x.LastUpdateDatetimeCC = DateTime.Now;
            x.CCUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusCC == 1)
            {
                if (x.StatusFL == 1)
                    x.ActionTypeID = 7;

                else
                    x.ActionTypeID = 11;
            }

            else if (o.StatusCC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusCC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 3;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusCC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusCC == 1) // success
            {
                if (x.StatusFL == 1)
                    SendUpdatedOrderEmailNotification(x, 26);
            }

            else if (o.StatusCC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcCCTerminateUpdate(LogonUser logonUser, OrderEditCCModel o, Order_Fibre x)
        {
            x.StatusCC = o.StatusCC;
            x.ReasonRejectCC = o.ReasonRejectCC;
            x.RemarksCC = o.RemarksCC;
            x.LastUpdateDatetimeCC = DateTime.Now;
            x.CCUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusCC == 1)
                x.ActionTypeID = 12;

            else if (o.StatusCC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusCC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 4;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusCC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusCC == 1) // success
            {
                if (x.StatusFL == 1)
                    SendUpdatedOrderEmailNotification(x, 30);
            }

            else if (o.StatusCC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcAcUpdate(LogonUser logonUser, OrderEditACModel o, Order_Fibre x)
        {
            x.StatusAC = o.StatusAC;
            x.ReasonRejectAC = o.ReasonRejectAC;
            x.RemarksAC = o.RemarksAC;
            x.IsFormReceived = o.IsFormReceived;
            x.LastUpdateDatetimeAC = DateTime.Now;
            x.ACUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusAC == 1)
                x.ActionTypeID = 13;

            else if (o.StatusAC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 18;
            }

            else if (o.StatusAC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 5;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusAC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusAC == 1) // success
            {
                SendUpdatedOrderEmailNotification(x, 6);
            }

            else if (o.StatusAC == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 7);
            }

            else if (o.StatusAC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcACResubmitUpdate(LogonUser logonUser, OrderEditACModel o, Order_Fibre x)
        {
            x.StatusAC = o.StatusAC;
            x.ReasonRejectAC = o.ReasonRejectAC;
            x.RemarksAC = o.RemarksAC;
            x.IsFormReceived = o.IsFormReceived;
            x.LastUpdateDatetimeAC = DateTime.Now;
            x.ACUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusAC == 1)
                x.ActionTypeID = 15;

            else if (o.StatusAC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 19;
            }

            else if (o.StatusAC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 6;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusAC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusAC == 1) // success
            {
                SendUpdatedOrderEmailNotification(x, 19);
            }

            else if (o.StatusAC == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 18);
            }

            else if (o.StatusAC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcACWithdrawUpdate(LogonUser logonUser, OrderEditACModel o, Order_Fibre x)
        {
            x.StatusAC = o.StatusAC;
            x.ReasonRejectAC = o.ReasonRejectAC;
            x.RemarksAC = o.RemarksAC;
            x.IsFormReceived = o.IsFormReceived;
            x.LastUpdateDatetimeAC = DateTime.Now;
            x.ACUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusAC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 20;
            }

            else if (o.StatusAC == 3)
            {
                x.StatusSC = 3; // cancelled
                x.ActionTypeID = null;
            }

            else if (o.StatusAC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 7;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusAC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusAC == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 27);
            }

            else if (o.StatusAC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcACTerminateUpdate(LogonUser logonUser, OrderEditACModel o, Order_Fibre x)
        {
            x.StatusAC = o.StatusAC;
            x.ReasonRejectAC = o.ReasonRejectAC;
            x.RemarksAC = o.RemarksAC;
            x.IsFormReceived = o.IsFormReceived;
            x.LastUpdateDatetimeAC = DateTime.Now;
            x.ACUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusAC == 1)
                x.ActionTypeID = 17;

            else if (o.StatusAC == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusAC == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 8;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusAC;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusAC == 1) // success
            {
                SendUpdatedOrderEmailNotification(x, 31);
            }

            else if (o.StatusAC == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcFLUpdate(LogonUser logonUser, OrderEditFLModel o, Order_Fibre x)
        {
            x.StatusFL = o.StatusFL;
            x.ReasonRejectFL = o.ReasonRejectFL;
            x.AllocatedFixedLineNo = o.AllocatedFixedLineNo;
            x.RemarksFL = o.RemarksFL;
            x.LastUpdateDatetimeFL = DateTime.Now;
            x.FLUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusFL == 1)
                x.ActionTypeID = 5;

            else if (o.StatusFL == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 18;
            }

            else if (o.StatusFL == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 9;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusFL;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusFL == 1) // success
            {
                SendUpdatedOrderEmailNotification(x, 4);
            }

            else if (o.StatusFL == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 5);
            }

            else if (o.StatusFL == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcFLResubmitUpdate(LogonUser logonUser, OrderEditFLModel o, Order_Fibre x)
        {
            x.StatusFL = o.StatusFL;
            x.ReasonRejectFL = o.ReasonRejectFL;
            x.AllocatedFixedLineNo = o.AllocatedFixedLineNo;
            x.RemarksFL = o.RemarksFL;
            x.LastUpdateDatetimeFL = DateTime.Now;
            x.FLUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusFL == 1)
                x.ActionTypeID = 6;

            else if (o.StatusFL == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 19;
            }

            else if (o.StatusFL == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 10;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusFL;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusFL == 1) // success
            {
                SendUpdatedOrderEmailNotification(x, 17);
            }

            else if (o.StatusFL == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 16);
            }

            else if (o.StatusFL == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcFLWithdrawUpdate(LogonUser logonUser, OrderEditFLModel o, Order_Fibre x)
        {
            x.StatusFL = o.StatusFL;
            x.ReasonRejectFL = o.ReasonRejectFL;
            x.AllocatedFixedLineNo = o.AllocatedFixedLineNo;
            x.RemarksFL = o.RemarksFL;
            x.LastUpdateDatetimeFL = DateTime.Now;
            x.FLUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusFL == 1)
            {
                if (x.StatusCC == 1)
                    x.ActionTypeID = 7;

                else
                    x.ActionTypeID = 3;
            }

            else if (o.StatusFL == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusFL == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 11;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusFL;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusFL == 1) // success
            {
                if (x.StatusCC == 1)
                    SendUpdatedOrderEmailNotification(x, 26);
            }

            else if (o.StatusFL == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcFLTerminateUpdate(LogonUser logonUser, OrderEditFLModel o, Order_Fibre x)
        {
            x.StatusFL = o.StatusFL;
            x.ReasonRejectFL = o.ReasonRejectFL;
            x.AllocatedFixedLineNo = o.AllocatedFixedLineNo;
            x.RemarksFL = o.RemarksFL;
            x.LastUpdateDatetimeFL = DateTime.Now;
            x.FLUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusFL == 1)
                x.ActionTypeID = 8;

            else if (o.StatusFL == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusFL == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 12;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusFL;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusFL == 1) // success
            {
                if (x.StatusCC == 1)
                    SendUpdatedOrderEmailNotification(x, 30);
            }

            else if (o.StatusFL == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcInstallUpdate(LogonUser logonUser, OrderEditInstallModel o, Order_Fibre x)
        {
            DateTime? oldInstallDatetime = x.InstallDatetime;

            ValidateInstallDate(o.InstallDate);
            ValidateInstallTime(o.InstallTime);

            DateTime? installDatetime = Utils.GetDateTime(o.InstallDate, o.InstallTime);

            x.StatusInstall = o.StatusInstall;
            x.ReasonRejectInstall = o.ReasonRejectInstall;

            if (installDatetime != null)
                x.InstallDatetime = installDatetime;

            x.BTUID = o.BTUID;
            x.BTUInstalled = o.BTUInstalled;
            x.SIPPort = o.SIPPort;
            x.LastUpdateDatetimeInstall = DateTime.Now;
            x.InstallUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusInstall == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusInstall == 1)
            {
                x.StatusSC = 1;
                x.ActionTypeID = null;
            }

            else if (o.StatusInstall == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 13;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusInstall;
            z.UserID = logonUser.UserID;

            bool b = IsInstallDateChanged(oldInstallDatetime, x.InstallDatetime);

            if (b)
            {
                z.IsInstallDateChange = true;
                bool a = IsInstallDateValid(oldInstallDatetime, x.InstallDatetime);

                if (!a)
                    z.IsInstallationPenalty = true;
            }

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusInstall == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
                return;
            }

            CheckInstallDate(oldInstallDatetime, x);
        }

        private void ProcInstallResubmitUpdate(LogonUser logonUser, OrderEditInstallModel o, Order_Fibre x)
        {
            ValidateInstallDate(o.InstallDate);
            ValidateInstallTime(o.InstallTime);

            DateTime? installDatetime = Utils.GetDateTime(o.InstallDate, o.InstallTime);

            x.StatusInstall = o.StatusInstall;
            x.ReasonRejectInstall = o.ReasonRejectInstall;

            if (installDatetime != null)
                x.InstallDatetime = installDatetime;

            x.BTUID = o.BTUID;
            x.BTUInstalled = o.BTUInstalled;
            x.SIPPort = o.SIPPort;
            x.LastUpdateDatetimeInstall = DateTime.Now;
            x.InstallUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusInstall == 1)
                x.ActionTypeID = 2;

            else if (o.StatusInstall == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 19;
            }

            else if (o.StatusInstall == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 14;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusInstall;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusInstall == 1) // success
            {
                SendUpdatedOrderEmailNotification(x, 12);
            }

            else if (o.StatusInstall == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 11);
            }

            else if (o.StatusInstall == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcInstallResubmit1Update(LogonUser logonUser, OrderEditInstallModel o, Order_Fibre x)
        {
            DateTime? oldInstallDatetime = x.InstallDatetime;

            ValidateInstallDate(o.InstallDate);
            ValidateInstallTime(o.InstallTime);

            DateTime? installDatetime = Utils.GetDateTime(o.InstallDate, o.InstallTime);

            x.StatusInstall = o.StatusInstall;
            x.ReasonRejectInstall = o.ReasonRejectInstall;

            if (installDatetime != null)
                x.InstallDatetime = installDatetime;

            x.BTUID = o.BTUID;
            x.BTUInstalled = o.BTUInstalled;
            x.SIPPort = o.SIPPort;
            x.LastUpdateDatetimeInstall = DateTime.Now;
            x.InstallUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusInstall == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusInstall == 1)
            {
                x.StatusSC = 1;
                x.ActionTypeID = null;
            }

            else if (o.StatusInstall == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 15;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusInstall;
            z.UserID = logonUser.UserID;

            bool b = IsInstallDateChanged(oldInstallDatetime, x.InstallDatetime);

            if (b)
            {
                z.IsInstallDateChange = true;
                bool a = IsInstallDateValid(oldInstallDatetime, x.InstallDatetime);

                if (!a)
                    z.IsInstallationPenalty = true;
            }

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusInstall == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
                return;
            }

            CheckInstallDateResubmit(oldInstallDatetime, x);
        }

        private void ProcInstallWithdrawUpdate(LogonUser logonUser, OrderEditInstallModel o, Order_Fibre x)
        {
            ValidateInstallDate(o.InstallDate);
            ValidateInstallTime(o.InstallTime);

            DateTime? installDatetime = Utils.GetDateTime(o.InstallDate, o.InstallTime);

            x.StatusInstall = o.StatusInstall;
            x.ReasonRejectInstall = o.ReasonRejectInstall;

            if (installDatetime != null)
                x.InstallDatetime = installDatetime;

            x.BTUID = o.BTUID;
            x.BTUInstalled = o.BTUInstalled;
            x.SIPPort = o.SIPPort;
            x.LastUpdateDatetimeInstall = DateTime.Now;
            x.InstallUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusInstall == 1)
                x.ActionTypeID = 3; // goes to CC first

            else if (o.StatusInstall == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = 20;
            }

            else if (o.StatusInstall == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 16;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusInstall;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusInstall == 1) // success
            {
                SendUpdatedOrderEmailNotification(x, 24);
                SendUpdatedOrderEmailNotification(x, 25);
            }

            else if (o.StatusInstall == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 22);
            }

            else if (o.StatusInstall == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private void ProcInstallTerminateUpdate(LogonUser logonUser, OrderEditInstallModel o, Order_Fibre x)
        {
            ValidateInstallDate(o.InstallDate);
            ValidateInstallTime(o.InstallTime);

            DateTime? installDatetime = Utils.GetDateTime(o.InstallDate, o.InstallTime);

            x.StatusInstall = o.StatusInstall;
            x.ReasonRejectInstall = o.ReasonRejectInstall;

            if (installDatetime != null)
                x.InstallDatetime = installDatetime;

            x.BTUID = o.BTUID;
            x.BTUInstalled = o.BTUInstalled;
            x.SIPPort = o.SIPPort;
            x.LastUpdateDatetimeInstall = DateTime.Now;
            x.InstallUserID = logonUser.UserID;
            x.LastActionDatetime = DateTime.Now;

            if (o.StatusInstall == 2)
            {
                x.StatusSC = 2;
                x.ActionTypeID = null;
            }

            else if (o.StatusInstall == 1)
            {
                x.StatusSC = 1;
                x.ActionTypeID = null;
            }

            else if (o.StatusInstall == 4)
            {
                x.StatusSC = 4;
                x.KIVActionTypeID = 17;
                List<Role> roles = logonUser.Roles;
                Role role = roles.FirstOrDefault();
                x.KIVRoleID = role.ID;
            }

            OrderAudit_Fibre z = new OrderAudit_Fibre();
            z.ActionDatetime = x.LastActionDatetime;
            z.CustName = x.CustName;
            z.ID = Guid.NewGuid();
            z.IsInstallationPenalty = false;
            z.IsInstallDateChange = false;
            z.OrderID = x.ID;
            z.OrderTypeID = x.OrderTypeID;
            z.OverallStatus = GetOverallStatus(x);
            z.SalesPersonID = x.SalesPersonID;
            z.Status = o.StatusInstall;
            z.UserID = logonUser.UserID;

            repository.Update(x);
            repository.InsertOrderAudit(z);
            repository.Save();

            if (o.StatusInstall == 2) // reject
            {
                SendUpdatedOrderEmailNotification(x, 32);
            }

            else if (o.StatusInstall == 4) // kiv
            {
                SendUpdatedOrderEmailNotification(x, 33);
            }
        }

        private bool GetSortQuery(ref IQueryable<Order_Fibre> q, string column)
        {
            bool a = true;

            switch (column)
            {
                case "OrderID":
                    q = q.OrderBy(x => x.ID);
                    break;

                case "OrderID_desc":
                    q = q.OrderByDescending(x => x.ID);
                    break;

                case "SalesPerson":
                    q = q.OrderBy(x => x.SalesPerson.Name);
                    break;

                case "SalesPerson_desc":
                    q = q.OrderByDescending(x => x.SalesPerson.Name);
                    break;

                case "OrderType":
                    q = q.OrderBy(x => x.OrderTypeID);
                    break;

                case "OrderType_desc":
                    q = q.OrderByDescending(x => x.OrderTypeID);
                    break;

                case "ReceivedDatetime":
                    q = q.OrderBy(x => x.ReceivedDatetime);
                    break;

                case "ReceivedDatetime_desc":
                    q = q.OrderByDescending(x => x.ReceivedDatetime);
                    break;

                case "InstallDatetime":
                    q = q.OrderBy(x => x.InstallDatetime);
                    break;

                case "InstallDatetime_desc":
                    q = q.OrderByDescending(x => x.InstallDatetime);
                    break;

                case "CustID":
                    q = q.OrderBy(x => x.CustID);
                    break;

                case "CustID_desc":
                    q = q.OrderByDescending(x => x.CustID);
                    break;

                case "CustName":
                    q = q.OrderBy(x => x.CustName);
                    break;

                case "CustName_desc":
                    q = q.OrderByDescending(x => x.CustName);
                    break;

                case "CustAddr":
                    q = q.OrderBy(x => x.CustAddr);
                    break;

                case "CustAddr_desc":
                    q = q.OrderByDescending(x => x.CustAddr);
                    break;

                case "ContactPerson":
                    q = q.OrderBy(x => x.ContactPerson);
                    break;

                case "ContactPerson_desc":
                    q = q.OrderByDescending(x => x.ContactPerson);
                    break;

                case "IsCoverageAvailable":
                    q = q.OrderBy(x => x.IsCoverageAvailable);
                    break;

                case "IsCoverageAvailable_desc":
                    q = q.OrderByDescending(x => x.IsCoverageAvailable);
                    break;

                case "IsDemandList":
                    q = q.OrderBy(x => x.IsDemandList);
                    break;

                case "IsDemandList_desc":
                    q = q.OrderByDescending(x => x.IsDemandList);
                    break;

                case "IsReqFixedLine":
                    q = q.OrderBy(x => x.IsReqFixedLine);
                    break;

                case "IsReqFixedLine_desc":
                    q = q.OrderByDescending(x => x.IsReqFixedLine);
                    break;

                case "IsCeoApproved":
                    q = q.OrderBy(x => x.IsCeoApproved);
                    break;

                case "IsCeoApproved_desc":
                    q = q.OrderByDescending(x => x.IsCeoApproved);
                    break;

                case "IsWithdrawFixedLineReq":
                    q = q.OrderBy(x => x.IsWithdrawFixedLineReq);
                    break;

                case "IsWithdrawFixedLineReq_desc":
                    q = q.OrderByDescending(x => x.IsWithdrawFixedLineReq);
                    break;

                case "IsServiceUpgrade":
                    q = q.OrderBy(x => x.IsServiceUpgrade);
                    break;

                case "IsServiceUpgrade_desc":
                    q = q.OrderByDescending(x => x.IsServiceUpgrade);
                    break;

                case "Comments":
                    q = q.OrderBy(x => x.Comments);
                    break;

                case "Comments_desc":
                    q = q.OrderByDescending(x => x.Comments);
                    break;

                case "RemarksCC":
                    q = q.OrderBy(x => x.RemarksCC);
                    break;

                case "RemarksCC_desc":
                    q = q.OrderByDescending(x => x.RemarksCC);
                    break;

                case "RemarksFL":
                    q = q.OrderBy(x => x.RemarksFL);
                    break;

                case "RemarksFL_desc":
                    q = q.OrderByDescending(x => x.RemarksFL);
                    break;

                case "RemarksAC":
                    q = q.OrderBy(x => x.RemarksAC);
                    break;

                case "RemarksAC_desc":
                    q = q.OrderByDescending(x => x.RemarksAC);
                    break;

                default:
                    a = false;
                    break;
            }

            return a;
        }

        private object GetListObject(List<Order_Fibre> l)
        {
            object lx = l.Select(x => new
            {
                ID = x.ID,
                Status = GetOverallStatus(x),
                IsInstallDatePassed = IsInstallDatePassed(x),
                SalesPersonName = x.SalesPerson == null ? null : x.SalesPerson.Name,
                OrderTypeName = x.OrderType == null ? null : x.OrderType.Name,
                ReceivedDatetime = x.ReceivedDatetime,
                InstallDatetime = x.InstallDatetime,
                CustID = x.CustID,
                CustName = x.CustName,
                CustAddr = Utils.FormatHtml(x.CustAddr),
                ContactPerson = x.ContactPerson,
                ContactPersonNo = x.ContactPersonNo,
                IsCoverageAvailable = x.IsCoverageAvailable,
                IsDemandList = x.IsDemandList,
                IsReqFixedLine = x.IsReqFixedLine,
                IsCeoApproved = x.IsCeoApproved,
                IsWithdrawFixedLineReq = x.IsWithdrawFixedLineReq,
                IsServiceUpgrade = x.IsServiceUpgrade,
                Comments = Utils.FormatHtml(x.Comments),
                RemarksCC = Utils.FormatHtml(x.RemarksCC),
                RemarksFL = Utils.FormatHtml(x.RemarksFL),
                RemarksAC = Utils.FormatHtml(x.RemarksAC),
                IsUrgent = IsUrgent(x),
                IsKIV = x.IsKIV,
                IsBTUInstalled = x.IsBTUInstalled
            });
            return lx;
        }

        private bool IsUrgent(Order_Fibre o)
        {
            bool a = false;
            TimeSpan ts = DateTime.Now - o.LastActionDatetime;

            if (User.IsInRole(Constants.CREDIT_CONTROL))
            {
                if (o.ActionTypeID >= 1 && o.ActionTypeID <= 4 && ts.Days > 1)
                    a = true;
            }

            else if (User.IsInRole(Constants.BILLING))
            {
                if (o.ActionTypeID >= 5 && o.ActionTypeID <= 8 && ts.Days > 1)
                    a = true;
            }

            else if (User.IsInRole(Constants.FIXED_LINE))
            {
                if (o.ActionTypeID >= 9 && o.ActionTypeID <= 12 && ts.Days > 1)
                    a = true;
            }

            else if (User.IsInRole(Constants.INSTALLERS))
            {
                if (o.ActionTypeID == 13 && ts.Days > 3)
                    a = true;

                else if (o.ActionTypeID == 14 && ts.Days > 1)
                    a = true;

                else if (o.ActionTypeID == 15 && ts.Days > 3)
                    a = true;

                else if (o.ActionTypeID == 16 && ts.Days > 1)
                    a = true;

                else if (o.ActionTypeID == 17 && ts.Days > 3)
                    a = true;
            }

            else if (User.IsInRole(Constants.SALES))
            {
                if (o.ActionTypeID == 18 && ts.Days > 2)
                    a = true;

                else if (o.ActionTypeID == 19 && ts.Days > 1)
                    a = true;

                else if (o.ActionTypeID == 20 && ts.Days > 2)
                    a = true;
            }

            return a;
        }

        private string GetAction(Order_Fibre o)
        {
            string a = "Details";

            switch (o.KIVActionTypeID)
            {
                case 1:
                    a = "DetailsCC";
                    break;

                case 2:
                    a = "DetailsCCResubmit";
                    break;

                case 3:
                    a = "DetailsCCWithdraw";
                    break;

                case 4:
                    a = "DetailsCCTerminate";
                    break;

                case 5:
                    a = "DetailsAC";
                    break;

                case 6:
                    a = "DetailsACResubmitDetailsACResubmit";
                    break;

                case 7:
                    a = "DetailsACWithdraw";
                    break;

                case 8:
                    a = "DetailsACTerminate";
                    break;

                case 9:
                    a = "DetailsFL";
                    break;

                case 10:
                    a = "DetailsFLResubmit";
                    break;

                case 11:
                    a = "DetailsFLWithdraw";
                    break;

                case 12:
                    a = "DetailsFLTerminate";
                    break;

                case 13:
                    a = "DetailsInstall";
                    break;

                case 14:
                    a = "DetailsInstallResubmit";
                    break;

                case 15:
                    a = "DetailsInstallResubmit1";
                    break;

                case 16:
                    a = "DetailsInstallWithdraw";
                    break;

                case 17:
                    a = "DetailsInstallTerminate";
                    break;
            }

            return a;
        }
    }
}
