task :default do
  files = Dir.glob("*.fs")
  files = ["Expectations.fs", "TestCollection.fs", "selftests.fs", "main.fs"].join(" ")
  system "fsharpc Expectations.fs TestCollection.fs --out:fspec.core.dll --target:library --resident"
  system "fsharpc Selftests.fs --out:fspec.selftests.dll --reference:fspec.core.dll --target:library --resident"
  system "fsharpc main.fs --out:fspec.exe --reference:fspec.core.dll --reference:fspec.selftests.dll --resident"
  system "mono fspec.exe" if $? == 0
end
