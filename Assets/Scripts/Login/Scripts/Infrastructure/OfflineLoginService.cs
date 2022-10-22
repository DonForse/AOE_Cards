using System;
using Infrastructure.DTOs;
using Infrastructure.Services;
using Login.Scripts.Domain;
using ServerLogic.Controllers;
using ServerLogic.Users.Service;
using UniRx;
using UnityEngine;

namespace Login.Scripts.Infrastructure
{
    public class OfflineLoginService : ILoginService
    {
        private readonly UserController _userController;

        public OfflineLoginService(UserController userController )
        {
            _userController = userController;
        }

        public IObservable<UserResponseDto> Register(string playerName, string password)
        {
            var response = _userController.Put(new UserRequestDto() {password = password, username = playerName});
            var dto = JsonUtility.FromJson<UserResponseDto>(response.response);
            return Observable.Return(dto);
        }

        public IObservable<UserResponseDto> Login(string playerName, string password)
        {
            var response = _userController.Post(new UserRequestDto() {password = password, username = playerName});
            var dto = JsonUtility.FromJson<UserResponseDto>(response.response);
            return Observable.Return(dto);
        }
    }
}