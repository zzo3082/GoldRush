using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Test_Login.Models;

namespace Test_Login.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            int tt = 2131414;
            long tt2 = 213134235414;
            long tt3 = 213141345324244;
            long tt4 = 213145345234534514;
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        #region 討論區
        // Message 取得資料庫資料
        public ActionResult Message()
        {
            messageManager messageManager = new messageManager();
            List<message> messageList = messageManager.GetMessages();
            ViewBag.messageList = messageList;
            return View();
        }

        [HttpPost]
        public ActionResult Message(string UserNameBox, string mainBox, string UserIdbox, string hashtagBox, HttpPostedFileBase imageBox, int heart = 0, int messageID = 0)
        {
            messageManager messageManager = new messageManager();
            if (UserNameBox != null)
            {
                message message = new message();
                message.UserId = UserIdbox;
                message.UserName = UserNameBox;
                message.main = mainBox;
                byte[] photoBytes;
                if (imageBox != null && imageBox.ContentLength > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        imageBox.InputStream.CopyTo(ms);
                        photoBytes = ms.GetBuffer();
                        message.photo = photoBytes;
                    }
                }
                else
                {
                    byte[] bytes = { 0 };
                    message.photo = bytes;
                }
                if (hashtagBox != null)
                {
                    message.hashtagID = hashtagBox;
                }
                else
                {
                    message.hashtagID = "0";
                }

                messageManager.CreateMessage(message);
                List<message> messageList = messageManager.GetMessages();
                ViewBag.messageList = messageList;
                return Redirect("Message");
            }
            else
            {
                message message = new message()
                {
                    messageID = (int)messageID,
                    heart = (int)heart
                };
                //messageManager.Like(message);
                List<message> messageList = messageManager.GetMessages();
                ViewBag.messageList = messageList;
                return View();
            }

        }
        [HttpPost]
        public ActionResult Reply(string UserNameBox, string UserIdbox, string mainBox, int messageID, int hashtagBox = 0)
        {
            messageManager messageManager = new messageManager();
            message message = new message()
            {
                UserId = UserIdbox,
                replyID = messageID,
                UserName = UserNameBox,
                main = mainBox
            };
            messageManager.ReplyMessage(message);
            List<message> messageList = messageManager.GetMessages();
            ViewBag.messageList = messageList;
            return Redirect("Message");
        }

        [HttpPost]
        public JsonResult Delete(message message)
        {
            messageManager messageManager = new messageManager();
            int result = messageManager.DeleteMessage(message);
            return Json(result);
        }

        [HttpPost]
        public ActionResult Edit(string UserNameBox, string UserIdbox, string mainBox, int messageID)
        {
            messageManager messageManager = new messageManager();
            message message = new message()
            {
                messageID = messageID,
                main = mainBox
            };
            messageManager.EditMessage(message);
            List<message> messageList = messageManager.GetMessages();
            ViewBag.messageList = messageList;
            return Redirect("Message");
        }


        // 當按讚post到這裡 從ajax收到一個message
        [HttpPost]
        public JsonResult APILike(message message)
        {
            messageManager messageManager = new messageManager();
            int heartReturn = messageManager.LikeAjax(message);
            return Json(new { heart = heartReturn, messageID = message.messageID }, JsonRequestBehavior.AllowGet);
        }
        // 當取消讚post到這裡 從ajax收到一個message
        [HttpPost]
        public JsonResult APILikeCancel(message message)
        {
            messageManager messageManager = new messageManager();
            int heartReturn = messageManager.LikeCancelAjax(message);
            return Json(new { heart = heartReturn, messageID = message.messageID }, JsonRequestBehavior.AllowGet);
        }
        // 當按讚post到這裡 從ajax收到一個message
        [HttpPost]
        public JsonResult APIDislike(message message)
        {
            messageManager messageManager = new messageManager();
            int dislikeReturn = messageManager.DislikeAjax(message);
            return Json(new { dislike = dislikeReturn, messageID = message.messageID }, JsonRequestBehavior.AllowGet);
        }
        // 當按讚post到這裡 從ajax收到一個message
        [HttpPost]
        public JsonResult APIDislikeCancel(message message)
        {
            messageManager messageManager = new messageManager();
            int dislikeReturn = messageManager.DislikeCancelAjax(message);
            return Json(new { dislike = dislikeReturn, messageID = message.messageID }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}