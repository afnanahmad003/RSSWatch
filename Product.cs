using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using ApplicationLogger;
using RESSWATCH;

namespace BL
{
    public class Product
    {
        #region Methods
        public static bool Add(string ParentIDP, string ShopifyID, string VarientID, string VarientSKU)
        {
            bool result = false;
            try
            {                
                if (DataAccess.ExecuteNonQuery(CommandType.Text, "INSERT INTO [Product] ( [ParentID],[ShopifyID],[VarientID],[VarientSKU])VALUES ('"+ParentIDP+"','"+ShopifyID+"','"+VarientID+"','"+VarientSKU+"')", null) > 0)
                    result = true;
                else
                    result = false;
            }
            catch (Exception Exp)
            {
                Logger.WriteException(Exp);
            }
            return result;
        }
        
        public static DataTable Get_ProductByparentID(string ParentID)
        {
            DataTable dTResult = new DataTable();
            try
            {
                DataSet dSet = ((DataSet)DataAccess.ExecuteDataSet(CommandType.Text, "Select * from Product where parentid='"+ParentID+"'", null));
                dTResult = dSet.Tables[0];
            }
            catch (Exception Exp)
            {
                Logger.WriteException(Exp);
            }
            return dTResult;
        }

        public static DataTable GetVarientBySKU(string ParentID, string VarientSKU)
        {
            DataTable dTResult = new DataTable();
            try
            {
                DataSet dSet = ((DataSet)DataAccess.ExecuteDataSet(CommandType.Text, "Select * from Product where  VarientSKU='" + VarientSKU + "'", null));
                dTResult = dSet.Tables[0];
            }
            catch (Exception Exp)
            {
                Logger.WriteException(Exp);
            }
            return dTResult;
        }
        #endregion Methods
    }
}