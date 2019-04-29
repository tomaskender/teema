using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace Teema {
    public class ChatHub : Hub {
        public void SendMessage(string receiver, string message) {
            //addMessageToBrowser(message, sender)
            Clients.User(receiver).addMessageToBrowser(message, Context.User.Identity.Name);
        }

        public override System.Threading.Tasks.Task OnConnected() {
            if (!Context.User.Identity.IsAuthenticated) {
                Clients.Caller.disconnectFromHub();
            } else {
                UserHandler.Clients.Add(new ConnectedClient() {
                    ConnectionId = Context.ConnectionId, User = Context.User.Identity.Name
                });
            }
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected(bool stopCalled) {
            UserHandler.Clients.RemoveAll(c => c.ConnectionId == Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        [Authorize]
        public string GetNotificationList() {
            List<Models.ShowNotificationsModel> notifications = new Models.UserNotifications().Notifications.OrderByDescending(n => n.Date).ToList();
            return JsonConvert.SerializeObject(notifications);
        }

        [Authorize]
        public string GetFriendList() {
            TeemaDBEntities entities = new TeemaDBEntities();
            List<int> followedIds = entities.Follows.Where(f => f.FollowerUser.Username == Context.User.Identity.Name).Select(f => f.FollowedId).ToList();
            List<int> followerIds = entities.Follows.Where(f => f.FollowedUser.Username == Context.User.Identity.Name).Select(f => f.FollowerId).ToList();
            List<int> friendIds = followedIds.Intersect(followerIds).ToList();
            List<string> friends = entities.Users.Where(u => friendIds.Contains(u.Id)).Select(u => u.Username).ToList();
            string onlineFriendsJson = JsonConvert.SerializeObject(UserHandler.Clients.Where(c => friends.Contains(c.User)).Select(c => c.User).Distinct());
            return onlineFriendsJson;
        }
    }

    public static class UserHandler {
        public static List<ConnectedClient> Clients = new List<ConnectedClient>();
    }

    public class ConnectedClient {
        public string ConnectionId;
        public string User;
    }
}