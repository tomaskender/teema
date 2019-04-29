using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
//using System.Web.Mvc;
using System.Security;

namespace Teema.Models {
    public class AccountProfileModel {
        const int maxDisplayedEventsOnPage = 12;
        public AccountProfileModel(int userId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            User user = entities.Users.First(u => u.Id == userId);
            Username = user.Username;
            Registered = user.Registered;
            Description = user.Description;
            HasPrivateProfile = user.HasPrivateProfile;
            IsFriendsWith = new AccountShowModel(userId).IsFriendsWith;

            Events = new UserEvents(userId).Events.Take(maxDisplayedEventsOnPage).ToList();
        }
        public string Username { get; }
        public DateTime Registered { get; }
        public string Description { get; }
        public bool HasPrivateProfile { get; }
        public bool IsFriendsWith { get; }
        public List<ShowEventModel> Events { get; }
    }
    public class AccountListMemberModel {
        public AccountListMemberModel(int UserId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            User user = entities.Users.First(u => u.Id == UserId);
            Username = user.Username;
            Country = user.Country.Name;
            Code = user.Country.Code;
        }
        public string Username { get; }
        public string Country { get; }
        public string Code { get; }
    }

    public class AccountShowModel : AccountListMemberModel {
        public AccountShowModel(int userId) : base(userId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            HasPrivateProfile = entities.Users.First(u => u.Id == userId).HasPrivateProfile;
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                int MyId = entities.Users.First(u => u.Username == HttpContext.Current.User.Identity.Name).Id;
                IsFollowing = entities.Follows.Where(f => f.FollowerId == MyId && f.FollowedId == userId).Count() > 0;
                if (entities.Follows.Any(f => f.FollowerId == MyId && f.FollowedId == userId)
                    && entities.Follows.Any(f => f.FollowerId == userId && f.FollowedId == MyId)) {
                    IsFriendsWith = true;
                } else {
                    IsFriendsWith = false;
                }
            }
            SubscriptionsCount = new SubscriptionListModel(userId).Subscriptions.Count;
            FollowsCount = new FollowListModel(userId).FollowerList.Count;
        }
        public bool HasPrivateProfile { get; }

        public bool IsFriendsWith { get; }

        public bool IsFollowing { get; }

        public int SubscriptionsCount { get; }

        public int FollowsCount { get; }
    }

    public class TeemaAccountListMember : AccountListMemberModel {
        public TeemaAccountListMember(int TeemaId, int UserId) : base(UserId) {
            if (new TeemaDBEntities().TeemaAccesses.Any(ta => ta.TeemaId == TeemaId && ta.UserId == UserId))
                Role = (TeemaRoles)new TeemaDBEntities().TeemaAccesses.First(ta => ta.TeemaId == TeemaId && ta.UserId == UserId).RoleId;
        }
        public TeemaRoles Role { get; }
    }

    public class PrivilegedAccountsListModel {
        public PrivilegedAccountsListModel(int TeemaId) {
            Users = new List<TeemaPrivilegedListMemberModel>();
            List<TeemaAccess> accesses = new TeemaDBEntities().TeemaAccesses.Where(ta => ta.TeemaId == TeemaId && ta.RoleId >= 3).ToList();
            foreach (TeemaAccess access in accesses) {
                Users.Add(new TeemaPrivilegedListMemberModel(TeemaId, access.UserId));
            }
            Users = Users.OrderBy(u => u.Username).OrderByDescending(u => u.Role).ToList();
        }
        public List<TeemaPrivilegedListMemberModel> Users { get; }
    }

    public class AccountLoginModel {
        public string ReturnUrl { get; set; }

        [Required(ErrorMessage = "Username Required", AllowEmptyStrings = false)]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Password Required", AllowEmptyStrings = false)]
        public string Password { get; set; }
    }

    public class AccountRegisterModel {
        public AccountRegisterModel() {
            Countries = new TeemaDBEntities().Countries.OrderBy(c=>c.Name).Select(c => new System.Web.Mvc.SelectListItem { Text = c.Name, Value = c.Name });
        }

        [Required(ErrorMessage = "Username Required", AllowEmptyStrings = false)]
        [StringLength(25, MinimumLength = 4, ErrorMessage = "Your username needs to be between 4 and 25 characters long.")]
        [RegularExpression(@"[a-z0-9]+", ErrorMessage = "You can only use lower-case basic latin characters and numbers.")]
        public string Username { get; set; }
        //[System.Web.Mvc.Remote("CheckUsername", ErrorMessage = "Username is taken.")]

        [Required(ErrorMessage = "Email Required", AllowEmptyStrings = false)]
        [EmailAddress(ErrorMessage = "Entered email is not valid.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password Required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [StringLength(25, MinimumLength = 4, ErrorMessage = "Your password needs to be between 4 and 25 characters long.")]
        public string Password { get; set; }

        //[NotMapped]
        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = "Confirming Password Required", AllowEmptyStrings = false)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Confirming Password and Password do not match.")]
        public string ConfirmPassword { get; set; }

        //[ForeignKey("Country")]
        public string Country { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Countries { get; set; }
    }

    public class AccountRoleModel {
        public AccountRoleModel() {
            HasLowerStatus = false;
        }

        [Display(Name = "Username")]
        [Required(ErrorMessage = "Username Required", AllowEmptyStrings = false)]
        public string Username { get; set; }

        [Display(Name = "Role")]
        [Required(ErrorMessage = "Defined Role Required")]
        public TeemaRoles Role { get; set; }

        public bool HasLowerStatus { get; set; }
    }

    public class AccountOptionsModel {
        public AccountOptionsModel() {
            TeemaDBEntities entities = new TeemaDBEntities();
            User user = entities.Users.First(u => u.Username == HttpContext.Current.User.Identity.Name);
            Description = user.Description;
            HasPrivateProfile = user.HasPrivateProfile;
        }
        [Display(Name = "Bio")]
        [MaxLength(150)]
        public string Description { get; set; }

        [Display(Name = "Should your profile be private?")]
        public bool HasPrivateProfile { get; set; }


        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; }
    }

    public enum TeemaRoles {
        Undefined,
        Viewer,
        Poster,
        Moderator,
        Admin,
        Owner
    }
}