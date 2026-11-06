using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Trident.Web.Core.Models;

namespace Trident.Web.Test.Fakes
{
    internal static class FakeArticleContent
    {
        public static string GetKSLArticleContent()
        {
            var fileToOpen = Path.Combine(Path.Combine(Directory.GetCurrentDirectory()), $"KSLExample.html");
            return File.ReadAllText(fileToOpen);
        }
    }
}
