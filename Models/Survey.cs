using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MakeASurvey.Models
{
    public class Survey
    {
        [Key]
        public int SurveyID { get; set; }
        public string Name { get; set;  }
        public int TotalResponses { get; set; }
        public ICollection<Question> Questions { get; set; }
    }
}
