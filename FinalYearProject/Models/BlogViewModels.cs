using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FinalYearProject.Models
{
    /// <summary>
    /// The model used to structure data contained in a post
    /// </summary>
    public class Post
    {

        public string Id { get; set; }
        [Required(ErrorMessage = "Title is required")]
        [Display(Name = "Title")]
        public string Title { get; set; }

        
        [Display(Name = "ShortDescription")]
        public string ShortDescription { get; set; }

        
        [Display(Name = "Body")]
        public string Body { get; set; }

        [Required(ErrorMessage = "Meta is required")]
        [Display(Name = "Meta")]
        public string Meta { get; set; }

        [Required(ErrorMessage = "UrlSeo is required")]
        [Display(Name = "UrlSeo")]
        public string UrlSeo { get; set; }

        public bool Published { get; set; }

        
        public DateTime PostedOn { get; set; }
        public DateTime? Modified { get; set; }



        public ICollection<Comment> Comments { get; set; }
        public ICollection<Reply> Replies { get; set; }
        public ICollection<PostCategory> PostCategories { get; set; }

    }

    /// <summary>
    /// The model used to structure data contained in a category
    /// </summary>
    public class Category
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "UrlSeo is required")]
        [Display(Name = "UrlSeo")]
        public string UrlSeo { get; set; }

        [Required(ErrorMessage = "Category Description is required")]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public bool Checked { get; set; }
        public ICollection<PostCategory> PostCategories { get; set; }
    }

    /// <summary>
    /// The model used to structure data contained in a postcategory
    /// </summary>
    public class PostCategory
    {
        [Key]
        [Column(Order = 0)]
        public string PostId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string CategoryId { get; set; }

        public bool Checked { get; set; }
        public Post Post { get; set; }
        public Category Category { get; set; }
    }

    /// <summary>
    /// The model used to structure data contained in a comment
    /// </summary>
    public class Comment
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage = "Comment Body is required")]
        public string Body { get; set; }

        [DefaultValue(false)]
        public bool Deleted { get; set; }

        public Post Post { get; set; }
        public ICollection<Reply> Replies { get; set; }
        
    }

    /// <summary>
    /// The model used to structure data contained in a reply
    /// </summary>
    public class Reply
    {
        public string Id { get; set; }
        public string PostId { get; set; }
        public string CommentId { get; set; }
        public string ParentReplyId { get; set; }
        public DateTime DateTime { get; set; }
        public string UserName { get; set; }

        [Required(ErrorMessage = "Comment Reply Body is required")]
        public string Body { get; set; }

        [DefaultValue(false)]
        public bool Deleted { get; set; }

        public Post Post { get; set; }
        public Comment Comment { get; set; }
        
    }

    /// <summary>
    /// The view model used to create the view for a forum post
    /// </summary>
    public class BlogViewModel
    {
        public DateTime PostOn { get; set; }
        public DateTime? Modified { get; set; }
        public int PostDislikes { get; set; }
        public int PostLikes { get; set; }
        public int TotalPosts { get; set; }
        public List<string> Category { get; set; }
        public Post Post { get; set; }
        public string Id { get; set; }
        public string ShortDescription { get; set; }
        public string Title { get; set; }
        public IList<Category> PostCategories { get; set; }
        public string UrlSlug { get; set; }

    }

    /// <summary>
    /// The view model used to create the view for all forum posts
    /// </summary>
    public class AllPostsViewModel
    {
        public IList<Category> PostCategories { get; set; }
        public string PostId { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public Category Category { get; set; }
        public bool Checked { get; set; }
        public string UrlSlug { get; set; }
    }

    /// <summary>
    /// The view model used to create the view for a single post post
    /// </summary>
    public class PostViewModel
    {
        public string Body { get; set; }
        public string FirstPostId { get; set; }
        public string ID { get; set; }
        public string LastPostId { get; set; }
        public string NextPostSlug { get; set; }
        public int PostCount { get; set; }

        public string PreviousPostSlug { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        public IList<Category> PostCategories { get; set; }
        public string Meta { get; set; }
        public string UrlSeo { get; set; }
        public string ShortDescription { get; set; }

        public IList<Category> Categories { get; set; }

        public IList<Comment> Comments { get; set; }
    }

    /// <summary>
    /// The view model used to create the view for post comments
    /// </summary>
    public class CommentViewModel
    {
        public CommentViewModel() { }

        public CommentViewModel(Comment comment)
        {
            Comment = comment;
        }

        public Comment Comment { get; set; }
        public DateTime DateTime { get; set; }
        public IList<CommentViewModel> ChildReplies { get; set; }
        public string Body { get; set; }
        public string Id { get; set; }
        public string ParentReplyId { get; set; }
        public string UserName { get; set; }
    }

    /// <summary>
    /// The model is used to structure data for who has seen a post
    /// </summary>
    public class PostSeenModel
    {
        [Key]
        public string Id { get; set; }
        
        public string postId { get; set; }
        public string userId { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        
    }

    /// <summary>
    /// Model class for the seen users
    /// </summary>
    public class SeenUsers
    {
        public string Username { get; set; }
    }
}

