using System;
using System.Collections.Generic;
using System.Linq;
using Features.ServerLogic.Matches.Service;
using ServerLogic.Cards.Domain.Units;
using ServerLogic.Cards.Domain.Upgrades;
using ServerLogic.Matches.Domain;
using ServerLogic.Matches.Service;
using ServerLogic.Users.Domain;

namespace Features.ServerLogic.Matches.Domain
{
    public class ServerMatch
    {
        private readonly Random random = new Random();
        public string Guid;
        public Board Board;
        public IList<User> Users;
        public User MatchWinner;
        public bool IsTie;
        public bool IsFinished;
        public bool IsBot;
        public int BotDifficulty;
        
        public void PlayUnitCard(string userId, IUnitCard unitCard)
        {
            var currentRound = Board.RoundsPlayed.Last();

            if (!currentRound.PlayerCards.ContainsKey(userId))
                throw new ApplicationException("Player is not in Match");

            if (currentRound.PlayerCards[userId].UnitCard != null)//check swap
                throw new ApplicationException("Unit card has already been played");

            //if (currentRound.PlayerCards.Where(p=>p.Value.UpgradeCard != null).Count() < Users.Count)
            if (currentRound.RoundState != RoundState.Unit)
                throw new ApplicationException("Unit card sent but not expecting it");

            var upgrades = GetUpgradeCardsByPlayer(currentRound, userId);

            ApplicatePreUnitEffects(userId, upgrades);
            unitCard.Play(this, userId);
            ApplicatePostUnitEffects(userId, upgrades);

            if (IsRoundFinished(currentRound))
            {
                currentRound.ChangeRoundState(RoundState.Finished);
                DetermineWinner();
                if (!DetermineMatchWinner())
                    CreateNewRound();
            }
        }

        private void ApplicatePostUnitEffects(string userId, List<UpgradeCard> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPostUnit(this, userId);
            }
        }

        private void ApplicatePreUnitEffects(string userId, List<UpgradeCard> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPreUnit(this, userId);
            }
        }

        private bool DetermineMatchWinner()
        {
            var winnersGrouped = Board.RoundsPlayed.SelectMany(r => r.PlayerWinner).GroupBy(c => c.Id);
            //TODO: Coul be a tie
            IsFinished = winnersGrouped.Any(w => w.Count() >= 4);
            if (!IsFinished)
                return false;

            IsTie = winnersGrouped.Count(w => w.Count() >= 4) > 1;

            this.MatchWinner = winnersGrouped.FirstOrDefault(w => w.Count() >= 4).First();
            if (IsFinished)
                Board.RoundsPlayed.Last().ChangeRoundState(RoundState.GameFinished);
            return true;

        }

        private void CreateNewRound()
        {
            var round = new Round(this.Users.Select(u=>u.Id))
            {
                RoundUpgradeCard = Board.Deck.TakeUpgradeCards(1).FirstOrDefault(),
                PlayerWinner = null,
                roundNumber = this.Board.RoundsPlayed.Count + 1,
                NextAction = new Random().Next( new ServerConfiguration().GetMaxBotWaitForPlayRoundTimeInSeconds(), new ServerConfiguration().GetRoundTimerDurationInSeconds())
            };
            if (round.roundNumber == 3 || round.roundNumber == 6)
                round.ChangeRoundState(RoundState.Reroll);
            else
                round.ChangeRoundState(RoundState.Upgrade);

            this.Board.RoundsPlayed.Add(round);
        }

        private bool IsRoundFinished(Round round)
        {
            return round.PlayerCards.Where(pc=>pc.Value.UnitCard != null).Count() == this.Users.Count;
        }

        private void DetermineWinner()
        {
            var currentRound = Board.RoundsPlayed.Last();

            //TODO: CAREFUL WITH TIES.
            var higherValueUser = new List<User>();
            var higgerValue = 0;
            foreach (var user in this.Users)
            {
                List<UpgradeCard> upgradeCards = GetUpgradeCardsByPlayer(currentRound, user.Id);
                List<UnitCard> vsUnits = GetVsUnits(currentRound, user.Id);
                var currentUnitCard = currentRound.PlayerCards[user.Id].UnitCard;
                ApplicatePreCalculusEffects(user.Id, upgradeCards);
                var power = currentUnitCard.CalculatePower(this, user.Id);
                ApplicatePostCalculusEffects(user.Id, upgradeCards);
                currentRound.PlayerCards[user.Id].UnitCardPower = power;
                if (power == higgerValue)
                {
                    higherValueUser.Add(user);
                }
                if (power > higgerValue)
                {
                    higherValueUser.Clear();
                    higgerValue = power;
                    higherValueUser.Add(user);
                }
            }
            currentRound.PlayerWinner = higherValueUser;
        }


        private void ApplicatePreCalculusEffects(string userId, List<UpgradeCard> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPreCalculus(this, userId);
            }
        }

        private void ApplicatePostCalculusEffects(string userId, List<UpgradeCard> upgrades)
        {
            foreach (var upgrade in upgrades)
            {
                upgrade.ApplicateEffectPostCalculus(this, userId);
            }
        }



        public List<UnitCard> GetVsUnits(Round currentRound, string userId)
        {
            return currentRound.PlayerCards
                .Where(pc => pc.Key != userId)
                .Select(c => c.Value.UnitCard).ToList();
        }

        public List<UpgradeCard> GetUpgradeCardsByPlayer(Round currentRound, string userId)
        {
            var upgradeCards = Board.RoundsPlayed.SelectMany(r => r.PlayerCards
                .Where(pc => pc.Key == userId && pc.Value.UpgradeCard != null)
                .Select(pc => pc.Value.UpgradeCard)).ToList();
            upgradeCards.Add(currentRound.RoundUpgradeCard);
            return upgradeCards;
        }
    }
}

