namespace Features.Game.Scripts.Domain
{
    public interface IMatchStateRepository
    {
        GameState Get();
        void Set(GameState gameState);
    }
}