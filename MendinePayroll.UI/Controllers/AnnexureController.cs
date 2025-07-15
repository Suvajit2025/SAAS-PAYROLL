using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using Newtonsoft.Json;
using MendinePayroll.UI.BLL;
using System.Globalization;
using MendinePayroll.UI.Models;
using Common.Utility;
using System.IO;

namespace MendinePayroll.UI.Controllers
{
    public class AnnexureController : Controller
    {
        public ActionResult Annexure92B()
        {
            return View();
        }
        [HttpGet]
        public JsonResult Annexure92BData(String Month, int Year, String EmployeeIDS)
        {
            DataTable dt = ShowTable();
            if (EmployeeIDS != "")
            {
                String[] ids = EmployeeIDS.Split(',');
                for (int index = 0; index <= ids.Length - 1; index++)
                {
                    DataRow dr = dt.NewRow();
                    dr["ID"] = clsHelper.fnConvert2Long(ids[index]);
                    dt.Rows.Add(dr);
                }
            }

            var Result = clsAnnexure.Annexure92B_Data(Month, Year, dt);
            return Json(Result, JsonRequestBehavior.AllowGet);
        }
        private DataTable ShowTable()
        {
            DataTable dt = new DataTable("Data");
            dt.Columns.Add("ID", typeof(Int32));
            return dt;
        }
        public JsonResult Annexure92BSave(List<clsAnnexure92BInfo> info)
        {
            var Result = clsAnnexure.Annexure92B_Save(info);
            return Json(new { success = Result }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult AllEmployeeList()
        {
            var employees = clsMaster.EmployeeList();
            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult YearList()
        {
            var years = clsSalary.SalaryYear();
            return Json(years, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult CompanyList()
        {
            var comp = clsSalary.CompanyList();
            return Json(comp, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult EmployeeList(String Companycode)
        {
            var emp = clsSalary.EmployeeListCompanyWise(Companycode);
            return Json(emp, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Form16List(String Companycode, int Year, int Employeeno)
        {
            var emp = clsSalary.Form16List(Year, Companycode, Employeeno);
            return Json(emp, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost]
        //public JsonResult Form16Upload(FormCollection frm)
        //{
        //    String Result = "";
        //    HttpPostedFileBase postedFileA = Request.Files[0];
        //    HttpPostedFileBase postedFileB = Request.Files[1];
        //    int year = clsHelper.fnConvert2Int(frm["year"]);
        //    String empno = frm["empno"].ToString();
        //    long idform = clsHelper.fnConvert2Long(frm["IDform"]);
        //    String filenameA = year.ToString() + "_" + empno + "_" + postedFileA.FileName;
        //    String filenameB = year.ToString() + "_" + empno + "_" + postedFileB.FileName;
        //    String pathA = Path.Combine(Server.MapPath("/Form16Data"), filenameA);
        //    String pathB = Path.Combine(Server.MapPath("/Form16Data"), filenameB);
        //    postedFileA.SaveAs(pathA);
        //    postedFileB.SaveAs(pathB);

        //    Result = clsSalary.Form16Upload(idform, year, empno, pathA, filenameA , pathB, filenameB);
        //    if (Result == "")
        //        return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
        //    else
        //    {
        //        return Json(new { error = Result }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        [HttpPost]
        public JsonResult Form16Upload(FormCollection frm) 
        {
            String Result = "";
            HttpPostedFileBase postedFileA = Request.Files[0];
            HttpPostedFileBase postedFileB = Request.Files[1];
            int year = clsHelper.fnConvert2Int(frm["year"]);
            String empno = frm["empno"].ToString();
            long idform = clsHelper.fnConvert2Long(frm["idform"]);
            string Companycode= frm["companycode"].ToString();
            //String filenameA = year.ToString() + "_" + empno + "_" + postedFileA.FileName;
            //String filenameB = year.ToString() + "_" + empno + "_" + postedFileB.FileName;

            String filenameA = postedFileA.FileName;
            String filenameB = postedFileB.FileName;

            String pathA = Path.Combine(Server.MapPath("/Form16Data"), filenameA);
            String pathB = Path.Combine(Server.MapPath("/Form16Data"), filenameB);
            postedFileA.SaveAs(pathA);
            postedFileB.SaveAs(pathB);
            Result = clsSalary.Form16Upload(idform, year, Companycode, empno, pathA, filenameA, pathB, filenameB);
            if (Result == "")
                return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { error = Result }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Form16()
        {
            return View();
        }

        //public ActionResult Form16UPLoad()
        //{
        //    return View();
        //}


        [HttpPost]
        public JsonResult allform16upload()
        {
            string folderPath = Server.MapPath("/Form16");
            string[] fileNames = Directory.GetFiles(folderPath);
            List<string> fileList = new List<string>();
            String Result = "";
            foreach (string fileName in fileNames)
            {
                string[] parts = Path.GetFileNameWithoutExtension(fileName).Split('_');
                string filesName = Path.GetFileName(fileName);
                string part = parts[1].ToString();
                string aadharNo = parts[0].ToString();
                int year = DateTime.Now.Year;
                String filepath = Path.Combine(Server.MapPath("/Form16Data"), filesName);

                Result = clsSalary.AllForm16Upload(aadharNo, filesName, part, year, filepath);
            }
            if (Result == "")
                return Json(new { success = "" }, JsonRequestBehavior.AllowGet);
            else
            {
                return Json(new { error = Result }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}