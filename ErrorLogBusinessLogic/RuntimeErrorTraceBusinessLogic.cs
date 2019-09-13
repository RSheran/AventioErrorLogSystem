using ErrorLogDataAccess.DataClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ErrorLogBusinessLogic
{
    public class RuntimeErrorTraceBusinessLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        public RuntimeErrorTraceBusinessLogic()
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


        
        #region "Functions to retrieve error trace"

        public Array GetAllRuntimeErrors()
        {

            try
            {
   
                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.ErrorLogger>("ErrorLogger").AsQueryable().ToList();
                var resultArray = resultList.AsQueryable()
                                  .Select(el => new
                                    {
                                        el.ErrorLoggerId,
                                        el.TimeStamp,
                                        el.Schema,
                                        el.Username,
                                        el.Exception_Error
                                    }).OrderBy(el => el.TimeStamp).ToArray();

                return resultArray;
            }

            catch (Exception ex)
            {
                throw;
            }

        }


        #endregion

        #region "Delete selcted error traces/ all error traces"

        public bool DeleteSelectedErrorTraces(ErrorLogger[] errorTraceArr)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {

                    var errorLoggerCollection = mongoDatabaseRunTime.GetCollection<ErrorLogger>("ErrorLogger");

                    foreach (var errorTraceCurr in errorTraceArr)
                    {
                            int errorLoggerID = Convert.ToInt32(errorTraceCurr.ErrorLoggerId);
                            var filterObj = Builders<ErrorLogger>.Filter.Eq("ErrorLoggerId", errorLoggerID);

                            errorLoggerCollection.DeleteOne(filterObj);

                    }


                    scope1.Complete();


                    


                    return true;
                }
                catch (Exception ex)
                {
                    //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    scope1.Dispose();
                    throw ex;
                }
            }
        }

        public bool DeleteAllErrorTraces()
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {

                    var errorLoggerCollection = mongoDatabaseRunTime.GetCollection<ErrorLogger>("ErrorLogger");


                    mongoDatabaseRunTime.DropCollection("ErrorLogger");

                    scope1.Complete();



                    return true;
                }
                catch (Exception ex)
                {
                    //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    scope1.Dispose();
                    throw ex;
                }
            }
        }




        #endregion



    }
}
