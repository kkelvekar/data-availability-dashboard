using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaDashboard.DataSource.GraphQL.Services
{
    [Flags]
    public enum ParameterExistence
    {
        None = 0,
        EntityName = 1,
        EffectiveDate = 2,
        EntityNameAndEffDate = 3,
    }

}
