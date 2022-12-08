using System;

namespace Features.Infrastructure.DTOs
{
    [Serializable]
    public class PlayerCardDto
    {
        public string player;
        public string upgradecard;
        public string unitcard;
        public int unitcardpower;
    }
}