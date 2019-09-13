using ErrorLogDataAccess.DataClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace AventioErrorLogSystem.Common
{
    public class ErrorLogHelper
    {
        static MongoClient mongoClient;
        static string mongoDBTargetDatabase = System.Configuration.ConfigurationSettings.AppSettings["MongoDBDatabase"].ToString();

        //Connect to public Mongo DB repository
        public static MongoClient InitializeMongoClient()
        {
            try
            {
                string mongoDbCompleteURL = System.Configuration.ConfigurationSettings.AppSettings["MongoDBStartURL"].ToString() +
                                             "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBReplicaSet"].ToString() +
                                             "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBAuthSource"].ToString();
                mongoClient = new MongoClient("mongodb://rajindra:rajindra123@cluster1-shard-00-00-w6nfe.mongodb.net:27017,cluster1-shard-00-01-w6nfe.mongodb.net:27017,cluster1-shard-00-02-w6nfe.mongodb.net:27017/test?ssl=true&replicaSet=Cluster1-shard-0&authSource=admin");


                return mongoClient;

            }
            catch (Exception ex)
            {
                return null;
            }



        }

       

        public static void UpdatingErrorLog(string schema, string uName, Exception ex)
        {
            var logFilePath = System.Configuration.ConfigurationSettings.AppSettings["AventioErrorLog_Error_Path"];

            string exString = String.Empty;

            if (ex.InnerException == null)
            {
                exString = ex.Message.ToString();

            }
            else if (ex.InnerException.InnerException != null)
            {
                exString = ex.InnerException.InnerException.GetBaseException().ToString();
            }
            else
            {
                exString = ex.InnerException.ToString();
            }


            if (Directory.Exists(logFilePath))
            {
                var FileName = "AventioErrrLog_ErrorLog" + "_" + DateTime.Now.Year + "_" + DateTime.Now.Month + "_" + DateTime.Now.Day + ".txt";
                var fullPath = logFilePath + FileName;
                string errorLogContent = "-----------------------------------------------------------------------" + Environment.NewLine +
                                            "Time Stamp     : " + DateTime.Now.ToLongTimeString() + Environment.NewLine +
                                            "Schema         : " + schema + Environment.NewLine +
                                            "Username       : " + uName + Environment.NewLine +
                                            "Exception/Error: " + exString + Environment.NewLine +
                                            "---------------------------------------------------------------------" + Environment.NewLine;

                //if (File.Exists(fullPath))
                //{

                File.AppendAllText(fullPath, errorLogContent);

                //}
                //else
                //{

                //    UpdatingErrorLog(schema,uName,exception);
                //}
            }

            
            //Save the error in mongoDb databse too
            MongoClient mongoClient = InitializeMongoClient();
            var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

            var ErrorCollection = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.ErrorLogger>("ErrorLogger");

            var errorLogger = new ErrorLogDataAccess.DataClasses.ErrorLogger()
            {
                ErrorLoggerId= GetNewErrorLogID(),
                TimeStamp  = DateTime.Now.ToString(),
                Schema = schema,
                Username = uName,
                Exception_Error = exString
            };

            ErrorCollection.InsertOne(errorLogger);

        }

         //Function to get new Domain ID
        public static int GetNewErrorLogID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    // Get the mongo db database.
                    mongoClient = InitializeMongoClient();
                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);
                    int newErrorLogID = 0;

                    var errorLogCollection = mongoDatabase.GetCollection<ErrorLogger>("ErrorLogger");



                    if (errorLogCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = errorLogCollection.AsQueryable()
                                   .OrderByDescending(a => a.ErrorLoggerId)
                                   .FirstOrDefault().ErrorLoggerId;

                        newErrorLogID = maxID + 1;


                    }
                    else
                    {
                        scope1.Complete();
                        newErrorLogID = 1;
                    }

                    return newErrorLogID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

    }
}
