using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MakeASurvey.Models;

namespace MakeASurvey.Data
{
    public class MakeASurveyContext : DbContext
    {
        public MakeASurveyContext (DbContextOptions<MakeASurveyContext> options)
            : base(options)
        {
        }

        public DbSet<MakeASurvey.Models.Survey> Surveys { get; set; }
        public DbSet<MakeASurvey.Models.Question> Questions { get; set; }
        public DbSet<MakeASurvey.Models.Answer> Answers { get; set; }
    }
}
