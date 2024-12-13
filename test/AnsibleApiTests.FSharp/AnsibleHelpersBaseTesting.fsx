
#r "bin/Debug/netstandard2.0/plenidev.AnsibleHelpers.dll"

open plenidev.AnsibleHelpers;

JobTemplates.Launch.Tests.RunTests();

let inline init f x = 
    f x; x

type ExtraVars() =
    member val X = 1 with get, set
    member val About20OfThem = 21 with get, set


    
let x = Tests.TestExtraVars(About20OfThem=1, IThoughtMore="xy")


let postOps = JobTemplates.Launch.PostOptionsAll<ExtraVars>()

    