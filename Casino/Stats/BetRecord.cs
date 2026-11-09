using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp_Exam.Casino.Stats
{
    public class BetRecord
    {
        public string UserId { get; set; }
        public string GameName { get; set; }
        public decimal BetAmount { get; set; }
        public decimal Payout { get; set; }
        public BetResult Result { get; set; }
        public DateTime Date { get; set; } // Дата та час ставки
    }
}
