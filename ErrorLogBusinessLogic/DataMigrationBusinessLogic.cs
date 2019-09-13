using ErrorLogDataAccess;
using ErrorLogDataAccess.DataClasses;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ErrorLogBusinessLogic
{
    public class DataMigrationBusinessLogic
    {
        string mongoDbCompleteURL, mongoDBTargetDatabase, migrationMongoDbCompleteURL;
        IMongoDatabase mongoDatabaseRunTime, migrationMongoDbRunTime;

        MongoClient mongoClientCurrent, mongoClientTarget;

        public DataMigrationBusinessLogic()
        {
            #region "For Current Database"

            mongoClientCurrent = new MongoClient();

            mongoDbCompleteURL = System.Configuration.ConfigurationSettings.AppSettings["MongoDBStartURL"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBReplicaSet"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBAuthSource"].ToString();
            mongoDBTargetDatabase = System.Configuration.ConfigurationSettings.AppSettings["MongoDBDatabase"].ToString();

            //Connect to public Mongo DB repository
            mongoClientCurrent = new MongoClient(mongoDbCompleteURL);

            //Get Database
            mongoDatabaseRunTime = mongoClientCurrent.GetDatabase(mongoDBTargetDatabase);

            #endregion


   
        }

        public DataMigrationBusinessLogic(string targetMongoDbURL)
        {
            #region "For Current Database"

            mongoClientCurrent = new MongoClient();

            mongoDbCompleteURL = System.Configuration.ConfigurationSettings.AppSettings["MongoDBStartURL"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBReplicaSet"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBAuthSource"].ToString();
            mongoDBTargetDatabase = System.Configuration.ConfigurationSettings.AppSettings["MongoDBDatabase"].ToString();

            //Connect to public Mongo DB repository
            mongoClientCurrent = new MongoClient(mongoDbCompleteURL);

            //Get Database
            mongoDatabaseRunTime = mongoClientCurrent.GetDatabase(mongoDBTargetDatabase);

            #endregion


            #region "For migration/target database=>Database where the current data will be moved to"

            mongoClientTarget = new MongoClient();
            migrationMongoDbCompleteURL = targetMongoDbURL;

            //Connect to public Mongo DB repository
            mongoClientTarget = new MongoClient(migrationMongoDbCompleteURL);

            //Get Database
            migrationMongoDbRunTime = mongoClientTarget.GetDatabase(mongoDBTargetDatabase);



            #endregion
        }

        #region "Test whether the URL is working"

        public  bool IsDestinationURLValid()
        {
            try
            {
                bool isMongoLive = migrationMongoDbRunTime.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);

                if (isMongoLive)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
               
                throw;
            }
        }

        #endregion


        #region "Region to retrieve all collection/table data and migrate to the target db"

        //Retrieve all available tables/collections for selection
        public List<string> GetAllCollections()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    List<string> _CollectionList = new List<string>();
                    string collectionName;

                    //Loop through the CURENT database to get the collections
                    //Add the collections one by one to the string type List
                    foreach (BsonDocument collection in mongoDatabaseRunTime.ListCollectionsAsync().Result.ToListAsync<BsonDocument>().Result)
                    {
                        collectionName = collection["name"].AsString;
                       _CollectionList.Add(collectionName);
                    }

                   
                    scope1.Complete();
                    return _CollectionList;
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        //Push/Upload/Migrate data to the target database
        public bool PushDataToTargetDatabase(string[] collectionArr)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {


                    //First drop all the collections/tables in the target database
                    //Loop through the current database,but delete the collections in the target database

                    foreach (var collectionElem in collectionArr)
                    {

                        migrationMongoDbRunTime.DropCollection(collectionElem);
                        ChooseCollectionAndMigrate(collectionElem);

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

        public void ChooseCollectionAndMigrate(string collectionName)
        {
                try
                {

                    switch (collectionName)
                    {
                      case "Domain":
                        var domainCollection = migrationMongoDbRunTime.GetCollection<Domain>("Domain");
                        var domainCollectionList = mongoDatabaseRunTime.GetCollection<Domain>("Domain").AsQueryable().ToList();

                        if (domainCollectionList.Count > 0)
                        {
                            domainCollection.InsertMany(domainCollectionList);

                        }

                        break;

                    case "Field":
                        var fieldCollection = migrationMongoDbRunTime.GetCollection<Field>("Field");
                        var fieldCollectionList = mongoDatabaseRunTime.GetCollection<Field>("Field").AsQueryable().ToList();

                        if (fieldCollectionList.Count > 0)
                        {
                            fieldCollection.InsertMany(fieldCollectionList);

                        }

                        break;

                    case "Client":
                        var clientCollection = migrationMongoDbRunTime.GetCollection<Client>("Client");
                        var clientCollectionList = mongoDatabaseRunTime.GetCollection<Client>("Client").AsQueryable().ToList();

                        if (clientCollectionList.Count > 0)
                        {
                            clientCollection.InsertMany(clientCollectionList);

                        }

                        break;

                    case "ErrorFrequency":
                        var errorFreqCollection = migrationMongoDbRunTime.GetCollection<ErrorFrequency>("ErrorFrequency");
                        var errorFreqCollectionList = mongoDatabaseRunTime.GetCollection<ErrorFrequency>("ErrorFrequency").AsQueryable().ToList();

                        if (errorFreqCollectionList.Count > 0)
                        {
                            errorFreqCollection.InsertMany(errorFreqCollectionList);

                        }

                        break;

                    case "ErrorMaster":
                        var errorCollection = migrationMongoDbRunTime.GetCollection<ErrorMaster>("ErrorMaster");
                        var errorCollectionList = mongoDatabaseRunTime.GetCollection<ErrorMaster>("ErrorMaster").AsQueryable().ToList();

                        if (errorCollectionList.Count > 0)
                        {
                            errorCollection.InsertMany(errorCollectionList);

                        }

                        break;

                    case "SolutionMaster":
                        var solutionCollection = migrationMongoDbRunTime.GetCollection<SolutionMaster>("SolutionMaster");
                        var solutionCollectionList = mongoDatabaseRunTime.GetCollection<SolutionMaster>("SolutionMaster").AsQueryable().ToList();

                        if (solutionCollectionList.Count > 0)
                        {
                            solutionCollection.InsertMany(solutionCollectionList);

                        }

                        break;
                    case "ErrorLogger":
                        var runtimeErrorCollection = migrationMongoDbRunTime.GetCollection<ErrorLogger>("ErrorLogger");
                        var runtimeErrorCollectionList = mongoDatabaseRunTime.GetCollection<ErrorLogger>("ErrorLogger").AsQueryable().ToList();

                        if (runtimeErrorCollectionList.Count > 0)
                        {
                            runtimeErrorCollection.InsertMany(runtimeErrorCollectionList);

                        }

                        break;
                    case "SystemUser":
                        var userCollection = migrationMongoDbRunTime.GetCollection<SystemUser>("SystemUser");
                        var userCollectionList = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();

                        if (userCollectionList.Count > 0)
                        {
                            userCollection.InsertMany(userCollectionList);

                        }

                        break;
                    case "SystemUserGroup":
                        var userGroupCollection = migrationMongoDbRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");
                        var userGroupCollectionList = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup").AsQueryable().ToList();

                        if (userGroupCollectionList.Count > 0)
                        {
                            userGroupCollection.InsertMany(userGroupCollectionList);

                        }

                        break;
                    case "UserSessionLog":
                        var userSessionLogCollection = migrationMongoDbRunTime.GetCollection<UserSessionLog>("UserSessionLog");
                        var userSessionLogCollectionList = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog").AsQueryable().ToList();

                        if (userSessionLogCollectionList.Count > 0)
                        {
                            userSessionLogCollection.InsertMany(userSessionLogCollectionList);

                        }

                        break;
                    case "UserMessage":
                        var messageCollection = migrationMongoDbRunTime.GetCollection<UserMessage>("UserMessage");
                        var messageCollectionList = mongoDatabaseRunTime.GetCollection<UserMessage>("UserMessage").AsQueryable().ToList();

                        if (messageCollectionList.Count > 0)
                        {
                            messageCollection.InsertMany(messageCollectionList);

                        }

                        break;
                    default: break;                                  
                        
                                                 
                       

                }

            }
                catch (Exception ex)
                {
                    
                    throw;
                }


            
        }

       


        #endregion
    }
}
