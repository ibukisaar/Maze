using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;

namespace 迷宫控件 {
	class RandomMapperGenerator {
		public static CumulativeDistributionFunction Generate(string code) {
			string template = string.Format(@"
using System;
class Functor {{
	public static Func<double, double> func = x => {0};
}}
", code);

			CSharpCodeProvider provider = new CSharpCodeProvider();
			CompilerParameters parameters = new CompilerParameters();
			parameters.GenerateExecutable = false;
			parameters.GenerateInMemory = true;
			parameters.IncludeDebugInformation = false;
			parameters.CompilerOptions = "/optimize";
			var result = provider.CompileAssemblyFromSource(parameters, new string[] { template });
			if (result.Errors.Count > 0) {
				List<string> strs = new List<string>();
				foreach (var s in result.Errors) {
					strs.Add(s.ToString());
				}
				throw new Exception(string.Join(Environment.NewLine, strs));
			}
			var type = result.CompiledAssembly.GetType("Functor");
			return new CumulativeDistributionFunction(type.GetField("func").GetValue(null) as Func<double, double>);
		}
	}
}
