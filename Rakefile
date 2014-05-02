require 'rake/clean'

CLEAN.include("**/*.dll", "**/*.exe")

def windows?
  RUBY_PLATFORM =~ /mingw/i 
end

def compile(output_file, prerequisites, target)
  dir = File.dirname(output_file)
  Dir.mkdir(dir) unless Dir.exists?(dir)
  fs_files = prerequisites.select {|x| File.extname(x) == '.fs'}
  dll_files = prerequisites.select {|x| File.extname(x) == '.dll'}
  reference_args = dll_files.map {|x| "--reference:#{x}" }
  if windows?
    fsc = 'fsc'
  else
    fsc = 'fsharpc --resident'
  end
  sh "#{fsc} #{fs_files.join(" ")} --out:#{output_file} #{reference_args.join(" ")} --target:#{target}"
end

file 'output/FSpec.Core.dll' => [
    'core/DomainTypes.fs',
    'core/TestReport.fs', 
    'core/Matchers.fs', 
    'core/Dsl.fs',
    'core/DslV2.fs',
    'core/TestDiscovery.fs'
    ] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/FSpec.SelfTests.dll' => [
    'selftests/DslHelper.fs', 
    'selftests/RunnerSpecs.fs', 
    'selftests/TestRunnerSpecs.fs',
    'selftests/Selftests.fs', 
    'selftests/MatcherSpecs.fs', 
    'selftests/TestReportSpecs.fs',
    'selftests/MetaDataSpecs.fs',
    'selftests/DslV2Specs.fs',
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
