using LibGit2Sharp;
using Server.Models.Git;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.App.Git
{
    public class RepoBrowser
    {
        private readonly Repository _repository;

        public RepoBrowser(string repositoryPath)
        {
            if (!Repository.IsValid(repositoryPath))
            {
                //
            }

            _repository = new Repository(repositoryPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="referencename"></param>
        /// <returns></returns>
        public Commit GetCommitByName (string name, string referencename)
        {
            referencename = null;

            if (string.IsNullOrEmpty(name))
            {
                referencename = _repository.Head.FriendlyName;

                return _repository.Head.Tip;
            }

            var branch = _repository.Branches[name];
            if (branch != null && branch.Tip != null)
            {
                referencename = branch.FriendlyName;
                return branch.Tip;
            }

            var tag = _repository.Tags[name];
            if (tag == null)
            {
                return _repository.Lookup(name) as Commit;
            }

            referencename = tag.FriendlyName;
            return tag.Target as Commit;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="referenceName"></param>
        /// <returns></returns>
        public IEnumerable<Commit> GetHistory (string path, string name, string referenceName)
        {
            var commit = GetCommitByName(name, referenceName);

            if (commit == null || String.IsNullOrEmpty(path))
            {
                return Enumerable.Empty<Commit>();
            }

            return _repository.Commits.
                        QueryBy(new CommitFilter { IncludeReachableFrom = commit, SortBy = CommitSortStrategies.Topological })
                        .Where(c => c.Parents.Count() < 2 && c[path] != null && (c.Parents.Count() == 0 || c.Parents.FirstOrDefault()[path] == null || c[path].Target.Id != c.Parents.FirstOrDefault()[path].Target.Id))
                        .Select(s => s);
        }

        public Repository GetRepository()
        {
            return _repository;
        }

        /// <summary>
        /// Browse tree of given repository by branch
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <param name="referenceName"></param>
        /// <returns></returns>
        public List<TreeObject> GetTree(string path, string name, string referenceName)
        {
            var commit = GetCommitByName(name, referenceName);
            var tree = commit.Tree;

            var treeObjects = new List<TreeObject>();

            if (String.IsNullOrEmpty(path))
            {
                foreach (var item in tree)
                {
                    var commitMessages = GetHistory(item.Path, null, null);

                    try
                    {
                        treeObjects.Add(new TreeObject
                        {
                            Name = item.Name,
                            Sha = item.Target.Sha,
                            Type = item.TargetType.ToString(),
                            Path = item.Path,
                            CommitMessage = commitMessages.First().MessageShort,
                            When = commitMessages.Take(1).First().Author.When
                        });
                    } catch (ArgumentNullException e)
                    {
                        //
                    } catch (InvalidOperationException e)
                    {
                        //
                    }
                }
            } else
            {
                var subTree = tree[path];

                if (subTree.TargetType == TreeEntryTargetType.Tree)
                {
                    foreach (var item in (Tree)subTree.Target)
                    {
                        var commitMessages = GetHistory(item.Path, null, null);

                        try
                        {
                            treeObjects.Add(new TreeObject
                            {
                                Name = item.Name,
                                Sha = item.Target.Sha,
                                Type = item.TargetType.ToString(),
                                Path = item.Path,
                                CommitMessage = commitMessages.First().MessageShort,
                                When = commitMessages.Take(1).First().Author.When
                            });
                        }
                        catch (ArgumentNullException e)
                        {
                            //
                        }
                        catch (InvalidOperationException e)
                        {
                            //
                        }
                    }
                }
            }

            return treeObjects;
        }
    }
}
