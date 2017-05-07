using FinalYearProject.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalYearProject.Repositories
{
    /// <summary>
    /// This class is the repository of methods that are used to access the forums features
    /// </summary>
    public class ForumRepository : IForumRepository, IDisposable
    {
        public BlogDbContext _context;
        public ApplicationDbContext _userContext;

        /// <summary>
        /// Creates an instance of the repository
        /// </summary>
        /// <param name="context">Takes the forum db context</param>
        public ForumRepository(BlogDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This methos is used to return the list of all posts in the database
        /// </summary>
        /// <returns>returns the list of posts</returns>
        public IList<Post> GetPosts()
        {
            return _context.Posts.ToList();
        }

        /// <summary>
        /// This method is used for retrieving all users in the application
        /// </summary>
        /// <returns>returns a list of application users</returns>
        public List<ApplicationUser> GetAllUsers()
        {
            List<ApplicationUser> users;
            _userContext = new ApplicationDbContext();
            users = _userContext.Users.ToList();
            return users;
        }

        /// <summary>
        /// This method is used to get all the student users in the system
        /// </summary>
        /// <returns>returns the list of all the student user usernames</returns>
        public List<string> GetAllStudentUsers()
        {
            List<ApplicationUser> allUsers = GetAllUsers();
            List<string> result = new List<string>();

            try
            {
                foreach (var user in allUsers)
                {
                    if (user.Role.Equals("Student"))
                    {
                        result.Add(user.UserName);
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return result;
        }


       /// <summary>
       /// This method is used to retrieve the users that have seen a post
       /// </summary>
       /// <param name="postid">the id of the post in question</param>
       /// <returns>returns the list of users</returns>
        public List<ApplicationUser> GetPostUsers(string postid)
        {
            List<string> seenUsersIds = GetSeenUsernames(postid);
            List<ApplicationUser> allUsers = GetAllUsers();
            List<ApplicationUser> result = new List<ApplicationUser>();
            try
            {
                foreach (var id in seenUsersIds)
                {
                    foreach (var ids in allUsers)
                    {
                        if (id == ids.UserName && ids.Role.Equals("Student"))
                        {

                            result.Add(ids);
                        }
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return result;
        }

        /// <summary>
        /// This method returns the list of all the users usernames that have,
        /// seen a particular post
        /// </summary>
        /// <param name="postid">the id of the post in question</param>
        /// <returns>list of usewr ids</returns>
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
            try
            {
                foreach (var catId in categoryIds)
                {
                    categories.Add(_context.Categories.Where(p => p.Id == catId).FirstOrDefault());
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return categories;  
        }

        /// <summary>
        /// This repository method is used for retrieving the categories
        /// </summary>
        /// <returns>returns the categories from the db context</returns>
        public IList<Category> GetCategories()
        {
            return _context.Categories.ToList();
        }

        /// <summary>
        /// This method is used for returning a post by its id
        /// </summary>
        /// <param name="id">id of the post</param>
        /// <returns>returns the correct post from the database</returns>
        public Post GetPostById(string id)
        {
            return _context.Posts.Find(id);
        }

        /// <summary>
        /// This method is used for returning a post id by its url slug
        /// </summary>
        /// <param name="slug">the url slug of the post</param>
        /// <returns>returns the post from the context</returns>
        public string GetPostIdBySlug(string slug)
        {
            return _context.Posts.Where(x => x.UrlSeo == slug).FirstOrDefault().Id;
        }

        /// <summary>
        /// This method is used for adding a category for posts
        /// </summary>
        /// <param name="postCategory">post category object</param>
        public void AddPostCategories (PostCategory postCategory)
        {
            _context.PostCategories.Add(postCategory);
        }

        /// <summary>
        /// This method is use for removing a post category
        /// </summary>
        /// <param name="postid">id of the post</param>
        /// <param name="categoryid">id of the category</param>
        public void RemovePostCategories (string postid, string categoryid)
        {
            try
            {
                PostCategory postCategory = _context.PostCategories.Where(x => x.PostId == postid && x.CategoryId == categoryid).FirstOrDefault();
                _context.PostCategories.Remove(postCategory);
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Save();
        }

        /// <summary>
        /// This method is used for removing a category that is already attached to posts
        /// </summary>
        /// <param name="postid">the id of the posts</param>
        /// <param name="catname">the name of the category</param>
        public void RemoveCategoryFromPost(string postid, string catname)
        {
            try
            {
                var catid = _context.Categories.Where(x => x.Name == catname).Select(x => x.Id).FirstOrDefault();
                var cat = _context.PostCategories.Where(x => x.PostId == postid && x.CategoryId == catid).FirstOrDefault();
                _context.PostCategories.Remove(cat);
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Save();
        }

        /// <summary>
        /// This method is used for adding a new category
        /// </summary>
        /// <param name="catName">the name of the new category</param>
        /// <param name="catUrlSeo">the url of the category</param>
        /// <param name="catDesc">the description of the category</param>
        public void AddNewCategory(string catName, string catUrlSeo, string catDesc)
        {
            List<int> numList = new List<int>();
            try
            {
                int num = 0;
                var categories = _context.Categories.ToList();
                foreach (var cat in categories)
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
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Save();
        }

        /// <summary>
        /// This method is used for indicating that a post has been seen,
        /// by a user
        /// </summary>
        /// <param name="seen">Post seen model paramater is passed</param>
        public void AddNewSeen (PostSeenModel seen)
        {
            _context.PostSeen.Add(seen);
            Save();
        }

        /// <summary>
        /// This method is used to remove a category
        /// </summary>
        /// <param name="category">a category object</param>
        public void RemoveCategory(Category category)
        {
            var postCategories = _context.PostCategories.Where(x => x.CategoryId == category.Id).ToList();
            try
            {
                foreach (var postCat in postCategories)
                {
                    _context.PostCategories.Remove(postCat);
                }
                _context.Categories.Remove(category);
            } catch (Exception e)
            {

            }
            
            Save();
        }

        /// <summary>
        /// This method is used for deleting a post along with all the data associated with it,
        /// such as its comments
        /// </summary>
        /// <param name="postid">the id of the post</param>
        public void DeletePostAndComponents(string postid)
        {
            try
            {
                var postCategories = _context.PostCategories.Where(p => p.PostId == postid).ToList();
                var postComments = _context.Comments.Where(p => p.PostId == postid).ToList();
                var postReplies = _context.Replies.Where(p => p.PostId == postid).ToList();
                var post = _context.Posts.Find(postid);
                foreach (var pc in postCategories) _context.PostCategories.Remove(pc);


                _context.Posts.Remove(post);
            } catch (Exception e)
            {
                Console.WriteLine(e);

            }
            
            
            Save();
        }

        /// <summary>
        /// This method is used for adding a new post
        /// </summary>
        /// <param name="post">the post object and its values</param>
        public void AddNewPost(Post post)
        {
            _context.Posts.Add(post);
            Save();
        }

        /// <summary>
        /// This method is used for retrieving the comments that belong to a post
        /// </summary>
        /// <param name="post">the post object</param>
        /// <returns>returns the comments from the database</returns>
        public IList<Comment> GetPostComments (Post post)
        {
            return _context.Comments.Where(p => p.PostId == post.Id).ToList();
        }

        /// <summary>
        /// This method is responsible for returning the parent comments for a post
        /// </summary>
        /// <param name="comment">the comment which the parent replies belong</param>
        /// <returns>returns the parent replies of the comment</returns>
        public List<CommentViewModel> GetParentReplies (Comment comment)
        {
            var parentReplies = _context.Replies.Where(p => p.CommentId == comment.Id && p.ParentReplyId == null).ToList();
            List<CommentViewModel> parReplies = new List<CommentViewModel>();
            try
            {
                foreach (var pr in parentReplies)
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
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            return parReplies;
        }

        /// <summary>
        /// This method is used for returning the child replies associated to a parent reply,
        /// for a comment
        /// </summary>
        /// <param name="parentReply">The parent reply in question</param>
        /// <returns>returns the child replies</returns>
        public List<CommentViewModel> GetChildReplies(Reply parentReply)
        {
            List<CommentViewModel> chldReplies = new List<CommentViewModel>();
            try
            {
                if (parentReply != null)
                {
                    var childReplies = _context.Replies.Where(p => p.ParentReplyId == parentReply.Id).ToList();
                    foreach (var reply in childReplies)
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
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return chldReplies;
        }

        /// <summary>
        /// This method is used for returning a reply by its id
        /// </summary>
        /// <param name="id">the id of the reply</param>
        /// <returns>the reply from the database</returns>
        public Reply GetReplyById(string id)
        {
            return _context.Replies.Where(p => p.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// This method ensures that a comment item is deleted from the database
        /// </summary>
        /// <param name="commentId">the comment id</param>
        /// <returns>returns the boolean value of the query</returns>
        public bool CommentDeleteCheck(string commentId)
        {
            return _context.Comments.Where(x => x.Id == commentId).Select(x => x.Deleted).FirstOrDefault();
        }

        /// <summary>
        /// This method is used for checking is a reply comment is deleted from the database
        /// </summary>
        /// <param name="replyid">the reply id</param>
        /// <returns>returns the boolean value of the query</returns>
        public bool ReplyDeleteCheck(string replyid)
        {
            return _context.Replies.Where(x => x.Id == replyid).Select(x => x.Deleted).FirstOrDefault();
        }

        /// <summary>
        /// This method is used for returning a post based on the comment reply
        /// </summary>
        /// <param name="replyid">the reply id</param>
        /// <returns>returns the post from the database</returns>
        public Post GetPostByReply(string replyid)
        {
            var postid = _context.Replies.Where(x => x.Id == replyid).Select(x => x.PostId).FirstOrDefault();
            return _context.Posts.Where(x => x.Id == postid).FirstOrDefault();
        }

        /// <summary>
        /// This method is responsible for getting all comments
        /// </summary>
        /// <returns>a list of comments</returns>
        public IList<Comment> GetComments()
        {
            return _context.Comments.ToList();
        }

        /// <summary>
        /// This method is used for returning all comment replies
        /// </summary>
        /// <returns>a list of replies</returns>
        public IList<Reply> GetReplies()
        {
            return _context.Replies.ToList();
        }

        /// <summary>
        /// This method is used to add a new comment to the database
        /// </summary>
        /// <param name="comment">the comment object</param>
        public void AddNewComment(Comment comment)
        {
            _context.Comments.Add(comment);
            Save();
        }

        /// <summary>
        /// This method is used for adding a new reply to the database
        /// </summary>
        /// <param name="reply">the reply object containing its values</param>
        public void AddNewReply(Reply reply)
        {
            _context.Replies.Add(reply);
            Save();
        }

        /// <summary>
        /// This method is responsible for returning a comment based on its id
        /// </summary>
        /// <param name="id">the id of the comment</param>
        /// <returns>returns the comment from the database context</returns>
        public Comment GetCommentById(string id)
        {
            return _context.Comments.Where(p => p.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// This method is used for removing a comment from the database
        /// </summary>
        /// <param name="commentid">The comment id</param>
        public void DeleteComment(string commentid)
        {
            var comment = _context.Comments.Where(x => x.Id == commentid).FirstOrDefault();
            _context.Comments.Remove(comment);
            Save();
        }

        /// <summary>
        /// This method is used for deleting a reply of a comment from the database
        /// </summary>
        /// <param name="replyid">the reply comment id</param>
        public void DeleteReply(string replyid)
        {
            var reply = _context.Replies.Where(x => x.Id == replyid).FirstOrDefault();
            _context.Replies.Remove(reply);
            Save();
        }

        private bool disposed = false;

        /// <summary>
        /// Dispose method used for garbage collection when using the ToDo table
        /// </summary>
        /// <param name="disposing">boolean to dictate if the disposing,
        /// is to take place or not</param>
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

        /// <summary>
        /// This method is used to dispose of the current context
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This method is used to save changes to the current database context
        /// </summary>
        public void Save()
        {
            _context.SaveChanges();
        }

    }
}