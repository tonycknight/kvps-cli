namespace kvps

open System
open NuGet.Versioning
open NuGet.Common
open NuGet.Configuration
open NuGet.Protocol
open NuGet.Protocol.Core.Types

module NugetClient =
    let getLatestVersion () =
        try
            let cnxToken = System.Threading.CancellationToken.None
            let logger = new NuGet.Common.NullLogger()
            use cache = new SourceCacheContext()

            let sourceRepository =
                Repository.Factory.GetCoreV3(new PackageSource(NuGetConstants.V3FeedUrl))

            let mdr = sourceRepository.GetResource<MetadataResource>()

            let vsn =
                mdr.GetLatestVersion("kvps-cli", false, false, cache, logger, cnxToken).Result

            if vsn.IsPrerelease then
                new System.Version()
            else
                new System.Version(vsn.Major, vsn.Minor, vsn.Patch)
        with _ ->
            new System.Version()
