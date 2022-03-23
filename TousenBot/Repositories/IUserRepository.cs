using System.Collections.Generic;
using TousenBot.Models;

namespace TousenBot.Repositories
{
    public interface IUserRepository
    {
        void Add(List<User> user);
        void Add(User user);
        void Delete(User user);
        List<User> GetAll();
        void Update(List<User> user);
        void Update(User user);
        bool UserExist(string userName);
    }
}