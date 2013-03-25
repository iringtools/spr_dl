using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using org.iringtools.adapter;
using org.iringtools.library;
using System.IO;
using StaticDust.Configuration;
using Bechtel.iRING.SPR;

namespace Bechtel.iRING.SPRUtility
{
   public class SPRSynchronizationUtility
    {
        private string _baseDirectory = string.Empty;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private SPRDataLayer _dataLayer;
        StreamWriter _logFile = null;

        public SPRSynchronizationUtility(StreamWriter logFile)
        {
            _logFile = logFile;
            _settings = new NameValueCollection();

            _settings["ProjectName"] = "12345_000";
            _settings["XmlPath"] = @".\App_Data\";
            _settings["ApplicationName"] = "SQL";

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin")); 

            _settings["BaseDirectoryPath"] = _baseDirectory;
            Directory.SetCurrentDirectory(_baseDirectory);

            _adapterSettings = new AdapterSettings();
            _adapterSettings.AppendSettings(_settings);
            string appSettingsPath = String.Format("{0}{1}.{2}.config",
                _adapterSettings["XmlPath"],
                _settings["ProjectName"],
                _settings["ApplicationName"]
            );

           
            if (File.Exists(appSettingsPath))
            {
                AppSettingsReader appSettings = new AppSettingsReader(appSettingsPath);
                _adapterSettings.AppendSettings(appSettings);
            }

            _dataLayer = new SPRDataLayer(_adapterSettings);
            _dataLayer.GetDictionary();
            _dataLayer.EnableLogging(_logFile);
        }

       /// <summary>
       /// This would be called After Exchange and it will update the MDB.
       /// </summary>
        public void MDBSynchronization(string objectType)
        {
            try
            {
                _logFile.WriteLine("Copy the database from Mdb in to SQL.");
                Response response = _dataLayer.RefreshAll();
                IList<IDataObject> dataObjects = _dataLayer.Get(objectType, new DataFilter(), 0, 0);
                response = _dataLayer.Post(dataObjects);
                _dataLayer.ReverseRefresh();
                _logFile.WriteLine("Copy the database from SQL in to Mdb.");
            }
            catch(Exception ex)
            {
                _logFile.WriteLine(ex.Message + ex.StackTrace);
                throw ex;
            }
        }

       /// <summary>
       /// Its populating the Spool table in the SQL db based on the mdb values. 
       /// </summary>
        public void PopulateSpool()
        {
            _dataLayer.PopulateSpoolTable();
        }

        //Getting the list of objects to be displayed in dropdown list.
        public List<DataObject> GetObjects()
        {
            DatabaseDictionary _dictionary = _dataLayer.GetDatabaseDictionary();
            List<DataObject> objects = _dictionary.dataObjects;
            return objects;
        }

        // Passing the Mdb Name to the Data Layer.
        public void UpdateMdbFile(string fileName)
        {
            _dataLayer.UpdateMdbFileName(fileName);
        }
    }
}
