using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using ServerLogic.Matches.Infrastructure.DTO;
using ServerLogic.Users.Actions;
using ServerLogic.Users.Infrastructure;
using ServerLogic.Users.Service;

namespace AoeCards.Controllers
{
    
    public class UserController
    {
        private readonly IUsersRepository _usersRepository;

        public UserController(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        // GET api/play/matchid(guid-guid-guid-guid)
        /// <returns> no match available</returns>
        /// <returns>matchId</returns>
        public ResponseDto Post(JObject json)//encoded
        {
            var userInfo = JsonConvert.DeserializeObject<UserRequestDto>(json.ToString());
            var getUser = new GetUser(_usersRepository);

            var user = getUser.Execute(userInfo.username, userInfo.password, DateTime.ParseExact(userInfo.date, "dd-MM-yyyy hhmmss", System.Globalization.CultureInfo.InvariantCulture));
            if (user == null)
                throw new Exception();
            string accessToken = "";
            string refreshToken = "";

            var responseDto = new ResponseDto
            {
                response = JsonConvert.SerializeObject(new ResponseUserDto(user, accessToken, refreshToken)),
                error = string.Empty
            };
            return responseDto;
        }

        public ResponseDto Put(Object json)
        {
            var userInfo = JsonConvert.DeserializeObject<UserRequestDto>(json.ToString());
            var createUser = new CreateUser(_usersRepository);

            var user = createUser.Execute(userInfo.username, userInfo.password, DateTime.ParseExact(userInfo.date, "dd-MM-yyyy hhmmss", System.Globalization.CultureInfo.InvariantCulture));

            var responseDto = new ResponseDto
            {
                response = JsonConvert.SerializeObject(new ResponseUserDto(user, "", "")),
                error = string.Empty
            };
            return responseDto;
        }
    }
}
