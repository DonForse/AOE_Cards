using System;
using Features.Infrastructure.DTOs;
using Features.Login.Scripts.Domain;
using Features.ServerLogic.Controllers;
using Features.ServerLogic.Users.Service;
using UniRx;
using UnityEngine;

namespace Features.Login.Scripts.Infrastructure
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