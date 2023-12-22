open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

let packageDir = "./package"
let publishDir = "./publish"
let mainSolution = "./kvps-cli.sln"

let runNumber =
    (match Fake.BuildServer.GitHubActions.Environment.CI false with
     | true -> Fake.BuildServer.GitHubActions.Environment.RunNumber
     | _ -> "0")

let commitSha = Fake.BuildServer.GitHubActions.Environment.Sha

let versionSuffix =
    match Fake.BuildServer.GitHubActions.Environment.Ref with
    | null
    | "refs/heads/main" -> ""
    | _ -> "-preview"

let version = sprintf "0.1.%s%s" runNumber versionSuffix

let infoVersion =
    match commitSha with
    | null -> version
    | sha -> sprintf "%s.%s" version sha

let fileName (path: string) =
    System.IO.Path.GetFileNameWithoutExtension(path)

let (==>!) x y = x ==> y |> ignore

let (?=>!) x y = x ?=> y |> ignore


let assemblyInfoParams (buildParams) =
    [ ("Version", version); ("AssemblyInformationalVersion", infoVersion) ]
    |> List.append buildParams

let codeCoverageParams (buildParams) =
    [ ("CollectCoverage", "true")
      ("CoverletOutput", "./TestResults/coverage.info")
      ("CoverletOutputFormat", "cobertura") ]
    |> List.append buildParams

let packBuildParams (buildParams) =
    [ ("PackageVersion", version) ] |> List.append buildParams

let buildOptions =
    fun (opts: DotNet.BuildOptions) ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Release
            MSBuildParams =
                { opts.MSBuildParams with
                    Properties = assemblyInfoParams opts.MSBuildParams.Properties
                    WarnAsError = Some [ "*" ] } }

let restoreOptions = fun (opts: DotNet.RestoreOptions) -> opts

let testOptions (opts: DotNet.TestOptions) =
    let properties = codeCoverageParams opts.MSBuildParams.Properties

    { opts with
        NoBuild = false
        Configuration = DotNet.BuildConfiguration.Debug
        Logger = Some "trx;LogFileName=test_results.trx"
        Filter = Some "OS!=Windows"
        MSBuildParams =
            { opts.MSBuildParams with
                Properties = properties } }

let packOptions =
    fun (opts: DotNet.PackOptions) ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Release
            NoBuild = false
            MSBuildParams =
                { opts.MSBuildParams with
                    Properties = (packBuildParams opts.MSBuildParams.Properties |> assemblyInfoParams) }
            OutputPath = Some packageDir }

let publishOptions =
    fun (name: string) (opts: DotNet.PublishOptions) ->
        { opts with
            Configuration = DotNet.BuildConfiguration.Release
            NoBuild = false
            MSBuildParams =
                { opts.MSBuildParams with
                    Properties = (packBuildParams opts.MSBuildParams.Properties |> assemblyInfoParams) }
            OutputPath = Some(System.IO.Path.Combine(publishDir, name)) }

let publishProjects = !! "src/**/kvps.fsproj" |> List.ofSeq

let initTargets () =

    Target.create "Clean" (fun _ ->
        !! "src/**/bin" ++ "src/**/obj" |> Shell.cleanDirs

        !! "tests/**/bin" ++ "tests/**/obj" ++ "tests/**/TestResults" ++ publishDir ++ packageDir
        |> Shell.cleanDirs)

    Target.create "Restore" (fun _ -> !!mainSolution |> Seq.iter (DotNet.restore restoreOptions))

    Target.create "Compile" (fun _ -> !!mainSolution |> Seq.iter (DotNet.build buildOptions))

    Target.create "Pack" (fun _ -> publishProjects |> Seq.iter (DotNet.pack packOptions))

    Target.create "Publish" (fun _ ->
        publishProjects
        |> Seq.iter (fun p ->
            let output = fileName p
            let opts = publishOptions output
            DotNet.publish opts p))

    Target.create "Unit Tests" (fun _ -> !! "tests/**/*.fsproj" |> Seq.iter (DotNet.test testOptions))

    Target.create "Consolidate code coverage" (fun _ ->
        let args =
            sprintf
                @"-reports:""./tests/**/coverage.info"" -targetdir:""./%s/codecoverage"" -reporttypes:""Html"""
                publishDir

        let result = DotNet.exec id "reportgenerator" args

        if not result.OK then
            failwithf "reportgenerator failed!")

    Target.create "SCA" (fun _ ->
        let args = "-t"
        let result = DotNet.exec id "pkgchk" args

        if not result.OK then
            failwithf "pkgchk failed!")

    Target.create "Check Style Rules" (fun _ ->
        let args = "--recurse --check ./src/ ./tests/"
        let result = DotNet.exec id "fantomas" args

        if result.OK then
            Trace.log "No files need formatting"
        else
            failwith "Reformatting needed. Run build as 'Apply Style Rules'")

    Target.create "Apply Style Rules" (fun _ ->
        let args = "--recurse ./src/ ./tests/"
        let result = DotNet.exec id "fantomas" args

        if result.OK then
            Trace.log "No files need formatting"
        else
            failwith "Error reformatting!")

    Target.create "Echo variables" (fun _ -> version |> sprintf "Build number: %s" |> Trace.traceImportant)

    Target.create "Build" ignore
    Target.create "Tests" ignore

    Target.create "All" ignore

    "Clean" ?=>! "Restore"

    "Clean" ==> "Restore" ==> "Compile" ==> "Pack" ==> "Echo variables"
    ==>! "Build"

    "Restore" ==> "Unit Tests" ==> "Consolidate code coverage" ==>! "Tests"

    "Restore" ==> "SCA" ==>! "All"

    "Restore" ==> "Check Style Rules" ==>! "All"

    "Build" ==>! "All"

    "Tests" ==>! "All"

//-----------------------------------------------------------------------------
// Target Start
//-----------------------------------------------------------------------------
[<EntryPoint>]
let main argv =
    argv
    |> Array.toList
    |> Context.FakeExecutionContext.Create false "build.fsx"
    |> Context.RuntimeContext.Fake
    |> Context.setExecutionContext

    initTargets () |> ignore
    Target.runOrDefaultWithArguments ("All")

    0 // return an integer exit code
