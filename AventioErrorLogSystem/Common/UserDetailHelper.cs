//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;
//using System.Web.Security;


//namespace AventioErrorLogSystem.Common
//{
//    public class UserDetailHelper
//    {
//        //HBSErrorLogEntities _HBSErrorLogEntities;
//        public int? userIDForPwdChange;

//        public string getUserFullName(string userName)
//        {
//            try
//            {
//                _HBSErrorLogEntities = new HBSErrorLogEntities();
//                string fullName = _HBSErrorLogEntities.SystemUsers
//                                 .Where(a => a.username == userName).FirstOrDefault().fullName;
//                return fullName;
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }



//        public int getuserID(string userName)
//        {
//            try
//            {
//                _HBSErrorLogEntities = new HBSErrorLogEntities();

//                int userID = _HBSErrorLogEntities.SystemUsers
//                                 .Where(a => a.username == userName).FirstOrDefault().userId;

//                return userID;
//            }
//            catch (Exception ex)
//            {
//                return 0;
//            }
//        }

        
//        public string getUserType(string username)
//        {
//            try
//            {
//                HBSErrorLogEntities _HBSErrorLogEntities = new HBSErrorLogEntities();
//                string userType = _HBSErrorLogEntities.SystemUsers
//                                .Where(a => a.username == username)
//                                .FirstOrDefault().SystemUserGroup.userGroupName;
//                userIDForPwdChange = getuserID(username);
//                return userType;
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }


              

//        //Check whether user is active
//        public bool isUserActive(string userName)
//        {
//            _HBSErrorLogEntities = new HBSErrorLogEntities();
//            bool isActive = _HBSErrorLogEntities.SystemUsers
//                           .Where(a => a.username == userName).FirstOrDefault().isActive;
//            return isActive;
//        }

        







//    }
//}