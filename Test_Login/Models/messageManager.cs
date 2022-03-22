using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Test_Login.Models
{
    public class messageManager
    {
        private readonly string ConnStr = "Data Source=.;Initial Catalog=Lab;Integrated Security=True";

        // 取得資料
        public List<message> GetMessages()
        {
            List<message> messageList = new List<message>();
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmd = new SqlCommand("select * from Mymessage", conn);
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    message message = new message()
                    {
                        messageID = reader.GetInt32(reader.GetOrdinal("messageID")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        UserId = reader.GetString(reader.GetOrdinal("UserId")),
                        main = reader.GetString(reader.GetOrdinal("main")),
                        wholike = reader.GetString(reader.GetOrdinal("wholike")),
                        whohate = reader.GetString(reader.GetOrdinal("whohate")),
                        replyID = reader.GetInt32(reader.GetOrdinal("replyID")),
                        heart = reader.GetInt32(reader.GetOrdinal("heart")),
                        dislike = reader.GetInt32(reader.GetOrdinal("dislike")),
                        initDate = reader.GetDateTime(reader.GetOrdinal("initDate"))
                    };
                    // 每讀到一行 將資料加到 messageList 內
                    messageList.Add(message);
                }
            }
            else // 若沒有資料
            {
                Console.WriteLine("資料庫空的");
            }
            conn.Close();
            return messageList;
        }

        // 新增資料
        public void CreateMessage(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmd = new SqlCommand(
                //insert  into Mymessage (UserName, UserId, main, replyID, heart, dislike, wholike, whohate,  initDate) values ('小名', 'xiaomin', '早安', 0, 0, 0, '', '' ,  GETDATE())
                "insert  into Mymessage (UserName, UserId, main, replyID, heart, dislike, wholike, whohate, initDate) values (@UserName, @UserId, @main, 0, 0, 0, '' ,'', GETDATE())",
                conn);
            cmd.Parameters.Add(new SqlParameter("UserName", message.UserName));
            cmd.Parameters.Add(new SqlParameter("UserId", message.UserId));
            cmd.Parameters.Add(new SqlParameter("main", message.main));
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // 回覆資料
        public void ReplyMessage(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmd = new SqlCommand(
                //insert  into Mymessage (UserName, UserId, main, replyID, heart, dislike, wholike, whohate,  initDate) values ('小名', 'xiaomin', '早安', 0, 0, 0, '', '' ,  GETDATE())
                "insert  into Mymessage (UserName, UserId, main, replyID, heart, dislike, wholike, whohate, initDate) values (@UserName, @UserId, @main, @replyID, 0, 0, '', '' ,GETDATE())",
                conn);
            cmd.Parameters.Add(new SqlParameter("UserName", message.UserName));
            cmd.Parameters.Add(new SqlParameter("UserId", message.UserId));
            cmd.Parameters.Add(new SqlParameter("main", message.main));
            cmd.Parameters.Add(new SqlParameter("replyID", message.replyID));
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //// 案讚
        //public void Like(message message)
        //{
        //    SqlConnection conn = new SqlConnection(ConnStr);
        //    SqlCommand cmd = new SqlCommand(
        //        "update Mymessage set heart = @heart where messageID = @messageID",
        //        conn);
        //    cmd.Parameters.Add(new SqlParameter("heart", message.heart +1));
        //    cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));
        //    conn.Open();
        //    cmd.ExecuteNonQuery();
        //    conn.Close();
        //}

        // 案讚 ajax版
        public int LikeAjax(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmdselect = new SqlCommand(
                $"select wholike from Mymessage where messageID = {message.messageID}",
                conn);
            conn.Open(); // (string) cmdselect.ExecuteScalar()+',' + message.UserName
            string strWholike = (string)cmdselect.ExecuteScalar() == string.Empty ? message.UserName : (string)cmdselect.ExecuteScalar() + ',' + message.UserName;
            SqlCommand cmd = new SqlCommand(
                "update Mymessage set heart = @heart, wholike=@UserName where messageID = @messageID",
                conn);
            cmd.Parameters.Add(new SqlParameter("heart", message.heart + 1));
            cmd.Parameters.Add(new SqlParameter("UserName", strWholike));
            cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));

            cmd.ExecuteNonQuery();
            conn.Close();
            return message.heart + 1;
        }

        // 案讚 ajax版 因為 應該
        public int LikeCancelAjax(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmdselect = new SqlCommand(
                $"select wholike from Mymessage where messageID = {message.messageID}",
                conn);
            conn.Open();
            string strWholike = ""; //((string)cmdselect.ExecuteScalar()).Replace($"{message.UserName}", "");

            if (((string)cmdselect.ExecuteScalar()).Contains($",{message.UserName}"))
            {
                strWholike = ((string)cmdselect.ExecuteScalar()).Replace($",{message.UserName}", "");
            }
            else
            {
                strWholike = ((string)cmdselect.ExecuteScalar()).Replace($"{message.UserName}", "");
            }
            SqlCommand cmd = new SqlCommand();
            if (strWholike == string.Empty)
            {
                cmd.CommandText = $"update Mymessage set heart = {message.heart - 1}, wholike= ' ' where messageID = {message.messageID}";
                cmd.Connection = conn;
            }
            else
            {
                cmd.CommandText = $"update Mymessage set heart = {message.heart - 1}, wholike=@UserName where messageID = {message.messageID}";
                cmd.Parameters.Add(new SqlParameter("UserName", strWholike));
                cmd.Connection = conn;
            }
            cmd.ExecuteNonQuery();
            conn.Close();
            return message.heart - 1;
        }

        //// 案倒讚
        //public void Dislike(message message)
        //{
        //    SqlConnection conn = new SqlConnection(ConnStr);
        //    SqlCommand cmd = new SqlCommand(
        //        "update Mymessage set dislike = @dislike where messageID = @messageID",
        //        conn);
        //    cmd.Parameters.Add(new SqlParameter("dislike", message.dislike + 1));
        //    cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));
        //    conn.Open();
        //    cmd.ExecuteNonQuery();
        //    conn.Close();
        //}
        // 案倒讚 Ajax版
        public int DislikeAjax(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmdselect = new SqlCommand(
                $"select whohate from Mymessage where messageID = {message.messageID}",
                conn);
            conn.Open();
            string strWhohate = (string)cmdselect.ExecuteScalar() == string.Empty ? message.UserName : (string)cmdselect.ExecuteScalar() + ',' + message.UserName;
            SqlCommand cmd = new SqlCommand(
                "update Mymessage set dislike = @dislike, whohate=@UserName where messageID = @messageID",
                conn);
            cmd.Parameters.Add(new SqlParameter("dislike", message.dislike + 1));
            cmd.Parameters.Add(new SqlParameter("UserName", strWhohate));
            cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));
            cmd.ExecuteNonQuery();
            conn.Close();
            return message.dislike + 1;

        }
        //DislikeCancelAjax
        public int DislikeCancelAjax(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmdselect = new SqlCommand(
                $"select whohate from Mymessage where messageID = {message.messageID}",
                conn);
            conn.Open();
            string strWhohate = "";
            if (((string)cmdselect.ExecuteScalar()).Contains($",{message.UserName}"))
            {
                strWhohate = ((string)cmdselect.ExecuteScalar()).Replace($",{message.UserName}", "");
            }
            else
            {
                strWhohate = ((string)cmdselect.ExecuteScalar()).Replace($"{message.UserName}", "");
            }
            SqlCommand cmd = new SqlCommand();
            if (strWhohate == string.Empty)
            {
                cmd.CommandText = $"update Mymessage set dislike = {message.dislike - 1}, whohate= ' ' where messageID = {message.messageID}";
                cmd.Connection = conn;
            }
            else
            {
                cmd.CommandText = $"update Mymessage set dislike = {message.dislike - 1}, whohate=@UserName where messageID = {message.messageID}";
                cmd.Parameters.Add(new SqlParameter("UserName", strWhohate));
                cmd.Connection = conn;
            }
            cmd.ExecuteNonQuery();
            conn.Close();
            return message.dislike - 1;

        }
    }
}