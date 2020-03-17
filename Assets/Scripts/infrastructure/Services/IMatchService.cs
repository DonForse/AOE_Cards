using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public interface IMatchService
    {
        Task<MatchStatus> StartMatch();
        void PlayEventCard(string cardName);
        void PlayUnitCard(string cardName);
    }
}