using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ARB.AppValues
{
    public class clsAppVariables
    {
        // User 
        public long gblUserID { get; set; }
        public String gblUserCode { get; set; }
        public String gblUserName { get; set; }
        public String gblUserType { get; set; }

        public long gblUserIDDefaultBranch { get; set; }
        public String gblUserDefaulBranch { get; set; }
        public String gblUserDefaulBranchName { get; set; }

        // Financial year 

        public String gblFinancialCode { get; set; }
        public int gblIDFinancialYear { get; set; }
        public int gblFinancialYear { get; set; }
        public String gblFinancialDisplayCode { get; set; }


        // DateTime Format
        public String gblFormat { get; set; }
        public String gblSeperator { get; set; }
        public String gblDateFormat { get; set; }
        public String gblDateFormatforSave { get; set; }

        // Company info
        public int gblCompanyID { get; set; }
        public String gblCompanyName { get; set; }
        public String gblCompanyCode { get; set; }
        public String gblCompanyAddress { get; set; }
        public String gblCompanyAddress1 { get; set; }
        public String gblCompanyAddress2 { get; set; }
        public String gblCompanyState { get; set; }
        public String gblCompanyCountry { get; set; }
        public String gblCompanyPin { get; set; }
        public String gblCompanyBusinessType { get; set; }
        public String gblCompanyFax { get; set; }
        public String gblCompanyEmail { get; set; }
        public String gblCompanyPhoneNo { get; set; }
        public String gblCompanyWebsite { get; set; }
        public String gblCompanyStartDate { get; set; }
        public String gblCompanyEndDate { get; set; }
        public String gblCompanyDatabase { get; set; }
        public String gblCompanyConnection { get; set; }
        public String gblCompanyCIN { get; set; }
        public String gblCompanyGSTIN { get; set; }
        public String gblCompanyPANNO { get; set; }
        public String gblCompanyTANNO { get; set; }
        public String gblCompanyPTAXNO { get; set; }
        public String gblCompanyPFNO { get; set; }
        public String gblCompanyESICNO { get; set; }
        public String gblCompanyLogoPath { get; set; }
        public Decimal gblCompanyLogoHeight { get; set; }
        public Decimal gblCompanyLogoWidth { get; set; }
        public String gblCompanyDemoYN { get; set; }
        public String gblBOMHeaderName { get; set; }

        // Sale Invoice Document 
        public String gblCompanyType { get; set; }
        public int gblSaleInvoiceCopyNo { get; set; }

        public long gblCompanyDefaultWareHouse { get; set; }
        public String gblCompanyMultiWareHouseYN { get; set; }
        // Branch 
        public long gblBranchID { get; set; }
        public String gblBranchCode { get; set; }
        public String gblBranchName { get; set; }

        //public String gblCompanyCode { get; set; }
        //public String gblPassword { get; set; }

        //----Error----
        public String gblErrormsg { get; set; }
        public String gblBackpage { get; set; }

        // Master DB Connection string
        public string gblMasterConnection { get; set; }
        // Menu
        public DataTable gblUserMenu { get; set; }

        //Freeze data
        public DataTable gblFreezeMenu { get; set; }
        public DataTable gblFreezeDate { get; set; }


        //Grid PageSize
        public long glbPageSize { get; set; }
        public long gblshowMax { get; set; }

        //Inventory
        public String gblStockValuation { get; set; }

        public void fnClearValues()
        {
            gblUserID = 0;
            gblUserCode = "";
            gblUserName = "";
            gblUserType = "";

            // Financial year 
            gblFinancialCode = "";

            // DateTime Format
            gblFormat = "";
            gblSeperator = "";
            gblDateFormat = "";

            // Company info
            gblCompanyID = 0;
            gblCompanyName = "";
            gblCompanyCode = "";
            gblCompanyAddress = "";
            gblCompanyConnection = "";
            gblCompanyFax = "";
            gblCompanyEmail = "";
            gblCompanyWebsite = "";
            gblCompanyLogoPath = "";
            gblCompanyLogoHeight = 0;
            gblCompanyLogoWidth = 0;
            glbPageSize = 0;
        }
    }

}
