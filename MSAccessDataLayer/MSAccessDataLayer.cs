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
        private DataDictionary _dataDictionary = null;

        OleDbConnection _conn = null;
        string FileConnectionString = string.Empty;
        string _baseDirectory = @"C:\Users\GDHAMIJA\Documents\GitHub\spr_dl\MSAccessTest.NUnit";
        string _xmlPath = @".\App_Data\";
        string _projectName = "SPR" ;
        string _applicationName = "1234";

        public MSAccessDataLayer()
        {

            _filedirectory = Utility.Read<DatabaseDictionary>(@"./../MSAccessDataLayer/mdb2.xml");
            FileConnectionString = (_filedirectory.ConnectionString);
            System.IO.File.Move(FileConnectionString + "2", FileConnectionString);
            GetDictionary(); // Dictionary Generation Code.

            //System.IO.File.Move(FileConnectionString+"2", FileConnectionString);

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
                DataTable tableColumns = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strSheetTableName, null });
                foreach (DataRow row in tableColumns.Rows)
                {
                    var columnNameColumn = row["COLUMN_NAME"];
                    var dateTypeColumn = row["DATA_TYPE"];
                    var ordinalPositionColumn = row["ORDINAL_POSITION"];

                }
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

        public  DataDictionary GetDictionary()
        {
            string Connectionstring = string.Empty;

            string path = String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName);
            try
            {
                if ((File.Exists(path)))
                {
                    dynamic DataDictionary = Utility.Read<DataDictionary>(path);
                    _dataDictionary = Utility.Read<DataDictionary>(path);
                    return _dataDictionary;
                }
                else
                {
                    List<string> tableNames = LoadDataTable();
                    _dataDictionary = LoadDataObjects(tableNames);

                    DatabaseDictionary _databaseDictionary = new DatabaseDictionary();
                    _databaseDictionary.dataObjects = _dataDictionary.dataObjects;
                    _databaseDictionary.ConnectionString = FileConnectionString;
                    _databaseDictionary.Provider = "MDB";
                    _databaseDictionary.SchemaName = "dbo";

                    Utility.Write<DatabaseDictionary>(_databaseDictionary, String.Format("{0}{1}DataBaseDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                    Utility.Write<DataDictionary>(_dataDictionary, String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                    return _dataDictionary;
                }
            }
            catch
            {
                string error = "Error in getting dictionary";
                logger.Error(error);
                throw new Exception(error);
            }
        }

        private List<string> LoadDataTable()
        {

            try
            {
                List<string> _dataTables = new List<string>();
                ConnectToExcel();
                DataTable dt = _conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                foreach (DataRow tablerow in dt.Rows)
                {
                    _dataTables.Add(tablerow["TABLE_NAME"].ToString());
                }
                return _dataTables;
            }
            catch (Exception ex)
            {
                logger.Info("Error while fetching table names:   " + ex.Message);
                throw ex;
            }
        }

        private DataDictionary LoadDataObjects(List<string> tableNames)
        {
            try
            {
                DataObjects _dataObjects = new DataObjects();
                DataObject _dataObject = new DataObject();
                KeyProperty _keyproperties = new KeyProperty();
                DataProperty _dataproperties = new DataProperty();
                DataDictionary dataDictionary = new DataDictionary();           

                foreach (string strname in tableNames)
                {
                    _dataObject = new DataObject();
                    _dataObject.objectName = strname.ToLower();
                    _dataObject.tableName = strname.ToLower();
                    _dataObject.keyDelimeter = "_";

                    ConnectToExcel();
                    DataTable tableColumns = _conn.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strname, null });
                    foreach (DataRow row in tableColumns.Rows)
                    {                     
                        _dataproperties = new DataProperty();
                        _dataproperties.columnName = row["COLUMN_NAME"].ToString();
                        _dataproperties.propertyName = row["COLUMN_NAME"].ToString();
                        if (!(row["CHARACTER_MAXIMUM_LENGTH"] is DBNull))
                            _dataproperties.dataLength = Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]);
                        else
                            _dataproperties.dataLength = 255;
                        _dataproperties.isNullable = Convert.ToBoolean(row["IS_NULLABLE"]);

                        if (_dataproperties.columnName.ToUpper() == "ID" || _dataproperties.columnName.ToUpper() == "TAG")
                        {
                            _keyproperties = new KeyProperty();
                            _keyproperties.keyPropertyName = _dataproperties.columnName;
                            _dataObject.keyProperties.Add(_keyproperties);
                            _dataproperties.keyType = KeyType.assigned;
                        }
                        else
                        {
                            _dataproperties.keyType = KeyType.unassigned;
                        }


                        switch (row["DATA_TYPE"].ToString())
                        {
                            case "11":
                                _dataproperties.dataType = DataType.Boolean;
                                break;
                            case "3":
                            case "2":
                                _dataproperties.dataType = DataType.Int32;
                                break;
                            case "7":
                                _dataproperties.dataType = DataType.DateTime;
                                break;
                            default:
                                _dataproperties.dataType = DataType.String;
                                break;
                        }
                        _dataObject.dataProperties.Add(_dataproperties);
                    }                    
                    dataDictionary.dataObjects.Add(_dataObject);
                }
                return dataDictionary;
            }
            catch (Exception ex)
            {
                logger.Error("Error in generating the dictionary.");
                throw ex;
            }
            finally
            {
             _conn.Close();
            }
        }

        private void ConnectToExcel()
        {
            if (_conn == null || _conn.State == ConnectionState.Closed)
            {
                _MSdictionary = Utility.Read<DatabaseDictionary>(@"./../MSAccessDataLayer/MSAccessSettings.xml");
                _conn = new OleDbConnection(_MSdictionary.ConnectionString);
                _conn.Open();
            }
        }
    }
}