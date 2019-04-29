using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teema.Models {
    public class FollowListModel {
        public FollowListModel() { FollowerList = new List<FollowListMemberModel>(); }
        public FollowListModel(int UserId) {
            FollowerList = new List<FollowListMemberModel>();
            List<Follow> follows = new TeemaDBEntities().Follows.Where(f => f.FollowerId == UserId).ToList();
            foreach (Follow follow in follows)
                FollowerList.Add(new FollowListMemberModel(UserId, follow.FollowedId));
        }
        public List<FollowListMemberModel> FollowerList { get; }
    }

    public class FollowListMemberModel {
        public FollowListMemberModel(int followerId, int followedId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            User followedUser = entities.Users.First(u => u.Id == followedId);
            FollowedUsername = followedUser.Username;

            //checks if the followed user has public follows
            bool hasPublicFollows = !followedUser.HasPrivateProfile;
            if(hasPublicFollows) {
                //checks if the followed user follows back the follower
                bool isFollowingBack = entities.Follows.Any(f => f.FollowerId == followedId && f.FollowedId == followerId);
                if(isFollowingBack) {
                    FollowsBack = FollowStatus.Following;
                } else {
                    FollowsBack = FollowStatus.NotFollowing;
                }
            } else {
                FollowsBack = FollowStatus.NotAccessible;
            }


        }
        public string FollowedUsername { get; }
        public FollowStatus FollowsBack { get; }
    }

    public class ConfirmUserFollow {
        public ConfirmUserFollow(string targetUser) {
            Name = targetUser;
            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                TeemaDBEntities entities = new TeemaDBEntities();
                IsFollowing = entities.Follows.Any(f => f.FollowerUser.Username == HttpContext.Current.User.Identity.Name && f.FollowedUser.Username == targetUser);
            }
        }
        public string Name { get; }
        public bool IsFollowing { get; }
    }

    public enum FollowStatus {
        NotAccessible,
        NotFollowing,
        Following
    }
}