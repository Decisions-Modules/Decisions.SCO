using DecisionsFramework.Data.ORMapper;
using System.Runtime.Serialization;
using DecisionsFramework;
using DecisionsFramework.ServiceLayer;
using DecisionsFramework.Design.Properties;
using System.Collections.Generic;
using DecisionsFramework.ServiceLayer.Actions;
using DecisionsFramework.ServiceLayer.Services.Accounts;
using DecisionsFramework.ServiceLayer.Actions.Common;
using DecisionsFramework.ServiceLayer.Services.Portal;
using System.Reflection;
using System.IO;
using System;
using DecisionsFramework.Design.Properties.Attributes;
namespace SCOModule
{

    [ORMEntity]
    [DataContract]
    [ValidationRules]
    public class SCOIntegrationSettings : AbstractModuleSettings, IInitializable
    {
        private static readonly Log _log = new Log("SCO Module Settings");

        public SCOIntegrationSettings()
        {
            base.EntityName = "SCO Integration Settings";
        }

        #region properties

        [ORMField]
        private string scoServiceUrl = "";

        [DataMember]
        [PropertyClassification(0, "SCO Service URL", "SCO Connection")]
        public string SCOServiceUrl
        {
            get
            {
                return scoServiceUrl;
            }
            set
            {
                scoServiceUrl = value;
            }
        }

        [ORMField]
        private string scoServerUserName;

        [DataMember]
        [PropertyClassification(1, "SCO Server User Name", "SCO Connection")]
        public string SCOServerUsername
        {
            get
            {
                return scoServerUserName;
            }
            set
            {
                scoServerUserName = value;
            }
        }

        [ORMField(typeof(EncryptedConverter))]
        private string scoServerUserPassword;

        [DataMember]
        [PasswordText]
        [PropertyClassification(2, "SCO Server User Password", "SCO Connection")]
        public string SCOServerUserPassword
        {
            get
            {
                return scoServerUserPassword;
            }
            set
            {
                scoServerUserPassword = value;
            }
        }

        [ORMField]
        private string scoDomain;

        [DataMember]
        [PropertyClassification(3, "SCO Domain", "SCO Connection")]
        public string SCODomain
        {
            get
            {
                return scoDomain;
            }
            set
            {
                scoDomain = value;
            }
        }

        #endregion

        public void Initialize()
        {
        //    SCOIntegrationSettings.GetSettings();
        //    PortalSettings portalSettings = ModuleSettingsAccessor<PortalSettings>.GetSettings();
        //    ModuleSettingsAccessor<PortalSettings>.SaveSettings();
        }
        
        #region HelperMethods
        public static SCOIntegrationSettings GetSettings()
        {
            return ModuleSettingsAccessor<SCOIntegrationSettings>.GetSettings();
        }
        public static void SaveSettings()
        {
            ModuleSettingsAccessor<SCOIntegrationSettings>.SaveSettings();
        }
        #endregion

        public override DecisionsFramework.ServiceLayer.Actions.BaseActionType[] GetActions(DecisionsFramework.ServiceLayer.Utilities.AbstractUserContext userContext, DecisionsFramework.ServiceLayer.Actions.EntityActionType[] types)
        {
            List<BaseActionType> actionTypes = new List<BaseActionType>();
            Account account = userContext.GetAccount();

            actionTypes.Add(new EditEntityAction(typeof(SCOIntegrationSettings), "Edit Settings", "Edit Integration Settings"));
           
            return actionTypes.ToArray();
        }
    }
}