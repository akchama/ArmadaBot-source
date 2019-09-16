using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArmadaBot
{
    class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Nic { get; set; }

        public static bool IsEqual(User user1, User user2)
        {
            if (user1 == null || user2 == null)
            {
                return false;
            }

            if (user1.Username != user2.Username)
            {
                return false;
            }

            if (user1.Password != user2.Password)
            {
                return false;
            }

            return true;
        }
    }
}
