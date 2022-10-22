using System;

namespace Infrastructure.DTOs
{
    [Serializable]
    public class BoardDto
    {
        public RoundDto[] rounds;
    }
}