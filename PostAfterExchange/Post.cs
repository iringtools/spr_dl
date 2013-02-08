using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using org.iringtools.adapter;
using org.iringtools.sdk.spr;
using org.iringtools.library;
using System.IO;
using StaticDust.Configuration;

namespace PostAfterExchange
{
   public class Post
    {
        private string _baseDirectory = string.Empty;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private SQLDataLayer _dataLayer;
        private DataObject _objectDefinition;

        public Post()
        {
            _settings = new NameValueCollection();

            _settings["ProjectName"] = "12345_000";
            _settings["XmlPath"] = @".\App_Data\";
            _settings["ApplicationName"] = "SQL";
            //_settings["TestMode"] = "WriteFiles";

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

            _dataLayer = new SQLDataLayer(_adapterSettings);
            _objectDefinition = GetObjectDefinition("spool_proxy");
        }

        private DataObject GetObjectDefinition(string objectType)
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();

            if (dictionary.dataObjects != null)
            {
                foreach (DataObject dataObject in dictionary.dataObjects)
                {
                    if (dataObject.objectName.ToLower() == objectType.ToLower())
                    {
                        return dataObject;
                    }
                }
            }
            return null;
        }

        public void TestPost()
        {
            Response response = _dataLayer.RefreshAll();

            IList<IDataObject> dataObjects = _dataLayer.Get("Spools", new DataFilter(), 25, 0);

            dataObjects = new List<IDataObject>();

            IDataObject dataObject = new GenericDataObject() { ObjectType = "Spools" };

            dataObject.SetPropertyValue("Spool", "01EKG11PS02001");
            dataObject.SetPropertyValue("WorkPackage", "WP1");
            dataObject.SetPropertyValue("ConstructionStatus", "Go!");

            dataObjects.Add(dataObject);

            Response response = _dataLayer.Post(dataObjects);

        }
    }
}
