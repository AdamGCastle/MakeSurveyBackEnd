using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MakeASurvey.Models
{
    public class Answer
    {      
        public Answer() { }

        public Answer(string text, int questionID)
        {
            Text = text;
            QuestionID = questionID;
        }

        [Key]
        public int AnswerID { get; set; }
        public string Text { get; set; }
        public int CountResponses { get; set; } = 0;
        public int Percentage { get; set; }
        public int QuestionID { get; set; }
    }
    
}
