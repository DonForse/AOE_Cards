using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Service
{
    public class ResponseUserDto{
        public string guid;
        public string username;
        public string friendCode;
        public string accessToken;
        public string refreshToken;

        public ResponseUserDto(User user, string shortTermToken, string longTermToken)
        {
            guid = user.Id;
            username = user.UserName;
            friendCode = user.FriendCode;
            accessToken = shortTermToken;
            refreshToken = longTermToken;
        }
    }
}
