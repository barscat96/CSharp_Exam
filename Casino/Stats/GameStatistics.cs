using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Exam.Casino.Stats
{
    public class GameStatistics
    {
        public string GameName { get; set; }
        public int TotalBets { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalPayouts { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int TotalDraws { get; set; }
        public decimal NetResult => TotalBetAmount - TotalPayouts;
        public decimal AverageBetAmount => TotalBets > 0 ? TotalBetAmount / TotalBets : 0;
        public decimal AveragePayout => TotalBets > 0 ? TotalPayouts / TotalBets : 0;
        public decimal WinRatePercent => TotalBets > 0 ? (decimal)TotalWins / TotalBets * 100 : 0;
    }
}
