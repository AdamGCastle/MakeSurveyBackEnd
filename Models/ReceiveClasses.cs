using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MakeASurvey.Models
{
    public class ReceiveAnswer
    {
        public string text { get; set; }
        public int answerID { get; set; }
        public int questionID { get; set; }
        public  int countResponses { get; set; }
        public int percentage { get; set; }
    }

    public class ReceiveQuestion
    {
        public string text { get; set; }
        public int questionID { get; set; }
        public int surveyID { get; set; }
        public ReceiveAnswer[] answers { get; set; }
        public bool isMultipleChoice { get; set; }
        public int totalResponses { get; set; }
    }

    public class ReceiveSurvey
    {
        public string name { get; set; }
        public ReceiveQuestion[] questions { get; set; }
        public int surveyID { get; set; }
        public int totalResponses { get; set; }

    }


}
