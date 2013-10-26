def compile(input_files, output_file, references, target)
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
  sh "fsharpc #{input_files.join(" ")} --out:#{output_file} #{reference_args.join(" ")} --target:#{target} --resident"
end

task :default do
  files = Dir.glob("*.fs")
  files = ["Expectations.fs", "TestCollection.fs", "selftests.fs", "main.fs"].join(" ")

  compile(["Expectations.fs", "TestCollection.fs"], "fspec.core.dll", [], :library)
  compile(["selftests.fs"], "fspec.selftests.dll", ["fspec.core.dll"], :library)
  compile(["main.fs"], "fspec.exe", ["fspec.core.dll"], :exe)

  sh("mono fspec.exe fspec.selftests")
end
