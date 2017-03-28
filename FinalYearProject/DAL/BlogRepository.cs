﻿using CS348_CW2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CS348_CW2.DAL
{
    public class BlogRepository : IBlogRepository, IDisposable
    {
        public BlogDbContext _context;
        public ApplicationDbContext _userContext;

        public BlogRepository(BlogDbContext context)
        {
            _context = context;
        }

        public IList<Post> GetPosts()
        {
            return _context.Posts.ToList();
        }

        public List<ApplicationUser> GetAllUsers()
        {
            List<ApplicationUser> users;
            _userContext = new ApplicationDbContext();
            users = _userContext.Users.ToList();
            return users;
        }

        public List<ApplicationUser> GetPostUsers(string postid)
        {
            List<string> seenUsersIds = GetSeenUsernames(postid);
            List<ApplicationUser> allUsers = GetAllUsers();
            List<ApplicationUser> result = new List<ApplicationUser>();

            foreach (var id in seenUsersIds)
            {
                foreach (var ids in allUsers)
                {
                    if (id == ids.UserName)
                    {
                        result.Add(ids);
                    }
                }
            }
            return result;
        }

        public List<string> GetSeenUsernames(string postid)
        {

            List<string> userIds = new List<string>();
            var users = _context.PostSeen.Where(p => p.postId == postid).Select(p => p.Username).ToList();
            List<string> usersDistinct = users.Distinct().ToList();
            foreach (var us in usersDistinct)
            {
                userIds.Add(us);
            }

            return userIds;
        }

        public List<PostSeenModel> GetPostsSeen()
        {
            List<PostSeenModel> list = new List<PostSeenModel>();
            list = _context.PostSeen.ToList();
            return list;
        } 

        public IList<Category> GetPostCategories (Post post)
        {
            var categoryIds = _context.PostCategories.Where(p => p.PostId == post.Id).Select(p => p.CategoryId).ToList();
            List<Category> categories = new List<Category>();
            foreach (var catId in categoryIds)
            {
                categories.Add(_context.Categories.Where(p => p.Id == catId).FirstOrDefault());
            }
            return categories;  
        }


        public IList<Tag> GetPostTags(Post post)
        {
            var tagIds = _context.PostTags.Where(p => p.PostId == post.Id).Select(p => p.TagId).ToList();
            List<Tag> tags = new List<Tag>();
            foreach (var tagId in tagIds)
            {
                tags.Add(_context.Tags.Where(p => p.Id == tagId).FirstOrDefault());
            }
            return tags;
        }

        public IList<PostVideo> GetPostVideos(Post post)
        {
            var postUrls = _context.PostVideos.Where(p => p.PostId == post.Id).ToList();
            List<PostVideo> videos = new List<PostVideo>();
            foreach (var url in postUrls)
            {
                videos.Add(url);
            }
            return videos;
        }

        public int LikeDislikeCount(string typeAndLike, string Id)
        {
            switch (typeAndLike)
            {
                case "postlike":
                    return _context.PostLikes.Where(p => p.PostId == Id && p.Like == true).Count();
                case "postdislike":
                    return _context.PostLikes.Where(p => p.PostId == Id && p.Dislike == true).Count();
                case "commentlike":
                    return _context.CommentLikes.Where(p => p.CommentId == Id && p.Like == true).Count();
                case "commentdislike":
                    return _context.CommentLikes.Where(p => p.CommentId == Id && p.Dislike == true).Count();
                case "replylike":
                    return _context.ReplyLikes.Where(p => p.ReplyId == Id && p.Like == true).Count();
                case "replydislike":
                    return _context.ReplyLikes.Where(p => p.ReplyId == Id && p.Dislike == true).Count();
                default:
                    return 0;
            }
        }

        public IList<Tag> GetTags()
        {
            return _context.Tags.ToList();
        }

        public IList<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        public Post GetPostById(string id)
        {
            return _context.Posts.Find(id);
        }

        public string GetPostIdBySlug(string slug)
        {
            return _context.Posts.Where(x => x.UrlSeo == slug).FirstOrDefault().Id;
        }


        public void AddPostCategories  (PostCategory postCategory)
        {
            _context.PostCategories.Add(postCategory);
        }

        public void RemovePostCategories (string postid, string categoryid)
        {
            PostCategory postCategory = _context.PostCategories.Where(x => x.PostId == postid && x.CategoryId == categoryid).FirstOrDefault();
            _context.PostCategories.Remove(postCategory);
            Save();
        }

        public void RemoveCategoryFromPost(string postid, string catname)
        {
            var catid = _context.Categories.Where(x => x.Name == catname).Select(x => x.Id).FirstOrDefault();
            var cat = _context.PostCategories.Where(x => x.PostId == postid && x.CategoryId == catid).FirstOrDefault();
            _context.PostCategories.Remove(cat);
            Save();
        }

        public void AddNewCategory(string catName, string catUrlSeo, string catDesc)
        {
            List<int> numList = new List<int>();
            int num = 0;
            var categories = _context.Categories.ToList();
            foreach(var cat in categories)
            {
                var catid = cat.Id;
                Int32.TryParse(catid.Replace("cat", ""), out num);
                numList.Add(num);
            }
            numList.Sort();
            num = numList.Last();
            num++;
            var newid = "cat" + num.ToString();
            var category = new Category { Id = newid, Name = catName, Description = catDesc, UrlSeo = catUrlSeo, Checked = false };
            _context.Categories.Add(category);
            Save();
        }

        public void AddNewSeen (PostSeenModel seen)
        {
            _context.PostSeen.Add(seen);
            Save();
        }

        public void RemoveCategory(Category category)
        {
            var postCategories = _context.PostCategories.Where(x => x.CategoryId == category.Id).ToList();
            foreach(var postCat in postCategories)
            {
                _context.PostCategories.Remove(postCat);
            }
            _context.Categories.Remove(category);
            Save();
        }

        public void DeletePostAndComponents(string postid)
        {
            var postCategories = _context.PostCategories.Where(p => p.PostId == postid).ToList();
            var postLikes = _context.PostLikes.Where(p => p.PostId == postid).ToList();
            var postTags = _context.PostTags.Where(p => p.PostId == postid).ToList();
            var postVideos = _context.PostVideos.Where(p => p.PostId == postid).ToList();
            var postComments = _context.Comments.Where(p => p.PostId == postid).ToList();
            var postReplies = _context.Replies.Where(p => p.PostId == postid).ToList();
            var post = _context.Posts.Find(postid);
            foreach (var pc in postCategories) _context.PostCategories.Remove(pc);
            foreach (var pl in postLikes) _context.PostLikes.Remove(pl);
            foreach (var pt in postTags) _context.PostTags.Remove(pt);
            foreach (var pv in postVideos) _context.PostVideos.Remove(pv);
            foreach (var pcom in postComments)
            {
                var commentLikes = _context.CommentLikes.Where(x => x.CommentId == pcom.Id).ToList();
                foreach (var cl in commentLikes) _context.CommentLikes.Remove(cl);
                _context.Comments.Remove(pcom);
            }
            foreach (var pr in postReplies)
            {
                var replyLikes = _context.ReplyLikes.Where(x => x.ReplyId == pr.Id).ToList();
                foreach (var rl in replyLikes) _context.ReplyLikes.Remove(rl);
                _context.Replies.Remove(pr);
            }
            _context.Posts.Remove(post);
            Save();
        }

        public void AddNewPost(Post post)
        {
            _context.Posts.Add(post);
            Save();
        }

        public IList<Comment> GetPostComments (Post post)
        {
            return _context.Comments.Where(p => p.PostId == post.Id).ToList();
        }

        public List<CommentViewModel> GetParentReplies (Comment comment)
        {
            var parentReplies = _context.Replies.Where(p => p.CommentId == comment.Id && p.ParentReplyId == null).ToList();
            List<CommentViewModel> parReplies = new List<CommentViewModel>();
            foreach(var pr in parentReplies)
            {
                var chReplies = GetChildReplies(pr);
                parReplies.Add(new CommentViewModel()
                {
                    Body = pr.Body,
                    ParentReplyId = pr.ParentReplyId,
                    DateTime = pr.DateTime,
                    Id = pr.Id,
                    UserName = pr.UserName,
                    ChildReplies = chReplies
                });
            }

            return parReplies;
        }

        public List<CommentViewModel> GetChildReplies(Reply parentReply)
        {
            List<CommentViewModel> chldReplies = new List<CommentViewModel>();
            if(parentReply != null)
            {
                var childReplies = _context.Replies.Where(p => p.ParentReplyId == parentReply.Id).ToList();
                foreach(var reply in childReplies)
                {
                    var chReplies = GetChildReplies(reply);
                    chldReplies.Add(new CommentViewModel()
                    {
                        Body = reply.Body,
                        ParentReplyId = reply.ParentReplyId,
                        DateTime = reply.DateTime,
                        Id = reply.Id,
                        UserName = reply.UserName,
                        ChildReplies = chReplies
                    });
                }
            }
            return chldReplies;
        }

        public Reply GetReplyById(string id)
        {
            return _context.Replies.Where(p => p.Id == id).FirstOrDefault();
        }

        public bool CommentDeleteCheck(string commentId)
        {
            return _context.Comments.Where(x => x.Id == commentId).Select(x => x.Deleted).FirstOrDefault();
        }

        public bool ReplyDeleteCheck(string replyid)
        {
            return _context.Replies.Where(x => x.Id == replyid).Select(x => x.Deleted).FirstOrDefault();
        }

        public Post GetPostByReply(string replyid)
        {
            var postid = _context.Replies.Where(x => x.Id == replyid).Select(x => x.PostId).FirstOrDefault();
            return _context.Posts.Where(x => x.Id == postid).FirstOrDefault();
        }

        public IList<Comment> GetComments()
        {
            return _context.Comments.ToList();
        }

        public IList<Reply> GetReplies()
        {
            return _context.Replies.ToList();
        }

        public void AddNewComment(Comment comment)
        {
            _context.Comments.Add(comment);
            Save();
        }

        public void AddNewReply(Reply reply)
        {
            _context.Replies.Add(reply);
            Save();
        }

        public Comment GetCommentById(string id)
        {
            return _context.Comments.Where(p => p.Id == id).FirstOrDefault();
        }

        public void DeleteComment(string commentid)
        {
            var comment = _context.Comments.Where(x => x.Id == commentid).FirstOrDefault();
            _context.Comments.Remove(comment);
            Save();
        }

        
        public void DeleteReply(string replyid)
        {
            var reply = _context.Replies.Where(x => x.Id == replyid).FirstOrDefault();
            _context.Replies.Remove(reply);
            Save();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

    }
}