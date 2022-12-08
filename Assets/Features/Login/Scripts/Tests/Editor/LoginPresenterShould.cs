using System;
using Features.Infrastructure.DTOs;
using Features.Login.Scripts.Domain;
using Features.Login.Scripts.Presentation;
using NSubstitute;
using NUnit.Framework;
using UniRx;

namespace Features.Login.Scripts.Tests.Editor
{
    public class LoginPresenterShould
    {
        private LoginPresenter _loginPresenter;
        private ILoginService _loginService;
        private ILoginView _loginView;
        private ISubject<(string username, string password)> _loginButtonPressedSubject;
        private ISubject<Unit> _guestLoginButtonPressedSubject;
        private IPlayerPrefs _playerPrefs;
        private ISubject<(string username, string password)> _registerButtonPressedSubject;

        [SetUp]
        public void Setup()
        {
            _loginService = Substitute.For<ILoginService>();
            _loginView = Substitute.For<ILoginView>();
            _loginButtonPressedSubject = new Subject<(string username, string password)>();
            _loginView.OnLoginButtonPressed().Returns(_loginButtonPressedSubject);

            _guestLoginButtonPressedSubject = new Subject<Unit>();
            _loginView.OnGuestButtonPressed().Returns(_guestLoginButtonPressedSubject);

            _registerButtonPressedSubject = new Subject<(string username, string password)>();
            _loginView.OnRegisterButtonPressed().Returns(_registerButtonPressedSubject);

            _playerPrefs = Substitute.For<IPlayerPrefs>();
            _loginPresenter = new LoginPresenter(_loginView, _loginService, _playerPrefs);
        }

        [Test]
        public void ShowErrorWhenCallLoginWithEmptyUsername()
        {
            var expectedUsername = "";
            var expectedPassword = "password";
            GivenPresenterIsInitialized();
            WhenLoginButtonIsPressed(expectedUsername, expectedPassword);
            ThenWarningIsShown("Username cannot be empty");
        }

        [Test]
        public void ShowErrorWhenCallLoginWithEmptyPassword()
        {
            var expectedUsername = "username";
            var expectedPassword = "";
            GivenPresenterIsInitialized();
            WhenLoginButtonIsPressed(expectedUsername, expectedPassword);
            ThenWarningIsShown("Password cannot be empty");
            ThenDidNotCallLoginServiceLogin();
        }

        [Test]
        public void ShowErrorWhenCallRegisterWithEmptyUsername()
        {
            var expectedUsername = "";
            var expectedPassword = "password";
            GivenPresenterIsInitialized();
            WhenRegisterButtonIsPressed(expectedUsername, expectedPassword);
            ThenWarningIsShown("Username cannot be empty");
            ThenDidNotCallLoginServiceRegister();
        }

        [Test]
        public void ShowErrorWhenCallRegisterWithEmptyPassword()
        {
            var expectedUsername = "username";
            var expectedPassword = "";
            GivenPresenterIsInitialized();
            WhenRegisterButtonIsPressed(expectedUsername, expectedPassword);
            ThenWarningIsShown("Password cannot be empty");
            ThenDidNotCallLoginServiceRegister();
        }

        [Test]
        public void CallLoginServiceWhenLogin()
        {
            var expectedUsername = "username";
            var expectedPassword = "password";
            var expectedResponse = new UserResponseDto()
            {
                username = "user", guid = Guid.NewGuid().ToString(), accessToken = "token",
                friendCode = "code", refreshToken = "rToken"
            };
            GivenLoginServiceReturns(expectedResponse);
            GivenPresenterIsInitialized();
            WhenLoginButtonIsPressed(expectedUsername, expectedPassword);
            Received.InOrder(() =>
            {
                ThenDisableButtons();
                ThenReceivedLoginServiceLogin(expectedUsername, expectedPassword);
                ThenSavedUserInfoInPlayerPrefs(expectedResponse);
                ThenNavigateToHome();
                ThenEnableButtons();
            });
        }

        [Test]
        public void CallRegisterWhenGuestLogin()
        {
            var expectedResponse = new UserResponseDto()
            {
                username = "user", guid = Guid.NewGuid().ToString(), accessToken = "token",
                friendCode = "code", refreshToken = "rToken"
            };
            GivenLoginServiceReturns(expectedResponse);
            GivenPresenterIsInitialized();
            WhenGuestLoginButtonIsPressed();
            Received.InOrder(() =>
            {
                ThenDisableButtons();
                ThenReceivedLoginServiceRegister("GUEST", string.Empty);
                ThenSavedUserInfoInPlayerPrefs(expectedResponse);
                ThenNavigateToHome();
                ThenEnableButtons();
            });
        }

        [Test]
        public void CallRegisterWhenRegisterIsPressed()
        {
            var expectedUsername = "username";
            var expectedPassword = "password";
            var expectedResponse = new UserResponseDto()
            {
                username = "user", guid = Guid.NewGuid().ToString(), accessToken = "token",
                friendCode = "code", refreshToken = "rToken"
            };
            GivenLoginServiceReturns(expectedResponse);
            GivenPresenterIsInitialized();
            WhenRegisterButtonIsPressed(expectedUsername, expectedPassword);
            Received.InOrder(() =>
            {
                ThenDisableButtons();
                ThenReceivedLoginServiceRegister(expectedUsername, expectedPassword);
                ThenSavedUserInfoInPlayerPrefs(expectedResponse);
                ThenNavigateToHome();
                ThenEnableButtons();
            });
        }

        [Test]
        public void ShowErrorMessageWhenServiceLoginFails()
        {
            var expectedMessage = "error";
            GivenPresenterIsInitialized();
            var subject = GivenLoginFails();
            WhenLoginButtonIsPressed("any", "any");
            WhenFails();
            Received.InOrder(() =>
            {
                ThenDisableButtons();
                ThenErrorIsShown(expectedMessage);
                ThenEnableButtons();
            });
            void WhenFails() => subject.OnError(new Exception(expectedMessage));

        }

        [Test]
        public void ShowErrorMessageWhenServiceRegisterFailsOnGuest()
        {
            var expectedMessage = "error";
            GivenPresenterIsInitialized();
            var subject = GivenRegisterFails();
            WhenGuestLoginButtonIsPressed();
            WhenFails();
            Received.InOrder(() =>
            {
                ThenDisableButtons();
                ThenErrorIsShown(expectedMessage);
                ThenEnableButtons();
            });
            void WhenFails() => subject.OnError(new Exception(expectedMessage));
        }


        [Test]
        public void ShowErrorMessageWhenServiceRegisterFailsOnRegistration()
        {
            var expectedMessage = "error";
            GivenPresenterIsInitialized();
            var subject = GivenRegisterFails();
            WhenRegisterButtonIsPressed("any", "any");
            WhenFails();
            Received.InOrder(() =>
            {
                ThenDisableButtons();
                ThenErrorIsShown(expectedMessage);
                ThenEnableButtons();
            });
            void WhenFails() => subject.OnError(new Exception(expectedMessage));
        }


        private void GivenPresenterIsInitialized() => _loginPresenter.Initialize();

        private void GivenLoginServiceReturns(UserResponseDto expectedResponse)
        {
            _loginService.Login(Arg.Any<string>(), Arg.Any<string>()).Returns(Observable.Return(expectedResponse));
            _loginService.Register(Arg.Any<string>(), Arg.Any<string>()).Returns(Observable.Return(expectedResponse));
        }

        private Subject<UserResponseDto> GivenLoginFails()
        {
            var errorSubject = new Subject<UserResponseDto>();
            _loginService
                .Login(Arg.Any<string>(), Arg.Any<string>())
                .Returns(errorSubject);
            return errorSubject;
        }

        private Subject<UserResponseDto> GivenRegisterFails()
        {
            var errorSubject = new Subject<UserResponseDto>();

            _loginService.Register(Arg.Any<string>(), Arg.Any<string>())
                .Returns(errorSubject);
            return errorSubject;
        }

        private void WhenLoginButtonIsPressed(string expectedUsername, string expectedPassword) =>
            _loginButtonPressedSubject.OnNext((expectedUsername, expectedPassword));

        private void WhenRegisterButtonIsPressed(string expectedUsername, string expectedPassword) =>
            _registerButtonPressedSubject.OnNext((expectedUsername, expectedPassword));

        private void WhenGuestLoginButtonIsPressed() => _guestLoginButtonPressedSubject.OnNext(Unit.Default);
        private void ThenWarningIsShown(string error) => _loginView.Received(1).ShowWarning(error);
        private void ThenErrorIsShown(string error) => _loginView.Received(1).ShowError(error);

        private void ThenDidNotCallLoginServiceLogin() =>
            _loginService.DidNotReceive().Login(Arg.Any<string>(), Arg.Any<string>());

        private void ThenDidNotCallLoginServiceRegister() =>
            _loginService.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>());

        private void ThenEnableButtons() => _loginView.Received(1).EnableButtons();
        private void ThenDisableButtons() => _loginView.Received(1).DisableButtons();

        private void ThenReceivedLoginServiceLogin(string expectedUsername, string expectedPassword) =>
            _loginService.Received(1).Login(expectedUsername, expectedPassword);

        private void ThenReceivedLoginServiceRegister(string expectedUsername, string expectedPassword) =>
            _loginService.Received(1).Register(expectedUsername, expectedPassword);

        private void ThenNavigateToHome() => _loginView.Received(1).NavigateToHomeView();


        private void ThenSavedUserInfoInPlayerPrefs(UserResponseDto response)
        {
            _playerPrefs.Received(1).SetString(PlayerPrefsHelper.UserId, response.guid);
            _playerPrefs.Received(1).SetString(PlayerPrefsHelper.UserName, response.username);
            _playerPrefs.Received(1).SetString(PlayerPrefsHelper.FriendCode, response.friendCode);
            _playerPrefs.Received(1).SetString(PlayerPrefsHelper.AccessToken, response.accessToken);
            _playerPrefs.Received(1).SetString(PlayerPrefsHelper.RefreshToken, response.refreshToken);
            _playerPrefs.Received(1).Save();
        }
    }
}