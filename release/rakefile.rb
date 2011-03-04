require "albacore"
require "release/filesystem"
require "release/nuget"

task :default => [:unitTests]

desc "Inits the build"
task :initBuild do
	FileSystem.EnsurePath("reports")
	FileSystem.DeleteDirectory("deploy")
	FileSystem.EnsurePath("deploy/package/lib")
end

desc "Generate assembly info."
assemblyinfo :assemblyInfo => :initBuild do |asm|
    asm.version = ENV["GO_PIPELINE_LABEL"] + ".0"
    asm.company_name = "Ultraviolet Catastrophe"
    asm.product_name = "Gribble"
    asm.title = "Gribble"
    asm.description = "Gribble ORM"
    asm.copyright = "Copyright (c) 2010 Ultraviolet Catastrophe"
    asm.output_file = "src/Gribble/Properties/AssemblyInfo.cs"
end

desc "Builds the library."
msbuild :buildLibrary => :assemblyInfo do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Gribble/Gribble.csproj"
end

desc "Builds the test project."
msbuild :buildTestProject => :buildLibrary do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Tests/Tests.csproj"
end

desc "NUnit Test Runner"
nunit :unitTests => :buildTestProject do |nunit|
	nunit.command = "src/packages/NUnit.2.5.9.10348/Tools/nunit-console.exe"
	nunit.assemblies "src/Tests/bin/Release/Tests.dll"
	nunit.options "/xml=reports/TestResult.xml"
end

desc "Create the nuspec"
nuspec :createSpec => :unitTests do |nuspec|
   nuspec.id = "gribble"
   nuspec.version = ENV["GO_PIPELINE_LABEL"]
   nuspec.authors = "Mike O'Brien"
   nuspec.owners = "Mike O'Brien"
   nuspec.description = "Gribble is a simple, Linq enabled ORM designed to work with dynamically created tables."
   nuspec.summary = "Gribble is a simple, Linq enabled ORM designed to work with dynamically created tables."
   nuspec.language = "en-US"
   nuspec.licenseUrl = "https://github.com/mikeobrien/Gribble/blob/master/LICENSE"
   nuspec.projectUrl = "https://github.com/mikeobrien/Gribble"
   nuspec.working_directory = "deploy/package"
   nuspec.output_file = "gribble.nuspec"
   nuspec.tags = "orm sql"
end

desc "Push the package to the Nuget server"
task :prepPackage => :createSpec do
	FileSystem.CopyFiles("src/Gribble/bin/Release/Gribble.dll", "deploy/package/lib")
	FileSystem.CopyFiles("src/Gribble/bin/Release/Gribble.pdb", "deploy/package/lib")
end

desc "Create the nuget package"
nugetpack :createPackage => :prepPackage do |nugetpack|
   nugetpack.nuspec = "deploy/package/gribble.nuspec"
   nugetpack.base_folder = "deploy/package"
   nugetpack.output = "deploy"
end

desc "Push the nuget package"
nugetpush :pushPackage => :createPackage do |nugetpush|
   nugetpush.package = "deploy/gribble.#{ENV['GO_PIPELINE_LABEL']}.nupkg"
end

desc "Tag the current release"
task :tagRelease do
	#result = system("git", "tag", "-a", "v#{ENV['GO_PIPELINE_LABEL']}", "-m", "release-v#{ENV['GO_PIPELINE_LABEL']}")
	#result = system("git", "push", "--tags")
end
