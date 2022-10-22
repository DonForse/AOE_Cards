﻿using System;
using System.Collections.Generic;
using ServerLogic.Users.Domain;

namespace ServerLogic.Users.Actions
{
    internal class CreateBotUser
    {
        private Random random;
        public CreateBotUser()
        {
            random = new Random();
        }

        internal User Execute()
        {
           return new User
            {
                Id = "BOT",
                Password = "BOT",
                UserName = GetRandomBotName()
            };
        }

        private string GetRandomBotName()
        {
            return botNames[random.Next(0, botNames.Count)];
        }

        private IList<string> botNames = new List<string>(){
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