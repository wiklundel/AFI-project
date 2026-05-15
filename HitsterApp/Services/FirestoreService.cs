using Google.Cloud.Firestore;

namespace HitsterApp.Services
{
    public class FirestoreService
    {
        public FirestoreDb Db { get; }

        public FirestoreService()
        {
            string path = "Json/serviceAccountKey.json";

            Environment.SetEnvironmentVariable(
                "GOOGLE_APPLICATION_CREDENTIALS",
                path
            );

            Db = FirestoreDb.Create("hitsterapp-1902d");
        }
    }
}