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
    public class BlogController : Controller
    {
        private IAnnouncementRepository _announcementRepository;
        public static List<BlogViewModel> postList = new List<BlogViewModel>();
        public static List<AllPostsViewModel> checkCatList = new List<AllPostsViewModel>();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public BlogController()
        {
            _announcementRepository = new AnnouncementRepository(new BlogDbContext());
        }

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

        [HttpGet]
        [Authorize(Roles = "Admin, Lecturer, Student")]
        public ActionResult Index(int? page, string sortOrder, string searchString, string[] searchCategory, string[] searchTag)
        {

            checkCatList.Clear();
            CreateCatAndTagList();
            Posts(page, sortOrder, searchString, searchCategory);
            return View();
        }

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
            foreach (var post in posts)
            {
                var postCategories = GetPostCategoires(post);
                
                postList.Add(new BlogViewModel() { Post = post, Modified = post.Modified, Title = post.Title, ShortDescription = post.ShortDescription, PostOn = post.PostedOn, Id = post.Id,  PostCategories = postCategories,UrlSlug = post.UrlSeo });
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

            int pageSize = 4;
            int pageNumber = (page ?? 1);

            return PartialView("Posts", postList.ToPagedList(pageNumber, pageSize));
        
    }

        


        [ChildActionOnly]
        public ActionResult Comments(PostViewModel model, Post post, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DateSortParm = string.IsNullOrEmpty(sortOrder) ? "date_asc" : "";

            var postComments = _announcementRepository.GetPostComments(post).OrderByDescending(d => d.DateTime).ToList();

            foreach(var comment in postComments)
            {
                if (comment.Replies != null) comment.Replies.Clear();
                List<CommentViewModel> replies = _announcementRepository.GetParentReplies(comment);
                foreach(var reply in replies)
                {
                    var rep = _announcementRepository.GetReplyById(reply.Id);
                    comment.Replies.Add(rep);
                }
            }

            if(sortOrder == "date_asc")
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
            return PartialView(model);
        }

        public PartialViewResult Replies()
        {
            return PartialView();
        }

        public PartialViewResult ChildReplies()
        {
            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewComment(string commentBody, string comUserName, string slug, string postid)
        {
            if (commentBody == "")
            {

            } else
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
            
            return RedirectToAction("Post", new { slug = slug });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewParentReply(string replyBody, string comUserName, string postid, string commentid, string slug)
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
            return RedirectToAction("Post", new { slug = slug });
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteReply(string replyid)
        {
            var reply = _announcementRepository.GetReplyById(replyid);
            var repliesList = _announcementRepository.GetChildReplies(reply);
            var postid = reply.PostId;
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
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == postid).FirstOrDefault().UrlSeo });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult NewChildReply(string preplyid, string comUserName, string replyBody)
        {
            var repDelCheck = ReplyDeleteCheck(preplyid);
            var preply = _announcementRepository.GetReplyById(preplyid);
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
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == preply.PostId).FirstOrDefault().UrlSeo });
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditComment(string commentid, string commentBody)
        {
            var comment = _announcementRepository.GetCommentById(commentid);
            comment.Body = commentBody;
            comment.DateTime = DateTime.Now;
            _announcementRepository.Save();
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == comment.PostId).FirstOrDefault().UrlSeo });
        }

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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(string commentid)
        {
            var comment = _announcementRepository.GetCommentById(commentid);
            var postid = comment.PostId;
            var repliesList = _announcementRepository.GetParentReplies(comment);
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
            return RedirectToAction("Post", new { slug = _announcementRepository.GetPosts().Where(x => x.Id == postid).FirstOrDefault().UrlSeo });
        }

        #region Helpers

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await UserManager.FindByIdAsync(User.Identity.GetUserId());
        }

        public List<CommentViewModel> GetChildReplies(Reply parentReply)
        {
            return _announcementRepository.GetChildReplies(parentReply);
        }

        public bool CommentDeleteCheck(string commentid)
        {
            return _announcementRepository.CommentDeleteCheck(commentid);
        }

        public bool ReplyDeleteCheck(string replyid)
        {
            return _announcementRepository.ReplyDeleteCheck(replyid);
        }

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

            return date;
        }

        public string[] NewCommentDetails(string username)
        {
            string[] newCommentDetails = new string[3];
            newCommentDetails[0] = "td" + username; //comText
            newCommentDetails[1] = "tdc" + username; //comTextdiv
            newCommentDetails[2] = "tb" + username; //comTextBtn
            return newCommentDetails;
        }

        public string[] CommentDetails(Comment comment)
        {
            string[] commentDetails = new string[17];
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
            return commentDetails;
        }

        public string[] ReplyDetails(string replyId)
        {
            string[] replyDetails = new string[17];
            var reply = _announcementRepository.GetReplyById(replyId);
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

            return replyDetails;
        }

        public IList<Post> GetPosts()
        {
            return _announcementRepository.GetPosts();
        }

        public IList<Category> GetPostCategoires(Post post)
        {
            return _announcementRepository.GetPostCategories(post);
        }

        

        public void CreateCatAndTagList()
        {
            foreach (var ct in _announcementRepository.GetCategories())
            {
                checkCatList.Add(new AllPostsViewModel { Category = ct, Checked = false });
            }
            
        }

        public PostViewModel CreatePostViewModel(string slug)
        {
            PostViewModel model = new PostViewModel();
            var postid = _announcementRepository.GetPostIdBySlug(slug);
            var post = _announcementRepository.GetPostById(postid);
            model.ID = postid;
            model.Title = post.Title;
            model.Body = post.Body;
            model.Meta = post.Meta;
            model.UrlSeo = post.UrlSeo;
            
            model.PostCategories = _announcementRepository.GetPostCategories(post).ToList();
            model.ShortDescription = post.ShortDescription;
            return model;
        }

        

        #endregion
        
        
        public bool AlreadySeen(string username, string postid)
        {
            bool flag = false;
            var usernames = _announcementRepository.GetSeenUsernames(postid);
            foreach (var user in usernames)
            {
                
                if (user.Equals(username))
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }
        
        public void SeenPost(string postid, string title)
        {
            PostSeenModel model = new PostSeenModel();
            var user = UserManager.FindById(User.Identity.GetUserId());

           if (AlreadySeen(user.UserName,postid)==true)
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
        }

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
           

            foreach (var user in users)
            {

                SeenUsers temp = new SeenUsers();
                temp.Username = user.UserName;
                model.Add(temp);

            }

            foreach (var i in model)
            {
                
                //System.Diagnostics.Debug.WriteLine(i.Username);
                haveSeenPostUsername.Add(i.Username);
            }


            List<string> ThirdList = allStudentUsernames.Except(haveSeenPostUsername).ToList();
            

            foreach (var f in ThirdList)
            {
                //System.Diagnostics.Debug.WriteLine("->" + f);
                SeenUsers temp = new SeenUsers();
                temp.Username = f;
                test.Add(temp);
            }

            return View(test);
        }

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

            return View(model);
        }


        [Authorize(Roles ="Admin, Lecturer")]
        [HttpGet]
        public ActionResult EditPost(string slug)
        {
            var model = CreatePostViewModel(slug);
            return View(model);
        }


        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditPost(PostViewModel model)
        {
            
            var post = _announcementRepository.GetPostById(model.ID);
            post.Body = model.Body;
            post.Title = model.Title;
            post.ShortDescription = model.ShortDescription;
            post.Meta = model.Meta;
            post.UrlSeo = model.UrlSeo;
            post.Modified = DateTime.Now;
            _announcementRepository.Save();

            return RedirectToAction("Post", new { slug = model.UrlSeo });
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public ActionResult AddCategoryToPost(string postid)
        {
            PostViewModel model = new PostViewModel();
            model.ID = postid;
            model.Categories = _announcementRepository.GetCategories();
            return View(model);
        }



        [Authorize(Roles = "Admin, Lecturer")]
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
            return RedirectToAction("EditPost", new { slug = post.UrlSeo });
        }

        [Authorize(Roles = "Admin, Lecturer")]
        public ActionResult RemoveCategoryFromPost(string slug, string postid, string catname)
        {
            CreatePostViewModel(slug);
            _announcementRepository.RemoveCategoryFromPost(postid, catname);
            return RedirectToAction("EditPost", new { slug = slug });
        }

        [Authorize(Roles = "Admin, Lecturer")]
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

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewCategory(string postid, string catName, string catUrlSeo, string catDesc)
        {
            if(postid != null)
            {
                _announcementRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("AddCategoryToPost", new { postid = postid });
            }
            else
            {
                _announcementRepository.AddNewCategory(catName, catUrlSeo, catDesc);
                return RedirectToAction("Categories", "Blog");
            }
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public ActionResult Categories()
        {
            checkCatList.Clear();
            CreateCatAndTagList();
            return View();
        }

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

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpGet]
        public ActionResult DeletePost(PostViewModel model, string postid)
        {
            model.ID = postid;
            return View(model);
        }

        [Authorize(Roles = "Admin, Lecturer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(string postId)
        {
            _announcementRepository.DeletePostAndComponents(postId);
            return RedirectToAction("Index", "Blog");
        }

        [Authorize(Roles = "Admin, Lecturer")]
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

        [Authorize(Roles = "Admin, Lecturer")]
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

            return RedirectToAction("Post", "Blog", new { slug = model.Title + model.ID });
        }

    }

}