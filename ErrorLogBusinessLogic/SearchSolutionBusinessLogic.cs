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
    public class SearchSolutionBusinessLogic
    {
        string mongoDbCompleteURL, mongoDBTargetDatabase;

        MongoClient mongoClient;

        public SearchSolutionBusinessLogic()
        {
            mongoClient = new MongoClient();

            mongoDbCompleteURL = System.Configuration.ConfigurationSettings.AppSettings["MongoDBStartURL"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBReplicaSet"].ToString() +
                                "&" + System.Configuration.ConfigurationSettings.AppSettings["MongoDBAuthSource"].ToString();
            mongoDBTargetDatabase = System.Configuration.ConfigurationSettings.AppSettings["MongoDBDatabase"].ToString();


        }

        //Connect to public Mongo DB repository
        public MongoClient InitializeMongoClient()
        {
            try
            {

                mongoClient = new MongoClient(mongoDbCompleteURL);
                //("mongodb://rajindra:rajindra123@cluster1-shard-00-00-w6nfe.mongodb.net:27017,cluster1-shard-00-01-w6nfe.mongodb.net:27017,cluster1-shard-00-02-w6nfe.mongodb.net:27017/test?ssl=true&replicaSet=Cluster1-shard-0&authSource=admin");

                return mongoClient;

            }
            catch (Exception ex)
            {
                throw;
            }



        }

        #region "Get Error Details for explicit parameters"

        public object GetExistingErrorListForParam(int domainId,int fieldId,int clientId)
        {
            try
            {

                MongoClient mongoClient = InitializeMongoClient();

                var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                var errorList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                var domainList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();
                var fieldList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();
                var clientList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();
                var userList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SystemUser>("SystemUser").AsQueryable().ToList();
                var solutionList= mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionMaster>("SolutionMaster").AsQueryable().ToList();
                var solutionFeedbackList= mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionFeedback>("SolutionFeedback").AsQueryable().ToList();

                //Expand the solutionFeedbackList
                var expandedSolutionFeedbackList = solutionFeedbackList
                                                  .Select(sf => new
                                                  {
                                                      sf.SolutionFeedbackId,
                                                      sf.SolutionId,
                                                      sf.UserId,
                                                      FeedbackUsername = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().Username,
                                                      FeedbackUserFullName = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().FullName,
                                                      FeedbackUserImageURL = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().ProfilePicURL,
                                                      sf.IsSolutionCorrect,
                                                      sf.FeedBackComment,
                                                      sf.FeedBackDate

                                                  }).ToList();

                //Expand the  solutionList and map the above expanded solutionFeedback List 
                //against the solutionId
                var expandedSolutionList = solutionList
                                           .Select(a => new
                                            {
                                               a.SolutionId,
                                               a.ErrorId,
                                               a.SolutionCode,
                                               a.SolutionLogDate,
                                               a.SolutionComment,
                                               SolutionUsername = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().Username,
                                               SolutionUserFullName = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().FullName,
                                               SolutionUserImageURL = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().ProfilePicURL,
                                               a.ErrorAttachments,
                                               SolutionFeedbackList = expandedSolutionFeedbackList.Where(c=>c.SolutionId==a.SolutionId).Count() == 0 ? null :
                                                                      expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).ToList()
                                                                           

                                           }).ToList();



                if (domainId != 0 && fieldId != 0 && clientId != 0)
                {
                    var resultArray = errorList.AsQueryable()
                                   .Where(a=>a.DomainId==domainId && a.FieldId==fieldId &&  a.ClientId==clientId)
                                  .Select(er => new
                                  {
                                      er.ErrorCode,
                                      Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                      Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                      Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                      er.ErrorCaption,
                                      er.ErrorDescription,
                                      er.ErrorLogDate,
                                      ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                      ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                      ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                      er.ErrorAttachments,
                                      SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                        expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() : null
                                      // SolutionFeedbackList= solutionFeedbackList


                                  }).OrderBy(er => er.ErrorCode).ToList();

                    return resultArray;
                }
                else if (domainId != 0 && fieldId != 0 && clientId == 0)
                {
                    var resultArray = errorList.AsQueryable()
                                 .Where(a => a.DomainId == domainId && a.FieldId == fieldId)
                                .Select(er => new
                                {
                                    er.ErrorCode,
                                    Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                    Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                    Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                    er.ErrorCaption,
                                    er.ErrorDescription,
                                    er.ErrorLogDate,
                                    ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                    ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                    ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                    er.ErrorAttachments,
                                    SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                       expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() : null
                                    //SolutionFeedbackList = solutionFeedbackList

                                }).OrderBy(er => er.ErrorCode).ToList();

                    return resultArray;
                }
                else if (domainId != 0 && fieldId == 0 && clientId != 0)
                {
                    var resultArray = errorList.AsQueryable()
                                 .Where(a => a.DomainId == domainId && a.ClientId == clientId)
                                .Select(er => new
                                {
                                    er.ErrorCode,
                                    Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                    Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                    Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                    er.ErrorCaption,
                                    er.ErrorDescription,
                                    er.ErrorLogDate,
                                    ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                    ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                    ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                    er.ErrorAttachments,
                                    SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                            expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() : null
                                    // SolutionFeedbackList = solutionFeedbackList

                                }).OrderBy(er => er.ErrorCode).ToList();

                    return resultArray;
                }
                else if (domainId != 0 && fieldId == 0 && clientId == 0)
                {
                    var resultArray = errorList.AsQueryable()
                                 .Where(a => a.DomainId == domainId)
                                .Select(er => new
                                {
                                    er.ErrorCode,
                                    Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                    Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                    Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                    er.ErrorCaption,
                                    er.ErrorDescription,
                                    er.ErrorLogDate,
                                    ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                    ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                    ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                    er.ErrorAttachments,
                                    SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                            expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() : null
                                    //SolutionFeedbackList = solutionFeedbackList

                                }).OrderBy(er => er.ErrorCode).ToList();

                    return resultArray;
                }

                else if (domainId == 0 && fieldId == 0 && clientId == 0)
                {
                    var resultArray = errorList.AsQueryable()
                                      .Select(er => new
                                      {
                                          er.ErrorCode,
                                          Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                          Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                          Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                          er.ErrorCaption,
                                          er.ErrorDescription,
                                          er.ErrorLogDate,
                                          ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                          ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                          ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                          er.ErrorAttachments,
                                          SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() >0 ?
                                                            expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() :null

                                      }).OrderBy(er => er.ErrorCode).ToList();


                    return resultArray;
                }

                return null;





            }

            catch (Exception ex)
            {
                throw;
            }

        }

       
        public object GetExistingErrorListForErrorCode(string errorCode)
        {
            try
            {

                MongoClient mongoClient = InitializeMongoClient();

                var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                var errorList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                var domainList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();
                var fieldList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();
                var clientList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();
                var userList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SystemUser>("SystemUser").AsQueryable().ToList();
                var solutionList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionMaster>("SolutionMaster").AsQueryable().ToList();
                var solutionFeedbackList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionFeedback>("SolutionFeedback").AsQueryable().ToList();

                //Get the error Id for the error code
                int errorId = errorList.Where(ei => ei.ErrorCode.ToLower().Trim() == errorCode.ToLower().Trim())
                             .FirstOrDefault().ErrorId;

                //Expand the solutionFeedbackList
                var expandedSolutionFeedbackList = solutionFeedbackList
                                                  .Select(sf => new
                                                  {
                                                      sf.SolutionFeedbackId,
                                                      sf.SolutionId,
                                                      sf.UserId,
                                                      FeedbackUsername = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().Username,
                                                      FeedbackUserFullName = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().FullName,
                                                      FeedbackUserImageURL = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().ProfilePicURL,
                                                      sf.IsSolutionCorrect,
                                                      sf.FeedBackComment,
                                                      sf.FeedBackDate

                                                  }).ToList();

                //Expand the  solutionList and map the above expanded solutionFeedback List 
                //against the solutionId
                var expandedSolutionList = solutionList
                                           .Where(sol=>sol.ErrorId==errorId)
                                           .Select(a => new
                                           {
                                               a.SolutionId,
                                               a.ErrorId,
                                               a.SolutionCode,
                                               a.SolutionLogDate,
                                               a.SolutionComment,
                                               SolutionUsername = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().Username,
                                               SolutionUserFullName = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().FullName,
                                               SolutionUserImageURL = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().ProfilePicURL,
                                               a.ErrorAttachments,
                                               SolutionFeedbackList = expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).Count() == 0 ? null :
                                                                      expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).ToList(),
                                               
                                               //This field will store how many 'Yes' feedback scenarios  were there for the solution
                                               //In other words,this will store how many users verified the solution as correct
                                               //This will be used to put the 'Verified' tick to the most suitable solution
                                               VerifiedCount = expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).Count() == 0 ? 0 :
                                                               expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId && c.IsSolutionCorrect==true).Count(),

                                           }).ToList();



                var resultArray = errorList.AsQueryable()
                                       .Where(res=>res.ErrorId==errorId)
                                       .Select(er => new
                                       {
                                           er.ErrorCode,
                                           Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                           Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                           Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                           er.ErrorCaption,
                                           er.ErrorDescription,
                                           
                                           ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                           ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                           ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                           er.ErrorAttachments,
                                           SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                             expandedSolutionList.Where(es => es.ErrorId == er.ErrorId)
                                                             .OrderByDescending(k=>k.VerifiedCount).ThenByDescending(k=>k.SolutionLogDate).ToList()
                                                            : null

                                       }).OrderBy(er => er.ErrorCode).ToList();


                return resultArray;





            }

            catch (Exception ex)
            {
                throw;
            }

        }

        public object GetAllErrors()
        {
            try
            {

                MongoClient mongoClient = InitializeMongoClient();

                var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                var errorList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                var domainList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();
                var fieldList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();
                var clientList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();
                var userList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SystemUser>("SystemUser").AsQueryable().ToList();
                var solutionList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionMaster>("SolutionMaster").AsQueryable().ToList();
                var solutionFeedbackList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionFeedback>("SolutionFeedback").AsQueryable().ToList();


                //Expand the solutionFeedbackList
                var expandedSolutionFeedbackList = solutionFeedbackList
                                                  .Select(sf => new
                                                  {
                                                      sf.SolutionFeedbackId,
                                                      sf.SolutionId,
                                                      sf.UserId,
                                                      FeedbackUsername = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().Username,
                                                      FeedbackUserFullName = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().FullName,
                                                      FeedbackUserImageURL = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().ProfilePicURL,
                                                      sf.IsSolutionCorrect,
                                                      sf.FeedBackComment,
                                                      sf.FeedBackDate

                                                  }).ToList();

                //Expand the  solutionList and map the above expanded solutionFeedback List 
                //against the solutionId
                var expandedSolutionList = solutionList
                                           .Select(a => new
                                           {
                                               a.SolutionId,
                                               a.ErrorId,
                                               a.SolutionCode,
                                               a.SolutionLogDate,
                                               a.SolutionComment,
                                               SolutionUsername = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().Username,
                                               SolutionUserFullName = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().FullName,
                                               SolutionUserImageURL = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().ProfilePicURL,
                                               a.ErrorAttachments,
                                               SolutionFeedbackList = expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).Count() == 0 ? null :
                                                                      expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).ToList()


                                           }).ToList();



                    var resultArray = errorList.AsQueryable()
                                  .Select(er => new
                                  {
                                      er.ErrorCode,
                                      Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                      Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                      Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                      er.ErrorCaption,
                                      er.ErrorDescription,
                                      er.ErrorLogDate,
                                      ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                      ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                      ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                      er.ErrorAttachments,
                                      SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                        expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() : null

                                  }).OrderByDescending(er => er.ErrorLogDate).Take(10).ToList();

                        return resultArray;

                }

            catch (Exception ex)
            {
                throw;
            }

        }


        public object GetAllErrorsForSearchQuery(string searchQuery)
        {
            try
            {

                MongoClient mongoClient = InitializeMongoClient();

                var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                var errorList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                var domainList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();
                var fieldList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Field>("Field").AsQueryable().ToList();
                var clientList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();
                var userList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SystemUser>("SystemUser").AsQueryable().ToList();
                var solutionList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionMaster>("SolutionMaster").AsQueryable().ToList();
                var solutionFeedbackList = mongoDatabase.GetCollection<ErrorLogDataAccess.DataClasses.SolutionFeedback>("SolutionFeedback").AsQueryable().ToList();

                
                //Expand the solutionFeedbackList
                var expandedSolutionFeedbackList = solutionFeedbackList
                                                  .Select(sf => new
                                                  {
                                                      sf.SolutionFeedbackId,
                                                      sf.SolutionId,
                                                      sf.UserId,
                                                      FeedbackUsername = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().Username,
                                                      FeedbackUserFullName = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().FullName,
                                                      FeedbackUserImageURL = userList.Where(u => u.UserId == sf.UserId).FirstOrDefault().ProfilePicURL,
                                                      sf.IsSolutionCorrect,
                                                      sf.FeedBackComment,
                                                      sf.FeedBackDate

                                                  }).ToList();

                //Expand the  solutionList and map the above expanded solutionFeedback List 
                //against the solutionId
                var expandedSolutionList = solutionList
                                           .Select(a => new
                                           {
                                               a.SolutionId,
                                               a.ErrorId,
                                               a.SolutionCode,
                                               a.SolutionLogDate,
                                               a.SolutionComment,
                                               SolutionUsername = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().Username,
                                               SolutionUserFullName = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().FullName,
                                               SolutionUserImageURL = userList.Where(u => u.UserId == a.UserId).FirstOrDefault().ProfilePicURL,
                                               a.ErrorAttachments,
                                               SolutionFeedbackList = expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).Count() == 0 ? null :
                                                                      expandedSolutionFeedbackList.Where(c => c.SolutionId == a.SolutionId).ToList()


                                           }).ToList();


                if (searchQuery!="all")
                {
                    var resultArray = errorList.AsQueryable()
                                   .Select(er => new
                                   {
                                       er.ErrorCode,
                                       Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                       Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                       Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                       er.ErrorCaption,
                                       er.ErrorDescription,
                                       er.ErrorLogDate,
                                       ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                       ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                       ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                       er.ErrorAttachments,
                                       SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                                 expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() : null

                                   }).Where(c => (c.ErrorCode.ToLower()==searchQuery.ToLower())|| (c.ErrorCode.ToLower().Contains(searchQuery.ToLower()))|| (c.ErrorCaption.ToLower().Contains(searchQuery.ToLower())) ||
                                           (c.ErrorDescription.ToLower().Contains(searchQuery.ToLower())) ||
                                           (c.Domain.ToLower().Contains(searchQuery.ToLower())) ||
                                           (c.Field.ToLower().Contains(searchQuery.ToLower())) ||
                                           (c.Client.ToLower().Contains(searchQuery.ToLower())))
                                           .OrderByDescending(er => er.ErrorLogDate).Take(20).ToList();



                    return resultArray;
                }
                else //If search query is empty,the load the latest top 10 errors
                {

                    var resultArray = errorList.AsQueryable()
                                  .Select(er => new
                                  {
                                      er.ErrorCode,
                                      Domain = er.DomainId != null ? domainList.Where(a => a.DomainId == er.DomainId).FirstOrDefault().DomainName : "",
                                      Field = fieldList.Where(a => a.FieldId == er.FieldId).Count() > 0 ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "N/A",//er.FieldId != null ? fieldList.Where(a => a.FieldId == er.FieldId).FirstOrDefault().FieldName : "",
                                      Client = clientList.Where(a => a.ClientId == er.ClientId).Count() > 0 ? clientList.Where(a => a.ClientId == er.ClientId).FirstOrDefault().ClientName : "N/A",
                                      er.ErrorCaption,
                                      er.ErrorDescription,
                                      er.ErrorLogDate,
                                      ErrorUsername = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().Username,
                                      ErrorFullName = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().FullName,
                                      ErrorUserImageURL = userList.Where(u => u.UserId == er.UserId).FirstOrDefault().ProfilePicURL,
                                      er.ErrorAttachments,
                                      SolutionMasters = expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).Count() > 0 ?
                                                        expandedSolutionList.Where(es => es.ErrorId == er.ErrorId).ToList() : null

                                  }).OrderByDescending(er => er.ErrorLogDate).Take(10).ToList();

                    return resultArray;

                }





            }

            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion

        #region "Save User Feedback for the solution"

        //Function to get new Solution Feedback ID
        public int GetNewSolutionFeedbackID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    // Get the mongo db database.
                    mongoClient = InitializeMongoClient();
                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);
                    int newSolFeedbackID = 0;

                    var solFeedbackCollection = mongoDatabase.GetCollection<SolutionFeedback>("SolutionFeedback");



                    if (solFeedbackCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = solFeedbackCollection.AsQueryable()
                                   .OrderByDescending(a => a.SolutionFeedbackId)
                                   .FirstOrDefault().SolutionFeedbackId;

                        newSolFeedbackID = maxID + 1;


                    }
                    else
                    {
                        scope1.Complete();
                        newSolFeedbackID = 1;
                    }

                    return newSolFeedbackID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public bool SaveSolutionFeedback(SolutionFeedback solFeedbackObj, int userId,string solutionCode)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {


                try
                {
                    mongoClient = InitializeMongoClient();
                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    var SolutionCollection = mongoDatabase.GetCollection<SolutionMaster>("SolutionMaster");
                    var SolutionFeedbackCollection = mongoDatabase.GetCollection<SolutionFeedback>("SolutionFeedback");

                    //Get the solution Id for the current solution Code
                    int solutionId = SolutionCollection.AsQueryable()
                                    .Where(s => s.SolutionCode.ToLower()== solutionCode.ToLower())
                                    .FirstOrDefault().SolutionId;

                    var solutionFeedbackRow = new SolutionFeedback()
                    {
                        SolutionFeedbackId = GetNewSolutionFeedbackID(),
                        UserId = userId,
                        SolutionId = solutionId,
                        IsSolutionCorrect = solFeedbackObj.IsSolutionCorrect,
                        FeedBackComment = solFeedbackObj.FeedBackComment,
                        FeedBackDate = formattedDate,
                        lastModified = DateTime.Now

                    };

                    SolutionFeedbackCollection.InsertOne(solutionFeedbackRow);

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


    }
}
