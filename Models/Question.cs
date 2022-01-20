using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MakeASurvey.Models;

namespace MakeASurvey.Models
{
    public class Question
    {
        public Question()
        {
        }
        public Question(string text)
        {
            Text = text;
        }

        [Key]
        public int QuestionID { get; set; }
        public int SurveyID { get; set; }
        public string Text { get; set; }
        public bool IsMultipleChoice { get; set; }
        public int TotalResponses { get; set; }

        //IN FUTURE THIS FUNCTIONALITY WOULD ASSIST CREATING NEW FORMS

        [ForeignKey("QuestionID")]
        public IEnumerable<Answer> Answers { get; set; }

        public void AddAnswers(List<string> answerTexts)
        {
            foreach (string ans in answerTexts)
            {
                List<Answer> a = Answers.ToList();
                a.Add(new Answer(ans, QuestionID));
                Answers = a;
            }
        }
        public void AddAnswer(string text)
        {
            List<Answer> a = Answers.ToList();
            a.Add(new Answer(text, QuestionID));
            Answers = a;

        }
    }
}
