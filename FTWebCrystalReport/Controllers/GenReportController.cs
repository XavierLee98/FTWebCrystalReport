using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using FTWebCrystalReport.Models;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;

namespace FTWebCrystalReport.Controllers
{
    public class GenReportController : ApiController
    {
        [HttpGet]
        [Route("api/GenLayout/{id}/{docid}/{userid}")]
        public HttpResponseMessage GenerateReport(string id, string docid, string userid)
        {
            HttpStatusCode stsCode = HttpStatusCode.OK;
            string message = "";
            try
            {
                ft_ORPT t = new ft_ORPT();
                t = ft_ORPT.LoadById(int.Parse(id));

                //test = JsonConvert.SerializeObject(t);

                bool foundoid = false;
                foreach (ft_RPT1 d in t.Lines)
                {
                    if (d.ParamName == "docid" || d.ParamName == "@docid" || d.ParamName == "?docid")
                    {
                        d.ParamValue = docid;
                        foundoid = true;
                    }
                    if (d.ParamName == "userid" || d.ParamName == "@userid" || d.ParamName == "?userid")
                    {
                        d.ParamValue = userid;
                    }
                }
                if (!foundoid)
                {
                    //message = "oid parameter not found";
                    //stsCode = HttpStatusCode.BadRequest;
                    //return Request.CreateResponse<string>(stsCode, message);
                    throw new Exception("docid parameter not found");
                }

                message = genCrystalReport(t);

                var oid = docid.Split(',');

                for (int x = 0; x < oid.Length; x++)
                {
                    ft_ORPT.LoadLinesById_afterlayoutprint(int.Parse(id), oid[x]);
                }
            }
            catch (Exception ex)
            {
                message =  ex.Message;
                stsCode = HttpStatusCode.BadRequest;
            }

            return Request.CreateResponse<string>(stsCode, message);
        }

        [HttpPost]
        [Route("api/GenReport")]
        public HttpResponseMessage GenerateReport([FromBody] ReportRequest t)
        {
            HttpStatusCode stsCode = HttpStatusCode.OK;
            string message = "";
            try
            {
                if(!string.IsNullOrEmpty(t.DBName))
                {
                    message = genCrystalReportByDB(t.Report, t.DBName);
                }
                else
                {
                    message = genCrystalReport(t.Report);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                stsCode = HttpStatusCode.BadRequest;
            }
            return Request.CreateResponse<string>(stsCode, message);
        }

        private string genCrystalReportByDB(ft_ORPT t, string DBName)
        {
            string folder = "", folderName = "Output", path = "", key = "", filename = "";

            ExportFormatType rptFormat = ExportFormatType.PortableDocFormat;

            ft_ORPT h = new ft_ORPT();
            h = ft_ORPT.LoadById(t.Id);

            if (!string.IsNullOrEmpty(t.ReportFormat))
            {
                bool validFormat = Enum.TryParse(t.ReportFormat, true, out rptFormat);
                if (!validFormat) throw new Exception("Crystal report format not valid.");
            }

            var outputExtension = GetExtension(rptFormat);

            folder = HostingEnvironment.MapPath($"~/{folderName}/" + t.Id + "/");
            if (!File.Exists(folder)) Directory.CreateDirectory(folder);

            string[] filePaths = Directory.GetFiles(HostingEnvironment.MapPath($"~/{folderName}/" + t.Id + "/"), $"*.{outputExtension}");
            if (filePaths.Length > 0)
            {
                string root = "";
                foreach (string item in filePaths)
                {
                    root = item;
                    FileInfo info = new FileInfo(root);
                    filename = Path.GetFileName(info.FullName);
                    path = HostingEnvironment.MapPath($"~/{folderName}/" + h.Id + "/") + filename;
                    if (File.Exists(root))
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                }
            }

            ReportDocument a = new ReportDocument();
            //string sapdb = WebConfigurationManager.AppSettings["sapdb"].ToString();
            string rpt = HostingEnvironment.MapPath("~/" + h.RptPath);
            a.Load(rpt);

            string conStr = WebConfigurationManager.ConnectionStrings["WebConnectionString"].ToString();

            using (SqlConnection con = new SqlConnection(conStr))
            {

                ConnectionInfo crConnectionInfo = new ConnectionInfo();
                TableLogOnInfos crtableLogoninfos = new TableLogOnInfos();
                TableLogOnInfo crtableLogoninfo = new TableLogOnInfo();
                Tables CrTables;

                SqlCommand cmd = new SqlCommand("exec LoadCrConnByDB_sp " + DBName, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count == 0)
                    throw new Exception("SAP connection config not found.");

                var SAPConnJson = dt.Rows[0]["SAPConnJson"].ToString();

                var sapConn = JsonConvert.DeserializeObject<SapConnectionConfig>(SAPConnJson);


                if (sapConn == null)
                    throw new Exception("Invalid SAP connection JSON");

                crConnectionInfo.ServerName = sapConn.SAPSERVER;
                crConnectionInfo.DatabaseName = sapConn.SAPDB;
                crConnectionInfo.UserID = sapConn.SQLUSER;
                crConnectionInfo.Password = sapConn.SQLPASSWORD;

                CrTables = a.Database.Tables;
                foreach (Table CrTable in CrTables)
                {
                    crtableLogoninfo = CrTable.LogOnInfo;
                    crtableLogoninfo.ConnectionInfo = crConnectionInfo;
                    CrTable.ApplyLogOnInfo(crtableLogoninfo);
                }

                var paramDefs = h.Lines
                                 .GroupBy(x => x.ParamName, StringComparer.OrdinalIgnoreCase)
                                 .ToDictionary(g => g.Key, g => g.First());

                foreach (var p in t.Lines)
                {
                    if (!paramDefs.TryGetValue(p.ParamName, out var def))
                        throw new Exception($"Parameter definition not found for '{p.ParamName}'.");

                    var dataType = def.DataType?.ToUpper() ?? "STRING";

                    if (dataType == "DATE")
                    {
                        if (!DateTime.TryParse(p.ParamValue, out var dateVal))
                            throw new Exception($"Invalid date value for '{p.ParamName}'.");

                        a.SetParameterValue(p.ParamName, dateVal);
                    }
                    else if (dataType == "NUMERIC")
                    {
                        if (!int.TryParse(p.ParamValue, out var numVal))
                            throw new Exception($"Invalid numeric value for '{p.ParamName}'.");

                        a.SetParameterValue(p.ParamName, numVal);
                    }
                    else
                    {
                        var field = a.ParameterFields[p.ParamName];
                        var values = (p.ParamValue ?? string.Empty)
                            .Split(',');

                        if (values.Length > 1 && !field.EnableAllowMultipleValue)
                        {
                            throw new Exception(
                                $"Parameter '{p.ParamName}' requires multiple values, but multi-value is not enabled in Crystal Report.");
                        }

                        a.SetParameterValue(
                            p.ParamName,
                            values.Length > 1 ? (object)values : values.FirstOrDefault());
                    }
                }
            }

            folder = HostingEnvironment.MapPath($"~/{folderName}/" + t.Id + "/");
            if (!File.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            filename =  Guid.NewGuid().ToString("N") + outputExtension;
            path = folder + filename;
            a.ExportToDisk(rptFormat, path);
            a.Close();
            a.Dispose();

            string lastFolder = new DirectoryInfo(folder).Name;
            string result = Path.Combine(lastFolder, filename);
            //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //var stream = new FileStream(path, FileMode.Open);
            //result.Content = new StreamContent(stream);
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            //result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            //result.Content.Headers.ContentDisposition.FileName = filename;
            //return result;
            //}
            return result;
        }

        private string genCrystalReport(ft_ORPT t)
        {
            string folder = "", folderName = "Output", path = "", key = "", filename = "";

            ExportFormatType rptFormat = ExportFormatType.PortableDocFormat;

            ft_ORPT h = new ft_ORPT();
            h = ft_ORPT.LoadById(t.Id);

            if (!string.IsNullOrEmpty(t.ReportFormat))
            {
                bool validFormat = Enum.TryParse(t.ReportFormat, true, out rptFormat);
                if (!validFormat) throw new Exception("Crystal report format not valid.");
            }

            var outputExtension = GetExtension(rptFormat);

            folder = HostingEnvironment.MapPath($"~/{folderName}/" + t.Id + "/");
            if (!File.Exists(folder)) Directory.CreateDirectory(folder);

            string[] filePaths = Directory.GetFiles(HostingEnvironment.MapPath($"~/{folderName}/" + t.Id + "/"), $"*.{outputExtension}");
            if (filePaths.Length > 0)
            {
                string root = "";
                foreach (string item in filePaths)
                {
                    root = item;
                    FileInfo info = new FileInfo(root);
                    filename = Path.GetFileName(info.FullName);
                    path = HostingEnvironment.MapPath($"~/{folderName}/" + h.Id + "/") + filename;
                    if (File.Exists(root))
                    {
                        if (File.Exists(path))
                            File.Delete(path);
                    }
                }
            }

            ReportDocument a = new ReportDocument();
            //string sapdb = WebConfigurationManager.AppSettings["sapdb"].ToString();
            string rpt = HostingEnvironment.MapPath("~/" + h.RptPath);
            a.Load(rpt);

            string conStr = WebConfigurationManager.ConnectionStrings["WebConnectionString"].ToString();

            using (SqlConnection con = new SqlConnection(conStr))
            {

                ConnectionInfo crConnectionInfo = new ConnectionInfo();
                TableLogOnInfos crtableLogoninfos = new TableLogOnInfos();
                TableLogOnInfo crtableLogoninfo = new TableLogOnInfo();
                Tables CrTables;
                SqlCommand cmd = new SqlCommand("exec LoadCrConn_sp " + t.Id.ToString(), con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                crConnectionInfo.ServerName = dt.Rows[0]["CrServer"].ToString();
                crConnectionInfo.DatabaseName = dt.Rows[0]["CrDatabase"].ToString();// con.Database.ToString();// dt.Rows[0]["sapcompany"].ToString();
                crConnectionInfo.UserID = dt.Rows[0]["CrDBUser"].ToString();
                crConnectionInfo.Password = dt.Rows[0]["CrDBPwd"].ToString();
                CrTables = a.Database.Tables;
                foreach (Table CrTable in CrTables)
                {
                    crtableLogoninfo = CrTable.LogOnInfo;
                    crtableLogoninfo.ConnectionInfo = crConnectionInfo;
                    CrTable.ApplyLogOnInfo(crtableLogoninfo);
                }
                
                foreach (ft_RPT1 d in t.Lines)
                {
                    if (d.DataType.ToUpper() == "DATE")
                    {
                        a.SetParameterValue(d.ParamName, DateTime.Parse(d.ParamValue));
                    }
                    else if (d.DataType.ToUpper() == "NUMERIC")
                    {
                        a.SetParameterValue(d.ParamName, int.Parse(d.ParamValue));
                    }
                    else
                    {
                        if (a.ParameterFields[d.ParamName].EnableAllowMultipleValue)
                        {
                            Array values = d.ParamValue.Split(',');
                            //a.SetParameterValue(d.ParamName, values);
                            //for(int x = 0; x < values.Length; x++)
                            //{
                            //    a.ParameterFields[d.ParamName].CurrentValues.AddValue(values[x]);
                            //}
                            //if (values.Length > 1)
                            //{
                            //    Array c = values.ToArray();
                            //    a.SetParameterValue(d.ParamName, c);
                            //    //var v = a.ParameterFields[d.ParamName].CurrentValues[0];

                            //    //a.ParameterFields[d.ParamName].CurrentValues.AddRange(c);
                            //}
                            //else
                            a.SetParameterValue(d.ParamName, values);

                        }
                        else
                        {
                            if (d.ParamValue.Split(',').Length > 1)
                            {
                                throw new Exception("Please enable multi value option is crystal report.");
                            }
                            a.SetParameterValue(d.ParamName, d.ParamValue);
                        }
                    }
                }
            }


            folder = HostingEnvironment.MapPath($"~/{folderName}/" + t.Id + "/");
            if (!File.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            filename = key.Replace("/", "_") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + outputExtension;
            path = folder + filename;
            a.ExportToDisk(rptFormat, path);
            a.Close();
            a.Dispose();
            //HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            //var stream = new FileStream(path, FileMode.Open);
            //result.Content = new StreamContent(stream);
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            //result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            //result.Content.Headers.ContentDisposition.FileName = filename;
            //return result;
            //}
            return filename;
        }

        private string GetExtension(ExportFormatType type)
        {
            switch (type)
            {
                case ExportFormatType.CrystalReport: return ".rpt";
                case ExportFormatType.RichText: return ".rtf";
                case ExportFormatType.WordForWindows: return ".doc";
                case ExportFormatType.Excel: return ".xls";
                case ExportFormatType.PortableDocFormat: return ".pdf";
                case ExportFormatType.HTML32:
                case ExportFormatType.HTML40: return ".html";
                case ExportFormatType.ExcelRecord: return ".xls";
                case ExportFormatType.Text: return ".txt";
                case ExportFormatType.CharacterSeparatedValues: return ".csv";
                case ExportFormatType.TabSeperatedText: return ".tsv";
                case ExportFormatType.EditableRTF: return ".rtf";
                case ExportFormatType.Xml: return ".xml";
                case ExportFormatType.RPTR: return ".rptr";
                case ExportFormatType.ExcelWorkbook:
                case ExportFormatType.XLSXPagebased:
                case ExportFormatType.XLSXRecord:
                    return ".xlsx";
                default:
                    return "";
            }
        }
    }
}
