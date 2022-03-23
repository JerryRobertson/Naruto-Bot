using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using TousenBot.Data;
using TousenBot.Models;

namespace TousenBot.Repositories
{
    public class JutsuRepository : IJutsuRepository
    {
        private readonly ApplicationDbContext _context;
        public JutsuRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public List<Jutsu> GetAll()
        {
            return _context.Jutsu.ToList();
        }
        public Jutsu GetByExactName(string text)
        {
            var answer = _context.Jutsu.FirstOrDefault(j => j.Name.ToLower().Equals(text.ToLower()));
            //Console.WriteLine(answer.Name);
            return answer;
        }
        public List<Jutsu> GetLike(string text)
        {
            List<Jutsu> jutsu = _context.Jutsu
                //.Where(j => j.Name.ToLower().Contains(text.ToLower()))
                .ToList();
            List<Jutsu> filteredJutsu = jutsu.Where(j => j.Name.ToLower().Contains(text.ToLower())).ToList();
            return filteredJutsu;

        }
        public void Add(Jutsu jutsu)
        {
            _context.Add(jutsu);
            _context.SaveChanges();
        }
        public void Add(List<Jutsu> jutsu)
        {
            _context.AddRange(jutsu);
            _context.SaveChanges();
        }
        public void Delete(Jutsu jutsu)
        {
            _context.Remove(jutsu);
            _context.SaveChanges();
        }
        public void Update(Jutsu jutsu)
        {
            _context.Entry(jutsu).State = EntityState.Modified;
            _context.SaveChanges();
        }
        public void Update(List<Jutsu> jutsu)
        {
            foreach (var item in jutsu)
            {

                _context.Entry(item).State = EntityState.Modified;
            }
            _context.SaveChanges();
        }

    }
}
