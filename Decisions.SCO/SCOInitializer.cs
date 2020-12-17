using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DecisionsFramework;
using DecisionsFramework.ServiceLayer;

namespace DecisionsSCOrchestrator
{
    public class SCOInitializer: IModuleLicense
    {
        public void ValidateLicenseData(bool isPlatformOnTempLicense, ModuleLicenseDetail[] moduleLicenses)
        {
            if (isPlatformOnTempLicense == false && (moduleLicenses == null || moduleLicenses.FirstOrDefault(x => x.ModuleName.Equals("Decisions.SCO") || x.ModuleName.Equals("Decisions.Enterprise")) == null))
            {
                throw new LicensePolicyExceededException("Your license does not grant access to the Decisions.SCO Module.  Please contact support for a license.");
            }
        }
    }
}
