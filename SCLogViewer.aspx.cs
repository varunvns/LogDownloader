using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SearchPredicateSample.sitecore.admin.LogDownloader
{
    public partial class SCLogViewer : Sitecore.sitecore.admin.AdminPage //System.Web.UI.Page
    {
        private const string LOGFILEID = "LogID";

        protected override void OnInit(EventArgs e)
        {
            base.CheckSecurity(true);
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    if (string.IsNullOrEmpty(Request.QueryString[LOGFILEID]))
                    {
                        //ShowMessage("Please select the package to uninstall", "Error");

                    }
                    else
                    {
                        LoadLogFile();
                    }
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("An error occurred while loading the Log File", ex, this);
                //ShowMessage("An error occurred while loading package details : " + ex.Message, "Error");
            }
        }

        private void LoadLogFile()
        {
            FileStream fileStream = File.Open(LOGFILEID, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader streamReader = new StreamReader(fileStream);
            txtLogDetails.Text = streamReader.ReadToEnd();
            //lblHeader.Text = "";
        }


    }
}