using System;
using Features.Infrastructure.DTOs;
using Features.Login.Scripts.Domain;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Users.Service;
using UniRx;
using UnityEngine;

namespace Features.Login.Scripts.Infrastructure
{
    public class OfflineLoginService : ILoginService
    {
        private readonly UserHandler _userHandler;

        public OfflineLoginService(UserHandler userHandler )
        {
            _userHandler = userHandler;
        }

        public IObservable<UserResponseDto> Register(string playerName, string password)
        {
            var response = _userHandler.Put(new UserRequestDto() {password = password, username = playerName});
            var dto = JsonUtility.FromJson<UserResponseDto>(response.response);
            return Observable.Return(dto);
        }

        public IObservable<UserResponseDto> Login(string playerName, string password)
        {
            var response = _userHandler.Post(new UserRequestDto() {password = password, username = playerName});
            var dto = JsonUtility.FromJson<UserResponseDto>(response.response);
            return Observable.Return(dto);
        }
    }
}