using System;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Infrastructure;
using Features.ServerLogic.Users.Service;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    
    public class UserHandler
    {
        private readonly IUsersRepository _usersRepository;

        public UserHandler(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
        }

        // GET api/play/matchid(guid-guid-guid-guid)
        /// <returns> no match available</returns>
        /// <returns>matchId</returns>
        public ResponseDto Post(UserRequestDto json)//encoded
        {
            var userInfo = json;
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

        public ResponseDto Put(UserRequestDto json)
        {
            var userInfo = json;
            var createUser = new CreateUser(_usersRepository);

            var user = createUser.Execute(userInfo.username, userInfo.password);

            var responseDto = new ResponseDto
            {
                response = JsonConvert.SerializeObject(new ResponseUserDto(user, "", "")),
                error = string.Empty
            };
            return responseDto;
        }
    }
}
