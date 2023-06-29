using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace MendinePayroll.UI.Controllers
{
    public class PStateListController : Controller
    {
        // GET: PStateList
        public ActionResult Index()
        {
            //string mainconn = ConfigurationManager.ConnectionStrings[""].ConnectionString;
            //SqlConnection sqlconn = new SqlConnection(mainconn);
            //SqlDataAdapter sqlda = new SqlDataAdapter("Usp_GetAllState", mainconn);
            //sqlda.SelectCommand.CommandType = CommandType.StoredProcedure;
            //sqlconn.Open(); 
            //DataSet ds = new DataSet();
            //sqlda.Fill(ds);
            //ViewBag.statename = ds.Tables[0];

            //List<SelectListItem> getstatename = new List<SelectListItem>();
            //foreach (DataRow dr in ViewBag.statename.Rows)
            //{
            //    getstatename.Add(new SelectListItem { Text = @dr["statename"].ToString(), Value = @dr["statename"].ToString() });
            //}
            //ViewBag.states = getstatename;
            //sqlconn.Close(); 
            if (Session["UserName"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
    }
}