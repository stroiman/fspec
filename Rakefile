#encoding: utf-8
require 'rake/clean'
require 'bundler/setup'
require 'albacore'
require 'albacore/tasks/versionizer'
#
# Albacore uses the Jenkins supplied BUILD_NUMBER for PATCH
ENV['BUILD_NUMBER'] = nil 

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

task :pack => ['output/pkg', :versioning] do
  system "mono nuget.exe pack fspec.nuspec -properties version=#{ENV['NUGET_VERSION']}"
  system "mono nuget.exe pack FSpec.MbUnitWrapper.nuspec -properties version=#{ENV['NUGET_VERSION']}"
  system "mono nuget.exe pack FSpec.AutoFoq.nuspec -properties version=#{ENV['NUGET_VERSION']}"
end

task :push => [:pack] do
  system "mono nuget.exe push FSpec.#{ENV['NUGET_VERSION']}.nuget -ApiKey #{ENV['NUGET_API_KEY']}"
  system "mono nuget.exe push FSpec.AutoFoq.#{ENV['NUGET_VERSION']}.nuget -ApiKey #{ENV['NUGET_API_KEY']}"
  system "mono nuget.exe push FSpec.MbUnitWrapper.#{ENV['NUGET_VERSION']}.nuget -ApiKey #{ENV['NUGET_API_KEY']}"
end

task :default => [:build, :test]
task :ci => [:restore, :pack]
