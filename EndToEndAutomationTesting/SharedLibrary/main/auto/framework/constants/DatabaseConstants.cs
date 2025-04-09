namespace SharedLibrary.main.auto.framework.constants
{
    public class DatabaseConstants
    {
        // Database Type
        public const string DB_TYPE = "MSSQL";

        // Database connection details :: QAT
        public const string QAT_DB_HOST = "qat-db-mssql.openlm.net";
        public const string QAT_DB_USER = "qatk8sonprem";
        public const string QAT_DB_PASS = "nsmOmy6vbeiGJO9wJtIS";
        public const string DB_PORT = "1433";

        public const string QAT_DB_Load = "OpenLMAutomationTestData_Performance";
        public const string QAT_ENV_SER = "OpenLM_Environment_Service";
        public const string QAT_DB_LAC = "lac_automation_qat";
        
        public const string QatServerK8SOnPrem = "qat_kube_olmdb_customer_none";

        public const string QatBrokerConnectorServer = "OpenLMAutomation_BrokerHub_Connector_Server";
        public const string PersistedBrokerServer = "OpenLMAutomation_Persisted";
    }
}