using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using Sitecore.Data.Managers;
using System.Xml;
//using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using SearchPredicateSample.sitecore.admin.LogDownloader.Resources.Models;

namespace SearchPredicateSample.sitecore.admin.LogDownloader
{
    public partial class SCAnyLogDownloader : System.Web.UI.Page //Sitecore.sitecore.admin.AdminPage
    {
        /// <summary>
        /// Method for Security Check
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnInit(EventArgs e)
        //{
        //    base.CheckSecurity(true);
        //    base.OnInit(e);
        //}
        #region Event Functions

        protected void Page_Init(object sender, EventArgs e)
        {


        }

        protected void Page_Load(object sender, EventArgs e)
        {
            lblInfo.Visible = false;
            if (!IsPostBack)
            {
                ListItem listItem;
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.Load(Server.MapPath("~/sitecore/admin/LogDownloader/Resources/FileLocations.xml"));
                XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/LogFileLocations/LogFileLocation");

                listItem = new ListItem();
                listItem.Text = "Select Log File Location";
                listItem.Value = "Select Log File Location";
                ddlFileLocations.Items.Add(listItem);

                foreach (XmlNode node in nodeList)
                {
                    listItem = new ListItem();
                    listItem.Text = node.SelectSingleNode("DisplaName").InnerText;
                    listItem.Value = node.SelectSingleNode("Location").InnerText;
                    ddlFileLocations.Items.Add(listItem);
                }
            }

            if (ddlFileLocations.SelectedIndex > 0)
            {
                //
                if (ddlFileLocations.SelectedValue != null && Directory.Exists(ddlFileLocations.SelectedValue))
                {
                    List<LogFileModel> logFileList = new List<LogFileModel>();
                    List<String> logFiles = new List<string>();
                    logFiles = Directory.GetFiles(ddlFileLocations.SelectedValue).ToList();

                    //Fetch the Log Files from the Selected Directory to the list -- logFileList
                    GetLogFiles(logFileList, logFiles);

                    //Sort by Descending as the one created at the last should come at the top.
                    IOrderedEnumerable<LogFileModel> logFileListOrderedDescending = logFileList.OrderByDescending(l => l.CreatedDateTime);

                    //If there are any files generate the Table HeaderRow
                    if (logFileListOrderedDescending.Count() > 1)
                    {
                        tblLogFiles.Visible = true;
                        //Generate the Table using the Descending Sorted List object of LogFileModel Type
                        GenerateTable(logFileListOrderedDescending);

                    }
                    else
                        tblLogFiles.Visible = false;
                }
                else
                {
                    DisplayMessage("Selected", MessageType.Error);
                    //Sitecore Log Entry
                }
            }
            else
            {
                DisplayMessage("Please select a Directory Location!", MessageType.Warning);
            }
        }

        protected void ddlFileLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            lblInfo.Visible = false;
            List<string> selectedFiles = new List<string>();
            //Select the List of files to be downloaded
            foreach (TableRow tableRow in tblLogFiles.Rows)
            {
                if (!(tableRow is TableHeaderRow))
                {
                    CheckBox checkBox = tableRow.Cells[0].Controls[0] as CheckBox;
                    if (checkBox != null && checkBox.Checked)
                    {
                        selectedFiles.Add(ddlFileLocations.SelectedValue + "\\" + checkBox.ID);
                    }
                }
            }

            //If there are any selected files, in SelectedFiles 
            if (selectedFiles.Count() > 0)
            {
                //Use Memory Stream to avoid creating a zip file in a Folder Location.
                MemoryStream memoryStream = new MemoryStream();
                FileStream fileStream = null;
                ZipOutputStream zipOutputStream = null;
                try
                {
                    //string fileName = ddlFileLocations.SelectedValue.Split(@"\").Last();
                    //ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(ddlFileLocations.SelectedValue +"\\" + fileName + ".zip"));
                    zipOutputStream = new ZipOutputStream(memoryStream);
                    zipOutputStream.SetLevel(9); // 0 - store only to 9 - means best compression
                    //Crc32 crc = new Crc32();
                    if (selectedFiles != null)
                    {
                        foreach (var file in selectedFiles)
                        {
                            fileStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                            byte[] buffer = new byte[fileStream.Length];
                            fileStream.Read(buffer, 0, buffer.Length);
                            ZipEntry zipEntry = new ZipEntry(Path.GetFileName(file));
                            zipEntry.DateTime = DateTime.Now;
                            zipEntry.Comment = "SC Any Log Downloader - Multiple files";
                            //zipEntry.ZipFileIndex = 1;
                            zipEntry.Size = fileStream.Length;
                            //crc.Reset();
                            //crc.Update(buffer);
                            //zipEntry.Crc = crc.Value;
                            //Adding the Zip Entry to the Output Stream -- which would eventually go to Memory Stream!
                            zipOutputStream.PutNextEntry(zipEntry);
                            zipOutputStream.Write(buffer, 0, buffer.Length);
                        }
                        zipOutputStream.Finish();
                        zipOutputStream.Flush();
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.CacheControl = "public";
                        HttpContext.Current.Response.ContentType = "application/octet-stream";
                        HttpContext.Current.Response.AddHeader("Content-Length", memoryStream.Length.ToString());
                        HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + ddlFileLocations.SelectedItem.Text.Replace(" ", "-") + "-" + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + ".zip");
                        //HttpContext.Current.Response.OutputStream.Write(memoryStream.ToArray(), 0, memoryStream.Length);
                        HttpContext.Current.Response.BinaryWrite(memoryStream.ToArray());
                        HttpContext.Current.Response.Flush();
                        fileStream.Close();
                        zipOutputStream.Close();
                        HttpContext.Current.Response.End();
                    }
                }
                catch (System.Threading.ThreadAbortException)
                {
                    //Ignoring the thread abort exception, that will occur whenever we perform Response.End()
                }
                catch (Exception ex)
                {
                    DisplayMessage("Something went wrong while Downloading the Selected Log Files, Please contact your Administrator for more details.", MessageType.Error);
                    Sitecore.Diagnostics.Log.Error(ex.Message, ex);
                }
                finally
                {
                    //Very Important, else the Memory Stream will continue to keep the resources on our Server.
                    memoryStream.Flush();
                    memoryStream.Close();
                    memoryStream.Dispose();
                }
            }
            else
                //ERROR 
                DisplayMessage("No File selected. Please select at least one Log File to download it.", MessageType.Error);
        }

        private void LogFileViewClick(object sender, EventArgs e)
        {
            lblInfo.Visible = false;
            //Button btnLogFileView = (Button)sender;
            //string fileName = btnLogFileView.ID.Remove(btnLogFileView.ID.IndexOf(".view"), 5);
            //lblViewFileName.Text = fileName;
            //string header = btnLogFileView.ID;
            //FileStream fileStream = File.Open(ddlFileLocations.SelectedValue + "\\" + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            //StreamReader streamReader = new StreamReader(fileStream);
            //txtLogDetails.Text = streamReader.ReadToEnd();
            //string message = streamReader.ReadToEnd();
            //ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('');", true);
            //pop.Attributes["display:block
        }

        private void SingleFileDownloadClick(object sender, EventArgs e)
        {
            lblInfo.Visible = false;
            //Use Memory Stream to avoid creating a zip file in a Folder Location.
            MemoryStream memoryStream = new MemoryStream();
            FileStream fileStream = null;
            ZipOutputStream zipOutputStream = null;
            try
            {
                Button btnSingleFileDownload = (Button)sender;
                string fileName = btnSingleFileDownload.ID.Remove(btnSingleFileDownload.ID.IndexOf(".download"), 9);
                Crc32 crc = new Crc32();
                //ZipOutputStream zipOutputStream = new ZipOutputStream(File.Create(ddlFileLocations.SelectedValue +"\\" + fileName + ".zip"));
                zipOutputStream = new ZipOutputStream(memoryStream);
                zipOutputStream.SetLevel(9); // 0 - store only to 9 - means best compression
                fileStream = File.Open(ddlFileLocations.SelectedValue + "\\" + fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                //ZipEntry zipEntry = new ZipEntry(ZipEntry.CleanName(ddlFileLocations.SelectedValue + "\\" + fileName));
                ZipEntry zipEntry = new ZipEntry(Path.GetFileName(ddlFileLocations.SelectedValue + "\\" + fileName));
                zipEntry.DateTime = DateTime.Now;
                zipEntry.Comment = "SC Any Log Downloader - Single file";
                //zipEntry.ZipFileIndex = 1;
                zipEntry.Size = fileStream.Length;

                crc.Reset();
                crc.Update(buffer);
                zipEntry.Crc = crc.Value;
                zipOutputStream.PutNextEntry(zipEntry);
                zipOutputStream.Write(buffer, 0, buffer.Length);
                zipOutputStream.Finish();
                zipOutputStream.Flush();
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.CacheControl = "public";
                HttpContext.Current.Response.ContentType = "application/octet-stream";
                HttpContext.Current.Response.AddHeader("Content-Length", memoryStream.Length.ToString());
                //fileName - the name of the zip file
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName + ".zip");
                HttpContext.Current.Response.BinaryWrite(memoryStream.ToArray());
                //HttpContext.Current.Response.OutputStream.Write(buffer, 0, buffer.Length);
                HttpContext.Current.Response.Flush();
                fileStream.Close();
                zipOutputStream.Close();
                HttpContext.Current.Response.End();
            }
            catch (System.Threading.ThreadAbortException)
            {
                //Ignoring the thread abort exception, that will occur whenever we perform Response.End()

            }
            catch (Exception ex)
            {
                DisplayMessage("Something went wrong while Downloading Log File you chose, Please contact your Administrator for more details.", MessageType.Error);
                Sitecore.Diagnostics.Log.Error(ex.Message, ex);
            }
            finally
            {
                //Very Important, else the Memory Stream will continue to keep the resources on our Server.
                memoryStream.Flush();
                memoryStream.Close();
                memoryStream.Dispose();
            }
        }
        #endregion Event Functions

        #region Business Case Related Functions

        private void GenerateTable(IOrderedEnumerable<LogFileModel> logFileListOrderedDescending)
        {
            lblInfo.Visible = false;
            //Generate Table Header Row.
            TableHeaderRow tableHeaderRow;
            tableHeaderRow = GenerateHeader();

            //Add the Table Header Row to the Table
            tblLogFiles.Rows.Add(tableHeaderRow);

            //Add the Log Files sorted in Descending order, in a Page to each Row in a Table
            foreach (var logFile in logFileListOrderedDescending)
            {
                TableRow tableRow = new TableRow();
                tableRow.ID = "log" + logFile.FileName;
                tableRow.TableSection = TableRowSection.TableBody;

                TableCell tableCell = new TableCell();
                tableCell.CssClass = "CenterCell";
                //tableCell.Attributes.Add("style", "width:10% !important");
                //Select File Column - has CheckBox to Select the Log File
                CheckBox checkBoxSelectLog = new CheckBox();
                checkBoxSelectLog.ID = logFile.FileName;
                tableCell.Controls.Add(checkBoxSelectLog);
                tableRow.Cells.Add(tableCell);

                //File Name Column
                tableCell = new TableCell();
                tableCell.CssClass = "CenterCell";
                //tableCell.Attributes.Add("style", "width:30% !important");
                tableCell.Text = logFile.FileName;
                tableRow.Cells.Add(tableCell);

                //File Created DateTime Column
                tableCell = new TableCell();
                tableCell.CssClass = "CenterCell";
                //tableCell.Attributes.Add("style", "width:30% !important");
                tableCell.Text = logFile.CreatedDateTime.ToString();
                tableRow.Cells.Add(tableCell);

                //File Size Column
                tableCell = new TableCell();
                tableCell.CssClass = "CenterCell";
                //tableCell.Attributes.Add("style", "width:10% !important");
                tableCell.Text = logFile.Size;
                tableRow.Cells.Add(tableCell);

                //View LogFile Button Column
                tableCell = new TableCell();
                tableCell.CssClass = "CenterCell";
                //tableCell.Attributes.Add("style", "width:10% !important");
                Button btnLogFileView = new Button();
                btnLogFileView.ID = logFile.FileName + ".view";
                btnLogFileView.Text = "View";
                btnLogFileView.CssClass = "btn btn-primary";
                btnLogFileView.Attributes["data-toggle"] = "modal";
                btnLogFileView.Attributes["data-target"] = "#myModal";
                //btnLogFileView.Attributes["disabled"] = "disabled";
                //Register the "View LogFile" on Button Click Event
                btnLogFileView.Click += new EventHandler(LogFileViewClick);
                tableCell.Controls.Add(btnLogFileView);
                //TODO: Link with logic to view the Log File In Popup Window
                tableRow.Cells.Add(tableCell);

                //Download LogFile Button Column
                tableCell = new TableCell();
                tableCell.CssClass = "CenterCell";
                //tableCell.Attributes.Add("style", "width:10% !important");
                Button btnLogSingleFileDownload = new Button();
                btnLogSingleFileDownload.ID = logFile.FileName + ".download";
                btnLogSingleFileDownload.Text = "Download";
                btnLogSingleFileDownload.CssClass = "btn btn-primary";
                //Register the "Download LogFile" on Button Click Event
                btnLogSingleFileDownload.Click += new EventHandler(SingleFileDownloadClick);
                tableCell.Controls.Add(btnLogSingleFileDownload);
                tableRow.Cells.Add(tableCell);

                //Add the current row to the Table
                tblLogFiles.Rows.Add(tableRow);
            }
            Session.Remove("LogFileTable");
            Session.Add("LogFileTable", (Table)tblLogFiles);
        }

        private static TableHeaderRow GenerateHeader()
        {
            //lblInfo.Visible = false;
            TableHeaderRow tableHeaderRow;
            tableHeaderRow = new TableHeaderRow();

            //Select File Column - HEADER
            TableHeaderCell tableHeaderCell = new TableHeaderCell();
            tableHeaderCell.CssClass = "CenterCell";
            //tableHeaderCell.Attributes.Add("style", "width:10% !important");
            tableHeaderCell.Text = "Select File";
            tableHeaderRow.Cells.Add(tableHeaderCell);

            //Log File Name Column - HEADER
            tableHeaderCell = new TableHeaderCell();
            tableHeaderCell.CssClass = "CenterCell";
            //tableHeaderCell.Attributes.Add("style", "width:30% !important");
            tableHeaderCell.Text = "Log File Name";
            tableHeaderRow.Cells.Add(tableHeaderCell);

            //Log File Created Date Column - HEADER
            tableHeaderCell = new TableHeaderCell();
            tableHeaderCell.CssClass = "CenterCell";
            //tableHeaderCell.Attributes.Add("style", "width:30% !important");
            tableHeaderCell.Text = "Created Date (Sorted Descending)";
            tableHeaderRow.Cells.Add(tableHeaderCell);

            //Log File Size Column - HEADER
            tableHeaderCell = new TableHeaderCell();
            tableHeaderCell.CssClass = "CenterCell";
            //tableHeaderCell.Attributes.Add("style", "width:10% !important");
            tableHeaderCell.Text = "Log File Size";
            tableHeaderRow.Cells.Add(tableHeaderCell);

            //Log File View in a Popup Column - HEADER
            tableHeaderCell = new TableHeaderCell();
            tableHeaderCell.CssClass = "CenterCell";
            //tableHeaderCell.Attributes.Add("style", "width:10% !important");
            tableHeaderCell.Text = "View Log File? <font color=\"red\">Coming Soon!</font>";
            tableHeaderRow.Cells.Add(tableHeaderCell);

            //Single Log File Download - HEADER
            tableHeaderCell = new TableHeaderCell();
            tableHeaderCell.CssClass = "CenterCell";
            //tableHeaderCell.Attributes.Add("style", "width:10% !important");
            tableHeaderCell.Text = "Download Log File";
            tableHeaderRow.Cells.Add(tableHeaderCell);
            return tableHeaderRow;
        }

        private void GetLogFiles(List<LogFileModel> logFileList, List<String> logFiles)
        {
            lblInfo.Visible = false;
            foreach (var file in logFiles)
            {
                FileInfo fileInfo = new FileInfo(file);
                FileAttributes fileAttributes = fileInfo.Attributes;

                //Creating & Initialing LogFile Object
                LogFileModel logFile = new LogFileModel();
                //Name of the File
                logFile.FileName = fileInfo.Name;
                //Created DateTime of the File
                logFile.CreatedDateTime = fileInfo.CreationTime;
                //Log File Location
                logFile.Location = fileInfo.FullName;
                //Log File Location
                logFile.Size = GetFileSize(fileInfo);
                //Adding LogFile object to the List
                logFileList.Add(logFile);
            }
        }

        public string GetFileSize(FileInfo logFile)
        {
            lblInfo.Visible = false;
            double fileSize = (logFile.Length / 1024);
            return fileSize.ToString() + " Kb";
        }

        public void DisplayMessage(string message, MessageType messageType)
        {
            lblInfo.Visible = true;
            lblInfo.Text = message;
            if (messageType == MessageType.Error)
            {
                lblInfo.CssClass = "text-danger";
            }
            else if (messageType == MessageType.Warning)
            {
                lblInfo.CssClass = "text-warning";
            }
            else if (messageType == MessageType.Information)
            {
                lblInfo.CssClass = "text-success";
            }
            else
            {

            }

        }

        #endregion Business Case Related Functions
    }
    public enum MessageType
    {
        Information,
        Warning,
        Error
    }
}