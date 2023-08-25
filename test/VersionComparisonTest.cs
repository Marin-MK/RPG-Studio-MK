namespace RPGStudioMK.Tests;

public class VersionComparisonTest : TestSuite
{
    [TestMethod]
    void TestAll()
    {
        assertEqual(0, MKUtils.VersionMetadata.CompareVersions("1.0", "1"));
        assertEqual(0, MKUtils.VersionMetadata.CompareVersions("1.0", "1.0"));
        assertEqual(0, MKUtils.VersionMetadata.CompareVersions("1.0", "1.0.0.0"));
        assertEqual(0, MKUtils.VersionMetadata.CompareVersions("1", "1"));
        assertEqual(0, MKUtils.VersionMetadata.CompareVersions("1.1", "1.1.0"));
        assertEqual(0, MKUtils.VersionMetadata.CompareVersions("1.1.1", "1.1.1"));
        assertEqual(0, MKUtils.VersionMetadata.CompareVersions("1.0.0.1", "1.0.0.1.0.0.0"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("1.1", "1"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("1.1.1", "1.1.0"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("1.2", "1.1.1"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("1.2", "1.1.2"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("2.1.1", "1.1.2"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("2", "1.9.9"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("2.0.0.0.0.1", "2"));
        assertEqual(1, MKUtils.VersionMetadata.CompareVersions("2.0.0.0.0.1", "2.0.0.0.0.0.0.0"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1", "2"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("2", "3"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("3", "4"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("4", "5"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("5", "6"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("6", "7"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("7", "8"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("8", "9"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1", "1.1"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1.0", "1.1"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1.1.0", "1.1.1"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1.1.0", "1.1.0.1"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1.2", "1.3"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1.0.0.1", "1.0.1"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1.0.0.1", "1.1.0.0.0"));
        assertEqual(-1, MKUtils.VersionMetadata.CompareVersions("1.0.0.1", "1.0.1.0"));

    }
}
