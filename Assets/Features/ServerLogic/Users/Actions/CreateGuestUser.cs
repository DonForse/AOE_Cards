using System;
using System.Collections.Generic;
using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Actions
{
    internal class CreateGuestUser
    {
        private Random random;
        public CreateGuestUser()
        {
            random = new Random();
        }

        internal User Execute()
        {
            var name = GetRandomGuestName();
            var randomNumber = random.Next(0, 100000);
            return new User
            {
                Id = Guid.NewGuid().ToString(),
                Password = Guid.NewGuid().ToString(),
                UserName = string.Format("{0}-{1}", name, randomNumber),
                FriendCode = string.Format("{0}#{1}", name.Substring(0, Math.Min(5, name.Length)).Trim(), randomNumber)
            };
        }

        private string GetRandomGuestName()
        {
            return guestNames[random.Next(0, guestNames.Count)];
        }

        private IList<string> guestNames = new List<string>()
    {
        "Juana Azurduy",
        "Jose de San Martin",
        "Napoleón Bonaparte",
        "Joan D'Arc",
        "Abraham Lincoln",
        "Alexander The Great",
        "Genghis Khan",
        "Attila the Hun",
        "Julius Caesar",
        "Simon Bolivar",
        "Sun Tzu",
        "Peter The Great",
        "El Cid",
        "Charlemagne",
        "Bad Neighbour",
        "Emperor in a Barrel",
        "Saladin",
        "King Sancho",
        "Obama"
    };
    }
}