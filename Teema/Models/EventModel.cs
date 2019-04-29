using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teema.Models {

    public class FollowedUsersEvents {

    }

    public class UserEvents {
        public UserEvents(int userId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Events = new List<ShowEventModel>();
            string username = entities.Users.Find(userId).Username;
            if (entities.Users.First(u => u.Id == userId).HasPrivateProfile) {
                if (HttpContext.Current.User.Identity.IsAuthenticated) {
                    int myId = entities.Users.First(u => u.Username == HttpContext.Current.User.Identity.Name).Id;
                    if (!(entities.Follows.Any(f => f.FollowerId == myId && f.FollowedId == userId)
                        && entities.Follows.Any(f => f.FollowerId == userId && f.FollowedId == myId))) {
                        return;
                    }
                } else {
                    return;
                }
            }
            List<Post> posts = entities.Posts.Where(p => p.AuthorId == userId).ToList();
            foreach (Post post in posts) {
                Events.Add(new ShowEventModel() {
                    Username = username,
                    Date = post.Created,
                    Action = "wrote a post in " + post.Thread.Name,
                    Message = post.Message,
                    Link = "/t/" + post.Thread.Teema.Name + "/" + post.Thread.LinkId + "/" + post.Id
                });
            }
            List<Follow> follows = entities.Follows.Where(p => p.FollowerId == userId).ToList();
            foreach (Follow follow in follows) {
                Events.Add(new ShowEventModel() {
                    Username = username,
                    Date = follow.Date,
                    Action = "started following " + follow.FollowedUser.Username,
                    Link = "/u/" + follow.FollowedUser.Username
                });
            }
            List<Subscription> subscriptions = entities.Subscriptions.Where(p => p.UserId == userId).ToList();
            foreach (Subscription subscription in subscriptions) {
                Events.Add(new ShowEventModel() {
                    Username = username,
                    Date = subscription.Date,
                    Action = "subscribed to " + subscription.Teema.Name,
                    Link = "/t/" + subscription.Teema.Name
                });
            }
            Events = Events.OrderByDescending(t => t.Date).ToList();
        }
        public List<ShowEventModel> Events { get; set; }
    }

    public class UserNotifications {
        public UserNotifications() {
            TeemaDBEntities entities = new TeemaDBEntities();
            Notifications = new List<ShowNotificationsModel>();

            if (!HttpContext.Current.User.Identity.IsAuthenticated) return;

            List<NotificationGroup> notGroups = entities.NotificationGroups.Where(n => n.User.Username == HttpContext.Current.User.Identity.Name).ToList();

            foreach (NotificationGroup notGroup in notGroups) {
                //List<NotificationGroupMember> notGroupMembers = entities.NotificationGroupMembers.Where(n => n.NotificationGroupId == notGroup.Id).ToList();
                string action;
                string link;
                switch ((EventType)notGroup.EventType) {
                    case EventType.Post:
                        Post post = entities.Posts.Find(notGroup.EventId);
                        action = "commented your post.";
                        link = "/t/" + post.Thread.Teema.Name + "/" + post.Thread.LinkId + "/" + post.Id;
                        break;
                    case EventType.Karma:
                        Vote vote = entities.Votes.Find(notGroup.EventId);
                        action = "upvoted your post.";
                        link = "/t/" + vote.Post.Thread.Teema.Name + "/" + vote.Post.Thread.LinkId + "/" + vote.Post.Id;
                        break;
                    case EventType.Follow:
                        action = "followed you.";
                        link = "/u/" + notGroup.User.Username;
                        break;
                    default: action = ""; link = ""; break;
                }

                string otherPeople;
                switch (notGroup.NotificationGroupMembers.Count) {
                    case 1: otherPeople = ""; break;
                    case 2: otherPeople = "and 1 other person"; break;
                    default: otherPeople = "and " + (notGroup.NotificationGroupMembers.Count - 1) + " other people"; break;
                }

                Notifications.Add(new ShowNotificationsModel() {
                    Id = notGroup.Id,
                    Date = notGroup.NotificationGroupMembers.Last().Date,
                    Username = notGroup.NotificationGroupMembers.Last().User.Username,
                    Action = otherPeople + " " + action,
                    Link = link,
                    Seen = notGroup.Seen
                });
            }
            Notifications = Notifications.OrderByDescending(n => n.Date).ToList();
        }
        public List<ShowNotificationsModel> Notifications { get; }
    }

    public enum EventType {
        Post,
        Follow,
        Subscribe,
        Karma
    }

    public class ShowEventModel {

        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string Link { get; set; }
    }

    public class ShowNotificationsModel {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }
        public string Link { get; set; }
        public bool Seen { get; set; }
    }

}