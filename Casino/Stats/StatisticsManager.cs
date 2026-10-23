using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Exam.Casino.Stats
{
    public class StatisticsManager
    {
        private readonly List<BetRecord> _betRecords = new();
        public int TotalBets => _betRecords.Count;

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

            _betRecords.Add(record);
        }

        public List<BetRecord> GetAllBets() { return new List<BetRecord>(_betRecords); }
    }

}