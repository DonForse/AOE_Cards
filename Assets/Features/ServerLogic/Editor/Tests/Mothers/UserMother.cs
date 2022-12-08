using Features.ServerLogic.Users.Domain;

namespace Features.ServerLogic.Editor.Tests.Mothers
{
    public static class UserMother
    {
        public static User Create(string withId = null, string withFriendCode = null, string withPassword = null, string withUserName = null)
        {
            return new User
            {
                Id = withId,
                FriendCode = withFriendCode,
                Password = withPassword,
                UserName = withUserName
            };
        }
    }
}