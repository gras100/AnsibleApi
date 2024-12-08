open System
open System.Text.RegularExpressions
open System.Text

// private static readonly Regex snakeCaseReplacer = new("([^_])([A-Z])", RegexOptions.Compiled);
// private static readonly string snakeCaseReplaceToken = "$1_$2";

let snakeCaseReplacer = Regex("([^_])([A-Z])", RegexOptions.Compiled)
let snakeCaseReplacerWithDigits = Regex("([^_])([A-Z]|\d+?)", RegexOptions.Compiled)
let snakeCaseReplaceToken = "$1_$2";

let snakeCaseReplaceWithDigits2 = Regex("(?:([^_])([A-Z]|\\d+?)|(?:([\d])([a-z])))", RegexOptions.Compiled);


let toSnakeCase (minPrefix: int) (v: string) =
  
    let inline (!) v = ignore v

    let inline underscore pc c = 
        (pc <> '_' && Char.IsUpper(c))
        || (Char.IsNumber(pc) <> Char.IsNumber(c))

    let from = max 1 minPrefix
    let sb = new StringBuilder(v.Length*2)
    if from > v.Length then v else
        printfn "1"
        !sb.Append(v[0..from-1])
        printfn $"2"
        for i in from .. v.Length - 1 do
            printfn $"3.{i}"
            let c = v[i]
            printfn $"4.{i}.{c}"
            if underscore v[i - 1] c
            then !sb.Append('_').Append(c)
            else !sb.Append(c)
            printfn $"4.{i}.{sb}"
        sb.ToString()
        

toSnakeCase 2 "A1b2" = "A1_b_2"
toSnakeCase 1 "ABCT10A20" = "A_B_C_T_10_A_20"
toSnakeCase 2 "ABCT10A20" = "AB_C_T_10_A_20"

snakeCaseReplacer.Replace("ABridgeTo10Or20", snakeCaseReplaceToken)

snakeCaseReplacerWithDigits.Replace("ABridgeTo10Or20", snakeCaseReplaceToken).ToLowerInvariant()
snakeCaseReplacerWithDigits.Replace("ABridgeTo10or20", snakeCaseReplaceToken).ToLowerInvariant()
snakeCaseReplaceWithDigits2.Replace("ABCT10A20", "$1$3_$2$4").ToLowerInvariant()

snakeCaseReplaceWithDigits2.Replace("ABridgeTo10Or20", snakeCaseReplaceToken).ToLowerInvariant()
snakeCaseReplaceWithDigits2.Replace("ABridgeTo10or20", "$1$3_$2$4").ToLowerInvariant()


snakeCaseReplaceWithDigits2.Replace("ABCT10A20", "$1$3_$2$4").ToLowerInvariant()

"A1B"


snakeCaseReplaceWithDigits2.Replace("ABridgeTo10or20", ":$0~$1~$2~$3~$4").ToLowerInvariant()


