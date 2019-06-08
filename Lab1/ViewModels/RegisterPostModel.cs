using Lab1.Models;
using Lab1.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Lab1.ViewModels
{
    public class RegisterPostModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required, StringLength(50, MinimumLength = 6)]
        public string Password { get; set; }
        public string UserRole { get; set; }


        public static User ToUser(RegisterPostModel registerPostModel)
        {
            UserRole userRole = new UserRole();
            userRole = Models.UserRole.Regular;

            return new User()
            {
                FirstName = registerPostModel.FirstName,
                LastName = registerPostModel.LastName,
                Email = registerPostModel.Email,
                Username = registerPostModel.Username,
                Password = UsersService.ComputeSha256Hash(registerPostModel.Password),
                DateRegistered = DateTime.Now
            };
        }

        public static User ToUpdateUser(User existingUser, RegisterPostModel registerPostModel, User connectedUser)
        {
            UserRole userRole = new UserRole();
            userRole = Models.UserRole.Regular;


            existingUser.FirstName = registerPostModel.FirstName;
            existingUser.LastName = registerPostModel.LastName;
            existingUser.Email = registerPostModel.Email;
            existingUser.Username = registerPostModel.Username;
            existingUser.Password = UsersService.ComputeSha256Hash(registerPostModel.Password);
            if (connectedUser.UserRole == Models.UserRole.Admin)
            {
                existingUser.UserRole = userRole;
            }
            if (connectedUser.UserRole == Models.UserRole.UserManager && ((DateTime.Now - connectedUser.DateRegistered).Days > 180))
            {
                existingUser.UserRole = userRole;
            }

            return existingUser;
        }
    }

}
