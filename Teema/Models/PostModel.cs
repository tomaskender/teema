using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Teema.Models {
    public class PostShowModel {
        public PostShowModel(int PostId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Post post = entities.Posts.First(p => p.Id == PostId);
            Id = PostId;
            Author = entities.Users.Find(post.AuthorId).Username;
            Date = post.Created;
            Message = post.Message;
            ParentId = post.ParentPostId;
            Karma = post.Karma;
            if (!HttpContext.Current.User.Identity.IsAuthenticated) {
                Vote = 0;
            } else {
                if (entities.Votes.Any(v => v.PostId == Id && v.User.Username == HttpContext.Current.User.Identity.Name)) {
                    if (entities.Votes.First(v => v.PostId == Id && v.User.Username == HttpContext.Current.User.Identity.Name).IsUpvote)
                        Vote = 1;
                    else
                        Vote = -1;
                } else {
                    Vote = 0;
                }
            }
        }
        public int Id { get; }
        public string Author { get; }
        public DateTime Date { get; }
        public string Message { get; }
        public int? ParentId { get; set; }
        public int Karma { get; }
        //Vote=1 -> upvoted by the user, 0 -> the user hasn't voted yet, -1 -> downvoted by the user
        public int Vote { get; }
        public bool HasHiddenChildPosts { get; set; }
        public bool IsSelected { get; set; }
    }

    public class PostCreateModel {
        public PostCreateModel() { }
        public PostCreateModel(int threadId, int? parentPostId) {
            TeemaDBEntities entities = new TeemaDBEntities();
            Thread thread = entities.Threads.Find(threadId);
            if(thread.Posts.Any(p=>p.ParentPostId == null) && parentPostId == null) {
                throw new ArgumentException("Trying to create a parent post in a thread which already has a parent post.");
            }
            ParentPostId = parentPostId;
            teema = thread.Teema.Name;
            LinkId = thread.LinkId;
        }

        public string teema { get; set; }

        [Required]
        public string Message { get; set; }
        
        [Required]
        public int? ParentPostId { get; set; }
        
        [Required]
        public string LinkId { get; set; }
    }

    public class PostEditModel {
        public PostEditModel(int postId) {
            TeemaDBEntities entities = new TeemaDBEntities();

            Teema = entities.Posts.Find(postId).Thread.Teema.Name;
            PostId = postId;
        }
        public string Teema { get; }
        public int PostId { get; }
    }

    public class PostDeleteModel {
        public PostDeleteModel(int postId) {
            TeemaDBEntities entities = new TeemaDBEntities();

            Teema = entities.Posts.Find(postId).Thread.Teema.Name;
            PostId = postId;
        }
        public string Teema { get; }
        public int PostId { get; }
    }

    public class PostCreatedModel {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
    }
}