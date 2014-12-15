guard 'shell' do
  watch(%r{^Rakefile$}) { system "rake quick_build" }
  watch(%r{^.*\.fs$})   { system "rake guard:default" }
  watch(%r{^.*\.fsx$})  { system "./fake.sh" }
end
