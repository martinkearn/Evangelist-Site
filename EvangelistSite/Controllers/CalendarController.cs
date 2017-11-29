using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using EvangelistSite.Models;
using Microsoft.Extensions.Options;
using EvangelistSite.Data;
using Microsoft.EntityFrameworkCore;
using EvangelistSite.Models.CalendarViewModels;

namespace EvangelistSite.Controllers
{
    public class CalendarController : Controller
    {
        private ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var conferences = await _context.Conference
                .Where(o => o.Date >= DateTime.Now.Subtract(new TimeSpan(1,0,0,0)))
                .Include(o => o.ConferenceTalks)
                .ThenInclude(o => o.Talk)
                .OrderBy(o => o.Date)
                .ToListAsync();

            var vm = new IndexViewModel()
            {
                Conferences = conferences
            };

            return View(vm);
        }
    }
}