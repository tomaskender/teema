using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Teema.Models;

namespace Teema.Controllers {
    public class ThreadController : Controller {
        private const int maxTitleLengthInLinkId = 14;
        private const int randomLinkIdLength = 5;
        private const string characters = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int maxSeenNotificationsInDatabase = 10;

        TeemaDBEntities entities = new TeemaDBEntities();

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Poster)]
        public ActionResult Create(string teema) {
            ViewBag.teema = teema;
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Poster)]
        public ActionResult Create(string teema, ThreadCreateModel model) {
            if (ModelState.IsValid) {
                //generate 5 chars long unique identifier for the new thread
                string randomLinkId = null;
                Random random = new Random();

                int teemaId = entities.Teemas.First(t => t.Name == teema).Id;
                string username = User.Identity.Name;
                string title = model.Title;

                //------- Normalizacia nadpisu- znaky ako áéíóúčšž atd. sa premenia na zakladnu latinku a nasledne ju skrati na pozadovanu dlzku
                var normalizedTitle = title.Normalize(System.Text.NormalizationForm.FormD);
                var stringBuilder = new System.Text.StringBuilder();

                foreach (var c in normalizedTitle) {
                    var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark) {
                        stringBuilder.Append(c);
                    }
                }

                normalizedTitle = stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
                normalizedTitle = System.Text.RegularExpressions.Regex.Replace(normalizedTitle, @"\W+", "-");
                string shortenedNormalizedTitle = normalizedTitle.Length > maxTitleLengthInLinkId ? normalizedTitle.Remove(maxTitleLengthInLinkId) : normalizedTitle;
                shortenedNormalizedTitle = System.Text.RegularExpressions.Regex.Replace(shortenedNormalizedTitle, @"\W+\Z", "");
                //-----------------

                string linkId = null;
                do {
                    randomLinkId = "";
                    for (int i = 0; i < randomLinkIdLength; i++) {
                        randomLinkId += characters[random.Next(0, characters.Length)];
                    }
                    linkId = shortenedNormalizedTitle + "-" + randomLinkId;
                } while (entities.Threads.Any(t => t.TeemaId == teemaId && t.LinkId == linkId));


                Thread createdThread = new Thread() {
                    Name = model.Title,
                    Created = DateTime.Now,
                    //Name example: Hello world lorem ipsum....
                    //LinkId format: Hello-world-lo-5e4zb
                    LinkId = linkId,
                    TeemaId = teemaId
                };
                entities.Threads.Add(createdThread);
                entities.SaveChanges();

                PostCreateModel createdPost = new PostCreateModel(createdThread.Id, null) {
                    Message = model.Message
                };
                CreatePost(teema, createdPost);

                return PartialView("CreatedThreadInfo", new ThreadCreatedInfoModel(createdThread.Id));
            } else return PartialView(model);
        }

        [Authorize]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Moderator)]
        public ActionResult Delete(string teema, string linkId) {
            if (!string.IsNullOrEmpty(teema) && !string.IsNullOrEmpty(linkId)) {
                Thread threadToDelete = entities.Threads.Single(t => t.Teema.Name == teema && t.LinkId == linkId);
                foreach (Post post in threadToDelete.Posts) {
                    entities.Votes.RemoveRange(post.Votes);
                }
                entities.SaveChanges();
                entities.Posts.RemoveRange(threadToDelete.Posts);
                entities.SaveChanges();
                entities.Threads.Remove(threadToDelete);
                entities.SaveChanges();
                return RedirectToAction("Show", "Teema", new { teema = teema });
            } else {
                return new EmptyResult();
            }
        }

        [AllowAnonymous]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Viewer)]
        public ActionResult Show(string teema, string linkId, int? parentPostId) {
            if (linkId == null && teema != null)
                return RedirectToAction("Show", "Teema", new { teema = teema });
            else if (teema == null)
                return RedirectToAction("Index", "Teema");

            int teemaId = entities.Teemas.First(t => t.Name == teema).Id;
            if (entities.Threads.Where(t => t.LinkId == linkId && t.TeemaId == teemaId).Count() > 0) {
                return View("Show", new ThreadShowModel(teemaId, linkId, parentPostId));
            }
            return View("Error");
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreatePost(string teema, string linkId, int parentId) {
            if (teema == null || linkId == null) return PartialView("Error");

            //int teemaId = entities.Teemas.First(t => t.Name == teema).Id;
            PostCreateModel model = new PostCreateModel(entities.Threads.First(t => t.LinkId == linkId && t.Teema.Name == teema).Id, parentId);
            return PartialView(new PostCreateModel(entities.Threads.First(t => t.LinkId == linkId && t.Teema.Name == teema).Id, parentId));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [AuthorizeTeemaAccess(RequiredRole = TeemaRoles.Poster)]
        public ActionResult CreatePost(string teema, PostCreateModel model) {
            //checks if the teema name wasn't changed to a different value by the user
            if (ModelState.IsValid) {
                int teemaId = entities.Teemas.First(t => t.Name == teema).Id;
                Post createdPost = new Post() {
                    ThreadId = entities.Threads.First(t => t.LinkId == model.LinkId && t.TeemaId == teemaId).Id,
                    AuthorId = entities.Users.First(u => u.Username == User.Identity.Name).Id,
                    Created = DateTime.Now,
                    Message = model.Message,
                    ParentPostId = model.ParentPostId
                };
                entities.Posts.Add(createdPost);

                if (model.ParentPostId != null) {
                    int targetId = entities.Posts.Find(model.ParentPostId).AuthorId;
                    if (!entities.NotificationGroups.Any(n => n.EventType == (int)EventType.Post && n.EventId == model.ParentPostId)) {
                        entities.NotificationGroups.Add(new NotificationGroup() {
                            EventType = (int)EventType.Post,
                            EventId = (int)model.ParentPostId,
                            TargetId = targetId,
                            Seen = false
                        });
                        entities.SaveChanges();
                    } else {
                        entities.NotificationGroups.First(n => n.EventType == (int)EventType.Post && n.EventId == model.ParentPostId).Seen = false;
                    }

                    entities.NotificationGroupMembers.Add(new NotificationGroupMember() {
                        SourceId = entities.Users.First(u => u.Username == User.Identity.Name).Id,
                        NotificationGroupId = entities.NotificationGroups.First(n => n.EventType == (int)EventType.Post && n.EventId == model.ParentPostId).Id,
                        Date = DateTime.Now
                    });

                    entities.SaveChanges();

                    IEnumerable<NotificationGroup> groups = entities.NotificationGroups.Where(n => n.TargetId == targetId && !n.Seen);
                    if (groups.Count() > maxSeenNotificationsInDatabase) {
                        IEnumerable<NotificationGroup> groupsToRemove = groups.Take(groups.Count() - maxSeenNotificationsInDatabase);
                        foreach (NotificationGroup notGroup in groupsToRemove) {
                            entities.NotificationGroupMembers.RemoveRange(notGroup.NotificationGroupMembers);
                        }
                        entities.SaveChanges();

                        entities.NotificationGroups.RemoveRange(groupsToRemove);
                    }
                }

                entities.SaveChanges();
                return Content(Newtonsoft.Json.JsonConvert.SerializeObject(new PostCreatedModel() {
                    Id = createdPost.Id,
                    Username = User.Identity.Name,
                    Message = createdPost.Message
                }));
            } else return PartialView(model);
        }

        [HttpGet]
        [Authorize]
        public ActionResult EditPost(int postId) {
            return PartialView(new PostEditModel(postId));
        }

        [HttpPost]
        [Authorize]
        public ActionResult EditPost(string teema, int postId, string message) {
            int roleId = entities.TeemaAccesses.Where(ta => ta.User.Username == User.Identity.Name && ta.Teema.Name == teema).Select(ta => ta.RoleId).FirstOrDefault();
            bool isCreatorOfPost = entities.Posts.Find(postId).User.Username == User.Identity.Name;
            if (roleId >= (int)TeemaRoles.Moderator || isCreatorOfPost) {
                entities.Posts.Find(postId).Message = message + " | edited by @"+User.Identity.Name;
                entities.SaveChanges();
                return Content(entities.Posts.First(p => p.Id == postId).Message);
            } else {
                return new EmptyResult();
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult DeletePost(int postId) {
            return PartialView(new PostDeleteModel(postId));
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeletePost(string teema, int postId) {
            int userId = entities.Users.First(u => u.Username == User.Identity.Name).Id;
            int teemaId = entities.Teemas.First(t => t.Name == teema).Id;
            int roleId = entities.TeemaAccesses.Where(ta => ta.UserId == userId && ta.TeemaId == teemaId).Select(ta => ta.RoleId).FirstOrDefault();
            bool isCreatorOfPost = entities.Posts.First(p => p.Id == postId).AuthorId == userId;
            if (roleId >= (int)TeemaRoles.Moderator || isCreatorOfPost) {
                entities.Posts.First(p => p.Id == postId).Message = "[This post was deleted by @" + User.Identity.Name + "]";
                entities.SaveChanges();
                return Content(entities.Posts.First(p => p.Id == postId).Message);
            } else {
                return new ViewResult {
                    ViewName = "~/Views/Shared/Error.cshtml",
                    ViewData = new ViewDataDictionary() { Model = "You need to be either the creator of the post or a moderator to delete this post!" }
                };
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddPostKarma(int postId) {
            int userId = entities.Users.First(u => u.Username == HttpContext.User.Identity.Name).Id;
            if (!entities.Votes.Any(v => v.PostId == postId && v.UserId == userId)) {
                Post post = entities.Posts.First(p => p.Id == postId);
                post.Karma++;
                Vote vote = new Vote() { UserId = userId, PostId = postId, IsUpvote = true };
                entities.Votes.Add(vote);
                entities.SaveChanges();

                if (!entities.NotificationGroups.Any(n => n.EventType == (int)EventType.Karma && n.EventId == postId)) {
                    entities.NotificationGroups.Add(new NotificationGroup() {
                        EventType = (int)EventType.Karma,
                        EventId = vote.Id,
                        TargetId = entities.Posts.Find(postId).AuthorId,
                        Seen = false
                    });
                } else {
                    entities.NotificationGroups.First(n => n.EventType == (int)EventType.Karma && n.EventId == postId).Seen = false;
                }
                entities.SaveChanges();

                entities.NotificationGroupMembers.Add(new NotificationGroupMember() {
                    SourceId = entities.Users.First(u => u.Username == User.Identity.Name).Id,
                    NotificationGroupId = entities.NotificationGroups.First(n => n.EventType == (int)EventType.Karma && n.EventId == vote.Id).Id,
                    Date = DateTime.Now,
                });

                entities.SaveChanges();

                IEnumerable<NotificationGroup> groups = entities.NotificationGroups.Where(n => n.TargetId == post.User.Id && !n.Seen);
                if (groups.Count() > maxSeenNotificationsInDatabase) {
                    IEnumerable<NotificationGroup> groupsToRemove = groups.Take(groups.Count() - maxSeenNotificationsInDatabase);
                    foreach (NotificationGroup notGroup in groupsToRemove) {
                        entities.NotificationGroupMembers.RemoveRange(notGroup.NotificationGroupMembers);
                    }
                    entities.SaveChanges();

                    entities.NotificationGroups.RemoveRange(groupsToRemove);
                }

                entities.SaveChanges();

            } else if (entities.Votes.First(v => v.PostId == postId && v.UserId == userId).IsUpvote == false) {
                entities.Posts.First(p => p.Id == postId).Karma++;
                entities.Votes.Remove(entities.Votes.First(v => v.UserId == userId && v.PostId == postId));
                entities.SaveChanges();
            } else {
                entities.Posts.First(p => p.Id == postId).Karma--;
                entities.Votes.Remove(entities.Votes.First(v => v.UserId == userId && v.PostId == postId));
                entities.SaveChanges();
            }
            return Content(entities.Posts.First(p => p.Id == postId).Karma.ToString(), "text");
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemovePostKarma(int postId) {
            int userId = entities.Users.First(u => u.Username == HttpContext.User.Identity.Name).Id;
            if (entities.Votes.Where(v => v.PostId == postId && v.UserId == userId).Count() == 0) {
                entities.Posts.First(p => p.Id == postId).Karma--;
                entities.Votes.Add(new Vote() { UserId = userId, PostId = postId, IsUpvote = false });
                entities.SaveChanges();

            } else if (entities.Votes.First(v => v.PostId == postId && v.UserId == userId).IsUpvote == true) {
                entities.Posts.First(p => p.Id == postId).Karma--;
                entities.Votes.Remove(entities.Votes.First(v => v.UserId == userId && v.PostId == postId));
                entities.SaveChanges();
            } else {
                entities.Posts.First(p => p.Id == postId).Karma++;
                entities.Votes.Remove(entities.Votes.First(v => v.UserId == userId && v.PostId == postId));
                entities.SaveChanges();
            }
            return Content(entities.Posts.First(p => p.Id == postId).Karma.ToString(), "text");
        }

        [Authorize]
        public ActionResult GetVoteStatus(int postId) {
            return Content(new PostShowModel(postId).Vote.ToString());
        }
    }
}