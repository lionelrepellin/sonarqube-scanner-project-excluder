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
				throw new FileNotFoundException($"Not found: {mainProjectFilePath}");

			FindChildProjects(mainProjectFilePath);

			Console.WriteLine($"The End: {_processedFiles.Count} files have been processed");
#if DEBUG
			Console.Read();
#endif
		}

		private static void FindChildProjects(string filePath)
		{
			filePath = new DirectoryInfo(filePath).FullName;

			var baseDirectory = Path.GetDirectoryName(filePath);
			var document = XDocument.Load(filePath);
			var itemGroups = FindItemGroupElements(document);

			//Console.WriteLine($"FindChildProjects from: {Path.GetFileName(filePath)}");			

			foreach (var itemGroup in itemGroups)
			{
				var projectReferences = FindProjectReferenceElements(itemGroup);
				if (projectReferences.Any())
				{
					//Console.WriteLine($"We have {referenceElements.Count()} Project reference included in {Path.GetFileName(filePath)}");

					foreach (var projectReference in projectReferences)
					{
						var childProjectPath = new DirectoryInfo(Path.Combine(baseDirectory, projectReference.Attribute("Include").Value)).FullName;

						if (!File.Exists(childProjectPath))
							throw new FileNotFoundException($"Not found: {childProjectPath}");

						AddSonarQubeElement(childProjectPath);
						FindChildProjects(childProjectPath);
					}
				}
			};
		}
		
		private static void AddSonarQubeElement(string filePath)
		{
			var fileName = Path.GetFileName(filePath);

			if (!_processedFiles.Any(s => s == fileName))
			{
				var document = XDocument.Load(filePath);
				var parentNameSpace = document.Root.Name.Namespace;
				var propertyGroup = FindPropertyGroup(document);
#if DEBUG
				//Console.WriteLine($"Add SonarQubeExclude attribute to: {fileName}");
#else
				propertyGroup.Add(new XElement(parentNameSpace + "SonarQubeExclude", true));
#endif
				_processedFiles.Add(fileName);
#if DEBUG
				Console.WriteLine($"Save project file: {fileName}");
#else
				document.Save(filePath);
#endif
			}
		}
		
		private static IEnumerable<XElement> FindProjectReferenceElements(XElement itemGroup)
		{
			return itemGroup.Elements().Where(e =>
							e.Name.LocalName == "ProjectReference" &&
							e.HasAttributes &&
							e.Attribute("Include") != null &&
							e.Attribute("Include").Value.Contains(".csproj")
			).ToList();
		}

		private static IEnumerable<XElement> FindItemGroupElements(XDocument document)
		{
			return document.Root.Elements().Where(e => e.Name.LocalName == "ItemGroup" && !e.HasAttributes && e.HasElements).ToList();
		}

		private static XElement FindPropertyGroup(XDocument document)
		{
			return document.Root.Elements().FirstOrDefault(e => e.Name.LocalName == "PropertyGroup" && !e.HasAttributes && e.HasElements);
		}
	}
}