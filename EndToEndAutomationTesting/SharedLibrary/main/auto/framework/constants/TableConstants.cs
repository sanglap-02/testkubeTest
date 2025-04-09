namespace SharedLibrary.main.auto.framework.constants
{
    public class TableConstants
    {
        public const string Environment = "[OpenLM_Environment_Service].[dbo].[Environment]";
        public const string Services = "[OpenLM_Environment_Service].[dbo].[Services]";

        public const string LAC_BrokerHub = "[lac_automation_qat].[dbo].[BrokerHub]";
        public const string LAC_LicenseServersDetails = "[lac_automation_qat].[dbo].[LicenseServersDetails]";
        public const string LAC_OptionsFile = "[lac_automation_qat].[dbo].[OptionsFile]";

        public const string LOAD_TEST_TABLE = "[OpenLMAutomationTestData_Performance].[dbo].[LoadTests]";
        public const string DSS_USERS = "[OpenLMAutomationTestData_Performance].[dbo].[Users]";

        public const string AGENT_HUB_TABLE = "[OpenLMAutomationTestData_Performance].[dbo].[AgentHubLoad]";

        public const string ServerConnectorBrokerHub =
            "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[BrokerHub]";

        public const string ServerConnectorLicSer =
            "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[LicenseServersDetails]";

        public const string CandidateHosts = "[qat_kube_olmdb_customer_none].[dbo].[OLM_CANDIDATE_HOSTS]";

        public const string LicenseManagerTypes =
            "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[LicenseManagerTypes]";
        public const string LicenseManagerComparision =
            "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[LicenseManagerComparision]";
        public const string OlmLicenseServer = "[qat_kube_olmdb_customer_none].[dbo].[OLM_LICENSE_SERVERS]";
        public const string Product = "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[Product]";
        public const string BrokerHubBack = "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[BrokerHub_Back]";
        public const string CloudBroker = "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[CloudBroker]";
        public const string BrokerServerUserGroup = "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[UsersAndGroups]";
        public const string BrokerServerCompositeTable =
            "[OpenLMAutomation_BrokerHub_Connector_Server].[dbo].[BrokerComposite]";
        
        public const string EnrichmentBrokerHub = "[OpenLMAutomation_Enrichment].[dbo].[BrokerHub]";
        public const string EnrichmentLicSer = "[OpenLMAutomation_Enrichment].[dbo].[LicenseServersDetails]";
        public const string UsersAndGroups = "[OpenLMAutomation_Enrichment].[dbo].[UsersAndGroups]";
        public const string EnrichmentBrokerComposite = "[OpenLMAutomation_Enrichment].[dbo].[BrokerComposite]";

        public const string PersistedBrokerHub = "[OpenLMAutomation_Persisted].[dbo].[BrokerHub]";
        public const string PersistedLicSer = "[OpenLMAutomation_Persisted].[dbo].[LicenseServersDetails]";
        public const string PersistedUsersAndGroups = "[OpenLMAutomation_Persisted].[dbo].[UsersAndGroups]";
        public const string PersistedBrokerComposite = "[OpenLMAutomation_Persisted].[dbo].[BrokerComposite]";
        public const string PersistedOptionFiles = "[OpenLMAutomation_Persisted].[dbo].[OptionFiles]";
        public const string PersistedLicenseFiles = "[OpenLMAutomation_Persisted].[dbo].[LicenseFile]";
    }
}