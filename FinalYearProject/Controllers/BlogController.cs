using FinalYearProject.Repositories;
using FinalYearProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Globalization;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Diagnostics;
using System.Web.Security;

namespace FinalYearProject.Controllers
{
    /// <summary>
    /// This controller is responsible for dealing with the methods required,
    /// to operate the forum functionality
    /// </summary>
    public class BlogController : Controller
    {
        private IAnnouncementRepository _announcementRepository;
        public static List<BlogViewModel> postList = new List<BlogViewModel>();
        public static List<AllPostsViewModel> checkCatList = new List<AllPostsViewModel>();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        /// <summary>
        /// Creates an instance of the forum repository 
        /// </summary>
        public BlogController()
        {
            _announcementRepository = new AnnouncementRepository(new BlogDbContext());
        }

        /// <summary>
        /// Creates an instance of the forum repository interface
        /// </summary>
        /// <param name="blogRepository"></param>
        public BlogController(IAnnouncementRepository blogRepository)
        {
            _announcementRepository = blogRepository;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }


        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        /// <summary>
        /// This method is responsible for returning the index page of the forum, here the user will see,
        /// the forum posts dictated by sorting, searching and the page number
        /// </summary>
        /// <param name="page">The page that is to be viewed</param>
        /// <param name="sortOrder">The order in which the forum will be sorted</param>
        /// <param name="searchString">The search restrictions for the forum posts</param>
        /// <param name="searchCategory">The category which the post is to be searched</param>
        /// <param name="searchTag">The tag in which the post is to be searched</param>
        /// <returns>retruns the index with contraining the correct post</returns>
        [HttpGet]
        [Authorize(Roles = "Admin, Lecturer, Student")]
        public ActionResult Index(int? page, string sortOrder, string searchString, string[] searchCategory, string[] searchTag)
        {
            try
            {
                checkCatList.Clear();
                CreateCatAndTagList();
                Posts(page, sortOrder, searchString, searchCategory);
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return View();
        }

        /// <summary>
        /// This method is responsible for returning all the posts that are required and defined,
        /// by the search paramaters
        /// </summary>
        /// <param name="page">The page that is to be viewed</param>
        /// <param name="sortOrder">The order in which the forum will be sorted</param>
        /// <param name="searchString">The search restrictions for the forum posts</param>
        /// <param name="searchCategory">The category which the post is to be searched</param>
        /// <returns>returns a partial view containing the posts</returns>
        [ChildActionOnly]
        public ActionResult Posts(int? page, string sortOrder, string searchString, string[] searchCategory)
        {
            postList.Clear();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentSearchString = searchString;
            ViewBag.CurrentSearchCategory = searchCategory;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewBag.TitleSortParm = sortOrder == "Title" ? "title_desc" : "Title";


            var posts = _announcementRepository.GetPosts();
            try
            {
                foreach (var post in posts)
                {
                    var postCategories = GetPostCategoires(post);

                    postList.Add(new BlogViewModel() { Post = post, Modified = post.Modified, Title = post.Title, ShortDescription = post.ShortDescription, PostOn = post.PostedOn, Id = post.Id, PostCategories = postCategories, UrlSlug = post.UrlSeo });
                }

                if (searchString != null)
                {
                    postList = postList.Where(x => x.Title.ToLower().Contains(searchString.ToLower())).ToList();
                }

                if (searchCategory != null)
                {
                    List<BlogViewModel> newlist = new List<BlogViewModel>();
                    foreach (var catName in searchCategory)
                    {
                        foreach (var item in postList)
                        {
                            if (item.PostCategories.Where(x => x.Name == catName).Any())
                            {
                                newlist.Add(item);
                            }
                        }
                        foreach (var item in checkCatList)
                        {
                            if (item.Category.Name == catName)
                            {
                                item.Checked = true;
                            }
                        }
                    }
                    postList = postList.Intersect(newlist).ToList();
                }



                switch (sortOrder)
                {
                    case "date_desc":
                        postList = postList.OrderByDescending(x => x.PostOn).ToList();
                        break;
                    case "Title":
                        postList = postList.OrderBy(x => x.Title).ToList();
                        break;
                    case "title_desc":
                        postList = postList.OrderByDescending(x => x.Title).ToList();
                        break;
                    default:
                        postList = postList.OrderBy(x => x.PostOn).ToList();
                        break;
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            int pageSize = 4;
            int pageNumber = (page ?? 1);

            return PartialView("Posts", postList.ToPagedList(pageNumber, pageSize));
        
    }

        

        /// <summary>
        /// This method is responsible for rendering the comments that can appear on each post,
        /// in the forum
        /// </summary>
        /// <param name="model">The post view model that contains,
        /// the comments viem model</param>
        /// <param name="post">The post who's comments are to be loaded</param>
        /// <param name="sortOrder">The current sort order</param>
        /// <returns>returns the partial view of the comments</returns>
        [ChildActionOnly]
        public ActionResult Comments(PostViewModel model, Post post, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";

            var postComments = _announcementRepository.GetPostComments(post).OrderByDescending(d => d.DateTime).ToList();
            try
            {
                foreach (var comment in postComments)
                {
                    if (comment.Replies != null) comment.Replies.Clear();
                    List<CommentViewModel> replies = _announcementRepository.GetParentReplies(comment);
                    foreach (var reply in replies)
                    {
                        var rep = _announcementRepository.GetReplyById(reply.Id);
                        comment.Replies.Add(rep);
                    }
                }

                if (sortOrder == "date_asc")
                {
                    postComments = postComments.OrderBy(x => x.DateTime).ToList();
                    ViewBag.DateSortLink = "active";
                }
                else
                {
                    postComments = postComments.OrderByDescending(x => x.DateTime).ToList();
                    ViewBag.DateSortLink = "active";
                }

                model.UrlSeo = post.UrlSeo;
                model.Comments = postComments;
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
            return PartialView(model);
        }

        /// <summary>
        /// This method is responsible for rendering the partial view or the repies,
        /// that each comment post may contain
        /// </summary>
        /// <returns>returns the partial view of replies</returns>
        public PartialViewResult Replies()
        {
            return PartialView();
        }

        /// <summary>
        /// This method is responsible for retruning the child replies,
        /// that are associated with each comment reply
        /// </summary>
        /// <returns>returns the partial view containing child replies</returns>
        public PartialViewResult ChildReplies()
        {
            return PartialView();
        }

        /// <summary>
        /// This method is used to create a new comment that is to be added and saved,
        /// to a forum post
        /// </summary>
        /// <param name="commentBody">The content of the comment</param>
        /// <param name="comUserName">the user who left the comment</param>
        /// <param name="slug">the slug url of the post</param>
        /// <param name="postid">the id of the post</param>
        /// <returns>retunrs the post containg the new comment</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewComment(string commentBody, string comUserName, string slug, string postid)
        {
            try
            {
                if (commentBody == "")
                {

                }
                else
                {
                    List<int> numlist = new List<int>();
                    int num = 0;
                    var comments = _announcementRepository.GetComments().ToList();
                    if (comments.Count() != 0)
                    {
                        foreach (var cmnt in comments)
                        {
                            var comid = cmnt.Id;
                            Int32.TryParse(comid.Replace("cmt", ""), out num);
                            numlist.Add(num);
                        }
                        numlist.Sort();
                        num = numlist.Last();
                        num++;
                    }
                    else
                    {
                        num = 1;
                    }
                    var newid = "cmt" + num.ToString();
                    var comment = new Comment()
                    {
                        Id = newid,
                        PostId = postid,
                        DateTime = DateTime.Now,
                        UserName = comUserName,
                        Body = commentBody,
                    };
                    _announcementRepository.AddNewComment(comment);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            
            return RedirectToAction("Post", new { slug = slug });
        }

        /// <summary>
        /// This method allows the creation of parent comment reply
        /// </summary>
        /// <param name="replyBody">The content of the reply comment</param>
        /// <param name="comUserName">the user who left the comment</param>
        /// <param name="postid">the id of the post in which the comment will be left</param>
        /// <param name="commentid">the id of the comment that is being replied to</param>
        /// <param name="slug">the url slug of the post</param>
        /// <returns>retunrs the post containg the new parent comment</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewParentReply(string replyBody, string comUserName, string postid, string commentid, string slug)
        {

            try
            {
                var comDelChck = CommentDeleteCheck(commentid);
                if (!comDelChck)
                {
                    List<int> numlist = new List<int>();
                    int num = 0;
                    var replies = _announcementRepository.GetReplies().ToList();
                    if (replies.Count != 0)
                    {
                        foreach (var rep in replies)
                        {
                            var repid = rep.Id;
                            Int32.TryParse(repid.Replace("rep", ""), out num);
                            numlist.Add(num);
                        }
                        numlist.Sort();
                        num = numlist.Last();
                        num++;
                    }
                    else
                    {
                        num = 1;
                    }
                    var newid = "rep" + num.ToString();
                    var reply = new Reply()
                    {
                        Id = newid,
                        PostId = postid,
                        CommentId = commentid,
                        ParentReplyId = null,
                        DateTime = DateTime.Now,
                        UserName = comUserName,
                        Body = replyBody,
                    };
                    _announcementRepository.AddNewReply(reply);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return RedirectToAction("Post", new { slug = slug });
        }


        /// <summary>
        /// This method allows the user to delete a comment reply that they have posted
        /// </summary>
        /// <param name="model">the comment view model</param>
        /// <param name="replyid">the id of the reply in quesiton</param>
        /// <returns>redirects to post view</returns>
        [HttpGet]
        public async Task<ActionResult> DeleteReply(CommentViewModel model, string replyid)
        {
            var user = await GetCurrentUserAsync();
            var reply = _announcementRepository.GetReplyById(replyid);
            if (reply.UserName == user.UserName)
            {
                model.Id = replyid;
                return View(model);
            }
            else
            {
                return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == reply.PostId).FirstOrDefault().UrlSeo });
            }
        }

        /// <summary>
        /// This method is responsible for supplying the logic that is required to delete the comment,
        /// from the forum database, once deleted the updated view is shown
        /// </summary>
        /// <param name="replyid">This is the id of the comment in question</param>
        /// <returns>redirects the user to the post with updated comments</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReply(string replyid)
        {
            var reply = _announcementRepository.GetReplyById(replyid);
            var repliesList = _announcementRepository.GetChildReplies(reply);
            var postid = reply.PostId;
            try
            {
                if (repliesList.Count() == 0)
                {
                    _announcementRepository.DeleteReply(replyid);
                }
                else
                {
                    reply.DateTime = DateTime.Now;
                    reply.Body = "<p style=\"color:red;\"><i>This comment has been deleted.</i></p>";
                    reply.Deleted = true;
                    _announcementRepository.Save();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == postid).FirstOrDefault().UrlSeo });
        }


        /// <summary>
        /// This method allows the creation of a new child reply to a reply comment,
        /// that has already been left on a post
        /// </summary>
        /// <param name="preplyid">reply of the parent comment</param>
        /// <param name="comUserName">user to which the comment will belong</param>
        /// <param name="replyBody">the content of the comment</param>
        /// <returns>redirects the user to the post with updated comments</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewChildReply(string preplyid, string comUserName, string replyBody)
        {
            var repDelCheck = ReplyDeleteCheck(preplyid);
            var preply = _announcementRepository.GetReplyById(preplyid);
            try
            {
                if (!repDelCheck)
                {
                    List<int> numlist = new List<int>();
                    int num = 0;
                    var replies = _announcementRepository.GetReplies().ToList();
                    if (replies.Count != 0)
                    {
                        foreach (var rep in replies)
                        {
                            var repid = rep.Id;
                            Int32.TryParse(repid.Replace("rep", ""), out num);
                            numlist.Add(num);
                        }
                        numlist.Sort();
                        num = numlist.Last();
                        num++;
                    }
                    else
                    {
                        num = 1;
                    }
                    var newid = "rep" + num.ToString();
                    var reply = new Reply()
                    {
                        Id = newid,
                        PostId = preply.PostId,
                        CommentId = preply.CommentId,
                        ParentReplyId = preply.Id,
                        DateTime = DateTime.Now,
                        UserName = comUserName,
                        Body = replyBody,
                    };
                    _announcementRepository.AddNewReply(reply);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == preply.PostId).FirstOrDefault().UrlSeo });
        }


        /// <summary>
        /// This method allows the user to edit a comment that they have left on a post
        /// </summary>
        /// <param name="model">The comment view model</param>
        /// <param name="commentid">the id of the comment that is to be deleted</param>
        /// <returns>redirect to post with updated comments</returns>
        [HttpGet]
        public async Task<ActionResult> EditComment(CommentViewModel model, string commentid)
        {
            var user = await GetCurrentUserAsync();
            var comment = _announcementRepository.GetCommentById(commentid);
            if (comment.UserName == user.UserName)
            {
                model.Id = commentid;
                model.Body = comment.Body;
                return View(model);
            }
            else
            {
                return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == comment.PostId).FirstOrDefault().UrlSeo });
            }

        }

        /// <summary>
        /// This controller method contains the logic required to change the comments,
        /// values in the forum database
        /// </summary>
        /// <param name="commentid">the id of the comment</param>
        /// <param name="commentBody">the new comment body</param>
        /// <returns>redirect to post with updated comments</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditComment(string commentid, string commentBody)
        {
            var comment = _announcementRepository.GetCommentById(commentid);
            try
            {
                comment.Body = commentBody;
                comment.DateTime = DateTime.Now;
                _announcementRepository.Save();
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == comment.PostId).FirstOrDefault().UrlSeo });
        }


        /// <summary>
        /// This method allows the user to delete a comment they have posted on a post
        /// </summary>
        /// <param name="model">The comment view model</param>
        /// <param name="commentid">The id of the comment to be deleted</param>
        /// <returns>redirect to post with updated comments</returns>
        [HttpGet]
        public async Task<ActionResult> DeleteComment(CommentViewModel model, string commentid)
        {
            var user = await GetCurrentUserAsync();
            var comment = _announcementRepository.GetCommentById(commentid);
            if (comment.UserName == user.UserName)
            {
                model.Id = commentid;
                return View(model);
            }
            else
            {
                return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == comment.PostId).FirstOrDefault().UrlSeo });
            }
        }

        /// <summary>
        /// This method contains the logic required to delete the comment from the database
        /// </summary>
        /// <param name="commentid">The comment id that is to be deleted</param>
        /// <returns>redirect to post with updated comments</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(string commentid)
        {
            var comment = _announcementRepository.GetCommentById(commentid);
            var postid = comment.PostId;
            var repliesList = _announcementRepository.GetParentReplies(comment);

            try
            {
                if (repliesList.Count() == 0)
                {
                    _announcementRepository.DeleteComment(commentid);
                }
                else
                {
                    comment.DateTime = DateTime.Now;
                    comment.Body = "<p style=\"color:red;\"><i>This comment has been deleted.</i></p>";
                    comment.Deleted = true;
                    _announcementRepository.Save();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == postid).FirstOrDefault().UrlSeo });
        }

        #region Helpers

        /// <summary>
        /// This method is used for retrieving the current user
        /// </summary>
        /// <returns>returns the current user</returns>
        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await UserManager.FindByIdAsync(User.Identity.GetUserId());
        }

        /// <summary>
        /// This method is used for returning the child replies
        /// </summary>
        /// <param name="parentReply">the parent to reply</param>
        /// <returns>returns the child replies of the parent</returns>
        public List<CommentViewModel> GetChildReplies(Reply parentReply)
        {
            return _announcementRepository.GetChildReplies(parentReply);
        }

        /// <summary>
        /// A check that is in place to ensure a comment has been deleted
        /// </summary>
        /// <param name="commentid">The comment id in question</param>
        /// <returns>the result of the query</returns>
        public bool CommentDeleteCheck(string commentid)
        {
            return _announcementRepository.CommentDeleteCheck(commentid);
        }

        /// <summary>
        /// This method is used to check is a comment reply has been deleted
        /// </summary>
        /// <param name="replyid">the reply id of the comment in question</param>
        /// <returns>the result of the query</returns>
        public bool ReplyDeleteCheck(string replyid)
        {
            return _announcementRepository.ReplyDeleteCheck(replyid);
        }

        /// <summary>
        /// This method is used to calculate the time that has passed since,
        /// the date the post was originally posted to the forum
        /// </summary>
        /// <param name="postDate">The post date</param>
        /// <returns>returns the time passed</returns>
        public static string TimePassed(DateTime postDate)
        {
            string date = null;
            double dateDiff = 0.0;
            var timeDiff = DateTime.Now - postDate;
            var yearPassed = timeDiff.TotalDays / 365;
            var monthPassed = timeDiff.TotalDays / 30;
            var dayPassed = timeDiff.TotalDays;
            var hourPassed = timeDiff.TotalHours;
            var minutePassed = timeDiff.TotalMinutes;
            var secondPassed = timeDiff.TotalSeconds;
            try
            {
                if (Math.Floor(yearPassed) > 0)
                {
                    dateDiff = Math.Floor(yearPassed);
                    date = dateDiff == 1 ? dateDiff + " year ago" : dateDiff + " years ago";
                }
                else
                {
                    if (Math.Floor(monthPassed) > 0)
                    {
                        dateDiff = Math.Floor(monthPassed);
                        date = dateDiff == 1 ? dateDiff + " month ago" : dateDiff + " months ago";
                    }
                    else
                    {
                        if (Math.Floor(dayPassed) > 0)
                        {
                            dateDiff = Math.Floor(dayPassed);
                            date = dateDiff == 1 ? dateDiff + " day ago" : dateDiff + " days ago";
                        }
                        else
                        {
                            if (Math.Floor(hourPassed) > 0)
                            {
                                dateDiff = Math.Floor(hourPassed);
                                date = dateDiff == 1 ? dateDiff + " hour ago" : dateDiff + " hours ago";
                            }
                            else
                            {
                                if (Math.Floor(minutePassed) > 0)
                                {
                                    dateDiff = Math.Floor(minutePassed);
                                    date = dateDiff == 1 ? dateDiff + " minute ago" : dateDiff + " minutes ago";
                                }
                                else
                                {
                                    dateDiff = Math.Floor(secondPassed);
                                    date = dateDiff == 1 ? dateDiff + " second ago" : dateDiff + " seconds ago";
                                }
                            }
                        }
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }

            

            return date;
        }


        /// <summary>
        /// This method is responsible for the returning the details of a new comment
        /// </summary>
        /// <param name="username">the username of the user</param>
        /// <returns>returns string array with comment details</returns>
        public string[] NewCommentDetails(string username)
        {
            string[] newCommentDetails = new string[3];
            try
            {
                newCommentDetails[0] = "td" + username; //comText
                newCommentDetails[1] = "tdc" + username; //comTextdiv
                newCommentDetails[2] = "tb" + username; //comTextBtn
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return newCommentDetails;
        }


        /// <summary>
        /// This method is responsible for showing the comment details for a,
        /// particular comment
        /// </summary>
        /// <param name="comment">A comment object</param>
        /// <returns>returns the string array of comment details</returns>
        public string[] CommentDetails(Comment comment)
        {
            string[] commentDetails = new string[17];
            try
            {
                commentDetails[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(comment.UserName);//username
                commentDetails[1] = "/Content/images/profile/" + commentDetails[0] + ".png?time=" + DateTime.Now.ToString();//imgUrl
                commentDetails[2] = TimePassed(comment.DateTime);//passed time
                commentDetails[3] = comment.DateTime.ToLongDateString().Replace(comment.DateTime.DayOfWeek.ToString() + ", ", "");//comment date
                commentDetails[4] = "gp" + comment.Id; //grandparentid
                commentDetails[5] = "mc" + comment.Id; //maincommentId
                commentDetails[6] = "crp" + comment.Id; //repliesId
                commentDetails[7] = "cex" + comment.Id; //commentExpid
                commentDetails[8] = "ctex" + comment.Id; //ctrlExpId
                commentDetails[9] = "ctflg" + comment.Id; //ctrlFlagId
                commentDetails[10] = "sp" + comment.Id; //shareParentId
                commentDetails[11] = "sc" + comment.Id; //shareChildId
                commentDetails[12] = "td" + comment.Id; //comText
                commentDetails[13] = "tdc" + comment.Id; //comTextdiv
                commentDetails[14] = "rpl" + comment.Id; //Reply
                commentDetails[15] = "cc1" + comment.Id; //commentControl
                commentDetails[16] = "cc2" + comment.Id; //commentMenu
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return commentDetails;
        }


        /// <summary>
        /// This method is responsible for returning details of a reply comment,
        /// through using its reply id
        /// </summary>
        /// <param name="replyId">The id of the reply</param>
        /// <returns>returns the string array of comment details</returns>
        public string[] ReplyDetails(string replyId)
        {
            string[] replyDetails = new string[17];
            var reply = _announcementRepository.GetReplyById(replyId);
            try
            {
                replyDetails[0] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(reply.UserName);//username
                replyDetails[1] = "/Content/images/profile/" + replyDetails[0] + ".png?time=" + DateTime.Now.ToString(); //imgUrl
                replyDetails[2] = TimePassed(reply.DateTime); //passed time
                replyDetails[3] = reply.DateTime.ToLongDateString().Replace(reply.DateTime.DayOfWeek.ToString() + ", ", ""); //reply date
                replyDetails[4] = "gp" + replyId; //grandparentid
                replyDetails[5] = "rp" + replyId; //parentreplyId
                replyDetails[6] = "crp" + replyId; //repliesId
                replyDetails[7] = "cex" + replyId; //commentExpid
                replyDetails[8] = "ctex" + replyId; //ctrlExpId
                replyDetails[9] = "ctflg" + replyId; //ctrlFlagId
                replyDetails[10] = "sp" + replyId; //shareParentId
                replyDetails[11] = "sc" + replyId; //shareChildId;
                replyDetails[12] = "td" + replyId; //comText
                replyDetails[13] = "tdc" + replyId; //comTextdiv
                replyDetails[14] = "rpl" + replyId; //Reply
                replyDetails[15] = "cc1" + replyId; //commentControl
                replyDetails[16] = "cc2" + replyId; //commentMenu
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            return replyDetails;
        }


        /// <summary>
        /// Gets all the forum posts
        /// </summary>
        /// <returns>returns the posts</returns>
        public IList<Post> GetPosts()
        {
            return _announcementRepository.GetPosts();
        }

        /// <summary>
        /// Gets the categories associated with the posts
        /// </summary>
        /// <param name="post">The post in question</param>
        /// <returns>The categories for that post</returns>
        public IList<Category> GetPostCategoires(Post post)
        {
            return _announcementRepository.GetPostCategories(post);
        }

        /// <summary>
        /// Creates the list of categories and tags for the posts
        /// </summary>
        public void CreateCatAndTagList()
        {
            foreach (var ct in _announcementRepository.GetCategories())
            {
                checkCatList.Add(new AllPostsViewModel { Category = ct, Checked = false });
            }
            
        }

        /// <summary>
        /// This method is responsible for returning the model of the post
        /// </summary>
        /// <param name="slug">the url slug for the post</param>
        /// <returns>returns the post view model and its values</returns>
        public PostViewModel CreatePostViewModel(string slug)
        {
            PostViewModel model = new PostViewModel();
            var postid = _announcementRepository.GetPostIdBySlug(slug);
            var post = _announcementRepository.GetPostById(postid);
            try
            {
                model.ID = postid;
                model.Title = post.Title;
                model.Body = post.Body;
                model.Meta = post.Meta;
                model.UrlSeo = post.UrlSeo;

                model.PostCategories = _announcementRepository.GetPostCategories(post).ToList();
                model.ShortDescription = post.ShortDescription;
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return model;
        }

        

        #endregion
        
        /// <summary>
        /// This method indicates which users have already viewed a post
        /// </summary>
        /// <param name="username">The username of the user</param>
        /// <param name="postid">the post viewed</param>
        /// <returns>returns the boolean result</returns>
        public bool AlreadySeen(string username, string postid)
        {
            bool flag = false;
            var usernames = _announcementRepository.GetSeenUsernames(postid);
            try
            {
                foreach (var user in usernames)
                {

                    if (user.Equals(username))
                    {
                        flag = true;
                        break;
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return flag;
        }
        
        /// <summary>
        /// This method is used to flag that a user has viewed a forum post
        /// </summary>
        /// <param name="postid">the post id that is viewed</param>
        /// <param name="title">the title of the post</param>
        public void SeenPost(string postid, string title)
        {
            PostSeenModel model = new PostSeenModel();
            var user = UserManager.FindById(User.Identity.GetUserId());
            try
            {
                if (AlreadySeen(user.UserName, postid) == true)
                {

                }
                else
                {
                    if (user.Role.Equals("Student"))
                    {

                        List<int> numlist = new List<int>();
                        int num = 0;
                        var posts = _announcementRepository.GetPostsSeen();
                        if (posts.Count != 0)
                        {
                            foreach (var post in posts)
                            {
                                var id = post.Id;
                                Int32.TryParse(id, out num);
                                numlist.Add(num);
                            }
                            numlist.Sort();
                            num = numlist.Last();
                            num++;

                        }
                        else
                        {

                            num = 1;
                        }
                        var newid = num.ToString();
                        model.Id = newid;
                        model.postId = postid;
                        model.userId = user.Id;
                        model.Username = user.UserName;
                        model.Title = title;
                        _announcementRepository.AddNewSeen(model);

                    }
                }

            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        /// <summary>
        /// This method allows authorized users to see who has viewed a post
        /// </summary>
        /// <param name="postid">the post id in question</param>
        /// <returns>returns the view showing the users who have seen the post</returns>
        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public ActionResult PostSeen(string postid)
        {
            List<SeenUsers> model = new List<SeenUsers>();
            List<SeenUsers> test = new List<SeenUsers>();
            List<ApplicationUser> users = _announcementRepository.GetPostUsers(postid);
            List<string> haveSeenPostUsername = new List<string>();
            List<string> allStudentUsernames = _announcementRepository.GetAllStudentUsers();
            List<string> hope = new List<string>();
           
            try
            {
                foreach (var user in users)
                {

                    SeenUsers temp = new SeenUsers();
                    temp.Username = user.UserName;
                    model.Add(temp);

                }

                foreach (var i in model)
                {

                    haveSeenPostUsername.Add(i.Username);
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            


            List<string> ThirdList = allStudentUsernames.Except(haveSeenPostUsername).ToList();
            
            try
            {
                foreach (var f in ThirdList)
                {
                    //System.Diagnostics.Debug.WriteLine("->" + f);
                    SeenUsers temp = new SeenUsers();
                    temp.Username = f;
                    test.Add(temp);
                }

            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            return View(test);
        }

        /// <summary>
        /// This controller method is used to retrieve a particular post
        /// </summary>
        /// <param name="sortOrder">The sort order specified</param>
        /// <param name="slug">the url slug of the post</param>
        /// <returns>returns a post view model with required posts values</returns>
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Post(string sortOrder,string slug)
        {
            PostViewModel model = new PostViewModel();
            var posts = GetPosts();
            var postid = _announcementRepository.GetPostIdBySlug(slug);
            var post = _announcementRepository.GetPostById(postid);
            var firstPostId = posts.OrderBy(i => i.PostedOn).First().Id;
            var lastPostId = posts.OrderBy(i => i.PostedOn).Last().Id;
            var nextId = posts.OrderBy(i => i.PostedOn).SkipWhile(i => i.Id != postid).Skip(1).Select(i => i.Id).FirstOrDefault();
            var previousId = posts.OrderBy(i => i.PostedOn).TakeWhile(i => i.Id != postid).Select(i => i.Id).LastOrDefault();
            try
            {
                model.FirstPostId = firstPostId;
                model.LastPostId = lastPostId;
                model.PreviousPostSlug = posts.Where(x => x.Id == previousId).Select(x => x.UrlSeo).FirstOrDefault();
                model.NextPostSlug = posts.Where(x => x.Id == nextId).Select(x => x.UrlSeo).FirstOrDefault();
                model.ID = post.Id;
                model.PostCount = posts.Count();
                model.UrlSeo = post.UrlSeo;
                model.Title = post.Title;
                model.Body = post.Body;
                Comments(model, post, sortOrder);
                SeenPost(postid, post.Title);
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
            

            return View(model);
        }


        /// <summary>
        /// This method allows the user to edit a post
        /// </summary>
        /// <param name="slug">the url slug of the post</param>
        /// <returns>the view with the post model</returns>
        [Authorize(Roles ="Admin, Lecturer")]
        [HttpGet]
        public ActionResult EditPost(string slug)
        {
            var model = CreatePostViewModel(slug);
            return View(model);
        }

        /// <summary>
        /// This POST method performs the logic required to edit a post
        /// </summary>
        /// <param name="model">the post view model</param>
        /// <returns>returns the user to the updated post</returns>
        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditPost(PostViewModel model)
        {
            
            var post = _announcementRepository.GetPostById(model.ID);
            try
            {
                post.Body = model.Body;
                post.Title = model.Title;
                post.ShortDescription = model.ShortDescription;
                post.Meta = model.Meta;
                post.UrlSeo = model.UrlSeo;
                post.Modified = DateTime.Now;
                _announcementRepository.Save();
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }
           

            return RedirectToAction("Post", new { slug = model.UrlSeo });
        }

        /// <summary>
        /// This method allows the user to add a category to a post
        /// </summary>
        /// <param name="postid">the id of the post</param>
        /// <returns>returns the post view model to the view</returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        [HttpGet]
        public ActionResult AddCategoryToPost(string postid)
        {
            PostViewModel model = new PostViewModel();
            model.ID = postid;
            model.Categories = _announcementRepository.GetCategories();
            return View(model);
        }

        /// <summary>
        /// This method is responsible for performing the logic of adding a category to a post
        /// </summary>
        /// <param name="model">The post view model</param>
        /// <returns>redirect to the post with updated category</returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCategoryToPost(PostViewModel model)
        {
            var post = _announcementRepository.GetPostById(model.ID);
            var postCats = _announcementRepository.GetPostCategories(post);
            List<string> pCatIds = new List<string>();
            foreach (var pCat in postCats)
            {
                pCatIds.Add(pCat.Id);
            }
            var newCats = model.Categories.Where(x => x.Checked == true).ToList();
            List<string> nCatIds = new List<string>();

            try
            {
                foreach (var pCat in newCats)
                {
                    nCatIds.Add(pCat.Id);
                }
                if (!pCatIds.SequenceEqual(nCatIds))
                {
                    foreach (var pCat in postCats)
                    {
                        _announcementRepository.RemovePostCategories(model.ID, pCat.Id);
                    }
                    foreach (var cat in model.Categories)
                    {
                        PostCategory postCategory = new PostCategory();
                        if (cat.Checked == true)
                        {
                            postCategory.PostId = model.ID;
                            postCategory.CategoryId = cat.Id;
                            postCategory.Checked = true;
                            _announcementRepository.AddPostCategories(postCategory);
                        }
                    }
                    _announcementRepository.Save();
                }
            } catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return RedirectToAction("Post", "Blog", new { slug = post.Title + post.Id });
        }

        /// <summary>
        /// This method allows the user to remove a category from a post
        /// </summary>
        /// <param name="slug">the url slug of the post</param>
        /// <param name="postid">the id of the post</param>
        /// <param name="catname">the category nbame</param>
        /// <returns>redriect to edit post</returns>
        [Authorize(Roles = "Admin, Lecturer")]
        public ActionResult RemoveCategoryFromPost(string slug, string postid, string catname)
        {
            CreatePostViewModel(slug);
            _announcementRepository.RemoveCategoryFromPost(postid, catname);
            return RedirectToAction("EditPost", new { slug = slug });
        }

        /// <summary>
        /// This method allows the user to add a new category for the posts on the forum
        /// </summary>
        /// <param name="postid">the post id</param>
        /// <param name="callfrompost">boolean flag</param>
        /// <returns>returns the relevant view</returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        [HttpGet]
        public ActionResult AddNewCategory(string postid, bool callfrompost)
        {
            if (postid != null && callfrompost)
            {
                PostViewModel model = new PostViewModel();
                model.ID = postid;
                return View(model);
            }
            else
            {
                return View();
            }
        }

        /// <summary>
        /// This method provides the logic for adding a new category
        /// </summary>
        /// <param name="postid">the post id in question</param>
        /// <param name="catName">the name of the category</param>
        /// <param name="catUrlSeo">the url of the category</param>
        /// <param name="catDesc">tge description of the category</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewCategory(string postid, string catName, string catUrlSeo, string catDesc)
        {
            if(postid != null)
            {
                catUrlSeo = catName;
                _announcementRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("AddCategoryToPost", new { postid = postid });
            }
            else
            {
                _announcementRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("Categories", "Blog");
            }
        }

        /// <summary>
        /// This method is resposible for returning the categories available
        /// </summary>
        /// <returns>returns the relevant view</returns>
        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public ActionResult Categories()
        {
            checkCatList.Clear();
            CreateCatAndTagList();
            return View();
        }

        /// <summary>
        /// This method is responsible for calling the logic of showing the categories
        /// </summary>
        /// <param name="categoryNames">string array of category names</param>
        /// <returns>redirects to relevant view</returns>
        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Categories(string[] categoryNames)
        {
            if (categoryNames != null)
            {
                foreach (var catName in categoryNames)
                {
                    var category = _announcementRepository.GetCategories().Where(x => x.Name == catName).FirstOrDefault();
                    _announcementRepository.RemoveCategory(category);
                }
            }          
            return RedirectToAction("Categories", "Blog");
        }

        /// <summary>
        /// This method is responsible for handling the deletion of a post
        /// </summary>
        /// <param name="model">the post view model</param>
        /// <param name="postid">the post id</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public ActionResult DeletePost(PostViewModel model, string postid)
        {
            model.ID = postid;
            return View(model);
        }

        /// <summary>
        /// This method is responsible for calling the logic required to delete a post,
        /// in the inteface
        /// </summary>
        /// <param name="postId">the post id</param>
        /// <returns>redirect to index forum action</returns>
        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(string postId)
        {
            _announcementRepository.DeletePostAndComponents(postId);
            return RedirectToAction("Index", "Blog");
        }

        /// <summary>
        /// This method is responsible for dealing with the adding of a new post
        /// </summary>
        /// <returns>returns the post viel model to the view</returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        [HttpGet]
        public ActionResult AddNewPost()
        {
            List<int> numlist = new List<int>();
            int num = 0;
            var posts = _announcementRepository.GetPosts();
            if (posts.Count != 0)
            {
                foreach (var post in posts)
                {
                    var postid = post.Id;
                    Int32.TryParse(postid, out num);
                    numlist.Add(num);
                }
                numlist.Sort();
                num = numlist.Last();
                num++;
            }
            else
            {
                num = 1;
            }
            var newid = num.ToString();
            PostViewModel model = new PostViewModel();
            model.ID = newid;
            
            return View(model);
        }


        /// <summary>
        /// This method is responsible for the logic of adding a new post
        /// </summary>
        /// <param name="model">The post view model</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin, Lecturer, Student")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddNewPost(PostViewModel model)
        {
            var post = new Post
            {

                Id = model.ID,
                Body = model.Body,
                Meta = model.Title,
                PostedOn = DateTime.Now,
                Published = true,
                ShortDescription = model.ShortDescription,
                Title = model.Title,
                UrlSeo = model.Title + model.ID
            };
            _announcementRepository.AddNewPost(post);
            AddCategoryToPost(post.Id);

            return RedirectToAction("AddCategoryToPost", "Blog", new { postid = model.ID });
        }

    }

}