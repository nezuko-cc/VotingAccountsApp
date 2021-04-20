using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DotNetAppSqlDb.Models;
using System.Diagnostics;
using DotNetAppSqlDb.ViewModels;
using System.Text;
using System.IO;

namespace DotNetAppSqlDb.Controllers
{
    public class AccountsController : Controller
    {
        private MyDatabaseContext db = new MyDatabaseContext();
        private static int MaxCcount = 200;
        private static string AvailableState = "Open";
        private static string UnavailableState = "Claimed";
        private static char AccountPasswordSeparator = '-';

        // Homepage
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAccounts(IndexViewModel indexViewModel)
        {
            Trace.WriteLine("GET /Accounts/GetAccounts");

            if (indexViewModel.AccountType == null)
            {
                ModelState.AddModelError("AccountType", "領票類型不正確");
                return View("Index", indexViewModel);
            }
            string typeName = indexViewModel.AccountType.ToString();
            List<Account> accountList = new List<Account>();

            // Special retrieval, admin only. Thus we don't consider the output limit here
            if (indexViewModel.AccountType != AccountType.Normal && indexViewModel.AccountType != AccountType.Urgent)
            {
                if (indexViewModel.AdminSecretToken != "y@2swIvY3P1CTBqy" && indexViewModel.AdminSecretToken != "X!JjKVOrKTfdh1Tu")
                {
                    ModelState.AddModelError("AccountType", "你沒有權限觀看");
                    return View("Index", indexViewModel);
                }

                var admiOnlyResult = db.Accounts.Where(account => account.AccountType == typeName).Take(indexViewModel.Count).ToList();
                return View(admiOnlyResult);
            }

            // Normal user use case
            if (indexViewModel.Count <= 0 || indexViewModel.Count > MaxCcount)
            {
                ModelState.AddModelError("Count", "領號數量必須是 1 - 200");
                return View("Index", indexViewModel);
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    //var tests = db.Accounts
                    //    .SqlQuery("SELECT * FROM dbo.Accounts")
                    //    .ToList();

                    var result = db.Accounts
                        .Where(account => account.AccountType == typeName && account.ClaimedState == AvailableState)
                        .OrderBy(r => Guid.NewGuid()).Take(indexViewModel.Count);
                    accountList = result.ToList();

                    if (accountList.Count == 0)
                    {
                        return View(accountList);
                    }

                    // Update ClaimedState
                    StringBuilder sb = new StringBuilder("UPDATE dbo.Accounts SET ClaimedState = '").Append(UnavailableState).Append("' WHERE AccountId IN (");
                    for (int i = 0; i < accountList.Count; i++)
                    {
                        sb.Append("'").Append(accountList[i].AccountId).Append("'");
                        if (i < accountList.Count - 1)
                        {
                            sb.Append(",");
                        }
                    }
                    string sqlRawQuery = sb.Append(")").ToString();
                    db.Database.ExecuteSqlCommand(sqlRawQuery);

                    db.SaveChanges();
                    transaction.Commit();

                    stopWatch.Stop();
                    long SortDuration = stopWatch.ElapsedMilliseconds;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Trace.WriteLine("Update retrieved accounts error: " + ex.Message);
                    ModelState.AddModelError("ErrorMsg", "可能與別人撞號了!請再次取號，若多次失敗請聯絡所屬打投群的管理者");
                    return View("Index", indexViewModel);
                }
            }

            return View(accountList);
        }

        // GET: Todos/Create
        public ActionResult Create()
        {
            Trace.WriteLine("GET /Accounts/Create");
            return View(new Account { });
        }

        // POST: Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AccountId,Password,AccountType,ClaimedState")] Account account)
        {
            Trace.WriteLine("POST /Accounts/Create");
            if (ModelState.IsValid)
            {
                db.Accounts.Add(account);
                db.SaveChanges();
                return RedirectToAction("");
            }

            return View(account);
        }

        public ActionResult Upsert()
        {
            Trace.WriteLine("GET /Accounts/Upsert");
            return View();
        }

        //// Bulk upsert
        //// POST: Accounts/Upsert
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Upsert(UpsertViewModel upsertViewModel)
        //{
        //    Trace.WriteLine("POST /Accounts/Upsert");

        //    //string[] accountArray = upsertViewModel.Input.TrimEnd().Replace("\n", "----").Split(AccountPasswordSeparator);

        //    string trimedString = upsertViewModel.Input.TrimEnd();
        //    Dictionary<string, string> keyValuePairs = trimedString.Split('\n')
        //                                                  .Select(value => value.Split('-'))
        //                                                  .ToDictionary(pair => pair[0], pair => pair[1]);

        //    if (ModelState.IsValid)
        //    {
        //        db.SaveChanges();
        //        return RedirectToAction("");
        //    }

        //    return View();
        //}

        // Bulk upsert
        // POST: Accounts/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upsert(UpsertViewModel upsertViewModel)
        {
            Trace.WriteLine("POST /Accounts/Upsert");
            string[] stringSeparators = new string[] { "----", "---", "--", "-" };

            if (!IsCsvFile(upsertViewModel.UploadedCSVFile))
            {
                //Error message
                return View();
            }

            // Verify that the user selected a file
            if (upsertViewModel.UploadedCSVFile != null && upsertViewModel.UploadedCSVFile.ContentLength > 0)
            {

                StreamReader csvReader = new StreamReader(upsertViewModel.UploadedCSVFile.InputStream);
                {
                    string inputLine = "";
                    while ((inputLine = csvReader.ReadLine()) != null)
                    {
                        string[] values = inputLine.Split(stringSeparators, StringSplitOptions.None);

                        string accountId = values[0];
                        var returnedAccount = db.Accounts.Find(accountId);
                        // New account to be added
                        if (returnedAccount == null)
                        {
                            Account newAccount = new Account();
                            newAccount.AccountId = values[0];
                            newAccount.Password = values[1];
                            newAccount.AccountType = upsertViewModel.NewAccountType;
                            db.Accounts.Add(newAccount);
                        }
                        else
                        {

                        }

                        db.SaveChanges();
                    }
                    csvReader.Close();
                }
            }

            return View();
        }

        private bool IsCsvFile(HttpPostedFileBase inputFile)
        {
            if (inputFile == null)
            {
                return false;
            }

            int lastIndexOfDot = inputFile.FileName.LastIndexOf('.');
            string fileTypeName = inputFile.FileName.Substring(lastIndexOfDot + 1).ToUpper();
            if (fileTypeName == "CSV")
            {
                return true;
            }

            return false;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
