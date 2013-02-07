using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.iringtools.library;
using org.iringtools.adapter;
using org.iringtools.utility;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using log4net;
using System.Data.OleDb;
using System.IO;


namespace org.iringtools.sdk.sql
{
    public class SQLDataLayer : BaseSQLDataLayer
    {
        private SqlDataAdapter _adapter = null;
        private SqlCommandBuilder _command = null;

        private string _applicationName = string.Empty;
        private string _projectName = string.Empty;
        private string _xmlPath = string.Empty;
        private string _baseDirectory = string.Empty;
        private DatabaseDictionary _dictionary = null;
        private DataDictionary _dataDictionary = null;

        private string _appConfigXML;
        public OleDbConnection _connOledb = null;
        public SqlConnection _conn = null;
        private string _mdbConnectionString = string.Empty;
        private string _dbConnectionString = string.Empty;
        private string _mdbFileName = string.Empty;
        private string _providerName;
        private StaticDust.Configuration.AppSettingsReader _sprSettings;
        private static readonly ILog logger = LogManager.GetLogger(typeof(SQLDataLayer));
        private int Spool_Index = 0;
        private Dictionary<int, string> _newProperties = new Dictionary<int, string>();
        private bool IsSpoolPropertyAdded = false;

        public SQLDataLayer(AdapterSettings settings)
            : base(settings)
        {
            _settings = settings;
            _xmlPath = _settings["xmlPath"];
            _projectName = _settings["projectName"];
            _applicationName = _settings["applicationName"];
            _baseDirectory = Directory.GetCurrentDirectory();
            _appConfigXML = String.Format("{0}{1}.{2}.config", _xmlPath, _projectName, _applicationName);
            _sprSettings = new StaticDust.Configuration.AppSettingsReader(_appConfigXML);

            _mdbFileName = String.Format("{0}{1}{2}", _baseDirectory, _xmlPath, _sprSettings["mdbfilename"].ToString());

            _appConfigXML = String.Format("{0}{1}.{2}.config", _xmlPath, _projectName, _applicationName);
            _sprSettings = new StaticDust.Configuration.AppSettingsReader(_appConfigXML);
            _dbConnectionString = EncryptionUtility.Decrypt(_sprSettings["dbconnection"].ToString());
            _providerName = _sprSettings["mdbprovider"].ToString();
            _mdbConnectionString = String.Format("Provider={0};Data Source={1}", _providerName, _mdbFileName);

            //Response response = ReverseRefresh();  //calling reverse refresh
        }

        public override DatabaseDictionary GetDatabaseDictionary()
        {
            _dictionary = Utility.Read<DatabaseDictionary>(String.Format("{0}{1}DataBaseDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
            string connStr = EncryptionUtility.Decrypt(_dictionary.ConnectionString);
            _conn = new SqlConnection(connStr);
            
            return _dictionary;
        }

        public override DataDictionary GetDictionary()
        {
            try
            {
                string path = String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName);

                if ((File.Exists(path)))
                {
                    dynamic DataDictionary = Utility.Read<DataDictionary>(path);
                    _dataDictionary = Utility.Read<DataDictionary>(path);
                    return _dataDictionary;
                }

                DataDictionary dataDictionary = new DataDictionary();
                string configPath = String.Format("{0}Configuration.{1}.{2}.xml", _baseDirectory + _xmlPath, _projectName, _applicationName);
                DataDictionary configDictionary = File.Exists(configPath) ? Utility.Read<DataDictionary>(configPath) : null;

                if (configDictionary != null)
                {
                    dataDictionary.dataObjects = configDictionary.dataObjects;

                    _dataDictionary = dataDictionary;

                    DatabaseDictionary _databaseDictionary = new DatabaseDictionary();
                    _databaseDictionary.dataObjects = _dataDictionary.dataObjects;
                    _databaseDictionary.ConnectionString = EncryptionUtility.Encrypt(_dbConnectionString);
                    _databaseDictionary.Provider = "MDB";
                    _databaseDictionary.SchemaName = "dbo";

                    Utility.Write<DatabaseDictionary>(_databaseDictionary, String.Format("{0}{1}DataBaseDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                    Utility.Write<DataDictionary>(_dataDictionary, String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                }

                return _dataDictionary;

                //string Connectionstring = string.Empty;

                //string path = String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName);
                //try
                //{
                //    if ((File.Exists(path)))
                //    {
                //        dynamic DataDictionary = Utility.Read<DataDictionary>(path);
                //        _dataDictionary = Utility.Read<DataDictionary>(path);
                //        return _dataDictionary;
                //    }
                //    else
                //    {
                //        List<string> tableNames = LoadDataTable();
                //        _dataDictionary = LoadDataObjects(tableNames);

                //        DatabaseDictionary _databaseDictionary = new DatabaseDictionary();
                //        _databaseDictionary.dataObjects = _dataDictionary.dataObjects;
                //        _databaseDictionary.ConnectionString = _dbConnectionString;
                //        _databaseDictionary.Provider = "MDB";
                //        _databaseDictionary.SchemaName = "dbo";

                //        Utility.Write<DatabaseDictionary>(_databaseDictionary, String.Format("{0}{1}DataBaseDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                //        Utility.Write<DataDictionary>(_dataDictionary, String.Format("{0}{1}DataDictionary.{2}.{3}.xml", _baseDirectory, _xmlPath, _projectName, _applicationName));
                //        return _dataDictionary;
                //    }
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
                ConnectToAccess();
                DataTable dt = _connOledb.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
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
                ConnectToAccess();

                foreach (string strname in tableNames)
                {
                    _dataObject = new DataObject();
                    _dataObject.objectName = strname.ToLower();
                    _dataObject.tableName = strname.ToLower();
                    _dataObject.keyDelimeter = "_";

                    DataTable colKey = _connOledb.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new Object[] { null, null, strname });
                    DataView view = colKey.DefaultView;
                    view.Sort = "ORDINAL";
                    colKey = view.ToTable();
                    List<string> tableKeys = new List<string>();
                    foreach (DataRow row in colKey.Rows)
                    {
                        tableKeys.Add(row["COLUMN_NAME"].ToString());
                    }

                    DataTable tableColumns = _connOledb.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strname, null });
                    foreach (DataRow row in tableColumns.Rows)
                    {
                        _dataproperties = new DataProperty();
                        _dataproperties.columnName = row["COLUMN_NAME"].ToString();
                        _dataproperties.propertyName = row["COLUMN_NAME"].ToString();

                        if (!(row["CHARACTER_MAXIMUM_LENGTH"] is DBNull))
                        {
                            _dataproperties.dataLength = Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"]);
                            if(_dataproperties.dataLength == 0)
                                _dataproperties.dataLength = 8000; // Memo Stores up to 8000 characters
                        }
                        else
                            _dataproperties.dataLength = 50; 

                        _dataproperties.isNullable = Convert.ToBoolean(row["IS_NULLABLE"]);

                        if (tableKeys.Contains(_dataproperties.columnName))
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
                            case "2":
                            case "3":
                            case "4":
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
                _connOledb.Close();
            }
        }

        public override DataTable GetDataTable(string tableName, IList<string> identifiers)
        {
            string query = string.Empty;
            DataSet dataSet = new DataSet();
            string delimiter = string.Empty;
            StringBuilder ids = new StringBuilder();
            StringBuilder qry = new StringBuilder();
            string qrySeparator = "";
            string separator = "";
            IList<string> keyProperties = new List<string>();
            DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
            keyProperties = (from p in dataObject.keyProperties
                             select p.keyPropertyName).ToList<string>();
            string tempQry = string.Empty;

            int i = 0;
            if (keyProperties.Count > 1)
            {
                delimiter = dataObject.keyDelimeter;

                foreach (string prop in keyProperties)
                {

                    tempQry += prop + " in (";
                    foreach (string identifier in identifiers)
                    {
                        string[] idArray = null;
                        if (identifier.Contains(delimiter.FirstOrDefault()))
                        {
                            idArray = identifier.Split(delimiter.FirstOrDefault());
                            ids.Append(separator + "'" + idArray[i] + "'");
                            separator = ",";
                        }
                    }
                    qry.Append(tempQry + ids + ")");
                    i++;
                    if (i < keyProperties.Count)
                    {
                        qrySeparator = " and ";
                    }
                    else
                        qrySeparator = "";
                    tempQry = qry.ToString() + qrySeparator;
                    ids.Clear();
                    separator = "";
                    qry.Clear();
                }
            }
            else
            {
                StringBuilder idString = new StringBuilder();
                string diff = "";
                foreach (string identifier in identifiers)
                {
                    idString.Append(diff + "'" + identifier + "'");
                    diff = ",";

                }
                tempQry = keyProperties[0] + " in (" + idString + ")";

            }
            try
            {
                query = "SELECT * FROM " + tableName + " where " + tempQry;
                ConnectToSqL();
                _adapter = new SqlDataAdapter();
                _adapter.SelectCommand = new SqlCommand(query, _conn);

                _command = new SqlCommandBuilder(_adapter);
                //_adapter.UpdateCommand = _command.GetUpdateCommand();
                _adapter.SelectCommand.ExecuteNonQuery();
                _adapter.Fill(dataSet, tableName);
                DataTable dataTable = dataSet.Tables[tableName];
                return dataTable;
            }
            catch (Exception ex)
            {
                logger.Info("Error while retrieving the data:   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
            }
        }

        public override DataTable GetDataTable(string tableName, string whereClause, long start, long limit)
        {
            List<string> keys = (from p in _dictionary.dataObjects
                                 where p.objectName == tableName
                                 select p.keyProperties.FirstOrDefault().keyPropertyName).ToList();

            string key = keys[0];
            string query = string.Empty;

            if (string.IsNullOrEmpty(whereClause))
            {
                query = "select * from (SELECT *, ROW_NUMBER() OVER (order by " + key + ") AS RN FROM " + tableName +
                        ") As " + tableName + " where RN >" + start + " and " + "RN <=" +(start + limit);
            
            }
            else
            {
                query = "select * from (SELECT *, ROW_NUMBER() OVER (order by " + key + ") AS RN FROM " + tableName +
                        ") As " + tableName + whereClause + " and RN >" + start + " and " + "RN <=" + (start + limit); 
            }

            try
            {
                ConnectToSqL();
                _adapter = new SqlDataAdapter();
                _adapter.SelectCommand = new SqlCommand(query, _conn);

                _command = new SqlCommandBuilder(_adapter);
                _adapter.SelectCommand.ExecuteNonQuery();
                DataSet dataSet = new DataSet();
                _adapter.Fill(dataSet, tableName);
                return dataSet.Tables[tableName];
            }
            catch (Exception ex)
            {
                logger.Info("Error while retrieving the data:   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
            }
        }

        public override Response PostDataTables(IList<DataTable> dataTables)
        {
            Response response = new Response();
            string status = string.Empty;
            try
            {
                // Updating SQL
                ConnectToSqL();
                string tableName = dataTables.First().TableName;
                string query = "SELECT * FROM " + tableName;
                _adapter = new SqlDataAdapter();
                _adapter.SelectCommand = new SqlCommand(query, _conn);

                _command = new SqlCommandBuilder(_adapter);
                _adapter.UpdateCommand = _command.GetUpdateCommand();

                DataSet dataSet = new DataSet();
                foreach (DataTable dataTable in dataTables)
                {
                    DataTable dt = Utility.CloneSerializableObject<DataTable>(dataTable);
                    dataSet.Tables.Add(dt);
                }

                _adapter.Update(dataSet, tableName);

                //Spool Properties needs to be added once.
                if (!IsSpoolPropertyAdded)
                {
                    AddSpoolProperties();
                }

                //For each row in proxy table update Label,Label name and Label Values.
                DataTable datatable = new DataTable();
                datatable = dataTables[0];
                foreach (DataRow row in datatable.Rows)
                {
                    UpdateOnEveryPost(row);
                }


                // Don't update Access here as Spool is only in SQL not in Access.
                /*
                // Updating Access
                ConnectToAccess();
                OleDbDataAdapter Oledbadapter = new OleDbDataAdapter();
                Oledbadapter.SelectCommand = new OleDbCommand(query, _connOledb);

                OleDbCommandBuilder OleDbcommand = new OleDbCommandBuilder(Oledbadapter);
                Oledbadapter.UpdateCommand = OleDbcommand.GetUpdateCommand();

                DataSet OledbdataSet = new DataSet();
                foreach (DataTable dataTable in dataTables)
                {
                    DataTable dt = Utility.CloneSerializableObject<DataTable>(dataTable);
                    OledbdataSet.Tables.Add(dt);
                }

                int i = Oledbadapter.Update(OledbdataSet, tableName);
                */
                status = "success";
            }
            catch(Exception ex)
            {
                logger.Info("Error occured while posting the data in cache tables:   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
                disconnectAccess();
                response.StatusList.Add(new Status
                {
                    Level = (status == "success") ? StatusLevel.Success : StatusLevel.Error,
                    Messages = new org.iringtools.library.Messages { (status == "success") ? " Record posted successfully." : " Error in posting this record." }
                });
            }
            
            return response;
        }

        private void UpdateOnEveryPost(DataRow row)
        {
            try
            {
                ConnectToSqL();
                ConnectToAccess();

                var list = (from dProperties in _dataDictionary.dataObjects[0].keyProperties
                            select dProperties).ToList();

                string tag = list[0].keyPropertyName;
                string Tagvalue = row[tag].ToString();

                string query = "select linkage_index from labels inner join label_values on labels.label_value_index = label_values.label_value_index"
                                 + " where label_values.label_value =" + Tagvalue
                                 + " and labels.label_name_index = " + Spool_Index;

                SqlCommand comm = new SqlCommand(query, _conn);
                SqlDataAdapter da = new SqlDataAdapter(comm);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // List of linkage indexes ... 
                if (dt.Rows.Count > 0)
                {
                    OleDbCommand commOledb = null;
                    foreach (KeyValuePair<int, string> keyVal in _newProperties)
                    {
                        // Inserting in Label Value...
                        query = "SELECT MAX (label_value_index)+1 FROM label_values";
                        comm = new SqlCommand(query, _conn);
                        int iLastValueIndex = (Int32)comm.ExecuteScalar();


                        string val = row[keyVal.Value].ToString();
                        query = "select count(*) from label_values where label_value= " + val;
                        comm = new SqlCommand(query, _conn);
                        int count = (Int32)comm.ExecuteScalar();

                        //If value for new properties not found in label_values , insert it.
                        if (count == 0)
                        {
                            double lblValNumeric = ConvertToDouble(val);
                            query = "insert into label_values (label_value_index,label_value, label_value_numeric) values ( " +
                                     iLastValueIndex + "," + val + "," + lblValNumeric + " )"; // Numeric conversion .

                            comm = new SqlCommand(query, _conn);
                            comm.ExecuteNonQuery();

                            //Insert into Access simultaneously - Start
                            commOledb = new OleDbCommand(query, _connOledb);
                            commOledb.ExecuteNonQuery();
                            //Insert into Access  simultaneously - End

                            iLastValueIndex++;
                        }

                        //Inserting into Label ... 
                        int lable_line_count = 0;
                        foreach (DataRow linkageRow in dt.Rows)
                        {

                            query = "select Max(label_line_number)+1 from labels where linkage_index= " + linkageRow["linkage_index"];
                            comm = new SqlCommand(query, _conn);
                            lable_line_count = (Int32)comm.ExecuteScalar();


                            query = "Insert into labels (linkage_index,label_name_index,label_value_index,label_line_number,extended_label) values ( " +
                                     linkageRow["linkage_index"] + "," + keyVal.Key + "," + val + "," + lable_line_count + ", 0)";

                            comm = new SqlCommand(query, _conn);
                            comm.ExecuteNonQuery();

                            //Insert into Access simultaneously - Start
                            commOledb = new OleDbCommand(query, _connOledb);
                            commOledb.ExecuteNonQuery();
                            //Insert into Access  simultaneously - End
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                logger.Info("Error Occured while updating the Label and its associated Label tables" + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
                disconnectAccess();
            }
        }


        /// <summary>
        /// Handling label_value_numeric
        /// </summary>
        public static Double ConvertToDouble(string s)
        {
            try
            {
                string result = "";
                System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[1-9][0-9,.]*");
                var arr = reg.Match(s).Value.Split('.');

                if (arr.Length == 1)
                    result = arr[0];
                else
                    result = arr[0] + "." + arr[1];

                result = result.Trim('.');

                if (String.IsNullOrWhiteSpace(result))
                    return 0;

                return Convert.ToDouble(result);
            }
            catch (Exception ex)
            {
                logger.Info("Error occured in ConvertToDouble for label_value_numeric"+ex.Message);
                return 0;
            }
        }

        public override Response DeleteDataTable(string tableName, string whereClause)
        {
            Response response = new Response();
            response.StatusList = new List<Status>();
            Status status = new Status();
            status.Level = StatusLevel.Error;
            status.Identifier = tableName;
            string query = "Delete FROM " + tableName + whereClause;

            try
            {
                // Delete from SQL DB
                ConnectToSqL();
                SqlCommand command = new SqlCommand(query, _conn);
                int iSqlRowCount = command.ExecuteNonQuery();
                
                // Delete from Access
                ConnectToAccess();
                OleDbCommand Oledbcommand = new OleDbCommand(query, _connOledb);
                int iAccessRowCount = Oledbcommand.ExecuteNonQuery();

                if (iSqlRowCount > 0 && iAccessRowCount > 0)
                {
                    status.Level = StatusLevel.Success;
                    status.Messages = new Messages();
                    status.Messages.Add(string.Format("Record have been deleted successfully."));
                    response.Append(status);
                }
                return response;
            }
            catch (Exception ex)
            {
                logger.Info("Error while deleting the data:   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
                disconnectAccess();
            }
        }

        public override Response DeleteDataTable(string tableName, IList<string> identifiers)
        {
            Response response = new Response();
            response.StatusList = new List<Status>();

            string delimiter = string.Empty;
            IList<string> keyProperties = new List<string>();
            DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
            keyProperties = (from p in dataObject.keyProperties
                             select p.keyPropertyName).ToList<string>();
            if (keyProperties.Count > 1)
            {
                delimiter = dataObject.keyDelimeter;
            }

            foreach (string identifier in identifiers)
            {
                int i = 0;
                string[] ids = null;
                string tempQry = string.Empty;
                if (keyProperties.Count > 1 && identifier.Contains(delimiter.FirstOrDefault()))
                {
                    ids = identifier.Split(delimiter.FirstOrDefault());

                }
                while (i < keyProperties.Count())
                {
                    if (i != 0)
                    {
                        tempQry += " and ";
                    }
                    IList<DataProperty> property = (from p in dataObject.dataProperties 
                                            where p.propertyName == keyProperties[i].ToString()
                                                 select p).ToList();
                    
                    if (keyProperties.Count > 1)
                    {
                        if (property[0].dataType == DataType.Int32)
                        {
                            tempQry = tempQry + keyProperties[i] + " = " + ids[i];
                        }
                        else
                        {
                            tempQry = tempQry + keyProperties[i] + " = '" + ids[i] + "'";
                        }
                    }
                    else
                    {
                        if (property[0].dataType == DataType.Int32)
                        {
                            tempQry = tempQry + keyProperties[i] + " = " + identifier;
                        }
                        else
                        {
                            tempQry = tempQry + keyProperties[i] + " = '" + identifier + "'";
                        }
                    }
                    i++;
                }
                try
                {
                    // Delete From SQL ...
                    ConnectToSqL();
                    string query = "Delete FROM " + tableName + " where " + tempQry;
                    SqlCommand command = new SqlCommand(query, _conn);
                    int iSqlRowCount = command.ExecuteNonQuery();

                    // Delete From Access ...
                    ConnectToAccess();
                    OleDbCommand OleDbcommand = new OleDbCommand(query, _connOledb);
                    int iAccessRowCount = OleDbcommand.ExecuteNonQuery();

                    if (iSqlRowCount > 0 && iAccessRowCount > 0)
                    {
                        Status status = new Status();
                        status.Messages = new Messages();
                        status.Identifier = identifier;
                        status.Messages.Add(string.Format("Record [{0}] have been deleted successfully.", identifier));
                        response.Append(status);
                    }

                }
                catch (Exception ex)
                {
                    logger.Info("Error while deleting the data:   " + ex.Message);
                    throw ex;
                }
                finally
                {
                    disconnectSqL();
                    disconnectAccess();
                }
            }
            return response;
        }

        public override IList<string> GetIdentifiers(string tableName, string whereClause)
        {
            IList<string> identifiers = new List<string>();
            string query = string.Empty;
            string delimiter = string.Empty;
            IList<string> keyProperties = new List<string>();
            DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
            keyProperties = (from p in dataObject.keyProperties
                             select p.keyPropertyName).ToList<string>();

            StringBuilder keys = new StringBuilder();
            string separator = "";
            foreach (string prop in keyProperties)
            {
                keys.Append(separator + prop);
                separator = ",";
            }

            if (string.IsNullOrEmpty(whereClause))
            {
                query = "select " + keys + " from " + tableName;
            }
            else
            {
                query = "select " + keys + " from " + tableName + whereClause;
            }

            try
            {
                ConnectToSqL();
                _adapter = new SqlDataAdapter();
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.CommandText = query;
                sqlCmd.Connection = _conn;
                _adapter = new SqlDataAdapter(sqlCmd);
                DataSet dataSet = new DataSet();
                _adapter.Fill(dataSet, tableName);

                DataTable dataTable = dataSet.Tables[tableName];
                if (keyProperties.Count > 1)
                {
                    delimiter = dataObject.keyDelimeter;
                }

                foreach (DataRow row in dataTable.Rows)
                {
                    if (keyProperties.Count > 1)
                    {
                        identifiers.Add(Convert.ToString(row[0]) + delimiter + Convert.ToString(row[1]));
                    }
                    else
                        identifiers.Add(Convert.ToString(row[0]));
                }
                return identifiers;
            }
            catch (Exception ex)
            {
                logger.Info("Error while retrieving the data:   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
            }
        }

        public override DataTable CreateDataTable(string tableName, IList<string> identifiers)
        {
            DataTable result = null;
            if (identifiers == null || identifiers.Count == 0 || identifiers[0] == null)
            {
                result = GetDataTable(tableName, " WHERE 1=0", 0, 1);
                result.Rows.Add(result.NewRow());
                return result;
            }
            else
            {
                return GetDataTable(tableName, identifiers);
            }
            /*
            DataTable dataTable = new DataTable();
            DataTable dTable = new DataTable(tableName);
            string query = string.Empty;
            string delimiter = string.Empty;
            string[] idArray = null;
            DataObject dataObject = _dictionary.dataObjects.Where<DataObject>(p => p.tableName == tableName).FirstOrDefault();
            IList<string> keyProperties = keyProperties = (from p in dataObject.keyProperties
                                                           select p.keyPropertyName).ToList<string>();
            if (keyProperties.Count > 1)
            {
                delimiter = dataObject.keyDelimeter;
            }
            IList<string> idList = new List<string>();
            if (identifiers != null)
            {

                int j = 0;
                foreach (string identifier in identifiers)
                {
                    idList.Add(identifier);
                    dataTable = GetDataTable(tableName, idList);
                    idList.Clear();
                    if (dataTable != null && dataTable.Rows != null)
                    {
                        if (dTable.Rows.Count == 0)
                            dTable = dataTable.Clone();
                        if (dataTable.Rows.Count > 0)
                            dTable.Rows.Add(dataTable.Rows[j].ItemArray);
                        else
                        {
                            DataRow drow = null;

                            for (int i = 0; i < keyProperties.Count; i++)
                            {
                                if (identifier.Contains(delimiter.FirstOrDefault()))
                                {
                                    idArray = identifier.Split(delimiter.FirstOrDefault());
                                }
                                else if (keyProperties.Count == 1)
                                {
                                    idArray = identifier.Split();
                                }
                            }
                            drow = dTable.NewRow();
                            dTable.Rows.Add(drow);
                            for (int i = 0; i < keyProperties.Count; i++)
                            {
                                drow[keyProperties[i]] = idArray[i];
                                drow.AcceptChanges();
                            }
                        }
                    }
                    j++;
                }

            }
            return dTable;*/
        }

        public override DataTable GetRelatedDataTable(DataRow dataRow, string relatedTableName)
        {
            DataObject dataObject = (from p in _dictionary.dataObjects
                                     where p.tableName == dataRow.Table.TableName
                                     select p).FirstOrDefault();

            DataObject relatedDataObject = (from p in _dictionary.dataObjects
                                            where p.tableName == relatedTableName
                                            select p).FirstOrDefault();

            string relationshipType = (from p in dataObject.dataRelationships
                                       where p.relatedObjectName == relatedDataObject.objectName
                                       select p.relationshipType.ToString()).FirstOrDefault();

            IList<string> dataPropertyNames = (from p in dataObject.dataRelationships
                                               where p.relatedObjectName == relatedDataObject.objectName
                                               select p.propertyMaps.FirstOrDefault().dataPropertyName).ToList<string>();

            IList<string> relatedPropertyNames = (from p in dataObject.dataRelationships
                                                  where p.relatedObjectName == relatedDataObject.objectName
                                                  select p.propertyMaps.FirstOrDefault().relatedPropertyName).ToList<string>();
            string query = string.Empty;
            string tempqry = string.Empty;
            string qrySeparator = "";
            for (int i = 0; i < relatedPropertyNames.Count; i++)
            {

                if (tempqry.Length > 0)
                    qrySeparator = " and ";

                tempqry = qrySeparator + relatedPropertyNames[i] + " = '" + dataRow[dataPropertyNames[i]] + "'";
            }

            try
            {
                if (relationshipType.ToUpper() == "ONETOONE")
                    query = "select Top 1 * from " + relatedTableName + " where " + tempqry;
                else
                    query = "select * from " + relatedTableName + " where " + tempqry;

                ConnectToSqL();

                _adapter = new SqlDataAdapter();
                _adapter.SelectCommand = new SqlCommand(query, _conn);

                _command = new SqlCommandBuilder(_adapter);
                _adapter.UpdateCommand = _command.GetUpdateCommand();
                DataSet dataSet = new DataSet();
                _adapter.Fill(dataSet, relatedTableName);
                DataTable dataTable = dataSet.Tables[relatedTableName];
                return dataTable;
            }
            catch (Exception ex)
            {
                logger.Info("Error while retrieving the data:   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
            }
        }

        public override long GetCount(string tableName, string whereClause)
        {
            long count = 0;
            string query = string.Empty;
            if (!string.IsNullOrEmpty(whereClause))
                query = "select count(*) from " + tableName + whereClause;
            else
                query = "select count(*) from" + tableName;
            try
            {
                ConnectToSqL();
                _adapter = new SqlDataAdapter();
                _adapter.SelectCommand = new SqlCommand(query, _conn);

                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.CommandText = query;
                sqlCmd.Connection = _conn;
                _adapter = new SqlDataAdapter(sqlCmd);
                DataSet dataSet = new DataSet();
                _adapter.Fill(dataSet, tableName);
                DataTable dataTable = dataSet.Tables[tableName];
                return dataTable.Rows.Count;
            }
            catch (Exception ex)
            {
                logger.Info("Error while retrieving the data:   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
            }
        }

        public override Response Configure(XElement configuration)
        {
            throw new NotImplementedException();
        }

        public override XElement GetConfiguration()
        {
            throw new NotImplementedException();
        }

        public override Response RefreshDataTable(string tableName)
        {
            throw new NotImplementedException();
        }

        public override Response RefreshAll()
        {
           // RefreshSqLDataBase();
            Response response = CreateCacheAndFill();
            CreateSpoolinCache();
            return response;
        }


        private Response CreateCacheAndFill()
        {
            
            Response response = new Response();
            string status = string.Empty;
            try
            {
                ConnectToAccess();

                DataTable dataTable = _connOledb.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                foreach (DataRow tablerow in dataTable.Rows)
                {
                    string strSheetTableName = tablerow["TABLE_NAME"].ToString();

                    //Get Primary keys of the table - Start
                    DataTable colKey = _connOledb.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new Object[] { null, null, strSheetTableName });
                    DataView keyview = colKey.DefaultView;
                    keyview.Sort = "ORDINAL";
                    colKey = keyview.ToTable();
                    List<string> tableKeys = new List<string>();
                    foreach (DataRow row in colKey.Rows)
                    {
                        tableKeys.Add(row["COLUMN_NAME"].ToString());
                    }
                    //Get Primary keys of the table - End

                    // Get All columns of the table.
                    DataTable cols = _connOledb.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strSheetTableName, null });
                    DataView view = cols.DefaultView;
                    view.Sort = "ORDINAL_POSITION";
                    cols = view.ToTable();

                    StringBuilder sb = new StringBuilder();

                    foreach (DataRow colRow in cols.Rows)
                    {
                        string vsSQL = string.Empty;
                        string strColumn = colRow["COLUMN_NAME"].ToString();

                        string isNullable = string.Empty;
                        if (Convert.ToBoolean(colRow["IS_NULLABLE"]) && !tableKeys.Contains(strColumn))
                            isNullable = "NULL";
                        else
                            isNullable = "NOT NULL";


                        string dataType = string.Empty;
                        if (colRow["DATA_TYPE"].ToString() == "2" || colRow["DATA_TYPE"].ToString() == "3"
                             || colRow["DATA_TYPE"].ToString() == "4")
                        {
                            dataType = "INT";
                            vsSQL = "[" + strColumn + "] [" + dataType + "] " + isNullable + ",";
                        }
                        //else if (colRow["DATA_TYPE"].ToString() == "5")
                        //{
                        //    dataType = "FLOAT";
                        //    vsSQL = "[" + strColumn + "] [" + dataType + "] " + isNullable + ",";
                        //}
                        else if (colRow["DATA_TYPE"].ToString() == "11")
                        {
                            //dataType = "BIT"; True/false in 0 & 1
                            dataType = "INT";
                            vsSQL = "[" + strColumn + "] [" + dataType + "] " + isNullable + ",";
                        }
                        else if (colRow["DATA_TYPE"].ToString() == "128")
                        {
                            dataType = "image";
                            vsSQL = "[" + strColumn + "] [" + dataType + "] " + isNullable + ",";
                        }
                        else if (colRow["DATA_TYPE"].ToString() == "7")
                        {
                            dataType = "DATE";
                            vsSQL = "[" + strColumn + "] [" + dataType + "] " + isNullable + ",";
                        }
                        else
                        {
                            int length = 0;
                            if (!string.IsNullOrEmpty(colRow["CHARACTER_MAXIMUM_LENGTH"].ToString()))
                                length = Convert.ToInt32(colRow["CHARACTER_MAXIMUM_LENGTH"]);

                            if (length == 0)
                                vsSQL = "[" + strColumn + "] [varchar] (MAX) " + isNullable + ",";
                            else
                                vsSQL = "[" + strColumn + "] [varchar] (" + length + ") " + isNullable + ",";
                        }

                        sb.Append(vsSQL);
                    }

                    // Generate Primary keys string
                    string vInheritColumns = string.Empty;
                    string sKeys = string.Empty;
                    foreach (string key in tableKeys)
                    {
                        sKeys += key + ",";
                    }

                    if (!string.IsNullOrEmpty(sKeys))
                    {
                        sKeys = sKeys.Substring(0, sKeys.LastIndexOf(','));
                        sKeys = "PRIMARY KEY (" + sKeys + " )";
                        sb.Append(sKeys);
                        vInheritColumns = sb.ToString();
                    }
                    else
                    {
                        vInheritColumns = sb.ToString(0, sb.Length - 1);
                    }

                    ConnectToSqL();
                    SqlCommand sqlcomm = new SqlCommand();
                    sqlcomm.Connection = _conn;

                    // First, dropping the table if it exists. 
                    sqlcomm.CommandText = string.Format(SqlConstant.DROP_DB, strSheetTableName);
                    sqlcomm.ExecuteNonQuery();

                    // Second, Creating the table in the cache.
                    sqlcomm.CommandText = "CREATE TABLE [dbo].[" + strSheetTableName + "] (" + vInheritColumns + ")";
                    sqlcomm.ExecuteNonQuery();

                    OleDbCommand command = new OleDbCommand();
                    command.Connection = _connOledb;
                    DataTable tableColumns = _connOledb.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, strSheetTableName, null });
                    command.CommandText = "Select * from " + strSheetTableName;
                    OleDbDataReader tAccess = command.ExecuteReader();

                    // Bulk copy from Access to SQL ...  
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_conn))
                    {
                        bulkCopy.BulkCopyTimeout = 600;
                        bulkCopy.DestinationTableName = "dbo." + strSheetTableName;
                        try
                        {
                            bulkCopy.WriteToServer(tAccess);
                        }
                        catch (Exception ex)
                        {
                            status = "fail";
                            Console.WriteLine(ex.Message);
                        }
                    }
                    status = "success";
                }
            }
            catch (Exception ex)
            {
                logger.Info("Error occured while caching the tables and the data :   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectAccess();
                disconnectSqL();
                response.StatusList.Add(new Status
                {
                    Level = (status == "success") ? StatusLevel.Success : StatusLevel.Error,
                    Messages = new org.iringtools.library.Messages { (status == "success") ? " Record cached successfully." : " Error occured while caching the tables and the data" }
                });
            }
            return response;
        }

        private bool CreateSpoolinCache()
        {
            try
            {
                if (_dictionary == null)
                {
                    _dictionary = GetDatabaseDictionary();
                }

                StringBuilder sb = new StringBuilder();
                foreach (DataObject dataobject in _dictionary.dataObjects)
                {
                    string tableName = dataobject.tableName;
                    string query = string.Empty;
                    string vInheritColumns = string.Empty;

                    foreach (DataProperty property in dataobject.dataProperties)
                    {
                        string isNullable = string.Empty;
                        if (property.isNullable)
                            isNullable = "NULL";
                        else
                            isNullable = "NOT NULL";

                        if (property.dataType == DataType.String)
                        {
                            query = "[" + property.propertyName + "] [varchar] (" + property.dataLength + ") " + isNullable + ",";
                        }
                        else if (property.dataType == DataType.DateTime)
                        {
                            query = "[" + property.propertyName + "] [DATETIME] " + isNullable + ",";
                        }
                        else if (property.dataType == DataType.Int32 || property.dataType == DataType.Boolean) // Access saves booleans in 0 &1.
                        {
                            query = "[" + property.propertyName + "] [INT] (" + property.dataLength + ") " + isNullable + ",";
                        }

                        sb.Append(query);
                    }

                    string sKeys = string.Empty;
                    foreach (KeyProperty key in dataobject.keyProperties)
                    {
                        sKeys += key.keyPropertyName + ",";
                    }

                    if (!string.IsNullOrEmpty(sKeys))
                    {
                        sKeys = sKeys.Substring(0, sKeys.LastIndexOf(','));
                        sKeys = "PRIMARY KEY (" + sKeys + " )";
                        sb.Append(sKeys);
                        vInheritColumns = sb.ToString();
                    }
                    else
                    {
                        vInheritColumns = sb.ToString(0, sb.Length - 1);
                    }

                    ConnectToSqL();
                    SqlCommand sqlcomm = new SqlCommand();
                    sqlcomm.Connection = _conn;

                    // First, dropping the table if it exists. 
                    sqlcomm.CommandText = string.Format(SqlConstant.DROP_DB, tableName);
                    sqlcomm.ExecuteNonQuery();

                    // Second, Creating the table in the cache.
                    sqlcomm.CommandText = "CREATE TABLE [dbo].[" + tableName + "] (" + vInheritColumns + ")";
                    sqlcomm.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                logger.Info("Error occured while creating the spool in cache :   " + ex.Message);
                return false;
            }
            finally
            {
                disconnectSqL();
            }

            return true;
        }

        public Response ReverseRefresh()
        {
            string status = string.Empty;
            Response response = new Response();
            try
            {
                ConnectToSqL();
                List<string> SQLtableNames = LoadSQLTable();
                List<string> AccesstableNames = new List<string>();

                ConnectToAccess();
                DataTable dt = _connOledb.GetSchema("tables");
                foreach (DataRow row in dt.Rows)
                {
                    if (row["TABLE_TYPE"].ToString() == "TABLE")
                        AccesstableNames.Add(row["TABLE_NAME"].ToString());
                }

                foreach (string tblName in SQLtableNames)
                {
                    if (AccesstableNames.Contains(tblName))
                    {
                        // Drop and create 

                        OleDbCommand command = new OleDbCommand();
                        command.Connection = _connOledb;
                        command.CommandText = string.Format("DELETE FROM {0}", tblName);
                        command.ExecuteNonQuery();
                        logger.Info("Table deleted");

                        DataTable sqlDatatable = new DataTable();
                        SqlCommand sqlCommand = new SqlCommand();
                        SqlDataAdapter sqlda = new SqlDataAdapter(string.Format("SELECT * FROM {0}", tblName), _conn);
                        sqlda.Fill(sqlDatatable);

                        DataTable oldbDatatable = new DataTable();
                        command = new OleDbCommand();
                        OleDbDataAdapter olda = new OleDbDataAdapter(string.Format("SELECT * FROM {0}", tblName), _connOledb);
                        olda.Fill(oldbDatatable);


                        foreach (DataRow row in sqlDatatable.Rows)
                        {
                            string query = "";
                            string colsNames = "";

                            command = new OleDbCommand();
                            foreach (DataColumn col in sqlDatatable.Columns)
                            {
                                colsNames += col.ColumnName + ",";
                                if (row[col].ToString().Contains("'"))
                                {
                                    query += "'" + row[col].ToString().Replace("'","''") + "'" + ",";
                                }
                                else
                                {
                                    if (oldbDatatable.Columns[col.ColumnName].DataType != sqlDatatable.Columns[col.ColumnName].DataType &&
                                    oldbDatatable.Columns[col.ColumnName].DataType == typeof(Double) && row[col].ToString().Contains("Infinity"))
                                    {
                                        OleDbParameter param = null;
                                        query += "? ,";
                                        if (row[col].ToString() == "Infinity")
                                        {
                                            param = new OleDbParameter(col.ColumnName, double.PositiveInfinity);
                                        }
                                        else if (row[col].ToString() == "-Infinity")
                                        {
                                            param = new OleDbParameter(col.ColumnName, double.NegativeInfinity);
                                        }
                                        command.Parameters.Add(param);
                                    }
                                    else
                                    {
                                        query += "'" + row[col] + "'" + ",";
                                    }
                                }
                            }

                            colsNames = colsNames.Substring(0, colsNames.LastIndexOf(','));
                            query = query.Substring(0, query.LastIndexOf(','));

                            query = "Insert into " + tblName + " (" + colsNames + " )  Values (" + query + " );";

                            command.CommandText = query;
                            command.Connection = _connOledb;
                            command.ExecuteNonQuery();

                        }
                    }

                }
                status = "success";
            }
            catch (Exception ex)
            {
                logger.Info("Error occured while updating the Access tables and the data :   " + ex.Message);
                throw ex;
            }
            finally
            {
                disconnectAccess();
                disconnectSqL();
                response.StatusList.Add(new Status
                {
                    Level = (status == "success") ? StatusLevel.Success : StatusLevel.Error,
                    Messages = new org.iringtools.library.Messages { (status == "success") ? " Record updated successfully." : " Error occured while updated the access tables and the data" }
                });
            }
            return response;
        }

        private List<string> LoadSQLTable()
        {

            try
            {
                List<string> _dataTables = new List<string>();
                SqlCommand cmd = new SqlCommand(SqlConstant.Get_tableName, _conn);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        _dataTables.Add(reader["TABLE_NAME"].ToString());
                    }
                }

                reader.Close();
                return _dataTables;
            }
            catch (Exception ex)
            {
                logger.Info("Error while fetching table names:   " + ex.Message);
                throw ex;
            }
        }


        public override long GetRelatedCount(DataRow dataRow, string relatedTableName)
        {
            throw new NotImplementedException();
        }

        private void ConnectToAccess()
        {
            if (_connOledb == null || _connOledb.State == ConnectionState.Closed)
            {
                _connOledb = new OleDbConnection(_mdbConnectionString);
                _connOledb.Open();
            }
        }

        private void ConnectToSqL()
        {
            if (_conn == null || _conn.State == ConnectionState.Closed)
            {
                _conn = new SqlConnection(_dbConnectionString);
                _conn.Open();
            }
        }

        private void disconnectAccess()
        {
            if (_connOledb != null && _connOledb.State == ConnectionState.Open)
            {
                _connOledb.Close();
            }
        }

        private void disconnectSqL()
        {
            if (_conn != null && _conn.State == ConnectionState.Open)
            {
                _conn.Close();
            }
        }

        private void AddSpoolProperties()
        {
            try
            {
                ConnectToSqL();
                string InitialQuery = "select label_name_index from label_names	where label_name = 'Spool'";

                SqlCommand comm = new SqlCommand(InitialQuery, _conn);
                SqlDataReader reader = comm.ExecuteReader();

                int lblNameIndex = 0;
                while (reader.Read())
                {
                    lblNameIndex =Convert.ToInt32(reader["label_name_index"]);
                    Spool_Index = lblNameIndex;
                    break;
                }

                if (lblNameIndex != 0)
                {

                    InitialQuery = "select MAX(label_name_index)+1 FROM label_names";
                    comm = new SqlCommand(InitialQuery, _conn);
                    int iLastlabel_nameCount = (Int32)comm.ExecuteScalar();

                    var list = from dProperties in _dataDictionary.dataObjects[0].dataProperties
                               select dProperties;

                    // Getting all spool Properties.
                    foreach (var property in list)
                    {
                        InitialQuery = "select count(*) FROM label_names where label_name =" + property.columnName;
                        comm = new SqlCommand(InitialQuery, _conn);
                        int count = (Int32)comm.ExecuteScalar();

                        // If Property not found, Plz insert it.
                        if (count == 0)
                        {
                            InitialQuery = "Insert into label_names (label_name_index,label_names) values ( " + iLastlabel_nameCount + "," + property.columnName + " )";
                            comm = new SqlCommand(InitialQuery, _conn);
                            comm.ExecuteNonQuery();


                            // Insert into mbd simultaneously  - START
                            ConnectToAccess();                           
                            OleDbCommand cmmdOledb = new OleDbCommand(InitialQuery, _connOledb);
                            cmmdOledb.ExecuteNonQuery();
                            // Insert into mbd simultaneously  - END

                            _newProperties.Add(iLastlabel_nameCount, property.columnName);
                            iLastlabel_nameCount++;
                        }

                    }
                    IsSpoolPropertyAdded = true; // One time Spool Properties added.
                }
                else
                {
                    throw new Exception("Spool not found in the label_names");
                }
                
            }
            catch (Exception ex)
            {
                logger.Info(ex.Message);
                throw ex;
            }
            finally
            {
                disconnectSqL();
                disconnectAccess();
            }
        }

    }
}
