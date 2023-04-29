HazelRootDirectory = os.getenv("HAZEL_DIR")
include (path.join(HazelRootDirectory, "Hazelnut", "Resources", "LUA", "Hazel.lua"))

workspace "Taent"
	targetdir "build"
	startproject "Taent"
	
	configurations 
	{ 
		"Debug", 
		"Release",
		"Dist"
	}

group "Hazel"
project "Hazel-ScriptCore"
	location "%{HazelRootDirectory}/Hazel-ScriptCore"
	kind "SharedLib"
	language "C#"
	dotnetframework "4.7.2"

	targetdir ("%{HazelRootDirectory}/Hazelnut/Resources/Scripts")
	objdir ("%{HazelRootDirectory}/Hazelnut/Resources/Scripts/Intermediates")

	files
	{
		"%{HazelRootDirectory}/Hazel-ScriptCore/Source/**.cs",
		"%{HazelRootDirectory}/Hazel-ScriptCore/Properties/**.cs"
	}

	linkAppReferences()

	filter "configurations:Debug"
		optimize "Off"
		symbols "Default"

	filter "configurations:Release"
		optimize "On"
		symbols "Default"

	filter "configurations:Dist"
		optimize "Full"
		symbols "Off"

group ""

project "Taent"
	kind "SharedLib"
	language "C#"
	dotnetframework "4.7.2"

	targetname "Taent"
	targetdir ("%{prj.location}/Assets/Scripts/Binaries")
	objdir ("%{prj.location}/Intermediates")

	files 
	{
		"Assets/**.cs", 
	}

	linkAppReferences()

	filter "configurations:Debug"
		optimize "Off"
		symbols "Default"

	filter "configurations:Release"
		optimize "On"
		symbols "Default"

	filter "configurations:Dist"
		optimize "Full"
		symbols "Off"
