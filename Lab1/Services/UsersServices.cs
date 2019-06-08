using Lab1.Models;
using Lab1.Services;
using Lab1.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Lab1.Services
{
    public interface IUsersService
    {
        UserGetModel Authenticate(string username, string password);
        User GetCurrentUser(HttpContext httpContext);

        IEnumerable<UserGetModel> GetAll();
        UserGetModel Register(RegisterPostModel registerInfo);
        User UpsertUser(int id, RegisterPostModel userRegister, User connectedUser);
        User DeleteUser(int id, User connectedUser);
        RegisterPostModel GetUserById(int id);
    }

    public class UsersService : IUsersService
    {
        private DataDbContext context;
        private readonly AppSettings appSettings;

        public UsersService(DataDbContext context, IOptions<AppSettings> appSettings)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public UserGetModel Authenticate(string username, string password)
        {
            var user = context.Users
                .SingleOrDefault(x => x.Username == username &&
                                 x.Password == ComputeSha256Hash(password));

            if (user == null)
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString()),
                    new Claim(ClaimTypes.Role, user.UserRole.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var result = new UserGetModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Token = tokenHandler.WriteToken(token),
                UserRole = user.UserRole.ToString()
            };
            return result;
        }

        public User GetCurrentUser(HttpContext httpContext)
        {
            string username = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value;

            return context.Users.FirstOrDefault(u => u.Username == username);
        }

        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            { 
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        public IEnumerable<UserGetModel> GetAll()
        {
            return context.Users.Select(user => new UserGetModel
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                UserRole = user.UserRole.ToString(),
                DateRegistered = user.DateRegistered
            });
        }

        public UserGetModel Register(RegisterPostModel registerInfo)
        {
            User existing = context.Users.FirstOrDefault(u => u.Username == registerInfo.Username);
            if (existing != null)
            {
                return null;
            }

            context.Users.Add(RegisterPostModel.ToUser(registerInfo));
            context.SaveChanges();

            return Authenticate(registerInfo.Username, registerInfo.Password);
        }

        public RegisterPostModel GetUserById(int id)
        {
            User user = context.Users.FirstOrDefault(c => c.Id == id);

            return new RegisterPostModel()
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                UserRole = user.UserRole.ToString()
            };
        } 

        public User DeleteUser(int id, User connectedUser)
        {
            User existing = context.Users.FirstOrDefault(u => u.Id == id);
            if (existing == null)
            {
                return null;
            }
            if (connectedUser.UserRole == UserRole.Admin)
            {
                return HelpMethodForDelete(existing);
            }
            //if(existing.UserRole == UserRole.UserManager))
            if (existing.UserRole == UserRole.UserManager && ((DateTime.Now - connectedUser.DateRegistered).Days > 180))
            {
                return HelpMethodForDelete(existing);
            }
            if (existing.UserRole != UserRole.Admin && existing.UserRole != UserRole.UserManager)
            {
                return HelpMethodForDelete(existing);
            }

            return null;
        }

        public User UpsertUser(int id, RegisterPostModel modifiedUser, User connectedUser)
        {
            var existing = context.Users.AsNoTracking().FirstOrDefault(c => c.Id == id);
            if (existing == null)
            {
                User user = RegisterPostModel.ToUser(modifiedUser);
                context.Users.Add(user);
                context.SaveChanges();
                return user;
            }
            if (connectedUser.UserRole == UserRole.Admin)
            {
                return HelpMethodForUpsert(existing, modifiedUser, connectedUser);
            }
            //if(existing.UserRole == UserRole.UserManager))
            if (existing.UserRole == UserRole.UserManager && ((DateTime.Now - connectedUser.DateRegistered).Days > 180))
            {
                return HelpMethodForUpsert(existing, modifiedUser, connectedUser);
            }
            if (existing.UserRole != UserRole.Admin && existing.UserRole != UserRole.UserManager)
            {
                return HelpMethodForUpsert(existing, modifiedUser, connectedUser);
            }

            return null;
        }

        private User HelpMethodForDelete(User existing)
        {
            context.Users.Remove(existing);
            context.SaveChanges();
            return existing;
        }
        private User HelpMethodForUpsert(User existing, RegisterPostModel modifiedUser, User connectedUser)
        {
            User updated = RegisterPostModel.ToUpdateUser(existing, modifiedUser, connectedUser);
            context.Users.Update(updated);
            context.SaveChanges();
            return updated;
        }
    }
}
