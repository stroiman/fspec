guard 'shell' do
  # watch(%r{^Rakefile$}) { system "rake quick_build" }
  watch(%r{^core/.*\.fs$})   { system "rake guard:core" }
  watch(%r{^selftests/.*\.fs$})   { system "rake guard:selftests" }
  watch(%r{^cli/.*\.fs$})   { system "rake guard:cli" }
  # watch(%r{^.*\.fs$})   { system "rake guard:default" }
  watch(%r{^.*\.fsx$})  { system "./fake.sh" }
end
