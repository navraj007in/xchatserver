using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using xChat.Models;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
namespace xChat.Controllers
{
    public class APIController : Controller
    {
        public static string GenerateRandomString(int length)
        {
            Random random = new Random();
            string characters = "@0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz._";
            StringBuilder result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                result.Append(characters[random.Next(characters.Length)]);
            }
            return result.ToString();
        }
        private bool ValidateAPIKey(string apikey)
        {
            
            return true;
        }
        //
        // GET: /API/
        xChatDataContext db = new xChatDataContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult getCities(int StateCode,String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                User iUser = new xChat.Models.User();
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            var rooms = (from rm in db.ChatRoomsListViews
                         where rm.StateId == StateCode
                         select rm).OrderBy(x=>x.RoomName).ToList();
            return Json(rooms, JsonRequestBehavior.AllowGet);


        }

        public ActionResult getRooms(int SubCatCode,String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                User iUser = new xChat.Models.User();
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }


            var subcatrooms = (from rm in db.ChatRoomsListViews
                         where rm.StateId == SubCatCode
                         select rm).OrderBy(x=>x.RoomName ).ToList();

            return Json(subcatrooms, JsonRequestBehavior.AllowGet);

        }

        public ActionResult getSubcategories(int CategoryCode,String ApiKey)
        {
            var subcat= (from xSub in db.Subcategories 
                          where xSub.Category == CategoryCode 
                          select xSub ).OrderBy(x=>x.Name).ToList();
            return Json(subcat , JsonRequestBehavior.AllowGet);

        }

        public ActionResult getStates(int Country,String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                User iUser = new xChat.Models.User();
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            var states = (from xCountries in db.States 
                          where xCountries.Country==Country
                             select xCountries).OrderBy(x=>x.State1).ToList();
            return Json(states , JsonRequestBehavior.AllowGet);

        }
        public ActionResult getCountries(String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                User iUser = new xChat.Models.User();
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            var countries = (from xCountries in db.Countries
                             select xCountries).OrderBy(x=>x.Country1).ToList();
            return Json(countries, JsonRequestBehavior.AllowGet);
        }
        public ActionResult getCategories(String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey==null)
            {
                User iUser = new xChat.Models.User();
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            var countries = (from xCategories in db.Categories
                             select xCategories).OrderBy(x=>x.Category1).ToList();
            return Json(countries, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendMessage(String Message,long UserID)
        {
            var countries = (from xCountries in db.Countries
                             select xCountries).ToList();
            return Json(countries, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Login(String Email, String iPass, String AppID)
        {
            User iUser = new xChat.Models.User();
            iUser.UserName = Email;
            iUser.Password = "";
            iUser.Token = "";
            Chatter member = (from mem in db.Chatters
                             where mem.Email == Email && mem.Password == iPass
                             select mem).SingleOrDefault();
            if (member != null)
            {
                String st = GenerateRandomString(32);
                iUser.Token = st;
                iUser.HTTPResponse = "Authentication Successful";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            else
            {
                iUser.HTTPResponse = "User Name/Password Incorrect.";
                return Content(JsonConvert.SerializeObject(iUser));
            }
        }
        public ActionResult getBlockedList(String token, String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                var rostr = (from xBlock in db.Blockeds
                             from xChat in db.Chatters
                             where (xBlock.UserID == chatter.ID
                             && xChat.ID == xBlock.BlockedUser)
                             select new
                             {
                                 NickName = xChat.NickName,
                                 ID = xChat.ID,
                                 Name = xChat.Name,
                                 Gender = xChat.Gender,
                                 City = xChat.City,
                                 Country = xChat.Country,
                                 ProfileImage = xChat.ProfileImage
                             }).ToList();
                return Content(JsonConvert.SerializeObject(rostr));

            }
            else
            {
                return null;
            }

        }

        public ActionResult GetProfile(String ApiKey,String nickname)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            var chatter = (from xChatter in db.Chatters
                               where xChatter.NickName == nickname
                               select xChatter);
            if (chatter != null)
            {
                return Json(chatter.AsEnumerable().Select(r => new
                {
                    ID=r.ID,
                    DOB=r.DOB.GetValueOrDefault().ToString("dd/MM/yyyy"),
                    Email=r.Email,
                    Name=r.Name,
                    NickName=r.NickName,
                    City=r.City,
                    State=r.State,
                    Country=r.Country,
                    Gender=r.Gender,
                    ProfileImage=r.ProfileImage,
                    Status=r.Status
                }), JsonRequestBehavior.AllowGet);
            //    return Content(JsonConvert.SerializeObject(chatter));

            }
            iUser.HTTPResponse = "404";
            return Content(JsonConvert.SerializeObject(iUser));

        }


        public ActionResult getAccessToken(String nickname)
        {

            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.NickName == nickname
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {

                db.SubmitChanges();
                iUser.HTTPResponse = "SUCCESS";
                return Content(JsonConvert.SerializeObject(chatter.AccessToken));

            }
            else
            {
                iUser.HTTPResponse = "Invalid Nickname";
                return Content(JsonConvert.SerializeObject(chatter.AccessToken));


            }

        }
        public ActionResult getJoiningSummary()
        {

            User iUser = new xChat.Models.User();

            var chatters = (from xChatter in db.Chatters
                            select new
                            {
                                xChatter.ID,
                                xChatter.Email,
                                xChatter.Name,
                                xChatter.NickName,
                                xChatter.City,
                                xChatter.State,
                                xChatter.Country,
                                xChatter.DeviceID
                            }).ToList().OrderByDescending(x => x.ID);

            db.SubmitChanges();
            return Content(JsonConvert.SerializeObject(chatters));

        }

        public ActionResult getChatters()
        {

            User iUser = new xChat.Models.User();

            var chatters = (from xChatter in db.Chatters
                            select new
                            {
                                xChatter.ID,
                                xChatter.Email,
                                xChatter.Name,
                                xChatter.NickName,
                                xChatter.City,
                                xChatter.State,
                                xChatter.Country,
                                xChatter.DeviceID
                            }).ToList().OrderByDescending(x=>x.ID);

                db.SubmitChanges();
                return Content(JsonConvert.SerializeObject(chatters));

        }
        public ActionResult GetRecentlyJoined(int page, int pagesize)
        {
            var chatters = (from xChatter in db.Chatters
                            select new
                            {
                                xChatter.ID,
                                xChatter.Name,
                                xChatter.NickName,
                                xChatter.City,
                                xChatter.State,
                                xChatter.Country,
                                xChatter.DOJ,
                                xChatter.DOJStamp,
                                xChatter.ProfileImage
                            }).ToList().OrderByDescending(x => x.ID)
                            .Skip((page - 1) * pagesize).Take(pagesize);

            return Json(chatters.AsEnumerable()
                .Select(r => new
                {
                    ID = r.ID,
                    Name = r.Name,
                    NickName=r.NickName,
                    City=r.City,
                    State=r.State,
                    Country=r.Country,
                    ProfileImage=r.ProfileImage,
                    DOJStamp=r.DOJ.GetValueOrDefault().ToString("yyyy-MM-dd"),
                    DOJ = r.DOJStamp.GetValueOrDefault().ToString("yyyy-MM-dd hh:mm:ss")
                }), JsonRequestBehavior.AllowGet);
        }
        public string getIP()
        {
            return Request.ServerVariables["REMOTE_ADDR"];
        }
        public ActionResult GetActiveUsers(int page, int pagesize)
        {
            var chatters = (from xChatter in db.Chatters
                            from xChatRoom in db.ChatRooms
                            from xRoom in db.Rooms 
                            where xChatter.ID== xChatRoom.UserID
                            && xRoom.ID== xChatRoom.RoomID 
                            && xChatter.DeviceID.Length>0
                            select new
                            {
                                xChatter.ID,
                                xChatter.Name,
                                xChatter.NickName,
                                xChatter.City,
                                xChatter.State,
                                xChatter.Country,
                                xChatter.DOJ,
                                xChatter.DOJStamp,
                                xChatter.ProfileImage,
                                xRoom.RoomName 
                            }).ToList().OrderByDescending(x => x.ID)
                            .Skip((page - 1) * pagesize).Take(pagesize);

            return Json(chatters.AsEnumerable()
                .Select(r => new
                {
                    ID = r.ID,
                    Name = r.Name,
                    NickName = r.NickName,
                    City = r.City,
                    State = r.State,
                    Country = r.Country,
                    RoomName=r.RoomName ,
                    ProfileImage = r.ProfileImage,
                    DOJStamp = r.DOJ.GetValueOrDefault().ToString("yyyy-MM-dd"),
                    DOJ = r.DOJStamp.GetValueOrDefault().ToString("yyyy-MM-dd hh:mm:ss")
                }), JsonRequestBehavior.AllowGet);
        }
        public string insertLog(int senderID,int RecieverID,
            int MessageType,DateTime MessageStamp,String MessageBody,
            DateTime MessageDate)
        {
            MessageLog iMessageLog = new MessageLog();
            iMessageLog.MessageBody = MessageBody;
            iMessageLog.SenderID = senderID;
            iMessageLog.RecieverID = RecieverID;
            iMessageLog.MessageType = MessageType;
            iMessageLog.MessageStamp = MessageStamp;
            iMessageLog.MessageBody = MessageBody;
            iMessageLog.MessageDate = MessageDate;

            db.MessageLogs.InsertOnSubmit(iMessageLog);
            db.SubmitChanges();

            return "";
        }
        public ActionResult getTopRooms(String ApiKey)
        {

            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                User iUser = new xChat.Models.User();
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            var rooms = (from rm in db.TopChatRoomsListViews
                         select rm).ToList();
            return Json(rooms, JsonRequestBehavior.AllowGet);


        }

        public ActionResult getActiveRooms()
        {

            var room = from s in db.Rooms
                       join p in db.ChatRooms on s.ID equals p.RoomID
                       group s by new { s.ID, s.RoomName }
                           into result
                           from subset in result.DefaultIfEmpty()
                           select new
                           {
                                RoomId = result.Key.ID,
                                RoomName = result.Key.RoomName,
                                RoomUsers = (subset==null ? 0:result.Count())

                           };
            var rec = room.Distinct();
            return Content(JsonConvert.SerializeObject(rec));


        }
        
        public ActionResult Logout(String token, String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                chatter.DeviceID = "";

                db.SubmitChanges();
                iUser.HTTPResponse = "SUCCESS";
                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "404";
                return Content(JsonConvert.SerializeObject(iUser));

            }

        }

            public ActionResult JoinRoom(int RoomID,int UserID,String DeviceID,String AccessToken,String ApiKey)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            Chatter chatter = (from xChatter in db.Chatters
                             where xChatter.AccessToken== AccessToken
                             select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                int count = (from curRoom in db.ChatRooms
                             where curRoom.RoomID == RoomID
                             select curRoom.UserID).Count();
                if (count > 50)
                {
                    iUser.HTTPResponse = "FULL";
                    return Json(iUser, JsonRequestBehavior.AllowGet);

                }

                ChatRoom existingRoom = (from exRoom in db.ChatRooms
                                         where exRoom.UserID == chatter.ID
                                         select exRoom).FirstOrDefault();
                if (existingRoom != null)
                {
                    db.ChatRooms.DeleteOnSubmit(existingRoom);
                    db.SubmitChanges();
                }

                long rID = (from xRoom in db.ChatRooms
                            select xRoom.ID).Max();
                rID++;

                ChatRoom xChat = new ChatRoom();
                xChat.DeviceID = DeviceID;
                xChat.UserID = chatter.ID;
                xChat.RoomID = RoomID;
                xChat.ID = rID;
                db.ChatRooms.InsertOnSubmit(xChat);

                Chatter exChatter = (from xChatUser in db.Chatters
                                     where xChatUser.ID == UserID
                                     select xChatUser).FirstOrDefault();
                if (exChatter != null)
                {
                    exChatter.DeviceID = DeviceID;

                }
                db.SubmitChanges();

                iUser.RoomUsers = GetRoomUsersObject(RoomID);
                iUser.HTTPResponse = "Room Entered Successfully";
                return Json(iUser, JsonRequestBehavior.AllowGet);

            }
            else
            {
                iUser.HTTPResponse = "404-Invalid Login";
                return Json(iUser, JsonRequestBehavior.AllowGet);

            }

        
        }
        public Object  GetRoomUsersObject(int RoomID)
        {
            var roomUsers = (from iRoom in db.ChatRooms
                             from iUsers in db.Chatters
                             where iRoom.RoomID == RoomID
                             && iRoom.UserID == iUsers.ID
                             select new
                             {
                                 User = iUsers.Email,
                                 NickName = iUsers.NickName,
                                 Name = iUsers.Name,
                                 ID = iUsers.ID

                             }).ToList();
            return roomUsers;
        }

        public ActionResult GetRoomUsers(int RoomID,String ApiKey)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            var roomUsers = (from iRoom in db.ChatRooms
                                        from iUsers in db.Chatters
                                        where iRoom.RoomID == RoomID
                                        && iRoom.UserID == iUsers.ID
                                        select new
                                        {
                                            User=iUsers.Email ,
                                            NickName=iUsers.NickName,
                                            Name=iUsers.Name,
                                            ID=iUsers.ID 

                                        }).ToList();


            iUser.RoomUsers = GetRoomUsersObject(RoomID);

            return Content(JsonConvert.SerializeObject(iUser));



        }
        public ActionResult LeaveRoom( int UserID,String ApiKey,String AccessToken)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            Chatter chatter = (from xChatter in db.Chatters
                             where xChatter.AccessToken== AccessToken
                             select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                ChatRoom ID = (from xChat in db.ChatRooms
                               where xChat.UserID == chatter.ID
                               select xChat).FirstOrDefault();


                if (ID != null)
                {
                    db.ChatRooms.DeleteOnSubmit(ID);
                    db.SubmitChanges();
                    iUser.HTTPResponse = "Room Left Successfully";
                }
                else
                {
                    iUser.HTTPResponse = "No such entry";
                }
                return Json(iUser, JsonRequestBehavior.AllowGet);

            }
            else
            {
                iUser.HTTPResponse = "404-Invalid Login";
                return Json(iUser, JsonRequestBehavior.AllowGet);

            }
        }

        public String RegisterDevice(int UserID, String DeviceId,String ApiKey)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return iUser.HTTPResponse;
            }

            Chatter xUser = (from xChatter in db.Chatters
                          where xChatter.ID == UserID
                          select xChatter).FirstOrDefault();
            if (xUser != null)
            {
                xUser.DeviceID = DeviceId;
                db.SubmitChanges();

            }
            return "Device ID updated";
        }
        public ActionResult BuzzUser(int UserID,int fromID,String fromUser,String ApiKey,String AccessToken)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }


            string DeviceID = (from xChat in db.Chatters
                               where xChat.ID == UserID
                               select xChat.DeviceID).FirstOrDefault();
            string[] RegistrationID = new string[1];
            RegistrationID[0] = DeviceID;
            Message iMessage = new Message();
            iMessage.From = fromUser ;
            iMessage.Type = "PM";
            iMessage.MessageBody = "You Recieved a buzz from " + fromUser ;
            iMessage.RoomID = 0;
            iMessage.ContentType = "buzz";
            iMessage.TimeStamp = DateTime.Now.ToString();
            String jMessage = JsonConvert.SerializeObject(iMessage);
            SendMessage(RegistrationID, jMessage, "", "C");
            return null;
        }
        public ActionResult BuzzzUser(String nickName, String fromUser, String ApiKey, String AccessToken)
        {
            User iUser = new xChat.Models.User();

            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            Chatter chatter = (from xChatter in db.Chatters
                             where xChatter.AccessToken== AccessToken
                             select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Chatter toChatter = (from xToChatter in db.Chatters
                                     where xToChatter.NickName == nickName
                                     select xToChatter).FirstOrDefault();
                if (toChatter == null)
                {
                    iUser.HTTPResponse = "Invalid Nickname.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                string DeviceID = toChatter.DeviceID;
                string[] RegistrationID = new string[1];
                RegistrationID[0] = DeviceID;


                Message iMessage = new Message();
            iMessage.From = chatter.NickName;
            iMessage.Type = "PM";
            iMessage.MessageBody = "You Recieved a buzz from " + chatter.NickName;
            iMessage.RoomID = 0;
            iMessage.ContentType = "buzz";
            iMessage.TimeStamp = DateTime.Now.ToString();
            String jMessage = JsonConvert.SerializeObject(iMessage);
            SendMessage(RegistrationID, jMessage, "", "C");

            }
            return null;
        }
        public ActionResult SendContact(String to, String displayName, int fromId,
            String fromUsr, String AccessToken, String ApiKey, String number)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == AccessToken
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Chatter toChatter = (from xToChatter in db.Chatters
                                     where xToChatter.NickName == to
                                     select xToChatter).FirstOrDefault();
                if (toChatter == null)
                {
                    iUser.HTTPResponse = "404";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                string DeviceID = toChatter.DeviceID;
                string[] RegistrationID = new string[1];
                RegistrationID[0] = DeviceID;
                Message iMessage = new Message();
                iMessage.From = chatter.NickName;
                iMessage.Type = "Contact";
                iMessage.MessageBody = displayName;
                iMessage.RoomID = fromId;
                iMessage.ContentType = "text";
                iMessage.fromId = Convert.ToString(fromId);
                iMessage.number = number;
                iMessage.TimeStamp = DateTime.Now.ToString();

                String jMessage = JsonConvert.SerializeObject(iMessage);
                SendMessage(RegistrationID, jMessage, "", "C");
                iUser.HTTPResponse = "SUCCESS";
                iUser.subResponse = number;

                return Content(JsonConvert.SerializeObject(iUser));
            }
            else
            {
                iUser.HTTPResponse = "401";
            }
            return Content(JsonConvert.SerializeObject(iUser));

        }

        public ActionResult SendPM(String to, String Message, int fromId, String fromUsr, String AccessToken, String ApiKey, String mid)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            Chatter chatter = (from xChatter in db.Chatters
                             where xChatter.AccessToken== AccessToken
                             select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Chatter toChatter = (from xToChatter in db.Chatters
                                   where xToChatter.NickName == to
                                   select xToChatter).FirstOrDefault();
                if (toChatter == null)
                {
                    iUser.HTTPResponse = "Invalid Nickname.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                string DeviceID = toChatter.DeviceID;
                string[] RegistrationID = new string[1];
                RegistrationID[0] = DeviceID;
                Message iMessage = new Message();
                iMessage.From = chatter.NickName;
                iMessage.Type = "PM";
                iMessage.MessageBody = Message;
                iMessage.RoomID = fromId;
                iMessage.ContentType = "text";
                iMessage.fromId = Convert.ToString(fromId);
                iMessage.mid = mid;
                iMessage.TimeStamp = DateTime.Now.ToString();

                String jMessage = JsonConvert.SerializeObject(iMessage);
                SendMessage(RegistrationID, jMessage, "", "C");
                iUser.HTTPResponse = "Message sent successfully";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            else
            {
                iUser.HTTPResponse = "Wrong User Credentials!!!";
            }
            return Content(JsonConvert.SerializeObject(iUser));

        }
        public ActionResult updateStatus(String token, String ApiKey, String Status)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                chatter.Status = Status;

                db.SubmitChanges();
                iUser.HTTPResponse = "SUCCESS";
                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "Invalid User Credentials";
                return Content(JsonConvert.SerializeObject(iUser));

            }

        }

        [HttpGet]
        public ActionResult SendFlash(String to, String Message,
            String AccessToken, String ApiKey)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == AccessToken
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Chatter toChatter = (from xToChatter in db.Chatters
                                     where xToChatter.NickName == to
                                     select xToChatter).FirstOrDefault();
                if (toChatter == null)
                {
                    iUser.HTTPResponse = "Invalid Nickname.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                string DeviceID = toChatter.DeviceID;
                string[] RegistrationID = new string[1];
                RegistrationID[0] = DeviceID;
                Message iMessage = new Message();
                iMessage.From = chatter.NickName;
                iMessage.Type = "PM";
                iMessage.MessageBody = Message;
                iMessage.RoomID = 0;
                iMessage.ContentType = "flash";
                iMessage.fromId = chatter.NickName;

                iMessage.TimeStamp = DateTime.Now.ToString();

                String jMessage = JsonConvert.SerializeObject(iMessage);
                SendMessage(RegistrationID, jMessage, "", "C");
                iUser.HTTPResponse = "Message sent successfully";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            else
            {
                iUser.HTTPResponse = "Wrong User Credentials!!!";
            }
            return Content(JsonConvert.SerializeObject(iUser));

        }

        public ActionResult SendRequest(String token, String ApiKey, String NickName,String Message)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }
            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Chatter friendUser = (from xFriend in db.Chatters
                                       where xFriend.NickName == NickName
                                       select xFriend).FirstOrDefault();
                if (friendUser != null)
                {
                    var curFriend = (from xBlock in db.Friends
                                     where xBlock.FriendID == Convert.ToString( friendUser.ID)
                                     && xBlock.UserID == Convert.ToString(chatter.ID)
                                     select xBlock).FirstOrDefault();
                    if (curFriend != null)
                    {
                        iUser.HTTPResponse = "You are already friends with " + friendUser.NickName;
                        return Content(JsonConvert.SerializeObject(iUser));

                    }

                    var curRequest = (from xRequest in db.Requests 
                                      where xRequest.UserID == friendUser.ID
                                      && xRequest.SenderID==chatter.ID
                                      select xRequest).FirstOrDefault();
                    if (curRequest != null)
                    {
                        iUser.HTTPResponse = "Request Already Sent";
                        return Content(JsonConvert.SerializeObject(iUser));

                    }

                    Request fRequest = new xChat.Request();
                    fRequest.SenderID= chatter.ID;
                    fRequest.UserID = friendUser.ID;
                    fRequest.Message = Message;

                    db.Requests.InsertOnSubmit(fRequest);
                    db.SubmitChanges();

                    string[] RegistrationID = new string[1];
                    RegistrationID[0] = friendUser.DeviceID;
                    Message iMessage = new Message();
                    iMessage.From = chatter.NickName;
                    iMessage.Type = "BuddyRequest";
                    iMessage.MessageBody = Message;
                    iMessage.RoomID = 0;
                    iMessage.ContentType = "BuddyRequest";
                    iMessage.TimeStamp = DateTime.Now.ToString();
                    String jMessage = JsonConvert.SerializeObject(iMessage);
                    SendMessage(RegistrationID, jMessage, "", "C");

                    iUser.HTTPResponse = "SUCCESS";

                }
                else
                {
                    iUser.HTTPResponse = "Invalid User Name.Please try again.";
                }
                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "Invalid User Credentials";
                return Content(JsonConvert.SerializeObject(iUser));

            }

        }

        public ActionResult replyBuddyRequest(String token, String ApiKey, String NickName,string Response,String Message)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }
            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                            Chatter fchatter = (from xfChatter in db.Chatters
                               where xfChatter.NickName == NickName
                               select xfChatter).FirstOrDefault();
                if(fchatter!=null)
                {
                if (Response == "Yes")
                {

                    Friend iFriend = new Friend();
                    iFriend.UserID = Convert.ToString(chatter.ID);
                    iFriend.FriendID = Convert.ToString( fchatter.ID);

                    Friend uFriend = new Friend();
                    uFriend.UserID = Convert.ToString(fchatter.ID);
                    uFriend.FriendID = Convert.ToString(chatter.ID);
                    db.Friends.InsertOnSubmit(iFriend);
                    db.Friends.InsertOnSubmit(uFriend);

                    iUser.HTTPResponse="Added Successfully";

                }
                else
                {
                    iUser.HTTPResponse="Request Rejected";

                }

                Request irRequest = (from xReq in db.Requests
                                    where xReq.UserID == chatter.ID
                                    && xReq.SenderID == fchatter.ID
                                    select xReq).FirstOrDefault();
                db.Requests.DeleteOnSubmit(irRequest);

                db.SubmitChanges();

                }
                else
                {
                    iUser.HTTPResponse="Invalid Request";
                }

                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "Invalid User Credentials";
                return Content(JsonConvert.SerializeObject(iUser));

            }

        }

        public ActionResult hotlistUser(String token, String ApiKey,String NickName)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }
            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Chatter hotlistUser = (from xHotLister in db.Chatters
                                        where xHotLister.NickName == NickName
                                        select xHotLister).FirstOrDefault();
                if (hotlistUser != null)
                {
                    var curHotlist = (from xBlock in db.Hotlists 
                                      where xBlock.Hotlister== hotlistUser.ID
                                      && xBlock.UserID==chatter.ID
                                      select xBlock).FirstOrDefault();
                    if (curHotlist != null)
                    {
                        iUser.HTTPResponse = "User Already Hotlisted";
                        return Content(JsonConvert.SerializeObject(iUser));

                    }

                    Hotlist addHotlist = new Hotlist();

                    addHotlist.UserID = chatter.ID;
                    addHotlist.Hotlister = hotlistUser.ID;
                    db.Hotlists.InsertOnSubmit(addHotlist);
                    db.SubmitChanges();
                    iUser.HTTPResponse = NickName + " Hotlisted Successfully";

                }
                else
                {
                    iUser.HTTPResponse = "Invalid User Name.Please try again.";
                }
                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "Invalid User Credentials";
                return Content(JsonConvert.SerializeObject(iUser));

            }

        }
        public ActionResult removeFromHotlist(String token, String ApiKey, String NickName)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Hotlist blockedUsers = (from blockeds in db.Hotlists
                                        where blockeds.UserID == chatter.ID 
                                        select blockeds).FirstOrDefault();
                if (blockedUsers != null)
                {
                    db.Hotlists.DeleteOnSubmit(blockedUsers);
                    db.SubmitChanges();
                }

                iUser.HTTPResponse = "SUCCESS";
                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "Invalid User Credentials";
                return Content(JsonConvert.SerializeObject(iUser));

            }
        }

        public ActionResult blockUser(String token, String ApiKey,String NickName)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Chatter hotlistUser = (from xHotLister in db.Chatters
                                       where xHotLister.NickName == NickName
                                       select xHotLister).FirstOrDefault();
                if (hotlistUser != null)
                {
                    var curBlocked = (from xBlock in db.Blockeds
                                      where xBlock.BlockedUser == hotlistUser.ID
                                      && xBlock.UserID==chatter.ID
                                      select xBlock).FirstOrDefault();
                    if (curBlocked != null)
                    {
                        iUser.HTTPResponse = "User Already Blocked";
                        return Content(JsonConvert.SerializeObject(iUser));

                    }
                    Blocked addHotlist = new Blocked();

                    addHotlist.UserID = chatter.ID;
                    addHotlist.BlockedUser= hotlistUser.ID;
                    db.Blockeds.InsertOnSubmit(addHotlist);
                    db.SubmitChanges();
                    iUser.HTTPResponse = "SUCCESS";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                else
                {
                    iUser.HTTPResponse = "Invalid User Name.Please try again.";
                }
                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "Invalid User Credentials";
                return Content(JsonConvert.SerializeObject(iUser));

            }

        }
        public ActionResult unblockUser(String token, String ApiKey, String NickName)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            User iUser = new xChat.Models.User();

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Blocked blockedUsers = (from blockeds in db.Blockeds
                                         where blockeds.UserID == chatter.ID
                                         select blockeds).FirstOrDefault();
                if (blockedUsers != null)
                {
                    db.Blockeds.DeleteOnSubmit(blockedUsers);
                    db.SubmitChanges();
                }

                iUser.HTTPResponse = "SUCCESS";
                return Content(JsonConvert.SerializeObject(iUser));

            }
            else
            {
                iUser.HTTPResponse = "Invalid User Credentials";
                return Content(JsonConvert.SerializeObject(iUser));

            }
        }

        public ActionResult getRequests(String token, String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                var rostr = (from xReq in db.Requests
                             from xChat in db.Chatters
                             where (xReq.UserID == chatter.ID 
                             && xChat.ID==xReq.SenderID)
                             select new
                             {
                                 NickName = xChat.NickName,
                                 ID = xChat.ID,
                                 Name = xChat.Name,
                                 Gender = xChat.Gender,
                                 City = xChat.City,
                                 Country = xChat.Country,
                                 Message =xReq.Message
                             }).ToList();
                return Content(JsonConvert.SerializeObject(rostr));

            }
            else
            {
                return null;
            }

        }

        public ActionResult getRoster(String token,String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            Chatter chatter = (from xChatter in db.Chatters
                               where xChatter.AccessToken == token
                               select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                var rostr = (from xRoom in db.Friends
                             from xChat in db.Chatters
                             where (xRoom.UserID == Convert.ToString(chatter.ID) &&
                             xChat.ID == Convert.ToInt64(xRoom.FriendID))
                             select new
                             {
                                 NickName = xChat.NickName,
                                 ID = xChat.ID,
                                 Name = xChat.Name,
                                 Gender = xChat.Gender,
                                 City = xChat.City,
                                 State = xChat.State,
                                 Zip = xChat.Status,
                                 Country = xChat.Country,
                                 ProfileImage = xChat.ProfileImage,
                                 Status = xChat.Status
                             }).OrderBy(x => x.Name).ToList();
                return Content(JsonConvert.SerializeObject(rostr));

            }
            else
            {
                return null;
            }

        }
        public ActionResult Authenticate(String AppID, String Email,String OS, String DeviceID,String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                return Content(JsonConvert.SerializeObject(null));
            }

            try
            {
                var chatusr = (from chatter in db.Chatters
                               where chatter.Email == Email
                               select new
                               {
                                   userExists = "Yes",
                                   Email = chatter.Email,
                                   NickName = chatter.NickName,
                                   Name = chatter.Name,
                                   Gender = chatter.Gender,
                                   DOB = chatter.DOB.GetValueOrDefault().ToShortDateString(),
                                   City = chatter.City,
                                   State = chatter.State,
                                   Country = chatter.Country,
                                   Token = GenerateRandomString(32),
                                   ProfileImage = chatter.ProfileImage
                               }).FirstOrDefault();
                if (chatusr != null)
                {
                    Chatter xChatter = (from xChatUser in db.Chatters
                                        where xChatUser.Email == chatusr.Email
                                        select xChatUser).FirstOrDefault();
                    if (xChatter != null)
                    {
                        xChatter.AccessToken = chatusr.Token;
                        xChatter.DeviceID = DeviceID;
                    }
                    db.SubmitChanges();

                    return Content(JsonConvert.SerializeObject(chatusr));
                }
                else
                {
                    var response = new
                    {
                        userExists = "No",
                        Email = "",
                        NickName = "",
                        Name = "",
                        Gender = "",
                        DOB = "",
                        City = "",
                        State = "",
                        Country = ""

                    };
                    return Content(JsonConvert.SerializeObject(response));
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public ActionResult RegisterGPlus(String AppID, String Name,
String NickName, String Email, String DeviceID)
        {
            User iUser = new xChat.Models.User();

            try
            {
                int count = (from cmem in db.Chatters
                             where cmem.Email == Email
                             select cmem.ID).Count();
                if (count > 0)
                {
                    iUser.HTTPResponse = "UserName already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                count = (from cmem in db.Chatters
                         where cmem.NickName == NickName
                         select cmem.ID).Count();
                if (count > 0)
                {
                    iUser.HTTPResponse = "Nick Name already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                String st = GenerateRandomString(32);

                Chatter mem = new Chatter
                {
                    Name = Name,
                    NickName = NickName,
                    Email = Email,
                };
                iUser.UserName = NickName;
                iUser.Token = st;
                db.Chatters.InsertOnSubmit(mem);
                db.SubmitChanges();
                iUser.HTTPResponse = "Register Successful";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            catch (Exception ex)
            {
                iUser.HTTPResponse = ex.Message;
                return Content(JsonConvert.SerializeObject(iUser));

            }
        }

        public ActionResult Register(String AppID, String Name,
        String NickName, String ZipCode, String Gender, String Category,
            String BusinessName, String Email, String password,DateTime DOB,String City,
            String State,String Country,String DeviceID,String IMEI,String ApiKey)
        {
            User iUser = new xChat .Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            try
            {
                int count = (from cmem in db.Chatters 
                             where cmem.Email == Email
                             select cmem.ID).Count();
                if (count > 0)
                {
                    iUser.HTTPResponse = "UserName already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                count = (from cmem in db.Chatters
                         where cmem.NickName == NickName
                         select cmem.ID).Count();
                if (count > 0)
                {
                    iUser.HTTPResponse = "Nick Name already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                if (NickName.Length < 6)
                {
                    iUser.HTTPResponse = "Nick Name already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));
                }
                if(!IsUsername(NickName))
                {
                    iUser.HTTPResponse = "User Name has invalid characters!Please Try again.";
                    return Content(JsonConvert.SerializeObject(iUser));
                }
                String st = GenerateRandomString(32);

                Chatter  mem = new Chatter 
                {
                    Name = Name,
                    NickName = NickName,
                    Gender = Gender,
                    Email = Email,
                    Password = password,
                    City=City,
                    State=State,
                    Country=Country,
                    DeviceID=DeviceID,
                    AccessToken=st
                };
                iUser.Token = st;
                db.Chatters .InsertOnSubmit(mem);
                db.SubmitChanges();
                iUser.HTTPResponse = "Register Successful";
                return Content(JsonConvert.SerializeObject(iUser));


            }
            catch (Exception ex)
            {
                iUser.HTTPResponse = ex.Message;
                return Content(JsonConvert.SerializeObject(iUser));

            }
        }

        [HttpPost]
        public ActionResult RegisterPost(String AppID, String Name,
String NickName, String ZipCode, String Gender, String Category,
    String BusinessName, String Email, String password, DateTime DOB, String City,
    String State, String Country, String DeviceID, String IMEI, String ApiKey,String ProfileImage)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }

            try
            {
                int count = (from cmem in db.Chatters
                             where cmem.Email == Email
                             select cmem.ID).Count();
                if (count > 0)
                {
                    iUser.HTTPResponse = "UserName already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                count = (from cmem in db.Chatters
                         where cmem.NickName == NickName
                         select cmem.ID).Count();
                if (count > 0)
                {
                    iUser.HTTPResponse = "Nick Name already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));

                }
                if (NickName.Length < 6)
                {
                    iUser.HTTPResponse = "Nick Name already in use.";
                    return Content(JsonConvert.SerializeObject(iUser));
                }
                if (!IsUsername(NickName))
                {
                    iUser.HTTPResponse = "User Name has invalid characters!Please Try again.";
                    return Content(JsonConvert.SerializeObject(iUser));
                }
                String st = GenerateRandomString(32);

                Chatter mem = new Chatter
                {
                    Name = Name,
                    NickName = NickName,
                    Gender = Gender,
                    Email = Email,
                    Password = password,
                    City = City,
                    State = State,
                    Country = Country,
                    DeviceID = DeviceID,
                    AccessToken = st
                };
                iUser.Token = st;
                db.Chatters.InsertOnSubmit(mem);
                db.SubmitChanges();
                iUser.HTTPResponse = "Register Successful";
                return Content(JsonConvert.SerializeObject(iUser));


            }
            catch (Exception ex)
            {
                iUser.HTTPResponse = ex.Message;
                return Content(JsonConvert.SerializeObject(iUser));

            }
        }

        private class roomUsers
        {
            public long UserID;
            public string DeviceID="";
            public string xMessage;
            public string UserName;
        }
        public ActionResult getCitiesCount(int StateCode, String ApiKey)
        {
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                User iUser = new xChat.Models.User();
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return Content(JsonConvert.SerializeObject(iUser));
            }
            var room = from s in db.Rooms
                       join p in db.ChatRooms on s.ID equals p.RoomID
                       group s by new { s.ID, s.RoomName }
                           into result
                           from subset in result.DefaultIfEmpty()
                           select new
                           {
                                RoomId = result.Key.ID,
                                RoomName = result.Key.RoomName,
                                RoomUsers = (subset==null ? 0:result.Count())

                           };


            return Json(room, JsonRequestBehavior.AllowGet);

        }

        public string SendMessage(string[] RegistrationID,
		string Message, string AuthSting, string Ntype)
        {
            //-- Create C2DM Web Request O
            HttpWebRequest Request = (HttpWebRequest)
            WebRequest.Create("https://android.googleapis.com/gcm/send");
            Request.Method = "POST";
            Request.KeepAlive = false;
            string RegArr = string.Empty;
            RegArr = string.Join("\",\"", RegistrationID);
            string postData;
            if (Ntype == "C")// Send Single Notification For all messages of a
            	// regid if Device is not active by setting Collapse_Key value
            	// same for a particular regid each time
            {
                postData = "{ \"registration_ids\": [ \"" + RegArr + "\" ] ,\"data\": " + Message +", \"collapse_key\":\"" + Ntype + "\"}";


            }
            else
            {
                postData = "{ \"registration_ids\": [ \"" + RegArr + "\" ] ,\"data\": " + Message + ",\"collapse_key\":\"" + Message + "\"}}";
            }
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            Request.ContentType = "application/json";
            Request.Headers.Add("Authorization", "key=AIzaSyB5OwjkHSF0b3inUete_QC22VqGiGeL5Fs");
            ServicePointManager.ServerCertificateValidationCallback += delegate(
                       object
                       sender,
                       System.Security.Cryptography.X509Certificates.X509Certificate
                       pCertificate,
                       System.Security.Cryptography.X509Certificates.X509Chain pChain,
                       System.Net.Security.SslPolicyErrors pSSLPolicyErrors)
           {
               return true;
           };

            //-- Create Stream to Write Byte Array --//
            Stream dataStream = Request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            //-- Post a Message --//

            WebResponse Response = Request.GetResponse();
            HttpStatusCode ResponseCode = ((HttpWebResponse)Response).StatusCode;
            if (ResponseCode.Equals(HttpStatusCode.Unauthorized) ||
            	ResponseCode.Equals(HttpStatusCode.Forbidden))
            {
                return "Unauthorized - need new token";
            }
            else if (!ResponseCode.Equals(HttpStatusCode.OK))
            {
                return "Response from web service isn't OK";
            }

            StreamReader Reader = new StreamReader(Response.GetResponseStream());
            string responseLine = Reader.ReadLine();
            Reader.Close();

            return responseLine;
        }
        public string SendMessageToRoomOld(int RoomID,String Message, String DeviceID,String User)
        {
            Message iMessage=new Message ();
            iMessage.From=User ;
            iMessage.Type = "RM";
            iMessage.MessageBody=Message ;
            iMessage.RoomID = RoomID;
            iMessage.ContentType = "text";
            iMessage.TimeStamp=DateTime.Now.ToString();
            String  jMessage = JsonConvert.SerializeObject(iMessage);

            List<roomUsers> roomUsers = (from xChat in db.Chatters
                             from xChatRoom in db.ChatRooms
                             where xChatRoom.RoomID == RoomID
                             && xChatRoom.UserID == xChat.ID
                             select new roomUsers
                             {
                                 UserID=xChat.ID,
                                 DeviceID=xChat.DeviceID,
                                 xMessage=jMessage ,
                                 UserName=xChat.NickName
                             }).ToList();

            string regIDs="[";
            int i = 0;
            string[] regids=new string[roomUsers.Count];
            foreach (roomUsers ru in roomUsers)
            {
                regids[i++] = ru.DeviceID;
                regIDs += "\"";
                regIDs += ru.DeviceID;
                regIDs += "\",";
            }
            regIDs = regIDs.Substring(0, regIDs.Length - 1) +"]";
            string response=SendMessage(regids, jMessage, "", "C");
            return response;
        }
      
        public string SendMessageToRoom(int RoomID, String Message, String DeviceID, 
            String User,
            String NickName,String AccessToken,String ApiKey)
        {
            User iUser = new xChat.Models.User();
            if (!ValidateAPIKey(ApiKey) || ApiKey == null)
            {
                iUser.HTTPResponse = "Invalid Client Login Credentials";
                return iUser.HTTPResponse;
            }

            Chatter chatter = (from xChatter in db.Chatters
                             where xChatter.AccessToken== AccessToken
                             select xChatter).FirstOrDefault();
            if (chatter != null)
            {
                Message iMessage = new Message();
                iMessage.From = chatter.NickName;
                iMessage.Type = "RM";
                iMessage.MessageBody = Message;
                iMessage.RoomID = RoomID;
                iMessage.ContentType = "text";
                iMessage.TimeStamp = DateTime.Now.ToString();
                String jMessage = JsonConvert.SerializeObject(iMessage);

                List<roomUsers> roomUsers = (from xChat in db.Chatters
                                             from xChatRoom in db.ChatRooms
                                             where xChatRoom.RoomID == RoomID
                                             && xChatRoom.UserID == xChat.ID
                                             select new roomUsers
                                             {
                                                 UserID = xChat.ID,
                                                 DeviceID = xChat.DeviceID,
                                                 xMessage = jMessage,
                                                 UserName = xChat.NickName
                                             }).ToList();

                string regIDs = "[";
                int i = 0;
                string[] regids = new string[roomUsers.Count];
                foreach (roomUsers ru in roomUsers)
                {
                    regids[i++] = ru.DeviceID;
                    regIDs += "\"";
                    regIDs += ru.DeviceID;
                    regIDs += "\",";
                }
                regIDs = regIDs.Substring(0, regIDs.Length - 1) + "]";
                string response = SendMessage(regids, jMessage, "", "C");
                return response;

            }
            else
                return "404-Invalid Login";
        }
        private  bool IsUsername(string username)
        {
      string pattern;
      // start with a letter, allow letter or number, length between 6 to 12.
      pattern = "^[a-zA-Z][a-zA-Z0-9]{5,11}$";
 
      Regex regex = new Regex(pattern);
      return regex.IsMatch(username);
        }
        public void SendMessage(String sCommand,String DeviceID)
        {
            WebRequest tRequest;
            tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = "application/x-www-form-urlencoded";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", "AIzaSyB5OwjkHSF0b3inUete_QC22VqGiGeL5Fs"));
            String collaspeKey = Guid.NewGuid().ToString("n");
            String postData = string.Format("registration_id={0}&data.payload={1}&collapse_key={2}", DeviceID, sCommand, collaspeKey);
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            tRequest.ContentLength = byteArray.Length;
            Stream dataStream = tRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse tResponse = tRequest.GetResponse();
            dataStream = tResponse.GetResponseStream();
            StreamReader tReader = new StreamReader(dataStream);
            String sResponseFromServer = tReader.ReadToEnd();
            tReader.Close();
            dataStream.Close();
            tResponse.Close();
        }
    }
}
