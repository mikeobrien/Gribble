require "albacore"

task :default => [:deploySample]

desc "Inits the build"
task :initBuild do
end

desc "Generate assembly info."
assemblyinfo :assemblyInfo => :initBuild do |asm|
    asm.version = ENV["GO_PIPELINE_LABEL"]
    asm.company_name = "Ultraviolet Catastrophe"
    asm.product_name = "Wcf Rest Contrib"
    asm.title = "Wcf Rest Contrib"
    asm.description = "Goodies for Wcf Rest."
    asm.copyright = "Copyright (c) 2010 Ultraviolet Catastrophe"
    asm.output_file = "src/WcfRestContrib/Properties/AssemblyInfo.cs"
end

desc "Builds the library."
msbuild :buildLibrary => :setAssemblyVersion do |msb|
    msb.path_to_command = File.join(ENV['windir'], 'Microsoft.NET', 'Framework', 'v4.0.30319', 'MSBuild.exe')
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/WcfRestContrib/WcfRestContrib.csproj"
end

desc "Builds the test project."
msbuild :buildTestProject => :buildLibrary do |msb|
    msb.path_to_command = File.join(ENV['windir'], 'Microsoft.NET', 'Framework', 'v4.0.30319', 'MSBuild.exe')
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/WcfRestContrib.Tests/WcfRestContrib.Tests.csproj"
end

desc "Builds the sample app."
msbuild :buildSampleApp => :buildTestProject do |msb|
    msb.path_to_command = File.join(ENV['windir'], 'Microsoft.NET', 'Framework', 'v4.0.30319', 'MSBuild.exe')
    msb.properties :configuration => :Release
    msb.targets :Clean, :Build
    msb.solution = "src/NielsBohrLibrary/NielsBohrLibrary.csproj"
end

desc "Set assembly reference in the sample project."
task :addSampleAssemblyReference => :buildSampleApp do
    path = "src/NielsBohrLibrary/NielsBohrLibrary.csproj"
	replace = /<ProjectReference.*<\/ProjectReference>/m
	reference = "<Reference Include=\"WcfRestContrib\"><HintPath>bin\WcfRestContrib.dll</HintPath></Reference>"
    project = Common.ReadAllFileText(path)
    project = project.gsub(replace, reference)
    Common.WriteAllFileText(path, project) 
end

desc "NUnit Test Runner"
nunit :unitTests => :buildInstaller do |nunit|
	nunit.path_to_command = "lib/nunit/net-2.0/nunit-console.exe"
	nunit.assemblies "src/WcfRestContrib.Tests/bin/Release/WcfRestContrib.Tests.dll"
	nunit.options "/xml=reports/TestResult.xml"
end

desc "Inits the deploy"
task :initDeploy => :unitTests do
    Common.EnsurePath(ReleasePath)
end

