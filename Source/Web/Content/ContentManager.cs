using System.IO;
using NGit.Api;
using System.Web;
using Bifrost.Execution;
using System.Collections.Generic;
using System.Linq;
using System;
using Sharpen;
using Web.Content.Documentation;

namespace Web.Content
{
	[Singleton]
	public class ContentManager : IContentManager
	{
		public static string ContentPath; 

		public static void Initialize(HttpServerUtility server)
		{
			ContentPath = server.MapPath("~/App_Data/Repositories");
			var sitePath = server.MapPath ("~");
			Runtime.SetProperty("user.home", sitePath);
		}

		public void Synchronize(string project)
		{
			var repositoryUrl = GetRepositoryUrlFor(project);
			var repositoryPath = GetRepositoryPathFor(project);
			if( !HasRepository(repositoryPath)) 
			{
				Directory.CreateDirectory(repositoryPath);
				var cloneCommand = Git.CloneRepository();

				cloneCommand.SetURI(repositoryUrl);
				cloneCommand.SetDirectory(repositoryPath);
				cloneCommand.Call();
			} 
			else 
			{
				var git = Git.Open (repositoryPath);
				git.Pull().Call ();
			}
		}

		public void DeleteRepository(string project)
		{
			var repositoryPath = GetRepositoryPathFor(project);
			if( Directory.Exists(repositoryPath) )
				Directory.Delete(repositoryPath,true);
		}

		public IEnumerable<Group> GetDocumentationStructure (string project)
		{
			var groups = new List<Group>();
			var path = GetRepositoryPathFor(project);
			var git = Git.Open (path);
			var documentationPath = string.Format("{0}{1}Documentation",path,Path.DirectorySeparatorChar);

			var directory = new DirectoryInfo(documentationPath);
			foreach( var groupDirectory in directory.GetDirectories() ) 
			{
				if( groupDirectory.Name.StartsWith(".") ) continue;

				var group = new Group
				{
					Name = groupDirectory.Name,
					Elements = groupDirectory.GetFiles().Where(FileIsMarkDown).Select(f => ConvertFileInfoToElement(git, project, f)).ToArray()
				};

				var topics = new List<Topic>();

				foreach( var topicDirectory in groupDirectory.GetDirectories() ) 
				{
					var topic = new Topic 
					{
						Name = topicDirectory.Name,
						Elements = topicDirectory.GetFiles ().Where(FileIsMarkDown).Select(f => ConvertFileInfoToElement(git, project, f)).ToArray()
					};
					topics.Add (topic);
				}
				group.Topics = topics.ToArray ();
				groups.Add (group);
			}

			return groups;
		}

		public string GetFileContentFor(string project, string file)
		{
			var content = string.Empty;
			var path = string.Format ("{0}{1}{2}",GetRepositoryPathFor(project),Path.DirectorySeparatorChar,file);
			if( File.Exists(path))
				content = File.ReadAllText (path);

			return content;
		}

		bool FileIsMarkDown(FileInfo fileInfo)
		{
			return fileInfo.Extension == ".md";
		}

		Element ConvertFileInfoToElement(Git git, string project, FileInfo fileInfo)
		{
			var element =  new Element 
			{
				Name = Path.GetFileNameWithoutExtension(fileInfo.Name),
				File = GetRelativePathForFile(project, fileInfo),
				Author = "Unknown",
				LastChanged = "Unknown"
			};

			var logCommand = git.Log ();
			logCommand.AddPath (element.File);
			var result = logCommand.Call ();
			var commit = result.FirstOrDefault();
			if( commit != null ) 
			{
				element.Author = commit.GetCommitterIdent().GetName();
			}
			return element;
		}

		bool HasRepository(string path)
		{
			if( Directory.Exists(path) &&
			    Directory.Exists (string.Format ("{0}{1}{2}", path, Path.DirectorySeparatorChar, ".git")))
				return true;

			return false;
		}

		string GetRelativePathForFile(string project, FileInfo file)
		{
			var repositoryPath = GetRepositoryPathFor(project);
			var path = file.FullName.Substring(repositoryPath.Length+1);
			return path;
		}

		string GetRepositoryUrlFor(string project)
		{
			return string.Format ("https://github.com/dolittle/{0}-Site.git", project);
		}

		static string GetRepositoryPathFor(string project)
		{
			var fullPath = string.Format ("{0}{1}{2}-Site", ContentPath, Path.DirectorySeparatorChar, project);
			return fullPath;
		}
	}
}
