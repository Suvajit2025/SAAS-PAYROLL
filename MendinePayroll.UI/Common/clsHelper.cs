using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Security.Cryptography;
using System.IO;
using ARB.AppValues;
using System.Globalization;
using System.Collections;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Net;
using System.Threading;
using System.Reflection;

namespace Common.Utility
{
   public class clsHelper
    {
        public static void setPermissionVisualizer(MasterPage master, String addPermission, String editPermission, String deletePermission)
        {
            string defaultClass = "pull-right permission-bg m-r-5 ";
            string grantClass = "permission-granted";
            Button add = (Button)master.FindControl("btnPermissionAdd");
            Button edit = (Button)master.FindControl("btnPermissionEdit");
            Button delete = (Button)master.FindControl("btnPermissionDelete");
            add.CssClass = addPermission == "0" ? defaultClass : defaultClass + grantClass;
            edit.CssClass = editPermission == "0" ? defaultClass : defaultClass + grantClass;
            delete.CssClass = deletePermission == "0" ? defaultClass : defaultClass + grantClass;
        }

        public static void setReportPermissionVisualizer(MasterPage master, String viewPermission)
        {
            Control controls = (Control)master.FindControl("divReportControls");
            Control permission = (Control)master.FindControl("divReportPermission");
            if(viewPermission == "0")
            {
                controls.Visible = false;
                permission.Visible = true;
            }
            else
            {
                controls.Visible = true;
                permission.Visible = false;
            }
        }

        public static string ConvertAmountToINR(double dblAmount, bool boolUseShortFormat = false) //string strAmount
        {
            string strFormattedAmount = "";
            if (boolUseShortFormat == false)
            {
                strFormattedAmount = dblAmount.ToString("#,0.00", System.Globalization.CultureInfo.CreateSpecificCulture("hi-IN"));
            }
            else
            {
                string strAmt = "", strAmtPart1 = "", strAmtPart2 = "";
                double dblAmtPart1 = 0, dblAmtPart2 = 0;

                // Displays 123,45,67,890   
                if (dblAmount < 1000)
                    strFormattedAmount = dblAmount.ToString("#,0.00", System.Globalization.CultureInfo.CreateSpecificCulture("hi-IN"));

                // Displays 123,45,68K
                else if (dblAmount >= 1000 && dblAmount < 100000)
                    strFormattedAmount = dblAmount.ToString("#,#,K", System.Globalization.CultureInfo.CreateSpecificCulture("hi-IN"));//InvariantCulture

                // Displays 123,5L
                else if (dblAmount >= 100000 && dblAmount < 10000000)
                {
                    strAmt = dblAmount.ToString();
                    strAmtPart1 = strAmt.Substring(0, (strAmt.Length - 5));
                    strAmtPart2 = strAmt.Substring((strAmt.Length - 5), 5);

                    dblAmtPart1 = Convert.ToDouble(strAmtPart1);
                    dblAmtPart2 = Convert.ToDouble(strAmtPart2);

                    if (dblAmtPart2 > 55999)
                    {
                        dblAmtPart1 = dblAmtPart1 + 1;
                    }

                    strAmtPart1 = dblAmtPart1.ToString("#,#", System.Globalization.CultureInfo.CreateSpecificCulture("hi-IN"));

                    strFormattedAmount = strAmtPart1 + "L";
                }
                // Displays 123C
                else if (dblAmount >= 10000000)
                {
                    strAmt = dblAmount.ToString();
                    strAmtPart1 = strAmt.Substring(0, (strAmt.Length - 7));
                    strAmtPart2 = strAmt.Substring((strAmt.Length - 7), 7);

                    dblAmtPart1 = Convert.ToDouble(strAmtPart1);
                    dblAmtPart2 = Convert.ToDouble(strAmtPart2);

                    if (dblAmtPart2 > 5599999)
                    {
                        dblAmtPart1 = dblAmtPart1 + 1;
                    }

                    strAmtPart1 = dblAmtPart1.ToString("#,#", System.Globalization.CultureInfo.CreateSpecificCulture("hi-IN"));

                    strFormattedAmount = strAmtPart1 + "C";
                }
            }

            return strFormattedAmount;
        }

        public static DataTable fnDashboardIncExpReport(string pSDate, string pEDate, int IDFinYear,string BranchCode)
        {
            return clsDatabase.fnDataTable("PRC_Income_Expense", pSDate, pEDate, IDFinYear, BranchCode);
        }

        public static DataTable fnDashboardCustomerAgeing(int IDCustomer, string pSDate, string pEDate, long BranchID, int FinYearID, int AgeingCount, int AgeingPeriod)
        {
            return clsDatabase.fnDataTable("PRC_DSH_Customer_Ageing", IDCustomer, pSDate, pEDate, BranchID, FinYearID, AgeingCount, AgeingPeriod);
        }

        public static DataTable fnDashboardVendorAgeing(int IDVendor, string pSDate, string pEDate, long BranchID, int FinYearID, int AgeingCount, int AgeingPeriod)
        {
            return clsDatabase.fnDataTable("PRC_DSH_Vendor_Ageing", IDVendor, pSDate, pEDate, BranchID, FinYearID, AgeingCount, AgeingPeriod);
        }

        public static long fnConvert2Long(object pText)
        {
            try
            {
                if (pText == null || pText == String.Empty || pText == "{}")
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt32(pText);
                }
            }
            catch
            {
                return 0;
            }
        }
        public static int fnConvert2Int(object pText)
        {
            try
            {
                if (pText == null || pText == string.Empty || pText == "{}")
                {
                    return 0;
                }
                else
                {
                    return Convert.ToInt16(pText);
                }
            }
            catch
            {
                return 0;
            }
        }
        public static decimal fnConvert3Decimal(object pText)
        {
            try
            {
                if (pText == null || pText == "" || pText == "{}")
                {
                    return 0;
                }
                else
                {
                    //return Convert.ToDecimal(pText).ToString("0.00");
                    return Convert.ToDecimal(Convert.ToDecimal(pText).ToString("0.000"));
                }
            }
            catch
            {
                return 0;
            }
        }
        public static decimal fnConvert2Decimal(object pText)
        {
            try
            {
                if (pText == null || pText == "" || pText == "{}")
                {
                    return 0;
                }
                else
                {
                    //return Convert.ToDecimal(pText).ToString("0.00");
                    return Convert.ToDecimal(Convert.ToDecimal(pText).ToString("0.00"));
                }
            }
            catch
            {
                return 0;
            }
        }
        public static double fnConvert2Double(object pText)
        {
            try
            {
                if (pText == null || pText.ToString() == "" || pText.ToString() == "{}")
                {
                    return 0;
                }
                else
                {
                    //return Convert.ToDecimal(pText).ToString("0.00");
                    return Convert.ToDouble(pText);
                }
            }
            catch
            {
                return 0;
            }
        }
        public static string fnConvert2String(String pText)
        {
            try
            {
                if (String.IsNullOrEmpty(pText))
                {
                    return "";
                }
                else
                {
                    return Convert.ToString(pText);
                }
            }
            catch
            {
                return "";
            }
        }
        public static String fnConvert2PascalWithSpace(String pText)
        {
            String mText = pText;
            if (mText != "")
            {
                mText = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(mText.ToLower());
            }
            return mText;
        }
        public static String fnConvert2StartCapital(String pText)
        {
            String mText = pText;
            if (mText != "")
            {
                return char.ToUpper(mText[0]) + pText.Substring(1).ToLower();
            }
            return mText;
        }
        public static bool fnIsNumeric(string pText)
        {
            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(pText);
        }
        public static string fnAutoNumber()
        {
            String mAutoNo = System.DateTime.Now.ToString("ddmmyy");
            return mAutoNo;
        }
        public static String fnAutoNumber(int pdigit)
        {
            Random generator = new Random();
            String mAutoNo = Convert.ToString(generator.Next(0, 1000000).ToString("D6"));
            return mAutoNo;
        }
        public static String fnAutoNumber8()
        {
            Random generator = new Random();
            String mAutoNo = Convert.ToString(generator.Next(0, 100000000).ToString("D8"));
            return mAutoNo;
        }
        public static void fnShowMessageBoxSessionOut(Page Page, Type Type)
        {
        }
        public static void fnShowMessageBox(Page Page, Type Type, string msg)
        {
            ScriptManager.RegisterStartupScript(Page, Type, "temp", "<script type='text/javascript'>alert('" + msg + "');</script>", false);
        }
        public static void fnCheckUser(String pSessionVariable)
        {
            if (System.Web.HttpContext.Current.Session[pSessionVariable] == null)
            {
                String mLoginPagePath = System.Configuration.ConfigurationManager.AppSettings["LoginPage"].ToString();
                System.Web.HttpContext.Current.Response.Redirect(mLoginPagePath);
            }
        }
        public static void fnShowErrorPage(String pDetailError)
        {

        }
        public static string encrypt(string encryptString)
        {
            string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ@#$%";
            byte[] clearBytes = Encoding.Unicode.GetBytes(encryptString);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
            0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 , 0x20, 0x4d, 0x49, 0x76
             });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    encryptString = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encryptString;
        }
        public static DataTable fnSearch(String TableName,String FieldName, String SearchText, long IDBranch, long IDFinyear)
        {
            return clsDatabase.fnDataTable("PRC_Search_On_Table", TableName,FieldName, SearchText, IDBranch, IDFinyear);
        }

        public static string DecryptBySql(string id)
        {
            SqlConnection mCon = new SqlConnection(HttpContext.Current.Session["Connection"].ToString());

            string DecryptID = "";
            try
            {
                mCon.Open();
                SqlCommand cmd = new SqlCommand("select [dbo].[fnDecrypt](0x" + id + ")", mCon);
                cmd.CommandType = CommandType.Text;
                DecryptID = cmd.ExecuteScalar().ToString();
                return DecryptID;
            }
            catch (Exception ex)
            {
                DecryptID = ex.Message.ToString();
                return DecryptID;
            }
            finally { mCon.Close(); }
        }
        public static string SplitQueryString(string QueryString)
        {

            string strReq = "";
            try
            {

                strReq = QueryString;
                strReq = strReq.Substring(strReq.IndexOf('?') + 1);

                return strReq;
            }
            catch (Exception ex)
            {
                strReq = ex.Message.ToString();
                return strReq;
            }
            finally { }
        }

        public static string SplitQueryString(string QueryString, char SplitType)
        {

            string strReq = "";
            try
            {

                strReq = QueryString;
                strReq = strReq.Substring(strReq.IndexOf(SplitType) + 1);

                return strReq;
            }
            catch (Exception ex)
            {
                strReq = ex.Message.ToString();
                return strReq;
            }
            finally { }
        }

        public static string DecryptBySqlString(string Value)
        {
            SqlConnection mCon = new SqlConnection(HttpContext.Current.Session["Connection"].ToString());

            string DecryptID = "";
            try
            {
                mCon.Open();
                SqlCommand cmd = new SqlCommand("select [dbo].[fnDecryptString](0x" + Value + ")", mCon);
                cmd.CommandType = CommandType.Text;
                DecryptID = cmd.ExecuteScalar().ToString();
                return DecryptID;
            }
            catch (Exception ex)
            {
                DecryptID = ex.Message.ToString();
                return DecryptID;
            }
            finally { mCon.Close(); }
        }

        public static string Decrypt(string cipherText)
        {
            string EncryptionKey = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ@#$%";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] {
                 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76, 0x20, 0x4d, 0x49, 0x76
            });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
        public static Boolean fnConvert2Boolean(String pText)
        {
            try
            {
                if (String.IsNullOrEmpty(pText))
                {
                    return false;
                }
                else
                {
                    return Convert.ToBoolean(pText);
                }
            }
            catch
            {
                return false;
            }
        }
        public Boolean fnValidEmail(String pEmail)
        {
            String mPattern = "^[a-zA-Z0-9][-\\._a-zA-Z0-9]*@[a-zA-Z0-9][-\\.a-zA-Z0-9]*\\.(com|edu|info|gov|int|mil|net|org|biz|name|museum|coop|aero|pro|tv|[a-zA-Z]{2})$";
            System.Text.RegularExpressions.Regex mCheck = new System.Text.RegularExpressions.Regex(mPattern, RegexOptions.IgnorePatternWhitespace);
            Boolean mValid = false;
            if (String.IsNullOrEmpty(pEmail) == true)
            {
                mValid = true;
            }
            else
            {
                mValid = mCheck.IsMatch(pEmail);
            }
            return mValid;
        }
        public static String fnConvert2DateTimeCommon(String pText, String Inputtype)
        {
            // String iFormat=Master.gblDateFormat;
            //// String oFormat = "dd-MMM-yyyy";//System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.ToString();// IIS Server date format 
            //   try
            //   {
            //       if (String.IsNullOrEmpty(pText))
            //       {
            //           return "";
            //       }
            //       else
            //       {
            //        //   string ptextnew = pText.Substring(0, 11);
            //           string datetime = "";
            //           if (Inputtype.ToUpper() == "INPUT")
            //           {
            //                //datetime = DateTime.ParseExact(ptextnew, iFormat, CultureInfo.InvariantCulture).ToString(oFormat, CultureInfo.InvariantCulture);  
            //               datetime = Convert.ToDateTime(pText).ToString("dd/MMM/yyyy"); 
            //           }
            //           else if (Inputtype.ToUpper() == "OUTPUT")
            //           {
            //              // string stdate = DateTime.Parse(ptextnew.Trim()).ToString("dd-MMM-yyyy");
            //               datetime = Convert.ToDateTime(pText).ToString("dd/MM/yyyy"); 
            //                   //DateTime.ParseExact(stdate, oFormat, CultureInfo.InvariantCulture).ToString(iFormat, CultureInfo.InvariantCulture);                    
            //           }
            //           return datetime;
            //       }               
            //   }
            //   catch
            //   {               
            //       return "";
            //   }
            return "";
        }
        public static String FnDecimalformeted(String Dtext)
        {

            string str;
            if (String.IsNullOrEmpty(Dtext))
            {
                str = "0.00";
            }
            else
            {
                str = Convert.ToDecimal(Dtext).ToString(); //string.Format(System.Globalization.CultureInfo.GetCultureInfo("id-ID"), "{0:#,##0.00}", double.Parse(Dtext));
                //string.Format("{0:#,##0.00}", double.Parse(Dtext)); 
                //String.Format("{0:0,0.00}", clsHelper.fnConvert2Decimal(Dtext)).ToString();

            }

            return str;
        }
        public static decimal[] fnRoundoff(decimal netamt)
        {
            decimal[] myarray = new decimal[2];
            decimal roundoff = 0;
            decimal double_result = (decimal)((netamt - (decimal)((long)netamt)));
            if (double_result != 0)
            {
                if (double_result >= (decimal)(0.5))
                {
                    roundoff = (1 - double_result);
                }
                else
                {
                    roundoff = -(double_result);
                }
            }
            else
            {
                roundoff = 0;
            }
            myarray[0] = roundoff;
            myarray[1] = (netamt + roundoff);
            return myarray;
        }
        public static decimal fnceilingFloor(Decimal decvalue)
        {
            decimal trnvalue = 0;
            int mint = (int)(decvalue);
            float floatpart = ((float)decvalue - mint);
            if (floatpart >= 0.5)
            {
                trnvalue = Math.Ceiling(decvalue);
            }
            else
            {
                trnvalue = Math.Floor(decvalue);
            }

            return trnvalue;
        }
        public static void fnBtnShowHide(Page Page, Type Type, string ButtonList)
        {
            ScriptManager.RegisterStartupScript(Page, Type, "tmp", "<script type='text/javascript'>JqBtnFunction('" + ButtonList + "');</script>", false);
        }

        public static void fnErrorLog(String Detail)
        {
            String path = HttpContext.Current.Server.MapPath("~\\CompanyData\\ErrorLog\\ErrorLog.txt");
            String mError = "----------START-------- - "+
                            "Date & Time : " + DateTime.Now + Environment.NewLine +
                            "Message :  " + Detail + Environment.NewLine +
                            "----------END--------- " + Environment.NewLine;
            try
            {
                if (File.Exists(path) == false)
                {
                    File.Create(path);
                }
                File.WriteAllText(path, mError);
            }
            catch (Exception) { }
            finally { }
        }




        //For Admin 12.01.2019
        public static String fnAutoNumber4()
        {
            Random generator = new Random();
            String mAutoNo = Convert.ToString(generator.Next(0, 1000000).ToString("D6"));
            return mAutoNo;
        }
        public static void fnPageClose(Page Page, Type Type)
        {
            ScriptManager.RegisterStartupScript(Page, Type, "PageClose", "<script type='text/javascript'>window.close();</script>", false);
        }
        public static string fnSendEmail(string SMTPServer, Boolean EnableSSL_TrueFalse, string Username, string Password, int SMTP_Port, string mfromEmail, string mToEmail, string mSubject, string mBody)
        {
            string EmailStatus = "";
            try
            {
                using (MailMessage mm = new MailMessage())
                {
                    mm.From = new MailAddress(mfromEmail);
                    mm.To.Add(mToEmail);
                    mm.Subject = mSubject;
                    mm.Body = mBody;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = SMTPServer;
                    smtp.EnableSsl = EnableSSL_TrueFalse;
                    NetworkCredential NetworkCred = new NetworkCredential(Username, Password);

                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = SMTP_Port;
                    smtp.Send(mm);
                    EmailStatus = "";
                    return EmailStatus;
                }
            }
            catch (Exception ex)
            {
                EmailStatus = ex.Message.ToString();
                return EmailStatus;
            }
            finally { }
        }
        public static void ExportGridToExcel(string mFileName,GridView mGridView)
        {
            try
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.ClearContent();
                HttpContext.Current.Response.ClearHeaders();
                HttpContext.Current.Response.Charset = "";
                string FileName = mFileName + "_DummyFormat.xls";
                StringWriter strwritter = new StringWriter();
                HtmlTextWriter htmltextwrtter = new HtmlTextWriter(strwritter);
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
                HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName);
                mGridView.GridLines = GridLines.Both;
                mGridView.HeaderStyle.Font.Bold = true;
                mGridView.RenderControl(htmltextwrtter);
                HttpContext.Current.Response.Write(strwritter.ToString());
                HttpContext.Current.Response.End();
            }
            catch (ThreadAbortException ex) { }
        }

       
        public static string EmailHelper(String calledFrom, String smtpServer, int smtpPort, bool SSL, String password, String fromEmail, String toEmail, String emailSubject, String emailBody, String attachmentPath, String attachmentName)
        {
            string error = "";
            try
            {
                NetworkCredential login = new NetworkCredential(fromEmail, password);
                SmtpClient smtp = new SmtpClient(smtpServer);
                smtp.Port = smtpPort;
                smtp.EnableSsl = SSL;
                smtp.Credentials = login;

                MailMessage msg = new MailMessage { From = new MailAddress(fromEmail) };
                msg.To.Add(new MailAddress(toEmail));
                msg.Subject = emailSubject;
                msg.Body = emailBody;

                System.Net.Mail.Attachment attachment;
                String Path = HttpContext.Current.Server.MapPath(attachmentPath);
                attachment = new System.Net.Mail.Attachment(Path);
                msg.Attachments.Add(attachment);

                msg.IsBodyHtml = false;
                msg.Priority = MailPriority.Normal;
                msg.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                smtp.Send(msg);
                error = "";
            }
            catch (Exception ex)
            {
                error = "Issue";
                clsDatabase.fnErrorLog(calledFrom, ex.Message.ToString());
            }
            return error;
        }
        public static DataTable fnToDataTable<T>(List<T> items, string[] redundantColumns = null)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            if(redundantColumns != null)
            {
                foreach (string rc in redundantColumns)
                {
                    dataTable.Columns.Remove(rc);
                }
            }
            return dataTable;
        }
        public static List<T> fnDataTableToList<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItemFromDataTable<T>(row);
                data.Add(item);
            }
            return data;
        }

        #region Private Methods
        private static T GetItemFromDataTable<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }
        #endregion
    }


    public class clsDatabase
    {
        public static void fnErrorLog(String SPName, String Result)
        {
            String path = System.Web.Hosting.HostingEnvironment.MapPath("~\\CompanyData\\ErrorLog\\ErrorLog.txt");
            //String path = HttpContext.Current.Server.MapPath("~\\CompanyData\\ErrorLog\\ErrorLog.txt");
            String mError = "Date & Time : " + DateTime.Now + Environment.NewLine +
                            "SP Name :  " + SPName + Environment.NewLine +
                            "Message :  " + Result + Environment.NewLine +
                            "----------END--------- " + Environment.NewLine;
            try
            {
                if (File.Exists(path)== false)
                {
                    File.Create(path);
                }
                File.WriteAllText(path, mError); 
            }
            catch (Exception) { }
            finally { }
        }
        public static String fnDBOperation(String pSPName, params object[] pParaValue)
        {
            String mConnection = ConfigurationManager.ConnectionStrings["Admin"].ToString();
            SqlConnection con = new SqlConnection(mConnection);
            string mResult = "";
            try
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(pSPName, con);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(cmd);
                cmd.Parameters.RemoveAt(0);
                SqlParameter[] pParaName = new SqlParameter[cmd.Parameters.Count];
                cmd.Parameters.CopyTo(pParaName, 0);
                cmd.Parameters.Clear();
                mResult = fnAddParaValue(cmd, pParaName, pParaValue);
                if (mResult == "" ) 
                {
                    return Convert.ToString(cmd.ExecuteScalar());
                }
                else
                {
                    return mResult;
                }
            }
            catch (Exception e)
            {
                mResult = fnError(e); // Error Message
                fnErrorLog(pSPName, mResult); // Error Logging 
                return mResult;
            }
            finally
            {
                con.Close();
            }
        }
        public static DataTable fnDataTable(string pSPName, params object[] pParaValue)
        {
            String mConnection = ConfigurationManager.ConnectionStrings["Admin"].ToString();
            SqlConnection mCon = new SqlConnection(mConnection);
            DataTable DT = new DataTable("Data");
            String mResult = "";
            try
            {
                mCon.Open();
                SqlCommand mCom = new SqlCommand(pSPName, mCon);
                mCom.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(mCom);
                mCom.Parameters.RemoveAt(0);
                SqlParameter[] pParaName = new SqlParameter[mCom.Parameters.Count];
                mCom.Parameters.CopyTo(pParaName, 0);
                mCom.Parameters.Clear();
                if ((pParaName.Length > 0))
                {
                    for (int index = 0; index <= pParaName.Length - 1; index++)
                    {
                        mCom.Parameters.AddWithValue(pParaName[index].ParameterName, pParaValue[index]);
                    }
                }
                DT.Load(mCom.ExecuteReader());
                return DT;
            }
            catch (Exception e)
            {
                mResult = fnError(e);
                return DT;
            }
            finally
            {
                fnErrorLog(pSPName, mResult);
                mCon.Close();
            }
        }
        public static DataSet fnDataSet(string pSPName, params object[] pParaValue)
        {
            String mConnection = ConfigurationManager.ConnectionStrings["Admin"].ToString();
            SqlConnection mCon = new SqlConnection(mConnection);
            DataSet DS = new DataSet("Data");
            String Result = "";
            try
            {
                mCon.Open();
                SqlCommand mCom = new SqlCommand(pSPName, mCon);
                mCom.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(mCom);
                mCom.Parameters.RemoveAt(0);
                SqlParameter[] pParaName = new SqlParameter[mCom.Parameters.Count];
                mCom.Parameters.CopyTo(pParaName, 0);
                mCom.Parameters.Clear();
                if ((pParaName.Length > 0))
                {
                    for (int index = 0; index <= pParaName.Length - 1; index++)
                    {
                        mCom.Parameters.AddWithValue(pParaName[index].ParameterName, pParaValue[index]);
                    }
                }
                SqlDataAdapter DAP = new SqlDataAdapter(mCom);
                DAP.Fill(DS);
                return DS;
            }
            catch (Exception e)
            {
                Result = fnError(e);
                return DS;
            }
            finally
            {
                fnErrorLog(pSPName, Result);
                mCon.Close();
            }
        }
        public static DataTable fnDataTable(string pSPName)
        {
            String mConnection = ConfigurationManager.ConnectionStrings["Admin"].ToString();
            SqlConnection mCon = new SqlConnection(mConnection);
            DataTable DT = new DataTable("Data");
            String mResult = "";
            try
            {
                mCon.Open();
                SqlCommand mCom = new SqlCommand(pSPName, mCon);
                mCom.CommandType = CommandType.StoredProcedure;
                DT.Load(mCom.ExecuteReader());
                return DT;
            }
            catch (Exception e)
            {
                mResult = fnError(e);
                return DT;
            }
            finally
            {
                fnErrorLog(pSPName, mResult);
                mCon.Close();
            }
        }
        public static string fnAddParaValue(SqlCommand pCom, SqlParameter[] pParaName, object[] pParaValue)
        {
            if ((pCom == null))
            {
                return "SQL Command initialization issue...";
            }
            if (((pParaName == null) | (pParaValue == null)))
            {
                return "SQL Command Parameter initialization issue...";
            }
            if ((pParaName.Length != pParaValue.Length))
            {
                return "SQL Command Parameter length size issue...";
            }
            for (int index = 0; index <= pParaName.Length - 1; index++)
            {

                pCom.Parameters.AddWithValue(pParaName[index].ParameterName, pParaValue[index]);
            }
            return "";
        }
        private static string fnSQLError(SqlException pError)
        {
            string mMessage = "";
            mMessage += "Error No : " + pError.Number.ToString() + Environment.NewLine;
            mMessage += "Error Line : " + pError.LineNumber.ToString() + Environment.NewLine;
            mMessage += "Error Proc Name : " + pError.Procedure + Environment.NewLine;
            mMessage += "Error Message : " + pError.Message;
            return mMessage;
        }
        public static string fnError(Exception pError)
        {
            String mMessage = "";
            mMessage += "Error Message : " + pError.Message.ToString() + Environment.NewLine;
            return mMessage;
        }
    }
}
