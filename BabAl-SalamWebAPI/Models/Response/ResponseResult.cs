using System;
namespace BabAl_SalamWebAPI.Models
{
    public class ResponseResult
    {
        public string ResponseMessage { set; get; }
        public string MessageStanding { set; get; }
        public UserDataInformation Data { set; get; }
    }
}
