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
    public class UserSessionLogBusinessLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        private UserDetailsBusinessLogic _UserDetailsBusinessLogic;

        public UserSessionLogBusinessLogic()
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

        #region "Get Logged In Sessions"

        //To get all logged in sessions
        public List<UserSessionLogDTO> GetAllLoggedInSessions(int userId)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {

                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");
                    var userGroupCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");
                    var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");
                    var userGroupList = userGroupCollection.AsQueryable().ToList();
                    var userSessionLogList = userSessionLogCollection.AsQueryable().ToList();
                    var userCollectionList = userCollection.AsQueryable().ToList();

                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                    //Load session log or all users if userId=0
                    //Else load the session log for the selected user
                    if (userId == 0)
                    {
                        var userLogData = userSessionLogList
                                        .Select(ul => new UserSessionLogDTO
                                        {
                                            UserSessionLogId = ul.UserSessionLogId,
                                            UserId = ul.UserId,
                                            ProfilePicURL = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().ProfilePicURL,
                                            Username = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().Username,
                                            UserCallingName = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().CallingName,
                                            UserFullName = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().FullName,
                                            UserGroupName = userGroupList.Where(a => a.UserGroupId == userCollectionList.Where(b => b.UserId == ul.UserId).FirstOrDefault().UserGroupId).FirstOrDefault().UserGroupName,
                                            IPAddress=ul.IPAddress,
                                            CountryCode= ul.CountryCode!=null?ul.CountryCode.ToLower():null,
                                            Country=ul.Country,
                                            City=ul.City,
                                            Region=ul.Region,
                                            LoggedInTimestamp = ul.LoggedInTimestamp,
                                            LoggedOffTimestamp = ul.LoggedOffTimestamp

                                        }).OrderByDescending(a=>a.UserSessionLogId).ToList();

                        return userLogData;
                    }
                    else
                    {
                        var userLogData = userSessionLogList
                                         .Where(a => a.UserId == userId)
                                        .Select(ul => new UserSessionLogDTO
                                        {
                                            UserSessionLogId = ul.UserSessionLogId,
                                            UserId = ul.UserId,
                                            ProfilePicURL = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().ProfilePicURL,
                                            Username = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().Username,
                                            UserCallingName = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().CallingName,
                                            UserFullName = userCollectionList.Where(a => a.UserId == ul.UserId).FirstOrDefault().FullName,
                                            UserGroupName = userGroupList.Where(a => a.UserGroupId == userCollectionList.Where(b => b.UserId == ul.UserId).FirstOrDefault().UserGroupId).FirstOrDefault().UserGroupName,
                                            IPAddress = ul.IPAddress,
                                            CountryCode = ul.CountryCode != null ? ul.CountryCode.ToLower() : null,
                                            Country = ul.Country,
                                            City = ul.City,
                                            Region = ul.Region,
                                            LoggedInTimestamp = ul.LoggedInTimestamp,
                                            LoggedOffTimestamp = ul.LoggedOffTimestamp

                                        }).OrderByDescending(a => a.UserSessionLogId).ToList();

                        return userLogData;
                    }

                }
                catch (Exception ex)
                {

                    throw;
                }
            }
        }

        #endregion

        #region "Clear Session Details"

        //Clear(Delete) all session details except the latest details for a given user
        //The latest details are NOT DELETED to ensure that a record is remaining at the time the user logs out
        //This record is required to update the Logged Out Time
        public bool ClearSessionDetails(int userId)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {

                    var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");
                    var userSessionLogList = userSessionLogCollection.AsQueryable().ToList();

                    //Get the maximum User Session Log Id for the user
                    int maxUserSessionLogId = userSessionLogList.Where(a => a.UserId == userId)
                                             .OrderByDescending(a=>a.UserSessionLogId).FirstOrDefault().UserSessionLogId;

                    //Get all session log details less than the above 'maxUserSessionLogId'
                    var sessionListToDelete = userSessionLogList
                                             .Where(b => b.UserSessionLogId < maxUserSessionLogId && b.UserId==userId).ToList();

                    //Delete these selected details one by one
                    foreach (var deleteObj in sessionListToDelete)
                    {
                        var filterObj = Builders<UserSessionLog>.Filter.Eq("UserSessionLogId", deleteObj.UserSessionLogId);
                        userSessionLogCollection.DeleteOne(filterObj);
                    }
                             
                    scope1.Complete();
                    
                    return true;
                }
                catch (Exception ex)
                {
                  
                    scope1.Dispose();
                    throw ex;
                }
            }
        }


        #endregion
    }
}
