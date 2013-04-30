using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Demo.Models
{
    /// <summary>
    /// Contains data on a specific player.
    /// </summary>
    public class UserProfile
    {
        [Key]
        public int ID { get; set; }

        [Display(Name = "Username")]
        public string UserName { get; set; }

        /// <summary>
        /// The player's total score.
        /// </summary>
        [Display(Name = "Total Score")]
        public int TotalScore
        {
            get
            {
                int totalScore = 0;
                foreach (PointScore score in Score)
                    totalScore += score.Score;
                return totalScore;
            }
        }

        public int ScoreFor(string pointType)
        {
            foreach (PointScore score in Score)
            {
                if (score.PointPath.Name == pointType)
                    return score.Score;
            }

            return 0;
        }

        /*
        public class RegisterModel
        {
            [Required]
            [Display(Name = "User name")]
            public string UserName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public class LoginModel
        {
            [Required]
            [Display(Name = "User name")]
            public string UserName { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public class LocalPasswordModel
        {
            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }*/

        /// <summary>
        /// Each inidividual score. The key should be the ARG path.
        /// </summary>
        public virtual ICollection<PointScore> Score { get; set; }

        /// <summary>
        /// The task that have been completed by the player.
        /// </summary>
        public virtual ICollection<Task> CompletedTask { get; set; }
    }
}