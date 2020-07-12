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

                        var questions = db.Questions.Where(q => q.SurveyId == id).ToList();
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
                    question.Title = title;
                    question.Points = points;
                    question.SortIndex = sortIndex;
                    
                    db.SaveChanges();
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
    }
}