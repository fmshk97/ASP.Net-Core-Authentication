using Authentication.Apis.Models;
using System.Collections.Generic;

namespace Authentication.Apis
{
    public class UserRepository
    {
        public static List<AppUser> UsersDb;

        static UserRepository()
        {
            UsersDb = new List<AppUser>();
        }
    }
}
