using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.sdk.sql;
using org.iringtools.utility;
using StaticDust.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;
using log4net;
using System.Xml;

namespace org.iringtools.sdk.MSAccess
{
    public class MSAccessDataLayer
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(MSAccessDataLayer));
        private DatabaseDictionary _MSdictionary = null;
        private DatabaseDictionary _filedirectory = null;
        private DatabaseDictionary _SQLdictionary = null;
        private SqlConnection _connSQL;


        public MSAccessDataLayer()
        {

            _filedirectory = Utility.Read<DatabaseDictionary>(@"./../MSAccessDataLayer/mdb2.xml");
            string FileConnectionString = (_filedirectory.ConnectionString);


            System.IO.File.Move(FileConnectionString+"2", FileConnectionString);

 


            _MSdictionary = Utility.Read<DatabaseDictionary>(@"./../MSAccessDataLayer/MSAccessSettings.xml");


            string ConnectionString = (_MSdictionary.ConnectionString);
            OleDbConnection connection = new OleDbConnection(ConnectionString);

            _SQLdictionary = Utility.Read<DatabaseDictionary>(@"./../MSAccessDataLayer/SampleDictionary.xml");

            string SQLConnectionString = (_SQLdictionary.ConnectionString);
            _connSQL = new SqlConnection(SQLConnectionString);

            connection.Open();

            System.Data.DataTable dt = null;

            dt = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            foreach (DataRow tablerow in dt.Rows)
            {
                string strSheetTableName = tablerow["TABLE_NAME"].ToString();

                DataTable cols = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strSheetTableName, null });


                StringBuilder sb = new StringBuilder();

                foreach (DataRow colRow in cols.Rows)
                {
                    string strTblColumn = colRow[2].ToString();
                    string strColumn = colRow[3].ToString();
                    string strColumnSize = colRow[13].ToString();

                    string vsSQL = "[" + strColumn + "] [varchar] (MAX) NOT NULL, ";

                    sb.Append(vsSQL);

                }


                string vInheritColumns = sb.ToString(0, sb.Length - 2);

                SqlCommand sqlcomm = new SqlCommand();
                _connSQL.Open();

                sqlcomm.Connection = _connSQL;
                sqlcomm.CommandText = "CREATE TABLE [dbo].[" + strSheetTableName + "] (" + vInheritColumns + ")";

                sqlcomm.ExecuteNonQuery();



                OleDbCommand command = new OleDbCommand();
                command.Connection = connection;
                command.CommandText = "Select * from " + strSheetTableName;
                OleDbDataReader tAccess = command.ExecuteReader();



                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_connSQL))
                {

                    bulkCopy.DestinationTableName = "dbo." + strSheetTableName;

                    try
                    {

                        bulkCopy.WriteToServer(tAccess);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                }

                _connSQL.Close();
            }

            
            connection.Close();

        }

        
    }
}