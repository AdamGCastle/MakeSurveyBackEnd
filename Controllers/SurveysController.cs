using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MakeASurvey.Data;
using MakeASurvey.Models;
using Microsoft.AspNetCore.Cors;

namespace MakeASurvey.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors]
    public class SurveysController : ControllerBase
    {
        private readonly MakeASurveyContext _context;

        public SurveysController(MakeASurveyContext context)
        {
            _context = context;
        }

        // GET: api/Surveys
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Survey>>> GetSurvey()
        {
            return await _context.Surveys.ToListAsync();
        }

        // GET: api/Surveys/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Survey>> GetSurvey(string id)
        {
            Survey survey = null;

            if(Int32.TryParse(id, out int ID))
            {
                survey = await _context.Surveys.AsNoTracking().Include(s => s.Questions).FirstOrDefaultAsync(s => s.SurveyID == ID);


                if (survey == null)
                {
                    return NotFound();
                }

                foreach (Question q in survey.Questions)
                {
                    var answers = _context.Answers.Where(a => a.QuestionID == q.QuestionID);
                    q.Answers = answers;
                }
                return survey;
            }
            return survey;        

            

            
            
        }

        // PUT: api/Surveys/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSurvey(int id, Survey survey)
        {
            if (id != survey.SurveyID)
            {
                return BadRequest();
            }

            _context.Entry(survey).State = EntityState.Modified;

            foreach (var q in survey.Questions)
            {   
                if (_context.Questions.Any(x => x.QuestionID == q.QuestionID))
                {
                    _context.Entry(q).State = EntityState.Modified;
                }
                else
                {

                    Question NewQuestion = new Question { SurveyID = survey.SurveyID, Text = q.Text, TotalResponses = 0, IsMultipleChoice = q.IsMultipleChoice };
                    _context.Questions.Add(NewQuestion);
                    //This savechanges() is important otherwise the questions aren't saved into the database
                    await _context.SaveChangesAsync();
                    
                }               

            }

            await _context.SaveChangesAsync();

            foreach(Question q in survey.Questions)
            {
                foreach (Answer a in q.Answers)
                {
                    if (_context.Answers.Any(x => x.AnswerID == a.AnswerID))
                    {
                        _context.Entry(a).State = EntityState.Modified;
                    }
                    else
                    {
                        Answer NewAnswer = new Answer { CountResponses = 0, Percentage = 0, Text = a.Text, QuestionID = q.QuestionID };
                        _context.Answers.Add(a);
                    }
                }

            }

            await _context.SaveChangesAsync();



            //Deletes questions and answers from database if they're associated with this survey but have been taken out in the updated version
            foreach (Question q in _context.Questions)
            {
                if (q.SurveyID == id)
                {
                    if(!survey.Questions.Any(x => x.QuestionID == q.QuestionID))
                    {
                        _context.Remove(q);
                        break;
                    }
                    foreach(Answer a in q.Answers)
                    {
                        if(!q.Answers.Any(x => x.AnswerID == a.AnswerID))
                        {
                            _context.Remove(a);
                        } 
                    }
                    
                }  
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SurveyExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut]
        public void Put(string[] responses)
        {
            int[] responseInts = Array.ConvertAll(responses, s => int.Parse(s));
            List<int> allIDsOfQuestionsAnswered = new List<int>();

            foreach (int a in responseInts)
            {
                Answer toUpdate = _context.Answers.FirstOrDefault(x => x.AnswerID == a);                
                allIDsOfQuestionsAnswered.Add(toUpdate.QuestionID);
            }

            List<int> questionsAnswered = allIDsOfQuestionsAnswered.Distinct().ToList();
            
            foreach (int q in questionsAnswered)
            {
                Question toUpdate = _context.Questions.Include(x => x.Answers).FirstOrDefault(x => x.QuestionID == q);                    
                   
                toUpdate.TotalResponses++;
                foreach(Answer a in toUpdate.Answers)
                {
                    if(responseInts.Contains(a.AnswerID))
                    {
                        a.CountResponses++;
                    }
                    int totalResponses = toUpdate.TotalResponses;
                    double ratio = (double)a.CountResponses / (double)totalResponses;
                    double percentage = 100 * Math.Round(ratio, 2);
                    a.Percentage = (int)percentage;
                }
                
            }
            _context.SaveChanges();

            //int currentSurveyId;
            //if (questionsAnswered.Count > 0 )
            //{
            //    Question q = _context.Questions.FirstOrDefault(x => x.QuestionID == questionsAnswered[0]);
            //    currentSurveyId = q.SurveyID;
            //}
                     

            //foreach (Answer a in _context.Answers)           
            //{   
                                                
            //    int totalResponses = _context.Questions.FirstOrDefault(q => q.QuestionID == a.QuestionID).TotalResponses;
            //    double ratio = (double)a.CountResponses / (double)totalResponses;
            //    double percentage = 100 * Math.Round(ratio, 2);
            //    a.Percentage = (int)percentage;
                

            //}
            //_context.SaveChanges();


        }

        // POST: api/Surveys
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<ReceiveSurvey>> PostSurvey(ReceiveSurvey survey)
        {
            Survey NewSurvey = new Survey();
            NewSurvey.Name = survey.name;
             _context.Surveys.Add(NewSurvey);
            await _context.SaveChangesAsync();

            int SurveyID = NewSurvey.SurveyID;
            

            foreach(ReceiveQuestion q in survey.questions)
            {
                Question NewQuestion = new Question { SurveyID = SurveyID, Text = q.text, TotalResponses = 0, IsMultipleChoice = q.IsMultipleChoice };
                _context.Questions.Add(NewQuestion);
                //This savechanges() is important otherwise the questions aren't saved into the database
                await _context.SaveChangesAsync();

                foreach (ReceiveAnswer a in q.answers)
                {
                    Answer NewAnswer = new Answer { CountResponses = 0, Percentage = 0, Text = a.text, QuestionID = NewQuestion.QuestionID };
                    _context.Answers.Add(NewAnswer);
                    
                }
                await _context.SaveChangesAsync();


            }
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSurvey", new { id = NewSurvey.SurveyID }, survey);
        }

        // DELETE: api/Surveys/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Survey>> DeleteSurvey(int id)
        {
            var survey = await _context.Surveys.FindAsync(id);
            if (survey == null)
            {
                return NotFound();
            }

            _context.Surveys.Remove(survey);

            

            foreach(Question q in _context.Questions)
            {
                if(q.SurveyID == id)
                {
                    foreach(Answer a in _context.Answers)
                    {
                        if(a.QuestionID == q.QuestionID)
                        {
                            _context.Remove(a);
                        }
                    }
                    _context.Remove(q);
                }
            }
           


            await _context.SaveChangesAsync();

            return survey;
        }

        private bool SurveyExists(int id)
        {
            return _context.Surveys.Any(e => e.SurveyID == id);
        }
    }
}
