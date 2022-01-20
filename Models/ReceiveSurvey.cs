using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MakeASurvey.Models
{
    public class ReceiveSurvey
    {
        public string name { get; set; }
        public ReceiveQuestion[] questions { get; set; }
    }

    public class ReceiveQuestion
    {
        public string text { get; set; }
        public ReceiveAnswer[] answers { get; set; }
        public bool IsMultipleChoice { get; set; }
        public int numberInSurvey { get; set; }

    }
    public class ReceiveAnswer
    {
        public string text { get; set; }
    }
}
