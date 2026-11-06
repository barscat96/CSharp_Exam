using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp_Exam.Casino.Stats
{
    public class StatisticsManager
    {
        private readonly List<BetRecord> _betRecords = new();
        public int TotalBets => _betRecords.Count;

        public decimal TotalBetAmount => _betRecords.Sum(record => record.BetAmount);
        public decimal TotalPayouts => _betRecords.Sum(record => record.Payout);
        public decimal NetResult => TotalBetAmount - TotalPayouts;

        public void AddBet(BetRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (string.IsNullOrWhiteSpace(record.UserId))
            {
                throw new ArgumentException("User ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(record.GameName))
            {
                throw new ArgumentException("Game name cannot be empty.");
            }

            if (record.BetAmount <= 0)
            {
                throw new ArgumentException("Bet amount must be greater than zero.");
            }

            if (record.Payout < 0)
            {
                throw new ArgumentException("Payout cannot be negative.");
            }

            if (record.Date == default)
            {
                record.Date = DateTime.Now;
            }

            if (!Enum.IsDefined(typeof(BetResult), record.Result) || record.Result == BetResult.Unknown)
            {
                throw new ArgumentException("Invalid bet result.");
            }

            _betRecords.Add(record);
        }

        public GameStatistics GetGameStatistics(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                throw new ArgumentException("Game name can't be empty.");
            }

            var gameBets = _betRecords.Where(record => record.GameName.Equals(gameName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            return new GameStatistics
            {
                GameName = gameName.Trim(),
                TotalBets = gameBets.Count,
                TotalBetAmount = gameBets.Sum(record => record.BetAmount),
                TotalPayouts = gameBets.Sum(record => record.Payout),
                TotalWins = gameBets.Count(record => record.Result == BetResult.Win),
                TotalLosses = gameBets.Count(record => record.Result == BetResult.Loss),
                TotalDraws = gameBets.Count(record => record.Result == BetResult.Draw)
            };
        }

        public UserStatistics GetUserStatistics(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID can't be empty.");
            }

            var userBets = _betRecords.Where(record => record.UserId.Equals(userId.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            var favouriteGame = userBets.GroupBy(record => record.GameName).OrderByDescending(group => group.Count()).FirstOrDefault();

            return new UserStatistics
            {
                UserId = userId.Trim(),
                TotalBets = userBets.Count,
                TotalBetsAmount = userBets.Sum(record => record.BetAmount),
                TotalPayouts = userBets.Sum(record => record.Payout),
                TotalWins = userBets.Count(record => record.Result == BetResult.Win),
                TotalLosses = userBets.Count(record => record.Result == BetResult.Loss),
                TotalDraws = userBets.Count(record => record.Result == BetResult.Draw),
                TotalGamesPlayed = userBets.Count,
                FavouriteGame = favouriteGame?.Key ?? string.Empty,
                BiggestBet = userBets.Count > 0 ? userBets.Max(record => record.BetAmount) : 0,
                BiggestPayout = userBets.Count > 0 ? userBets.Max(record => record.Payout) : 0
            };
        }

        public List<BetRecord> GetBetsByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be earlier than end date.");
            }

            return _betRecords
                .Where(record => record.Date >= startDate && record.Date <= endDate)
                .ToList();
        }

        public List<BetRecord> GetBetsByGame(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                throw new ArgumentException("Game name cannot be empty.");
            }

            return _betRecords
                .Where(record => record.GameName.Equals(
                    gameName.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<BetRecord> GetBetsByUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be empty.");
            }

            return _betRecords
                .Where(record => record.UserId.Equals(
                    userId.Trim(),
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<BetRecord> GetBetsSortedByDate()
        {
            return _betRecords
                .OrderByDescending(record => record.Date)
                .ToList();
        }

        public List<BetRecord> GetBetsSortedByAmount()
        {
            return _betRecords
                .OrderByDescending(record => record.BetAmount)
                .ToList();
        }

        public BetRecord? GetBiggestBet()
        {
            return _betRecords
                .OrderByDescending(record => record.BetAmount)
                .FirstOrDefault();
        }

        public BetRecord? GetBiggestPayout()
        {
            return _betRecords
                .OrderByDescending(record => record.Payout)
                .FirstOrDefault();
        }

        public List<BetRecord> GetAllBets() { return new List<BetRecord>(_betRecords); }
    }

}