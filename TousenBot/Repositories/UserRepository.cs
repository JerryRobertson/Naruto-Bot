using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TousenBot.Data;
using TousenBot.Models;

namespace TousenBot.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<User> GetAll()
        {
            return _context.User.ToList();
        }
        public bool UserExist(string userName)
        {
            var user = _context.User.FirstOrDefault(u => u.Name == "userName");
            if (user == null) { return false; }
            return true;
        }
        public void Add(User user)
        {
            _context.Add(user);
            _context.SaveChanges();
        }
        public void Add(List<User> user)
        {
            _context.AddRange(user);
            _context.SaveChanges();
        }
        public void Delete(User user)
        {
            _context.Remove(user);
            _context.SaveChanges();
        }
        public void Update(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            _context.SaveChanges();
        }
        public void Update(List<User> user)
        {
            foreach (var item in user)
            {

                _context.Entry(item).State = EntityState.Modified;
            }
            _context.SaveChanges();
        }


    }
}
