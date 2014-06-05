guard 'shell' do
  watch(%r{^Rakefile$}) { system "rake" }
  watch(%r{^.*\.fs$})   { system "rake" }
  watch(%r{^.*\.fsx$})  { system "./fake.sh" }
end
