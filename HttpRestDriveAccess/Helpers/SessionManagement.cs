using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HttpRestDriveAccess.Helpers
{
    public static class SessionManagement
    {
        public static void SetSession(this ISession session, string key, string value)
        {
            session.SetString(key, value);
        }

        public static string GetSession(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? string.Empty : value;
        }
    }
}
