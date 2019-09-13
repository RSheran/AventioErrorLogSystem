using ErrorLogDataAccess.DataClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErrorLogBusinessLogic
{
    public class TrendingErrorsBusinessLogic
    {
        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        public TrendingErrorsBusinessLogic()
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

     
        
        #region "Load Top Trending Errors=>Latest"

        public object GetLatestTopErrors()
        {
            try
            {              
               
                var errorList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                var domainList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();
                var fieldList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();
                var clientList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();
                var userList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.SystemUser>("SystemUser").AsQueryable().ToList();
                

                var resultArray = errorList.AsQueryable()
                              .Select(er => new
                              {
                                  er.ErrorCode,
                                  Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                  Field = fieldList.Where(a => a.FieldId == er.FieldId).Count()>0? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName:"N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                  Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                  er.ErrorCaption,
                                  er.ErrorDescription,
                                  er.ErrorLogDate,
                                  ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                  ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                  ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                                                

                              }).OrderByDescending(er => er.ErrorLogDate).Take(10).ToList();

                return resultArray;

            }

            catch (Exception ex)
            {
                throw;
            }

        }


        #endregion

        #region "Load Top Trending Errors=>Most Searched"

        public object GetMostSearchedTopErrors()
        {
            try
            {                               
                
                var errorList = mongoDatabaseRunTime.GetCollection<ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                var errorFreqList = mongoDatabaseRunTime.GetCollection<ErrorFrequency>("ErrorFrequency").AsQueryable().ToList();
                var domainList = mongoDatabaseRunTime.GetCollection<Domain>("Domain").AsQueryable().ToList();
                var fieldList = mongoDatabaseRunTime.GetCollection<Field>("Field").AsQueryable().ToList();
                var clientList = mongoDatabaseRunTime.GetCollection<Client>("Client").AsQueryable().ToList();
                var userList = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();


                var resultArray = errorList.AsQueryable()
                              .Select(er => new
                              {
                                  er.ErrorCode,
                                  ErrorFrequency= errorFreqList.Where(ef=>ef.ErrorCode.ToLower()==er.ErrorCode.ToLower()).Count()>0? errorFreqList.Where(ef => ef.ErrorCode.ToLower() == er.ErrorCode.ToLower()).FirstOrDefault().SearchFrequency:0,
                                  Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                  Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                  Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                  er.ErrorCaption,
                                  er.ErrorDescription,
                                  er.ErrorLogDate,
                                  ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                  ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                  ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,


                              }).Where(er=>er.ErrorFrequency>0).OrderByDescending(er=>er.ErrorFrequency).ThenByDescending(er => er.ErrorLogDate).Take(10).ToList();

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
