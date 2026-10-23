using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Exam.Casino.Stats
{
    public class UserStatistics
    {
        public string UserId { get; set; }
        public int TotalBets { get; set; }
        public decimal TotalBetsAmount { get; set; }
        public decimal TotalPayouts { get; set; }
        public int TotalWins { get; set; }
        public int TotalLosses { get; set; }
        public int TotalDraws { get; set; }
        public int TotalGamesPlayed { get; set; }
        public decimal NetProfit => TotalPayouts - TotalBetsAmount;
        public decimal WinRate => TotalGamesPlayed > 0 ? (decimal)TotalWins / TotalGamesPlayed : 0;
        public string FavouriteGame { get; set; }
        public decimal BiggestBet { get; set; }
        public decimal BiggestPayout { get; set; }
    }
}
