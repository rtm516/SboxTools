using System;

namespace SboxTools.Types
{
	public struct ConsoleOutput
	{
		public string Level { get; set; }

		public string Logger { get; set; }
		
		public string Msg { get; set; }
		
		public string Stack { get; set; }
		
		public DateTimeOffset Time { get; set; }
		
		public bool HasStack
		{
			get
			{
				return !string.IsNullOrWhiteSpace(this.Stack);
			}
		}
		
		public string[] StackLines
		{
			get
			{
				return this.HasStack ? this.Stack.Split(new char[]
				{
					'\r',
					'\n'
				}, StringSplitOptions.RemoveEmptyEntries) : null;
			}
		}
		
		public int Instances { get; set; }
	}
}
