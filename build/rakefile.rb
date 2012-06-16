require "albacore"
require_relative "filesystem"
require_relative "gallio-task"

reportsPath = "reports"
version = ENV["BUILD_NUMBER"]

task :build => [:createCorePackage, :createNHibernatePackage]
task :pushPackages => [:pushCorePackage, :pushNHibernatePackage]

assemblyinfo :coreAssemblyInfo do |asm|
    asm.version = version
    asm.company_name = "Ultraviolet Catastrophe"
    asm.product_name = "Gribble"
    asm.title = "Gribble"
    asm.description = "Gribble ORM"
    asm.copyright = "Copyright (c) 2011 Ultraviolet Catastrophe"
    asm.output_file = "src/Gribble/Properties/AssemblyInfo.cs"
end

msbuild :buildCore => :coreAssemblyInfo do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Gribble/Gribble.csproj"
end

assemblyinfo :nhibernateAssemblyInfo do |asm|
    asm.version = version
    asm.company_name = "Ultraviolet Catastrophe"
    asm.product_name = "Gribble NHibernate Integration"
    asm.title = "Gribble NHibernate Integration"
    asm.description = "Gribble NHibernate Integration"
    asm.copyright = "Copyright (c) 2011 Ultraviolet Catastrophe"
    asm.output_file = "src/Gribble.NHibernate/Properties/AssemblyInfo.cs"
end

msbuild :buildNHibernate => [:buildCore, :nhibernateAssemblyInfo] do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Gribble/Gribble.csproj"
end

msbuild :buildTestProject => [:buildCore, :buildNHibernate] do |msb|
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/Tests/Tests.csproj"
end

task :unitTestInit do
	FileSystem.EnsurePath(reportsPath)
end

gallio :unitTests => [:buildTestProject, :unitTestInit] do |runner|
	runner.echo_command_line = true
	runner.add_test_assembly("src/Tests/bin/Release/Tests.dll")
	runner.verbosity = 'Normal'
	runner.report_directory = reportsPath
	runner.report_name_format = 'tests'
	runner.add_report_type('Html')
end

nugetApiKey = ENV["NUGET_API_KEY"]
deployPath = "deploy"

corePackagePath = File.join(deployPath, "corepackage")
coreNuspec = "gribble.nuspec"
corePackageLibPath = File.join(corePackagePath, "lib")
coreBinPath = "src/Gribble/bin/Release"

nhibernatePackagePath = File.join(deployPath, "nhpackage")
nhibernateNuspec = "gribble.nhibernate.nuspec"
nhibernatePackageLibPath = File.join(nhibernatePackagePath, "lib")
nhibernateBinPath = "src/Gribble.NHibernate/bin/Release"

desc "Prep the packages"
task :prepPackages => :unitTests do
	FileSystem.DeleteDirectory(deployPath)
	
	FileSystem.EnsurePath(corePackageLibPath)
	FileSystem.CopyFiles(File.join(coreBinPath, "Gribble.dll"), corePackageLibPath)
	FileSystem.CopyFiles(File.join(coreBinPath, "Gribble.pdb"), corePackageLibPath)
	
	FileSystem.EnsurePath(nhibernatePackageLibPath)
	FileSystem.CopyFiles(File.join(nhibernateBinPath, "Gribble.NHibernate.dll"), nhibernatePackageLibPath)
	FileSystem.CopyFiles(File.join(nhibernateBinPath, "Gribble.NHibernate.pdb"), nhibernatePackageLibPath)
end

nuspec :createCoreSpec => :prepPackages do |nuspec|
   nuspec.id = "gribble"
   nuspec.version = version
   nuspec.authors = "Mike O'Brien"
   nuspec.owners = "Mike O'Brien"
   nuspec.title = "Gribble ORM"
   nuspec.description = "Gribble is a simple, Linq enabled ORM designed to work with dynamically created tables."
   nuspec.summary = "Gribble is a simple, Linq enabled ORM designed to work with dynamically created tables."
   nuspec.language = "en-US"
   nuspec.licenseUrl = "https://github.com/mikeobrien/Gribble/blob/master/LICENSE"
   nuspec.projectUrl = "https://github.com/mikeobrien/Gribble"
   nuspec.iconUrl = "https://github.com/mikeobrien/Gribble/raw/master/misc/gribble.png"
   nuspec.working_directory = corePackagePath
   nuspec.output_file = coreNuspec
   nuspec.tags = "orm dal sql"
end

nugetpack :createCorePackage => :createCoreSpec do |nugetpack|
   nugetpack.nuspec = File.join(corePackagePath, coreNuspec)
   nugetpack.base_folder = corePackagePath
   nugetpack.output = deployPath
end

nugetpush :pushCorePackage => :createCorePackage do |nuget|
    nuget.apikey = nugetApiKey
    nuget.package = File.join(deployPath, "gribble.#{version}.nupkg")
end

nuspec :createNHibernateSpec => :prepPackages do |nuspec|
   nuspec.id = "gribble.nhibernate"
   nuspec.version = version
   nuspec.authors = "Mike O'Brien"
   nuspec.owners = "Mike O'Brien"
   nuspec.title = "Gribble ORM NHibernate Integration"
   nuspec.description = "Gribble NHibernate integration."
   nuspec.summary = "Gribble NHibernate integration."
   nuspec.language = "en-US"
   nuspec.licenseUrl = "https://github.com/mikeobrien/Gribble/blob/master/LICENSE"
   nuspec.projectUrl = "https://github.com/mikeobrien/Gribble"
   nuspec.iconUrl = "https://github.com/mikeobrien/Gribble/raw/master/misc/gribble.png"
   nuspec.working_directory = nhibernatePackagePath
   nuspec.output_file = nhibernateNuspec
   nuspec.tags = "orm dal sql nhibernate"
   nuspec.dependency "gribble", version
   nuspec.dependency "NHibernate", "3.1.0.4000"
end

nugetpack :createNHibernatePackage => :createNHibernateSpec do |nugetpack|
   nugetpack.nuspec = File.join(nhibernatePackagePath, nhibernateNuspec)
   nugetpack.base_folder = nhibernatePackagePath
   nugetpack.output = deployPath
end

nugetpush :pushNHibernatePackage => :createNHibernatePackage do |nuget|
    nuget.apikey = nugetApiKey
    nuget.package = File.join(deployPath, "gribble.nhibernate.#{version}.nupkg")
end