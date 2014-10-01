require 'rake/clean'

CLEAN.include("output/*.dll", "output/*.exe")

def windows?
  RUBY_PLATFORM =~ /mingw/i 
end

def compile(output_file, prerequisites, target)
  dir = File.dirname(output_file)
  Dir.mkdir(dir) unless Dir.exists?(dir)
  fs_files = prerequisites.select {|x| File.extname(x) == '.fs'}
  dll_files = prerequisites.select {|x| File.extname(x) == '.dll'}
  package_files = dll_files.select {|x| x.start_with? "package"}
  cp package_files, 'output/'
  reference_args = dll_files.map {|x| "--reference:#{x}" }
  if windows?
    fsc = 'fsc'
  else
    fsc = 'fsharpc'
  end
  sh "#{fsc} #{fs_files.join(" ")} --out:#{output_file} #{reference_args.join(" ")} --target:#{target}"
end

file 'output/FSpec.dll' => [
    'core/DomainTypes.fs',
    'core/TestContext.fs',
    'core/TestReport.fs', 
    'core/Runner.fs',
    'core/Matchers.fs', 
    'core/Dsl.fs',
    'core/TestDiscovery.fs',
    'core/Compatibility.fs',
    'core/AssemblyInfo.fs'
    ] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/FSpec.MbUnitWrapper.dll' => [
    'FSpec.MbUnitWrapper/TestFactory.fs',
    'output/FSpec.dll',
    'packages/mbunit.3.3.454.0/lib/net40/Gallio.dll',
    'packages/mbunit.3.3.454.0/lib/net40/Gallio40.dll',
    'packages/mbunit.3.3.454.0/lib/net40/MbUnit.dll',
    'packages/mbunit.3.3.454.0/lib/net40/MbUnit40.dll',
    ] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/FSpec.AutoFoq.dll' => [
    'FSpec.AutoFoq/FoqMockingKernel.fs',
    'FSpec.AutoFoq/TestContext.fs',
    'FSpec.AutoFoq/AssemblyInfo.fs',
    'output/FSpec.dll',
    'packages/Ninject.MockingKernel.3.2.0.0/lib/net45-full/Ninject.MockingKernel.dll',
    'packages/Ninject.3.2.0.0/lib/net45-full/Ninject.dll',
    'packages/Foq.1.6/Lib/net45/Foq.dll'
    ] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/FSpec.SelfTests.dll' => [
    'selftests/Helpers.fs',
    'selftests/ExampleHelper.fs',
    'selftests/CustomMatchers.fs',
    'selftests/DomainTypesSpecs.fs',
    'selftests/RunnerSpecs.fs', 
    'selftests/TestRunnerSpecs.fs',
    'selftests/MatchersSpecs.fs',
    'selftests/TestReportSpecs.fs',
    'selftests/MetaDataSpecs.fs',
    'selftests/DslSpecs.fs',
    'selftests/QuotationExampleSpecs.fs',
    'selftests/TestContextSpecs.fs',
    'selftests/AutoFoqSpecs.fs',
    'selftests/MbUnitWrapperSpecs.fs',
    'packages/Ninject.MockingKernel.3.2.0.0/lib/net45-full/Ninject.MockingKernel.dll',
    'packages/Ninject.3.2.0.0/lib/net45-full/Ninject.dll',
    'packages/Foq.1.6/Lib/net45/Foq.dll',
    'output/FSpec.AutoFoq.dll',
    'output/FSpec.MbUnitWrapper.dll',
    'output/FSpec.dll',
    'packages/mbunit.3.3.454.0/lib/net40/Gallio.dll',
    'packages/mbunit.3.3.454.0/lib/net40/Gallio40.dll',
    'packages/mbunit.3.3.454.0/lib/net40/MbUnit.dll',
    'packages/mbunit.3.3.454.0/lib/net40/MbUnit40.dll',
    ] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/fspec-runner.exe' => ['cli/Main.fs', 'output/FSpec.dll'] do |t|
  compile(t.name, t.prerequisites, :exe)
end

task :build => ['output/fspec-runner.exe','output/FSpec.AutoFoq.dll'] do
end

task :test => ['output/fspec-runner.exe', 'output/FSpec.SelfTests.dll'] do
  executer = ""
  executer = "mono " unless windows?
  sh("#{executer}output/fspec-runner.exe output/FSpec.SelfTests.dll")
end

task :default => [:build, :test]
