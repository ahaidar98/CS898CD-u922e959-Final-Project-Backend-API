using System;
using System.Collections.Generic;

namespace BabAl_SalamWebAPI.Models
{
    public class UserInformation
    {
        public UserDTO User { set; get; }
    }

    public class UserDataInformation
    {
        public UserInformation UserInformation { set; get; }
        public IEnumerable<UserDTO> Users { set; get; }
    }
}
