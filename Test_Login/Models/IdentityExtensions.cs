using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Test_Login.Models.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetNickName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("NickName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetStockBag(this IIdentity identity)
        {
            var claim2 = ((ClaimsIdentity)identity).FindFirst("StockBag");
            // Test for null to avoid issues during local testing
            return (claim2 != null) ? claim2.Value : string.Empty;
        }

        public static string GetUserTier(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserTier");
            // 測試是否為null
            return (claim != null) ? claim.Value : "404 userTier";
        }
    }
}