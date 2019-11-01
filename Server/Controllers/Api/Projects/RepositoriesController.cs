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
using Server.Models.Git;

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

        /// <summary>
        /// Returns items from repository
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("tree/{id}")]
        public async Task<List<TreeObject>> GetTree(int id)
        {
            string path = HttpContext.Request.Query["path"];
            var project = await _context.Projects.FindAsync(id);
            _repository = new Repository(Path.Combine(_config.Value.Path, project.Name));
            
            var treeObjects = new List<Server.Models.Git.TreeObject>();
            var tree = _repository.Head.Tip.Tree;

            if (string.IsNullOrEmpty(path))
            {
                foreach (var item in tree)
                {
                    treeObjects.Add(new TreeObject { 
                        Name = item.Name,
                        Sha = item.Target.Sha,
                        Type = item.TargetType.ToString(),
                        Path = item.Path
                    });
                }
            } else
            {
                var xxx = tree[path];

                // For tree
                if (xxx.TargetType == TreeEntryTargetType.Tree)
                {
                    foreach (var item in (Tree)xxx.Target)
                    {
                        treeObjects.Add(new TreeObject {
                            Name = item.Name,
                            Sha = item.Target.Sha,
                            Type = item.TargetType.ToString(),
                            Path = item.Path
                        });
                    }
                }

                // For blobs
                if (xxx.TargetType == TreeEntryTargetType.Blob)
                {
                    var file = (Blob)xxx.Target;

                    var blobObject = new TreeObject {
                        Name = xxx.Name,
                        Sha = xxx.Target.Sha,
                        Type = xxx.TargetType.ToString(),
                        Path = xxx.Path
                    };

                    if (!file.IsBinary)
                    {
                        string blobContent = null;
                        using (var sr = new StreamReader(file.GetContentStream(), Encoding.UTF8))
                        {
                            blobContent = sr.ReadToEnd();
                        }

                        blobObject.Content = blobContent;
                    }

                    treeObjects.Add(blobObject);
                }
            }

            return treeObjects;
        }

        /// <summary>
        /// Return file content from repository if it is not binary
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("blob/{id}")]
        public async Task<TreeObject> GetBlob(int id)
        {
            string path = HttpContext.Request.Query["path"];
            var project = await _context.Projects.FindAsync(id);
            _repository = new Repository(Path.Combine(_config.Value.Path, project.Name));

            var tree = _repository.Head.Tip.Tree;
            var xxx = tree[path];

            if (xxx != null)
            {
                if (xxx.TargetType == TreeEntryTargetType.Blob)
                {
                    var file = (Blob)xxx.Target;

                    var blobObject = new TreeObject
                    {
                        Name = xxx.Name,
                        Sha = xxx.Target.Sha,
                        Type = xxx.TargetType.ToString(),
                        Path = xxx.Path
                    };

                    if (!file.IsBinary)
                    {
                        string blobContent = null;
                        using (var sr = new StreamReader(file.GetContentStream(), Encoding.UTF8))
                        {
                            blobContent = sr.ReadToEnd();
                        }

                        blobObject.Content = blobContent;
                    }
                    return blobObject;
                }
            }

            return new TreeObject();
        }

        [HttpGet("history/{id}")]
        public async Task<string> GetFileHistory(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            var repo = new Repository(_config.Value.Path + @"\" + project.Name);

            var historyEntries = repo.Commits.QueryBy(".travis.yml").Select(h => h.Commit.Sha).Last();

            return historyEntries;
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