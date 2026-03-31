using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TestGenerator
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(TestGeneratorPackage.PackageGuidString)]

    [InstalledProductRegistration("Single File Generator Sample", "", "1.0")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideCodeGenerator(typeof(MinifyCodeGenerator), MinifyCodeGenerator.Name, MinifyCodeGenerator.Description, true)]
    [ProvideUIContextRule("69760bd3-80f0-4901-818d-c4656aaa08e9", // Must match the GUID in the .vsct file
        name: "UI Context",
        expression: "js | css | html", // This will make the button only show on .js, .css and .htm(l) files
        termNames: new[] { "js", "css", "html" },
        termValues: new[] { "HierSingleSelectionName:.js$", "HierSingleSelectionName:.css$", "HierSingleSelectionName:.html?$" })]
    public sealed class TestGeneratorPackage : AsyncPackage
    {
        /// <summary>
        /// TestGeneratorPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "86b65b0b-8df4-4ce9-81c9-90d2859a925c";

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

            await ApplyCustomTool.InitializeAsync(this);
        }
    }

    internal sealed class ApplyCustomTool
    {
        private const int _commandId = 0x0100;
        private static readonly Guid _commandSet = new Guid("4aaf93c0-70ae-4a4b-9fb6-1ad3997a9adf");
        private static DTE _dte;

        public static async Task InitializeAsync(AsyncPackage package)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();

            _dte = await package.GetServiceAsync(typeof(DTE)) as DTE;

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as IMenuCommandService;
            var cmdId = new CommandID(_commandSet, _commandId);

            var cmd = new OleMenuCommand(OnExecute, cmdId)
            {
                // This will defer visibility control to the VisibilityConstraints section in the .vsct file
                Supported = false
            };

            commandService.AddCommand(cmd);
        }

        private static void OnExecute(object sender, EventArgs e)
        {
            ProjectItem item = _dte.SelectedItems.Item(1).ProjectItem;

            if (item != null)
            {
                item.Properties.Item("CustomTool").Value = MinifyCodeGenerator.Name;
            }
        }
    }

    [Guid("cffb7601-6a1b-4f28-a2d0-a435e6686a2e")]
    public sealed class MinifyCodeGenerator : BaseCodeGeneratorWithSite
    {
        public Type TargetType { get; set; }

        public override string GetDefaultExtension()
        {
            var item = GetService(typeof(ProjectItem)) as ProjectItem;
            return ".min" + Path.GetExtension(item?.FileNames[1]);
        }

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            var generator = new Frank.Libraries.CodeGeneration.Generators.TestGenerator();



            generator.Generate(TargetType.Namespace + ".Tests", )


        }
    }
}
