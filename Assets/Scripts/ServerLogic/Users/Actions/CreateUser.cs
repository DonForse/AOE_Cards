using System;
using System.Text;
using ServerLogic.Users.Domain;
using ServerLogic.Users.Infrastructure;

namespace ServerLogic.Users.Actions
{
    internal class CreateUser
    {
        private readonly IUsersRepository _usersRepository;
        private readonly CreateGuestUser _createGuest;
        private Random random;
        public CreateUser(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
            _createGuest = new CreateGuestUser();
            random = new Random();
        }

        internal User Execute(string username, string password, DateTime dt)
        {
            if (username == "GUEST")
            {
                var guest = _createGuest.Execute();
                _usersRepository.Add(guest);
                return guest;
            }

            if (_usersRepository.Get(username) != null)
                throw new ApplicationException("user already exists");
            password = DecodePassword(password, dt);
            string savedPasswordHash = User.EncryptPassword(password);
            var user = new User()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = username,
                Password = savedPasswordHash,
                FriendCode = string.Format("{0}#{1}", username.Substring(0, Math.Min(5, username.Length)).Trim(), random.Next(0,100000))
            };

            _usersRepository.Add(user);
            return user;
        }

        private string DecodePassword(string encodedPassword, DateTime dt)
        {
            var encoding = Encoding.GetEncoding("ISO-8859-1");
            encodedPassword = encoding.GetString(Convert.FromBase64String(encodedPassword));

            var phrase = "!AoE.MAG1#C-4nt11C4it##";
            byte[] bytesPhrase = encoding.GetBytes(phrase);

            var date = encoding.GetBytes(dt.ToString("dd-MM-yyyy hhmmss"));

            var bytePassword = encoding.GetBytes(encodedPassword);

            var newPassword = new byte[bytePassword.Length];
            for (int i = 0; i < bytePassword.Length; i++)
            {
                var dateIndex = i;
                while (date.Length - dateIndex <= 0)
                {
                    dateIndex = dateIndex - date.Length;
                }
                var phraseIndex = i;
                while (bytesPhrase.Length - phraseIndex <= 0)
                {
                    phraseIndex = phraseIndex - bytesPhrase.Length;
                }

                newPassword[i] = (byte)((int)bytePassword[i] - (int)date[dateIndex] - (int)bytesPhrase[i]);
            }
            return encoding.GetString(newPassword);
        }
    }
}