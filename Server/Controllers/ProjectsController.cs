using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Server.App.Git;
using Server.Config;
using Server.Data;
using Server.Models;

namespace Server.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOptions<RepositoryConfig> _config;

        public ProjectsController(ApplicationDbContext context, IOptions<RepositoryConfig> config)
        {
            _context = context;
            _config = config;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Projects.Include(p => p.Owner);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Projects/Details/5
        [HttpGet("Projects/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var repo = new RepoBrowser(Path.Combine(_config.Value.Path, project.Name));
            var treeObjects = repo.GetTree(null, project.DefaultBranch, null);

            ViewBag.treeObjects = treeObjects;

            return View(project);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet("Projects/{id}/tree/{branch}/{*path}")]
        public async Task<IActionResult> Tree(int id , string branch, string path)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var repo = new RepoBrowser(Path.TrimEndingDirectorySeparator(Path.Combine(_config.Value.Path, project.Name)));
            var treeObjects = repo.GetTree(path, branch, null);

            ViewBag.treeObjects = treeObjects;
            ViewBag.currentBranch = branch;

            ViewBag.parentPath = String.IsNullOrEmpty(path) ? "" : Path.TrimEndingDirectorySeparator("/" + Path.GetDirectoryName(path));
            

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            //ViewData["OwnerID"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,DefaultBranch")] Project project)
        {
            project.OwnerID = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();

                string rootedPath = Repository.Init(Path.Combine(_config.Value.Path, project.Name), true);

                return RedirectToAction(nameof(Index));
            }

            //ViewData["OwnerID"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", project.OwnerID);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            //ViewData["OwnerID"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", project.OwnerID);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,DefaultBranch")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OwnerID"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", project.OwnerID);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
