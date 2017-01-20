using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ProjectExcluder
{
	class Program
	{
		private static int _counter = 0;
		private static List<string> _processedFiles = new List<string>();

		static void Main(string[] args)
		{
			var mainProjectFilePath = string.Empty;

#if DEBUG
			mainProjectFilePath = @"C:\dvlp\Project\ASPNET4\Report2\Report2.Web\Report2.Web.csproj";
#else
			if (args.Count() != 1)
				return;

			mainProjectFilePath = args[0];
#endif

			if (!File.Exists(mainProjectFilePath))
				throw new FileNotFoundException("not found !");

			var document = XDocument.Load(mainProjectFilePath);

			FindChildProjects(mainProjectFilePath);

#if DEBUG
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine($"Save main project file: {Path.GetFileName(mainProjectFilePath)}");
			Console.ForegroundColor = ConsoleColor.Gray;
#else
			document.Save(mainProjectFilePath);
#endif
			Console.WriteLine("End");
			Console.Read();
			//}

		}

		private static void FindChildProjects(string filePath)
		{
			//counter++;

			var di = new DirectoryInfo(filePath);
			filePath = di.FullName;

			var document = XDocument.Load(filePath);
			var baseDirectory = Path.GetDirectoryName(filePath);

			Console.WriteLine($"FindChildProjects from: {Path.GetFileName(filePath)}");

			#region Content elements are empty
			/*
			foreach (var itemGroup in document.Root.Elements().Where(e => e.Name.LocalName == "ItemGroup" && !e.HasAttributes && e.HasElements))
			{
				var contentElements = GetContentElement(itemGroup);
				if (contentElements.Any())
				{
					Console.WriteLine(C($"We have {referenceElements.Count()} Project reference element(s)"));

					foreach(var contentElement in contentElements.Where(c => c.Attribute("Include").Value.Contains(".csproj")))
					{
						var childProjectFullPath = Path.Combine(baseDirectory, ExtractProjectFile(contentElement));
						AddSonarQubeElement(childProjectFullPath);
						FindChildProjects(childProjectFullPath);
					}
				}
			}
			*/
			#endregion

			foreach (var itemGroup in document.Root.Elements().Where(e => e.Name.LocalName == "ItemGroup" && !e.HasAttributes && e.HasElements))
			{
				var referenceElements = GetReferenceElement(itemGroup);
				if (referenceElements.Any())
				{
					Console.WriteLine($"We have {referenceElements.Count()} Project reference included in {Path.GetFileName(filePath)}");

					foreach (var contentElement in referenceElements.Where(c => c.Attribute("Include").Value.Contains(".csproj")))
					{
						var childProjectFullPath = Path.Combine(baseDirectory, GetRelativeProjectFilePath(contentElement));
						AddSonarQubeElement(childProjectFullPath);
						FindChildProjects(childProjectFullPath);
					}
				}
			}
		}

		private static string GetRelativeProjectFilePath(XElement content)
		{
			return content.Attribute("Include").Value;
		}

		//private static IEnumerable<XElement> GetContentElement(XElement itemGroup)
		//{
		//	return itemGroup.Elements().Where(e => e.Name.LocalName == "Content" && e.HasAttributes).ToList();
		//}

		private static IEnumerable<XElement> GetReferenceElement(XElement itemGroup)
		{
			return itemGroup.Elements().Where(e => e.Name.LocalName == "ProjectReference" && e.HasAttributes).ToList();
		}

		private static void AddSonarQubeElement(string filePath)
		{
			var fileName = Path.GetFileName(filePath);

			if (!_processedFiles.Any(s => s == fileName))
			{
				var document = XDocument.Load(filePath);
				var parentNameSpace = document.Root.Name.Namespace;

				var propertyGroup = document.Root.Elements().Where(e => e.Name.LocalName == "PropertyGroup" && !e.HasAttributes && e.HasElements).FirstOrDefault();

				//var sonarQubeAttribute = propertyGroup.Elements().SingleOrDefault(p => p.Name.LocalName == "SonarQubeExclude");

				//if (sonarQubeAttribute == null)
				//{
#if DEBUG
				Console.WriteLine($"Add SonarQubeExclude attribute to: {fileName}");
#else
				propertyGroup.Add(new XElement(parentNameSpace + "SonarQubeExclude", true));
#endif
				_processedFiles.Add(fileName);
				//}

#if DEBUG
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"Save project file: {fileName}");
				Console.ForegroundColor = ConsoleColor.Gray;
#else
			document.Save(filePath);
#endif
			}
		}
	}
}
