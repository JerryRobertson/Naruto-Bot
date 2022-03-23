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
    public class FeatRepository : IFeatRepository
    {
        private readonly ApplicationDbContext _context;
        public FeatRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<Feat> GetAll()
        {
            return _context.Feat.ToList();
        }
        public List<Feat> GetLike(string text)
        {
            List<Feat> feats = _context.Feat.ToList();
            return feats.Where(f => f.Name.ToLower().Contains(text.ToLower())).ToList();
        }
        public void Add(Feat feat)
        {
            _context.Add(feat);
            _context.SaveChanges();
        }
        public void Add(List<Feat> feat)
        {
            _context.AddRange(feat);
            _context.SaveChanges();
        }
        public void Delete(Feat feat)
        {
            _context.Remove(feat);
            _context.SaveChanges();
        }
        public void Update(Feat feat)
        {
            _context.Entry(feat).State = EntityState.Modified;
            _context.SaveChanges();
        }
        public void Update(List<Feat> feat)
        {
            foreach (var item in feat)
            {
                _context.Entry(item).State = EntityState.Modified;

            }
            _context.SaveChanges();
        }
    }
}
