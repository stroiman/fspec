require 'rake/clean'
require 'bundler/setup'
require 'albacore'
require 'albacore/tasks/versionizer'

Albacore::Tasks::Versionizer.new :versioning

CLEAN.include("output/*.dll", "output/*.exe", "output/*.mdb", "output/*.xml")

def windows?
  RUBY_PLATFORM =~ /mingw/i 
end

desc 'restore all nugets as per the packages.config files'
nugets_restore :restore do |p|
  p.out = 'packages'
  p.exe = 'nuget.exe'
end

desc 'Perform full build'
build :build => [:versioning] do |b|
  b.sln = 'FSpec.sln'
end

task :test do
  executer = ""
  executer = "mono " unless windows?
  sh("#{executer}output/fspec-runner.exe output/FSpec.SelfTests.dll")
end

directory 'output/pkg'

nugets_pack :pack => ['output/pkg', :versioning, :build] do |p|
  p.files = FileList['core/*.{fsproj,nuspec}']
  p.out = 'output/pkg'
  p.exe = 'nuget.exe'
  p.with_metadata do |m|
    m.version = ENV['NUGET_VERSION']
    m.authors = 'Peter Strøiman'
    m.license_url = 'https://raw.githubusercontent.com/PeteProgrammer/fspec/master/LICENSE'
    m.project_url = 'https://github.com/PeteProgrammer/fspec'
    m.summary = 'An F# based context/specification test framework'
    m.description = 'A context/specification test framework, heavily inspired by RSpec'
    m.copyright = 'Copyright 2014'
    m.tags = 'tdd unit test testing unittest unittesting bdd'
  end
end

task :default => [:build, :test]
