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
                        userName = reader.GetString(reader.GetOrdinal("userName")),
                        main = reader.GetString(reader.GetOrdinal("main")),
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
                "insert  into Mymessage (userName, main, replyID, heart, dislike, initDate) values (@userName, @main, 0, 0, 0, GETDATE())",
                conn);
            cmd.Parameters.Add(new SqlParameter("userName", message.userName));
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
                "insert  into Mymessage (userName, main, replyID, heart, dislike, initDate) values (@userName, @main, @replyID, 0, 0, GETDATE())",
                conn);
            cmd.Parameters.Add(new SqlParameter("userName", message.userName));
            cmd.Parameters.Add(new SqlParameter("main", message.main));
            cmd.Parameters.Add(new SqlParameter("replyID", message.replyID));
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // 案讚
        public void Like(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmd = new SqlCommand(
                "update Mymessage set heart = @heart where messageID = @messageID",
                conn);
            cmd.Parameters.Add(new SqlParameter("heart", message.heart +1));
            cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        // 案讚 ajax版
        public int LikeAjax(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmd = new SqlCommand(
                "update Mymessage set heart = @heart where messageID = @messageID",
                conn);
            cmd.Parameters.Add(new SqlParameter("heart", message.heart + 1));
            cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            return message.heart + 1;
        }

        // 案倒讚
        public void Dislike(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmd = new SqlCommand(
                "update Mymessage set dislike = @dislike where messageID = @messageID",
                conn);
            cmd.Parameters.Add(new SqlParameter("dislike", message.dislike + 1));
            cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
        // 案倒讚 Ajax版
        public int DislikeAjax(message message)
        {
            SqlConnection conn = new SqlConnection(ConnStr);
            SqlCommand cmd = new SqlCommand(
                "update Mymessage set dislike = @dislike where messageID = @messageID",
                conn);
            cmd.Parameters.Add(new SqlParameter("dislike", message.dislike + 1));
            cmd.Parameters.Add(new SqlParameter("messageID", message.messageID));
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
            return message.dislike + 1;
        }


    }
}