using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace JayGenPackage
{
    [Guid("5BF0DC07-AA56-4A9D-AC92-CE6CE076E2D4")]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(SourceGen.Description, "", "1.1")]
    [ProvideCodeGenerator(typeof(SourceGen), SourceGen.Name, SourceGen.Description, true)]
    [ProvideCodeGeneratorExtension(SourceGen.Name, ".y")]
    [ProvideCodeGeneratorExtension(SourceGen.Name, ".abc")]
    public sealed class JayGenPackage : AsyncPackage
    {
        /// <summary>
        /// JayGenPackagePackage GUID string.
        /// </summary>
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        }

        #endregion
    }
}
