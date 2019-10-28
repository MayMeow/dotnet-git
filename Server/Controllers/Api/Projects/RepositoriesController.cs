using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Server.Config;
using Server.Data;

namespace Server.Controllers.Api.Projects
{
    [Produces("application/json")]
    [Route("api/Projects/[controller]")]
    [ApiController]
    public class RepositoriesController : ControllerBase
    {
        Repository _repository;

        private readonly ApplicationDbContext _context;
        private readonly IOptions<RepositoryConfig> _config;

        public RepositoriesController(ApplicationDbContext context, IOptions<RepositoryConfig> config)
        {
            _context = context;
            _config = config;
        }

        [HttpGet("tree/{id}")]
        public async Task<IEnumerable<TreeEntry>> GetTree(int id)
        {
            string sha = HttpContext.Request.Query["sha"];

            var project = await _context.Projects.FindAsync(id);

            var repo = new Repository(_config.Value.Path + @"\" + project.Name);

            //var tree = repo.Lookup<Tree>("48fe937f62cbb3a5cf6e1820348cf0a007aad529");
            //var tree = repo.

            if (sha == null)
            {
                var tree = repo.Head.Tip.Tree;
                return tree;
            } else
            {
                var tree = repo.Lookup<Tree>(sha);
                return tree;
            }
        }

        [HttpGet("blob/{id}")]
        public async Task<string> GetBlob(int id)
        {
            string content = null;

            string sha = HttpContext.Request.Query["sha"];
            var project = await _context.Projects.FindAsync(id);
            var repo = new Repository(_config.Value.Path + @"\" + project.Name);


            var tree = repo.Lookup<Blob>(sha).GetContentStream();
            //var meesage = repo.Commits;
            using (var sr = new StreamReader(tree, Encoding.UTF8))
            {
                content = sr.ReadToEnd();
            }
            
            return content;
        }

        [HttpGet("hello")]
        public async Task<string> GetHello()
        {
            return "Hello";
        }

        [HttpGet("path/{id}")]
        public async Task<string> GetPath(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            return _config.Value.Path + @"\" + project.Name;
        }
    }
}