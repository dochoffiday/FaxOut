using FaxOut.Data.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace FaxOut.Data
{
    public class DbContext : IDisposable
    {
        private Context.DataContext Context;
        
        public DbContext()
        {
            if (HttpContext.Current.Request.IsLocal)
            {

            }

            Context = new Context.DataContext(ConfigurationManager.AppSettings["SQLSERVER_CONNECTION_STRING"]);
        }

        public List<Contact> GetPublicContacts()
        {
            using (var reader = new StreamReader(HttpContext.Current.Server.MapPath(@"~/App_Data/contacts.json")))
            {
                var json = reader.ReadToEnd();

                return JsonConvert.DeserializeObject<List<Contact>>(json);
            }
        }

        public Stats GetStats(bool isTest)
        {
            var faxes = Context.Faxes.Count(p => p.ExternalIsTest == isTest && p.ExternalCompletedAt.HasValue);
            var pages = faxes > 0 ? Context.Faxes.Where(p => p.ExternalIsTest == isTest && p.ExternalCompletedAt.HasValue && p.ExternalPages.HasValue).Sum(p => p.ExternalPages.Value) : 0;
            var cents = faxes > 0 ? Context.Faxes.Where(p => p.ExternalIsTest == isTest && p.ExternalCompletedAt.HasValue && p.ExternalCents.HasValue).Sum(p => p.ExternalCents.Value) : 0;

            return new Stats
            {
                Faxes = faxes,
                Pages = pages,
                Cents = cents
            };
        }

        public Fax GetFax(int id)
        {
            return Context.Faxes.FirstOrDefault(p => p.FaxId == id);
        }

        public Fax Create(string to, string number, string subject, string message, string ip)
        {
            var fax = new Fax
            {
                Name = to,
                Number = number,
                Subject = subject,
                Text = message,
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = ip
            };

            Context.Faxes.InsertOnSubmit(fax);
            Context.SubmitChanges();

            return fax;
        }

        public void Update(Fax fax, string externalId)
        {
            fax.ExternalId = externalId;

            Context.SubmitChanges();
        }

        public void Update(Fax fax, bool externalIsTest, int externalPages, int externalCents, string externalStatus, DateTime? externalCompletedAt)
        {
            fax.ExternalIsTest = externalIsTest;
            fax.ExternalPages = externalPages;
            fax.ExternalCents = externalCents;
            fax.ExternalStatus = externalStatus;
            fax.ExternalCompletedAt = externalCompletedAt;

            Context.SubmitChanges();
        }

        public void SubmitChanges()
        {
            Context.SubmitChanges();
        }

        public void Dispose()
        {
            Context.Dispose();
        }
    }
}