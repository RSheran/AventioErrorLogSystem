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
   
    public class DomainBusinessLogic
    {

       string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        public DomainBusinessLogic()
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

        #region "Functions to add new domain"

        //To check product category code is available
        public bool IsDomainAvailable(string domainCode)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    
                    //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var domainCollection = mongoDatabaseRunTime.GetCollection<Domain>("Domain");



                    if (domainCollection.AsQueryable()
                      .Where(d => d.DomainCode.ToLower() == domainCode.ToLower()).Count() > 0)
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

        //Function to get new Domain ID
        public int GetNewDomainID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                
                    int newDomainID = 0;

                    var domainCollection = mongoDatabaseRunTime.GetCollection<Domain>("Domain");



                    if (domainCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = domainCollection.AsQueryable()
                                   .OrderByDescending(a => a.DomainId)
                                   .FirstOrDefault().DomainId;

                        newDomainID = maxID + 1;

                        
                    }
                    else
                    {
                        scope1.Complete();
                        newDomainID= 1;
                    }

                    return newDomainID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }


        //save using a 'Domain' type variable
        public bool SaveDomain(ErrorLogDataAccess.DataClasses.Domain domainObj,int userId=1)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {
                   
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                

                    var DomainCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain");


                    var domainRow = new ErrorLogDataAccess.DataClasses.Domain()
                    {
                        DomainId = GetNewDomainID(),
                        DomainCode = domainObj.DomainCode,
                        DomainName= domainObj.DomainName,
                        DomainDescription = domainObj.DomainDescription,
                        IsActive = domainObj.IsActive,
                        EntryDate=formattedDate,
                        UserId=1

                    };

                    DomainCollection.InsertOne(domainRow);

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


        #region "Functions to update domain"

        public bool UpdateDomain(ErrorLogDataAccess.DataClasses.Domain domainObj)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {
                  
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                  

                    var domainCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain");
                                       

                    var filterObj = Builders<ErrorLogDataAccess.DataClasses.Domain>.Filter.Eq("DomainCode", domainObj.DomainCode);
                                    
                    var updateObj = Builders<ErrorLogDataAccess.DataClasses.Domain>.Update
                                 .Set("DomainName", domainObj.DomainName)
                                 .Set("DomainDescription", domainObj.DomainDescription)
                                 .Set("IsActive", domainObj.IsActive)
                                 .Set("LastUpdatedDate", formattedDate)
                               .CurrentDate("lastModified");
                                     
                    
                    domainCollection.UpdateOne(filterObj, updateObj);
                    

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

        #region"Functions to change status of domain"
        //Update status of selected expense types
        public bool UpdateStatusOfSelectedDomains(Domain[] domainObj)
        {
            
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                 
                    var domainCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain");
                                   

                    if (domainObj.Count() != 0)
                    {
                        foreach (var domainCurr in domainObj)
                        {
                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.Domain>.Filter.Eq("DomainCode", domainCurr.DomainCode);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.Domain>.Update
                                            .Set("IsActive", domainCurr.IsActive)
                                            .Set("LastUpdatedDate", formattedDate)
                                            .CurrentDate("lastModified");


                            domainCollection.UpdateOne(filterObj, updateObj);
                            
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

        public bool ChangeStatusForAllDomains(bool isActiveForAll)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    
                    var domainCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain");
                    var domainList = domainCollection.AsQueryable().ToList();

                 

                    if (domainList.Count() != 0)
                    {
                        foreach (var domainCurr in domainList)
                        {
                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.Domain>.Filter.Eq("DomainCode", domainCurr.DomainCode);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.Domain>.Update
                                            .Set("IsActive", isActiveForAll)
                                            .Set("LastUpdatedDate", formattedDate)
                                            .CurrentDate("lastModified");


                            domainCollection.UpdateOne(filterObj, updateObj);

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

        #region "Load Domain/Domain Details"

        public Array GetAllDomains(int isActive)
        {
            
            try
            {
               
               

                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();
                if (isActive == 0)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.DomainCode != null)
                                     .Select(p => new
                                     {
                                         p.DomainId,
                                         p.DomainCode,
                                         p.DomainName,
                                         p.DomainDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.DomainCode).ToArray();

                    return resultArray;


                }

                else if (isActive == 1)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.DomainCode != null && w.IsActive==true)
                                     .Select(p => new
                                     {
                                         p.DomainId,
                                         p.DomainCode,
                                         p.DomainName,
                                         p.DomainDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.DomainCode).ToArray();

                    return resultArray;
                }

                else
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.DomainCode != null && w.IsActive == false)
                                     .Select(p => new
                                     {
                                         p.DomainId,
                                         p.DomainCode,
                                         p.DomainName,
                                         p.DomainDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.DomainCode).ToArray();

                    return resultArray;
                }

                return null;
            }

            catch (Exception ex)
            {
                throw;
            }

        }


        //Get details for a given domain  code
        public Array GetDomainDetailsForCode(string domainCode)
        {

            try
            {
                                             

                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.DomainCode.ToLower().Trim()==domainCode.ToLower().Trim())
                                     .Select(p => new
                                     {
                                         p.DomainCode,
                                         p.DomainName,
                                         p.DomainDescription,
                                         p.IsActive,
                                         p.UserId
                                     }).OrderBy(p => p.DomainCode).ToArray();

                    return resultArray;

            }

            catch (Exception ex)
            {
                throw;
            }

        }

        //Get domain codes for autosuggestion(Top 10)
        public Array GetDomainCodesForAutoComplete(string domainCode)
        {
            
            try
            {
                           

                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.DomainCode.ToLower().Contains(domainCode.ToLower().Trim()))
                                     .Select(p => new
                                     {
                                         p.DomainCode,
                                         p.DomainName
                                        
                                     }).OrderBy(p => p.DomainCode).Take(10).ToArray();

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
