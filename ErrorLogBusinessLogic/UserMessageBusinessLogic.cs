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
using System.Net.Mail;

namespace ErrorLogBusinessLogic
{

    public class UserMessageBusinessLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase, userMessageEmailLink;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        private UserDetailsBusinessLogic _UserDetailsBusinessLogic;
        Random rnd;


        public UserMessageBusinessLogic()
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

            //Email Body Link
            userMessageEmailLink = System.Configuration.ConfigurationSettings.AppSettings["UserMessageEmailLink"].ToString();

        }

        //Connect to public Mongo DB repository
        public MongoClient InitializeMongoClient()
        {
            try
            {

                mongoClient = new MongoClient(mongoDbCompleteURL);
                //("mongodb://rajindra:rajindra123@cluster1-shard-00-00-w6nfe.mongodb.net:27017,cluster1-shard-00-01-w6nfe.mongodb.net:27017,cluster1-shard-00-02-w6nfe.mongodb.net:27017/test?ssl=true&replicaSet=Cluster1-shard-0&authSource=admin");

                mongoDatabaseRunTime = mongoClient.GetDatabase(mongoDBTargetDatabase);
                return mongoClient;

            }
            catch (Exception ex)
            {
                throw;
            }



        }

        public object SMGGetUserList(string sessionUser)
        {
            try
            {
                MongoClient mongoClient = InitializeMongoClient();

                var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();

                return UserVar.AsQueryable()
                    .Where(s => s.Username.ToLower() != sessionUser.ToLower())
                    .Select(s => new
                    {
                        s.UserId,
                        s.Username,
                        s.FullName
                    }).OrderBy(s => s.Username).ToArray();
            }
            catch
            {
                return null;
            }
        }

        public int SMGGetNewMessageID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    int newMessageID = 0;

                    var messageCollection = mongoDatabaseRunTime.GetCollection<UserMessage>("UserMessage");

                    if (messageCollection.AsQueryable().Count() > 0)
                    {
                        scope1.Complete();
                        var maxID = messageCollection.AsQueryable()
                                   .OrderByDescending(a => a.UserMessageId)
                                   .FirstOrDefault().UserMessageId;

                        newMessageID = maxID + 1;
                    }
                    else
                    {
                        scope1.Complete();
                        newMessageID = 1;
                    }

                    return newMessageID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public int GetMessageCount(string sessionUser)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    int result = 0;

                    MongoClient mongoClient = InitializeMongoClient();
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();
                    int sessionUserID = UserVar.AsQueryable().First(s => s.Username.ToLower() == sessionUser.ToLower()).UserId;

                    var MessageCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.UserMessage>("UserMessage");

                    result = MessageCollection.AsQueryable().Where(s => s.IsDeleted == false && s.ReceivedUserId == sessionUserID && s.IsMessageRead == false).Count();

                    scope1.Complete();
                    return result;
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public int SMGSendMessage(List<int> UList, string subject, string message, string sessionUser)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    MongoClient mongoClient = InitializeMongoClient();
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();
                    int sessionUserID = UserVar.AsQueryable().First(s => s.Username.ToLower() == sessionUser.ToLower()).UserId;

                    int parentMessageID = SMGGetNewMessageID();
                    foreach (int Uid in UList)
                    {
                        var MessageCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.UserMessage>("UserMessage");
                        var messageRow = new ErrorLogDataAccess.DataClasses.UserMessage()
                        {
                            UserMessageId = SMGGetNewMessageID(),
                            ParentUserMessageId = parentMessageID,
                            MessageSubject = subject,
                            MessageBody = message,
                            IsMessageRead = false,
                            SentUserId = sessionUserID,
                            ReceivedUserId = Uid,
                            MessageSentDate = DateTime.Now
                        };
                        MessageCollection.InsertOne(messageRow);

                    }

                    rnd = new Random();
                    string requestId = rnd.Next(1, 1000).ToString();

                    string sessionUserName = _UserDetailsBusinessLogic.GetUserDetails(sessionUserID)
                                            .FirstOrDefault().Username;
                    string msgSubject = sessionUserName + " sent you  a message";
                    string msgBody = "Click the following link and navigate to 'Message Portal 'to view the message" + Environment.NewLine +
                                      userMessageEmailLink + requestId;

                    MailMessage msg = CommonCommunicationBusinessLogic.ConstructEmail(msgSubject, msgBody, UList);
                    CommonCommunicationBusinessLogic.SendEmail(msg);


                    scope1.Complete();
                    return 0;
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public List<object> VSMGDeleteMessages(string sessionUser, List<int> deleteList)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    MongoClient mongoClient = InitializeMongoClient();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    var MessageCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.UserMessage>("UserMessage");

                    foreach (int i in deleteList)
                    {
                        var filterObj = Builders<ErrorLogDataAccess.DataClasses.UserMessage>.Filter.Eq("UserMessageId", i);

                        var updateObj = Builders<ErrorLogDataAccess.DataClasses.UserMessage>.Update
                                 .Set("IsDeleted", true)
                                 .Set("LastUpdatedDate", formattedDate)
                               .CurrentDate("lastModified");


                        MessageCollection.UpdateOne(filterObj, updateObj);
                    }
                    
                    scope1.Complete();
                    return VSMGLoadSentMessages(sessionUser);
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public int VSMGDeleteAllMessages(string sessionUser)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    MongoClient mongoClient = InitializeMongoClient();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    
                    var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();
                    int sessionUserID = UserVar.AsQueryable().First(s => s.Username.ToLower() == sessionUser.ToLower()).UserId;

                    var MessageCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.UserMessage>("UserMessage");

                    foreach (var Message in MessageCollection.Find(s => s.SentUserId == sessionUserID && s.IsDeleted == false).ToList())
                    {
                        var filterObj = Builders<ErrorLogDataAccess.DataClasses.UserMessage>.Filter.Eq("UserMessageId", Message.UserMessageId);

                        var updateObj = Builders<ErrorLogDataAccess.DataClasses.UserMessage>.Update
                                 .Set("IsDeleted", true)
                                 .Set("LastUpdatedDate", formattedDate)
                               .CurrentDate("lastModified");
                        MessageCollection.UpdateOne(filterObj, updateObj);
                    }
                    
                    scope1.Complete();
                    return 0;
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public List<object> RMSGLoadMessageList(string sessionUser)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    MongoClient mongoClient = InitializeMongoClient();
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();
                    int sessionUserID = UserVar.AsQueryable().First(s => s.Username.ToLower() == sessionUser.ToLower()).UserId;

                    var MessageList = mongoDatabase.GetCollection<UserMessage>("UserMessage").AsQueryable()
                        .ToList();

                    List<object> returnMessageList = new List<object>();

                    foreach (var msg in MessageList.Where(s => s.IsDeleted == false  && (s.ReceivedUserId == sessionUserID )).OrderByDescending(s => s.IsMessageRead).ThenByDescending(s => s.MessageSentDate).GroupBy(s => s.SentUserId))
                    {
                        returnMessageList.Add(new
                        {
                            msg.FirstOrDefault().UserMessageId,
                            IsSent = (msg.FirstOrDefault().SentUserId == sessionUserID) ? true : false,
                            UserDetails = new { ProfilePicture = UserVar.First(s => s.UserId == msg.FirstOrDefault().SentUserId).ProfilePicURL, FullName = (UserVar.First(s => s.UserId == msg.FirstOrDefault().SentUserId).FullName) != null ? UserVar.First(s => s.UserId == msg.FirstOrDefault().SentUserId).FullName : UserVar.First(s => s.UserId == msg.FirstOrDefault().SentUserId).Username },
                            ParentUserMessageId = msg.FirstOrDefault().ParentUserMessageId == null ? 0 : msg.FirstOrDefault().ParentUserMessageId,
                            MessageSubject = msg.FirstOrDefault().MessageSubject,
                            MessageBody = msg.FirstOrDefault().MessageBody,
                            IsMessageRead = msg.FirstOrDefault().IsMessageRead,                            
                            MessageSentDate = String.Format("{0:g}", msg.FirstOrDefault().MessageSentDate),
                            MessageDeliveryDate= String.Format("{0:g}", msg.FirstOrDefault().MessageDeliveryDate)                            
                        });
                    }

                    scope1.Complete();

                    return returnMessageList;
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public List<object> RMSGSendMessage(string sessionUser, int messageID, string subject, string message)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    MongoClient mongoClient = InitializeMongoClient();
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();
                    int sessionUserID = UserVar.AsQueryable().First(s => s.Username.ToLower() == sessionUser.ToLower()).UserId;

                    int NewMessageID = SMGGetNewMessageID();

                    var MessageList = mongoDatabase.GetCollection<UserMessage>("UserMessage").AsQueryable()
                             .ToList();
                    UserMessage msg = MessageList.FirstOrDefault(s => s.UserMessageId == messageID);

                    int ReceiveUserID = (msg.SentUserId == sessionUserID) ? msg.ReceivedUserId.Value : msg.ReceivedUserId.Value;

                    var MessageCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.UserMessage>("UserMessage");
                    var messageRow = new ErrorLogDataAccess.DataClasses.UserMessage()
                    {
                        UserMessageId = NewMessageID,
                        ParentUserMessageId = messageID,
                        MessageSubject = subject,
                        MessageBody = message,
                        IsMessageRead = false,
                        SentUserId = sessionUserID,
                        ReceivedUserId = ReceiveUserID,
                        MessageSentDate = DateTime.Now
                    };
                    MessageCollection.InsertOne(messageRow);


                    rnd = new Random();
                    string requestId = rnd.Next(1, 1000).ToString();

                    string sessionUserName = _UserDetailsBusinessLogic.GetUserDetails(sessionUserID)
                                            .FirstOrDefault().Username;
                    string msgSubject = sessionUserName + " sent you  a message";
                    string msgBody = "Click the following link and navigate to 'Message Portal 'to view the message" + Environment.NewLine +
                                      userMessageEmailLink + requestId;
                    List<int> UList = new List<int>();
                    UList.Add(ReceiveUserID);
                    MailMessage msgs = CommonCommunicationBusinessLogic.ConstructEmail(msgSubject, msgBody, UList);
                    CommonCommunicationBusinessLogic.SendEmail(msgs);


                    scope1.Complete();
                    return RMSGLoadMessageList(sessionUser);
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public List<object> VSMGLoadSentMessages(string sessionUser)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    MongoClient mongoClient = InitializeMongoClient();
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();
                    int sessionUserID = UserVar.AsQueryable().First(s => s.Username.ToLower() == sessionUser.ToLower()).UserId;

                    var MessageList = mongoDatabase.GetCollection<UserMessage>("UserMessage").AsQueryable()
                        .ToList();

                    List<object> returnMessageList = new List<object>();

                    foreach (var msg in MessageList.Where(s => s.IsDeleted == false && s.SentUserId == sessionUserID).OrderByDescending(s => s.MessageSentDate))
                    {
                        returnMessageList.Add(new
                        {
                            msg.UserMessageId,
                            UserDetails = new { ProfilePicture = UserVar.First(s => s.UserId == msg.ReceivedUserId).ProfilePicURL, FullName = (UserVar.First(s => s.UserId == msg.ReceivedUserId).FullName) != null ? UserVar.First(s => s.UserId == msg.ReceivedUserId).FullName : UserVar.First(s => s.UserId == msg.ReceivedUserId).Username },
                            MessageSentDate = String.Format("{0:g}", msg.MessageSentDate),
                            MessageDeliveryDate = msg.MessageDeliveryDate,
                            msg.MessageSubject,
                            msg.MessageBody,
                            IsDelete = false
                        });
                    }

                    scope1.Complete();

                    return returnMessageList;
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }

        public List<object> RMSGLoadPreviousMessages(string UserName, int messageID)
        {
            int MessageID;
            if (messageID > 0) { MessageID = messageID; }
            else return null;
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                    MongoClient mongoClient = InitializeMongoClient();
                    _UserDetailsBusinessLogic = new UserDetailsBusinessLogic();

                    var mongoDatabase = mongoClient.GetDatabase(mongoDBTargetDatabase);

                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    var UserVar = mongoDatabase.GetCollection<SystemUser>("SystemUser").AsQueryable().ToList();
                    int sessionUserID = UserVar.AsQueryable().First(s => s.Username.ToLower() == UserName.ToLower()).UserId;

                    var MessageList = mongoDatabase.GetCollection<UserMessage>("UserMessage").AsQueryable()
                        .ToList();

                    int SUserID = MessageList.FirstOrDefault(s => s.UserMessageId == MessageID).SentUserId.Value;
                    int RUserID = MessageList.FirstOrDefault(s => s.UserMessageId == MessageID).ReceivedUserId.Value;

                    int OtherUserID = (SUserID == sessionUserID ? RUserID : SUserID);

                    List<object> returnMessageList = new List<object>();

                    foreach (var msg in MessageList.Where(s => s.IsDeleted == false && ((s.ReceivedUserId == SUserID && s.SentUserId == RUserID ) || (s.ReceivedUserId == RUserID && s.SentUserId == SUserID)) ).OrderBy(s => s.MessageSentDate))
                    {
                        returnMessageList.Add(new
                        {
                            msg.UserMessageId,
                            IsSent = (msg.SentUserId == sessionUserID) ? true : false,
                            UserDetails = new { ProfilePicture = UserVar.First(s => s.UserId == OtherUserID).ProfilePicURL, FullName = (UserVar.First(s => s.UserId == OtherUserID).FullName) != null ? UserVar.First(s => s.UserId == OtherUserID).FullName : UserVar.First(s => s.UserId == OtherUserID).Username },
                            ParentUserMessageId = msg.ParentUserMessageId == null ? 0 : msg.ParentUserMessageId,
                            MessageSubject = msg.MessageSubject,
                            MessageBody = msg.MessageBody,
                            IsMessageRead = msg.IsMessageRead,                            
                            MessageSentDate = String.Format("{0:g}", msg.MessageSentDate),
                            MessageDeliveryDate = String.Format("{0:g}", msg.MessageDeliveryDate)
                        });

                        if (msg.ReceivedUserId == sessionUserID)
                        {
                            var MessageCollection = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.UserMessage>("UserMessage");

                            var filterObj = Builders<ErrorLogDataAccess.DataClasses.UserMessage>.Filter.Eq("UserMessageId", msg.UserMessageId);

                            var updateObj = Builders<ErrorLogDataAccess.DataClasses.UserMessage>.Update
                                .Set("IsMessageRead", true)
                                .Set("MessageDeliveryDate", String.Format("{0:g}", formattedDate))
                                .Set("LastUpdatedDate", formattedDate)
                              .CurrentDate("lastModified");


                            MessageCollection.UpdateOne(filterObj, updateObj);
                        } 
                    }

                    scope1.Complete();

                    return returnMessageList;
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
