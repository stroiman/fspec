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

file 'output/FSpec.Core.dll' => [
    'core/DomainTypes.fs',
    'core/TestReport.fs', 
    'core/Runner.fs',
    'core/Matchers.fs', 
    'core/MatchersV3.fs',
    'core/Dsl.fs',
    'core/TestDiscovery.fs',
    'core/AssemblyInfo.fs'
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
    'selftests/MatchersV3Specs.fs',
    'selftests/TestReportSpecs.fs',
    'selftests/MetaDataSpecs.fs',
    'selftests/DslSpecs.fs',
    'selftests/QuotationExampleSpecs.fs',
    'selftests/TestContextSpecs.fs',
    'output/FSpec.Core.dll'
    ] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/fspec.exe' => ['cli/Main.fs', 'output/FSpec.Core.dll'] do |t|
  compile(t.name, t.prerequisites, :exe)
end

task :build => ['output/fspec.exe'] do
end

task :test => ['output/fspec.exe', 'output/FSpec.SelfTests.dll'] do
  executer = ""
  executer = "mono " unless windows?
  sh("#{executer}output/fspec.exe output/FSpec.SelfTests.dll")
end

task :default => [:build, :test]
