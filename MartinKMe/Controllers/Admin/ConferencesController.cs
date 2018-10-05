using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MartinKMe.Data;
using MartinKMe.Models;
using MartinKMe.Models.ConferencesViewModels;

namespace MartinKMe.Controllers.Admin
{
    public class ConferencesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConferencesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: Conferences
        public async Task<IActionResult> Index()
        {
            return View(await _context.Conference.ToListAsync());
        }

        // GET: Conferences/Create
        public async Task<IActionResult> Create()
        {
            var talks = new List<SelectListItem>();
            foreach (var item in await _context.Talk.ToListAsync())
            {
                talks.Add(new SelectListItem() { Text = item.Title, Value = item.Id.ToString() });
            }

            var conference = new Conference();
            conference.Date = DateTime.Now;

            var vm = new CreateViewModel()
            {
                Conference = conference,
                Talks = talks
            };

            return View(vm);
        }

        // POST: Conferences/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Conference conference)
        {
            if (ModelState.IsValid)
            {
                _context.Add(conference);
                await _context.SaveChangesAsync();

                //update Talks
                await CreateUpdateTalkMappings(conference.Id, Request.Form["TalkIds"].ToList());

                return RedirectToAction("Index");
            }
            return View(conference);
        }

        // GET: Conferences/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conference.SingleOrDefaultAsync(m => m.Id == id);
            if (conference == null)
            {
                return NotFound();
            }

            //get Conference <> Talk mappings for this Conference
            var selectedTalks = await _context.ConferenceTalk.Where(o => o.ConferenceId == id).ToListAsync();

            var talks = new List<SelectListItem>();
            foreach (var item in await _context.Talk.ToListAsync())
            {
                //figure out if this Talk is selected for this Conference
                var isSelected = (selectedTalks.Where(o => o.TalkId == item.Id).Count() > 0);

                //construct and add Select List Item
                var selectListItem = new SelectListItem()
                {
                    Text = item.Title,
                    Value = item.Id.ToString(),
                    Selected = isSelected
                };
                talks.Add(selectListItem);
            }

            var vm = new EditViewModel()
            {
                Conference = conference,
                Talks = talks
            };

            return View(vm);
        }

        // POST: Conferences/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Conference conference)
        {
            if (id != conference.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //update Conference
                    _context.Update(conference);
                    await _context.SaveChangesAsync();

                    //update Talks
                    await CreateUpdateTalkMappings(id, Request.Form["TalkIds"].ToList());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConferenceExists(conference.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(conference);
        }

        // GET: Conferences/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var conference = await _context.Conference.SingleOrDefaultAsync(m => m.Id == id);
            if (conference == null)
            {
                return NotFound();
            }

            return View(conference);
        }

        // POST: Conferences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //delete Resource <> Talk mappings
            await DeleteTalkMappings(id);

            //delete conference
            var conference = await _context.Conference.SingleOrDefaultAsync(m => m.Id == id);
            _context.Conference.Remove(conference);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private bool ConferenceExists(int id)
        {
            return _context.Conference.Any(e => e.Id == id);
        }

        private async Task<bool> CreateUpdateTalkMappings(int conferenceId, List<string> talkIds)
        {
            try
            {
                await DeleteTalkMappings(conferenceId);

                //add Resource <> Talk mappings based on submitted form
                foreach (var talkId in talkIds)
                {
                    _context.ConferenceTalk.Add(new ConferenceTalk()
                    {
                        ConferenceId = conferenceId,
                        TalkId = Convert.ToInt32(talkId)
                    });
                }
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> DeleteTalkMappings(int conferenceId)
        {
            try
            {
                _context.ConferenceTalk.RemoveRange(_context.ConferenceTalk.Where(o => o.ConferenceId == conferenceId));
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
