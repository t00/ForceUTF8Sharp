using System.Resources;
using System.Text;
using NUnit.Framework;

namespace ForceUtf8Sharp.Tests
{
    public class ForceUtf8Tests
    {

        [TestCase(@"ForceUtf8Sharp.Tests.Files.test1.txt", "Hírek\n")]
        [TestCase(@"ForceUtf8Sharp.Tests.Files.russian.txt", "hello žš, привет\n")]
        [TestCase(@"ForceUtf8Sharp.Tests.Files.a.txt", "Fédération Camerounaise de Football\n")]
        [TestCase(@"ForceUtf8Sharp.Tests.Files.b.txt", "Fédération Camerounaise de Football\n")]
        [TestCase(@"ForceUtf8Sharp.Tests.Files.c.txt", "Fédération Camerounaise de Football\n")]
        [TestCase(@"ForceUtf8Sharp.Tests.Files.d.txt", "Fédération Camerounaise de Football\n")]
        public void TestFixUtf8(string resourceName, string expectedText)
        {
            var contents = GetResource(resourceName);
            var convertedBytes = ForceUtf8.FixUtf8(contents);
            var bytesText = Encoding.UTF8.GetString(convertedBytes);
            Assert.AreEqual(expectedText, bytesText);

            var text = Encoding.Default.GetString(contents);
            var converted = ForceUtf8.FixUtf8(text);
            if (string.Equals(converted, text))
            {
                Assert.AreEqual(expectedText, converted, "No change");
            }
            else
            {
                Assert.AreEqual(expectedText, converted);
            }
        }

        private static byte[] GetResource(string resourceName)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using var resourceStream = assembly.GetManifestResourceStream(resourceName);
            Assert.IsNotNull(resourceStream);
            var contents = new byte[resourceStream.Length];
            resourceStream.Read(contents);
            return contents;
        }
    }
}