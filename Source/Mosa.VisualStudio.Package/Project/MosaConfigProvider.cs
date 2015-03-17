using Microsoft.VisualStudio.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosa.VisualStudio.Package.Project
{
    class MosaConfigProvider : ConfigProvider
    {
        public MosaConfigProvider(ProjectNode manager)
            : base(manager)
        {
        }

        protected override ProjectConfig CreateProjectConfiguration(string configName)
        {
            return new MosaProjectConfig(this.ProjectMgr, configName);
        }
    }
}
