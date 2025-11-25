using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using FTWebCrystalReport.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Hosting;
using System.Web.Http;
using WebGrease.Activities;

namespace FTWebCrystalReport.Controllers
{
    public class GenExcelController : ApiController
    {
        public GenExcelController() { }

        [AllowAnonymous]
        [HttpPost]
        [Route("api/GenExcel/{id}")]
        public HttpResponseMessage GenerateExcel(int id, [FromBody] Dictionary<string, object> request)
        {
            HttpStatusCode stsCode = HttpStatusCode.OK;

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            string message = "";

            var parameters = new Dictionary<string, object>(request, StringComparer.OrdinalIgnoreCase);

            try
            {
                ExcelConfig excelConfig = ExcelConfig.LoadExcelConfig(id);
                if (excelConfig?.ExcelSheets == null || !excelConfig.ExcelSheets.Any())
                    throw new Exception($"Excel Config or sheets not found for Id = {id}.");

                if (excelConfig == null || excelConfig.ExcelSheets == null) throw new Exception($"Excel Config or sheets not found for Id = {id}.");

                if (!excelConfig.Active) throw new Exception($"Excel not active.");

                foreach (var sheet in excelConfig.ExcelSheets)
                {
                    string query = sheet.Query;
                    var matches = Regex.Matches(query, @"\{(.*?)\}", RegexOptions.IgnoreCase);

                    var requiredParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (Match match in matches)
                    {
                        string paramName = match.Groups[1].Value;

                        if (!requiredParams.Contains(paramName))
                            requiredParams.Add(paramName);
                    }

                    foreach (var param in requiredParams)
                    {
                        if (!parameters.ContainsKey(param))
                            throw new Exception($"Sheet '{sheet.SheetName}' is missing required parameter '{param}'.");
                    }
                }

                var filePath = GenerateExcel(excelConfig, parameters);

                if (!System.IO.File.Exists(filePath))
                    throw new Exception("Generated Excel file not found.");

                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(fileBytes)
                };
                response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = Path.GetFileName(filePath)
                };
                response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                return response;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(ex.Message)
                };
            }
        }

        private string GenerateExcel(ExcelConfig config, Dictionary<string, object> userParams)
        {
            string folder = "", path = "", key = "", filename = "";

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    foreach (var sheet in config.ExcelSheets)
                    {
                        string query = sheet.Query;

                        var matches = Regex.Matches(query, @"\{(.*?)\}", RegexOptions.IgnoreCase);
                        var requiredParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        foreach (Match match in matches)
                        {
                            string paramName = match.Groups[1].Value;
                            requiredParams.Add(paramName);
                        }

                        foreach (var param in requiredParams)
                        {
                            if (!userParams.ContainsKey(param))
                                throw new Exception($"Sheet '{sheet.SheetName}' is missing required parameter '{param}'.");
                        }

                        foreach (var param in requiredParams)
                        {
                            var value = userParams[param] ?? DBNull.Value;
                            string formattedValue = value is string || value is DateTime
                                ? $"'{value}'"
                                : value.ToString();
                            query = query.Replace($"{{{param}}}", formattedValue);
                        }

                        DataTable dt = new DataTable();
                        using (SqlConnection conn = new SqlConnection(DAC.con))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(dt);
                            }
                        }

                        var ws = workbook.Worksheets.Add(sheet.SheetName);
                        ws.Cell(1, 1).InsertTable(dt);
                        ws.Columns().AdjustToContents();
                        ws.Rows().AdjustToContents();
                    }

                    folder = HostingEnvironment.MapPath("~/Excel/" + config.Id + "_" + config.ExcelName + "/");
                    if (!File.Exists(folder)) Directory.CreateDirectory(folder);

                    filename = config.ExcelName + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                    path = folder + filename;

                    workbook.SaveAs(path);

                    return path;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}