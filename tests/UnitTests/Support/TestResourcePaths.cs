/*-
 * #%L
 * Smart ID .NET client — unit tests
 * #L%
 */

using System;
using System.IO;

namespace SK.SmartId.Support
{
    internal static class TestResourcePaths
    {
        internal static string ReadAllText(string relativePathUnderResources)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Resources", relativePathUnderResources);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Test resource not found: " + path);
            }
            return File.ReadAllText(path);
        }
    }
}
