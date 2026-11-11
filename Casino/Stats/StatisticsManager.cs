using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharp_Exam.Casino.Services.Logging;

namespace CSharp_Exam.Casino.Stats
{
    public class StatisticsManager
    {
        private readonly LoggingService _logger;
        public StatisticsManager(LoggingService? logger = null)
        {
            _logger = logger ?? LoggingService.Default;
        }
        
        private readonly List<BetRecord> _betRecords = new();
        public int TotalBets => _betRecords.Count;

        public decimal TotalBetAmount => _betRecords.Sum(record => record.BetAmount);
        public decimal TotalPayouts => _betRecords.Sum(record => record.Payout);
        public decimal NetResult => TotalBetAmount - TotalPayouts;

        public void AddBet(BetRecord record)
        {
            if (record == null)
            {
                _logger.Log(LogLevel.Warning, "Bet rejected: record is null.");
                throw new ArgumentNullException(nameof(record));
            }

            if (string.IsNullOrWhiteSpace(record.UserId))
            {
                _logger.Log(LogLevel.Warning, "Bet rejected: User ID is empty.");
                throw new ArgumentException("User ID cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(record.GameName))
            {
                _logger.Log(LogLevel.Warning, "Bet rejected: game name is empty.");
                throw new ArgumentException("Game name cannot be empty.");
            }

            if (record.BetAmount <= 0)
            {
                _logger.Log(LogLevel.Warning, $"Bet rejected: invalid bet amount ({record.BetAmount}).");
                throw new ArgumentException("Bet amount must be greater than zero.");
            }

            if (record.Payout < 0)
            {
                _logger.Log(LogLevel.Warning, $"Bet rejected: invalid payout ({record.Payout}).");
                throw new ArgumentException("Payout cannot be negative.");
            }

            if (record.Date == default)
            {
                record.Date = DateTime.Now;
            }

            if (!Enum.IsDefined(typeof(BetResult), record.Result) || record.Result == BetResult.Unknown)
            {
                _logger.Log(LogLevel.Warning, $"Bet rejected: invalid result ({record.Result}).");
                throw new ArgumentException("Invalid bet result.");
            }

            _betRecords.Add(record);

            _logger.Log(
                LogLevel.Info,
                $"Bet added for: User={record.UserId}, Game={record.GameName}, Amount={record.BetAmount}, Payout={record.Payout}, Result={record.Result}"
            );
        }

        public GameStatistics GetGameStatistics(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                _logger.Log(LogLevel.Warning, "Cannot get game statistics: game name is empty.");
                throw new ArgumentException("Game name can't be empty.");
            }

            string normalizedGameName = gameName.Trim();

            _logger.Log(LogLevel.Debug, $"Getting statistics for game: {normalizedGameName}");

            var gameBets = _betRecords
                .Where(record => record.GameName.Equals(normalizedGameName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var statistics = new GameStatistics
            {
                GameName = normalizedGameName,
                TotalBets = gameBets.Count,
                TotalBetAmount = gameBets.Sum(record => record.BetAmount),
                TotalPayouts = gameBets.Sum(record => record.Payout),
                TotalWins = gameBets.Count(record => record.Result == BetResult.Win),
                TotalLosses = gameBets.Count(record => record.Result == BetResult.Loss),
                TotalDraws = gameBets.Count(record => record.Result == BetResult.Draw)
            };

            _logger.Log(
                LogLevel.Debug,
                $"Game statistics retrieved: Game={normalizedGameName}, Bets={statistics.TotalBets}"
            );

            return statistics;
        }

        public PeriodStatisticsReport GetPeriodReport(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                _logger.Log(LogLevel.Warning, "Cannot create report: start date is later than end date.");
                throw new ArgumentException("Start date must be earlier than or equal to end date.");
            }

            _logger.Log(LogLevel.Debug, $"Creating statistics report for period: {startDate} - {endDate}");

            var periodBets = _betRecords
                .Where(record => record.Date >= startDate && record.Date <= endDate)
                .ToList();

            var report = new PeriodStatisticsReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBets = periodBets.Count,
                TotalBetAmount = periodBets.Sum(record => record.BetAmount),
                TotalPayouts = periodBets.Sum(record => record.Payout),
                TotalWins = periodBets.Count(record => record.Result == BetResult.Win),
                TotalLosses = periodBets.Count(record => record.Result == BetResult.Loss),
                TotalDraws = periodBets.Count(record => record.Result == BetResult.Draw),
                GameStatistics = periodBets
                                    .GroupBy(record => record.GameName,
                                            StringComparer.OrdinalIgnoreCase)
                                    .Select(group => new GameStatistics
                                    {
                                        GameName = group.Key,
                                        TotalBets = group.Count(),
                                        TotalBetAmount = group.Sum(record => record.BetAmount),
                                        TotalPayouts = group.Sum(record => record.Payout),
                                        TotalWins = group.Count(record => record.Result == BetResult.Win),
                                        TotalLosses = group.Count(record => record.Result == BetResult.Loss),
                                        TotalDraws = group.Count(record => record.Result == BetResult.Draw)
                                    })
                                    .OrderBy(game => game.GameName)
                                    .ToList()
            };

            _logger.Log(LogLevel.Debug, $"Statistics report created. Bets: {report.TotalBets}, games: {report.GameStatistics.Count}.");

            return report;
        }

        public List<GameStatistics> GetAllGameStatistics()
        {
            _logger.Log(LogLevel.Debug, "Getting statistics for all games.");

            var statistics = _betRecords
                .GroupBy(record => record.GameName,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group => new GameStatistics
                {
                    GameName = group.Key,
                    TotalBets = group.Count(),
                    TotalBetAmount = group.Sum(record => record.BetAmount),
                    TotalPayouts = group.Sum(record => record.Payout),
                    TotalWins = group.Count(record => record.Result == BetResult.Win),
                    TotalLosses = group.Count(record => record.Result == BetResult.Loss),
                    TotalDraws = group.Count(record => record.Result == BetResult.Draw)
                }).OrderBy(statistics => statistics.GameName).ToList();

            _logger.Log(LogLevel.Debug, $"Statistics retrieved for {statistics.Count} games.");

            return statistics;
        }

        public List<GameStatistics> GetGamesByPopularity()
        {
            _logger.Log(LogLevel.Debug, "Getting games ranked by popularity.");

            var ranking = GetAllGameStatistics()
                .OrderByDescending(game => game.TotalBets)
                .ThenBy(game => game.GameName)
                .ToList();

            _logger.Log(LogLevel.Debug, $"Popularity ranking created for {ranking.Count} games.");

            return ranking;
        }

        public UserStatistics GetUserStatistics(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.Log(LogLevel.Warning, "Cannot get user statistics: User ID is empty.");
                throw new ArgumentException("User ID can't be empty.");
            }

            string normalizedUserId = userId.Trim();

            _logger.Log(LogLevel.Debug, $"Getting statistics for user: {normalizedUserId}");

            var userBets = _betRecords
                .Where(record => record.UserId.Equals(normalizedUserId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var favouriteGame = userBets
                .GroupBy(record => record.GameName)
                .OrderByDescending(group => group.Count())
                .FirstOrDefault();

            var statistics = new UserStatistics
            {
                UserId = normalizedUserId,
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

            _logger.Log(
                LogLevel.Debug,
                $"User statistics retrieved: User={normalizedUserId}, Bets={statistics.TotalBets}"
            );

            return statistics;
        }

        public List<BetRecord> GetBetsByDateRange(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                _logger.Log(LogLevel.Warning, "Cannot filter bets: start date is later than end date.");
                throw new ArgumentException("Start date must be earlier than end date.");
            }

            _logger.Log(LogLevel.Debug, $"Filtering bets by date: {startDate} - {endDate}");

            var bets = _betRecords
                .Where(record => record.Date >= startDate && record.Date <= endDate)
                .ToList();

            _logger.Log(LogLevel.Debug, $"Date filter completed. Found bets: {bets.Count}");

            return bets;
        }

        public List<BetRecord> GetBetsByGame(string gameName)
        {
            if (string.IsNullOrWhiteSpace(gameName))
            {
                _logger.Log(LogLevel.Warning, "Cannot filter bets: game name is empty.");
                throw new ArgumentException("Game name cannot be empty.");
            }

            string normalizedGameName = gameName.Trim();

            _logger.Log(LogLevel.Debug, $"Filtering bets by game: {normalizedGameName}");

            var bets = _betRecords
                .Where(record => record.GameName.Equals(
                    normalizedGameName,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.Log(LogLevel.Debug, $"Game filter completed. Found bets: {bets.Count}");

            return bets;
        }

        public List<BetRecord> GetBetsByUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.Log(LogLevel.Warning, "Cannot filter bets: User ID is empty.");
                throw new ArgumentException("User ID cannot be empty.");
            }

            string normalizedUserId = userId.Trim();

            _logger.Log(LogLevel.Debug, $"Filtering bets by user: {normalizedUserId}");

            var bets = _betRecords
                .Where(record => record.UserId.Equals(
                    normalizedUserId,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            _logger.Log(LogLevel.Debug, $"User filter completed. Found bets: {bets.Count}");

            return bets;
        }

        public List<BetRecord> GetBetsSortedByDate()
        {
            _logger.Log(LogLevel.Debug, "Sorting bets by date.");

            var bets = _betRecords
                .OrderByDescending(record => record.Date)
                .ToList();

            _logger.Log(LogLevel.Debug, $"Date sorting completed. Bets sorted: {bets.Count}");

            return bets;
        }

        public List<BetRecord> GetBetsSortedByAmount()
        {
            _logger.Log(LogLevel.Debug, "Sorting bets by amount.");

            var bets = _betRecords
                .OrderByDescending(record => record.BetAmount)
                .ToList();

            _logger.Log(LogLevel.Debug, $"Amount sorting completed. Bets sorted: {bets.Count}");

            return bets;
        }

        public BetRecord? GetBiggestBet()
        {
            _logger.Log(LogLevel.Debug, "Searching for biggest bet.");

            var bet = _betRecords
                .OrderByDescending(record => record.BetAmount)
                .FirstOrDefault();

            _logger.Log(
                LogLevel.Debug,
                bet == null
                    ? "No bets found."
                    : $"Biggest bet found: User={bet.UserId}, Amount={bet.BetAmount}"
            );

            return bet;
        }

        public BetRecord? GetBiggestPayout()
        {
            _logger.Log(LogLevel.Debug, "Searching for biggest payout.");

            var bet = _betRecords
                .OrderByDescending(record => record.Payout)
                .FirstOrDefault();

            _logger.Log(
                LogLevel.Debug,
                bet == null
                    ? "No bets found."
                    : $"Biggest payout found: User={bet.UserId}, Payout={bet.Payout}"
            );

            return bet;
        }

        public List<BetRecord> GetAllBets() { return new List<BetRecord>(_betRecords); }
    }

}