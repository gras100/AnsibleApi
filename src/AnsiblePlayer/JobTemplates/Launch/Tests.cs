using System;
using System.Diagnostics.Contracts;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;


using plenidev.AnsiblePlayer;
using plenidev.AnsiblePlayer.Core;

namespace plenidev.AnsiblePlayer
{
    public static partial class JobTemplates
    {
        public static partial class Launch
        {
            static public class Tests
            {
                public class TestExtraVars
                {
                    public int? About20OfThem { get; init; } = 21;
                    public string? IThoughtMore { get; set; } = "I thid thos.";
                }

                static public void RunTests()
                {
                    var options = new PostOptionsCommon<TestExtraVars>()
                    {
                        ExtraVars = new TestExtraVars()
                        {
                            IThoughtMore = @"C:\Program Files\Other Stuff<between>""ua"" then 'me'"
                        },
                        Verbosity = Core.AnsibleChoices.Verbosity.WinRmDebug
                    } as IAnsibleOptions;

                    Console.Write("Wire serialization: ");
                    Console.WriteLine(options.ToJsonDataString());
                    Console.Write("Pretty Serialization: ");
                    Console.WriteLine(options.ToJsonDataString(pretty: true));

                }
            }
        }
    }
     
}
