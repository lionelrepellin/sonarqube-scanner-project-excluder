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
		static int counter = 0;

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
			Console.WriteLine($"Save main project file: {Path.GetFileName(mainProjectFilePath)}");
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
			
			Console.WriteLine(C($"FindChildProjects from: {Path.GetFileName(filePath)}"));
			/*
			foreach (var itemGroup in document.Root.Elements().Where(e => e.Name.LocalName == "ItemGroup" && !e.HasAttributes && e.HasElements))
			{
				var contentElements = ContainsContentElement(itemGroup);
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
			foreach (var itemGroup in document.Root.Elements().Where(e => e.Name.LocalName == "ItemGroup" && !e.HasAttributes && e.HasElements))
			{
				var referenceElements = ContainsReferenceElement(itemGroup);
				if (referenceElements.Any())
				{
					Console.WriteLine(C($"We have {referenceElements.Count()} Project reference included in {Path.GetFileName(filePath)}"));

					foreach (var contentElement in referenceElements.Where(c => c.Attribute("Include").Value.Contains(".csproj")))
					{
						var childProjectFullPath = Path.Combine(baseDirectory, ExtractProjectFile(contentElement));
						AddSonarQubeElement(childProjectFullPath);
						FindChildProjects(childProjectFullPath);
					}
				}
			}
		}

		private static string ExtractProjectFile(XElement content)
		{
			return content.Attribute("Include").Value;
		}

		private static IEnumerable<XElement> ContainsContentElement(XElement itemGroup)
		{
			return itemGroup.Elements().Where(e => e.Name.LocalName == "Content" && e.HasAttributes).ToList();
		}

		private static IEnumerable<XElement> ContainsReferenceElement(XElement itemGroup)
		{
			return itemGroup.Elements().Where(e => e.Name.LocalName == "ProjectReference" && e.HasAttributes).ToList();
		}

		private static XDocument AddSonarQubeElement(string filePath)
		{
			var document = XDocument.Load(filePath);
			var parentNameSpace = document.Root.Name.Namespace;

			var propertyGroup = document.Root.Elements().Where(e => e.Name.LocalName == "PropertyGroup" && !e.HasAttributes && e.HasElements).FirstOrDefault();

			var sonarQubeAttribute = propertyGroup.Elements().SingleOrDefault(p => p.Name.LocalName == "SonarQubeExclude");

			if(sonarQubeAttribute == null)
			{
#if DEBUG
				Console.WriteLine(C($"Add SonarQubeExclude attribute to: {Path.GetFileName(filePath)}"));
#else
				propertyGroup.Add(new XElement(parentNameSpace + "SonarQubeExclude", true));
#endif
			}

#if DEBUG
			Console.WriteLine(C($"Save project file: {Path.GetFileName(filePath)}"));
#else
			document.Save(filePath);
#endif

			return document;
		}

		private static string C(string msg)
		{
			var s = string.Empty;
			for (int x = 0; x < counter; x++)
			{
				s += '-';
			}
			return $"{s}> {msg}";
		}
	}
}
