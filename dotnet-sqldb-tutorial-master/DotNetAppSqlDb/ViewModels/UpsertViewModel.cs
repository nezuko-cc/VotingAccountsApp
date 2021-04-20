using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace DotNetAppSqlDb.ViewModels
{
    public class UpsertViewModel
    {
        public string Input { get; set; }

        public String NewAccountType { get; set; }

        public string AdminSecretToken { get; set; }

        public HttpPostedFileBase UploadedCSVFile { get; set; }
    }
}
