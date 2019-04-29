using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Teema.Models {
    /*public class TeemaListModel {
        public TeemaListModel() {
            //get teemas
        }

        List<TeemaListMemberModel> featuredTeemas = new List<TeemaListMemberModel>();
        List<TeemaListModel> subscribedTeemas = new List<TeemaListModel>();
    }*/

    public class TeemaListModel {
        public TeemaListModel() {
            TeemaDBEntities entities = new TeemaDBEntities();
            Teemas = new List<TeemaListMemberModel>();
            List<int> unfilteredTeemaIds = entities.Teemas.Select(t => t.Id).ToList();
            foreach (int teemaId in unfilteredTeemaIds) {
                Teemas.Add(new TeemaListMemberModel(teemaId));
            }
        }
        public List<TeemaListMemberModel> Teemas = new List<TeemaListMemberModel>();
    }

    public class TeemaListMemberModel {
        public TeemaListMemberModel(int teemaId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            if (entities.Teemas.Any(t => t.Id == teemaId)) {
                Name = entities.Teemas.First(t => t.Id == teemaId).Name;
                if (HttpContext.Current.User.Identity.IsAuthenticated) {
                    IsSubscribed = entities.Subscriptions.Any(s => s.TeemaId == teemaId && s.User.Username == HttpContext.Current.User.Identity.Name);
                }
                //ThreadCount = entities.Threads.Where(t => t.TeemaId == teemaId).Count();
            }

        }
        public string Name { get; }
        public bool IsSubscribed { get; }
        //public int ThreadCount { get; }
    }
    public class TeemaShowModel : TeemaListMemberModel {
        public TeemaShowModel(int teemaId, int page) : base(teemaId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Description = entities.Teemas.Find(teemaId).Description;
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                UserRole = (TeemaRoles)entities.TeemaAccesses.Where(ta => ta.User.Username == HttpContext.Current.User.Identity.Name && ta.TeemaId == teemaId).Select(ta => ta.RoleId).FirstOrDefault();
                AnyoneCanPost = entities.Teemas.Find(teemaId).AnyoneCanPost || UserRole >= TeemaRoles.Poster;
            } else {
                AnyoneCanPost = entities.Teemas.Find(teemaId).AnyoneCanPost;
            }
            ThreadList = new ThreadListModel(teemaId, page);
            PriviledAccountModels = new PrivilegedAccountsListModel(teemaId);
        }
        public string Description { get; }
        public ThreadListModel ThreadList { get; }
        public TeemaRoles UserRole { get; }
        public PrivilegedAccountsListModel PriviledAccountModels { get; }
        public bool AnyoneCanPost { get; }
    }

    public class TeemaCreateModel {
        [Required]
        [Display(Name = "Name")]
        [StringLength(maximumLength: 20, MinimumLength = 1, ErrorMessage = "The name needs to be between 1 and 20 characters long.")]
        [RegularExpression(@"[a-z0-9]+", ErrorMessage = "You can only use lower-case basic latin characters and numbers.")]
        public string Name { get; set; }

        [Display(Name = "Anyone can view this teema")]
        public bool AnyoneCanView { get; set; }

        [Display(Name = "Anyone can post into this teema")]
        public bool AnyoneCanPost { get; set; }
    }

    public class TeemaFeaturedListMemberModel {
        public TeemaFeaturedListMemberModel(int teemaId) {
            Teema teema = new TeemaDBEntities().Teemas.First(t => t.Id == teemaId);
            Name = teema.Name;
            Description = teema.Description;
        }
        public string Name { get; }
        public string Description { get; }
    }

    public class TeemaPrivilegedListMemberModel {
        public TeemaPrivilegedListMemberModel(int teemaId, int userId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Username = entities.Users.First(u => u.Id == userId).Username;
            Role = (TeemaRoles)entities.TeemaAccesses.First(ta => ta.TeemaId == teemaId && ta.UserId == userId).RoleId;
            CountryCode = entities.Users.Find(userId).Country.Code;
        }
        public string Username { get; }
        public TeemaRoles Role { get; }
        public string CountryCode { get; }
    }

    public class TeemaSettingsModel {
        public TeemaSettingsModel() { }

        public TeemaSettingsModel(int teemaId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Teema teema = entities.Teemas.Find(teemaId);
            Description = teema.Description;
            AnyoneCanPost = teema.AnyoneCanPost;
            AnyoneCanView = teema.AnyoneCanView;
        }
        
        public string Description { get; set; }

        [Display(Name = "Allow anyone to post")]
        public bool AnyoneCanPost { get; set; }

        [Display(Name = "Allow anyone to view")]
        public bool AnyoneCanView { get; set; }
    }
}