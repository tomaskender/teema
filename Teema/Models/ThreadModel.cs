using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Teema.Models {
    public class ThreadListModel {
        const int maxThreadsInList = 15;
        const int maxEventsInList = 10;

        public ThreadListModel(bool showSubscriptions, int page) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Random random = new Random();
            List<int> randomTeemaIndexes = new List<int>();
            FeaturedTeemas = new List<TeemaFeaturedListMemberModel>();
            do {
                int randomIndex = random.Next(0, entities.Teemas.Count());
                if (!randomTeemaIndexes.Contains(randomIndex))
                    randomTeemaIndexes.Add(randomIndex);
            } while (randomTeemaIndexes.Count < 3);

            List<Teema> teemas = entities.Teemas.ToList();

            FeaturedTeemas = new List<TeemaFeaturedListMemberModel>() {
                new TeemaFeaturedListMemberModel(teemas[randomTeemaIndexes[0]].Id),
                new TeemaFeaturedListMemberModel(teemas[randomTeemaIndexes[1]].Id),
                new TeemaFeaturedListMemberModel(teemas[randomTeemaIndexes[2]].Id)
            };

            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                //EVENTS
                FollowedUsersEvents = new List<ShowEventModel>();
                List<Follow> follows = entities.Follows.Where(f => f.FollowerUser.Username == HttpContext.Current.User.Identity.Name).ToList();
                foreach (Follow follow in follows) {
                    FollowedUsersEvents.AddRange(new UserEvents(follow.FollowedId).Events);
                }

                FollowedUsersEvents = FollowedUsersEvents.OrderByDescending(e => e.Date).Take(maxEventsInList).ToList();
            }

            ShowingSubscribedThreads = showSubscriptions;

            List<Thread> unprocessedThreads = new List<Thread>();
            if (showSubscriptions) {
                List<Subscription> subscriptions = entities.Subscriptions.Where(s => s.User.Username == HttpContext.Current.User.Identity.Name).ToList();
                foreach (Subscription subscription in subscriptions) {
                    unprocessedThreads.AddRange(entities.Threads.Where(t => t.TeemaId == subscription.TeemaId).ToList());
                }
            } else {
                unprocessedThreads = entities.Threads.Where(th => th.Teema.AnyoneCanView).ToList();

            }
            List<Thread> dbThreads = unprocessedThreads.OrderByDescending(t => t.Created).Skip(maxThreadsInList * (page - 1)).Take(maxThreadsInList).ToList();

            Threads = new List<ThreadListMemberModel>();
            foreach (Thread thread in dbThreads) {
                Threads.Add(new ThreadListMemberModel(thread.TeemaId, thread.LinkId));
            }
            Page = page;
            PreviousPageExists = page > 1;
            NextPageExists = unprocessedThreads.Count() - maxThreadsInList * page > 0;

            Teemas = new TeemaListModel().Teemas.OrderBy(t => t.Name).OrderByDescending(t => t.IsSubscribed).ToList();
        }

        public ThreadListModel(int teemaId, int page) {
            TeemaDBEntities entities = new TeemaDBEntities();
            List<Thread> unprocessedThreads = entities.Threads.Where(t => t.TeemaId == teemaId).ToList();
            List<Thread> dbThreads = unprocessedThreads.OrderByDescending(t => t.Created).Skip(maxThreadsInList * (page - 1)).Take(maxThreadsInList).ToList();

            Threads = new List<ThreadListMemberModel>();

            foreach (Thread thread in dbThreads) {
                Threads.Add(new ThreadListMemberModel(teemaId, thread.LinkId));
            }
            Page = page;
            PreviousPageExists = page > 1;
            NextPageExists = unprocessedThreads.Count() - maxThreadsInList * page > 0;

        }

        public List<ThreadListMemberModel> Threads { get; }
        public List<TeemaFeaturedListMemberModel> FeaturedTeemas { get; }
        public List<ShowEventModel> FollowedUsersEvents { get; }
        public bool ShowingSubscribedThreads;
        public int Page { get; }
        public bool NextPageExists { get; }
        public bool PreviousPageExists { get; }
        public List<TeemaListMemberModel> Teemas { get; }
    }

    public class ThreadListMemberModel {
        public ThreadListMemberModel(int teemaId, string threadLinkId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Thread thread = entities.Threads.First(t => t.TeemaId == teemaId && t.LinkId == threadLinkId);
            LinkId = thread.LinkId;
            Teema = entities.Teemas.First(t => t.Id == thread.TeemaId).Name;
            Title = thread.Name;
            Author = thread.Posts.Single(p => p.ParentPostId == null).User.Username;
            Created = thread.Created;
            Karma = thread.Posts.Single(p => p.ParentPostId == null).Karma;
            CommentsCount = thread.Posts.Where(p => p.ThreadId == thread.Id && p.ParentPostId != null).Count();
        }

        public string LinkId { get; }
        public string Teema { get; }
        public string Title { get; }
        public string Author { get; }
        public DateTime Created { get; }
        public int Karma { get; }
        public int CommentsCount { get; }
    }

    public class ThreadShowModel : ThreadListMemberModel {
        public ThreadShowModel(int teemaId, string linkId, int? parentPostId) : base(teemaId, linkId) {
            const int maxThreadDeepness = 4;

            TeemaDBEntities entities = new TeemaDBEntities();
            Thread thread = entities.Threads.First(t => t.TeemaId == teemaId && t.LinkId == LinkId);
            List<Post> DbPosts = thread.Posts.ToList();

            OriginalParentId = DbPosts.First(p => p.ParentPostId == null).Id;
            if (parentPostId == null)
                ParentId = OriginalParentId;
            else
                ParentId = (int)parentPostId;

            List<int> allPostIndexes = new List<int>() { (int)ParentId };
            List<int> allIndexesWithHiddenPosts = new List<int>();
            List<int> currentPostIndexes = allPostIndexes;
            List<int> nextPostIndexes = new List<int>();
            for (int i = 1; i <= maxThreadDeepness; i++) {
                if (currentPostIndexes.Count == 0) break;

                foreach (int currentPost in currentPostIndexes) {
                    foreach (Post post in DbPosts.Where(p => p.ParentPostId == currentPost).OrderByDescending(p => p.Karma)) {
                        if (i == maxThreadDeepness) {
                            if (allPostIndexes.Contains((int)post.ParentPostId)) {
                                allIndexesWithHiddenPosts.Add((int)post.ParentPostId);
                            }
                        } else {
                            nextPostIndexes.Add(post.Id);
                        }
                    }
                }

                if (i != maxThreadDeepness) {
                    allPostIndexes = allPostIndexes.Concat(nextPostIndexes).ToList();
                    currentPostIndexes.Clear();
                    currentPostIndexes = currentPostIndexes.Concat(nextPostIndexes).ToList();
                    nextPostIndexes.Clear();
                }
            }

            if (parentPostId != null) {
                if (entities.Posts.First(p => p.Id == parentPostId).ParentPostId != null) {
                    allPostIndexes.Insert(0, (int)entities.Posts.First(p => p.Id == parentPostId).ParentPostId);
                }
            }

            Posts = new List<PostShowModel>();

            foreach (int postId in allPostIndexes) {
                PostShowModel post = new PostShowModel(postId);
                if (postId == parentPostId)
                    post.IsSelected = true;
                if (allIndexesWithHiddenPosts.Contains(postId))
                    post.HasHiddenChildPosts = true;
                Posts.Add(post);
            }

            if (Posts.First().ParentId != null) {
                Posts.First().ParentId = null;
            }

            if (HttpContext.Current.User.Identity.IsAuthenticated) {
                UserRole = (TeemaRoles)entities.TeemaAccesses.Where(ta => ta.User.Username == HttpContext.Current.User.Identity.Name && ta.TeemaId == teemaId).Select(ta => ta.RoleId).FirstOrDefault();
            } else {
                UserRole = TeemaRoles.Undefined;
            }
        }

        public TeemaRoles UserRole { get; }
        public int OriginalParentId { get; set; }
        public int ParentId { get; set; }
        public List<PostShowModel> Posts { get; }
    }

    public class ThreadCreateModel {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Teema { get; set; }

        [Required]
        [StringLength(maximumLength: 150, MinimumLength = 1, ErrorMessage = "The message can be 1-150 characters long.")]
        public string Message { get; set; }
    }

    public class ThreadDeleteModel {
        [Required]
        public string Teema { get; set; }

        [Required]
        public string LinkId { get; set; }
    }

    public class ThreadCreatedInfoModel {
        public ThreadCreatedInfoModel(int threadId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Thread thread = entities.Threads.First(t => t.Id == threadId);
            Title = thread.Name;
            Teema = thread.Teema.Name;
            LinkId = thread.LinkId;
        }
        public string Title { get; }
        public string Teema { get; }
        public string LinkId { get; }
    }


}