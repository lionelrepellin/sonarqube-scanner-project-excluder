# Sonarqube Scanner Project Excluder

This executable is useful when you use Sonarqube with Sonaqube Scanner for MSBuild and does not want to scan subprojects included in your main project.

There are two ways to exclude subprojects from the scanner :

- You can use a pattern like: `**/subproject/**/*.*` and include it in the Sonaqube global/project configuration or even in the .csproj of the main project. I tried many different patterns but this way didn't work for me.
- The other way is to include a **SonarQubeExclude** attribute in each subproject .csproj file. I tried this and it works fine.

The context: I work with a solution that includes about **sixty** projects. There are less than fifteen main projects and all include a lot of subprojects. So, I can't open manually each .csproj to add a simple attribute.

Exemple of project architecture :

```
MainProject
├── ProjectA
│   ├── ProjectB
│   └── ProjectC
|       └── ProjectD
└── ProjectE
    └── ProjectD
```

How to use it:

```
ProjectExcluder.exe "C:\Dev\MySolution\MainProject\MainProject.csproj"
```

Subprojects A, B, C, D, and E will be updated with a new attribute in their .csproj file.

You can run the program many times, it will add the **SonarQubeExclude** attribute only once.
