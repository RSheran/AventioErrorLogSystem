using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using ErrorLogDataAccess.DataClasses;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.Globalization;

namespace ErrorLogBusinessLogic
{
    public class ErrorMgtBusinesssLogic
    {

        string mongoDbCompleteURL, mongoDBTargetDatabase;
        IMongoDatabase mongoDatabaseRunTime;

        MongoClient mongoClient;

        public ErrorMgtBusinesssLogic()
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

        #region "Load Already Existing errors/Details for error code"

        public object GetExistingErrorList()
        {
            try
            {
                
                var errorList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                var domainList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Domain>("Domain").AsQueryable().ToList();
                var clientList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.Client>("Client").AsQueryable().ToList();

                var resultArray = errorList.AsQueryable()
                                    .Select(er => new
                                    {
                                        er.ErrorCode,
                                        Domain = er.DomainId!=null? domainList.Where(a=>a.DomainId==er.DomainId).FirstOrDefault().DomainName:"",
                                        Field="N/A",
                                        Client="N/A",
                                        er.ErrorCaption,
                                        er.ErrorDescription,
                                        er.SolutionMasters,
                                        er.ErrorAttachments
                                        
                                    }).OrderBy(er => er.ErrorCode).ToList();

             
                return resultArray;
            }

            catch (Exception ex)
            {
                throw;
            }

        }

        public Array  GetDetailsForErrorCode(string errorCode)
        {
            try
            {
                              
                var errorList = mongoDatabaseRunTime.GetCollection<ErrorLogDataAccess.DataClasses.ErrorMaster>("ErrorMaster").AsQueryable().ToList();
                

                var resultArray = errorList.AsQueryable()
                                    .Where(er=>er.ErrorCode.ToLower().Trim()== errorCode.ToLower().Trim())
                                    .Select(er => new
                                    {
                                        er.ErrorCode,
                                        er.DomainId,
                                        er.FieldId,
                                        er.ClientId,
                                        er.ErrorCaption,
                                        er.ErrorDescription,
                                        er.SolutionMasters

                                    }).ToArray();


                return resultArray;
            }

            catch (Exception ex)
            {
                throw;
            }

        }

        #endregion


        #region "Save Error/Solution"

        //To check error code is available
        public bool IsErrorCodeAvailable(string errorCode)
        {

            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                  
                    //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var domainCollection = mongoDatabaseRunTime.GetCollection<ErrorMaster>("ErrorMaster");



                    if (domainCollection.AsQueryable()
                      .Where(d => d.ErrorCode.ToLower() == errorCode.ToLower()).Count() > 0)
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

        public string GetNewErrorCode()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                  
                    //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var errorCollection = mongoDatabaseRunTime.GetCollection<ErrorMaster>("ErrorMaster");
                    
                    int newErrorID = 0;
                    string newCode = String.Empty;

                  
                    if (errorCollection.AsQueryable().Count() > 0)
                    {
                        
                        var maxID = errorCollection.AsQueryable()
                                   .OrderByDescending(a => a.ErrorId)
                                   .FirstOrDefault().ErrorId;

                        newErrorID = maxID + 1;
                        

                    }
                    else
                    {
                        
                        newErrorID = 1;
                    }

                    newCode = "ERL" + newErrorID.ToString().PadLeft(5, '0');

                    scope1.Complete();

                    return newCode;

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

        public int GetNewErrorID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                   
                    int newErrorID = 0;

                    var errorCollection = mongoDatabaseRunTime.GetCollection<ErrorMaster>("ErrorMaster");
                    
                    if (errorCollection.AsQueryable().Count() > 0)
                    {
                       
                        var maxID = errorCollection.AsQueryable()
                                   .OrderByDescending(a => a.ErrorId)
                                   .FirstOrDefault().ErrorId;

                        newErrorID = maxID + 1;


                    }
                    else
                    {
                        
                        newErrorID = 1;
                    }

                    scope1.Complete();

                    return newErrorID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }


        public string GetNewSolutionCode()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                   //Get the Post collection. By default, we'll use
                    //the name of the class as the collection name. Again,
                    //if it doesn't exist, MongoDB will create it when we first use it.
                    var solutionCollection = mongoDatabaseRunTime.GetCollection<SolutionMaster>("SolutionMaster");
                    int newSolutionID = 0;
                    string newCode = String.Empty;

                   

                    if (solutionCollection.AsQueryable().Count() > 0)
                    {
                        
                        var maxID = solutionCollection.AsQueryable()
                                   .OrderByDescending(a => a.SolutionId)
                                   .FirstOrDefault().SolutionId;

                        newSolutionID = maxID + 1;


                    }
                    else
                    {
                        newSolutionID = 1;
                       
                       
                    }

                    newCode = "SOL" + newSolutionID.ToString().PadLeft(5, '0');

                    scope1.Complete();
                    return newCode;

                }

                catch (Exception ex)
                {
                    scope1.Dispose();
                 
                    throw;
                }
            }

        }

        public int GetNewSolutionID()
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                   
                    int newSolutionID = 0;

                    var solutionCollection = mongoDatabaseRunTime.GetCollection<SolutionMaster>("SolutionMaster");

                    if (solutionCollection.AsQueryable().Count() > 0)
                    {
                     
                        var maxID = solutionCollection.AsQueryable()
                                   .OrderByDescending(a => a.SolutionId)
                                   .FirstOrDefault().SolutionId;

                        newSolutionID = maxID + 1;


                    }
                    else
                    {

                        newSolutionID = 1;
                    }

                    scope1.Complete();

                    return newSolutionID;

                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    throw;
                }
            }
        }




        public string SaveMainErrorDetails(ErrorMaster errorObjParam,SolutionMaster solObjParam,ErrorAttachment[] errorImageList,
                                           ErrorAttachment[] solutionImageList,bool isSolutionAddForError,bool isErrorWithSolution,int userId=1)
        {
            using (TransactionScope scope1 = new TransactionScope())
            {
                try
                {
                                     
                    string formattedDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    var ErrorCollection = mongoDatabaseRunTime.GetCollection<ErrorMaster>("ErrorMaster");
                    var SolutionCollection = mongoDatabaseRunTime.GetCollection<SolutionMaster>("SolutionMaster");

                    int newErrorID = GetNewErrorID();
                    int newSolutionID = GetNewSolutionID();

                    //To Convert Image array to list
                    List<ErrorAttachment> _ErrorImgList = new List<ErrorAttachment>();
                    List<ErrorAttachment> _SolutionImgList = new List<ErrorAttachment>();
                    ErrorAttachment _ErrorImgFile,_SolutionImageFile;

                    //To store solutions
                    List<SolutionMaster> _SolutionMasterList = new List<SolutionMaster>();

                    //Images for error
                    if (errorImageList != null)
                    {
                        foreach (var imgMedia in errorImageList)
                        {

                            _ErrorImgFile = new ErrorAttachment();
                            _ErrorImgFile.MediaType = imgMedia.MediaType;
                            _ErrorImgFile.MediaName = imgMedia.MediaName;
                            _ErrorImgFile.IsErrorAttachment = imgMedia.IsErrorAttachment;
                            _ErrorImgFile.MediaURL = imgMedia.MediaURL;
                            _ErrorImgFile.ClientId = errorObjParam.ClientId;
                            _ErrorImgFile.ErrorId = newErrorID;
                            _ErrorImgFile.IsErrorAttachment = imgMedia.IsErrorAttachment;
                            _ErrorImgFile.MediaMIMEType = imgMedia.MediaMIMEType;
                            _ErrorImgFile.MediaPath = imgMedia.MediaPath;
                           _ErrorImgList.Add(_ErrorImgFile);

                        }
                    }

                    //Images for solution
                    if (solutionImageList != null)
                    {
                        foreach (var imgMedia in solutionImageList)
                        {

                            _SolutionImageFile = new ErrorAttachment();
                            _SolutionImageFile.MediaType = imgMedia.MediaType;
                            _SolutionImageFile.MediaName = imgMedia.MediaName;
                            _SolutionImageFile.MediaURL = imgMedia.MediaURL;
                            _SolutionImageFile.ClientId = errorObjParam.ClientId;
                            _SolutionImageFile.ErrorId = newErrorID;
                            _SolutionImageFile.IsErrorAttachment = imgMedia.IsErrorAttachment;
                            _SolutionImageFile.MediaMIMEType = imgMedia.MediaMIMEType;
                            _SolutionImageFile.MediaPath = imgMedia.MediaPath;
                            _SolutionImgList.Add(_SolutionImageFile);

                        }
                    }

                    if (isErrorWithSolution == false && isSolutionAddForError == false)
                    {


                        var errorTuple = new ErrorLogDataAccess.DataClasses.ErrorMaster()
                        {
                            ErrorId = newErrorID,
                            ErrorCode = errorObjParam.ErrorCode,
                            DomainId = errorObjParam.DomainId,
                            FieldId = errorObjParam.FieldId,
                            ClientId = errorObjParam.ClientId,
                            ErrorCaption = errorObjParam.ErrorCaption,
                            ErrorDescription = errorObjParam.ErrorDescription,
                            UserId = userId,//Must get the userID from session later
                            ErrorLogDate =formattedDate,
                            ErrorAttachments = _ErrorImgList.ToList(),


                        };

                        ErrorCollection.InsertOneAsync(errorTuple);
                    }
                    else if (isErrorWithSolution == true)
                    {
                        var solutionTuple = new ErrorLogDataAccess.DataClasses.SolutionMaster()
                        {
                            SolutionId = newSolutionID,
                            SolutionCode = solObjParam.SolutionCode,
                            ErrorId = newErrorID,
                            DomainId = solObjParam.DomainId,
                            FieldId = solObjParam.FieldId,
                            ClientId = solObjParam.ClientId,
                            UserId = userId,
                            SolutionComment = solObjParam.SolutionComment,
                            SolutionLogDate = formattedDate,
                            ErrorAttachments = _SolutionImgList.ToList()

                        };

                        _SolutionMasterList.Add(solutionTuple);

                        var errorTuple = new ErrorLogDataAccess.DataClasses.ErrorMaster()
                        {
                            ErrorId = newErrorID,
                            ErrorCode = errorObjParam.ErrorCode,
                            DomainId = errorObjParam.DomainId,
                            FieldId = errorObjParam.FieldId,
                            ClientId = errorObjParam.ClientId,
                            ErrorCaption = errorObjParam.ErrorCaption,
                            ErrorDescription = errorObjParam.ErrorDescription,
                            UserId = 1,//Must get the userID from session later
                            ErrorLogDate = formattedDate,
                            ErrorAttachments = _ErrorImgList.ToList(),
                            SolutionMasters = _SolutionMasterList.ToList()


                        };

                        //First add the error
                        ErrorCollection.InsertOne(errorTuple);

                        //Then add the solution
                        SolutionCollection.InsertOne(solutionTuple);

                    }

                    else if (isSolutionAddForError == true)
                    {
                        //Get the specific error tuple/document ID
                        var errorTupleId = ErrorCollection.AsQueryable()
                                      .Where(a => a.ErrorCode==errorObjParam.ErrorCode)
                                      .FirstOrDefault().ErrorId;

                        //Get the existing solutions for this error
                        _SolutionMasterList = SolutionCollection.AsQueryable()
                                            .Where(a => a.ErrorId == errorTupleId).ToList();
                        
                         var solutionTuple = new ErrorLogDataAccess.DataClasses.SolutionMaster()
                        {
                            SolutionId = newSolutionID,
                            SolutionCode = solObjParam.SolutionCode,
                            ErrorId = errorTupleId,
                            DomainId = solObjParam.DomainId,
                            FieldId = solObjParam.FieldId,
                            ClientId = solObjParam.ClientId,
                            UserId = userId,
                            SolutionComment = solObjParam.SolutionComment,
                            SolutionLogDate = formattedDate,
                            ErrorAttachments = _SolutionImgList.ToList()

                        };

                        _SolutionMasterList.Add(solutionTuple);

                        //Add the solution to the document
                        SolutionCollection.InsertOne(solutionTuple);


                        var filterObj = Builders<ErrorLogDataAccess.DataClasses.ErrorMaster>.Filter.Eq("ErrorCode", errorObjParam.ErrorCode);

                        var updateObj = Builders<ErrorLogDataAccess.DataClasses.ErrorMaster>.Update
                                       .Set("LastUpdatedDate", formattedDate)
                                       .Set("SolutionMasters", _SolutionMasterList)
                                       .CurrentDate("lastModified");

                        //Then update the ErrorMaster document with the updated SolutionMaster list
                        ErrorCollection.UpdateOne(filterObj, updateObj);
                                                        

                    }


                    scope1.Complete();

                    return errorObjParam.ErrorCode;
                }
                catch (Exception ex)
                {
                    scope1.Dispose();
                    //return "0000";
                    throw;
                }

            }



        }

        //Save Media Files to Amazon S3 bucket
        public bool SaveFileToS3Bucket(System.IO.Stream localFilePath, string bucketName, string subDirectoryInBucket, string fileNameInS3)
        {
            try
            {
                IAmazonS3 client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast1);
                Amazon.S3.Transfer.TransferUtility utility = new TransferUtility(client);
                TransferUtilityUploadRequest request = new TransferUtilityUploadRequest();

                if (subDirectoryInBucket == "" || subDirectoryInBucket == null)
                {
                    request.BucketName = bucketName; //no subdirectory just bucket name  
                }
                else
                {   // subdirectory and bucket name  
                    request.BucketName = bucketName + @"/" + subDirectoryInBucket;
                }
                request.Key = fileNameInS3; //file name up in S3  
                request.InputStream = localFilePath;
                utility.Upload(request); //commensing the transfer  

                return true; //indicate that the file was sent  
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion



    }
}
