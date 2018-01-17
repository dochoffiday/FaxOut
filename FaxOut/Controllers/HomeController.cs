using FaxOut.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Mvc;

namespace FaxOut.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                using (var Context = new DbContext())
                {
                    var pc = new Phaxio.Phaxio(PhaxioHelper.Key, PhaxioHelper.Secret);

                    foreach (var pendingFax in Context.GetPendingFaxes())
                    {
                        var faxInfo = pc.GetFaxInfo(pendingFax.ExternalId);
                        Context.Update(pendingFax, faxInfo.IsTest, faxInfo.PageCount, faxInfo.CostInCents, faxInfo.Status, faxInfo.CompletedAt);
                    }
                }
            }, null);

            using (var Context = new DbContext())
            {
                var model = Context.GetPublicContacts();

                return View(model);
            }
        }

        public ActionResult Stats()
        {
            using (var Context = new DbContext())
            {
                var model = Context.GetStats(PhaxioHelper.IsTest);
                
                var pc = new Phaxio.Phaxio(PhaxioHelper.Key, PhaxioHelper.Secret);
                model.Credit = pc.GetAccountStatus().Balance;
                    
                return View(model);
            }
        }

        public ActionResult Create(CreateFax model)
        {
            using (var Context = new DbContext())
            {
                var returnVal = new CreateFaxResult();
                var numberAsInt = (long)0;

                if (string.IsNullOrWhiteSpace(model.To))
                {
                    returnVal.Errors.Add(new KeyValuePair<string, string>("to", "Name is required!"));
                }

                if (string.IsNullOrWhiteSpace(model.Subject))
                {
                    returnVal.Errors.Add(new KeyValuePair<string, string>("subject", "Subject is required!"));
                }

                if (string.IsNullOrWhiteSpace(model.Message))
                {
                    returnVal.Errors.Add(new KeyValuePair<string, string>("message", "Message is required!"));
                }

                if (!long.TryParse(model.Number, out numberAsInt) || model.Number.Length != 10)
                {
                    returnVal.Errors.Add(new KeyValuePair<string, string>("number", "Invalid phone number!"));
                }

                if (!returnVal.Errors.Any())
                {
                    var fax = Context.Create(model.To, model.Number, model.Subject, model.Message, GetIpAddress());

                    returnVal.Id = fax.FaxId;
                }

                return Json(returnVal);
            }
        }

        public ActionResult Send(int id)
        {
            using (var Context = new DbContext())
            {
                var fax = Context.GetFax(id);
                var model = new SendFax { Id = id };

                if (fax == null) return RedirectToAction("home");

                try
                {
                    var pc = new Phaxio.Phaxio(PhaxioHelper.Key, PhaxioHelper.Secret);

                    if (string.IsNullOrWhiteSpace(fax.ExternalId))
                    {
                        var fileName = Server.MapPath(string.Format("~/temp/{0}.html", id));

                        using (var wc = new WebClient())
                        {
                            wc.DownloadFile(Url.Action("Html", "Home", new { id = id }, this.Request.Url.Scheme), fileName);
                        }

                        var externalId = pc.SendFax(new Phaxio.Entities.FaxRequest
                        {
                            ToNumber = "+1" + fax.Number,
                            File = new System.IO.FileInfo(fileName)
                        });

                        Context.Update(fax, externalId);

                        model.WarningMessage = string.Format("This fax is listed as: \"{0}\". Refresh the page to update the status.", "creating");

                        return View(model);
                    }

                    var faxInfo = pc.GetFaxInfo(fax.ExternalId);

                    Context.Update(fax, faxInfo.IsTest, faxInfo.PageCount, faxInfo.CostInCents, faxInfo.Status, faxInfo.CompletedAt);

                    if (!string.IsNullOrWhiteSpace(faxInfo.ErrorMessage))
                    {
                        throw new Exception(faxInfo.ErrorMessage);
                    }

                    if (faxInfo.CompletedAt.HasValue)
                    {
                        model.SuccessMessage = string.Format("This fax was successfully sent on {0} and cost <b>{1}</b>.", faxInfo.CompletedAt.Value.ToString("g"), ModelHelper.CentsToDollars(faxInfo.CostInCents));
                    }
                    else
                    {
                        model.WarningMessage = string.Format("This fax is listed as: \"{0}\". Refresh the page to update the status.", faxInfo.Status);
                    }
                }
                catch (Exception ex)
                {
                    model.ErrorMessage = ex.ToString();
                }

                return View(model);
            }
        }

        public ActionResult Html(int id)
        {
            using (var Cotext = new DbContext())
            {
                var fax = Cotext.GetFax(id);

                return View(fax);
            }
        }

        #region Privates

        private string GetIpAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }

        #endregion
    }
}