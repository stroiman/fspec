#encoding: utf-8
require 'xsemver'
require 'rake/clean'
require 'bundler/setup'
require 'albacore'
require 'albacore/tasks/versionizer'

# Avoid that Albacore uses the Jenkins supplied BUILD_NUMBER for PATCH
ENV['BUILD_NUMBER'] = nil 

Albacore::Tasks::Versionizer.new :versioning

CLEAN.include("output/*.dll", "output/*.exe", "output/*.mdb", "output/*.xml", "bin/**/*.*", "obj/**/*.*")
def run_assembly cmd
  system cmd or raise "Error running #{cmd}"
end

namespace :paket do
  file '.paket/paket.exe' do
    run_assembly ".paket/paket.bootstrapper.exe"
  end

  task :restore => '.paket/paket.exe' do
    run_assembly '.paket/paket.exe restore'
  end

  task :update => '.paket/paket.exe' do
    run_assembly '.paket/paket.exe update'
  end
end

def windows?
  RUBY_PLATFORM =~ /mingw/i 
end

asmver :asmver_fspec do |a|
  a.file_path  = 'core/AssemblyInfo.fs' 
  a.namespace  = 'FSpec'
  a.attributes assembly_title: 'FSpec',
    assembly_copyright: "(c) #{Time.now.year} by Peter Strøiman",
    assembly_version: ENV['LONG_VERSION'],
    assembly_file_version: ENV['LONG_VERSION']
end

asmver :asmver_fspec_autofoq do |a|
  a.file_path  = 'FSpec.AutoFoq/AssemblyInfo.fs' 
  a.namespace  = 'FSpec'
  a.attributes assembly_title: 'FSpec', 
    assembly_copyright: "(c) #{Time.now.year} by Peter Strøiman",
    assembly_version: ENV['LONG_VERSION'],
    assembly_file_version: ENV['LONG_VERSION']
end

asmver :asmver_fspec_mbunitwrapper do |a|
  a.file_path  = 'FSpec.MbUnitWrapper/AssemblyInfo.fs' 
  a.namespace  = 'FSpec'
  a.attributes assembly_title: 'FSpec',
    assembly_copyright: "(c) #{Time.now.year} by Peter Strøiman",
    assembly_version: ENV['LONG_VERSION'],
    assembly_file_version: ENV['LONG_VERSION']
end

task :asmver_files => [:asmver_fspec, :asmver_fspec_autofoq, :asmver_fspec_mbunitwrapper] 

desc 'Perform full build'
build :build => [:versioning, :asmver_files] do |b|
  b.sln = 'FSpec.sln'
end

task :test do
  executer = ""
  executer = "mono " unless windows?
  sh("#{executer}output/fspec-runner.exe output/FSpec.SelfTests.dll")
end

directory 'output/pkg'

task :pack => ['output/pkg', :versioning] do
  system "mono nuget.exe pack fspec.nuspec -properties version=#{ENV['NUGET_VERSION']}"
  system "mono nuget.exe pack FSpec.MbUnitWrapper.nuspec -properties version=#{ENV['NUGET_VERSION']}"
  system "mono nuget.exe pack FSpec.AutoFoq.nuspec -properties version=#{ENV['NUGET_VERSION']}"
end

def push_package(package_name)
  system "mono nuget.exe push #{package_name}.#{ENV['NUGET_VERSION']}.nupkg #{ENV['MYGET_API_KEY']} -Source https://www.myget.org/F/fspec-prereleases/api/v2/package"
end

task :push => [:pack] do
  push_package "FSpec"
  push_package "FSpec.AutoFoq"
  push_package "FSpec.MbUnitWrapper"
end

task :increment_patch do
  v = XSemVer::SemVer.find
  v.minor += 1
  v.save
  Rake::Task["versioning"].execute
end

task :increment_minor do
  v = XSemVer::SemVer.find
  v.minor += 1
  v.save
  Rake::Task["versioning"].execute
end

task :commit do
  tag_name = "v-#{ENV['NUGET_VERSION']}"
  system "git add ."
  system "git ci -m \"#{tag_name}\""
  system "git tag #{tag_name}"
  system "git push origin head:master"
  system "git push --tags"
end

task :default => [:build, :test]
task :ci => ["paket:restore", :build, :pack]
#task :create_minor => [:increment_minor, :ci, :commit]
task :create_version => [:ci, :commit, :push]
