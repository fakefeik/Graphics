using System;
using System.Collections.Generic;

namespace Graphics.Model
{
    public class ChartModel
    {
        public List<Function> Functions { get; } = new List<Function>
        {
            new Function("MyFunc", new List<Parameter> {new Parameter("A", 1, 1, 1, 1), new Parameter("B", 1, 1, 1, 1)},
                (x, p) => p[0]*x*x/(Math.Pow(x*x - p[1]*p[1], 1/3.0))),
            new Function("Linear", new List<Parameter> {new Parameter("A"), new Parameter("B")},
                (x, p) => p[0]*x + p[1]),
            new Function("Quadratic",
                new List<Parameter> {new Parameter("A"), new Parameter("B"), new Parameter("C")},
                (x, p) => p[0]*x*x + p[1]*x + p[2]),
            new Function("Cubic",
                new List<Parameter> {new Parameter("A"), new Parameter("B"), new Parameter("C"), new Parameter("D")},
                (x, p) => x*x*x),
            new Function("Sin", new List<Parameter> {new Parameter("N")}, (x, p) => Math.Sin(p[0]*x)),
            new Function("Abs", new List<Parameter>(), (x, p) => Math.Abs(x)),
            new Function("Round", new List<Parameter>(), (x, p) => Math.Round(x)),
            new Function("1/x", new List<Parameter>(), (x, p) => 1/x),
            new Function("Gaussian",
                new List<Parameter> {new Parameter("A"), new Parameter("B"), new Parameter("C")},
                (x, p) => p[0]*Math.Exp(-(x - p[1])*(x - p[1])/(2*p[2]*p[2]))) // a = 8, b = 3, c = 4
        };
    }

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
        public int InitialValue { get; private set; }
        public int Step { get; private set; }
        public int MinValue { get; private set; }
        public int MaxValue { get; private set; }

        public Parameter(string name, int initialValue, int step, int min, int max)
        {
            Name = name;
            InitialValue = initialValue;
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
