using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Virinco.WATS.Interface;

namespace Virinco.WATS.Converter.Omron
{
    public class DLCMConverter : IReportConverter_v2
    {
        protected IDictionary<string, string> converterArguments;

        public DLCMConverter() : this(new Dictionary<string, string>()) { }

        public DLCMConverter(IDictionary<string, string> args)
        {
            converterArguments = args;
        }

        public Dictionary<string, string> ConverterParameters => new Dictionary<string, string>
        {
            { "operationTypeCode", "10" },
            { "sequenceName", "MainSequence" },
            { "cultureCode", "en-US" },
            { "utcOffsetHours", "0" }
        };

        public void CleanUp() { }

        CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
        string partNumber = "NA";
        string[] stepNames;
        string[] limits;
        List<Step> steps;

        public class Step
        {
            public string stepName { get; set; }
            public double lowLimit { get; set; }
            public double highLimit { get; set; }
            public int column { get; set; }
        }

        private void ReadHeaders(StreamReader s)
        {
            string line = s.ReadLine();
            while (!s.EndOfStream && !line.StartsWith("\t\t\t\t"))
            {
                if (line.StartsWith("OEDC: "))
                    partNumber = line.Replace("OEDC: ", "").Trim();
                line = s.ReadLine();
            }
            stepNames = line.Split('\t');
            line = s.ReadLine();
            limits = line.Split('\t');
            for (int i = 7; i < stepNames.Length - 1; i = i + 2)
            {
                steps.Add(new Step { stepName = stepNames[i], column = i, lowLimit = double.Parse(limits[i], culture), highLimit = double.Parse(limits[i + 1], culture) });
            }
        }

        public Report ImportReport(TDM api, Stream file)
        {
            int lineCount = 0;
            steps = new List<Step>();
            string[] currentLine;
            if (converterArguments.ContainsKey("cultureCode"))
                culture = CultureInfo.GetCultureInfo(converterArguments["cultureCode"]);
            double utcOffsetHours = converterArguments.ContainsKey("utcOffsetHours")
                ? double.Parse(converterArguments["utcOffsetHours"], CultureInfo.InvariantCulture)
                : 0.0;
            try
            {
                using (StreamReader s = new StreamReader(file))
                {
                    ReadHeaders(s);
                    while (!s.EndOfStream)
                    {
                        currentLine = s.ReadLine().Split('\t');
                        UUTReport uut = api.CreateUUTReport("oper", partNumber, "NA", currentLine[3],
                            converterArguments["operationTypeCode"], converterArguments["sequenceName"], "1.0.0");
                        uut.StartDateTime = DateTime.ParseExact(currentLine[0] + " " + currentLine[1], "M/d/yyyy h:mm:ss tt", culture);
                        if (utcOffsetHours != 0.0)
                            uut.StartDateTimeUTC = uut.StartDateTime.AddHours(utcOffsetHours);
                        uut.StationName = currentLine[2];
                        uut.ExecutionTime = double.Parse(currentLine[5], culture);
                        uut.GetRootSequenceCall().AddNumericLimitStep("Temp").AddTest(double.Parse(currentLine[6], culture), "°C");
                        string prevseqGroup = "";
                        string seqGroup = "";
                        string seqName = "";
                        SequenceCall sequence = uut.GetRootSequenceCall();
                        for (int i = 0; i < steps.Count; i++)
                        {
                            DLCMConverter.Step st = steps[i];
                            if (!string.IsNullOrEmpty(currentLine[st.column]))
                            {
                                Regex ex = new Regex(@"(?<group>\d+.\d+.\d+).\d+\w* (?<name>[^-]+)");
                                Match match = ex.Match(st.stepName);
                                if (match.Groups.Count > 0)
                                {
                                    seqGroup = match.Groups["group"].Value;
                                    seqName = match.Groups["name"].Value;
                                    if (seqGroup != prevseqGroup)
                                    {
                                        sequence = uut.GetRootSequenceCall().AddSequenceCall(seqGroup + " " + seqName);
                                        prevseqGroup = seqGroup;
                                    }
                                }
                                else
                                    sequence = uut.GetRootSequenceCall();
                                NumericLimitStep ns = sequence.AddNumericLimitStep(st.stepName);
                                ns.AddTest(GetDouble(currentLine[st.column]), CompOperatorType.GELE, st.lowLimit, st.highLimit, "",
                                    currentLine[st.column + 1] == "1" ? StepStatusType.Passed : StepStatusType.Failed);
                            }
                        }
                        uut.Status = currentLine[4] == "1" ? UUTStatusType.Passed : UUTStatusType.Failed;
                        api.Submit(uut);
                        lineCount++;
                    }
                }
            }
            catch (Exception inner)
            {
                LogError(api, lineCount, inner);
                throw new ApplicationException("Error converting DLCM file", inner);
            }
            return null;
        }

        double GetDouble(string s)
        {
            if (s.ToLower().StartsWith("inf"))
                return double.PositiveInfinity;
            else if (s.ToLower().StartsWith("-inf"))
                return double.NegativeInfinity;
            else
            {
                double d;
                if (double.TryParse(s, NumberStyles.Any, culture, out d))
                    return d;
                else
                    return double.NaN;
            }
        }

        private void LogError(TDM api, int lineCount, Exception ex)
        {
            StreamWriter errorlog = new StreamWriter(api.ConversionSource.ErrorLog);
            errorlog.WriteLine("Error in file {0}, line {1}: {2}", api.ConversionSource.SourceFile.Name, lineCount, ex.Message);
            errorlog.Flush();
        }
    }
}
