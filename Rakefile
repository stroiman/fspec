task :default do
  files = Dir.glob("*.fs")
  files = ["Expectations.fs", "TestCollection.fs", "selftests.fs", "main.fs"]
  system "fsharpc #{files.join(" ")} --out:fspec.exe --resident"
  system "mono fspec.exe" if $? == 0
end
