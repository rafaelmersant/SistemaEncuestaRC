using EncuestasRC.App_Start;
using EncuestasRC.Models;
using EncuestasRC.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EncuestasRC.Controllers
{
  
    public class SurveyController : Controller
    {
        public class QuestionAnswers
        {
            public int QuestionId { get; set; }
            public string QuestionTitle { get; set; }
            public int QuestionPoints { get; set; }
            public List<Answer> Answers { get; set; }
        }

        public class AnswersPoints
        {
            public int AnswerId { get; set; }
            public int AnswerPoints { get; set; }
        }

        // GET: Survey
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    List<SurveyViewModel> surveysModel = new List<SurveyViewModel>();

                    var allSurveys = db.SurveyHeaders.ToList();

                    var surveys = db.Surveys.OrderByDescending(o => o.CreateDate).ToList();

                    foreach(var survey in surveys)
                    {
                        int completed = allSurveys.Count(s => s.SurveyId == survey.Id);

                        SurveyViewModel surveyViewModel = new SurveyViewModel
                        {
                            Id = survey.Id,
                            Active = survey.Active,
                            CreateBy = survey.CreateBy,
                            CreateDate = survey.CreateDate,
                            Title = survey.Title,
                            Completed = completed
                        };

                        surveysModel.Add(surveyViewModel);
                    }

                    return View(surveysModel);
                }
            }
            catch(Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        public ActionResult CompletedSurveys(int id)
        {
            try
            {
                using(var db = new EncuestaRCEntities())
                {
                    var surveysHeader = db.SurveyHeaders.Where(s => s.SurveyId == id).OrderByDescending(o => o.SurveyEnded).ToList();
                    ViewBag.SurveyTitle = db.Surveys.FirstOrDefault(s => s.Id == id).Title;

                    return View(surveysHeader);
                }
            }
            catch(Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        public ActionResult New(int? id)
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            ViewBag.surveyId = 0;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    if (id.HasValue && id > 0)
                    {
                        var survey = db.Surveys.FirstOrDefault(s => s.Id == id);

                        if (survey != null)
                        {
                            ViewBag.surveyId = survey.Id;
                            ViewBag.surveyTitle = survey.Title;
                            ViewBag.active = survey.Active;
                        }

                        var questions = db.Questions.Where(q => q.SurveyId == id).OrderBy(o => o.SortIndex).ToList();
                        ViewBag.questions = questions;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Encuesta:" + id);
            }
            
            return View();
        }

        [HttpPost]
        public JsonResult CustomerNameForOrder(string order)
        {
            DataSet customerNameData = Helper.GetCustomerName(order);

            try
            {
                if (customerNameData.Tables.Count > 0 && customerNameData.Tables[0].Rows.Count > 0)
                {
                    return Json(new { result = "200", message = customerNameData.Tables[0].Rows[0].ItemArray[0].ToString() });
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "order:" + order);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "No encontrado" });
        }

        [HttpPost]
        public JsonResult NewSurvey(string title, bool active)
        {
            Survey survey;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    survey = new Survey
                    {
                        Title = title,
                        Active = active,
                        CreateBy = Session["email"] != null ? Session["email"].ToString() : "",
                        CreateDate = DateTime.Now,
                        SurveyMonth = DateTime.Today.Month,
                        SurveyYear = DateTime.Today.Year
                    };

                    db.Surveys.Add(survey);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Encuesta:" + title);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = survey.Id });
        }
        
        [HttpPost]
        public JsonResult EditSurvey(string title, bool active, int id)
        {
            Survey survey;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    survey = db.Surveys.FirstOrDefault(s => s.Id == id);
                    survey.Title = title;
                    survey.Active = active;
                    survey.ModifiedDate = DateTime.Now;
                    survey.ModifiedBy = Session["email"] != null ? Session["email"].ToString() : "";

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Encuesta:" + title);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = survey.Id });
        }

        [HttpPost]
        public JsonResult DeleteSurvey(int id)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    Survey _survey = db.Surveys.FirstOrDefault(s => s.Id == id);
                    if (_survey != null)
                    {
                        db.Surveys.Remove(_survey);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "EncuestaId:" + id);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = "Success" });
        }

        [HttpPost]
        public JsonResult NewQuestion(string title, int points, int sortIndex, int surveyId)
        {
            Question question;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    question = new Question
                    {
                        Title = title,
                        Points = points,
                        SortIndex = sortIndex,
                        Active = true,
                        CreateBy = Session["email"] != null ? Session["email"].ToString() : "",
                        CreateDate = DateTime.Now,
                        SurveyId = surveyId,
                    };

                    db.Questions.Add(question);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Pregunta:" + title + " |EncuestaId:" + surveyId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = question.Id });
        }

        [HttpPost]
        public JsonResult EditQuestion(string title, int points, int sortIndex, int questionId)
        {
            Question question;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    question = db.Questions.FirstOrDefault(q => q.Id == questionId);

                    if (question != null)
                    {
                        var currentSortIndex = question.SortIndex;

                        question.Title = title;
                        question.Points = points;
                        question.SortIndex = sortIndex;
                        question.ModifiedDate = DateTime.Now;
                        question.ModifiedBy = Session["email"] != null ? Session["email"].ToString() : "";

                        db.SaveChanges();

                        if (sortIndex != currentSortIndex)
                            Helper.UpdateSortIndexQuestions(question.SurveyId, questionId, sortIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Pregunta:" + title + " | Id:" + questionId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = question.Id });
        }
        
        [HttpPost]
        public JsonResult DeleteQuestion(int id)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    Question _question = db.Questions.FirstOrDefault(q => q.Id == id);
                    if (_question != null)
                    {
                        db.Questions.Remove(_question);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "PreguntaId:" + id);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = "Success" });
        }

        [HttpGet]
        public JsonResult AnswersByQuestion(int questionId)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    var answers = db.Answers.Where(a => a.QuestionId == questionId).ToList();

                    return Json(new { result = "200", data = answers }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Getting Answers --> Pregunta Id:" + questionId);

                return Json(new { result = "500", message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult NewAnswer(string title, int points, int questionId)
        {
            Answer answer;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    answer = new Answer
                    {
                        Title = title,
                        Points = points,
                        Active = true,
                        CreateBy = Session["email"] != null ? Session["email"].ToString() : "",
                        CreateDate = DateTime.Now,
                        QuestionId = questionId,
                    };

                    db.Answers.Add(answer);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Answer:" + title + " | Pregunta Id:" + questionId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = answer.Id });
        }

        [HttpPost]
        public JsonResult EditAnswer(string title, int points, int answerId)
        {
            Answer answer;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    answer = db.Answers.FirstOrDefault(q => q.Id == answerId);

                    if (answer != null)
                    {
                        answer.Title = title;
                        answer.Points = points;
                        answer.ModifiedDate = DateTime.Now;
                        answer.ModifiedBy = Session["email"] != null ? Session["email"].ToString() : "";
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Respuesta:" + title + " | Id:" + answerId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = answer.Id });
        }

        [HttpPost]
        public JsonResult DeleteAnswer(int id)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    Answer _answer = db.Answers.FirstOrDefault(a => a.Id == id);
                    if (_answer != null)
                    {
                        db.Answers.Remove(_answer);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "RespuestaId:" + id);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = "Success" });
        }

        private List<QuestionAnswers> QuestionsWithAnswers(int surveyId)
        {
            List<QuestionAnswers> _questions = new List<QuestionAnswers>();

            using (var db = new EncuestaRCEntities())
            {
                var questions = db.Questions.Where(q => q.SurveyId == surveyId).ToList();
                foreach (var item in questions)
                {
                    var answers = db.Answers.Where(a => a.QuestionId == item.Id).ToList();
                    
                    QuestionAnswers questionAnswers = new QuestionAnswers
                    {
                        QuestionId = item.Id,
                        QuestionTitle = item.Title,
                        QuestionPoints = item.Points.Value,
                        Answers = answers
                    };

                    _questions.Add(questionAnswers);
                }

                return _questions;
            }
        }

        public ActionResult Completar_Encuesta(int id)
        {
            try
            {
                if (Session["role"] == null) return RedirectToAction("Index", "Home");

                using (var db = new EncuestaRCEntities())
                {
                    Survey survey = db.Surveys.FirstOrDefault(s => s.Id == id);
                    if(survey != null)
                    {
                        List<QuestionAnswers> _questions = QuestionsWithAnswers(survey.Id);

                        ViewBag.Survey = survey;
                        ViewBag.Questions = _questions;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Encuesta para completar:" + id);
            }
            
            return View();
        }

        public ActionResult Encuesta_Completada(int id)
        {
            try
            {
                var surveyCompletedViewModel = new SurveyCompletedViewModel();

                using(var db = new EncuestaRCEntities())
                {
                    var surveyHeader = db.SurveyHeaders.FirstOrDefault(h => h.Id == id);
                    var surveyDetail = db.SurveyDetails.Where(d => d.SurveyHeaderId == id).ToList();

                    if(surveyHeader != null)
                    {
                        var survey = db.Surveys.FirstOrDefault(s => s.Id == surveyHeader.SurveyId);
                        
                        surveyCompletedViewModel.Title = survey.Title;
                        surveyCompletedViewModel.surveyHeader = surveyHeader;
                        surveyCompletedViewModel.surveyDetail = surveyDetail;

                        var questions = QuestionsWithAnswers(surveyHeader.SurveyId);
                        ViewBag.questions = questions;

                        return View(surveyCompletedViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }

            return View();
        }

        public ActionResult Reporte_Encuesta(int id)
        {
            try
            {
                if (Session["role"] == null) return RedirectToAction("Index", "Home");

                using (var db = new EncuestaRCEntities())
                {
                    var details = (from h in db.SurveyHeaders
                                   join d in db.SurveyDetails on h.Id equals d.SurveyHeaderId
                                   where h.SurveyId == id
                                   select d).ToList();
                    
                    Survey survey = db.Surveys.FirstOrDefault(s => s.Id == id);
                    if (survey != null)
                    {
                        List<QuestionAnswers> _questions = QuestionsWithAnswers(survey.Id);
                        List<AnswersPoints> _answers = new List<AnswersPoints>();

                        foreach(var question in _questions)
                        {
                            foreach(var answer in question.Answers)
                            {
                                var answerTotal = details.Count(a => a.AnswerId == answer.Id);

                                _answers.Add(new AnswersPoints() { AnswerId = answer.Id, AnswerPoints = answerTotal });
                            }
                        }

                        ViewBag.Survey = survey;
                        ViewBag.Questions = _questions;
                        ViewBag.Answers = _answers;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Reporte de Encuesta:" + id);
            }

            return View();
        }

        //Save Survey Header
        [HttpPost]
        public JsonResult SaveSurveyHeader(int Id, int surveyId, string customer, int customerType, string orderNo, string date)
        {
            SurveyHeader surveyHeader;

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    surveyHeader = db.SurveyHeaders.FirstOrDefault(a => a.Id == Id);
                    if (surveyHeader != null)
                    {
                        surveyHeader.Customer = customer;
                        surveyHeader.CustomerType = customerType;
                        surveyHeader.OrderNo = orderNo;
                        surveyHeader.SurveyEnded = DateTime.Now;
                    }
                    else
                    {
                        surveyHeader = new SurveyHeader
                        {
                            SurveyId = surveyId,
                            Customer = customer,
                            CustomerType = customerType,
                            OrderNo = orderNo,
                            SurveyEnded = Convert.ToDateTime(date),
                            UserLogged = Session["email"] != null ? Session["email"].ToString() : ""
                        };
                        
                        db.SurveyHeaders.Add(surveyHeader);
                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Save Survey SurveyId:" + surveyId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = surveyHeader.Id });
        }

        //Save Survey Detail
        [HttpPost]
        public JsonResult SaveSurveyDetail(int surveyHeaderId, int questionId, int answerId, int points)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    db.SurveyDetails.Add(new SurveyDetail
                    {
                        SurveyHeaderId = surveyHeaderId,
                        QuestionId = questionId,
                        AnswerId = answerId,
                        Points = points,
                        CreationDate = DateTime.Now
                    });

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Save Survey SurveyHeaderId:" + surveyHeaderId);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "200", message = "Success" });
        }

    }
}