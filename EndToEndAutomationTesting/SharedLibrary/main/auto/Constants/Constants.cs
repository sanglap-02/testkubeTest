namespace SharedLibrary.main.auto.Constants
{
    public static class Constants
    {
        //Path Constants

        public static string ReportPath = Directory.GetParent(@"../../../").FullName + Path.DirectorySeparatorChar + "Result";
        public static string LogPath = Directory.GetParent(@"../../../").FullName + Path.DirectorySeparatorChar + "Result";


        //public static string GoogleSpreadhsheetID = "1E7CD4uL22iWp1ykTFAmBm-l5eNplnJKUEocbWLs1BEQ";
        public static string GoogleSpreadhsheetID = "1BKd6qOZwLJ7GCErCOEHAsS3VOofzEcPKW3KUvhYiiI8";
        public static string GoogleSecurityJson = "client_secret.json";
        public static string GoogleSecurityJsonPath = Directory.GetParent(@"../../../../").FullName + Path.DirectorySeparatorChar + "SharedLibrary" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Google" + Path.DirectorySeparatorChar + "Security" + Path.DirectorySeparatorChar + "client_secret.json";

        public static string EXCELFILEPATH = Directory.GetParent(@"../../../../").FullName + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "TestData" + Path.DirectorySeparatorChar + "OpenLMAutomationTestData.xlsx";


        //Sheet Constants
        public static string ENVIRONMENT_SHEET = "Environment";
        public static string SERVICES_SHEET = "Services";

        public static readonly string BasePath = Directory.GetParent(@"../../../../").FullName + Path.DirectorySeparatorChar;
    }
}
