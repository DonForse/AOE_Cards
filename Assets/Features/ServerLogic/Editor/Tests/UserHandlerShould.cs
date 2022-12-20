using System;
using System.Globalization;
using Features.ServerLogic.Editor.Tests.Mothers;
using Features.ServerLogic.Handlers;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Domain;
using Features.ServerLogic.Users.Service;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using UnityEngine;

namespace Features.ServerLogic.Editor.Tests
{
    public class UserHandlerShould
    {
        private const string UserName = "UserName";
        private const string Password = "Password";
        
        private UserHandler _userHandler;
        private IGetUser _getUser;
        private ICreateUser _createUser;

        [SetUp]
        public void Setup()
        {
            _getUser = Substitute.For<IGetUser>();
            _createUser = Substitute.For<ICreateUser>();
            _userHandler = new UserHandler(_getUser, _createUser);
        }

        [Test]
        public void RespondsErrorWhenNoUser()
        {
            GivenNoUser();
            var userRequestDto = AUserRequestDto();
            var response = WhenPost(userRequestDto);
            ThenResponseIsExpectedError();
            
            void GivenNoUser() => _getUser.Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>()).Returns((User) null);
            void ThenResponseIsExpectedError()
            {
                Assert.AreEqual("{\"guid\":null,\"username\":null,\"friendCode\":null,\"accessToken\":null,\"refreshToken\":null}",
                    response.response);
                Assert.AreEqual("Username or password is wrong.", response.error);
            }
        }
        
        [Test]
        public void RespondsUserData()
        {
            GivenUser();
            var userRequestDto = AUserRequestDto();
            var response = WhenPost(userRequestDto);
            ThenRespondsUserInfo();

            void ThenRespondsUserInfo()
            {
                Assert.AreEqual("", response.error);
                Assert.AreEqual("{\"guid\":\"UserId\",\"username\":\"UserName\",\"friendCode\":" +
                                "\"FriendCode\",\"accessToken\":\"\",\"refreshToken\":\"\"}", response.response);
            }
        }

        [Test]
        public void CallCreateUserWhenPut()
        {
            var userRequestDto = AUserRequestDto(UserName, Password);
            WhenPut(userRequestDto);
            ThenCreateUser();

            void ThenCreateUser() => _createUser.Received(1).Execute(UserName, Password);
        }
        
        [Test]
        public void RespondsWhenPut()
        {
            var userRequestDto = AUserRequestDto(UserName, Password);
            GivenCreateUserReturns();
            var response = WhenPut(userRequestDto);
            Debug.Log(response.response);
            Assert.AreEqual("",response.error);
            Assert.AreEqual("{\"guid\":\"UserId\",\"username\":\"UserName\",\"friendCode\":\"FriendCode\",\"accessToken\":\"\",\"refreshToken\":\"\"}",response.response);

            void GivenCreateUserReturns() =>
                _createUser.Execute(Arg.Any<string>(), Arg.Any<string>()).Returns(
                    UserMother.Create("UserId", "FriendCode", Password, UserName));
        }
        
        [Test]
        public void RespondsErrorWhenPutFails()
        {
            GivenCreateUserFails();
            var userRequestDto = AUserRequestDto(UserName, Password);
            var response = WhenPut(userRequestDto);
            
            Assert.AreEqual("test-error",response.error);
            Assert.AreEqual("{\"guid\":null,\"username\":null,\"friendCode\":null,\"accessToken\":null,\"refreshToken\":null}",response.response);
            
            void GivenCreateUserFails() =>
                _createUser.Execute(UserName, Password).Throws(new ApplicationException("test-error"));
        }

        private static UserRequestDto AUserRequestDto(string withUserName = null, string withPassword= null) =>
            new UserRequestDto() {date = "19-12-2022 000000",password = withPassword, username = withUserName};
        private void GivenUser() => _getUser.Execute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<DateTime>())
            .Returns(UserMother.Create("UserId", "FriendCode", Password, UserName));
        private ResponseDto WhenPost(UserRequestDto userRequestDto) => _userHandler.Post(userRequestDto);
        private ResponseDto WhenPut(UserRequestDto userRequestDto) => _userHandler.Put(userRequestDto);
    }
}