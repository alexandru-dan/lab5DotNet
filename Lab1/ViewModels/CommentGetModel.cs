using Lab1.Models;
using System.Collections.Generic;

namespace Lab1.ViewModels
{
    public class CommentGetModel
    {

        public int Id { get; set; }
        public string Text { get; set; }
        public bool Important { get; set; }
        public int? MovieId { get; set; }


        public static List<CommentGetModel> FromComments(List<Comment> commentsGetModel)
        {
            if (commentsGetModel == null)
            {
                return null;
            }

            List<CommentGetModel> comments = new List<CommentGetModel>();

            foreach (var item in commentsGetModel)
            {
                comments.Add(new CommentGetModel()
                {
                    Id = item.Id,
                    Text = item.Text,
                    Important = item.Important,
                    MovieId = item.Movie.Id
                });
            }

            return comments;
        }

        public static List<Comment> ToComments(List<CommentGetModel> commentsGetModel)
        {
            if (commentsGetModel == null)
            {
                return null;
            }

            List<Comment> comments = new List<Comment>();

            foreach (var item in commentsGetModel)
            {
                comments.Add(new Comment()
                {
                    Id = item.Id,
                    Text = item.Text,
                    Important = item.Important
                });
            }

            return comments;
        }
    }
}
