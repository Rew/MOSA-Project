using Microsoft.VisualStudio.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MSBuild = Microsoft.Build.Evaluation;

namespace Mosa.VisualStudio.Package.Project
{
    [Guid(Guids.ProjectNode)]
    class MosaProjectNode : ProjectNode
    {
        private const string PROJECT_TYPE_NAME = "WitchCraft Project Name";
        private MosaPackage _package;
        private ReferenceContainerNode _referencesNode = null;

        public MosaProjectNode(MosaPackage package)
        {
            this._package = package;
        }

        #region overrides

        /// <summary>
        /// Gets the project GUID.
        /// </summary>
        /// <value>The project GUID.</value>
        public override Guid ProjectGuid
        {
            get { return typeof(MosaProjectFactory).GUID; }
        }

        /// <summary>
        /// Gets the type of the project.
        /// </summary>
        /// <value>The type of the project.</value>
        public override string ProjectType
        {
            get { return PROJECT_TYPE_NAME; }
        }

        /*protected override void Reload()
        {
            base.Reload();
        }*/

        protected override ConfigProvider CreateConfigProvider()
        {
            return new MosaConfigProvider(this);
        }

        public override FileNode CreateFileNode(ProjectElement item)
        {
            if (item.ItemName == "Kernel")
            {
            }
            return base.CreateFileNode(item);
        }

        #endregion
    }
}