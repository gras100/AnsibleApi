#r "bin/Debug/netstandard2.0/plenidev.Ansible.Api.dll"

open System.Text.RegularExpressions

open plenidev.Ansible

module ExtraVariablesBaseTests =

    [<AutoOpen>]
    module Types =

        type BaseTester(setOnce, options) = 
            inherit Api.AnsibleExtraVarsBase<BaseTester>(setOnce, options)
            static let t = Unchecked.defaultof<BaseTester>

            member public __.TestBaseGet(key) = __.BaseGet(key)

            member public __.TestBaseGetByMember() = __.BaseGet()

            member public __.TestBaseSetConcatOnly(key: string, value) = __.BaseSet(key, value)

            member public __.TestBaseSetConcatOnlyByMember(value) = __.BaseSet(value)


        type SimpleEnvVars(setOnce, options) = 
            inherit Api.AnsibleExtraVarsBase<SimpleEnvVars>(setOnce, options)
            static let t = Unchecked.defaultof<SimpleEnvVars>
            // 
            member __.SetThroughValuePropForKeyNameof = 
                __.BaseGetForKey(nameof(t.SetThroughValuePropForKeyNameof))

            member __.SetThroughValuePropByMemberName = 
                __.BaseGet()

            //
            member __.NonConcatPropForKeyNameof
                with get() = __.BaseGetForKey(nameof(t.NonConcatPropForKeyNameof))
                 and set(value) = __.BaseSet(nameof(t.NonConcatPropForKeyNameof), value)

            member __.ConcatPropForKeyNameof
                with get() = __.BaseGetForKey(nameof(t.ConcatPropForKeyNameof))
                 and set(value) = __.BaseSetConcatOnly(nameof(t.ConcatPropForKeyNameof), value)

            //

            member __.ConcatPropByMemberName
                with get() = __.BaseGet()
                 and set(value) = __.BaseSetConcatOnly(value)
            //
            member __.NonConcatPropByMemberName
                with get() = __.BaseGet()
                 and set(value) = __.BaseSet(value)

            //

            member __.A1b2C30d40x60
                with get() = __.BaseGet()
                 and set(value) = __.BaseSet(value)

    module OptionInsts =

        let prfxNumLower = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = true,
            SeparateNumbers = true,
            ConvertToLower = true
            )

        let prfxNum = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = true,
            SeparateNumbers = true,
            ConvertToLower = false
            )

        let prfxLower = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = true,
            SeparateNumbers = false,
            ConvertToLower = true
            )

        let numLower = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = false,
            SeparateNumbers = true,
            ConvertToLower = true
            )

        let prfx = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = true,
            SeparateNumbers = false,
            ConvertToLower = false
            )

        let num = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = false,
            SeparateNumbers = true,
            ConvertToLower = false
            )

        let cased = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = false,
            SeparateNumbers = false,
            ConvertToLower = true
            )

        let allOff = Api.AnsibleExtraVarNameOptions(
            AllowSingleCharacterPrefix = false,
            SeparateNumbers = false,
            ConvertToLower = false
            )


    module Tests = 

        [<AutoOpen>]
        module Helpers = 

            let inline nameTest (name: string) (n, v: Api.AnsibleExtraVar) = 
                if v.Name = name 
                then true, (n, "Pass")
                else false, (n, $"Failed: {v.Name} vs {name}.")

        let testPrfxNumLower() = 

            let opts = Api.AnsibleExtraVarNameOptions(
                AllowSingleCharacterPrefix = true,
                SeparateNumbers = true,
                ConvertToLower = true
                )

            let bs = BaseTester(setOnce = false, options = opts)

            [
            ("abcde", "abcde")
            ("ABCDE", "a_b_c_d_e")
            ("a1b2c", "a_1_b_2_c")
            ("A1B2C", "a_1_b_2_c")
            ("a10cd", "a_10_cd")
            ("A10CD", "a_10_c_d")
            ("abc10", "abc_10")
            ("AbCdE", "ab_cd_e")
            ("a_b_C10D", "a_b_c_10_d")
            ("_AbCd", "_ab_cd")
            ("_10_bcde", "_10_bcde")
            ]
            |> Seq.mapi (fun i (key, name) -> 
                (i, bs.TestBaseGet(key)) |> nameTest name    
                )
            |> Seq.filter (fun (passed, _) -> passed = false)
            |> Array.ofSeq
            

        let testMultiSetVars() = 
            ()
           

open ExtraVariablesBaseTests.Types
open ExtraVariablesBaseTests.OptionInsts

ExtraVariablesBaseTests.Tests.testPrfxNumLower()

let envar = SimpleEnvVars(setOnce = false, options = prfxNumLower)


envar.ConcatPropByMemberName.Value <- ""
envar.ConcatPropByMemberName
envar.NonConcatPropForKeyNameof + "xx"
