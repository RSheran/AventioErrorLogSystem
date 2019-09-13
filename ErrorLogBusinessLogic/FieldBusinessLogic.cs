using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ErrorLogDataAccess.DataClasses;
using System.Globalization;

namespace ErrorLogBusinessLogic
{

    public class FieldBusinessLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        public FieldBusinessLogic()
        {
            mongoClient = new MongoClient();

            mongoDbCompleteURL = System.Configuration.ConfigurationSettings.AppSettings["MongoDBStartURL"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBReplicaSet"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBAuthSource"].ToString();
            mongoDBTargetDatabase = System.Configuration.ConfigurationSettings.AppSettings["MongoDBDatabase"].ToString();

            //Connect to public Mongo DB repository
            mongoClient = new MongoClient(mongoDbCompleteURL);

            //Get Database
            mongoDatabaseRunTime = mongoClient.GetDatabase(mongoDBTargetDatabase);
        }

       

        #region "Functions to add new Field"

        //To check product category code is available
        public bool IsFieldAvailable(string FieldCode)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                  
                    //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var FieldCollection = mongoDatabaseRunTime.GetCollection<Field>("Field");



                    if (FieldCollection.AsQueryable()
                      .Where(d => d.FieldCode.ToLower() == FieldCode.ToLower()).Count() > 0)
                    {
                        scope1.Complete();
                        return true;
                    }
                    else
                    {
                        scope1.Complete();
                        return false;
                    }

                }

                catch (Exception ex)
                {
                    scope1.Dispose();
                    //string currentFile = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
                    //StackTrace st = new StackTrace();
                    //StackFrame sf = st.GetFrame(1);
                    //string methodName = sf.GetMethod().Name;
                    //ErrorLogHelper.UpdatingErrorLog(currentFile + "-" + methodName, "UName", ex);
                    //return false;
                    throw;
                }
            }



        }

        //Function to get new Field ID
        public int GetNewFieldID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                   
                    int newFieldID = 0;

                    var FieldCollection = mongoDatabaseRunTime.GetCollection<Field>("Field");
                    

                    if (FieldCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = FieldCollection.AsQueryable()
                                   .OrderByDescending(a => a.FieldId)
                                   .FirstOrDefault().FieldId;

                        newFieldID = maxID + 1;


                    }
                    else
                    {
                        scope1.Complete();
                        newFieldID = 1;
                    }

                    return newFieldID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }


        //save using a 'Field' type variable
        public bool SaveField(ErrorLogDataAccess.DataClasses.Field FieldObj, int userId = 1)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                
                    var FieldCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field");
                    
                    var FieldRow = new ErrorLogDataAccess.DataClasses.Field()
                    {
                        FieldId = GetNewFieldID(),
                        FieldCode = FieldObj.FieldCode,
                        FieldName = FieldObj.FieldName,
                        FieldDescription = FieldObj.FieldDescription,
                        IsActive = FieldObj.IsActive,
                        EntryDate = formattedDate,
                        UserId = 1

                    };

                    FieldCollection.InsertOne(FieldRow);

                    scope1.Complete();

                    return true;
                }
                catch (Exception)
                {
                    scope1.Dispose();
                    throw;
                }
            }

        }



        #endregion


        #region "Functions to update Field"

        public bool UpdateField(ErrorLogDataAccess.DataClasses.Field FieldObj)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    
                    var FieldCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field");


                    var filterObj = Builders<ErrorLogDataAccess.DataClasses.Field>.Filter.Eq("FieldCode", FieldObj.FieldCode);

                    var updateObj = Builders<ErrorLogDataAccess.DataClasses.Field>.Update
                                 .Set("FieldName", FieldObj.FieldName)
                                 .Set("FieldDescription", FieldObj.FieldDescription)
                                 .Set("IsActive", FieldObj.IsActive)
                                 .Set("LastUpdatedDate", formattedDate)
                               .CurrentDate("lastModified");


                    FieldCollection.UpdateOne(filterObj, updateObj);


                    scope1.Complete();

                    return true;

                }
                catch (Exception ex)
                {

                    scope1.Dispose();
                    throw;

                }

            }

        }

        #endregion

        #region"Functions to change status of Field"
        //Update status of selected expense types
        public bool UpdateStatusOfSelectedFields(Field[] FieldObj)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                  
                    var FieldCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field");
                    

                    if (FieldObj.Count() != 0)
                    {
                        foreach (var FieldCurr in FieldObj)
                        {
                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.Field>.Filter.Eq("FieldCode", FieldCurr.FieldCode);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.Field>.Update
                                            .Set("IsActive", FieldCurr.IsActive)
                                            .Set("LastUpdatedDate", formattedDate)
                                            .CurrentDate("lastModified");


                            FieldCollection.UpdateOne(filterObj, updateObj);

                        }


                        scope1.Complete();


                    }


                    return true;
                }
                catch (Exception ex)
                {
                    //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public bool ChangeStatusForAllFields(bool isActiveForAll)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    var FieldCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field");
                    var FieldList = FieldCollection.AsQueryable().ToList();
                    
                    if (FieldList.Count() != 0)
                    {
                        foreach (var FieldCurr in FieldList)
                        {
                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.Field>.Filter.Eq("FieldCode", FieldCurr.FieldCode);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.Field>.Update
                                            .Set("IsActive", isActiveForAll)
                                            .Set("LastUpdatedDate", formattedDate)
                                            .CurrentDate("lastModified");


                            FieldCollection.UpdateOne(filterObj, updateObj);

                        }


                        scope1.Complete();


                    }


                    return true;
                }
                catch (Exception ex)
                {
                    
                    scope1.Dispose();
                    return false;
                }
            }
        }


        #endregion

        #region "Load Field/Field Details"

        public Array GetAllFields(int isActive)
        {

            try
            {
                
                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();
                if (isActive == 0)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.FieldCode != null)
                                     .Select(p => new
                                     {
                                         p.FieldId,
                                         p.FieldCode,
                                         p.FieldName,
                                         p.FieldDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.FieldCode).ToArray();

                    return resultArray;


                }

                else if (isActive == 1)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.FieldCode != null && w.IsActive == true)
                                     .Select(p => new
                                     {
                                         p.FieldId,
                                         p.FieldCode,
                                         p.FieldName,
                                         p.FieldDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.FieldCode).ToArray();

                    return resultArray;
                }

                else
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.FieldCode != null && w.IsActive == false)
                                     .Select(p => new
                                     {
                                         p.FieldId,
                                         p.FieldCode,
                                         p.FieldName,
                                         p.FieldDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.FieldCode).ToArray();

                    return resultArray;
                }

                return null;
            }

            catch (Exception ex)
            {
                throw;
            }

        }


        //Get details for a given Field  code
        public Array GetFieldDetailsForCode(string FieldCode)
        {

            try
            {
                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.FieldCode.ToLower().Trim() == FieldCode.ToLower().Trim())
                                     .Select(p => new
                                     {
                                         p.FieldCode,
                                         p.FieldName,
                                         p.FieldDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.FieldCode).ToArray();

                return resultArray;

            }

            catch (Exception ex)
            {
                throw;
            }

        }

        //Get Field codes for autosuggestion(Top 10)
        public Array GetFieldCodesForAutoComplete(string FieldCode)
        {

            try
            {

                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.FieldCode.ToLower().Contains(FieldCode.ToLower().Trim()))
                                     .Select(p => new
                                     {
                                         p.FieldCode,
                                         p.FieldName

                                     }).OrderBy(p => p.FieldCode).Take(10).ToArray();

                return resultArray;

            }

            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion


    }
}
