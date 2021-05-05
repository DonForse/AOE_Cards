using UnityEngine;

namespace Infrastructure.Services
{
    public class UserResponseDto
    {
        public string username;
        public string guid;
        public string friendCode;
        public string accessToken;
        public string refreshToken;

        public static UserResponseDto Parse(string responseString)
        {
            return JsonUtility.FromJson<UserResponseDto>(responseString);
        }
    }
}