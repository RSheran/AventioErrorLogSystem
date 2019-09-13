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

    public class ClientBusinessLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        public ClientBusinessLogic()
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

        

        #region "Functions to add new Client"

        //To check product category code is available
        public bool IsClientAvailable(string ClientCode)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    
                    //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var ClientCollection = mongoDatabaseRunTime.GetCollection<Client>("Client");



                    if (ClientCollection.AsQueryable()
                      .Where(d => d.ClientCode.ToLower() == ClientCode.ToLower()).Count() > 0)
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

        //Function to get new Client ID
        public int GetNewClientID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                   
                    int newClientID = 0;

                    var ClientCollection = mongoDatabaseRunTime.GetCollection<Client>("Client");
                    
                    if (ClientCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = ClientCollection.AsQueryable()
                                   .OrderByDescending(a => a.ClientId)
                                   .FirstOrDefault().ClientId;

                        newClientID = maxID + 1;


                    }
                    else
                    {
                        scope1.Complete();
                        newClientID = 1;
                    }

                    return newClientID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }


        //save using a 'Client' type variable
        public bool SaveClient(ErrorLogDataAccess.DataClasses.Client ClientObj, int userId = 1)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                  

                    var ClientCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client");


                    var ClientRow = new ErrorLogDataAccess.DataClasses.Client()
                    {
                        ClientId = GetNewClientID(),
                        ClientCode = ClientObj.ClientCode,
                        ClientName = ClientObj.ClientName,
                        ClientDescription = ClientObj.ClientDescription,
                        IsActive = ClientObj.IsActive,
                        EntryDate = formattedDate
                    };

                    ClientCollection.InsertOne(ClientRow);

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


        #region "Functions to update Client"

        public bool UpdateClient(ErrorLogDataAccess.DataClasses.Client ClientObj)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                        

                    var ClientCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client");


                    var filterObj = Builders<ErrorLogDataAccess.DataClasses.Client>.Filter.Eq("ClientCode", ClientObj.ClientCode);

                    var updateObj = Builders<ErrorLogDataAccess.DataClasses.Client>.Update
                                 .Set("ClientName", ClientObj.ClientName)
                                 .Set("ClientDescription", ClientObj.ClientDescription)
                                 .Set("IsActive", ClientObj.IsActive)
                                 .Set("LastUpdatedDate", formattedDate)
                               .CurrentDate("lastModified");


                    ClientCollection.UpdateOne(filterObj, updateObj);


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

        #region"Functions to change status of Client"
        //Update status of selected expense types
        public bool UpdateStatusOfSelectedClients(Client[] ClientObj)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                   

                    var ClientCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client");

                    
                    if (ClientObj.Count() != 0)
                    {
                        foreach (var ClientCurr in ClientObj)
                        {
                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.Client>.Filter.Eq("ClientCode", ClientCurr.ClientCode);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.Client>.Update
                                            .Set("IsActive", ClientCurr.IsActive)
                                            .Set("LastUpdatedDate", formattedDate)
                                            .CurrentDate("lastModified");


                            ClientCollection.UpdateOne(filterObj, updateObj);

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

        public bool ChangeStatusForAllClients(bool isActiveForAll)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                                     

                    var ClientCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client");
                    var ClientList = ClientCollection.AsQueryable().ToList();



                    if (ClientList.Count() != 0)
                    {
                        foreach (var ClientCurr in ClientList)
                        {
                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.Client>.Filter.Eq("ClientCode", ClientCurr.ClientCode);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.Client>.Update
                                            .Set("IsActive", isActiveForAll)
                                            .Set("LastUpdatedDate", formattedDate)
                                            .CurrentDate("lastModified");


                            ClientCollection.UpdateOne(filterObj, updateObj);

                        }


                        scope1.Complete();


                    }


                    return true;
                }
                catch (Exception ex)
                {
                    //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                    scope1.Dispose();
                    return false;
                }
            }
        }


        #endregion

        #region "Load Client/Client Details"

        public Array GetAllClients(int isActive)
        {

            try
            {


                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();
                if (isActive == 0)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.ClientCode != null)
                                     .Select(p => new
                                     {
                                         p.ClientId,
                                         p.ClientCode,
                                         p.ClientName,
                                         p.ClientDescription,
                                         p.IsActive
                                     }).OrderBy(p => p.ClientCode).ToArray();

                    return resultArray;


                }

                else if (isActive == 1)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.ClientCode != null && w.IsActive == true)
                                     .Select(p => new
                                     {
                                         p.ClientId,
                                         p.ClientCode,
                                         p.ClientName,
                                         p.ClientDescription,
                                         p.IsActive
                                     }).OrderBy(p => p.ClientCode).ToArray();

                    return resultArray;
                }

                else
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.ClientCode != null && w.IsActive == false)
                                     .Select(p => new
                                     {
                                         p.ClientId,
                                         p.ClientCode,
                                         p.ClientName,
                                         p.ClientDescription,
                                         p.IsActive
                                     }).OrderBy(p => p.ClientCode).ToArray();

                    return resultArray;
                }

                return null;
            }

            catch (Exception ex)
            {
                throw;
            }

        }


        //Get details for a given Client  code
        public Array GetClientDetailsForCode(string ClientCode)
        {

            try
            {


                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.ClientCode.ToLower().Trim() == ClientCode.ToLower().Trim())
                                     .Select(p => new
                                     {
                                         p.ClientCode,
                                         p.ClientName,
                                         p.ClientDescription,
                                         p.IsActive
                                     }).OrderBy(p => p.ClientCode).ToArray();

                return resultArray;

            }

            catch (Exception ex)
            {
                throw;
            }

        }

        //Get Client codes for autosuggestion(Top 10)
        public Array GetClientCodesForAutoComplete(string ClientCode)
        {

            try
            {
           

                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.ClientCode.ToLower().Contains(ClientCode.ToLower().Trim()))
                                     .Select(p => new
                                     {
                                         p.ClientCode,
                                         p.ClientName

                                     }).OrderBy(p => p.ClientCode).Take(10).ToArray();

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
