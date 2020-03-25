using System;
using UnityEngine;

namespace Infrastructure.Services
{
    public class UserResponseDto
    {
        public string username;
        public string id;
        public string accessToken;

        public static UserResponseDto Parse(string responseString)
        {
            return JsonUtility.FromJson<UserResponseDto>(responseString);
        }
    }
}