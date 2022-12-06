using System;
using System.Text;
using Features.ServerLogic.Users.Domain;
using Features.ServerLogic.Users.Infrastructure;

namespace Features.ServerLogic.Users.Actions
{
    internal class GetUser
    {
        private IUsersRepository _usersRepository;

        public GetUser(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        internal User Execute(string username, string encodedPassword, DateTime dt)
        {
            //hashthingy
            var password = DecodePassword(encodedPassword, dt);
            var user = _usersRepository.Get(username);
            if (!User.ComparePassword(password, user.Password))
                return null;
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
                while (date.Length - dateIndex <= 0) {
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

        internal User Execute(string userId)
        {
            //hashthingy
            var user = _usersRepository.GetById(userId);
            return user;
        }
    }
}