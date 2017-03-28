using CS348_CW2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CS348_CW2.DAL
{
    public interface IBlogRepository : IDisposable
    {
        IList<Post> GetPosts();
        IList<Category> GetPostCategories(Post post);
        IList<Tag> GetPostTags(Post post);
        IList<PostVideo> GetPostVideos(Post post);
        int LikeDislikeCount(string typeAndLike, string Id);
        IList<Tag> GetTags();
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