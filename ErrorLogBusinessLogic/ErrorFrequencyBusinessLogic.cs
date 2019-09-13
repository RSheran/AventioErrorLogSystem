using ErrorLogDataAccess.DataClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ErrorLogBusinessLogic
{
    public class ErrorFrequencyBusinessLogic
    {
        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        public ErrorFrequencyBusinessLogic()
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

        #region "Update the frequency of Error=>Done whenever a user clicks on error"

        //To check whether a frequency row  is available for a given errorCode
        public bool IsErrorFrequencyAvailable(string errorCode)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                  
                    //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var errorFreqCollection = mongoDatabaseRunTime.GetCollection<ErrorFrequency>("ErrorFrequency");



                    if (errorFreqCollection.AsQueryable()
                      .Where(d => d.ErrorCode.ToLower() == errorCode.ToLower()).Count() > 0)
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
                  
                    throw;
                }
            }



        }

        //Function to get new error frequency ID
        public int GetNewErrorFrequencyID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                   
                    int newErrorFreqID = 0;

                    var errorFreqCollection = mongoDatabaseRunTime.GetCollection<ErrorFrequency>("ErrorFrequency");



                    if (errorFreqCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = errorFreqCollection.AsQueryable()
                                   .OrderByDescending(a => a.ErrorFrequencyId)
                                   .FirstOrDefault().ErrorFrequencyId;

                        newErrorFreqID = maxID + 1;


                    }
                    else
                    {
                        scope1.Complete();
                        newErrorFreqID = 1;
                    }

                    return newErrorFreqID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

     

        public bool UpdateSearchFrequencyForError(string errorCode)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    
                    var errorFreqCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.ErrorFrequency>("ErrorFrequency");

                    int frequencyCount = IsErrorFrequencyAvailable(errorCode) == false ? 1 :
                                        ((errorFreqCollection.AsQueryable().Where(a => a.ErrorCode.ToLower() == errorCode.ToLower())
                                        .FirstOrDefault().SearchFrequency) + 1);

                   

                    if (frequencyCount == 1)
                    {
                        var ErrorFrequencyRow = new ErrorLogDataAccess.DataClasses.ErrorFrequency()
                        {
                            ErrorFrequencyId= GetNewErrorFrequencyID(),
                            ErrorCode=errorCode,
                            SearchFrequency= frequencyCount,
                            LastUpdatedDate= formattedDate

                        };

                        errorFreqCollection.InsertOne(ErrorFrequencyRow);
                    }
                    else
                    {
                        var filterObj = Builders<ErrorLogDataAccess.DataClasses.ErrorFrequency>.Filter.Eq("ErrorCode", errorCode);

                        var updateObj = Builders<ErrorLogDataAccess.DataClasses.ErrorFrequency>.Update
                                        .Set("SearchFrequency", frequencyCount)
                                        .Set("LastUpdatedDate", formattedDate)
                                        .CurrentDate("lastModified");


                        errorFreqCollection.UpdateOne(filterObj, updateObj);

                    }
                


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
    }
}
