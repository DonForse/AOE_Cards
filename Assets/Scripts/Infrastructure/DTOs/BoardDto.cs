using System;
using System.Collections.Generic;

namespace Infrastructure.Services
{
    [Serializable]
    public class BoardDto
    {
        public RoundDto[] rounds;
    }
}