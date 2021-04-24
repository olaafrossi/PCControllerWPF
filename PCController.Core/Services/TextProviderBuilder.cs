using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MvvmCross.IoC;
using MvvmCross.Plugin.JsonLocalization;

namespace PCController.Core.Services
{
    public class TextProviderBuilder : MvxTextProviderBuilder
    {
        public TextProviderBuilder() : base("Playground.Core", "Resources", new MvxEmbeddedJsonDictionaryTextProvider(false))
        {
        }

        protected override IDictionary<string, string> ResourceFiles
        {
            get
            {
                Dictionary<string, string> dictionary = GetType().GetTypeInfo()
                    .Assembly
                    .CreatableTypes()
                    .Where(t => t.Name.EndsWith("ViewModel"))
                    .ToDictionary(t => t.Name, t => t.Name);

                dictionary.Add("Text", "Text");

                return dictionary;
            }
        }
    }
}
