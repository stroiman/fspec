require 'rake/clean'

CLEAN.include("**/*.dll", "**/*.exe")

def compile(output_file, prerequisites, target)
  dir = File.dirname(output_file)
  Dir.mkdir(dir) unless Dir.exists?(dir)
  fs_files = prerequisites.select {|x| File.extname(x) == '.fs'}
  dll_files = prerequisites.select {|x| File.extname(x) == '.dll'}
  reference_args = dll_files.map {|x| "--reference:#{x}" }
  sh "fsharpc #{fs_files.join(" ")} --out:#{output_file} #{reference_args.join(" ")} --target:#{target} --resident"
end

file 'output/FSpec.Core.dll' => ['core/Expectations.fs', 'core/TestCollection.fs'] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/FSpec.SelfTests.dll' => ['selftests/Selftests.fs', 'output/FSpec.Core.dll'] do |t|
  compile(t.name, t.prerequisites, :library)
end

file 'output/fspec.exe' => ['cli/Main.fs', 'output/FSpec.Core.dll'] do |t|
  compile(t.name, t.prerequisites, :exe)
end

task :build => ['output/fspec.exe'] do
end

task :test => ['output/fspec.exe', 'output/FSpec.SelfTests.dll'] do
  sh("mono output/fspec.exe FSpec.SelfTests")
end

task :default => [:build, :test]
