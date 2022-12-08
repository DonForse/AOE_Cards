using System;

namespace Features.Infrastructure.DTOs
{
    [Serializable]
    public class BoardDto
    {
        public RoundDto[] rounds;
    }
}