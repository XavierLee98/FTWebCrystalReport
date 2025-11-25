using System;
using System.Collections.Generic;
using System.Data;

namespace FTWebCrystalReport.Models
{
    public class ExcelConfig
    {
        public int Id { get; set; }
        public string ExcelName { get; set; }
        public string ExcelPathFile { get; set; }
        public string ExcelWebUrl { get; set; }
        public bool Active { get; set; }
        public List<ExcelSheet> ExcelSheets { get; set; } = new List<ExcelSheet>();

        protected static string CN_Id = "Id";
        protected static string CN_ExcelName = "ExcelName";
        protected static string CN_ExcelPathFile = "ExcelPathFile";
        protected static string CN_ExcelWebUrl = "ExcelWebUrl";
        protected static string CN_Active = "Active";
        
        public static ExcelConfig LoadExcelConfig(int id)
        {
            ExcelConfig h = new ExcelConfig();
            DataTable dt = new DataTable();
            dt = DAC.ExecuteDataTable("LoadExcelConfigById_sp",
                  DAC.Parameter(CN_Id, id));
            try
            {
                if (dt.Rows.Count > 0)
                {
                    h.Id = int.Parse(dt.Rows[0][CN_Id].ToString().Trim());
                    if (dt.Columns.Contains(CN_ExcelName)) h.ExcelName = dt.Rows[0][CN_ExcelName].ToString().Trim();
                    if (dt.Columns.Contains(CN_ExcelPathFile)) h.ExcelPathFile = dt.Rows[0][CN_ExcelPathFile].ToString().Trim();
                    if (dt.Columns.Contains(CN_ExcelWebUrl)) h.ExcelWebUrl = dt.Rows[0][CN_ExcelWebUrl].ToString().Trim();
                    if (dt.Columns.Contains(CN_Active)) h.Active = Convert.ToBoolean(dt.Rows[0][CN_Active]);
                    h.ExcelSheets = ExcelSheet.LoadExcelTabs(h.Id);
                }
                else
                {
                    h.Id = 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return h;
        }
    }

    public class ExcelSheet
    {
        public int Id { get; set; }
        public int ExcelId { get; set; }
        public string SheetName { get; set; }
        public string Query { get; set; }
        //public List<ExcelSheetParam> ExcelSheetParams { get; set; } = new List<ExcelSheetParam>();

        protected static string CN_Id = "Id";
        protected static string CN_ExcelId = "ExcelId";
        protected static string CN_SheetName = "SheetName";
        protected static string CN_Query = "Query";

        public static List<ExcelSheet> LoadExcelTabs(int excelId)
        {
            ExcelSheet d = null;
            string spName = "LoadExcelSheetsById_sp";

            DataTable dt = new DataTable();
            dt = DAC.ExecuteDataTable(spName,
                   DAC.Parameter(CN_ExcelId, excelId));
            List<ExcelSheet> lines = new List<ExcelSheet>();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    d = new ExcelSheet();
                    d.Id = int.Parse(dt.Rows[i][CN_Id].ToString().Trim());
                    d.ExcelId = excelId;
                    if (dt.Columns.Contains(CN_SheetName)) d.SheetName = dt.Rows[i][CN_SheetName].ToString().Trim();
                    if (dt.Columns.Contains(CN_Query)) d.Query = dt.Rows[i][CN_Query].ToString().Trim();
                    //d.ExcelSheetParams = ExcelSheetParam.LoadExcelTabParams(d.Id);

                    lines.Add(d);
                }
            }
            return lines;
        }
    }
}
