using System;
using System.Collections.Generic;

namespace Graphics.Model
{
    public class Function
    {
        public string Name { get; private set; }
        public List<Parameter> Params { get; private set; }
        public Func<double, List<double>, double> Func { get; private set; }

        public Function(string name, List<Parameter> paramList, Func<double, List<double>, double> func)
        {
            Name = name;
            Params = paramList;
            Func = func;
        }
    }

    public class Parameter
    {
        public string Name { get; private set; }
        public int Value { get; set; }
        public int Step { get; private set; }
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }

        public Parameter(string name, int value, int step, int min, int max)
        {
            Name = name;
            Value = value;
            Step = step;
            MinValue = min;
            MaxValue = max;
        }

        public Parameter() : this("", 1, 1, int.MinValue, int.MaxValue) { }

        public Parameter(string name) : this()
        {
            Name = name;
        }
    }
}
