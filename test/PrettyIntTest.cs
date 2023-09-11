namespace RPGStudioMK.Tests;

public class PrettyIntTest : TestSuite
{
	[TestMethod]
	void TestAll()
	{
		assertEqual("1", Utilities.PrettyInt(1));
		assertEqual("12", Utilities.PrettyInt(12));
		assertEqual("123", Utilities.PrettyInt(123));
		assertEqual("1,234", Utilities.PrettyInt(1234));
		assertEqual("12,345", Utilities.PrettyInt(12345));
		assertEqual("123,456", Utilities.PrettyInt(123456));
		assertEqual("1,234,567", Utilities.PrettyInt(1234567));
		assertEqual("12,345,678", Utilities.PrettyInt(12345678));
		assertEqual("123,456,789", Utilities.PrettyInt(123456789));
		assertEqual("1", Utilities.PrettyInt(1, ".."));
		assertEqual("12", Utilities.PrettyInt(12, ".."));
		assertEqual("123", Utilities.PrettyInt(123, ".."));
		assertEqual("1..234", Utilities.PrettyInt(1234, ".."));
		assertEqual("12..345", Utilities.PrettyInt(12345, ".."));
		assertEqual("123..456", Utilities.PrettyInt(123456, ".."));
		assertEqual("1..234..567", Utilities.PrettyInt(1234567, ".."));
		assertEqual("12..345..678", Utilities.PrettyInt(12345678, ".."));
		assertEqual("123..456..789", Utilities.PrettyInt(123456789, ".."));
	}
}
