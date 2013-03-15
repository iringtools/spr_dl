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

        public SPRSynchronizationUtility()
        {
            _settings = new NameValueCollection();

            _settings["ProjectName"] = "12345_000";
            _settings["XmlPath"] = @".\App_Data\";
            _settings["ApplicationName"] = "SQL";

            _baseDirectory = Directory.GetCurrentDirectory();
            _baseDirectory = _baseDirectory.Substring(0, _baseDirectory.LastIndexOf("\\bin")); // that's bad.
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
        }

       /// <summary>
       /// This would be called After Exchange and it will update the MDB.
       /// </summary>
        public void MDBSynchronization()
        {
            Response response = _dataLayer.RefreshAll();
            IList<IDataObject> dataObjects = _dataLayer.Get("Spools", new DataFilter(), 0, 0);
            response = _dataLayer.Post(dataObjects);
            _dataLayer.ReverseRefresh();
        }

       /// <summary>
       /// Its populating the Spool table in the SQL db based on the mdb values. 
       /// </summary>
        public void PopulateSpool()
        {
            _dataLayer.PopulateSpoolTable();
        }
    }
}
