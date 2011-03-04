require "albacore"
require "release/common"

task :default => [:unitTests]

desc "Inits the build"
task :initBuild do
	Common.EnsurePath("reports")
end

desc "Generate assembly info."
assemblyinfo :assemblyInfo => :initBuild do |asm|
    asm.version = ENV["GO_PIPELINE_LABEL"]
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

desc "Create the Nuget Package"
nuspec :createPackage => :unitTests do |nuspec|
   nuspec.id="gribble"
   nuspec.version = "1.0.0"
   nuspec.authors = "Mike O'Brien"
   nuspec.description = "Gribble is a simple, Linq enabled ORM designed to work with dynamically created tables."
   nuspec.title = "Gribble ORM"
   nuspec.language = "en-US"
   nuspec.licenseUrl = "https://github.com/mikeobrien/Gribble/blob/master/LICENSE"
   nuspec.projectUrl = "https://github.com/mikeobrien/Gribble"
   nuspec.working_directory = "Build/Deploy"
   nuspec.output_file = "gribble.nuspec"
   nuspec.tags = "orm sql"
   nuspec.file("src/Gribble/bin/Release/Gribble.dll", "lib")
   nuspec.file("src/Gribble/bin/Release/Gribble.pdb", "lib")
end

desc "Push the package to the Nuget server"
task :pushPackage => :createPackage do
	
end

desc "Tag the current release"
task :tagRelease do
	#result = system("git", "tag", "-a", "v#{ENV['GO_PIPELINE_LABEL']}", "-m", "release-v#{ENV['GO_PIPELINE_LABEL']}")
	#result = system("git", "push", "--tags")
end
