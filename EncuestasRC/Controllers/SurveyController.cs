﻿using EncuestasRC.App_Start;
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
            public decimal QuestionAverage { get; set; }
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
            //try
            //{
            //    using (var db = new EncuestaRCEntities())
            //    {
            //        var orders = db.SurveyHeaders.ToList();

            //        foreach (var item in orders)
            //        {
            //            DataSet customerNameData = Helper.GetCustomerName(item.OrderNo);

            //            if (customerNameData != null && customerNameData.Tables.Count > 0 && customerNameData.Tables[0].Rows.Count > 0)
            //            {
            //                DateTime? deliveryDate = null;
            //                if (customerNameData.Tables[0].Rows[0].ItemArray[6].ToString().Length >= 8)
            //                {
            //                    int year = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[6].ToString().Substring(0, 4));
            //                    int month = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[6].ToString().Substring(4, 2));
            //                    int day = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[6].ToString().Substring(6, 2));

            //                    deliveryDate = new DateTime(year, month, day);
            //                }

            //                DateTime? closeDate = null;
            //                if (customerNameData.Tables[0].Rows[0].ItemArray[5].ToString().Length >= 8)
            //                {
            //                    int year = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[5].ToString().Substring(0, 4));
            //                    int month = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[5].ToString().Substring(4, 2));
            //                    int day = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[5].ToString().Substring(6, 2));

            //                    closeDate = new DateTime(year, month, day);
            //                }

            //                item.DeliveryDate = deliveryDate;
            //                item.CloseDate = closeDate;
            //                db.SaveChanges();
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Helper.SendException(ex, "order:");
            //}

            if (Session["role"] == null) return RedirectToAction("Index", "Home");
            //if (Session["role"].ToString() != "Admin") return RedirectToAction("Index", "Home");

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
                        decimal result = GetSurveyResult(survey.Id);

                        SurveyViewModel surveyViewModel = new SurveyViewModel
                        {
                            Id = survey.Id,
                            Active = survey.Active,
                            CreateBy = survey.CreateBy,
                            CreateDate = survey.CreateDate,
                            Title = survey.Title,
                            Completed = completed,
                            Result = result
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

        public ActionResult CompletedSurveys(int? id, DateTime? startDate = null, DateTime? endDate = null, int filterBy = 1)
        {
            try
            {
                if (Session["role"] == null || id == null) return RedirectToAction("Index", "Home");

                List<SurveyListViewModel> surveyList = new List<SurveyListViewModel>();

                using (var db = new EncuestaRCEntities())
                {
                    startDate = startDate == null ? DateTime.Today.AddDays(-30) : startDate;
                    endDate = endDate == null ? DateTime.Today : endDate;
    
                    var surveysHeader = db.SurveyHeaders.Where(s => s.SurveyId == id).ToList();

                    if (filterBy == 1) //By CreationDate
                        surveysHeader = surveysHeader.Where(s => s.SurveyEnded >= startDate && s.SurveyEnded <= endDate).OrderByDescending(o => o.SurveyEnded).ToList();

                    if (filterBy == 2) //By DeliveryDate
                        surveysHeader = surveysHeader.Where(s => s.DeliveryDate >= startDate && s.DeliveryDate <= endDate).OrderByDescending(o => o.DeliveryDate).ToList();

                    if (filterBy == 3) //By CloseDate
                        surveysHeader = surveysHeader.Where(s => s.CloseDate >= startDate && s.CloseDate <= endDate).OrderByDescending(o => o.CloseDate).ToList();

                    ViewBag.SurveyTitle = db.Surveys.FirstOrDefault(s => s.Id == id).Title;
                    ViewBag.SurveyId = id;
                    ViewBag.FilterBy = filterBy;

                    foreach(var item in surveysHeader)
                    {
                        surveyList.Add(new SurveyListViewModel
                        {
                            Id = item.Id,
                            SurveyEnded = item.SurveyEnded,
                            Customer = item.Customer,
                            CustomerType = item.CustomerType,
                            OrderNo = item.OrderNo,
                            Result = GetSurveyResultByOne(id.Value, item.Id),
                            DeliveryDate = item.DeliveryDate,
                            CloseDate = item.CloseDate
                        });
                    }

                    ViewBag.startDate = startDate == null ? DateTime.Today.ToString("dd/MM/yyyy") : startDate.Value.ToString("dd/MM/yyyy");
                    ViewBag.endDate = endDate == null ? DateTime.Today.ToString("dd/MM/yyyy") : endDate.Value.ToString("dd/MM/yyyy");

                    return View(surveyList);
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
            if (!Session["role"].ToString().Contains("Admin")) return RedirectToAction("Index", "Home");

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

        public ActionResult Completar_Encuesta(int? id)
        {
            try
            {
                if (Session["role"] == null || id == null) return RedirectToAction("Index", "Home");

                using (var db = new EncuestaRCEntities())
                {
                    Survey survey = db.Surveys.FirstOrDefault(s => s.Id == id);
                    if (survey != null)
                    {
                        if (!survey.Active) return RedirectToAction("Index", "Survey");

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

        public ActionResult Encuesta_Completada(int? id, decimal Resultado)
        {
            try
            {
                if (Session["role"] == null || id == null) return RedirectToAction("Index", "Home");

                var surveyCompletedViewModel = new SurveyCompletedViewModel();

                using (var db = new EncuestaRCEntities())
                {
                    var surveyHeader = db.SurveyHeaders.FirstOrDefault(h => h.Id == id);
                    var surveyDetail = db.SurveyDetails.Where(d => d.SurveyHeaderId == id).ToList();

                    if (surveyHeader != null)
                    {
                        var survey = db.Surveys.FirstOrDefault(s => s.Id == surveyHeader.SurveyId);

                        surveyCompletedViewModel.Title = survey.Title;
                        surveyCompletedViewModel.surveyHeader = surveyHeader;
                        surveyCompletedViewModel.surveyDetail = surveyDetail;

                        var questions = QuestionsWithAnswers(surveyHeader.SurveyId);
                        ViewBag.questions = questions;

                        ViewBag.result = Resultado;

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

        public ActionResult Reporte_Encuesta(int? id)
        {
            try
            {
                if (Session["role"] == null || id == null) return RedirectToAction("Index", "Home");

                List<QuestionAnswers> _questionsResult = new List<QuestionAnswers>();

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

                        foreach (var question in _questions)
                        {
                            var answerMax = db.Answers.Where(a => a.QuestionId == question.QuestionId).OrderByDescending(o => o.Points).FirstOrDefault().Points;
                            //var answerMaxPrevios = db.Answers.Where(a => a.QuestionId == question.QuestionId && a.Id != _answerMax.Id).OrderByDescending(o => o.Points).FirstOrDefault().Points;

                            decimal answerSum = 0;
                            decimal answerCount = 0;

                            foreach (var answer in question.Answers)
                            {
                                var _answer = details.Where(a => a.AnswerId == answer.Id);
                                int answerTotal = _answer.Count();

                                answerCount += answerTotal;
                                answerSum += _answer.Sum(a => a.Points);

                                _answers.Add(new AnswersPoints() { AnswerId = answer.Id, AnswerPoints = answerTotal });
                            }

                            question.QuestionAverage = ((answerSum / answerCount) / answerMax) * 100;
                            question.QuestionAverage = Math.Round((question.QuestionAverage * question.QuestionPoints) / 100);

                            _questionsResult.Add(question);
                        }

                        ViewBag.Survey = survey;
                        ViewBag.Questions = _questionsResult;
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

        [HttpPost]
        public JsonResult CustomerNameForOrder(string order)
        {
            DataSet customerNameData = Helper.GetCustomerName(order);

            try
            {
                if (customerNameData.Tables.Count > 0 && customerNameData.Tables[0].Rows.Count > 0)
                {
                    DateTime? closeDate = null;
                    if (customerNameData.Tables[0].Rows[0].ItemArray[3].ToString().Length >= 8)
                    {
                        int year = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[3].ToString().Substring(0, 4));
                        int month = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[3].ToString().Substring(4, 2));
                        int day = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[3].ToString().Substring(6, 2));

                        closeDate = new DateTime(year, month, day);
                    }

                    DateTime? deliveryDate = null;
                    if (customerNameData.Tables[0].Rows[0].ItemArray[4].ToString().Length >= 8)
                    {
                        int year = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[4].ToString().Substring(0, 4));
                        int month = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[4].ToString().Substring(4, 2));
                        int day = int.Parse(customerNameData.Tables[0].Rows[0].ItemArray[4].ToString().Substring(6, 2));

                        deliveryDate = new DateTime(year, month, day);
                    }

                    return Json(new { result = "200", 
                        message = $"{customerNameData.Tables[0].Rows[0].ItemArray[0]}-" +
                                  $"{customerNameData.Tables[0].Rows[0].ItemArray[1]}-" +
                                  $"{customerNameData.Tables[0].Rows[0].ItemArray[2]}-" +
                                  $"{closeDate?.ToString("dd/MM/yyyy")}-" +
                                  $"{deliveryDate?.ToString("dd/MM/yyyy")}"
                    });
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
        public JsonResult RemoveCompletedSurvey(int id)
        {
            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    var surveyHeader = db.SurveyHeaders.FirstOrDefault(h => h.Id == id);
                    var surveyDetail = db.SurveyDetails.Where(d => d.SurveyHeaderId == id).ToList();

                    if (surveyHeader != null && surveyDetail.Count() > 0)
                    {
                        foreach (var item in surveyDetail)
                        {
                            db.SurveyDetails.Remove(item);
                            db.SaveChanges();
                        }

                        db.SurveyHeaders.Remove(surveyHeader);
                        db.SaveChanges();

                        return Json(new
                        {
                            result = "200",
                            message = "La encuesta fue eliminada"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "id:" + id);

                return Json(new { result = "500", message = ex.Message });
            }

            return Json(new { result = "404", message = "No encontrada" });
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

        //Save Survey Header
        [HttpPost]
        public JsonResult SaveSurveyHeader(int Id, int surveyId, string customer, int customerType, string orderNo, string date, string comments)
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
                        //surveyHeader.SurveyEnded = DateTime.Now;
                        surveyHeader.Comments = comments;
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
                            UserLogged = Session["email"] != null ? Session["email"].ToString() : "",
                            Comments = comments
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
                    var surveyDetail = db.SurveyDetails.FirstOrDefault(a => a.SurveyHeaderId == surveyHeaderId && a.QuestionId == questionId);
                    if (surveyDetail != null)
                    {
                        surveyDetail.AnswerId = answerId;
                        surveyDetail.Points = points;
                    }
                    else
                    {
                        db.SurveyDetails.Add(new SurveyDetail
                        {
                            SurveyHeaderId = surveyHeaderId,
                            QuestionId = questionId,
                            AnswerId = answerId,
                            Points = points,
                            CreationDate = DateTime.Now
                        });
                    }
                    
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

        [HttpPost]
        public JsonResult SurveyReportDetailed(int surveyId, DateTime startDate, DateTime endDate, int filterBy)
        {
            string detailed = "<table width='100%' border=1>";
            string surveyHeader = "";

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    var details = (from h in db.SurveyHeaders
                                   join d in db.SurveyDetails on h.Id equals d.SurveyHeaderId
                                   join q in db.Questions on d.QuestionId equals q.Id
                                   join a in db.Answers on d.AnswerId equals a.Id
                                   where h.SurveyId == surveyId
                                   orderby d.SurveyHeaderId, q.SortIndex
                                   select new
                                   {
                                       h.Id,
                                       h.SurveyId,
                                       h.Customer,
                                       h.CustomerType,
                                       h.OrderNo,
                                       d.Points,
                                       d.QuestionId,
                                       questionTitle = q.Title,
                                       d.AnswerId,
                                       answerTitle = a.Title,
                                       date = h.SurveyEnded,
                                       h.DeliveryDate,
                                       h.CloseDate
                                   }).ToArray();

                    if (filterBy == 1) //By CreationDate
                        details = details.Where(d => d.date.Value.Date >= startDate && d.date.Value.Date <= endDate).ToArray();

                    if (filterBy == 2) //By DeliveryDate
                        details = details.Where(d => d.DeliveryDate >= startDate && d.DeliveryDate <= endDate).ToArray();
                        
                    if (filterBy == 3) //By CloseDate
                        details = details.Where(d => d.CloseDate >= startDate && d.CloseDate <= endDate).ToArray();
                    
                    var questions = db.Questions.Where(q => q.SurveyId == surveyId).OrderBy(o => o.SortIndex).ToArray();
                    
                    //HEADER for table
                    detailed += "<tr>";
                    detailed += "<td><b>Nombre del Cliente</b></td>";
                    detailed += "<td><b>Tipo de Cliente</b></td>";
                    detailed += "<td><b>Orden No.</b></td>";
                    detailed += "<td><b>Fecha de Creación</b></td>";
                    detailed += "<td><b>Fecha de Cierre</b></td>";
                    detailed += "<td><b>Fecha de Entrega</b></td>";
                    //detailed += "<td><b>Hora</b></td>";
                    detailed += "<td><b>Resultado %</b></td>";

                    foreach (var question in questions)
                    {
                        detailed += "<td><b>" + question.Title + "</b></td>";
                        var answers = db.Answers.Where(a => a.QuestionId == question.Id);
                        foreach (var answer in answers)
                        {
                            detailed += "<td style='text-align:center'><b>" + answer.Title + "</b></td>";
                        }
                    }
                    detailed += "</tr>";

                    //BODY for table
                    int index = 0;

                    foreach (var detail in details)
                    {
                        if (index == 0) detailed += "<tr>";

                        foreach (var question in questions.Where(q => q.Id == detail.QuestionId))
                        {
                            surveyHeader = $"surveyHeaderId: {detail.Id} | questionID: {detail.QuestionId}";

                            if (index == 0)
                            {

                                detailed += "<td>" + detail.Customer + "</td>";
                                detailed += "<td>" + GetCustomerType(detail.CustomerType) + "</td>";
                                detailed += "<td>" + detail.OrderNo + "</td>";
                                detailed += "<td>" + detail.date.Value.ToString("dd/MM/yyyy") + "</td>";
                                detailed += "<td>" + detail.CloseDate?.ToString("dd/MM/yyyy") + "</td>";
                                detailed += "<td>" + detail.DeliveryDate?.ToString("dd/MM/yyyy") + "</td>";
                                //detailed += "<td>" + detail.date.Value.ToString("hh:mm t") + "</td>";
                                detailed += "<td>" + GetSurveyResultByOne(detail.SurveyId, detail.Id) + "</td>";
                            }
                            
                            detailed += "<td>" + question.Title + "</td>";

                            var answers = db.Answers.Where(a => a.QuestionId == question.Id);
                            foreach (var answer in answers)
                            {
                                //var selectedAnswer = answer.Id == detail.AnswerId && question.Id == detail.QuestionId ? "<span>X<span>" : "";
                                var selectedAnswer = answer.Id == detail.AnswerId ? "<span>X<span>" : "";
                                detailed += "<td style='text-align:center'>" + selectedAnswer + "</td>";
                            }
                        }

                        if (index == questions.Count() - 1)
                        {
                            index = 0;
                            detailed += "</tr>";
                        }
                        else
                            index++;
                    }

                    detailed += "</table>";
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "Encuesta Id:" + surveyId + " detail:" + surveyHeader);

                return Json(new { result = "500", message = ex.Message });
            }

            return new JsonResult() { Data = detailed, JsonRequestBehavior = JsonRequestBehavior.AllowGet, MaxJsonLength = Int32.MaxValue };

            //return Json(new { result = "200", message = detailed });
        }

        private decimal GetSurveyResult(int surveyId)
        {
            decimal result = 0;
            string detail = "";

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    var details = (from h in db.SurveyHeaders
                                   join d in db.SurveyDetails on h.Id equals d.SurveyHeaderId
                                   where h.SurveyId == surveyId
                                   select d).ToList();


                    List<QuestionAnswers> _questions = QuestionsWithAnswers(surveyId);
                    List<AnswersPoints> _answers = new List<AnswersPoints>();

                    foreach (var question in _questions)
                    {
                        var answerMax = db.Answers.Where(a => a.QuestionId == question.QuestionId).OrderByDescending(o => o.Points).FirstOrDefault().Points;

                        decimal answerSum = 0;
                        decimal answerCount = 0;

                        foreach (var answer in question.Answers)
                        {
                            detail = $"surveyID: {surveyId} | questionID: {question.QuestionId} | answer: {answer.Id}";

                            var _answer = details.Where(a => a.AnswerId == answer.Id);
                            int answerTotal = _answer.Count();

                            answerCount += answerTotal;
                            answerSum += _answer.Sum(a => a.Points);
                        }

                        question.QuestionAverage = ((answerSum / answerCount) / answerMax) * 100;
                        question.QuestionAverage = Math.Round((question.QuestionAverage * question.QuestionPoints) / 100);

                        result += question.QuestionAverage;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "More detail:" + detail);

                return 0;
            }

            return result;
        }

        public decimal GetSurveyResultByOne(int surveyId, int surveyIdHeader)
        {
            decimal result = 0;
            string detail = "";

            try
            {
                using (var db = new EncuestaRCEntities())
                {
                    var details = (from h in db.SurveyHeaders
                                   join d in db.SurveyDetails on h.Id equals d.SurveyHeaderId
                                   where h.Id == surveyIdHeader
                                   select d).ToList();


                    List<QuestionAnswers> _questions = QuestionsWithAnswers(surveyId);
                    List<AnswersPoints> _answers = new List<AnswersPoints>();

                    foreach (var question in _questions)
                    {
                        var answerMax = db.Answers.Where(a => a.QuestionId == question.QuestionId).OrderByDescending(o => o.Points).FirstOrDefault().Points;

                        decimal answerSum = 0;
                        decimal answerCount = 0;

                        foreach (var answer in question.Answers)
                        {
                            detail = $"surveyID: {surveyId} | surveyHeaderID: {surveyIdHeader} | questionID: {question.QuestionId} | answer: {answer.Id}";

                            var _answer = details.Where(a => a.AnswerId == answer.Id);
                            int answerTotal = _answer.Count();

                            answerCount += answerTotal;
                            answerSum += _answer.Sum(a => a.Points);
                        }

                        question.QuestionAverage = ((answerSum / answerCount) / answerMax) * 100;
                        question.QuestionAverage = Math.Round((question.QuestionAverage * question.QuestionPoints) / 100);

                        result += question.QuestionAverage;
                    }
                }
            }
            catch (Exception ex)
            {
                Helper.SendException(ex, "More detail:" + detail);

                return 0;
            }

            return result;
        }

        private string GetCustomerType(int? id)
        {
            //if (id == null || id == 0) return "";
            if (id == 1) return "Minorista";
            if (id == 2) return "Mayorista";
            if (id == 3) return "Clave";
            if (id == 4) return "Esporádico";
            if (id == 5) return "Centro de Servicios";

            return "";
        }

    }
}