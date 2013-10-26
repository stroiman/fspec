task :default do
  files = Dir.glob("*.fs")
  files = ["Expectations.fs", "TestCollection.fs", "selftests.fs", "main.fs"].join(" ")
  sh "fsharpc Expectations.fs TestCollection.fs --out:fspec.core.dll --target:library --resident"
  sh "fsharpc Selftests.fs --out:fspec.selftests.dll --reference:fspec.core.dll --target:library --resident"
  sh "fsharpc main.fs --out:fspec.exe --reference:fspec.core.dll --resident"
  sh "mono fspec.exe fspec.selftests" if $? == 0
end
