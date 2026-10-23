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
            
        }
    }
}