using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FTWebCrystalReport.Models
{
    public class ft_ORPT
    {
        #region Constructor
        public ft_ORPT() { }
        #endregion

        #region Const Column Name
        protected static string CN_Id = "OID";
        protected static string CN_RptName = "ReportName";
        protected static string CN_RptDisplayName = "ReportName";
        protected static string CN_RptPath = "ReportPathFile";
        protected static string CN_RptFormat = "ReportFormat";
        protected static string CN_docid = "docid";
        #endregion

        #region Properties
        public int Id { get; set; }
        public string RptName { get; set; }
        public string RptDisplayName { get; set; }
        public string ReportFormat { get; set; }
        public string RptPath { get; set; }
        public List<ft_RPT1> Lines { get; set; }
        #endregion

        #region Public Function
        public static ft_ORPT LoadById(int id)
        {
            ft_ORPT h = new ft_ORPT();
            DataTable dt = new DataTable();
            dt = DAC.ExecuteDataTable("LoadORPTById_sp",
                  DAC.Parameter(CN_Id, id));
            try
            {
                if (dt.Rows.Count > 0)
                {
                    h.Id = int.Parse(dt.Rows[0][CN_Id].ToString().Trim());
                    if (dt.Columns.Contains(CN_RptName)) h.RptName = dt.Rows[0][CN_RptName].ToString().Trim();
                    if (dt.Columns.Contains(CN_RptDisplayName)) h.RptDisplayName = dt.Rows[0][CN_RptDisplayName].ToString().Trim();
                    if (dt.Columns.Contains(CN_RptFormat)) h.RptDisplayName = dt.Rows[0][CN_RptFormat].ToString().Trim();
                    if (dt.Columns.Contains(CN_RptPath)) h.RptPath = dt.Rows[0][CN_RptPath].ToString().Trim();
                    h.Lines = ft_RPT1.LoadLinesBySKHdr(h.Id);
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
        public static void LoadLinesById_afterlayoutprint(int id, string docid)
        {
            string spName = "LoadORPTById_sp_afterlayoutprint";
            DAC.ExecuteDataTable(spName,
                   DAC.Parameter(CN_Id, id),
                   DAC.Parameter(CN_docid, docid));
        }
        #endregion
    }

    public class ft_RPT1
    {
        #region Constructor
        public ft_RPT1() { }
        #endregion

        #region Const Column Name
        protected static string CN_Id = "OID";
        protected static string CN_SKHdr = "CrReport";
        protected static string CN_ParamName = "ParamCode";
        protected static string CN_ParamDisplayName = "ParamName";
        protected static string CN_DataType = "ParamType";
        protected static string CN_ParamValue = "ParamValue";
        protected static string CN_ParamSQL = "ParamSQL";
        #endregion

        #region Properties
        public int Id { get; set; }
        public int SKHdr { get; set; }
        public string ParamName { get; set; }
        public string ParamDisplayName { get; set; }
        public string DataType { get; set; }
        public string ParamValue { get; set; }
        public string ParamSQL { get; set; }
        #endregion

        #region Public Function
        public static List<ft_RPT1> LoadLinesBySKHdr(int skhdr)
        {
            ft_RPT1 d = null;
            string spName = "LoadRPT1ByHdr_sp";

            DataTable dt = new DataTable();
            dt = DAC.ExecuteDataTable(spName,
                   DAC.Parameter(CN_SKHdr, skhdr));
            List<ft_RPT1> lines = new List<ft_RPT1>();
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    d = new ft_RPT1();
                    d.Id = int.Parse(dt.Rows[i][CN_Id].ToString().Trim());
                    d.SKHdr = skhdr;
                    if (dt.Columns.Contains(CN_ParamName)) d.ParamName = dt.Rows[i][CN_ParamName].ToString().Trim();
                    if (dt.Columns.Contains(CN_ParamDisplayName)) d.ParamDisplayName = dt.Rows[i][CN_ParamDisplayName].ToString().Trim();
                    if (dt.Columns.Contains(CN_DataType))
                    {
                        var raw = dt.Rows[i][CN_DataType].ToString().Trim();

                        if (!int.TryParse(raw, out var value) ||
                            !Enum.IsDefined(typeof(DataTypeEnum), value))
                        {
                            throw new Exception($"Invalid parameter data type detected.\r\nValue: {raw}. Expected values: 0 (String), 1 (Date), 2 (Numeric).");
                        }

                        d.DataType = ((DataTypeEnum)value).ToString();
                    }
                    if (dt.Columns.Contains(CN_ParamSQL)) d.ParamSQL = dt.Rows[i][CN_ParamSQL].ToString().Trim() ;
                    //if (dt.Columns.Contains(CN_ParamValue)) d.ParamValue = dt.Rows[i][CN_ParamValue].ToString().Trim();

                    lines.Add(d);
                }
            }
            return lines;

        }
        public static ft_RPT1 LoadLinesById(int id)
        {
            ft_RPT1 d = new ft_RPT1();
            string spName = "LoadRPT1ById_sp";

            DataTable dt = new DataTable();
            dt = DAC.ExecuteDataTable(spName,
                   DAC.Parameter(CN_Id, id));
            if (dt.Rows.Count > 0)
            {
                d.Id = int.Parse(dt.Rows[0][CN_Id].ToString().Trim());
                d.SKHdr = int.Parse(dt.Rows[0][CN_SKHdr].ToString().Trim());
                if (dt.Columns.Contains(CN_ParamName)) d.ParamName = dt.Rows[0][CN_ParamName].ToString().Trim();
                if (dt.Columns.Contains(CN_ParamDisplayName)) d.ParamDisplayName = dt.Rows[0][CN_ParamDisplayName].ToString().Trim();
                if (dt.Columns.Contains(CN_DataType)) d.DataType = dt.Rows[0][CN_DataType].ToString().Trim() == "0" ? "string" : "date";
                if (dt.Columns.Contains(CN_ParamSQL)) d.ParamSQL = dt.Rows[0][CN_ParamSQL].ToString().Trim();
                //if (dt.Columns.Contains(CN_ParamValue)) d.ParamValue = dt.Rows[i][CN_ParamValue].ToString().Trim();
            }
            return d;

        }
        #endregion
    }
}