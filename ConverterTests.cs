using System.Collections.Generic;
using Virinco.WATS.Testing;
using Virinco.WATS.Interface;
using Virinco.WATS.Converter.Omron;
using Xunit;

namespace Virinco.WATS.Converter.Omron.Tests
{
    public class DLCMConverterTests : ConverterTestBase
    {
        protected override IReportConverter_v2 CreateConverter()
        {
            var args = new DLCMConverter().ConverterParameters;
            return new DLCMConverter(args);
        }

        [Fact, Trait("TestMode", "ConvertOnly")]
        public void ConvertOnly_AllFiles() => RunAllFiles(TestMode.ConvertOnly);

        [Fact, Trait("TestMode", "ConvertAndValidate")]
        public void ConvertAndValidate_AllFiles() => RunAllFiles(TestMode.ConvertAndValidate);

        [Fact, Trait("TestMode", "ConvertAndSubmit"), Trait("RequiresServer", "true")]
        public void ConvertAndSubmit_AllFiles() => RunAllFiles(TestMode.ConvertAndSubmit);
    }
}
