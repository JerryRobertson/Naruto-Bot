using System.Collections.Generic;
using TousenBot.Models;

namespace TousenBot.Repositories
{
    public interface IJutsuRepository
    {
        void Add(Jutsu jutsu);
        void Add(List<Jutsu> jutsu);
        void Delete(Jutsu jutsu);
        List<Jutsu> GetAll();
        Jutsu GetByExactName(string text);
        List<Jutsu> GetLike(string text);
        void Update(Jutsu jutsu);
        void Update(List<Jutsu> jutsu);
    }
}