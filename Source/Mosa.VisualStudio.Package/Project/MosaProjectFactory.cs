using Microsoft.VisualStudio.Project;
using System;
using System.Runtime.InteropServices;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Mosa.VisualStudio.Package.Project
{
    [Guid(Guids.ProjectFactory)]
    class MosaProjectFactory : ProjectFactory
    {
        #region Fields

        private MosaPackage package;

        #endregion

        #region Constructors
        /// <summary>
        /// Explicit default constructor.
        /// </summary>
        /// <param name="package">Value of the project package for initialize internal package field.</param>
        public MosaProjectFactory(MosaPackage package)
            : base(package)
        {
            this.package = package;
        }

        #endregion

        #region Overriden implementation

        /// <summary>
        /// Creates a new project by cloning an existing template project.
        /// </summary>
        /// <returns></returns>
        protected override ProjectNode CreateProject()
        {
            MosaProjectNode project = new MosaProjectNode(this.package);
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return project;
        }

        #endregion
    }
}
