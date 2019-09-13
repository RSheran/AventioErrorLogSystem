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

    public class SystemUserDirectoryBusinessLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        private UserDetailsBusinessLogic _UserDetailsBusinessLogic;

        public SystemUserDirectoryBusinessLogic()
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

        #region "Retrieve all users' details=>For administrator only"
               
        public List<SystemUserDTO> GetAllUsers()
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


                    var userData = userCollectionList
                                   .Select(u => new SystemUserDTO
                                   {
                                         UserId = u.UserId,
                                         UserGroupId = u.UserGroupId,
                                         ProfilePicURL = u.ProfilePicURL,
                                         Username = u.Username,
                                         UserGroupName = userGroupList.Where(sg => sg.UserGroupId == u.UserGroupId).
                                                           FirstOrDefault().UserGroupName,
                                         FullName = u.FullName,
                                         CallingName = u.CallingName,
                                         Email = u.Email,
                                         IsActive = u.IsActive,
                                         IsLoggedIn = u.IsLoggedIn,
                                         LastLogInTime = GetLastLoggedInTime(u.UserId),
                                         LastLogOffTime = GetLastLoggedOffTime(u.UserId),
                                       
                                     }).OrderBy(a=>a.Username).ThenBy(a=>a.FullName).ToList();

                    return userData;
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
        }

        public string GetLastLoggedInTime(int userID)
        {
            try
            {
                var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");
                var userSessionLogList = userSessionLogCollection.AsQueryable().ToList();

                string result = userSessionLogList.Where(ul => ul.UserId == userID).Count() > 0 ?
                                                        userSessionLogList.Where(ul => ul.UserId == userID)
                                                       .OrderByDescending(a => a.UserSessionLogId).FirstOrDefault().LoggedInTimestamp : null;

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }

        }


        public string GetLastLoggedOffTime(int userID)
        {
            try
            {
                var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");
                var userSessionLogList = userSessionLogCollection.AsQueryable().ToList();

                string result = userSessionLogList.Where(ul => ul.UserId == userID).Count() > 0 ?
                                                        userSessionLogList.Where(ul => ul.UserId == userID)
                                                       .OrderByDescending(a => a.UserSessionLogId).FirstOrDefault().LoggedOffTimestamp : null;

                return result;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        //To get active(currently logged in) user count
        public int GetSessionActiveUserCount()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    int activeSessionUserCount = userCollection.AsQueryable().Where(a => a.IsLoggedIn).Count();

                    return activeSessionUserCount;
                }
                catch (Exception)
                {
                    throw;
                }
            }

        }
         


        #endregion

    }
}
