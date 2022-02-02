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

            if (Int32.TryParse(id, out int ID))
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
        public async Task<IActionResult> PutSurvey(int id, ReceiveSurvey receiveSurvey)
        {

            if (receiveSurvey.surveyID != id)
            {
                return BadRequest();
            }

            //UPDATE THE SURVEY OBJECT IN CASE ITS NAME HAS BEEN CHANGED

            Survey survey = new Survey
            {
                SurveyID = id,
                Name = receiveSurvey.name,
                TotalResponses = receiveSurvey.totalResponses,
            };

            _context.Entry(survey).State = EntityState.Modified;


            //GROUPS ALL QUESTIONS IN DB THAT BELONG TO THIS SURVEY
            List<Question> surveyQs = _context.Questions.Where(q => q.SurveyID == id).ToList();

            //ITERATES THROUGH THAT GROUP OF QUESTIONS IN DB
            foreach (Question q in surveyQs)
            {
                //GROUPS ALL ANSWERS FROM DB THAT BELONG TO THIS QUESTION
                List<Answer> QuestionsAs = _context.Answers.Where(a => a.QuestionID == q.QuestionID).ToList();

                //REMOVES THE QUESTION AND THEIR ANSWERS FROM DB IF THEY BELONG TO THIS SURVEY BUT AREN'T IN THE NEWLY EDITED VERSION
                if (!receiveSurvey.questions.Any(x => x.questionID == q.QuestionID))
                {
                    foreach (Answer a in QuestionsAs)
                    {
                        _context.Remove(a);
                    }
                    _context.Remove(q);
                }

                //REMOVES ANSWERS FROM DB IF THEY BELONG TO THE QUESTION BUT AREN'T IN THE NEWLY EDITED VERSION
                ReceiveQuestion newlyEditedQuestion = receiveSurvey.questions.FirstOrDefault(x => x.questionID == q.QuestionID);

                foreach (Answer a in QuestionsAs)
                {
                    if (!newlyEditedQuestion.answers.Any(x => x.answerID == a.AnswerID))
                    {

                        _context.Remove(a);
                    }
                }
            }


            await _context.SaveChangesAsync();



            //ADD NEW QUESTIONS AND ANSWERS AND UPDATE EXISTING ONES

            foreach (ReceiveQuestion receiveQuestion in receiveSurvey.questions)
            {
                Question NewQuestion = new Question
                {
                    Text = receiveQuestion.text,
                    SurveyID = receiveSurvey.surveyID,
                    IsMultipleChoice = receiveQuestion.isMultipleChoice,
                    TotalResponses = receiveQuestion.totalResponses,
                    QuestionID = receiveQuestion.questionID >= 0 ? receiveQuestion.questionID : 0
                };

                if (NewQuestion.QuestionID == 0)
                {
                    _context.Questions.Add(NewQuestion);
                    //This savechanges() is important otherwise the questions aren't saved into the database
                    await _context.SaveChangesAsync();
                    foreach (ReceiveAnswer a in receiveQuestion.answers)
                    {
                        Answer NewAnswer = new Answer { CountResponses = 0, Percentage = 0, Text = a.text, QuestionID = NewQuestion.QuestionID };
                        _context.Answers.Add(NewAnswer);
                    }
                }
                else if (_context.Questions.Any(x => x.QuestionID == NewQuestion.QuestionID))
                {
                    if (!ModelState.IsValid)
                    {
                        return NoContent();
                    }
                    //NewQuestion.Answers = _context.Answers.Where(a => a.QuestionID == NewQuestion.QuestionID);
                    Question QuestiontoUpdate = _context.Questions.FirstOrDefault(contextQuestion => contextQuestion.QuestionID == NewQuestion.QuestionID);
                    QuestiontoUpdate.Text = NewQuestion.Text;
                    _context.Entry(QuestiontoUpdate).State = EntityState.Modified;
                    _context.SaveChanges();

                   

                    foreach (ReceiveAnswer a in receiveQuestion.answers)
                    {
                        Answer NewAnswer = new Answer { CountResponses = a.countResponses, Percentage = a.percentage, Text = a.text, QuestionID = receiveQuestion.questionID, AnswerID = a.answerID };
                        if (a.answerID == 0)
                        {
                            if (!ModelState.IsValid)
                            {
                                return NoContent();
                            }
                            
                            _context.Answers.Add(NewAnswer);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            Answer AnswertoUpdate = _context.Answers.FirstOrDefault(contextAnswer => contextAnswer.AnswerID == NewAnswer.AnswerID);
                            AnswertoUpdate.Text = NewAnswer.Text;
                            _context.Attach(AnswertoUpdate).State = EntityState.Modified;
                        }

                    }
                }
            }

            await _context.SaveChangesAsync();





            //try
            //{
            //    _context.SaveChanges();
            //}
            //catch (DbUpdateConcurrencyException)
            //{
            //    if (!SurveyExists(id))
            //    {
            //        return NotFound();
            //    }
            //    else
            //    {
            //        throw;
            //    }
            //}

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
            Question question = _context.Questions.FirstOrDefault(q => questionsAnswered.Contains(q.QuestionID));
            Survey survey = _context.Surveys.FirstOrDefault(s => s.SurveyID == question.SurveyID);
            survey.TotalResponses++;
            _context.SaveChanges();

            foreach (int q in questionsAnswered)
            {
                Question toUpdate = _context.Questions.Include(x => x.Answers).FirstOrDefault(x => x.QuestionID == q);

                toUpdate.TotalResponses++;
                foreach (Answer a in toUpdate.Answers)
                {
                    if (responseInts.Contains(a.AnswerID))
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


            foreach (ReceiveQuestion q in survey.questions)
            {
                Question NewQuestion = new Question { SurveyID = SurveyID, Text = q.text, TotalResponses = 0, IsMultipleChoice = q.isMultipleChoice };
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



            foreach (Question q in _context.Questions)
            {
                if (q.SurveyID == id)
                {
                    foreach (Answer a in _context.Answers)
                    {
                        if (a.QuestionID == q.QuestionID)
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

// FROM PREVIOUS VERSION OF PUT SURVEY

//foreach (Question q in survey.Questions)
//{
//    List<Answer> qAnswersFromDb = _context.Answers.Where(a => a.QuestionID == q.QuestionID).ToList();
//    List<Answer> newAnswers = q.Answers.ToList();

//    foreach (Answer a in qAnswersFromDb)
//    {
//        if (!newAnswers.Contains(a))
//        {
//            _context.Answers.Remove(a);
//        }
//    }

//    if (q.Answers != null)
//    {
//        foreach (Answer a in q.Answers)
//        {
//            if (_context.Answers.Any(x => x.AnswerID == a.AnswerID))
//            {
//                _context.Entry(a).State = EntityState.Modified;
//            }
//            else
//            {
//                Answer NewAnswer = new Answer { CountResponses = 0, Percentage = 0, Text = a.Text, QuestionID = q.QuestionID };
//                _context.Answers.Add(a);
//            }
//        }
//    }



//}


//Deletes questions and answers from database if they're associated with this survey but have been taken out in the updated version

