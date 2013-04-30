using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Demo.Models;
using Demo.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    public class ScoreboardViewModel
    {
        [Display(Name="Scoreboard")]
        public List<ScoreboardRecord> Scoreboard { get; set; }

        [Display(Name = "Point Types")]
        public ScoreboardPointType ScoreboardPointTypes { get; set; }
    }

    public class ScoreboardRecord
    {
        [Display(Name="User")]
        public UserProfile User { get; set; }

        [Display(Name="Score")]
        public int Score { get; set; }
    }

    public class ScoreboardPointType
    {
        [Display(Name="Point Types")]
        public List<PointType> PointTypes { get; set; }

        [Display(Name="Selected Point Type")]
        public string SelectedPointType { get; set; }
    }
}

namespace Demo.Controllers
{
    /// <summary>
    /// Handles creates pages for the score board.
    /// </summary>
    public class ScoreboardController : Controller
    {
        private IRepository<UserProfile> _userRepo;
        private IRepository<PointScore> _pointScoreRepo;
        private IRepository<PointType> _pointTypeRepo;

        public ScoreboardController()
        {
            _userRepo = new BasicRepo<UserProfile>();
            _pointScoreRepo = new BasicRepo<PointScore>();
            _pointTypeRepo = new BasicRepo<PointType>();
        }

        //
        // GET: /Scoreboard/{pointType}
        public ActionResult Index(string pointType)
        {
            //List<UserProfile> users = _userRepo.GetAll().ToList();

            //users.Sort(CompareByScore);

            bool pointTypeExists = false;

            ScoreboardViewModel svm = new ScoreboardViewModel()
            {
                Scoreboard = new List<ScoreboardRecord>(),
                ScoreboardPointTypes = new ScoreboardPointType()
                {
                    PointTypes = _pointTypeRepo.GetAll().ToList(),
                    SelectedPointType = pointType
                }
            };

            foreach (PointType pt in svm.ScoreboardPointTypes.PointTypes)
            {
                pointTypeExists = (pt.Name == pointType);

                if (pointTypeExists) break;
            }

            foreach (var user in _userRepo.GetAll())
            {
                svm.Scoreboard.Add(new ScoreboardRecord()
                {
                    User = user,
                    Score = pointTypeExists ? user.ScoreFor(pointType) : user.TotalScore
                });
            }

            svm.Scoreboard.Sort(CompareByScore);

            return View(svm);
        }

        //
        // GET: /Scoreboard/Details/{id}
        public ActionResult Details(int id = 0)
        {
            //UserProfile selectedUser = _userRepo.GetAll().Single(su=>su.ID == id);
            return View(_userRepo.Get(id));
            //return RedirectToAction("Index");
        }

        private static int CompareByScore(ScoreboardRecord x, ScoreboardRecord y)
        {
            return x.Score.CompareTo(y.Score) * -1;
        }
    }
}
