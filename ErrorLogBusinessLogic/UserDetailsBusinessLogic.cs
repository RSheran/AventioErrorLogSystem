using ErrorLogDataAccess.DataClasses;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ErrorLogBusinessLogic
{
    public class UserDetailsBusinessLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        private static Random random = new Random(); //To generate temperory string

        MongoClient mongoClient;

        public UserDetailsBusinessLogic()
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


        #region "To retrieve/save/update User Group"

        #region "Functions to retrieve user group details"


        public Array GetAllUserGroups(int isActive)
        {

            try
            {

                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.SystemUserGroup>("SystemUserGroup").AsQueryable().ToList();
                if (isActive == 0)
                {
                    var resultArray = resultList.AsQueryable()
                                    // .Where(w => w.DomainCode != null)
                                     .Select(p => new
                                     {
                                         p.UserGroupId,
                                         p.UserGroupName,
                                         p.UserGroupDescription,
                                         p.CreatedUserId,
                                         p.CreatedDate,
                                         p.LastUpdatedDate,
                                         p.IsActive
                                     }).OrderBy(p => p.UserGroupName).ToArray();

                    return resultArray;


                }

                else if (isActive == 1)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w =>w.IsActive == true)
                                     .Select(p => new
                                     {
                                         p.UserGroupId,
                                         p.UserGroupName,
                                         p.UserGroupDescription,
                                         p.CreatedUserId,
                                         p.CreatedDate,
                                         p.LastUpdatedDate,
                                         p.IsActive
                                     }).OrderBy(p => p.UserGroupName).ToArray();

                    return resultArray;
                }

                else
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w =>w.IsActive == false)
                                     .Select(p => new
                                     {
                                         p.UserGroupId,
                                         p.UserGroupName,
                                         p.UserGroupDescription,
                                         p.CreatedUserId,
                                         p.CreatedDate,
                                         p.LastUpdatedDate,
                                         p.IsActive
                                     }).OrderBy(p => p.UserGroupName).ToArray();

                    return resultArray;
                }

                return null;
            }

            catch (Exception ex)
            {
                throw;
            }

        }

        //Overidden method to load user groups for offline new user or a user who is not an admin
        //This is to comply with the rule 'An administrator can be added only by an existing administrator '
        //Hence,the 'administrator' user group will not be displayed
        public Array GetAllUserGroupsForOtherUser(int isActive)
        {

            try
            {

                var resultList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.SystemUserGroup>("SystemUserGroup").AsQueryable().ToList();
                if (isActive == 0)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.UserGroupName.ToLower() != "administrator")
                                     .Select(p => new
                                     {
                                         p.UserGroupId,
                                         p.UserGroupName,
                                         p.UserGroupDescription,
                                         p.CreatedUserId,
                                         p.CreatedDate,
                                         p.LastUpdatedDate,
                                         p.IsActive
                                     }).OrderBy(p => p.UserGroupName).ToArray();

                    return resultArray;


                }

                else if (isActive == 1)
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.IsActive == true && w.UserGroupName.ToLower() != "administrator")
                                     .Select(p => new
                                     {
                                         p.UserGroupId,
                                         p.UserGroupName,
                                         p.UserGroupDescription,
                                         p.CreatedUserId,
                                         p.CreatedDate,
                                         p.LastUpdatedDate,
                                         p.IsActive
                                     }).OrderBy(p => p.UserGroupName).ToArray();

                    return resultArray;
                }

                else
                {
                    var resultArray = resultList.AsQueryable()
                                     .Where(w => w.IsActive == false && w.UserGroupName.ToLower() != "administrator")
                                     .Select(p => new
                                     {
                                         p.UserGroupId,
                                         p.UserGroupName,
                                         p.UserGroupDescription,
                                         p.CreatedUserId,
                                         p.CreatedDate,
                                         p.LastUpdatedDate,
                                         p.IsActive
                                     }).OrderBy(p => p.UserGroupName).ToArray();

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
        public Array GetUserGroupDetails(string userGroupName)
        {

            try
            {
                
                var resultList = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.UserGroupName.ToLower().Trim() == userGroupName.ToLower().Trim())
                                     .Select(p => new
                                     {
                                         p.UserGroupId,
                                         p.UserGroupName,
                                         p.UserGroupDescription,
                                         p.CreatedUserId,
                                         p.CreatedDate,
                                         p.LastUpdatedDate,
                                         p.IsActive
                                     }).OrderBy(p => p.UserGroupName).ToArray();

                return resultArray;

            }

            catch (Exception ex)
            {
                throw;
            }

        }

        //Get domain codes for autosuggestion(Top 10)
        public Array GetUserGroupNamesForAutoComplete(string userGroupName)
        {

            try
            {

                var resultList = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup").AsQueryable().ToList();

                var resultArray = resultList.AsQueryable()
                                     .Where(w => w.UserGroupName.ToLower().Contains(userGroupName.ToLower().Trim()))
                                     .Select(p => new
                                     {
                                         p.UserGroupName

                                     }).OrderBy(p => p.UserGroupName).Take(5).ToArray();

                return resultArray;

            }

            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion

        #region "Functions to add new user group"

        //To check product category code is available
        public bool IsUserGroupAvailable(string userGroupName)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
           
                    //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var dataCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");
                                        
                    if (dataCollection.AsQueryable()
                      .Where(d => d.UserGroupName.ToLower() == userGroupName.ToLower()).Count() > 0)
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
        public int GetNewUserGroupID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    
                    int newDomainID = 0;

                    var dataCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");
                    
                    if (dataCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = dataCollection.AsQueryable()
                                   .OrderByDescending(a => a.UserGroupId)
                                   .FirstOrDefault().UserGroupId;

                        newDomainID = maxID + 1;


                    }
                    else
                    {
                        scope1.Complete();
                        newDomainID = 1;
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


        //save using a 'User Group' type variable
        public bool SaveUserGroup(SystemUserGroup userGroupObj, int userId = 1)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {

                try
                {
                 
                    string currentDate = DateTime.Now.ToString();

                    var dataCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");


                    var userGroupTuple = new SystemUserGroup()
                    {
                        UserGroupId = GetNewUserGroupID(),
                        UserGroupName = userGroupObj.UserGroupName,
                        UserGroupDescription = userGroupObj.UserGroupDescription,
                        IsActive = userGroupObj.IsActive,
                        CreatedDate =currentDate,
                        LastUpdatedDate = currentDate,
                        CreatedUserId=userId

                    };

                    dataCollection.InsertOneAsync(userGroupTuple);

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

        #region "Functions to update existing user group"

        public bool UpdateUserGroup(SystemUserGroup userGroupObj, int userId = 1)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {

                try
                {
                
                    string currentDate = DateTime.Now.ToString();

                    var dataCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");
                    
                    var filterObj = Builders<ErrorLogDataAccess.DataClasses.SystemUserGroup>.Filter.Eq("UserGroupName", userGroupObj.UserGroupName);

                    var updateObj = Builders<ErrorLogDataAccess.DataClasses.SystemUserGroup>.Update
                                 .Set("UserGroupDescription", userGroupObj.UserGroupDescription)
                                 .Set("IsActive", userGroupObj.IsActive)
                                 .Set("LastUpdatedDate", DateTime.Now.ToString())
                                 .CurrentDate("lastModified");


                    dataCollection.UpdateOneAsync(filterObj, updateObj);


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


        #region"Functions to change status of user group"
        //Update status of selected expense types
        public bool UpdateStatusOfSelectedGroups(SystemUserGroup[] userGroupObj)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                  
                    var userGroupCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.SystemUserGroup>("SystemUserGroup");

                    string currentTime = DateTime.Now.ToString();

                    if (userGroupObj.Count() != 0)
                    {
                        foreach (var userGroupCurr in userGroupObj)
                        {
                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.SystemUserGroup>.Filter.Eq("UserGroupName", userGroupCurr.UserGroupName);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.SystemUserGroup>.Update
                                            .Set("IsActive", userGroupCurr.IsActive)
                                            .Set("LastUpdatedDate", currentTime)
                                            .CurrentDate("lastModified");


                            userGroupCollection.UpdateOneAsync(filterObj, updateObj);

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

        public bool ChangeStatusForAllGroups(bool isActiveForAll)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                  
                    var userGroupCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");
                    var userGroupList = userGroupCollection.AsQueryable().ToList();

                    string currentTime = DateTime.Now.ToString();

                    if (userGroupList.Count() != 0)
                    {
                        foreach (var userGroupCurr in userGroupList)
                        {
                            var filterObj = Builders<SystemUserGroup>.Filter.Eq("UserGroupName", userGroupCurr.UserGroupName);

                            var updateObj = Builders<SystemUserGroup>.Update
                                            .Set("IsActive", isActiveForAll)
                                            .Set("LastUpdatedDate", currentTime)
                                            .CurrentDate("lastModified");


                            userGroupCollection.UpdateOneAsync(filterObj, updateObj);

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

        #endregion

        #region "To retrieve/save/update User "

  

        #region "To retrieve User Details "
            //Get user details by user name
            public List<SystemUser> GetUserDetails(string userName)
            {
                try
                {
                
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");
                    var userGroupCollection= mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");

                    var systemUserList = userCollection.AsQueryable().ToList();
                    var systemUserGroupList = userGroupCollection.AsQueryable().ToList();

                    var userData = systemUserList.AsQueryable()
                                        .Where(sy => sy.Username.ToLower().Trim() == userName.ToLower().Trim())
                                        .Select(u => new SystemUser
                                        {
                                            UserId = u.UserId,
                                            UserGroupId = u.UserGroupId,
                                            Username = u.Username,
                                            Password = u.Password,
                                            CallingName = u.CallingName,
                                            FullName = u.FullName,
                                            Email = u.Email,
                                            ProfilePicURL = u.ProfilePicURL,
                                            IsActive = u.IsActive,
                                            UserGroupName = systemUserGroupList.Where(sg => sg.UserGroupId == u.UserGroupId).
                                                           FirstOrDefault().UserGroupName
                                        }).ToList();


                    return userData;

                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            //Get user details by user Id
            public List<SystemUser> GetUserDetails(int userId)
            {
                try
                {

                var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");
                var userGroupCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");

                var systemUserList = userCollection.AsQueryable().ToList();
                var systemUserGroupList = userGroupCollection.AsQueryable().ToList();

                var userData = systemUserList.AsQueryable()
                                    .Where(sy => sy.UserId== userId)
                                    .Select(u => new SystemUser
                                    {
                                        UserId = u.UserId,
                                        UserGroupId = u.UserGroupId,
                                        Username = u.Username,
                                        Password = u.Password,
                                        CallingName = u.CallingName,
                                        FullName = u.FullName,
                                        Email = u.Email,
                                        ProfilePicURL = u.ProfilePicURL,
                                        IsActive = u.IsActive,
                                        UserGroupName = systemUserGroupList.Where(sg => sg.UserGroupId == u.UserGroupId).
                                                       FirstOrDefault().UserGroupName
                                    }).ToList();


                return userData;

            }
                catch (Exception ex)
                {
                    throw;
                }
            }
        
            //Get User's full name
            public string GetUserFullName(string userName)
            {
                try
                {
                
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var systemUserList = userCollection.AsQueryable().ToList();

                    string userFullName = systemUserList.AsQueryable()
                                        .Where(sy => sy.Username.ToLower().Trim() == userName.ToLower().Trim())
                                        .FirstOrDefault().FullName;


                    return userFullName;

                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            //Get user ID
            public int GetUserID(string userName)
            {
                try
                {
                 
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var systemUserList = userCollection.AsQueryable().ToList();

                    int userId = systemUserList.AsQueryable()
                                        .Where(sy => sy.Username.ToLower().Trim() == userName.ToLower().Trim())
                                        .FirstOrDefault().UserId;


                    return userId;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            //Get User Type
            public string GetUserType(string userName)
            {
                try
                {
                 
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");
                    var userGroupCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");

                    var systemUserList = userCollection.AsQueryable().ToList();
                    var systemUserGroupList = userGroupCollection.AsQueryable().ToList();

                     //First get the user Type Id
                     int? userTypeId = systemUserList.AsQueryable()
                                        .Where(sy => sy.Username.ToLower().Trim() == userName.ToLower().Trim())
                                        .FirstOrDefault().UserGroupId;

                    //Then get the user type(user group) for the retrieved User Type Id
                    string userType = systemUserGroupList.AsQueryable()
                                        .Where(sg => sg.UserGroupId == userTypeId)
                                        .FirstOrDefault().UserGroupName;


                    return userType;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            public string GetUserType(int userId)
            {
                try
                {

                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");
                    var userGroupCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");

                    var systemUserList = userCollection.AsQueryable().ToList();
                    var systemUserGroupList = userGroupCollection.AsQueryable().ToList();

                    //First get the user Type Id
                    int? userTypeId = systemUserList.AsQueryable()
                                       .Where(sy => sy.UserId== userId)
                                       .FirstOrDefault().UserGroupId;

                    //Then get the user type(user group) for the retrieved User Type Id
                    string userType = systemUserGroupList.AsQueryable()
                                        .Where(sg => sg.UserGroupId == userTypeId)
                                        .FirstOrDefault().UserGroupName;


                    return userType;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

        //Check whether user is active
        public bool IsUserActive(string userName)
            {
                try
                {
                 
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var systemUserList = userCollection.AsQueryable().ToList();
                
                    bool isUserActive = systemUserList.AsQueryable()
                                             .Where(sy => sy.Username.ToLower().Trim() == userName.ToLower().Trim())
                                             .FirstOrDefault().IsActive;

                    return isUserActive;
                }
                catch (Exception ex)
                {
                    throw;
                }

            }

            public bool IsExistingUser(string username, string password)
            {
                try
                {
                  
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var systemUserList = userCollection.AsQueryable().ToList();


                    string checkPwd = "";
                                             
                
                    using (MD5 md5Hash = MD5.Create())
                    {
                       HashingModuleBusinessLogic hm = new HashingModuleBusinessLogic();
                        checkPwd = hm.GetMd5Hash(md5Hash, password);
                    }

                    //First get the count of the user in the System User Table
                    int count_u = systemUserList.AsQueryable()
                              .Where(a => a.Username.ToLower().Trim() == username.ToLower().Trim() && a.Password == checkPwd)
                              .Count();

                 //Then check whether the user is in the Password Change Request Table
                  bool IsUserInTempTable= IsUserInTempPwdTable(username, password);


                    if (count_u > 0|| IsUserInTempTable==true)
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

        public bool IsExistingUserForPasswordRecovery(string username, string email)
            {
                try
                {
        
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var systemUserList = userCollection.AsQueryable().ToList();
                                   

                    int count_u = systemUserList.AsQueryable()
                              .Where(a => a.Username.ToLower().Trim() == username.ToLower().Trim() && a.Email.ToLower().Trim() == email.ToLower().Trim())
                              .Count();

                    if (count_u > 0)
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

        //Check whether username is existing
        public bool IsUsernameExisting(string chkUserName)
             {
                    try
                    {
                      
                        var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                        var systemUserList = userCollection.AsQueryable().ToList();


                        int userNameCount = systemUserList.AsQueryable()
                                          .Where(a => a.Username.ToLower().Trim() == chkUserName.ToLower().Trim()).Count();

                        if (userNameCount > 0)
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

            //Check whether username is existing
       public bool IsUserEmailExisting(string chkUserEmail)
            {
                try
                {
              
                    var systemUserList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.SystemUser>("SystemUser").AsQueryable().ToList();

                     int userNameCount = systemUserList.AsQueryable()
                                      .Where(a => a.Email.ToLower().Trim() == chkUserEmail.ToLower().Trim()).Count();

                    if (userNameCount > 0)
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

        #region "To save/update user details"

        public int GetNewUserID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                   
                    int newUserID = 0;

                    var systemUserCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    if (systemUserCollection.AsQueryable().Count() > 0)
                    {

                        var maxID = systemUserCollection.AsQueryable()
                                   .OrderByDescending(a => a.UserId)
                                   .FirstOrDefault().UserId;

                        newUserID = maxID + 1;


                    }
                    else
                    {

                        newUserID = 1;
                    }

                    scope1.Complete();

                    return newUserID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public bool AddSystemUser(SystemUser systemUser,int entryUserId=0)
        {
            //If entryUserId=0,then it is  a first-time sign up

            using (TransactionScope scope1 = new TransactionScope())
            {

                try
                {
                    
                    string encPassword = "";

                    var systemUserCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    int newUserId = GetNewUserID();
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    //Hashing the password
                    using (MD5 md5Hash = MD5.Create())
                    {
                        HashingModuleBusinessLogic hm = new HashingModuleBusinessLogic();
                        encPassword = hm.GetMd5Hash(md5Hash, systemUser.Password);
                    }

                    var userTuple = new SystemUser()
                    {
                        UserId = newUserId,
                        UserGroupId = systemUser.UserGroupId,
                        Username = systemUser.Username,
                        Password = encPassword,
                        CallingName = systemUser.CallingName,
                        FullName = systemUser.FullName,
                        Email = systemUser.Email,
                        ProfilePicURL = systemUser.ProfilePicURL,
                        IsActive = systemUser.IsActive,
                        CreatedDate = formattedDate,
                        LastUpdatedDate= formattedDate,
                        LastUpdatedUserId=entryUserId
                        
                    };

                    systemUserCollection.InsertOneAsync(userTuple);

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

        public bool IsOldPwdValid(string chkPassword,int userId)
        {
            try
            {
        
                string encPassword = "";

                using (MD5 md5Hash = MD5.Create())
                {
                    HashingModuleBusinessLogic hm = new HashingModuleBusinessLogic();
                    encPassword = hm.GetMd5Hash(md5Hash, chkPassword);
                }

                var systemUserList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.SystemUser>("SystemUser").AsQueryable().ToList();
                var passwordReqList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.PasswordChangeRequest>("PasswordChangeRequest").AsQueryable().ToList();

                string userOriPassword = systemUserList.AsQueryable()
                                        .Where(a => a.UserId == userId).FirstOrDefault().Password;

                string tempPassword =   passwordReqList.AsQueryable()
                                        .Where(a => a.UserId == userId).Count() > 0 ? passwordReqList.AsQueryable()
                                        .Where(a => a.UserId == userId).FirstOrDefault().TempPassword : "";

                if ((encPassword == userOriPassword) || chkPassword == tempPassword)
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

        //Update user details
        public bool UpdateSytemUser(SystemUser systemUser,int updatingUserId)
        {
            //updatingUserId=>The person who is updating the user details
            //Sometimes,this may be the admin
            
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                  
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    var systemUserCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    int userIdToUpdate = systemUserCollection.AsQueryable().ToList()
                                        .Where(a => a.Username.ToLower().Trim() == systemUser.Username.ToLower().Trim())
                                        .FirstOrDefault().UserId;

                    var filterObj = Builders<SystemUser>.Filter.Eq("UserId", userIdToUpdate);


                    var updateObj = Builders<SystemUser>.Update
                               .Set("FullName", systemUser.FullName)
                               .Set("CallingName", systemUser.CallingName)
                               .Set("IsActive", systemUser.IsActive)
                               .Set("Email", systemUser.Email)
                               .Set("LastUpdatedDate", formattedDate)
                               .Set("LastUpdatedUserId", updatingUserId)
                               .CurrentDate("lastModified");


                    systemUserCollection.UpdateOne(filterObj, updateObj);
                    
                    scope1.Complete();

                    return true;
                }
                catch (Exception)
                {
                    scope1.Dispose();
                    return false;
                }
            }
        }

        public bool UpdatePassword(string newPassword,int userId)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
               
                    string encPassword = "";
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    using (MD5 md5Hash = MD5.Create())
                    {
                        HashingModuleBusinessLogic hm = new HashingModuleBusinessLogic();
                        encPassword = hm.GetMd5Hash(md5Hash, newPassword);
                    }

                    var systemUserCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var filterObj = Builders<ErrorLogDataAccess.DataClasses.SystemUser>.Filter.Eq("UserId", userId);
                    
                    var updateObj = Builders<ErrorLogDataAccess.DataClasses.SystemUser>.Update
                                   .Set("Password", encPassword)
                                   .Set("LastUpdatedDate", formattedDate)
                                   .CurrentDate("lastModified");


                    systemUserCollection.UpdateOneAsync(filterObj, updateObj);

                    scope1.Complete();

                    return true;
                }
                catch (Exception)
                {
                    scope1.Dispose();
                    return false;
                }
            }
        }

        public bool SaveImageWithTheUserID(string imageURL, int userId)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    
                    string encPassword = "";
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    var systemUserCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var filterObj = Builders<ErrorLogDataAccess.DataClasses.SystemUser>.Filter.Eq("UserId", userId);

                    var updateObj = Builders<ErrorLogDataAccess.DataClasses.SystemUser>.Update
                                   .Set("ProfilePicURL", imageURL)
                                   .Set("LastUpdatedDate", formattedDate)
                                   .CurrentDate("lastModified");


                    systemUserCollection.UpdateOne(filterObj, updateObj);

                    scope1.Complete();

                    return true;
                }
                catch (Exception)
                {
                    scope1.Dispose();
                    return false;
                }
            }
        }

        #endregion


        #region "To manage Temperory Password"

        //Check whether user is existing in temperory password table
        public bool IsUserInTempPwdTable(string username, string password)
        {
            try
            {
                
                var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");
                var tempPasswordCollection = mongoDatabaseRunTime.GetCollection<PasswordChangeRequest>("PasswordChangeRequest");
                //var userList = userCollection.AsQueryable().ToList();
                

                //Get user id by username
                int userId = userCollection.AsQueryable().ToList()
                             .Where(a => a.Username.ToLower().Trim() == username.ToLower().Trim()).Count() > 0 ?
                              userCollection.AsQueryable().ToList()
                             .Where(a => a.Username.ToLower().Trim() == username.ToLower().Trim()).FirstOrDefault().UserId : 0;

                int userCount = tempPasswordCollection.AsQueryable().ToList()
                                .Where(a => a.UserId == userId && a.TempPassword.Trim() == password.Trim()).Count();

                if (userCount > 0)
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

        public bool DeleteTemperoryPassword(string username)
        {
          
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    
                    var tempPasswordCollection = mongoDatabaseRunTime.GetCollection<PasswordChangeRequest>("PasswordChangeRequest");
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var systemUserList = userCollection.AsQueryable().ToList();
                    var tempPasswordList = tempPasswordCollection.AsQueryable().ToList();

                    //Get user id by username
                    int userId = systemUserList.Where(a => a.Username.ToLower().Trim() == username.ToLower().Trim())
                                 .FirstOrDefault().UserId;

                    int tempPwdCount = tempPasswordList.Where(a => a.UserId == userId).Count();

                    if (tempPwdCount > 0)
                    {
                        var filterObj = Builders<PasswordChangeRequest>.Filter.Eq("UserId", userId);

                        tempPasswordCollection.DeleteOne(filterObj);

                    }


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

        public bool DeleteTemperoryPassword(int userId)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {

                    var tempPasswordCollection = mongoDatabaseRunTime.GetCollection<PasswordChangeRequest>("PasswordChangeRequest");     
                    var tempPasswordList = tempPasswordCollection.AsQueryable().ToList();
                                     
                    int tempPwdCount = tempPasswordList.Where(a => a.UserId == userId).Count();

                    if (tempPwdCount > 0)
                    {
                        var filterObj = Builders<PasswordChangeRequest>.Filter.Eq("UserId", userId);

                        tempPasswordCollection.DeleteOne(filterObj);

                    }


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


        public string SaveTempPassword(string username)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {                  
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    var tempPasswordCollection = mongoDatabaseRunTime.GetCollection<PasswordChangeRequest>("PasswordChangeRequest");
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");

                    var systemUserList = userCollection.AsQueryable().ToList();
                    var tempPasswordList = tempPasswordCollection.AsQueryable().ToList();

                    //Get user id by username
                    int userId = systemUserList.Where(a => a.Username.ToLower().Trim() == username.ToLower().Trim())
                                 .FirstOrDefault().UserId;

                    //Generate new random string
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

                    string tempPassword = new string(Enumerable.Repeat(chars, 10)
                                         .Select(s => s[random.Next(s.Length)]).ToArray());

                    var pwdChangeReqTuple = new PasswordChangeRequest()
                    {
                        UserId = userId,
                        TempPassword = tempPassword,
                        PasswordRequestedTime = formattedDate
                        

                    };

                    tempPasswordCollection.InsertOne(pwdChangeReqTuple);
                    scope1.Complete();
                    return tempPassword;
                }

                catch (Exception)
                {
                    scope1.Dispose();
                    throw;
                }
            }


        }


        #endregion

        #endregion

        #region "Add/Update Session-related data for user"

        //Get New ID For UserSessionLog
        public int GetNewUserSessionLogID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {

                    int newUserSessionLogID = 0;

                    var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");

                    if (userSessionLogCollection.AsQueryable().Count() > 0)
                    {

                        var maxID = userSessionLogCollection.AsQueryable()
                                   .OrderByDescending(a => a.UserSessionLogId)
                                   .FirstOrDefault().UserSessionLogId;

                        newUserSessionLogID = maxID + 1;


                    }
                    else
                    {

                        newUserSessionLogID = 1;
                    }

                    scope1.Complete();

                    return newUserSessionLogID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        //Save Session Log Details
        public bool SaveSessionDetails(int userId,string ipAddress,string countryCode,string country,string city,string region)
        {
          
            using (TransactionScope scope1 = new TransactionScope())
            {

                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");


                    var sessionTuple = new UserSessionLog()
                    {
                       UserSessionLogId=GetNewUserSessionLogID(),
                       UserId=userId,
                       IPAddress=ipAddress,
                       CountryCode=countryCode,
                       Country=country,
                       City=city,
                       Region=region,
                       LoggedInTimestamp= formattedDate

                    };

                    userSessionLogCollection.InsertOne(sessionTuple);

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


        //Update SystemUser=>Update 'IsLoggedIn' flag 
        public bool UpdateLoggedInStatus(int userId,bool isLoggedIn)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {

                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");


                    var filterObj = Builders<SystemUser>.Filter.Eq("UserId", userId);


                    var updateObj = Builders<SystemUser>.Update
                               .Set("IsLoggedIn", isLoggedIn)
                               .Set("LastUpdatedDate", formattedDate)
                               .CurrentDate("lastModified");

                    userCollection.UpdateOne(filterObj, updateObj);

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

        //Get latest User Session Log Id for user
        public int GetLatestSessionIdForUser(int userId)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {

                try
                {
                    int latestSessionId = 0;

                    var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");

                    latestSessionId = userSessionLogCollection.AsQueryable().Where(a => a.UserId == userId).Count() > 0 ?
                                      userSessionLogCollection.AsQueryable().Where(a => a.UserId == userId)
                                     .OrderByDescending(b => b.UserSessionLogId).FirstOrDefault().UserSessionLogId : 0;

                    scope1.Complete();

                    return latestSessionId;
                }
                catch (Exception)
                {
                    scope1.Dispose();
                    throw;

                }

            }
        }

        //Update UserSessionLog=>Update the LoggedOffTime for the latest data row
        public bool UpdateUserSessionLog(int userSessionLogId)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {

                try
                {
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                    var userSessionLogCollection = mongoDatabaseRunTime.GetCollection<UserSessionLog>("UserSessionLog");


                    var filterObj = Builders<UserSessionLog>.Filter.Eq("UserSessionLogId", userSessionLogId);


                    var updateObj = Builders<UserSessionLog>.Update
                                   .Set("LoggedOffTimestamp", formattedDate)
                                   .CurrentDate("lastModified");

                    userSessionLogCollection.UpdateOne(filterObj, updateObj);

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

        #region Get user details for common calls

        public List<SystemUser> GetUserDetails(List<int> userId)
        {
            try
            {

                var userCollection = mongoDatabaseRunTime.GetCollection<SystemUser>("SystemUser");
                var userGroupCollection = mongoDatabaseRunTime.GetCollection<SystemUserGroup>("SystemUserGroup");

                var systemUserList = userCollection.AsQueryable().ToList();
                var systemUserGroupList = userGroupCollection.AsQueryable().ToList();

                var userData = systemUserList.AsQueryable()
                                    .Where(sy => userId.Contains(sy.UserId) && sy.IsActive == true)
                                    .Select(u => new SystemUser
                                    {
                                        UserId = u.UserId,
                                        Username = u.Username,
                                        FullName = u.FullName,
                                        Email = u.Email,
                                        ProfilePicURL = u.ProfilePicURL,
                                        IsActive = u.IsActive
                                    }).ToList();
                return userData;

            }
            catch (Exception ex)
            {
                throw;
            }
        }


        #endregion






    }
}
