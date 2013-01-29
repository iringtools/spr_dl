using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using NUnit.Framework;
using org.iringtools.adapter;
using org.iringtools.library;
using org.iringtools.sdk.sql;
using org.iringtools.utility;
using StaticDust.Configuration;
using System.Data.SqlClient;
using System.Text;
using System.Xml;

namespace org.iringtools.sdk.sql.test
{
    [TestFixture]
    public class SQLDataLayerTest 
    {
        private string _baseDirectory = string.Empty;
        private NameValueCollection _settings;
        private AdapterSettings _adapterSettings;
        private SQLDataLayer _dataLayer;
        private DataObject _objectDefinition;
        private string _modifiedObject = "display_set";

        public SQLDataLayerTest()
        {
            _settings = new NameValueCollection();

            _settings["ProjectName"] = "12345_000";
            _settings["XmlPath"] = @".\App_Data\";
            _settings["ApplicationName"] = "SQL";
            _settings["TestMode"] = "WriteFiles";

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
            _objectDefinition = GetObjectDefinition(_modifiedObject);

        }


        [Test]
        public void ReadWithIdentifiers()
        {
            IList<string> identifiers = new List<string>() 
            { 
                "528", 
                "520",
            };

            IList<IDataObject> dataObjects = _dataLayer.Get(_modifiedObject, identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            foreach (IDataObject dataObject in dataObjects)
            {

            }
        }

        [Test]
        public void ReadWithPaging()
        {
            DataFilter filter = new DataFilter();
            IList<IDataObject> dataObjects = _dataLayer.Get(_modifiedObject, filter, 2, 0);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            //Assert.AreEqual(dataObjects.Count(), 2);          
        }

        //[Test]
        public void TestAll()
        {

            IDataObject dataObject = _dataLayer.Get(_modifiedObject, new DataFilter(), 1, 0).First();
            string identifier = dataObject.GetPropertyValue("TAG").ToString();

            Response actual = _dataLayer.Delete("display_set", new List<string> { identifier });

            ((GenericDataObject)dataObject).ObjectType = "display_set";
            _dataLayer.Post(new List<IDataObject> { dataObject });

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void ReadWithFilter()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "display_set_user_id",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "10",
                        }
                    }
                }
            };
            IList<IDataObject> dataObjects = _dataLayer.Get(_modifiedObject, dataFilter, 10, 0);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(dataObjects.Count(), 1);
        }

        [Test]
        public void GetDictionary()
        {
            DataDictionary dictionary = _dataLayer.GetDictionary();
            Assert.IsNotNull(dictionary);
        }

       // [Test]
        public void DeleteDataTable()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "TAG",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "Hello",
                        }
                    }
                }
            };
            Response actual = _dataLayer.Delete(_modifiedObject, dataFilter);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        // [Test]
        public void RefreshCache()
        {
            Response actual = _dataLayer.RefreshAll();
            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

       // [Test]
        public void DeleteWithIdentifiers()
        {
            IList<string> identifiers = new List<string>() 
            { 
                "Hello2", 
                "Hello1",
            };

            Response actual = _dataLayer.Delete(_modifiedObject, identifiers);

            if (actual.Level != StatusLevel.Success)
            {
                throw new AssertionException(Utility.SerializeDataContract<Response>(actual));
            }

            Assert.IsTrue(actual.Level == StatusLevel.Success);
        }

        [Test]
        public void Read()
        {
            DataFilter filter = new DataFilter();

            IList<string> identifiers = _dataLayer.GetIdentifiers(_modifiedObject, filter);

            if (!(identifiers.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(identifiers.Count(), 11);

        }

        [Test]
        public void GetCount()
        {
            DataFilter dataFilter = new DataFilter
            {
                Expressions = new List<Expression>
                {
                    new Expression
                    {
                        PropertyName = "display_set_user_id",
                        RelationalOperator = RelationalOperator.EqualTo,
                        Values = new Values
                        {
                            "10",
                        }
                    }
                }
            };
            long count = _dataLayer.GetCount(_modifiedObject, dataFilter);

            if (count == 0)
            {
                throw new AssertionException("No Rows returned.");
            }

            Assert.AreEqual(count, 1);

        }

        //[Test]
        public void CreateDataTable()
        {

            IList<string> identifiers = new List<string>() 
            { 
                "530", 
                "531",
            };
            //display_set
            IList<IDataObject> dataObjects = _dataLayer.Create(_modifiedObject, identifiers);

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }

        }

        //[Test]
        public void GetRelatedDataTables()
        {

            IDataObject dataObject = _dataLayer.Get(_modifiedObject, new DataFilter(), 1, 0).First();

            ((GenericDataObject)dataObject).ObjectType = "display_set";

            IList<IDataObject> dataObjects = _dataLayer.GetRelatedObjects(dataObject, "Valves");

            if (!(dataObjects.Count() > 0))
            {
                throw new AssertionException("No Rows returned.");
            }
            Assert.Greater(dataObjects.Count, 0);
        }

        [Test]
        public void TestPostWithAddAndDeleteByFilter()
        {
            //
            // create new data object by getting an existing one and change its identifier
            //
            IList<IDataObject> dataObjects = _dataLayer.Get(_modifiedObject, new DataFilter(), 1, 8);
            string orgIdentifier = GetIdentifier(dataObjects[0]);
            string newIdentifier = "549";
            SetIdentifier(dataObjects[0], newIdentifier);

            // post new data object
            Response response = _dataLayer.Post(dataObjects);
            Assert.AreEqual(response.Level, StatusLevel.Success);
            //
            // delete the new data object with a filter
            //
            DataFilter filter = new DataFilter();

            filter.Expressions.Add(
              new Expression()
              {
                  PropertyName = "display_set_unique_id",
                  RelationalOperator = org.iringtools.library.RelationalOperator.EqualTo,
                  Values = new Values() { newIdentifier }
              }
            );

            response = _dataLayer.Delete(_modifiedObject, filter);
            Assert.AreEqual(response.Level, StatusLevel.Success);
        }

        [Test]
        public void TestPostWithAddAndDeleteByIdentifier()
        {
            //
            // create a new data object by getting an existing one and change its identifier
            //
            IList<IDataObject> dataObjects = _dataLayer.Get(_modifiedObject, new DataFilter(), 1, 8);
            string orgIdentifier = GetIdentifier(dataObjects[0]);

            string newIdentifier = "549";
            SetIdentifier(dataObjects[0], newIdentifier);

            // post the new data object
            Response response = _dataLayer.Post(dataObjects);
            Assert.AreEqual(response.Level, StatusLevel.Success);

            //
            // delete the new data object by its identifier
            //
            response = _dataLayer.Delete(_modifiedObject, new List<string> { newIdentifier });
            Assert.AreEqual(response.Level, StatusLevel.Success);
        }

        [Test]
        public void TestPostWithUpdate()
        {
            string _modifiedProperty = "display_set_user_id";
            IList<IDataObject> dataObjects = _dataLayer.Get(_modifiedObject, new DataFilter(), 1, 8);
            string orgIdentifier = GetIdentifier(dataObjects[0]);
            string orgPropValue = Convert.ToString(dataObjects[0].GetPropertyValue(_modifiedProperty)) ?? String.Empty;
            int newPropValue = 101;

            // post data object with modified property
            dataObjects[0].SetPropertyValue(_modifiedProperty, newPropValue);
            Response response = _dataLayer.Post(dataObjects);
            Assert.AreEqual(response.Level, StatusLevel.Success);

            // verify post result
            dataObjects = _dataLayer.Get(_modifiedObject, new List<string> { orgIdentifier });
            Assert.AreEqual(dataObjects[0].GetPropertyValue(_modifiedProperty), newPropValue);

            // reset property to its orginal value
            dataObjects[0].SetPropertyValue(_modifiedProperty, orgPropValue);
            response = _dataLayer.Post(dataObjects);
            Assert.AreEqual(response.Level, StatusLevel.Success);
        }

        private string GetIdentifier(IDataObject dataObject)
        {
            string[] identifierParts = new string[_objectDefinition.keyProperties.Count];

            int i = 0;
            foreach (KeyProperty keyProperty in _objectDefinition.keyProperties)
            {
                identifierParts[i] = dataObject.GetPropertyValue(keyProperty.keyPropertyName).ToString();
                i++;
            }

            return String.Join(_objectDefinition.keyDelimeter, identifierParts);
        }

        private void SetIdentifier(IDataObject dataObject, string identifier)
        {
            IList<string> keyProperties = GetKeyProperties();
            try
            {
                if (keyProperties.Count == 1)
                {
                    dataObject.SetPropertyValue(keyProperties[0], identifier);
                }
                else if (keyProperties.Count > 1)
                {
                    String[] Singleidentifier = identifier.Split(Convert.ToChar(_objectDefinition.keyDelimeter));
                    int i = 0;
                    StringBuilder identifierBuilder = new StringBuilder();

                    foreach (string keyProperty in keyProperties)
                    {
                        dataObject.SetPropertyValue(keyProperty, Singleidentifier[i]);

                        if (identifierBuilder.Length > 0)
                        {
                            identifierBuilder.Append(_objectDefinition.keyDelimeter);
                        }

                        identifierBuilder.Append(Singleidentifier[i]);
                        i++;
                    }

                    identifier = identifierBuilder.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Please check the identifier: " + ex.Message);
            }
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

        private IList<string> GetKeyProperties()
        {
            IList<string> keyProperties = new List<string>();

            foreach (DataProperty dataProp in _objectDefinition.dataProperties)
            {
                foreach (KeyProperty keyProp in _objectDefinition.keyProperties)
                {
                    if (dataProp.propertyName == keyProp.keyPropertyName)
                    {
                        keyProperties.Add(dataProp.propertyName);
                    }
                }
            }
            return keyProperties;
        }
    }
}

