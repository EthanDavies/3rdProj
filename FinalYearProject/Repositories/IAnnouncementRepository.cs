using FinalYearProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinalYearProject.Repositories
{
    /// <summary>
    /// This is the interface used to set all the variables required to access to databse context
    /// </summary>
    public interface IAnnouncementRepository : IDisposable
    {
        IList<Post> GetPosts();
        IList<Category> GetPostCategories(Post post);
        
        IList<Category> GetCategories();

        Post GetPostById(string postid);
        string GetPostIdBySlug(string slug);

        void AddNewSeen(PostSeenModel seen);
        void AddPostCategories(PostCategory postCategory);
        void RemovePostCategories(string postid, string categoryid);
        void RemoveCategoryFromPost(string postid, string catname);
        void AddNewCategory(string catName, string CatUrlSeo, string catDesc);
        void RemoveCategory(Category category);
        void DeletePostAndComponents(string postid);
        void AddNewPost(Post post);

        IList<Comment> GetPostComments(Post post);
        List<string> GetAllStudentUsers();
        List<PostSeenModel> GetPostsSeen();
        List<ApplicationUser> GetPostUsers(string postid);
        List<string> GetSeenUsernames(string postid);
        List<ApplicationUser> GetAllUsers();
        List<CommentViewModel> GetParentReplies(Comment comment);
        List<CommentViewModel> GetChildReplies(Reply reply);
        Reply GetReplyById(string id);
        bool CommentDeleteCheck(string commentid);
        bool ReplyDeleteCheck(string replyid);

        IList<Comment> GetComments();
        IList<Reply> GetReplies();
        void AddNewComment(Comment comment);
        void AddNewReply(Reply reply);
        Comment GetCommentById(string id);
        void DeleteComment(string commentid);
        void DeleteReply(string replyid);


        void Save();
    }
}