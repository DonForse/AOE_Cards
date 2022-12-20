using System;
using Features.ServerLogic.Matches.Infrastructure.DTO;
using Features.ServerLogic.Users.Actions;
using Features.ServerLogic.Users.Service;
using Newtonsoft.Json;

namespace Features.ServerLogic.Handlers
{
    public class UserHandler
    {
        private readonly IGetUser _getUser;
        private readonly ICreateUser _createUser;

        public UserHandler(IGetUser getUser, ICreateUser createUser)
        {
            _getUser = getUser;
            _createUser = createUser;
        }

        // GET api/play/matchid(guid-guid-guid-guid)
        /// <returns> no match available</returns>
        /// <returns>matchId</returns>
        public ResponseDto Post(UserRequestDto json) //encoded
        {
            var userInfo = json;
            try
            {
                var user = _getUser.Execute(userInfo.username, userInfo.password,
                    DateTime.ParseExact(userInfo.date, "dd-MM-yyyy hhmmss",
                        System.Globalization.CultureInfo.InvariantCulture));
                if (user == null)
                    throw new Exception("Username or password is wrong.");
                string accessToken = "";
                string refreshToken = "";

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new ResponseUserDto(user, accessToken, refreshToken)),
                    error = string.Empty
                };
                return responseDto;
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new ResponseUserDto(null, null, null)),
                    error = ex.Message
                };
            }
        }

        public ResponseDto Put(UserRequestDto json)
        {
            try
            {
                var userInfo = json;
                var user = _createUser.Execute(userInfo.username, userInfo.password);

                var responseDto = new ResponseDto
                {
                    response = JsonConvert.SerializeObject(new ResponseUserDto(user, "", "")),
                    error = string.Empty
                };
                return responseDto;
            }

            catch (Exception ex)
            {
                return new ResponseDto()
                {
                    response = JsonConvert.SerializeObject(new ResponseUserDto(null, null ,null)),
                    error = ex.Message
                };
            }
        }
    }
}