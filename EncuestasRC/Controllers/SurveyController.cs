using EncuestasRC.App_Start;
using EncuestasRC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EncuestasRC.Controllers
{
  
    public class SurveyController : Controller
    {
        // GET: Survey
        public ActionResult Index()
        {
            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    var surveys = db.Surveys.OrderByDescending(o => o.CreateDate).ToList();
                    return View(surveys);
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
        public JsonResult NewSurvey(string title, bool active)
        {
            Survey survey;

            try
            {
                using(var db = new EncuestaRCEntities())
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
                            UpdateSortIndexQuestions(question.SurveyId, questionId, sortIndex);
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

        private void UpdateSortIndexQuestions(int surveyId, int questionId, int sortIndex)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    bool sort = false;
                    int index = 1;

                    var questions = db.Questions.Where(q => q.SortIndex >= sortIndex && q.Id != questionId && q.SurveyId == surveyId).OrderBy(o => o.SortIndex).ToList();

                    foreach(var question in questions)
                    {
                        if (question.SortIndex == sortIndex)
                            sort = true;

                        if (sort)
                        {
                            if (questions.Count() == 1)
                                question.SortIndex = sortIndex - 1;
                            else
                                question.SortIndex = sortIndex + index;

                            db.SaveChanges();
                        }

                        index += 1;
                    }

                    var allQuestions = db.Questions.Where(q => q.SurveyId == surveyId).OrderBy(o => o.SortIndex).ToList();
                    index = 1;

                    foreach(var question in allQuestions)
                    {
                        question.SortIndex = index;
                        db.SaveChanges();

                        index += 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex);
            }
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
    }
}