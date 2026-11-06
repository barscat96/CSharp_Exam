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
    }
}
