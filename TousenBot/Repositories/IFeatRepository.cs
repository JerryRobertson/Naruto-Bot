using System.Collections.Generic;
using TousenBot.Models;

namespace TousenBot.Repositories
{
    public interface IFeatRepository
    {
        void Add(Feat feat);
        void Add(List<Feat> feat);
        void Delete(Feat feat);
        List<Feat> GetAll();
        List<Feat> GetLike(string text);
        void Update(Feat feat);
        void Update(List<Feat> feat);
    }
}