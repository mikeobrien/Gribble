require "albacore"

task :default => [:unitTests]

desc "Inits the build"
task :initBuild do
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

