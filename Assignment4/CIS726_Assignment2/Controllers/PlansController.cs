﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CIS726_Assignment2.Models;
using CIS726_Assignment2.ViewModels;
using CIS726_Assignment2.Repositories;
using CIS726_Assignment2.SystemBus;
using PagedList;
using System.Net;

namespace CIS726_Assignment2.Controllers
{
    public class PlansController : Controller
    {
        //private IGenericRepository<Plan> plans;
        //private IGenericRepository<PlanCourse> planCourses;
        //private IGenericRepository<Semester> semesters;
        private IGenericRepository<User> users;
        //private IGenericRepository<DegreeProgram> degreePrograms;

        private IRoles roles;
        private IWebSecurity webSecurity;

        private IMessageQueueProducer<Plan> _planProducer;
        private IMessageQueueProducer<PlanCourse> _planCourseProducer;
        private IMessageQueueProducer<Semester> _semesterProducer;
        private IMessageQueueProducer<DegreeProgram> _degreeProgramProducer;

        public PlansController()
        {
            CourseDBContext context = new CourseDBContext();
            //plans = new GenericRepository<Plan>(new StorageContext<Plan>(context));
            //planCourses = new GenericRepository<PlanCourse>(new StorageContext<PlanCourse>(context));
            //semesters = new GenericRepository<Semester>(new StorageContext<Semester>(context));
            users = new GenericRepository<User>(new StorageContext<User>(context));
            //degreePrograms = new GenericRepository<DegreeProgram>(new StorageContext<DegreeProgram>(context));
            roles = new RolesImpl();
            webSecurity = new WebSecurityImpl();

            _planProducer = new BasicMessageQueueProducer<Plan>();
            _planCourseProducer = new BasicMessageQueueProducer<PlanCourse>();
            _semesterProducer = new BasicMessageQueueProducer<Semester>();
            _degreeProgramProducer = new BasicMessageQueueProducer<DegreeProgram>();
        }


        public PlansController(IGenericRepository<Plan> fakePlan, IGenericRepository<PlanCourse> fakePlanCourse, IGenericRepository<Semester> fakeSem, IGenericRepository<User> fakeUser, IGenericRepository<DegreeProgram> fakeDegree, IRoles fakeRoles, IWebSecurity fakeWebSecurity)
        {
            //plans = fakePlan;
            //planCourses = fakePlanCourse;
            //semesters = fakeSem;
            users = fakeUser;
            //degreePrograms = fakeDegree;
            roles = fakeRoles;
            webSecurity = fakeWebSecurity;
        }
        //
        // GET: /Plans/
        [Authorize]
        public ActionResult Index(string sortOrder, int? page)
        {
            int pageSize = 100;
            int pageNumber = (page ?? 1);

            String currentSort = "";

            bool titleAsc = false;
            /*
            var plansListA = from s in plans.GetAll() select s;
            var plansList = plansListA
                .Include(pl => pl.degreeProgram)
                .Include(pl => pl.degreeProgram.requiredCourses.Select(s => s.course.prerequisites))
                .Include(pl => pl.degreeProgram.requiredCourses.Select(s => s.course.prerequisiteFor))
                .Include(pl => pl.degreeProgram.electiveCourses.Select(s => s.electiveList))
                .Include(pl => pl.user)
                .Include(pl => pl.semester)
                .Include(pl => pl.planCourses.Select(s => s.plan))
                .Include(pl=>pl.planCourses.Select(s => s.course))
                .Include(pl=>pl.planCourses.Select(s=>s.electiveList))
                .Include(pl=>;
            */

            var plansList = _planProducer.GetAll().AsQueryable();

            if (!webSecurity.CurrentUser.IsInRole("Advisor"))
            {
                int id = webSecurity.CurrentUserId;
                plansList = plansList.Where(s => s.userID == id);
            }

            if (sortOrder == null)
            {
                sortOrder = "title_asc";
            }

            String[] sorts = sortOrder.Split(';');

            int lastTitle = -1;

            for (int i = 0; i < sorts.Length; i++)
            {
                if (sorts[i].StartsWith("title"))
                {
                    if (lastTitle > 0)
                    {
                        sorts[lastTitle] = "";
                    }
                    else
                    {
                        lastTitle = i;
                    }
                }
            }

            foreach (string s in sorts)
            {
                if (s.Length <= 0)
                {
                    continue;
                }
                currentSort = currentSort + s + ";";
                if (s.Equals("title_asc"))
                {
                    plansList = plansList.OrderBy(x => x.user.username);
                    titleAsc = true;
                }
                if (s.Equals("title_desc"))
                {
                    plansList = plansList.OrderByDescending(x => x.user.username);
                    titleAsc = false;
                }
            }

            ViewBag.titleAsc = titleAsc;
            ViewBag.currentSort = currentSort;

            return View(plansList.ToPagedList(pageNumber, pageSize));
        }

        //
        // GET: /Plans/Details/5
        [Authorize]
        public ActionResult Details(int id = 0)
        {
            //Plan plan = plans.Find(id);

            Plan plan = _planProducer.Get(new Plan() { ID = id });

            if (plan == null)
            {
                return HttpNotFound();
            }
            if (webSecurity.CurrentUser.IsInRole("Advisor") || plan.userID == webSecurity.CurrentUserId)
            {  
                //var semesterList = semesters.Where(s => s.ID > plan.semesterID);
                var semesterList = _semesterProducer.GetAll().Where(s => s.ID > plan.semesterID);
                ViewBag.semesterID = new SelectList(semesterList.AsEnumerable(), "ID", "semesterName");
                return View(plan);
            }
            else
            {
                return HttpNotFound();
            }
        }

        //
        // GET: /Plans/Create
        [Authorize]
        public ActionResult Create()
        {
            //ViewBag.degreeProgramID = new SelectList(degreePrograms.GetAll().AsEnumerable(), "ID", "degreeProgramName");
            ViewBag.degreeProgramID = new SelectList(_degreeProgramProducer.GetAll().AsEnumerable(), "ID", "degreeProgramName");
            if (webSecurity.CurrentUser.IsInRole("Advisor"))
            {
                ViewBag.userID = new SelectList(users.GetAll().AsEnumerable(), "ID", "username");
                ViewBag.Advisor = true;
            }
            else
            {
                ViewBag.userID = webSecurity.CurrentUserId;
                ViewBag.Advisor = false;
            }
            //ViewBag.semesterID = new SelectList(semesters.Where(i => i.standard == true).AsEnumerable(), "ID", "semesterName");
            ViewBag.semesterID = new SelectList(_semesterProducer.GetAll().Where(i => i.standard == true).AsEnumerable(), "ID", "semesterName");
            return View();
        }

        //
        // POST: /Plans/Create

        [HttpPost]
        [Authorize]
        public ActionResult Create(Plan plan)
        {
            if (ModelState.IsValid)
            {
                //plans.Add(plan);
                //plans.SaveChanges();
                Plan newPlan = _planProducer.Create(plan).First();

                //Plan newPlan = plans.Find(plan.ID);
                //Plan newPlan = _planProducer.Get(new Plan() { ID = id });
                newPlan = _planProducer.Get(newPlan);
                //newPlan.degreeProgram = degreePrograms.Find(newPlan.degreeProgramID);
                ChangeDegreeProgram(newPlan);
                return RedirectToAction("Index");
            }
            ViewBag.degreeProgramID = new SelectList(_degreeProgramProducer.GetAll().AsEnumerable(), "ID", "degreeProgramName", plan.degreeProgramID);
            //ViewBag.degreeProgramID = new SelectList(degreePrograms.GetAll().AsEnumerable(), "ID", "degreeProgramName", plan.degreeProgramID);
            if (webSecurity.CurrentUser.IsInRole("Advisor"))
            {
                ViewBag.userID = new SelectList(users.GetAll().AsEnumerable(), "ID", "username");
                ViewBag.Advisor = true;
            }
            else
            {
                ViewBag.userID = webSecurity.CurrentUserId;
                ViewBag.Advisor = false;
            }
            var semesterList = _semesterProducer.GetAll().Where(i => i.standard == true);
            ViewBag.semesterID = new SelectList(semesterList.AsEnumerable(), "ID", "semesterName");
            return View(plan);
        }

        //
        // GET: /Plans/Edit/5
        [Authorize]
        public ActionResult Edit(int id = 0)
        {
            //Plan plan = plans.Find(id);
            Plan plan = _planProducer.Get(new Plan() { ID = id });
            if (plan == null)
            {
                return HttpNotFound();
            }
            if (webSecurity.CurrentUser.IsInRole("Advisor") || plan.userID == webSecurity.CurrentUserId)
            {
                //ViewBag.degreeProgramID = new SelectList(degreePrograms.GetAll().AsEnumerable(), "ID", "degreeProgramName", plan.degreeProgramID);
                ViewBag.degreeProgramID = new SelectList(_degreeProgramProducer.GetAll().AsEnumerable(), "ID", "degreeProgramName", plan.degreeProgramID);
                if (webSecurity.CurrentUser.IsInRole("Advisor"))
                {
                    ViewBag.userID = new SelectList(users.GetAll().AsEnumerable(), "ID", "username", plan.userID);
                    ViewBag.Advisor = true;
                }
                else
                {
                    ViewBag.userID = webSecurity.CurrentUserId;
                    ViewBag.Advisor = false;
                }
                //ViewBag.semesterID = new SelectList(semesters.Where(i => i.standard == true).AsEnumerable(), "ID", "semesterName", plan.semesterID);
                ViewBag.semesterID = new SelectList(_semesterProducer.GetAll().Where(i => i.standard == true).AsEnumerable(), "ID", "semesterName", plan.semesterID);
                return View(plan);
            }
            else
            {
                return HttpNotFound();
            }
        }

        //
        // POST: /Plans/Edit/5

        [HttpPost]
        [Authorize]
        public ActionResult Edit(Plan plan)
        {
            if (ModelState.IsValid)
            {
                //Plan planAttached = plans.Find(plan.ID);
                Plan planAttached = _planProducer.Get(plan);

                plan.userID = planAttached.userID;
                if (webSecurity.CurrentUser.IsInRole("Advisor") || plan.userID == webSecurity.CurrentUserId)
                {
                    _planProducer.Update(plan);
                    //plans.UpdateValues(planAttached, plan);
                    //plans.SaveChanges();
                    //Plan newPlan = plans.Find(plan.ID);
                    Plan newPlan = _planProducer.Get(plan);
                    //newPlan.degreeProgram = degreePrograms.Find(newPlan.degreeProgramID);
                    ChangeDegreeProgram(newPlan);
                    return RedirectToAction("Index");
                }
            }
            //ViewBag.degreeProgramID = new SelectList(degreePrograms.GetAll().AsEnumerable(), "ID", "degreeProgramName", plan.degreeProgramID);
            ViewBag.degreeProgramID = new SelectList(_degreeProgramProducer.GetAll().AsEnumerable(), "ID", "degreeProgramName", plan.degreeProgramID);
            if (webSecurity.CurrentUser.IsInRole("Advisor"))
            {
                ViewBag.userID = new SelectList(users.GetAll().AsEnumerable(), "ID", "username", plan.userID);
                ViewBag.Advisor = true;
            }
            else
            {
                ViewBag.userID = webSecurity.CurrentUserId;
                ViewBag.Advisor = false;
            }
            //ViewBag.semesterID = new SelectList(semesters.Where(i => i.standard == true).AsEnumerable(), "ID", "semesterName", plan.semesterID);
            ViewBag.semesterID = new SelectList(_semesterProducer.GetAll().Where(i => i.standard == true).AsEnumerable(), "ID", "semesterName", plan.semesterID);
            return View(plan);
        }

        private void ChangeDegreeProgram(Plan plan)
        {
            //List<PlanCourse> plans = planCourses.Where(i => i.planID == plan.ID.ToList();
            List<PlanCourse> plans = _planCourseProducer.GetAll().Where(pc => pc.planID == plan.ID).ToList();

            foreach (PlanCourse planCourse in plans)
            {
                _planCourseProducer.Remove(planCourse);
                //planCourses.Remove(planCourse);
                //planCourses.SaveChanges();
            }
            Dictionary<int, int> semesterOrders = new Dictionary<int, int>();
            Dictionary<int, int> semesterMap = new Dictionary<int, int>();
            int nowSem = 1;
            //List<Semester> semesterList = semesters.Where(i => i.ID >= plan.semesterID).ToList();
            List<Semester> semesterList = _semesterProducer.GetAll().Where(s=>s.ID >= plan.semesterID).ToList();
            foreach (Semester sem in semesterList)
            {
                if (sem.standard == true)
                {
                    semesterMap.Add(nowSem, sem.ID);
                    semesterOrders.Add(nowSem, 0);
                    nowSem++;
                }
            }

            List<RequiredCourse> requirements = plan.degreeProgram.requiredCourses.ToList();
            foreach (RequiredCourse req in requirements)
            {
                PlanCourse pcourse = new PlanCourse();
                pcourse.planID = plan.ID;
                int order = semesterOrders[req.semester];
                pcourse.order = order;
                semesterOrders[req.semester] = order + 1;
                pcourse.semesterID = semesterMap[req.semester];
                pcourse.courseID = req.courseID;
                pcourse.credits = req.course.courseHours;
                _planCourseProducer.Create(pcourse);
                //planCourses.Add(pcourse);
                //planCourses.SaveChanges();
            }

            List<ElectiveCourse> elects = plan.degreeProgram.electiveCourses.ToList();
            foreach (ElectiveCourse elect in elects)
            {
                PlanCourse pcourse = new PlanCourse();
                pcourse.planID = plan.ID;
                int order = semesterOrders[elect.semester];
                pcourse.order = order;
                semesterOrders[elect.semester] = order + 1;
                pcourse.semesterID = semesterMap[elect.semester];
                pcourse.electiveListID = elect.electiveListID;
                pcourse.credits = elect.credits.ToString();
                _planCourseProducer.Create(pcourse);
                //planCourses.Add(pcourse);
                //planCourses.SaveChanges();
            }
        }

        //
        // GET: /Plans/Delete/5
        [Authorize]
        public ActionResult Delete(int id = 0)
        {
            //Plan plan = plans.Find(id);
            Plan plan = _planProducer.Get(new Plan() { ID = id });

            if (plan == null)
            {
                return HttpNotFound();
            }
            if (webSecurity.CurrentUser.IsInRole("Advisor") || plan.userID == webSecurity.CurrentUserId)
            {
                return View(plan);
            }
            else
            {
                return HttpNotFound();
            }
        }

        //
        // POST: /Plans/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            //Plan plan = plans.Find(id);
            Plan plan = _planProducer.Get(new Plan() { ID = id });

            if (webSecurity.CurrentUser.IsInRole("Advisor") || plan.userID == webSecurity.CurrentUserId)
            {
                //plans.Remove(plan);
                //plans.SaveChanges();
                _planProducer.Remove(plan);
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        public ActionResult UpdateCourseInfo(int id = 0)
        {
            if (id > 0)
            {
                //PlanCourse pcourse = planCourses.Find(id);
                PlanCourse pcourse = _planCourseProducer.Get(new PlanCourse() { ID = id });
                if (pcourse != null)
                {
                    //Plan planAttached = plans.Find(pcourse.planID);
                    Plan planAttached = pcourse.plan;
                    if (webSecurity.CurrentUser.IsInRole("Advisor") || planAttached.userID == webSecurity.CurrentUserId)
                    {
                        if (pcourse.electiveListID != null)
                        {
                            List<DropdownCourse> courses = new List<DropdownCourse>();
                            List<ElectiveListCourse> elistCourses = pcourse.electiveList.courses.ToList();
                            foreach (ElectiveListCourse elistc in elistCourses)
                            {
                                DropdownCourse now = new DropdownCourse();
                                now.ID = elistc.courseID;
                                now.courseHeader = elistc.course.courseHeader;
                                courses.Add(now);
                            }
                            if (pcourse.courseID != null)
                            {
                                ViewBag.courseID = new SelectList(courses.AsEnumerable(), "ID", "courseHeader", pcourse.courseID);
                            }
                            else
                            {
                                ViewBag.courseID = new SelectList(courses.AsEnumerable(), "ID", "courseHeader");
                            }
                        }
                        return PartialView("PlanCourseFormPartial", new PlanCourseEdit(pcourse));
                    }
                }
            }
            return HttpNotFound();
        }

        [HttpPost]
        [Authorize]
        public ActionResult MoveCourse(int ID, int semester, int order)
        {
            //PlanCourse pcourseAttached = planCourses.Find(ID);
            PlanCourse pcourseAttached = _planCourseProducer.Get(new PlanCourse { ID = ID });
            //Plan planAttached = plans.Find(pcourseAttached.planID);
            Plan planAttached = pcourseAttached.plan;
            if (webSecurity.CurrentUser.IsInRole("Advisor") || planAttached.userID == webSecurity.CurrentUserId)
            {
                //if (semesters.Find(semester) != null)
                if(_semesterProducer.Get(new Semester(){ID = semester}) != null)
                {
                    int oldSemester = pcourseAttached.semesterID;
                    int oldOrder = pcourseAttached.order;
                    if (semester != oldSemester)
                    {
                        List<PlanCourse> moveUp = planAttached.planCourses.Where(s => s.semesterID == oldSemester).ToList();
                        //List<PlanCourse> moveUp = 
                        foreach (PlanCourse pc in moveUp)
                        {
                            if (pc.order > oldOrder)
                            {
                                pc.order = pc.order - 1;
                                //planCourses.UpdateValues(pc, pc);
                                _planCourseProducer.Update(pc);
                            }
                        }
                        List<PlanCourse> moveDown = planAttached.planCourses.Where(s => s.semesterID == semester).ToList();
                        foreach (PlanCourse pc in moveDown)
                        {
                            if (pc.order >= order)
                            {
                                pc.order = pc.order + 1;
                                //planCourses.UpdateValues(pc, pc);
                                _planCourseProducer.Update(pc);
                            }
                        }
                    }
                    else
                    {
                        if (oldOrder < order)
                        {
                            List<PlanCourse> moveUp = planAttached.planCourses.Where(s => s.semesterID == oldSemester).ToList();
                            foreach (PlanCourse pc in moveUp)
                            {
                                if (pc.order > oldOrder && pc.order <= order)
                                {
                                    pc.order = pc.order - 1;
                                    //planCourses.UpdateValues(pc, pc);
                                    _planCourseProducer.Update(pc);
                                }
                            }
                        }
                        else if (oldOrder > order)
                        {
                            List<PlanCourse> moveDown = planAttached.planCourses.Where(s => s.semesterID == oldSemester).ToList();
                            foreach (PlanCourse pc in moveDown)
                            {
                                if (pc.order >= order && pc.order < order)
                                {
                                    pc.order = pc.order + 1;
                                    //planCourses.UpdateValues(pc, pc);
                                    _planCourseProducer.Update(pc);
                                }
                            }
                        }
                    }
                    pcourseAttached.semesterID = semester;
                    if (order < 7)
                    {
                        pcourseAttached.order = order;
                    }
                    else
                    {
                        pcourseAttached.order = 7;
                    }
                    //planCourses.UpdateValues(pcourseAttached, pcourseAttached);
                    //planCourses.SaveChanges();
                    _planCourseProducer.Update(pcourseAttached);
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        [HttpPost]
        [Authorize]
        public ActionResult SaveCourseInfo(int ID, int? courseID, string notes)
        {
            if (ModelState.IsValid)
            {
                //PlanCourse pcourseAttached = planCourses.Find(ID);
                PlanCourse pcourseAttached = _planCourseProducer.Get(new PlanCourse() { ID = ID });
                //Plan planAttached = plans.Find(pcourseAttached.planID);
                Plan planAttached = pcourseAttached.plan;
                if (webSecurity.CurrentUser.IsInRole("Advisor") || planAttached.userID == webSecurity.CurrentUserId)
                {
                    pcourseAttached.notes = notes;
                    pcourseAttached.courseID = courseID;
                    //planCourses.UpdateValues(pcourseAttached, pcourseAttached);
                    //planCourses.SaveChanges();
                    _planCourseProducer.Update(pcourseAttached);
                    return new HttpStatusCodeResult(HttpStatusCode.OK);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.NotFound);
        }

        public JsonResult GetSemesters(int id)
        {
            //var sems = semesters.Where(i => i.ID >= id).ToList();
            var sems = _semesterProducer.GetAll().Where(i => i.ID >= id).ToList();
            var results = sems.Select(sem => new { sem.ID, sem.semesterName });
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetPlanCourses(int id)
        {
            //Plan plan = plans.Find(id);
            Plan plan = _planProducer.Get(new Plan() { ID = id });
            if (webSecurity.CurrentUser.IsInRole("Advisor") || plan.userID == webSecurity.CurrentUserId){
                List<FlowchartCourse> results = new List<FlowchartCourse>();
                foreach (PlanCourse pcourse in plan.planCourses)
                {
                    FlowchartCourse here = new FlowchartCourse();
                    here.order = pcourse.order;
                    here.semester = pcourse.semesterID;
                    here.hours = pcourse.credits;
                    here.pcourseID = pcourse.ID;
                    if (pcourse.courseID != null)
                    {
                        here.courseID = (int)pcourse.courseID;
                        here.courseTitle = pcourse.course.courseCatalogNumber;
                        here.courseName = pcourse.course.courseTitle;
                        here.prereq = new int[pcourse.course.prerequisites.Count];
                        int place = 0;
                        foreach(PrerequisiteCourse prereq in pcourse.course.prerequisites){
                            here.prereq[place++] = prereq.prerequisiteCourseID;
                        }
                    }
                    if (pcourse.electiveListID != null)
                    {
                        here.elistID = (int)pcourse.electiveListID;
                        here.elistName = pcourse.electiveList.shortName;
                    }
                    results.Add(here);
                }
                return Json(results.ToArray(), JsonRequestBehavior.AllowGet);
            }
            return Json(null);
        }

        protected override void Dispose(bool disposing)
        {
            //plans.Dispose();
            base.Dispose(disposing);
        }
    }
}