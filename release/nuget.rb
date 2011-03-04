require "yaml"

class NugetPush
    attr_accessor :apiKey, :package

    def run()
		if (@apiKey.nil?) @apiKey = YAML::load(File.open(ENV["USERPROFILE"] + "\.nuget\credentials"))["api_key"]
		puts @apiKey
		#system("nuget", "push", "-source", @package, @apiKey)
    end
end

def nugetpush(*args, &block)
    body = lambda { |*args|
        rc = NugetPush.new
        block.call(rc)
        rc.run
    }
    Rake::Task.define_task(*args, &body)
end