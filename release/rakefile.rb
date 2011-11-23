require "albacore"
require "release/filesystem"

reportsPath = "reports"

task :build => :unitTests
task :pushPackages => [:pushCorePackage, :pushNHibernatePackage]

desc "Generate core assembly info."
assemblyinfo :coreAssemblyInfo do |asm|
    asm.version = ENV["GO_PIPELINE_LABEL"] + ".0"
    asm.company_name = "Ultraviolet Catastrophe"
    asm.product_name = "Gribble"
    asm.title = "Gribble"
    asm.description = "Gribble ORM"
    asm.copyright = "Copyright (c) 2011 Ultraviolet Catastrophe"
    asm.output_file = "src/Gribble/Properties/AssemblyInfo.cs"
end

desc "Builds the core assembly."
msbuild :buildCore => :coreAssemblyInfo do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Gribble/Gribble.csproj"
end

desc "Generate nhibernate integration assembly info."
assemblyinfo :nhibernateAssemblyInfo do |asm|
    asm.version = ENV["GO_PIPELINE_LABEL"] + ".0"
    asm.company_name = "Ultraviolet Catastrophe"
    asm.product_name = "Gribble NHibernate Integration"
    asm.title = "Gribble NHibernate Integration"
    asm.description = "Gribble NHibernate Integration"
    asm.copyright = "Copyright (c) 2011 Ultraviolet Catastrophe"
    asm.output_file = "src/Gribble.NHibernate/Properties/AssemblyInfo.cs"
end

desc "Builds the nhibernate integration library."
msbuild :buildNHibernate => [:buildCore, :nhibernateAssemblyInfo] do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Gribble/Gribble.csproj"
end

desc "Builds the test project."
msbuild :buildTestProject => [:buildCore, :buildNHibernate] do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Tests/Tests.csproj"
end

desc "Inits the unit test environment"
task :unitTestInit do
	FileSystem.EnsurePath(reportsPath)
end

desc "NUnit Test Runner"
nunit :unitTests => [:buildTestProject, :unitTestInit] do |nunit|
	nunit.command = "src/packages/NUnit.2.5.9.10348/Tools/nunit-console.exe"
	nunit.assemblies "src/Tests/bin/Release/Tests.dll"
	nunit.options "/xml=#{reportsPath}/TestResult.xml"
end

nugetApiKey = YAML::load(File.open(ENV["USERPROFILE"] + "/.nuget/credentials"))["api_key"]
deployPath = "deploy"

corePackagePath = File.join(deployPath, "corepackage")
coreNuspec = "gribble.nuspec"
corePackageLibPath = File.join(corePackagePath, "lib")
coreBinPath = "src/Gribble/bin/Release"

desc "Prep the core package folder"
task :prepCorePackage => :unitTests do
	FileSystem.DeleteDirectory(deployPath)
	FileSystem.EnsurePath(corePackageLibPath)
	FileSystem.CopyFiles(File.join(coreBinPath, "Gribble.dll"), corePackageLibPath)
	FileSystem.CopyFiles(File.join(coreBinPath, "Gribble.pdb"), corePackageLibPath)
end

desc "Create the core nuspec"
nuspec :createCoreSpec => :prepCorePackage do |nuspec|
   nuspec.id = "gribble"
   nuspec.version = ENV["GO_PIPELINE_LABEL"]
   nuspec.authors = "Mike O'Brien"
   nuspec.owners = "Mike O'Brien"
   nuspec.title = "Gribble ORM"
   nuspec.description = "Gribble is a simple, Linq enabled ORM designed to work with dynamically created tables."
   nuspec.summary = "Gribble is a simple, Linq enabled ORM designed to work with dynamically created tables."
   nuspec.language = "en-US"
   nuspec.licenseUrl = "https://github.com/mikeobrien/Gribble/blob/master/LICENSE"
   nuspec.projectUrl = "https://github.com/mikeobrien/Gribble"
   nuspec.working_directory = corePackagePath
   nuspec.output_file = coreNuspec
   nuspec.tags = "orm dal sql"
end

desc "Create the core nuget package"
nugetpack :createCorePackage => :createCoreSpec do |nugetpack|
   nugetpack.nuspec = File.join(corePackagePath, coreNuspec)
   nugetpack.base_folder = corePackagePath
   nugetpack.output = deployPath
end

desc "Push the core nuget package"
nugetpush :pushCorePackage => :createCorePackage do |nuget|
    nuget.apikey = nugetApiKey
    nuget.package = File.join(deployPath, "gribble.#{ENV['GO_PIPELINE_LABEL']}.nupkg")
end

nhibernatePackagePath = File.join(deployPath, "nhpackage")
nhibernateNuspec = "gribble.nhibernate.nuspec"
nhibernatePackageLibPath = File.join(nhibernatePackagePath, "lib")
nhibernateBinPath = "src/Gribble.NHibernate/bin/Release"

desc "Prep the NHibernate package folder"
task :prepNHibernatePackage => :unitTests do
	FileSystem.DeleteDirectory(deployPath)
	FileSystem.EnsurePath(nhibernatePackageLibPath)
	FileSystem.CopyFiles(File.join(nhibernateBinPath, "Gribble.NHibernate.dll"), nhibernatePackageLibPath)
	FileSystem.CopyFiles(File.join(nhibernateBinPath, "Gribble.NHibernate.pdb"), nhibernatePackageLibPath)
end

desc "Create the NHibernate nuspec"
nuspec :createNHibernateSpec => :prepNHibernatePackage do |nuspec|
   nuspec.id = "gribble.nhibernate"
   nuspec.version = ENV["GO_PIPELINE_LABEL"]
   nuspec.authors = "Mike O'Brien"
   nuspec.owners = "Mike O'Brien"
   nuspec.title = "Gribble ORM NHibernate Integration"
   nuspec.description = "Gribble NHibernate integration."
   nuspec.summary = "Gribble NHibernate integration."
   nuspec.language = "en-US"
   nuspec.licenseUrl = "https://github.com/mikeobrien/Gribble/blob/master/LICENSE"
   nuspec.projectUrl = "https://github.com/mikeobrien/Gribble"
   nuspec.working_directory = nhibernatePackagePath
   nuspec.output_file = nhibernateNuspec
   nuspec.tags = "orm dal sql nhibernate"
   nuspec.dependency "gribble", ENV["GO_PIPELINE_LABEL"]
   nuspec.dependency "NHibernate", "3.1.0.4000"
end

desc "Create the NHibernate nuget package"
nugetpack :createNHibernatePackage => :createNHibernateSpec do |nugetpack|
   nugetpack.nuspec = File.join(nhibernatePackagePath, nhibernateNuspec)
   nugetpack.base_folder = nhibernatePackagePath
   nugetpack.output = deployPath
end

desc "Push the nhibernate nuget package"
nugetpush :pushNHibernatePackage => :createNHibernatePackage do |nuget|
    nuget.apikey = nugetApiKey
    nuget.package = File.join(deployPath, "gribble.nhibernate.#{ENV['GO_PIPELINE_LABEL']}.nupkg")
end

desc "Tag the current release"
task :tagRelease do
	result = system("git", "tag", "-a", "v#{ENV['GO_PIPELINE_LABEL']}", "-m", "release-v#{ENV['GO_PIPELINE_LABEL']}")
	result = system("git", "push", "--tags")
end
