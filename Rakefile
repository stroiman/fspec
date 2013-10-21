task :default do
  files = Dir.glob("*.fs").join(" ")
  system "fsharpc #{files} --out:fspec.exe --resident"
  system "mono fspec.exe" if $? == 0
end
