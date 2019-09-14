public partial class BuildTargets
{
    string name => "hagen";
    string company => "sidiandi";
    // string nugetPushSource => @"C:\src\local-nuget-repository";
	string nugetPushSource => @"default";
    string nugetPushSymbolSource => nugetPushSource;
}
