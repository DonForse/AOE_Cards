using System.Collections.Generic;

namespace Infrastructure.Services
{
    public partial class PlayService
    {
        public class RerollInfoDto
        {
            public IList<string> upgradeCards;
            public IList<string> unitCards;
        }
    }
}