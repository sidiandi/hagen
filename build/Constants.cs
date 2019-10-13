namespace Build
{
    public partial class Program
    {
        string name => "hagen";
        string company => "sidiandi";
        // string nugetPushSource => @"C:\src\local-nuget-repository";
        string nugetPushSource => @"default";
        string nugetPushSymbolSource => nugetPushSource;
    }
}