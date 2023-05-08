using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanoid.Common.Configuration.Templates;

namespace Sanoid.Common.Configuration.Datasets
{
    internal class Dataset
    {
        public Dataset( Template template )
        {
            Template = template;
        }

        internal Template Template { get; set; }

        internal Template? TemplateOverrides { get; set; }
    }
}
