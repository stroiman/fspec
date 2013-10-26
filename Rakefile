require 'rake/clean'

CLEAN.include("**/*.dll", "**/*.exe")

def compile(options)
  options = { references: [] }.merge(options)
  input_files = options[:input_files]
  input_folder = options[:input_folder]
  input_files.map! do |file|
    if input_folder then
      "#{input_folder}/#{file}"
    else
      file
    end
  end
  output_file = options[:output_file]
  references = options[:references].map! { |file| "output/#{file}" }
  target = options[:target]
  dependencies = input_files + references

  if (File.exists?(output_file))
    dependency_mtimes = dependencies.map { |x| File.open(x).mtime }
    output_mtime = File.open(output_file).mtime 
    changed_files_count = dependency_mtimes.count { |x| x > output_mtime }
    if changed_files_count == 0
      return
    end
  end
  reference_args = references.map {|x| "--reference:#{x}" }
  sh "fsharpc #{input_files.join(" ")} --out:output/#{output_file} #{reference_args.join(" ")} --target:#{target} --resident"
end

task :build do
  compile(
    input_folder: "core",
    input_files: ["Expectations.fs", "TestCollection.fs"],
    output_file: "FSpec.Core.dll",
    target: :library)
  compile(
    input_folder: "selftests",
    input_files: ["selftests.fs"],
    output_file: "FSpec.SelfTests.dll",
    references: ["FSpec.Core.dll"],
    target: :library)
  compile(
    input_folder: "cli",
    input_files: ["Main.fs"],
    output_file: "fspec.exe",
    references: ["FSpec.Core.dll"],
    target: :exe)
end

task :test do
  sh("mono output/fspec.exe fspec.selftests")
end

task :default => [:build, :test]
