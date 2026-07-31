using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Gamebee.Utils
{
    public class ExpressionEvaluator
    {
        public IExpValueProvider ExpValueProvider;

        // public static void Log(string log) => Debug.Log(log);
        // private void LogError(string log) => Debug.LogError(log);

        public void EvaluateExpressions(RCData rcData)
        {
            foreach (var expression in ExtractOverrides(rcData))
            {
                var data = expression.Evaluate(this);
                rcData.Data[expression.Name] = data.DeepClone();
            }
        }

        private IEnumerable<RCOverride> ExtractOverrides(RCData rcData)
        {
            if (rcData.Overrides == null) yield break;
            foreach (var expPair in rcData.Overrides)
            {
                if (!rcData.Data.TryGetValue(expPair.Key, out var data)) continue;

                var expression = RCOverride.Parse(expPair.Key, expPair.Value, data);
                if (expression == null) continue;

                yield return expression;
            }
        }

        internal T Evaluate<T>(string pExpression, T defaultValue)
        {
            try
            {
                // Evaluate the expression
                DataTable table = new DataTable();
                table.CaseSensitive = false;

                var expression = GetComputeExpression(pExpression);
                if (expression == "0") return defaultValue;

                // Log($"Expression: {expression}");
                var r = table.Compute(expression, "True") is { } result ? (T)result : defaultValue;
                // Log($"Expression: {expression} => " + r);
                return r;
            }
            catch (Exception e)
            {
                // LogError($"Error evaluating expression: {pExpression}\n{e}");
                return defaultValue;
            }
        }

        private string[] GetVariables(string pExpression)
        {
            return Regex.Matches(pExpression, @"\{([^}]*)\}")
                .Select(mc => mc.Value)
                .ToArray();
        }

        private string GetComputeExpression(string pExpression)
        {
            var variables = GetVariables(pExpression);
            var expression = new StringBuilder(pExpression);
            foreach (var variable in variables)
            {
                expression.Replace(variable, GetVariableValue(variable));
            }

            return expression.ToString();
        }

        private string GetVariableValue(string pVariable)
        {
            var name = pVariable.Trim('{', '}');
            if (!name.Contains(':'))
            {
                // Simple variable
                return name switch
                {
                    "DeviceModel" => "\'" + Get_DeviceModel() + "\'",
                    "Locale" => "\'" + Get_Locale() + "\'",
                    "TimeZone" => "\'" + Get_TimeZone() + "\'",
                    "TimeStamp" => "" + Get_TimeStamp(),
                    "IsTV" => "" + Get_IsTV(),
                    "LaunchCount" => "" + Get_LaunchCount(),
                    "ScreenWidth" => "" + Get_ScreenWidth(),
                    "ScreenHeight" => "" + Get_ScreenHeight(),
                    "Version" => "\'" + Get_Version() + "\'",
                    "IsUS" => "" + IsUS(),
                    "IsCA" => "" + IsCA(),
                    "IsEU" => "" + IsEU(),
                    "IsJP" => "" + IsJP(),
                    "IsAU" => "" + IsAU(),
                    "IsBR" => "" + IsBR(),
                    "IsOSX" => "" + IsOSX(),
                    "IsIOS" => "" + IsIOS(),
                    "IsTVOS" => "" + IsTVOS(),
                    _ => "0"
                };
            }

            // Variable with parameters
            var parts = name.Split(':');
            if (parts.Length != 4) return "0";

            name = parts[0];
            var source = parts[1];
            var type = parts[2];
            var defValue = parts[3];
            if (source != "PP") return "0";

            return type switch
            {
                "int" => "" + Get_PP(name, int.Parse(defValue)),
                "float" => "" + Get_PP(name, float.Parse(defValue)),
                "string" => "\'" + Get_PP(name, defValue) + "\'",
                "bool" => "" + Get_PP(name, bool.Parse(defValue)),
                _ => "0"
            };
        }

        private string Get_DeviceModel() => ExpValueProvider.DeviceModel();
        private string Get_TimeZone() => ExpValueProvider.TimeZone();
        private long Get_TimeStamp() => ExpValueProvider.TimeStamp();
        private string Get_Locale() => ExpValueProvider.Locale();
        private bool Get_IsTV() => ExpValueProvider.IsTV();
        private int Get_LaunchCount() => ExpValueProvider.LaunchCount();
        private int Get_ScreenWidth() => ExpValueProvider.ScreenWidth();
        private int Get_ScreenHeight() => ExpValueProvider.ScreenHeight();
        private string Get_Version() => ExpValueProvider.Version();

        private int Get_PP(string key, int def) => ExpValueProvider.PP(key, def);
        private float Get_PP(string key, float def) => ExpValueProvider.PP(key, def);
        private string Get_PP(string key, string def) => ExpValueProvider.PP(key, def);
        private bool Get_PP(string key, bool def) => ExpValueProvider.PP(key, def);

        private bool IsUS() => ExpValueProvider.IsUS();
        private bool IsCA() => ExpValueProvider.IsCA();
        private bool IsEU() => ExpValueProvider.IsEU();
        private bool IsJP() => ExpValueProvider.IsJP();
        private bool IsAU() => ExpValueProvider.IsAU();
        private bool IsBR() => ExpValueProvider.IsBR();
        private bool IsOSX() => ExpValueProvider.IsOSX();
        private bool IsIOS() => ExpValueProvider.IsIOS();
        private bool IsTVOS() => ExpValueProvider.IsTVOS();
    }

    public class RCOverride
    {
        public string Name;
        public Condition[] Conditions;
        public JToken Default;
        public JToken Result;

        public static RCOverride Parse(string name, JToken value, JToken defaultValue)
        {
            var rcOverride = new RCOverride
            {
                Name = name,
                Default = defaultValue
            };

            if (value is not JArray array) return null;
            rcOverride.Conditions = array
                .Select(c => new Condition(c as JObject))
                .ToArray();

            return rcOverride;
        }

        public JToken Evaluate(ExpressionEvaluator evaluator)
        {
            foreach (var condition in Conditions)
            {
                if (evaluator.Evaluate(condition.If, false))
                    return Result = condition.Then;
            }

            return Result = Default;
        }
    }

    public class Condition
    {
        public string If;
        public JToken Then;

        public Condition(JObject data)
        {
            If = data["If"].Value<string>();
            Then = data["Then"];
        }
    }
}