using System;
using UnityEngine;

namespace Infrastructure.Services
{
    public class UserResponseDto
    {
        public string username;
        public string guid;
        public string accessToken;

        public static UserResponseDto Parse(string responseString)
        {
            return JsonUtility.FromJson<UserResponseDto>(responseString);
        }
    }
}