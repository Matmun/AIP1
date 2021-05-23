using Microsoft.ML.Probabilistic.Math;
using Microsoft.ML.Probabilistic.Models;
using System;

namespace AIP3
{
    class Program
    {
        static void Main(string[] args)
        {
            int[] data = new int[10]; //number of throws

            for (int i = 0; i < data.Length; i++)
                data[i] = Rand.Binomial(10, 0.5);
            Variable<double> a = Variable.Beta(1, 1);

            for (int i = 0; i < data.Length; i++)
            {
                Variable<int> x = Variable.Binomial(10, a);
                x.ObservedValue = data[i];
            }

            InferenceEngine engine = new InferenceEngine();// Retrieve the posterior distributions  
            Console.WriteLine("resoult=" + engine.Infer(a));
        }
    }
}
